using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PixelComrades {
    public sealed class SetSpriteNode : StateGraphNode {
        public SpriteAnimationReference Sprite;
        public int Frame = 0;
        public int InstancedIndex = -1;

        protected override Vector2 GetNodeSize { get { return new Vector2(DefaultNodeSize.x, DefaultNodeSize.y * 1.25f); } }
        
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Sprite)), GUIContent.none, true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Frame)), GUIContent.none, true);
            so.ApplyModifiedProperties();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Instanced");
            InstancedIndex = UnityEditor.EditorGUILayout.IntSlider(InstancedIndex, -1, 1);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return Sprite != null ? Sprite.SubObjectName : "Sprite"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        private class RuntimeNode : RuntimeStateNode {
            private SetSpriteNode _spriteNode;
            private SpriteRendererComponent _spriteRenderer;
            private SpriteSimpleRendererComponent _simpleRenderer;
            private bool _setup = false;
            
            public RuntimeNode(SetSpriteNode node, RuntimeStateGraph graph) : base(node, graph) {
                _spriteNode = node;
                if (_spriteNode.InstancedIndex >= 0) {
                    _simpleRenderer = Graph.Entity.Get<SpriteSimpleRendererComponent>();
                }
                else {
                    _spriteRenderer = Graph.Entity.Get<SpriteRendererComponent>();
                }
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                var op = _spriteNode.Sprite.LoadAssetAsync<SpriteAnimation>();
                op.Completed += FinishSetup;
            }

            private void FinishSetup(AsyncOperationHandle<SpriteAnimation> animation) {
                _setup = true;
                var spriteAnimation = animation.Result;
                if (spriteAnimation == null) {
                    return;
                }
                var frame = _spriteNode.Frame;
                if (_spriteNode.InstancedIndex >= 0) {
                    var data = _simpleRenderer.Sprites[_spriteNode.InstancedIndex];
                    data.Sprite = spriteAnimation.GetSprite(frame);
                    data.Emissive = spriteAnimation.EmissiveMap;
                    data.Normal = spriteAnimation.NormalMap;
                    data.Flip = false;
                }
                else {
                    _spriteRenderer.SetSprite(
                        spriteAnimation.GetSprite(frame), spriteAnimation.NormalMap, spriteAnimation.EmissiveMap,
                        spriteAnimation.GetSpriteCollider(frame));
                }
            }

            public override void OnExit() {
                base.OnExit();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (!_setup) {
                    return false;
                }
                return true;
            }

            public override void Dispose() {
                base.Dispose();
                _setup = false;
                _spriteNode.Sprite.ReleaseAsset();
            }
        }
    }
}
