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
                    Renderer.Value.flipX = _billboard.Orientation.IsFlipped();
                }
                Renderer.Value.sprite = _animation.GetSpriteFrame(facing, FrameIndex);
                if (Collider != null) {
                    Collider.Value.UpdateCollider();
                }
            }

            public override bool TryComplete(float dt) {
                var idx = FrameIndex;
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (idx == FrameIndex) {
                    if (_billboard.Orientation != _lastOrientation) {
                        UpdateSprite();
                    }
                }
                return false;
            }
        }

        public class RuntimeSpriteAnimationNode : RuntimeStateNode {
            
            private SpriteAnimationNode _node;
            private RuntimeStateNode _exitNode;
            private float _frameTimer;
            protected int FrameIndex = 0;
            protected SpriteRendererComponent Renderer;
            protected SpriteColliderComponent Collider;

            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeSpriteAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base(node, graph) {
                _exitNode = GetOriginalNodeExit();
                _node = node;
                Renderer = graph.Entity.Get<SpriteRendererComponent>();
                Collider = graph.Entity.Get<SpriteColliderComponent>();
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                var block = Renderer.MaterialBlocks[0];
                block.SetTexture("_BumpMap", _node.Animation.NormalMap);
                block.SetTexture("_EmissionMap", _node.Animation.EmissiveMap);
                if (_node.Animation.EmissiveMap != null) {
                    Renderer.Value.material.EnableKeyword("_EMISSION");
                }
                else {
                    Renderer.Value.material.DisableKeyword("_EMISSION");
                }
                Renderer.Value.SetPropertyBlock(block);
                FrameIndex = 0;
                UpdateSprite();
                UpdateFrame(_node.Animation.GetFrame(0));
            }

            protected virtual void UpdateSprite() {
                Renderer.Value.sprite = _node.Animation.GetSpriteFrame(FrameIndex);
                if (Collider != null) {
                    Collider.Value.UpdateCollider();
                }
            }

            protected void UpdateFrame(AnimationFrame frame) {
                _frameTimer = _node.Animation.FrameTime * frame.Length;
                if (frame.HasEvent) {
                    Graph.Entity.Post(new AnimationEventTriggered(Graph.Entity, frame.Event == AnimationFrame.EventType.Default?
                        AnimationEvents.Default : frame.EventName));
                }
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                _frameTimer -= dt;
                if (_frameTimer > 0) {
                    return false;
                }
                FrameIndex++;
                var frame = _node.Animation.GetFrame(FrameIndex);
                if (frame != null) {
                    UpdateSprite();
                    UpdateFrame(frame);
                    return false;
                }
                if (_node.Animation.Looping && _node.AllowLooping) {
                    FrameIndex = 0;
                    UpdateSprite();
                    UpdateFrame(_node.Animation.GetFrame(FrameIndex));
                    return false;
                }
                return true;
            }
        }
    }
}
