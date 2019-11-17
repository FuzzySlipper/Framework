using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PixelComrades {
    public class SpriteAnimationNode : StateGraphNode {
        public SpriteAnimationReference Animation;
        public bool AllowLooping = true;
        public bool ForceReverse = false;
        public int InstancedIndex = -1;
        
        protected override Vector2 GetNodeSize { get { return new Vector2(DefaultNodeSize.x, DefaultNodeSize.y * 1.25f); } }
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Animation)), GUIContent.none, true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Loop");
            AllowLooping = UnityEditor.EditorGUILayout.Toggle(AllowLooping);
            GUILayout.Label("Reverse");
            ForceReverse = UnityEditor.EditorGUILayout.Toggle(ForceReverse);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Instanced");
            InstancedIndex = UnityEditor.EditorGUILayout.IntSlider(InstancedIndex, -1, 1);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            so.ApplyModifiedProperties();
            
#endif
            return false;
        }

        public override string Title { get { return Animation != null ? Animation.SubObjectName : "SpriteAnimation"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            if (InstancedIndex >= 0) {
                return new InstancedRuntimeAnimationNode(this, graph);
            }
            if (Animation.IsDirectional) {
                return new DirectionalRuntimeAnimationNode(this, graph);
            }
            return new RuntimeSpriteAnimationNode(this, graph);
        }
        public class InstancedRuntimeAnimationNode : RuntimeSpriteAnimationNode {

            private SpriteSimpleRendererComponent _simpleRenderer;
            
            public InstancedRuntimeAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base
            (node, graph) {
                _simpleRenderer = graph.Entity.Get<SpriteSimpleRendererComponent>();
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
            }

            public override void OnExit() {
                base.OnExit();
            }

            protected override void UpdateSprite() {
                var data = _simpleRenderer.Sprites[AnimNode.InstancedIndex];
                data.Sprite = Animator.CurrentAnimation.GetSprite(Animator.FrameIndex);
                data.Emissive = Animator.CurrentAnimation.EmissiveMap;
                data.Normal = Animator.CurrentAnimation.NormalMap;
                data.Flip = AnimNode.ForceReverse;
            }
        }

        public class DirectionalRuntimeAnimationNode : RuntimeSpriteAnimationNode {
            
            private IDirectionalAnimation _animation;
            private SpriteBillboardComponent _billboard;
            private DirectionsEight _lastOrientation;
            
            public DirectionalRuntimeAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base
            (node, graph) {
                _billboard = graph.Entity.Get<SpriteBillboardComponent>();
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
            }

            protected override void FinishSetup(AsyncOperationHandle<SpriteAnimation> animation) {
                base.FinishSetup(animation);
                _animation = Animator.CurrentAnimation as IDirectionalAnimation;
                if (_animation == null) {
                    Debug.Log(Animator.CurrentAnimation != null ? Animator.CurrentAnimation.name : "Null animation");
                }
            }

            public override void OnExit() {
                base.OnExit();
            }

            public override void Dispose() {
                base.Dispose();
                _animation = null;
            }

            protected override void UpdateSprite() {
                if (_animation == null) {
                    _animation = Animator.CurrentAnimation as IDirectionalAnimation;
                    if (_animation == null) {
                        return;
                    }
                }
                _lastOrientation = _billboard.Orientation;
                var facing = _billboard.Orientation;
                if (_billboard.Facing.RequiresFlipping()) {
                    facing = _billboard.Orientation.GetFlippedSide();
                    Renderer.Flip(_billboard.Orientation.IsFlipped());
                }
                var animFacing = _animation.GetFacing(facing);
                if (animFacing == null || Animator.FrameIndex >= animFacing.FrameIndices.Length) {
                    return;
                }
                var idx = animFacing.FrameIndices[Animator.FrameIndex];
                Renderer.SetSprite(_animation.GetSprite(idx), _animation.NormalMap, _animation.EmissiveMap, _animation.GetSpriteCollider(idx));
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

            protected SpriteAnimationNode AnimNode;
            protected SpriteRendererComponent Renderer;
            protected SpriteAnimatorComponent Animator;

            private bool _setup = false;
            
            public override string DebugInfo { 
                get {
                    if (Animator.CurrentAnimation == null) {
                        return "No Animation Setup: " + _setup;
                    }
                    return string.Format("{2} Frame {0:F3} Frame Time {1}", Animator.FrameIndex, Animator.FrameTimer, Animator.CurrentAnimation.name)
            ; } }

            public RuntimeSpriteAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base(node, graph) {
                AnimNode = node;
                Renderer = graph.Entity.Get<SpriteRendererComponent>();
                Animator = graph.Entity.Get<SpriteAnimatorComponent>();
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                //Renderer.SetTextures(_node.Animation.NormalMap, _node.Animation.EmissiveMap);
                var op = AnimNode.Animation.LoadAssetAsync<SpriteAnimation>();
                op.Completed += FinishSetup;
                
            }

            protected virtual void FinishSetup(AsyncOperationHandle<SpriteAnimation> animation) {
                _setup = true;
                var spriteAnimation = animation.Result;
                if (spriteAnimation == null) {
                    Debug.LogError(animation.DebugName + " " + Graph.OriginalGraph.name + " couldn't load animation " + AnimNode
                    .Animation.SubObjectName);
                    return;
                }
                Animator.CurrentAnimation = spriteAnimation;
                Animator.FrameIndex = 0;
                Renderer?.Flip(AnimNode.ForceReverse);
                UpdateFrame(Animator.CurrentAnimation.GetFrame(0));
                UpdateSprite();
            }

            public override void OnExit() {
                base.OnExit();
            }

            protected virtual void UpdateSprite() {
                Renderer.SetSprite(Animator.CurrentAnimation.GetSprite(Animator.FrameIndex), Animator.CurrentAnimation.NormalMap, Animator.CurrentAnimation.EmissiveMap,
                    Animator.CurrentAnimation.GetSpriteCollider(Animator.FrameIndex));
            }

            protected void UpdateFrame(AnimationFrame frame) {
                Animator.FrameTimer = Animator.CurrentAnimation.FrameTime * frame.Length;
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
                if (!_setup) {
                    return false;
                }
                Animator.FrameTimer -= dt;
                if (Animator.FrameTimer > 0) {
                    return false;
                }
                Animator.FrameIndex++;
                var frame = Animator.CurrentAnimation.GetFrame(Animator.FrameIndex);
                if (frame != null) {
                    UpdateFrame(frame);
                    UpdateSprite();
                    return false;
                }
                if (Animator.CurrentAnimation.Looping && AnimNode.AllowLooping) {
                    Animator.FrameIndex = 0;
                    UpdateFrame(Animator.CurrentAnimation.GetFrame(Animator.FrameIndex));
                    UpdateSprite();
                    return false;
                }
                return true;
            }

            public override void Dispose() {
                base.Dispose();
                _setup = false;
                Animator.CurrentAnimation = null;
                AnimNode.Animation.ReleaseAsset();
            }
        }
    }
}
