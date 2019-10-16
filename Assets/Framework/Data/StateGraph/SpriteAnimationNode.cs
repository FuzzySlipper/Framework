using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteAnimationNode : StateGraphNode {
        public SpriteAnimation Animation;
        public bool AllowLooping = true;

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Animation = UnityEditor.EditorGUILayout.ObjectField(Animation, typeof(SpriteAnimation), false) as
                SpriteAnimation;
            GUILayout.Label("Allow Loop");
            AllowLooping = UnityEditor.EditorGUILayout.Toggle(AllowLooping);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return Animation != null ? Animation.name : "SpriteAnimation"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            if (Animation is DirectionalAnimation dirAnim) {
                return new DirectionalRuntimeAnimationNode(dirAnim, this, graph);
            }
            return new RuntimeSpriteAnimationNode(this, graph);
        }

        public class DirectionalRuntimeAnimationNode : RuntimeSpriteAnimationNode {
            
            private DirectionalAnimation _animation;
            private SpriteBillboardComponent _billboard;
            private DirectionsEight _lastOrientation;
            
            public DirectionalRuntimeAnimationNode(DirectionalAnimation anim, SpriteAnimationNode node, RuntimeStateGraph graph) : base
            (node, graph) {
                _animation = anim;
                _billboard = graph.Entity.Get<SpriteBillboardComponent>();
            }

            protected override void UpdateSprite() {
                _lastOrientation = _billboard.Orientation;
                var facing = _billboard.Orientation;
                if (_billboard.Facing.RequiresFlipping()) {
                    facing = _billboard.Orientation.GetFlippedSide();
                    Renderer.Flip(_billboard.Orientation.IsFlipped());
                }
                Renderer.SetSprite(_animation.GetSpriteFrame(facing, Animator.FrameIndex));
                if (Collider != null) {
                    Collider.Value.UpdateCollider();
                }
            }

            public override bool TryComplete(float dt) {
                var idx = Animator.FrameIndex;
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (idx == Animator.FrameIndex) {
                    if (_billboard.Orientation != _lastOrientation) {
                        UpdateSprite();
                    }
                }
                return false;
            }
        }

        public class RuntimeSpriteAnimationNode : RuntimeStateNode {
            
            private SpriteAnimationNode _node;
            protected SpriteRendererComponent Renderer;
            protected SpriteColliderComponent Collider;
            protected SpriteAnimatorComponent Animator;
            
            public override string DebugInfo { get { return string.Format("Frame {0:F3} Frame Time {1}", Animator.FrameIndex, Animator.FrameTimer)
            ; } }

            public RuntimeSpriteAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base(node, graph) {
                _node = node;
                Renderer = graph.Entity.Get<SpriteRendererComponent>();
                Collider = graph.Entity.Get<SpriteColliderComponent>();
                Animator = graph.Entity.Get<SpriteAnimatorComponent>();
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                Renderer.SetTextures(_node.Animation.NormalMap, _node.Animation.EmissiveMap);
                Animator.CurrentAnimation = _node.Animation;
                Animator.FrameIndex = 0;
                UpdateFrame(_node.Animation.GetFrame(0));
                UpdateSprite();
            }

            protected virtual void UpdateSprite() {
                Renderer.SetSprite(_node.Animation.GetSpriteFrame(Animator.FrameIndex));
                if (Collider != null) {
                    Collider.Value.UpdateCollider();
                }
            }

            protected void UpdateFrame(AnimationFrame frame) {
                Animator.FrameTimer = _node.Animation.FrameTime * frame.Length;
                Animator.CurrentFrame = frame;
                if (frame.HasEvent) {
                    Graph.Entity.Post(new AnimationEventTriggered(Graph.Entity, frame.Event == AnimationFrame.EventType.Default?
                        AnimationEvents.Default : frame.EventName));
                }
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                Animator.FrameTimer -= dt;
                if (Animator.FrameTimer > 0) {
                    return false;
                }
                Animator.FrameIndex++;
                var frame = _node.Animation.GetFrame(Animator.FrameIndex);
                if (frame != null) {
                    UpdateFrame(frame);
                    UpdateSprite();
                    return false;
                }
                if (_node.Animation.Looping && _node.AllowLooping) {
                    Animator.FrameIndex = 0;
                    UpdateFrame(_node.Animation.GetFrame(Animator.FrameIndex));
                    UpdateSprite();
                    return false;
                }
                return true;
            }
        }
    }
}
