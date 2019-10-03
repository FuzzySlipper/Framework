using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PoseAnimationNode : AnimationNode {
        
        public AnimationCurve Curve;
        public MusclePose TargetPose;
        
        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph owner) {
            return new PoseAnimationRuntimeNode(this, owner);
        }
    }

    public class PoseAnimationRuntimeNode : RuntimeAnimationNode {

        private AnimationCurve _curve;
        private bool _started;
        private List<SavedMuscleInstance> _pose = new List<SavedMuscleInstance>();
        private PoseAnimator _animator;
        private bool _foundPrevious;
        
        public PoseAnimationRuntimeNode(PoseAnimationNode node, RuntimeStateGraph owner) : base(node, owner) {
            PoseAnimationNode previous = node.EnterNode as PoseAnimationNode;
            _foundPrevious = previous != null;
            _curve = node.Curve;
            for (int i = 0; i < node.TargetPose.Count; i++) {
                _pose.Add(new SavedMuscleInstance(node.TargetPose.Pose[i]));
                if (previous == null) {
                    continue;
                }
                var muscle = previous.TargetPose.GetMuscle(node.TargetPose.Pose[i].MuscleIndex);
                if (muscle != null) {
                    _pose[i].Start = muscle.Value;
                }
            }
        }

        public override void OnEnter(RuntimeStateNode lastNode) {
            base.OnEnter(lastNode);
            Setup();
        }

        public override void OnExit() {
            base.OnExit();
            _started = false;
        }

        private void Setup() {
            _started = true;
            _animator = PoseAnimator.Main;
            _animator.UpdatePose();
            if (_foundPrevious) {
                return;
            }
            for (int i = 0; i < _pose.Count; i++) {
                _pose[i].Start = _animator.HumanPose.muscles[_pose[i].MuscleIndex];
            }
        }

        protected override void UpdateAnimation(float percent) {
            if (!_started) {
                Setup();
            }
            var animationPercent = _curve.Evaluate(percent);
            for (int i = 0; i < _pose.Count; i++) {
                var muscle = _pose[i];
                _animator.HumanPose.muscles[muscle.MuscleIndex] = Mathf.Lerp(muscle.Start, muscle.Target, animationPercent);
            }
            _animator.RefreshPose();
        }
    }
}
