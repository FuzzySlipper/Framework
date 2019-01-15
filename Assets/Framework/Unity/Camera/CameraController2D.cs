using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController2D : MonoBehaviour {

    public enum BumperDirection {
        None = 0,
        HorizontalPositive = 1,
        HorizontalNegative = 2,
        VerticalPositive = 4,
        VerticalNegative = 8,
        Horizontal = HorizontalPositive | HorizontalNegative,
        Vertical = VerticalPositive | VerticalNegative,
        AllDirections = Horizontal | Vertical,
    }

    public enum MovementAxis {
        XY,
        XZ,
        YZ
    }

    private class OffsetData {
        public Vector3 StartPointRelativeToCamera { get; set; }
        public Vector3 Vector { get; set; }
        public Vector3 NormalizedVector { get; set; }
        public float DistanceFromStartPoint { get; set; }
    }

    /// <summary>
    ///     The position that the camera is attempting to move to. This is normally the unit's position
    ///     (or midpoint of the targets if there are multiple) plus the sum of all influences.  This is not
    ///     the final position the camera will move to, but the position it would move to if there are no
    ///     CameraBumpers to restrict movement and the camera has sufficient time to move.
    /// </summary>
    public Vector3 CameraSeekPosition {
        get { return CameraSeekTarget.position; }
        set {
            if (!ExclusiveModeEnabled) {
                throw new InvalidOperationException(
                    "Cannot set an explicit camera seek unit unless the camera is in exclusive mode");
            }
            CameraSeekTarget.position = value;
        }
    }

    /// <summary>
    ///     Modifies the "distance" to the unit.  For ortho this modifies the camera's ortho-size, for
    ///     perspective this modifies the actual height value.
    ///     *NOTE*: After changing the DistanceMultiplier the camera bounds will be inaccurate and camera
    ///     bumpers will not work properly.  Please call CalculateScreenBounds before collision with a
    ///     CameraBumper occurs.
    /// </summary>
    /// <value>1 = normal distance, less than one zooms in, more than one zooms out</value>
    public float DistanceMultiplier {
        get { return distanceMultiplier; }
        set {
            distanceMultiplier = value;
            if (GetComponent<Camera>().orthographic) {
                GetComponent<Camera>().orthographicSize = originalZoom * distanceMultiplier;
            }
            else {
                heightFromTarget = originalZoom * distanceMultiplier;
            }
        }
    }

    /// <summary>
    ///     Exclusive mode allows you to position the camera's unit manually.  You must enable exclusive mode before
    ///     attempting to set a CameraSeekPosition.  While in exclusive mode, all influences are ignored.
    /// </summary>
    public bool ExclusiveModeEnabled { get; set; }

    /// <summary>
    ///     Is the camera moving to a new position as the result of a SetTarget or AddTarget?
    /// </summary>
    public bool MovingToNewTarget { get { return panningToNewTarget; } }

    public MovementAxis axis = MovementAxis.XZ;
    public LayerMask cameraBumperLayers;

    /// <summary>
    ///     Where to position the camera, along the "up" vector from the unit.  The "up" vector is determined
    ///     based on the axis that is chosen.
    /// </summary>
    public float heightFromTarget;

    /// <summary>
    ///     Eqivalent to calling AddTarget with this Transform
    /// </summary>
    public Transform initialTarget;

    /// <summary>
    ///     The maximum move speed allowed when moving towards the camera's unit
    /// </summary>
    public float maxMoveSpeedPerSecond = 10;

    /// <summary>
    ///     Damping is a slowing or delay factor for the camera's movement.  Higher damping creates a "squishier"
    ///     feeling camera that lags behind when the unit moves.  To get zero lag use a value of 0.015.
    /// </summary>
    public float damping = .5f;

    /// <summary>
    ///     The distance from the unit where OnNewTargetReached callbacks should be sent.
    /// </summary>
    public float arrivalNotificationDistance = .01f;

    /// <summary>
    ///     This defines a rect in screen space (so 0.5, 0.5 is the center of the screen), that when moved around in, does
    ///     not cause the screen to move.  When exceeding any edge of the push box, the screen will move until the unit
    ///     is once again inside the push box.
    /// </summary>
    public Rect pushBox = new Rect(.5f, .5f, 0, 0);

    /// <summary>
    ///     The camera to use for collision checks with CameraBumpers.
    ///     Defaults to the camera attached to the current GameObject.
    /// </summary>
    public Camera cameraToUse;

    /// <summary>
    ///     Should the movement caculations be performed in FixedUpdate instead of LateUpdate
    /// </summary>
    public bool useFixedUpdate;

    // Called after the initial transition to a unit set via AddTarget or SetTarget
    public Action OnNewTargetReached = null;

    // Called after a new unit has been set via AddTarget
    public Action OnTargetAdded = null;

    // Called after a unit has been switched via SetTarget
    public Action OnTargetSwitched = null;

#if UNITY_EDITOR
    /// <summary>
    ///     Enable editor debug lines.  This increases the cost of many operations and will degrade the performance
    ///     of the camera slightly.
    /// </summary>
    public bool drawDebugLines;
#endif
    public List<Transform> TargetStack = new List<Transform>();

    private const float CAMERA_ARRIVAL_DISTANCE = .01f;
    private const float CAMERA_ARRIVAL_DISTANCE_SQUARED = CAMERA_ARRIVAL_DISTANCE * CAMERA_ARRIVAL_DISTANCE;

    private Transform CameraSeekTarget { get; set; }

    private Func<Vector3> HeightOffset;
    private Func<Vector3, Vector3> GetHorizontalComponent;
    private Func<Vector3, Vector3> GetVerticalComponent;
    private Func<Vector3, float> GetHorizontalValue;
    private Func<Vector3, float> GetVerticalValue;

    private OffsetData leftRaycastPoint;
    private OffsetData upperLeftRaycastPoint;
    private OffsetData lowerLeftRaycastPoint;
    private OffsetData rightRaycastPoint;
    private OffsetData upperRightRaycastPoint;
    private OffsetData lowerRightRaycastPoint;

    private OffsetData upRaycastPoint;
    private OffsetData downRaycastPoint;
    private OffsetData leftUpRaycastPoint;
    private OffsetData rightUpRaycastPoint;
    private OffsetData leftDownRaycastPoint;
    private OffsetData rightDownRaycastPoint;

    private Vector3 velocity;
    private List<Vector3> influences = new List<Vector3>(5);
    private bool panningToNewTarget;
    private float panningToNewTargetSpeed;
    private bool arrivalNotificationSent;
    private Action arrivedAtNewTargetCallback;
    private float arrivalNotificationDistanceSquared;
    private float distanceMultiplier = 1;
    private float originalZoom;
    private float revertAfterDuration;
    private float revertMoveSpeed;
    //private float acceleration;

#if UNITY_EDITOR
    private readonly Color ORANGE_COLOR = new Color(1, .8f, 0);
    private Vector3 lastCalculatedPosition;
    private Vector3 lastCalculatedIdealPosition;
    private Vector3 lastCalculatedInfluence;
    private Vector3[] influencesForGizmoRendering = new Vector3[0];
#endif

    public void SetTarget(Transform target) {
        SetTarget(new[] {
            target
        });
    }

    public void SetTarget(Transform target, Action callback) {
        SetTarget(new[] {
            target
        }, callback);
    }

    public void SetTarget(Transform target, float moveSpeed) {
        SetTarget(new[] {
            target
        }, moveSpeed);
    }

    public void SetTarget(Transform target, float moveSpeed, Action callback) {
        SetTarget(new[] {
            target
        }, moveSpeed, callback);
    }

    public void SetTarget(IEnumerable<Transform> targets) {
        SetTarget(targets, null);
    }

    public void SetTarget(IEnumerable<Transform> targets, Action callback) {
        SetTarget(targets, maxMoveSpeedPerSecond, callback);
    }

    public void SetTarget(IEnumerable<Transform> targets, float moveSpeed) {
        SetTarget(targets, moveSpeed, null);
    }

    public void SetTarget(IEnumerable<Transform> targets, float moveSpeed, Action callback) {
        if (0 == moveSpeed) {
            moveSpeed = maxMoveSpeedPerSecond;
        }
        RemoveCurrentTarget();
        TargetStack.AddRange(targets);
        panningToNewTarget = true;
        panningToNewTargetSpeed = moveSpeed;
        arrivalNotificationSent = false;
        if (OnTargetSwitched != null) {
            OnTargetSwitched();
        }

        if (callback != null) {
            if (arrivedAtNewTargetCallback != null) {
                arrivedAtNewTargetCallback += callback;
            }
            else {
                arrivedAtNewTargetCallback = callback;
            }
        }
    }

    public void AddTarget(Transform target) {
        AddTarget(new[] {
            target
        });
    }

    public void AddTarget(Transform target, Action callback) {
        AddTarget(new[] {
            target
        }, callback);
    }

    public void AddTarget(Transform target, float moveSpeed) {
        AddTarget(new[] {
            target
        }, moveSpeed);
    }

    public void AddTarget(Transform target, float moveSpeed, Action callback) {
        AddTarget(new[] {
            target
        }, moveSpeed, callback);
    }

    public void AddTarget(Transform target, float moveSpeed, float revertAfterDuration, float revertMoveSpeed) {
        AddTarget(new[] {
            target
        }, moveSpeed, revertAfterDuration, revertMoveSpeed);
    }

    public void AddTarget(Transform target, float moveSpeed, float revertAfterDuration, float revertMoveSpeed,
        Action callback) {
        AddTarget(new[] {
            target
        }, moveSpeed, revertAfterDuration, revertMoveSpeed, callback);
    }

    public void AddTarget(IEnumerable<Transform> targets) {
        AddTarget(targets, maxMoveSpeedPerSecond);
    }

    public void AddTarget(IEnumerable<Transform> targets, Action callback) {
        AddTarget(targets, maxMoveSpeedPerSecond, callback);
    }

    public void AddTarget(IEnumerable<Transform> targets, float moveSpeed) {
        AddTarget(targets, moveSpeed, null);
    }

    /// <summary>
    ///     Adds one or more targets to the unit stack.  Each variation of AddTarget has a version that takes a single
    ///     Transform as well as an IEnumerable
    ///     <Transform>.  <seealso cref="AddTarget(Transform unit, float moveSpeed, System.Action callback)">
    /// </summary>
    /// <param name="targets">An IEnumerable<Transform> of targets to be focused.</param>
    /// <param name="moveSpeed">The speed to move while transitioning to the new unit.</param>
    /// <param name="callback">A System.Action to be run when arriving at the new unit.</param>
    public void AddTarget(IEnumerable<Transform> targets, float moveSpeed, Action callback) {
        if (0 == moveSpeed) {
            moveSpeed = maxMoveSpeedPerSecond;
        }
        TargetStack.AddRange(targets);
        panningToNewTarget = true;
        panningToNewTargetSpeed = moveSpeed;
        arrivalNotificationSent = false;
        if (OnTargetAdded != null) {
            OnTargetAdded();
        }
        if (callback != null) {
            if (arrivedAtNewTargetCallback != null) {
                arrivedAtNewTargetCallback += callback;
            }
            else {
                arrivedAtNewTargetCallback = callback;
            }
        }
    }

    public void AddTarget(IEnumerable<Transform> targets, float moveSpeed, float revertAfterDuration,
        float revertMoveSpeed) {
        AddTarget(targets, moveSpeed, revertAfterDuration, revertMoveSpeed, null);
    }

    /// <summary>
    ///     Adds one or more targets to the unit stack.  Each variation of AddTarget has a version that takes a single
    ///     Transform as well as an IEnumerable
    ///     <Transform>
    ///         .
    ///         <seealso
    ///             cref="AddTarget(Transform unit, float moveSpeed, float revertAfterDuration, float revertMoveSpeed, System.Action callback)">
    /// </summary>
    /// <param name="targets">An IEnumerable<Transform> of targets to be focused.</param>
    /// <param name="moveSpeed">The speed to move while transitioning to the new unit.</param>
    /// <param name="revertAfterDuration">Should the newly added unit be automatically removed after a duration.</param>
    /// <param name="revertMoveSpeed">The speed to move when returning to the original unit.</param>
    /// <param name="callback">A System.Action to be run when arriving at the new unit.</param>
    public void AddTarget(IEnumerable<Transform> targets, float moveSpeed, float revertAfterDuration, float revertMoveSpeed, Action callback) {
        if (0 == moveSpeed) {
            moveSpeed = maxMoveSpeedPerSecond;
        }
        TargetStack.AddRange(targets);
        panningToNewTarget = true;
        panningToNewTargetSpeed = moveSpeed;
        arrivalNotificationSent = false;
        this.revertAfterDuration = revertAfterDuration;
        this.revertMoveSpeed = revertMoveSpeed;
        OnNewTargetReached += RevertAfterReachingTarget;
        if (OnTargetAdded != null) {
            OnTargetAdded();
        }

        if (callback != null) {
            if (arrivedAtNewTargetCallback != null) {
                arrivedAtNewTargetCallback += callback;
            }
            else {
                arrivedAtNewTargetCallback = callback;
            }
        }
    }

    public void RemoveCurrentTarget() {
        if (TargetStack.Count == 0) {
            return;
        }
        TargetStack.RemoveAt(0);
        panningToNewTarget = true;
        panningToNewTargetSpeed = maxMoveSpeedPerSecond;
        arrivalNotificationSent = false;
    }

    public void AddInfluence(Vector3 influence) {
        influences.Add(influence);
    }

    public void JumpToIdealPosition() {
        if (!ExclusiveModeEnabled) {
            throw new InvalidOperationException(
                "Cannot set an explicit camera position unless the camera is in exclusive mode");
        }
        transform.position = IdealCameraPosition();
    }

    public void JumpToTargetRespectingBumpersAndInfluences() {
        Vector3 idealPosition = IdealCameraPosition() + TotalInfluence();
        transform.position = idealPosition + CalculatePushBackOffset(idealPosition);
    }

    // This must be Awake and not start to ensure that all the delegates are assigned before scripts attempt to perform
    // any actions on the camera such as SetTarget or AddTarget.
    public void Awake() {
        if (null == cameraToUse) {
            cameraToUse = GetComponent<Camera>();
        }
        if (null == cameraToUse) {
            Debug.LogError(
                "No camera was specified and no Camera component is attached to this GameObject, CameraController2D requires a camera to function");
        }

        switch (axis) {
            case MovementAxis.XY:
                HeightOffset = () => Vector3.forward * heightFromTarget;
                GetHorizontalComponent = vector => new Vector3(vector.x, 0, 0);
                GetHorizontalValue = vector => vector.x;
                GetVerticalComponent = vector => new Vector3(0, vector.y, 0);
                GetVerticalValue = vector => vector.y;
                break;
            case MovementAxis.XZ:
                HeightOffset = () => -Vector3.up * heightFromTarget;
                GetHorizontalComponent = vector => new Vector3(vector.x, 0, 0);
                GetHorizontalValue = vector => vector.x;
                GetVerticalComponent = vector => new Vector3(0, 0, vector.z);
                GetVerticalValue = vector => vector.z;
                break;
            case MovementAxis.YZ:
                HeightOffset = () => -Vector3.right * heightFromTarget;
                GetHorizontalComponent = vector => new Vector3(0, 0, vector.z);
                GetHorizontalValue = vector => vector.z;
                GetVerticalComponent = vector => new Vector3(0, vector.y, 0);
                GetVerticalValue = vector => vector.y;
                break;
        }

        CameraSeekTarget = new GameObject("_CameraTarget").transform;

        if (cameraToUse.orthographic) {
            originalZoom = cameraToUse.orthographicSize;
        }
        else {
            originalZoom = heightFromTarget;
        }
        CalculateScreenBounds();

        if (initialTarget != null) {
            AddTarget(initialTarget);
            JumpToTargetRespectingBumpersAndInfluences();
        }

        arrivalNotificationDistanceSquared = arrivalNotificationDistance * arrivalNotificationDistance;
    }

    public void FixedUpdate() {
        if (useFixedUpdate) {
            ApplyMovement();
        }
    }

    public void LateUpdate() {
        if (!useFixedUpdate) {
            ApplyMovement();
        }
    }

    /// <summary>
    ///     This calculates the points to be used when determining collision with CameraBumper objects.
    ///     Typically this only needs to be run once, but if the DistanceMultiplier is changed this
    ///     will need to be run before the next collision with a CameraBumper.
    /// </summary>
    public void CalculateScreenBounds() {
        Func<Vector3, Vector3, OffsetData> AddRaycastOffsetPoint = (viewSpaceOrigin, viewSpacePoint) => {
            if (cameraToUse.orthographic) {
                Vector3 origin = cameraToUse.ViewportToWorldPoint(viewSpaceOrigin);
                Vector3 vectorToOffset = cameraToUse.ViewportToWorldPoint(viewSpacePoint) - origin;
                return new OffsetData {
                    StartPointRelativeToCamera = origin - transform.position,
                    Vector = vectorToOffset,
                    NormalizedVector = vectorToOffset.normalized,
                    DistanceFromStartPoint = vectorToOffset.magnitude
                };
            }
            else {
                Vector3 cameraPositionOnPlane = transform.position + (transform.forward * heightFromTarget);

                Ray originRay = cameraToUse.ViewportPointToRay(viewSpaceOrigin);
                float theta = Vector3.Angle(transform.forward, originRay.direction);
                float distanceToPlane = heightFromTarget / Mathf.Cos(theta * Mathf.Deg2Rad);
                Vector3 originPointOnPlane = originRay.origin + (originRay.direction * distanceToPlane);

                Ray pointRay = cameraToUse.ViewportPointToRay(viewSpacePoint);
                theta = Vector3.Angle(cameraToUse.transform.forward, pointRay.direction);
                distanceToPlane = heightFromTarget / Mathf.Cos(theta * Mathf.Deg2Rad);
                Vector3 pointOnPlane = pointRay.origin + (pointRay.direction * distanceToPlane);
                Vector3 vectorToOffset = pointOnPlane - originPointOnPlane;

                return new OffsetData {
                    StartPointRelativeToCamera = originPointOnPlane - cameraPositionOnPlane,
                    Vector = vectorToOffset,
                    NormalizedVector = vectorToOffset.normalized,
                    DistanceFromStartPoint = vectorToOffset.magnitude
                };
            }
        };

        leftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0, 0.5f));
        rightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(1, 0.5f));
        lowerLeftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(0, 0));
        lowerRightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(1, 0));
        upperLeftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(0, 1));
        upperRightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(1, 1));

        downRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 0));
        upRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 1));
        leftUpRaycastPoint = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 1));
        leftDownRaycastPoint = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 0));
        rightUpRaycastPoint = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 1));
        rightDownRaycastPoint = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 0));
    }

    private void ApplyMovement() {
        Vector3 position = transform.position;

        if (!ExclusiveModeEnabled) {
            CameraSeekTarget.position = IdealCameraPosition() + TotalInfluence();
        }
        Vector3 idealPosition = CameraSeekPosition;
        Vector3 targetPosition = idealPosition + CalculatePushBackOffset(idealPosition);

        Vector3 targetViewportPoint = cameraToUse.WorldToViewportPoint(targetPosition);
        bool targetWithinMoveBox = TargetViewportPointWithinMoveBox(targetViewportPoint);

        if (panningToNewTarget || !targetWithinMoveBox) {
            float maxSpeed = maxMoveSpeedPerSecond;
            if (panningToNewTarget) {
                maxSpeed = panningToNewTargetSpeed;
            }
            if (!targetWithinMoveBox && !panningToNewTarget) {
                var topLeftPoint = new Vector2(pushBox.x - (pushBox.width / 2), pushBox.y + (pushBox.height / 2));
                var bottomRightPoint = new Vector2(pushBox.x + (pushBox.width / 2), pushBox.y - (pushBox.height / 2));
                // move unit to edge of move box instead of moving as far as we are able to based on deltaTime
                // to avoid having a 3 no move, 1 move update stuttering pattern

                float xDifference = 0f;
                float yDifference = 0f;
                if (targetViewportPoint.x < topLeftPoint.x || targetViewportPoint.x > bottomRightPoint.x) {
                    xDifference = Mathf.Abs(targetViewportPoint.x - pushBox.x) / (pushBox.width / 2);
                }
                if (targetViewportPoint.y > topLeftPoint.y || targetViewportPoint.y < bottomRightPoint.y) {
                    yDifference = Mathf.Abs(targetViewportPoint.y - pushBox.y) / (pushBox.height / 2);
                }

                float scaleFactor;

                if (xDifference > yDifference) {
                    scaleFactor = 1 - (1 / xDifference);
                }
                else {
                    scaleFactor = 1 - (1 / yDifference);
                }

                targetPosition = transform.position + ((targetPosition - position) * scaleFactor);
            }

            Vector3 vectorToTarget = targetPosition - position;
            Vector3 vectorToTargetAlongPlane = GetHorizontalComponent(vectorToTarget) +
                                               GetVerticalComponent(vectorToTarget);
            Vector3 interpolatedPosition = Vector3.zero;
            float xDelta = Mathf.Abs(vectorToTargetAlongPlane.x);
            float yDelta = Mathf.Abs(vectorToTargetAlongPlane.y);
            float zDelta = Mathf.Abs(vectorToTargetAlongPlane.z);
            float xyzTotal = xDelta + yDelta + zDelta;

            interpolatedPosition.x = Mathf.SmoothDamp(position.x, targetPosition.x, ref velocity.x, damping,
                maxSpeed * (xDelta / xyzTotal));
            interpolatedPosition.y = Mathf.SmoothDamp(position.y, targetPosition.y, ref velocity.y, damping,
                maxSpeed * (yDelta / xyzTotal));
            interpolatedPosition.z = Mathf.SmoothDamp(position.z, targetPosition.z, ref velocity.z, damping,
                maxSpeed * (zDelta / xyzTotal));
            transform.position = interpolatedPosition;
        }
        else {
            velocity = Vector3.zero;
        }

#if UNITY_EDITOR
        if (drawDebugLines) {
            lastCalculatedPosition = transform.position;
            lastCalculatedIdealPosition = IdealCameraPosition();
            lastCalculatedInfluence = TotalInfluence();
            influencesForGizmoRendering = new Vector3[influences.Count];
            influences.CopyTo(influencesForGizmoRendering);
        }
#endif

        if (!arrivalNotificationSent && panningToNewTarget &&
            (targetPosition - position).sqrMagnitude <= arrivalNotificationDistanceSquared) {
            if (OnNewTargetReached != null) {
                OnNewTargetReached();
            }
            if (arrivedAtNewTargetCallback != null) {
                arrivedAtNewTargetCallback();
                arrivedAtNewTargetCallback = null;
            }
            arrivalNotificationSent = true;
            panningToNewTarget = false;
        }

        influences.Clear();
    }

    private Vector3 CalculatePushBackOffset(Vector3 idealPosition) {
        Vector3 idealCenterPointAtPlayerHeight = idealPosition + HeightOffset();
        Vector3 horizontalVector = GetHorizontalComponent(Vector3.one).normalized;
        Vector3 verticalVector = GetVerticalComponent(Vector3.one).normalized;
        int horizontalFacing = 0;
        int verticalFacing = 0;
        float horizontalPushBack = 0f;
        float verticalPushBack = 0f;
        float rightHorizontalPushBack = 0f;
        float leftHorizontalPushBack = 0f;
        float upVerticalPushBack = 0f;
        float downVerticalPushBack = 0f;

        rightHorizontalPushBack = CalculatePushback(rightRaycastPoint, idealCenterPointAtPlayerHeight,
            BumperDirection.HorizontalPositive);
        if (rightHorizontalPushBack > horizontalPushBack) {
            horizontalPushBack = rightHorizontalPushBack;
            horizontalFacing = 1;
        }
        if (0 == rightHorizontalPushBack) {
            upVerticalPushBack = CalculatePushback(rightUpRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.VerticalPositive);
            if (upVerticalPushBack > verticalPushBack) {
                verticalPushBack = upVerticalPushBack;
                verticalFacing = 1;
            }
            downVerticalPushBack = CalculatePushback(rightDownRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.VerticalNegative);
            if (downVerticalPushBack > verticalPushBack) {
                verticalPushBack = downVerticalPushBack;
                verticalFacing = -1;
            }
        }
        leftHorizontalPushBack = CalculatePushback(leftRaycastPoint, idealCenterPointAtPlayerHeight,
            BumperDirection.HorizontalNegative);
        if (leftHorizontalPushBack > horizontalPushBack) {
            horizontalPushBack = leftHorizontalPushBack;
            horizontalFacing = -1;
        }
        if (0 == leftHorizontalPushBack) {
            upVerticalPushBack = CalculatePushback(leftUpRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.VerticalPositive);
            if (upVerticalPushBack > verticalPushBack) {
                verticalPushBack = upVerticalPushBack;
                verticalFacing = 1;
            }
            downVerticalPushBack = CalculatePushback(leftDownRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.VerticalNegative);
            if (downVerticalPushBack > verticalPushBack) {
                verticalPushBack = downVerticalPushBack;
                verticalFacing = -1;
            }
        }
        upVerticalPushBack = CalculatePushback(upRaycastPoint, idealCenterPointAtPlayerHeight,
            BumperDirection.VerticalPositive);
        if (upVerticalPushBack > verticalPushBack) {
            verticalPushBack = upVerticalPushBack;
            verticalFacing = 1;
        }
        if (0 == upVerticalPushBack) {
            rightHorizontalPushBack = CalculatePushback(upperRightRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.HorizontalPositive);
            if (rightHorizontalPushBack > horizontalPushBack) {
                horizontalPushBack = rightHorizontalPushBack;
                horizontalFacing = 1;
            }
            leftHorizontalPushBack = CalculatePushback(upperLeftRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.HorizontalNegative);
            if (leftHorizontalPushBack > horizontalPushBack) {
                horizontalPushBack = leftHorizontalPushBack;
                horizontalFacing = -1;
            }
        }
        downVerticalPushBack = CalculatePushback(downRaycastPoint, idealCenterPointAtPlayerHeight,
            BumperDirection.VerticalNegative);
        if (downVerticalPushBack > verticalPushBack) {
            verticalPushBack = downVerticalPushBack;
            verticalFacing = -1;
        }
        if (0 == downVerticalPushBack) {
            rightHorizontalPushBack = CalculatePushback(lowerRightRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.HorizontalPositive);
            if (rightHorizontalPushBack > horizontalPushBack) {
                horizontalPushBack = rightHorizontalPushBack;
                horizontalFacing = 1;
            }
            leftHorizontalPushBack = CalculatePushback(lowerLeftRaycastPoint, idealCenterPointAtPlayerHeight,
                BumperDirection.HorizontalNegative);
            if (leftHorizontalPushBack > horizontalPushBack) {
                horizontalPushBack = leftHorizontalPushBack;
                horizontalFacing = -1;
            }
        }
        return (verticalVector * -verticalPushBack * verticalFacing) +
               (horizontalVector * -horizontalPushBack * horizontalFacing);
    }

    private float CalculatePushback(OffsetData offset, Vector3 idealCenterPoint, BumperDirection validDirections = BumperDirection.AllDirections) {
        RaycastHit hitInfo;
        float pushbackDueToCollision = 0f;

        if (Physics.Raycast(idealCenterPoint + offset.StartPointRelativeToCamera, offset.NormalizedVector, out hitInfo,
            offset.DistanceFromStartPoint, cameraBumperLayers)) {
            pushbackDueToCollision = offset.DistanceFromStartPoint - hitInfo.distance;
#if UNITY_EDITOR
            if (drawDebugLines) {
                Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera,
                    idealCenterPoint + offset.StartPointRelativeToCamera +
                    (offset.NormalizedVector * hitInfo.distance), Color.red);
            }
#endif
        }
