using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    public class SpriteColliderCriticalWindow : SpritePreviewWindow {
        
        [MenuItem("Window/Sprite Collider Critical Window")]
        public static SpriteColliderCriticalWindow ShowCriticalWindow() {
            var window = (SpriteColliderCriticalWindow) GetWindow(typeof(SpriteColliderCriticalWindow), false, "Sprite Collider Critical Window");
            return window;
        }

        private SpriteAnimation _animation;

        public void Set(SpriteAnimation anim) {
            _animation = anim;
            var texture = anim.GetSprite(0).texture;
            SetTexture(texture);
            var size = FindSize(texture.width, texture.height, position.width, position.height);
            position = new Rect(position.x, position.y, size.x, size.y);
        }

        protected override void OnGUI() {
            DisplayCurrent();
            var mousePos = Event.current.mousePosition;
            Rect currentRect = new Rect();
            Sprite currentSprite = null;
            SavedSpriteCollider spriteCollider = null;
            var cnt = _animation.LengthSprites;
            Handles.color = Color.red;
            var basePosition = TextureRect.position;
            int spriteCnt = 0;
            for (int i = 0; i < cnt; i++) {
                var sprite = _animation.GetSprite(i);
                if (sprite == null) {
                    continue;
                }
                var collider = _animation.GetSpriteCollider(i);
                spriteCnt++;
                var xPos = sprite.rect.position.x * Multiple;
                var yPos = (CurrentTexture.height - sprite.rect.position.y) * Multiple;
                var size = sprite.rect.size * Multiple;
                yPos -= size.y;
                var rect = new Rect(basePosition +  new Vector2(xPos, yPos), size);
                //rect.position += (rect.size * 0.5f);
                //EditorGUI.DrawRect(rect, Color.red);
                Handles.color = Color.blue;
                Handles.DrawWireCube(rect.center, rect.size);
                if (collider == null) {
                    GUI.Label(rect, "No Collider");
                }
                else {
                    //Handles.DrawWireCube(rect.center + (collider.CriticalRect.center * Multiple), collider.CriticalRect.size * Multiple);
                    var critPos = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, collider.CriticalRect.x),
                        Mathf.Lerp(rect.yMax, rect.yMin, collider.CriticalRect.y));
                    var critSize = new Vector2(Mathf.Lerp(0, size.x, collider.CriticalRect.size.x),
                        Mathf.Lerp(0, size.y, collider.CriticalRect.size.y));
                    Handles.color = Color.red;
                    Handles.DrawWireCube(critPos, critSize);
                }
                if (rect.Contains(mousePos)) {
                    currentRect = rect;
                    currentSprite = sprite;
                    spriteCollider = collider;
                }
            }
            Vector3 spriteNormalizedPosition = Vector3.zero;
            if (currentSprite != null) {
                spriteNormalizedPosition.x = Mathf.InverseLerp(currentRect.xMin, currentRect.xMax, mousePos.x);
                spriteNormalizedPosition.y = Mathf.InverseLerp(currentRect.yMax, currentRect.yMin, mousePos.y);
                EditorGUILayout.LabelField(currentSprite.name + " " + spriteNormalizedPosition);
            }
            else {
                EditorGUILayout.LabelField("No sprite " + spriteCnt + "/" + cnt + " " + mousePos + " / " + (mousePos -TextureRect.position));
            }
            if (currentSprite == null || spriteCollider == null) {
                return;
            }
            bool dirty = false;
            if (Event.current.type == EventType.MouseDown) {
                spriteCollider.CriticalRect = new Rect(spriteNormalizedPosition, spriteCollider.CriticalRect.size);
                dirty = true;
            }
            if (Event.current.type == EventType.ScrollWheel) {
                var sizeChange = -Event.current.delta.y * 0.01f;
                var size = spriteCollider.CriticalRect.size;
                if (Event.current.control) {
                    size.x += sizeChange;
                }
                else if (Event.current.alt) {
                    size.y += sizeChange;
                }
                else {
                    size += Vector2.one * sizeChange;
                }
                size = new Vector2(Mathf.Clamp01(size.x), Mathf.Clamp01(size.y));
                spriteCollider.CriticalRect = new Rect(spriteCollider.CriticalRect.position, size);
                dirty = true;
            }
            if (dirty) {
                EditorUtility.SetDirty(_animation);
                Repaint();
            }
            
        }
    }
}