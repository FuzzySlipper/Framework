using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Playables;

namespace PixelComrades {
    
    [System.Serializable]
    public class WeaponTweenBehaviour: PlayableBehaviour {

        public EasingTypes EaseType = EasingTypes.Linear;
        public Vector3 TargetPosition;
        public Quaternion TargetRotation;
        public Transform TargetTr;

        private bool _started = false;
        private float _startTime;
        private float _duration;
        private Vector3 _startPos;
        private Quaternion _startRot;
        private Func<float, float, float, float> _easeFunc;

        public override void OnGraphStart(Playable playable) {
            base.OnGraphStart(playable);
            _started = false;
        }

        public override void OnGraphStop(Playable playable) {
            base.OnGraphStop(playable);
            _started = false;
        }

        public override void OnPlayableCreate(Playable playable) {
            base.OnPlayableCreate(playable);
            _started = false;
        }

        public override void PrepareFrame(Playable playable, FrameData info) {
            base.PrepareFrame(playable, info);
            if (!_started) {
                Start(playable, info);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            base.ProcessFrame(playable, info, playerData);
            if (!_started) {
                return;
            }
            var percent =  _easeFunc(0, 1,(TimeManager.Time - _startTime) / _duration);
            TargetTr.localPosition = Vector3.Lerp(_startPos, TargetPosition, percent);
            TargetTr.localRotation = Quaternion.Slerp(_startRot, TargetRotation, percent);
        }

        private void Start(Playable playable, FrameData info) {
            if ((info.frameId == 0) || (info.deltaTime > 0)) {
                _started = true;
                _duration = (float) playable.GetDuration();
                _easeFunc = Easing.Function(EaseType);
                _startTime = TimeManager.Time;
                _startPos = TargetTr.localPosition;
                _startRot = TargetTr.localRotation;
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info) {
            base.OnBehaviourPlay(playable, info);
            Start(playable, info);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            base.OnBehaviourPause(playable, info);
            _started = false;
        }
    }

    public class WeaponTweenPlayable : PlayableAsset {

        public EasingTypes EaseType = EasingTypes.Linear;
        public Vector3 TargetPosition;
        public Vector3 TargetRotation;
        public bool IsPrimary = false;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<WeaponTweenBehaviour>.Create(graph);
            playable.SetDuration(0.25);
            var behavior = playable.GetBehaviour();
            behavior.EaseType = EaseType;
            behavior.TargetPosition = TargetPosition;
            behavior.TargetRotation = Quaternion.Euler(TargetRotation);
            behavior.TargetTr = IsPrimary ? PoseAnimator.Main.PrimaryPivot : PoseAnimator.Main.SecondaryPivot;
            return playable;
        }

        [Button]
        public void CopyMainPosition() {
            if (PoseAnimator.Main == null) {
                return;
            }
            var tr = IsPrimary ? PoseAnimator.Main.PrimaryPivot : PoseAnimator.Main.SecondaryPivot;
            TargetPosition = tr.localPosition;
            TargetRotation = tr.localRotation.eulerAngles;
        }

        [Button]
        public void CopyPositionTracker() {
            if (PoseAnimationHelper.PositionTracker == null) {
                return;
            }
            var tr = IsPrimary ? PoseAnimator.Main.PrimaryPivot : PoseAnimator.Main.SecondaryPivot;
            TargetPosition = tr.InverseTransformPoint(PoseAnimationHelper.PositionTracker.position);
            TargetRotation = (Quaternion.Inverse(tr.parent.rotation) * PoseAnimationHelper.PositionTracker.rotation).eulerAngles;
        }
    }
}
