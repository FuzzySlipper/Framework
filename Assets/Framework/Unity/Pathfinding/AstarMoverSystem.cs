//#define AStarPathfinding
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if AStarPathfinding
using Pathfinding;
using Pathfinding.RVO;
using Pathfinding.Util;

namespace PixelComrades {
    [RequireComponent(typeof(Seeker))]
    public class ActorPathMoverAstar : EntityIdentifier, IOnCreate {
        
        [SerializeField] private float _endReachedDistance = 0.05F;
        [SerializeField] private float _maxSpeed = 1;
        [SerializeField] private float _pickNextWaypointDist = 0.35f; // must be higher than slow down
        [SerializeField] private float _rotationSpeed = 180;
        [SerializeField] private float _slowdownDistance = 0.2F;
        [SerializeField] private bool _slowWhenNotFacingTarget = true;
        [SerializeField] private bool _useGravity = false;
        [SerializeField] private bool _canWalkThroughAllies = false;
        [SerializeField] private bool _goToExactEndPoint = true;
        [SerializeField] private float _slowdownTime = 0.5f;

        private RVOController _rvoController;
        private CooperativePathInterpolator _interpolator;
        private IMovementPlane _movementPlane = GraphTransform.identityTransform;
        private Path _path;
        private Seeker _seeker;
        private GraphNode _stuckNode = null;
        private ScaledTimer _errorTimer = new ScaledTimer(0.5f);
        private Vector2 _lastDeltaPosition;
        private float _lastDeltaTime;
        private ScaledTimer _pathTimer = new ScaledTimer(0.5f);
        private NpcMovementManager.TraversalProvider _traversal;
        private Vector2 _velocity2D;
        private float _verticalVelocity;
        private bool _waitingForPathCalculation;
        private LineRenderer _debugRenderer;

        public bool CanWalkThroughAllies { get { return _canWalkThroughAllies; } }
        public PathfindingGridNode CurrentGraphNode { get; set; }
        private bool ShouldRecalculatePath { get { return !_errorTimer.IsActive && !_pathTimer.IsActive && !_waitingForPathCalculation; } }
        public bool IsMoving { get { return _interpolator.valid && _rvoController.velocity.sqrMagnitude > 0.1f; } }
        public Vector3 SteeringTargetV3 { get { return _interpolator.valid ? _interpolator.position : transform.position; } }
        public float MaxSpeed { get { return Entity.Tags.Contain(EntityTags.Slowed) ? _maxSpeed * 0.25f : _maxSpeed; } }
        public Point3 SteeringTarget { get { return _interpolator.TargetGraphNode != null ? _interpolator.TargetGraphNode.position.toPoint3() : new Point3(SteeringTargetV3); } }
        public GraphNode SteeringTargetNode { get { return _interpolator.TargetGraphNode; } }
        public int MovePriority { get { return _interpolator.Priority; } }
        public Vector3 MoveTarget { get; protected set; }
        public bool TargetReached { get; protected set; }
        public bool Wandering { get; protected set; }
        public Transform Tr { get; protected set; }
        public Vector3 LocalCenter { get; protected set; }

        public void OnCreate(PrefabEntity entity) {
            _interpolator = new CooperativePathInterpolator(this);
            _seeker = GetComponent<Seeker>();
            _rvoController = GetComponent<RVOController>();
            _traversal = new NpcMovementManager.TraversalProvider(this);
            Tr = transform;
            var colliderComponent = GetComponent<Collider>();
            if (colliderComponent != null) {
                LocalCenter = new Vector3(0, colliderComponent.bounds.size.y/2, 0);
            }
            else {
                LocalCenter = Vector3.up;
            }
            if (GameOptions.Get("DebugPathfinding", false)) {
                _debugRenderer = gameObject.AddComponent<LineRenderer>();
                _debugRenderer.widthMultiplier = 0.3f;
            }
        }

        public void Stop() {
            if (_debugRenderer != null) {
                _debugRenderer.positionCount = 0;
            }
        }

        protected void Init() {
            CurrentGraphNode = null;
        }

        public void OnPoolSpawned() {
            _seeker.pathCallback += OnPathRequestComplete;
        }

