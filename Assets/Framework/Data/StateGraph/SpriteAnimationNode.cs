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

        public override string Title { get { return Animation != null ? Animation.Path : "SpriteAnimation"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeSpriteAnimationNode(this, graph);
        }
    
        public class RuntimeSpriteAnimationNode : RuntimeStateNode {

            private SpriteAnimationNode _animNode;
            private ISpriteRendererComponent _renderer;
            private SpriteAnimatorComponent _animator;
            private SpriteAnimation _animation;
            private IDirectionalAnimation _directionalAnimation;
            private SpriteBillboardComponent _billboard;
            private DirectionsEight _lastOrientation;
            private bool _setup = false;
            
            public override string DebugInfo { 
                get {
                    if (_animation == null) {
                        return "No Animation Setup: " + _setup;
                    }
                    return string.Format("{2} Frame {0:F3} Frame Time {1}", _animator.FrameIndex, _animator.FrameTimer, _animation.name)
            ; } }

            public RuntimeSpriteAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base(node, graph) {
                _animNode = node;
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                if (!_setup) {
                    _animNode.Animation.LoadAsset(FinishSetup);
                }
                else {
                    SetupAnimation();
                }
            }

            private void FinishSetup(SpriteAnimation op) {
                _setup = true;
                if (_animNode.InstancedIndex >= 0) {
                    _renderer = Graph.Entity.Get<SpriteSimpleRendererComponent>();
                }
                else {
                    _renderer = Graph.Entity.Get<SpriteRendererComponent>();
                }
                _animator = Graph.Entity.Get<SpriteAnimatorComponent>();
                if (op == null) {
                    Debug.LogError(
                        op + " " + Graph.OriginalGraph.name + " couldn't load animation " + _animNode
                            .Animation.Path);
                    return;
                }
                SetAnimation(op);
            }

            protected virtual void SetAnimation(SpriteAnimation animation) {
                _animation = animation;
                _directionalAnimation = animation as DirectionalAnimation;
                if (_directionalAnimation != null) {
                    _billboard = Graph.Entity.Get<SpriteBillboardComponent>();
                }
                SetupAnimation();
            }

            private void SetupAnimation() {
                _animator.CurrentAnimation = _animation;
                _animator.FrameIndex = 0;
                _renderer?.Flip(_animNode.ForceReverse);
                UpdateFrame(_animation.GetFrame(0));
                UpdateSprite();
            }

            protected virtual void UpdateSprite() {
                if (_directionalAnimation == null) {
                    _renderer.SetSprite(
                        _animation.GetSprite(_animator.FrameIndex), _animation.NormalMap, _animation.EmissiveMap,
                        _animation.GetSpriteCollider(_animator.FrameIndex), _animNode.InstancedIndex, _animNode.ForceReverse);
                    return;
                }
                _lastOrientation = _billboard.Orientation;
                var facing = _billboard.Orientation;
                if (_billboard.Facing.RequiresFlipping()) {
                    facing = _billboard.Orientation.GetFlippedSide();
                    _renderer.Flip(_billboard.Orientation.IsFlipped());
                }
                var animFacing = _directionalAnimation.GetFacing(facing);
                if (animFacing == null || _animator.FrameIndex >= animFacing.FrameIndices.Length) {
                    return;
                }
                var idx = animFacing.FrameIndices[_animator.FrameIndex];
                _renderer.SetSprite(_directionalAnimation.GetSprite(idx), _directionalAnimation.NormalMap, _directionalAnimation.EmissiveMap, _directionalAnimation.GetSpriteCollider(idx), _animNode.InstancedIndex, _animNode.ForceReverse);
            }
            

            private void UpdateFrame(AnimationFrame frame) {
                _animator.FrameTimer = _animation.FrameTime * frame.Length;
                _animator.CurrentFrame = frame;
                if (!frame.HasEvent) {
                    return;
                }
                // var pos = _renderer.GetEventPosition(frame.EventPosition, _animNode.InstancedIndex);
                // var rot = _renderer.GetRotation();
                if (frame.HasEvent) {
                    Graph.Entity.Post(
                        new AnimationEventTriggered(
                            Graph.Entity,
                            frame.Event == AnimationFrame.EventType.Default ?
                                AnimationEvents.Default :
                                frame.EventName));
                }
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (!_setup) {
                    return false;
                }
                _animator.FrameTimer -= dt;
                if (_animator.FrameTimer > 0) {
                    if (_directionalAnimation != null) {
                        CheckBillboard();
                    }
                    return false;
                }
                _animator.FrameIndex++;
                var frame = _animation.GetFrame(_animator.FrameIndex);
                if (frame != null) {
                    UpdateFrame(frame);
                    UpdateSprite();
                    return false;
                }
                if (_animation.Looping && _animNode.AllowLooping) {
                    _animator.FrameIndex = 0;
                    UpdateFrame(_animation.GetFrame(_animator.FrameIndex));
                    UpdateSprite();
                    return false;
                }
                if (_directionalAnimation != null) {
                    CheckBillboard();
                }
                return true;
            }

            private void CheckBillboard() {
                if (_billboard.Orientation != _lastOrientation) {
                    UpdateSprite();
                }
            }

            public override void Dispose() {
                base.Dispose();
                _setup = false;
                _animation = null;
                _directionalAnimation = null;
                _animNode.Animation.ReleaseAsset();
            }
        }
    }
}
