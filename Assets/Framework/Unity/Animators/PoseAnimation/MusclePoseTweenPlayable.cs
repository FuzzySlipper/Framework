using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Playables;

namespace PixelComrades {
    [System.Serializable]
    public class MusclePoseTweenBehaviour: PlayableBehaviour {

        public AnimationCurve Curve;
        public List<SavedMuscleInstance> Pose = new List<SavedMuscleInstance>();
        public PoseAnimator Animator;
        public bool UpdateStart;

        private bool _started = false;
        private float _duration;

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
            //var percent =  _easeFunc(0, 1,(TimeManager.Time - _startTime) / _duration);
            //var percent = _easeFunc(0, 1, (float) playable.GetTime() / _duration);
            var percent = Curve.Evaluate((float) playable.GetTime() / _duration);
            Lerp(percent);
            //Debug.Log(((float) playable.GetTime()).ToString("F2") + " " + _duration.ToString("F2") + " " + ((float) +percent));
        }

        private void Lerp(float percent) {
            for (int i = 0; i < Pose.Count; i++) {
                var muscle = Pose[i];
                Animator.HumanPose.muscles[muscle.MuscleIndex] = Mathf.Lerp(muscle.Start, muscle.Target, percent);
            }
            Animator.RefreshPose();
        }

        private void Setup(Playable playable) {
            _started = true;
            _duration = (float) playable.GetDuration();
            Animator = PoseAnimator.Main;
            Animator.UpdatePose();
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif
            if (!UpdateStart) {
                return;
            }
            for (int i = 0; i < Pose.Count; i++) {
                Pose[i].Start = Animator.HumanPose.muscles[Pose[i].MuscleIndex];
            }
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

    public class MusclePoseTweenPlayable : PlayableAsset {

        public AnimationCurve Curve;
        public MusclePose TargetPose;
        public MusclePose StartPose;
        public bool UseCurrent = true;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<MusclePoseTweenBehaviour>.Create(graph);
            playable.SetDuration(0.25);
            var behavior = playable.GetBehaviour();
            behavior.Curve = Curve;
            behavior.UpdateStart = UseCurrent;
            for (int i = 0; i < TargetPose.Count; i++) {
                behavior.Pose.Add(new SavedMuscleInstance(TargetPose.Pose[i]));
                if (StartPose == null) {
                    continue;
                }
                var muscle = StartPose.GetMuscle(TargetPose.Pose[i].MuscleIndex);
                if (muscle != null) {
                    behavior.Pose[i].Start = muscle.Value;
                }
            }
            return playable;
        }
    }
}