        public void OnPoolDespawned() {
            _seeker.pathCallback -= OnPathRequestComplete;
            if (_path != null) {
                _path.Release(this);
            }
            _path = null;
        }

        public void UpdateMovement() {
            if (ShouldRecalculatePath) {
                SearchPath();
            }
            _lastDeltaTime = TimeManager.DeltaTime;
            MovementUpdateInternal(TimeManager.DeltaTime);
            //if (_debugRenderer != null && _interpolator.segmentIndex > 0) {
            //    for (int i = _interpolator.segmentIndex - 1; i >= 0; i--) {
            //        _debugRenderer.SetPosition(i, SteeringTargetV3);
            //    }
            //}
        }

        

        public void ActorDied() {
            if (CurrentGraphNode != null) {
                CurrentGraphNode.Entity = null;
            }
            CurrentGraphNode = null;
        }

        public void SetMoveTarget(Vector3 moveTarget) {
            Wandering = false;
            if (moveTarget != MoveTarget) {
                if (!_errorTimer.IsActive) {
                    SearchPath();
                }
            }
        }

        public void OnPathRequestComplete(Path newPath) {
            var p = newPath as ABPath;
            if (p == null) {
                throw new Exception("This function only handles ABPaths, do not use special path types");
            }
            _waitingForPathCalculation = false;

            // Increase the reference count on the new path.
            // This is used for object pooling to reduce allocations.
            p.Claim(this);

            // Path couldn't be calculated of some reason.
            // More info in p.errorLog (debug string)
            if (p.error) {
                //Debug.LogErrorFormat("Path error {0} {1} is RandomPath {2} isWander {3} end {4}",Actor.Tr.position, p.errorLog, p is RandomPath, _wandering, p.originalEndPoint);
                p.Release(this);
                _errorTimer.StartNewTime(0.5f);
                Stop();
                return;
            }
            if (_path != null) {
                _path.Release(this);
            }

            _path = p;

            // Make sure the path contains at least 2 points
            if (_path.vectorPath.Count == 1) {
                _path.vectorPath.Add(_path.vectorPath[0]);
            }
            _interpolator.SetPath(_path);

            var graph = AstarData.GetGraph(_path.path[0]) as ITransformedGraph;
            _movementPlane = graph != null ? graph.transform : GraphTransform.identityTransform;
            TargetReached = false;

            // Simulate movement from the point where the path was requested
            // to where we are right now. This reduces the risk that the agent
            // gets confused because the first point in the path is far away
            // from the current position (possibly behind it which could cause
            // the agent to turn around, and that looks pretty bad).
            _interpolator.MoveToLocallyClosestPoint((Tr.position + p.originalStartPoint) * 0.5f);
            _interpolator.MoveToLocallyClosestPoint(Tr.position);
            if (Wandering) {
                MoveTarget = (Vector3) _path.path.LastElement().position;
            }
            if (_debugRenderer != null) {
                _debugRenderer.positionCount = _path.vectorPath.Count;
                _debugRenderer.SetPositions(_path.vectorPath.ToArray());
            }
            var distanceToEnd = _movementPlane.ToPlane(SteeringTargetV3 - Tr.position).magnitude + _interpolator.remainingDistance;
            if (distanceToEnd <= _endReachedDistance) {
                MovementCompleted();
            }
        }

        public void SetWander() {
            if (Wandering || (_stuckNode != null && _stuckNode == CurrentGraphNode)) {
                return;
            }
            Wandering = true;
            _waitingForPathCalculation = true;
            var rp = RandomPath.Construct(Tr.position, 7000);
            rp.traversalProvider = _traversal;
            _seeker.StartPath(rp);
        }

        public bool CanReach(Vector3 target) {
            return PathUtilities.IsPathPossible(CurrentGraphNode, AstarPath.active.GetNearest(target, NNConstraint.Default).node);
        }

