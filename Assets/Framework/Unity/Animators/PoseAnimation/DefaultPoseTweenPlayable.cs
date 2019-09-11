using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace PixelComrades {
    
    [System.Serializable]
    public class DefaultPoseTweenBehaviour : PlayableBehaviour {

        public AnimationCurve Curve;
        public PoseAnimator Animator;
        private GenericPool<SavedMuscleInstance> _musclePool = new GenericPool<SavedMuscleInstance>(20);
        private List<SavedMuscleInstance> _currentMuscles = new List<SavedMuscleInstance>();

        private bool _started = false;
        private float _duration;

        private void ClearSavedMuscles() {
            for (int i = 0; i < _currentMuscles.Count; i++) {
                _musclePool.Store(_currentMuscles[i]);
            }
            _currentMuscles.Clear();
        }

        private void SetupPoseTransition(MusclePose pose) {
            ClearSavedMuscles();
            for (int i = 0; i < pose.Pose.Count; i++) {
                var muscle = _musclePool.New();
                var savedMuscle = pose.Pose[i];
                muscle.Set(savedMuscle, Animator.HumanPose.muscles[savedMuscle.MuscleIndex]);
                _currentMuscles.Add(muscle);
            }
        }

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
                Setup(playable);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            base.ProcessFrame(playable, info, playerData);
            if (!_started) {
                Setup(playable);
            }
            var percent = Curve.Evaluate((float) playable.GetTime() / _duration);
            Lerp(percent);
        }

        private void Lerp(float percent) {
            for (int i = 0; i < _currentMuscles.Count; i++) {
                var muscle = _currentMuscles[i];
                Animator.HumanPose.muscles[muscle.MuscleIndex] = Mathf.Lerp(muscle.Start, muscle.Target, percent);
            }
            Animator.RefreshPose();
        }

        private void Setup(Playable playable) {
            _started = true;
            _duration = (float) playable.GetDuration();
            Animator = PoseAnimator.Main;
            Animator.UpdatePose();
            SetupPoseTransition(Animator.DefaultPose);
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info) {
            base.OnBehaviourPlay(playable, info);
            Setup(playable);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            base.OnBehaviourPause(playable, info);
            if (_started) {
                Lerp(1);
            }
            _started = false;
        }
    }

    public class DefaultPoseTweenPlayable : PlayableAsset {

        public AnimationCurve Curve;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<DefaultPoseTweenBehaviour>.Create(graph);
            playable.SetDuration(0.25);
            var behavior = playable.GetBehaviour();
            behavior.Curve = Curve;
            return playable;
        }
    }
}
