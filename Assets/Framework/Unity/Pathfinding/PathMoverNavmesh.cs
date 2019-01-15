using UnityEngine;
using UnityEngine.AI;

namespace PixelComrades {
    public class PathMoverNavmesh : MonoBehaviour {

        [SerializeField] private float _wanderDistance = 10;
        
        private NavMeshAgent _agent;
        private NavMeshQueryFilter? _filter;
        private float _normalSpeed;

        public Vector3 MoveTarget;
        public bool IsMoving;
        public Transform Tr { get { return transform; } }

        public void Setup(NavMeshAgent agent) {
            _agent = agent;
            _normalSpeed = _agent.speed;
        }

        public void OnPoolDespawned() {
            _filter = null;
        }

        private void CanMoveCheck() {
            if (!_agent.isOnNavMesh) {
                return;
            }
            //if (!Actor.CanMove) {
            //    _agent.isStopped = true;
            //}
            //else if (Active && Actor.CanMove && _agent.isStopped) {
            //    SetMovementDestination(MoveTarget);
            //}
        }

        public bool CanReach(Vector3 target) {
            NavMeshHit navHit;
            if (SamplePosition(target, 6, out navHit)) {
                //if (target.SqrDistanceXz(navHit.position) < 5) {
                //    return true;
                //}
                return true;
            }
            return false;
        }

        public void Teleport(Vector3 newPosition, bool clearPath = true) {
            if (_agent.isOnNavMesh) {
                _agent.Warp(newPosition);
            }
        }

        public void SetMoveTarget(Vector3 moveTarget) {
            _agent.speed = _normalSpeed;
            SetMovementDestination(moveTarget);
        }

        public void SetWander() {
            _agent.speed = _normalSpeed * 0.5f;
            FindWanderPosition();
        }

        public void Stop() {
            _agent.speed = _normalSpeed;
            _agent.isStopped = true;
        }

        public void ActorDied() {
            _agent.speed = _normalSpeed;
            _agent.isStopped = true;
        }

        //public override bool SetAttackMove(Actor targetActor) {
        //    if (targetActor == null) {
        //        return false;
        //    }
        //    for (int i = 0; i < _attackLookDistance; i++) {
        //        NavMeshHit navHit;
        //        if (SamplePosition(targetActor.WorldCenter, i + 0.1f, out navHit)) {
        //            _agent.speed = _normalSpeed;
        //            SetMovementDestination(navHit.position);
        //            break;
        //        }
        //    }
        //    return false;
        //}

        private void FindWanderPosition() {
            var limiter = 0;
            while (limiter < 50) {
                Vector3 randDirection = Random.insideUnitSphere * _wanderDistance;
                randDirection += Tr.position;
                NavMeshHit navHit;
                if (SamplePosition(randDirection, 5, out navHit)) {
                    SetMovementDestination(navHit.position);
                    break;
                }
                limiter++;
            }
        }

        private bool SamplePosition(Vector3 pos, float dist, out NavMeshHit hit) {
            if (_filter == null) {
                _filter = new NavMeshQueryFilter() {
                    areaMask = -1,
                    agentTypeID = _agent.agentTypeID
                };
            }
            if (NavMesh.SamplePosition(pos, out hit, dist, _filter.Value)) {
                return true;
            }
            return false;
        }

        private void SetMovementDestination(Vector3 target) {
            if (!_agent.isOnNavMesh) {
                NavMeshHit navHit;
                if (SamplePosition(Tr.position, 10, out navHit)) {
                    Teleport(navHit.position);
                }
                else {
                    return;
                }
            }
            MoveTarget = target;
            if (!gameObject.activeInHierarchy) {
                return;
            }
            //if (!Actor.CanMove) {
            //    IsMoving = false;
            //    _agent.isStopped = true;
            //    return;
            //}
            IsMoving = true;
            _agent.isStopped = false;
            _agent.SetDestination(target);
        }

    }
}