        //public override bool SetAttackMove(Actor targetActor) {
        //    if (targetActor == null) {
        //        _lastTarget = null;
        //        return false;
        //    }
        //    base.SetAttackMove(targetActor);
        //    GraphHitInfo hit;
        //    _linePositions.Clear();
        //    var target = targetActor.WorldCenter;
        //    if (!CellGridGraph.Current.Linecast(Actor.WorldCenter, target, null, out hit, _linePositions)) {
        //        for (int l = _linePositions.Count - 1; l >= 0; l--) {
        //            if (_linePositions[l].Walkable && _linePositions[l].Tag != IntConst.TagPlayer) {
        //                target = (Vector3) _linePositions[l].position;
        //                break;
        //            }
        //        }
        //    }
        //    _lastTarget = NpcMovementManager.FindFightingPosition(target, CurrentGraphNode, _lastTarget);
        //    if (_lastTarget != null) {
        //        SetMoveTarget((Vector3) _lastTarget);
        //        return true;
        //    }
        //    return false;
        //}

        private void ApplyGravity(float deltaTime) {
            // Apply gravity
            if (_useGravity) {
                float verticalGravity;
                _velocity2D += _movementPlane.ToPlane(deltaTime * Physics.gravity, out verticalGravity);
                _verticalVelocity += verticalGravity;
            }
            else {
                _verticalVelocity = 0;
            }
        }

        private Vector2 CalculateDeltaToMoveThisFrame(Vector2 position, float distanceToEndOfPath, float deltaTime) {
            if (_rvoController != null && _rvoController.enabled) {
                // Use RVOController to get a processed delta position
                // such that collisions will be avoided if possible
                return _movementPlane.ToPlane(_rvoController.CalculateMovementDelta(_movementPlane.ToWorld(position, 0), deltaTime));
            }
            // Direction and distance to move during this frame
            return Vector2.ClampMagnitude(_velocity2D * deltaTime, distanceToEndOfPath);
        }

        private void CancelCurrentPathRequest() {
            _waitingForPathCalculation = false;
            _seeker.CancelCurrentPathRequest();
        }


        private void Move(Vector3 position3D, Vector3 deltaPosition) {
            float lastElevation;
            _movementPlane.ToPlane(position3D, out lastElevation);
            position3D += deltaPosition;
            if (_useGravity) {
                position3D = RaycastPosition(position3D, lastElevation);
            }
            // Assign the final position to the character if we haven't already set it (mostly for performance, setting the position can be slow)
            //if (rigid != null) rigid.MovePosition(position3D);
            Tr.position = position3D;
        }

        private LerpHolder _slowLerp = new LerpHolder();

        private void MovementUpdateInternal(float deltaTime) {
            // a = v/t, should probably expose as a variable
            var acceleration = MaxSpeed / 0.4f;

            // Get our current position. We read from transform.position as few times as possible as it is relatively slow
            // (at least compared to a local variable)
            var currentPosition = Tr.position;

            // Update which point we are moving towards
            _interpolator.MoveToCircleIntersection2D(currentPosition, _pickNextWaypointDist, _movementPlane);
            var dir = _movementPlane.ToPlane(SteeringTargetV3 - currentPosition);

            // Calculate the distance to the end of the path
            var distanceToEnd = dir.magnitude + MathEx.Max(0, _interpolator.remainingDistance);

            // Check if we have reached the target
            var prevTargetReached = TargetReached;
            TargetReached = distanceToEnd <= _endReachedDistance && _interpolator.valid;
            if (!prevTargetReached && TargetReached) {
                MovementCompleted();
            }
            // Check if we have a valid path to follow and some other script has not stopped the character
            float slowdown = 1;
            if (_interpolator.valid) {
                // How fast to move depending on the distance to the destination.
                // Move slower as the character gets closer to the destination.
                // This is always a value between 0 and 1.
                if (distanceToEnd < _slowdownDistance) {
                    slowdown = Mathf.Sqrt(distanceToEnd / _slowdownDistance);
                    _slowLerp.Enabled = false;
                }
                else {
                    if (_interpolator.ShouldSlow()) {
                        if (!_slowLerp.Enabled) {
                            _slowLerp.RestartLerp(1, 0, _slowdownTime);
                        }
                        slowdown = _slowLerp.GetLerpValue();
                    }
                    else {
                        _slowLerp.Enabled = false;
                    }
                }
                if (TargetReached && _goToExactEndPoint) {
                    // Slow down as quickly as possible
                    _velocity2D -= Vector2.ClampMagnitude(_velocity2D, acceleration * deltaTime);
                }
                else {
                    // Normalized direction of where the agent is looking
                    var forwards = _movementPlane.ToPlane(Tr.rotation * Vector3.forward);
                    _velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized * MaxSpeed, _velocity2D, acceleration, _rotationSpeed, MaxSpeed, forwards) * deltaTime;
                }
            }
            else {
                slowdown = 1;
                // Slow down as quickly as possible
                _velocity2D -= Vector2.ClampMagnitude(_velocity2D, acceleration * deltaTime);
            }

