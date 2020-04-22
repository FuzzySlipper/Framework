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

        public override string Title { get { return Animation != null ? Animation.AssetReference.SubObjectName : "SpriteAnimation"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            if (Animation.Asset is DirectionalAnimation) {
                return new DirectionalRuntimeAnimationNode(this, graph);
            }
            return new RuntimeSpriteAnimationNode(this, graph);
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
            
            protected override void SetAnimation(SpriteAnimation animation) {
                _animation = animation as IDirectionalAnimation;
                if (_animation == null) {
                    Debug.Log(Animation != null ? Animation.name : "Null animation");
                }
                base.SetAnimation(animation);
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
                    _animation = Animation as IDirectionalAnimation;
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
                Renderer.SetSprite(_animation.GetSprite(idx), _animation.NormalMap, _animation.EmissiveMap, _animation.GetSpriteCollider(idx), AnimNode.InstancedIndex, AnimNode.ForceReverse);
            }

            public override bool TryComplete(float dt) {
                if (!Setup) {
                    return false;
                }
                if (base.TryComplete(dt)) {
                    return true;
                }
                var idx = Animator.FrameIndex;
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
            protected ISpriteRendererComponent Renderer;
            protected SpriteAnimatorComponent Animator;
            protected SpriteAnimation Animation;

            protected bool Setup = false;
            
            public override string DebugInfo { 
                get {
                    if (Animation == null) {
                        return "No Animation Setup: " + Setup;
                    }
                    return string.Format("{2} Frame {0:F3} Frame Time {1}", Animator.FrameIndex, Animator.FrameTimer, Animation.name)
            ; } }

            public RuntimeSpriteAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base(node, graph) {
                AnimNode = node;
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                if (!Setup) {
                    var op = AnimNode.Animation.AssetReference.LoadAssetAsync<SpriteAnimation>();
                    op.Completed += FinishSetup;
                }
                else {
                    SetupAnimation();
                }
            }

            private void FinishSetup(AsyncOperationHandle<SpriteAnimation> op) {
                Setup = true;
                if (AnimNode.InstancedIndex >= 0) {
                    Renderer = Graph.Entity.Get<SpriteSimpleRendererComponent>();
                }
                else {
                    Renderer = Graph.Entity.Get<SpriteRendererComponent>();
                }
                Animator = Graph.Entity.Get<SpriteAnimatorComponent>();
                if (op.Result == null) {
                    Debug.LogError(
                        op.DebugName + " " + Graph.OriginalGraph.name + " couldn't load animation " + AnimNode
                            .Animation.AssetReference.SubObjectName);
                    return;
                }
                SetAnimation(op.Result);
            }

            protected virtual void SetAnimation(SpriteAnimation animation) {
                Animation = animation;
                SetupAnimation();
            }

            private void SetupAnimation() {
                Animator.CurrentAnimation = Animation;
                Animator.FrameIndex = 0;
                Renderer?.Flip(AnimNode.ForceReverse);
                UpdateFrame(Animation.GetFrame(0));
                UpdateSprite();
            }

            public override void OnExit() {
                base.OnExit();
            }

            protected virtual void UpdateSprite() {
                Renderer.SetSprite(Animation.GetSprite(Animator.FrameIndex), Animation.NormalMap, Animation.EmissiveMap,
                    Animation.GetSpriteCollider(Animator.FrameIndex), AnimNode.InstancedIndex, AnimNode.ForceReverse);
            }

            private void UpdateFrame(AnimationFrame frame) {
                Animator.FrameTimer = Animation.FrameTime * frame.Length;
                Animator.CurrentFrame = frame;
                if (!frame.HasEvent) {
                    return;
                }
                var pos = Renderer.GetEventPosition(frame.EventPosition, AnimNode.InstancedIndex);
                var rot = Renderer.GetRotation();
                for (int i = 0; i < frame.Events.Length; i++) {
                    Graph.Entity.Post(new AnimationEventTriggered(Graph.Entity, new AnimationEvent(frame.Events[i], pos, rot )));
                }
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (!Setup) {
                    return false;
                }
                Animator.FrameTimer -= dt;
                if (Animator.FrameTimer > 0) {
                    return false;
                }
                Animator.FrameIndex++;
                var frame = Animation.GetFrame(Animator.FrameIndex);
                if (frame != null) {
                    UpdateFrame(frame);
                    UpdateSprite();
                    return false;
                }
                if (Animation.Looping && AnimNode.AllowLooping) {
                    Animator.FrameIndex = 0;
                    UpdateFrame(Animation.GetFrame(Animator.FrameIndex));
                    UpdateSprite();
                    return false;
                }
                return true;
            }

            public override void Dispose() {
                base.Dispose();
                Setup = false;
                Animation = null;
                AnimNode.Animation.AssetReference.ReleaseAsset();
            }
        }
    }
}