#if UNITY_EDITOR
        else if (drawDebugLines) {
            Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera,
                idealCenterPoint + offset.StartPointRelativeToCamera + offset.Vector, Color.green);
        }
#endif

        return pushbackDueToCollision;
    }

    private Vector3 TotalInfluence() {
        var totalVector3 = Vector3.zero;
        for (int i = 0; i < influences.Count; i++) {
            totalVector3 += influences[i];
        }
        return totalVector3;
        //return influences.Aggregate(Vector3.zero, (offset, influence) => offset + influence);
    }

    private Vector3 IdealCameraPosition() {
        for (int i = TargetStack.Count - 1; i >= 0; i--) {
            if (TargetStack[i] == null) {
                TargetStack.RemoveAt(i);
            }
        }
        if (TargetStack.Count == 0) {
            return Vector3.zero;
        }
        if (TargetStack.Count == 1) {
            return TargetStack[0].position - HeightOffset();
        }
        float minHorizontal = float.MaxValue;
        float maxHorizontal = float.MinValue;
        float minVertical = float.MaxValue;
        float maxVertical = float.MinValue;
        for (int i = 0; i < TargetStack.Count; i++) {
            var horizontal = GetHorizontalValue(TargetStack[i].position);
            if (horizontal < minHorizontal) {
                minHorizontal = horizontal;
            }
            if (horizontal > maxHorizontal) {
                maxHorizontal = horizontal;
            }
            var vertical = GetVerticalValue(TargetStack[i].position);
            if (vertical < minVertical) {
                minVertical = vertical;
            }
            if (vertical > maxVertical) {
                maxVertical = vertical;
            }
        }
        float horizontalOffset = (maxHorizontal - minHorizontal) * 0.5f;
        float verticalOffset = (maxVertical - minVertical) * 0.5f;
        return (GetHorizontalComponent(Vector3.one) * (minHorizontal + horizontalOffset)) +
               (GetVerticalComponent(Vector3.one) * (minVertical + verticalOffset)) - HeightOffset();
    }

    private void RevertAfterReachingTarget() {
        StartCoroutine(RemoveTargetAfterDelay(revertAfterDuration, revertMoveSpeed));
        OnNewTargetReached -= RevertAfterReachingTarget;
    }

    private IEnumerator RemoveTargetAfterDelay(float delay, float revertMoveSpeed) {
        yield return new WaitForSeconds(delay);
        panningToNewTarget = true;
        panningToNewTargetSpeed = revertMoveSpeed;
        arrivalNotificationSent = false;
        TargetStack.RemoveAt(0);
    }

    private bool TargetViewportPointWithinMoveBox(Vector3 target) {
        var topLeftPoint = new Vector2(pushBox.x - (pushBox.width / 2), pushBox.y + (pushBox.height / 2));
        var bottomRightPoint = new Vector2(pushBox.x + (pushBox.width / 2), pushBox.y - (pushBox.height / 2));
        return target.x >= topLeftPoint.x && target.x <= bottomRightPoint.x && target.y <= topLeftPoint.y &&
               target.y >= bottomRightPoint.y;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (Application.isPlaying && drawDebugLines) {
            if (!ExclusiveModeEnabled) {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(lastCalculatedIdealPosition, lastCalculatedIdealPosition + lastCalculatedInfluence);

                Gizmos.color = ORANGE_COLOR;
                var influence = Vector3.zero;
                for (int i = 0; i < influencesForGizmoRendering.Length; i++) {
                    influence += influencesForGizmoRendering[i];
                }
                Gizmos.DrawLine(lastCalculatedIdealPosition, lastCalculatedIdealPosition + influence);
            }

            Gizmos.color = ORANGE_COLOR;
            Gizmos.DrawWireSphere(lastCalculatedIdealPosition, .1f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastCalculatedPosition, .1f);

            Gizmos.color = Color.blue;
            var topLeftPoint = new Vector2(pushBox.x - (pushBox.width / 2), pushBox.y + (pushBox.height / 2));
            var bottomRightPoint = new Vector2(pushBox.x + (pushBox.width / 2), pushBox.y - (pushBox.height / 2));

            Gizmos.DrawLine(cameraToUse.ViewportToWorldPoint(new Vector3(topLeftPoint.x, topLeftPoint.y)),
                cameraToUse.ViewportToWorldPoint(new Vector3(bottomRightPoint.x, topLeftPoint.y)));
            Gizmos.DrawLine(cameraToUse.ViewportToWorldPoint(new Vector3(topLeftPoint.x, bottomRightPoint.y)),
                cameraToUse.ViewportToWorldPoint(new Vector3(bottomRightPoint.x, bottomRightPoint.y)));

            Gizmos.DrawLine(cameraToUse.ViewportToWorldPoint(new Vector3(topLeftPoint.x, topLeftPoint.y)),
                cameraToUse.ViewportToWorldPoint(new Vector3(topLeftPoint.x, bottomRightPoint.y)));
            Gizmos.DrawLine(cameraToUse.ViewportToWorldPoint(new Vector3(bottomRightPoint.x, topLeftPoint.y)),
                cameraToUse.ViewportToWorldPoint(new Vector3(bottomRightPoint.x, bottomRightPoint.y)));
        }
    }

#endif
}