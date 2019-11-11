using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace PixelComrades {
    public sealed class SetSpriteNode : StateGraphNode {
        public SpriteAnimationAssetReference Sprite;
        public int Frame = 0;

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Sprite)), GUIContent.none, true);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Frame)), GUIContent.none, true);
            so.ApplyModifiedProperties();
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
            public RuntimeNode(SetSpriteNode node, RuntimeStateGraph graph) : base(node, graph) {
                _spriteNode = node;
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _spriteNode.Sprite.LoadAssetAsync();
                var spriteRenderer = Graph.Entity.Get<SpriteRendererComponent>();
                if (spriteRenderer != null) {
                    var spriteAnimation = _spriteNode.Sprite.Asset as SpriteAnimation;
                    if (spriteAnimation == null) {
                        return;
                    }
                    var frame = _spriteNode.Frame;
                    spriteRenderer.SetSprite(spriteAnimation.GetSprite(frame), spriteAnimation.NormalMap, spriteAnimation.EmissiveMap, 
                    spriteAnimation.GetSpriteCollider(frame));
                }
            }

            public override void OnExit() {
                base.OnExit();
                _spriteNode.Sprite.ReleaseAsset();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                return true;
            }
        }
    }
}