            _velocity2D = MovementUtilities.ClampVelocity(
                _velocity2D, MaxSpeed, slowdown, _slowWhenNotFacingTarget,
                _movementPlane.ToPlane(Tr.forward));

            ApplyGravity(deltaTime);

            if (_rvoController != null && _rvoController.enabled) {
                // Send a message to the RVOController that we want to move
                // with this velocity. In the next simulation step, this
                // velocity will be processed and it will be fed back to the
                // rvo controller and finally it will be used by this script
                // when calling the CalculateMovementDelta method below

                // Make sure that we don't move further than to the end point
                // of the path. If the RVO simulation FPS is low and we did
                // not do this, the agent might overshoot the target a lot.
                var rvoTarget = currentPosition + _movementPlane.ToWorld(Vector2.ClampMagnitude(_velocity2D, distanceToEnd), 0f);
                _rvoController.SetTarget(rvoTarget, _velocity2D.magnitude, MaxSpeed);
            }

            Vector2 desiredRotationDirection;
            if (_rvoController != null && _rvoController.enabled) {
                // When using local avoidance, use the actual velocity we are moving with (delta2D/deltaTime) if that velocity
                // is high enough, otherwise fall back to the velocity that we want to move with (velocity2D).
                // The local avoidance velocity can be very jittery when the character is close to standing still
                // as it constantly makes small corrections. We do not want the rotation of the character to be jittery.
                //var actualVelocity = delta2D / deltaTime;
                var actualVelocity = _lastDeltaPosition / _lastDeltaTime;
                desiredRotationDirection = Vector2.Lerp(_velocity2D, actualVelocity, 4 * actualVelocity.magnitude / (MaxSpeed + 0.0001f));
            }
            else {
                desiredRotationDirection = _velocity2D;
            }
            var delta2D = _lastDeltaPosition = CalculateDeltaToMoveThisFrame(_movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);

            // Rotate towards the direction we are moving in
            var currentRotationSpeed = _rotationSpeed * MathEx.Max(0, (slowdown - 0.3f) / 0.7f);
            RotateTowards(desiredRotationDirection, currentRotationSpeed * deltaTime);

            var deltaPosition = _movementPlane.ToWorld(delta2D, _verticalVelocity * deltaTime);
            Move(currentPosition, deltaPosition);
        }

        protected void MovementCompleted() {
            if (_debugRenderer != null) {
                _debugRenderer.positionCount = 0;
            }
        }

        private Vector3 RaycastPosition(Vector3 position, float lastElevation) {
            RaycastHit hit;
            float elevation;
            _movementPlane.ToPlane(position, out elevation);
            var rayLength = LocalCenter.y + MathEx.Max(0, lastElevation - elevation);
            var rayOffset = _movementPlane.ToWorld(Vector2.zero, rayLength);

            if (Physics.Raycast(position + rayOffset, -rayOffset, out hit, rayLength, LayerMasks.Environment, QueryTriggerInteraction.Ignore)) {
                // Grounded
                // Make the vertical velocity fall off exponentially. This is reasonable from a physical standpoint as characters
                // are not completely stiff and touching the ground will not immediately negate all velocity downwards. The AI will
                // stop moving completely due to the raycast penetration test but it will still *try* to move downwards. This helps
                // significantly when moving down along slopes as if the vertical velocity would be set to zero when the character
                // was grounded it would lead to a kind of 'jumping' behavior (try it, it's hard to explain). Ideally this should
                // use a more physically correct formula but this is a good approximation and is much more performant. The constant
                // '5' in the expression below determines how quickly it converges but high values can lead to too much noise.
                _verticalVelocity *= Math.Max(0, 1 - 5 * _lastDeltaTime);
                return hit.point;
            }
            return position;
        }

