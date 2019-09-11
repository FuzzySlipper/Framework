using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PoseAnimationHelper : MonoSingleton<PoseAnimationHelper> {

        [SerializeField] private Transform _positionTracker = null;
        [SerializeField] private Transform[] _possibleTrackers = new Transform[0];
        [SerializeField] private PoseAnimator _poseAnimator = null;

        public Transform[] PossibleTrackers { get => _possibleTrackers; }
        public static Transform PositionTracker { get { return main._positionTracker; } }
        public static PoseAnimator PoseAnimator { get { return main._poseAnimator; } }
    }
}