        private void RotateTowards(Vector2 direction, float maxDegrees) {
            if (direction == Vector2.zero) {
                return;
            }
            var lookTarget = _movementPlane.ToWorld(direction, 0);
            if (lookTarget == Vector3.zero) {
                return;
            }
            var upDirection = _movementPlane.ToWorld(Vector2.zero, 1);
            if (upDirection == Vector3.zero) {
                return;
            }
            if (upDirection == lookTarget) {
                return;
            }
            var targetRotation = Quaternion.LookRotation(lookTarget, upDirection);
            Tr.rotation = Quaternion.RotateTowards(Tr.rotation, targetRotation, maxDegrees);
        }

        private void SearchPath() {
            _pathTimer.Activate();
            _waitingForPathCalculation = true;
            _seeker.CancelCurrentPathRequest();
            var p = ABPath.Construct(Tr.position, MoveTarget);
            p.traversalProvider = _traversal;
            _seeker.StartPath(p);
            //_seeker.StartPath(Tr.position, MoveTarget);
        }

        public void Teleport(Vector3 newPosition, bool clearPath = true) {
            _interpolator.SetPath(null);
            if (_rvoController != null) {
                _rvoController.Move(Vector3.zero);
            }
            if (clearPath) {
                CancelCurrentPathRequest();
                SearchPath();
            }
        }
        
#if UNITY_EDITOR
        protected static readonly Color GizmoColorRaycast = new Color(118.0f / 255, 206.0f / 255, 112.0f / 255);
        protected static readonly Color GizmoColor = new Color(46.0f / 255, 104.0f / 255, 201.0f / 255);
        [NonSerialized] private int _gizmoHash;
        [NonSerialized] private float _lastChangedTime = float.NegativeInfinity;

        protected virtual void OnDrawGizmos() {
            if (!Application.isPlaying) {
                return;
            }
            Gizmos.color = GizmoColorRaycast;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * LocalCenter.y);
            Gizmos.DrawLine(transform.position - transform.right * 0.1f, transform.position + transform.right * 0.1f);
            Gizmos.DrawLine(transform.position - transform.forward * 0.1f, transform.position + transform.forward * 0.1f);
            Draw.Gizmos.CircleXZ(MoveTarget, 0.2f, Color.blue);
            var newGizmoHash = _pickNextWaypointDist.GetHashCode() ^ _slowdownDistance.GetHashCode() ^ _endReachedDistance.GetHashCode();
            if (newGizmoHash != _gizmoHash && _gizmoHash != 0) {
                _lastChangedTime = Time.realtimeSinceStartup;
            }
            _gizmoHash = newGizmoHash;
            var alpha = Mathf.SmoothStep(1, 0, (Time.realtimeSinceStartup - _lastChangedTime - 5f) / 0.5f) * (UnityEditor.Selection.gameObjects.Length == 1 ? 1 : 0);
            if (alpha < 0) {
                return;
            }
            // Make sure the scene view is repainted while the gizmos are visible
            UnityEditor.SceneView.RepaintAll();
            Draw.Gizmos.Line(transform.position, SteeringTargetV3, GizmoColor * new Color(1, 1, 1, alpha));
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation * Quaternion.identity, Vector3.one);
            Draw.Gizmos.CircleXZ(Vector3.zero, _pickNextWaypointDist, GizmoColor * new Color(1, 1, 1, alpha));
            Draw.Gizmos.CircleXZ(Vector3.zero, _slowdownDistance, Color.Lerp(GizmoColor, Color.red, 0.5f) * new Color(1, 1, 1, alpha));
            Draw.Gizmos.CircleXZ(Vector3.zero, _endReachedDistance, Color.Lerp(GizmoColor, Color.red, 0.8f) * new Color(1, 1, 1, alpha));
        }
#endif
    }
}
#endif