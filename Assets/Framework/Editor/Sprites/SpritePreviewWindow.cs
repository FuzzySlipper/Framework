using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    public class SpritePreviewWindow : EditorWindow {

        protected Sprite CurrentSprite;
        protected Texture CurrentTexture;
        private Material _mat;
        private Shader _defaultShader;
        protected float Multiple;
        protected Rect TextureRect;

        public static SpritePreviewWindow ShowWindow() {
            var window = (SpritePreviewWindow) GetWindow(typeof(SpritePreviewWindow), false, "Sprite Preview Window");
            return window;
        }

        void OnEnable() {
            CreateDefaultMat();
        }

        private void CreateDefaultMat() {
            _defaultShader = Shader.Find("Unlit/Transparent");
            _mat = new Material(_defaultShader);
        }

        public void ChangeShader(Shader shader) {
            if (shader == null) {
                _mat.shader = _defaultShader;
                return;
            }
            _mat.shader = shader;
        }

        public void SetSprite(Sprite sprite) {
            CurrentSprite = sprite;
            minSize = sprite.rect.size;
            maxSize = sprite.rect.size * 3;
            position = new Rect(position.x, position.y, sprite.rect.width, sprite.rect.height);
        }

        public void SetTexture(Texture texture) {
            CurrentSprite = null;
            CurrentTexture = texture;
        }
        
        protected virtual void OnGUI() {
            DisplayCurrent();
        }

        protected virtual void DisplayCurrent() {
            if (_mat == null) {
                CreateDefaultMat();
            }
            EditorGUILayout.ObjectField(_mat, typeof(Material), false);
            if (CurrentSprite == null) {
                if (CurrentTexture != null) {
                    EditorGUILayout.ObjectField(CurrentTexture, CurrentTexture.GetType(), false);
                    var size = FindSize(CurrentTexture.width, CurrentTexture.height, position.width, position.height);
                    TextureRect = GUILayoutUtility.GetRect(size.x, size.y);
                    DrawTexture(TextureRect, CurrentTexture);
                    EditorGUILayout.LabelField(string.Format("{0},{1}", TextureRect.x, TextureRect.y));
                }
                return;
            }
            EditorGUILayout.ObjectField(CurrentSprite, typeof(Sprite), false);
            var spriteSize = FindSize(CurrentSprite.rect.width, CurrentSprite.rect.height, position.width, position.height);
            TextureRect = GUILayoutUtility.GetRect(spriteSize.x, spriteSize.y);
            DrawTextureGUI(TextureRect, CurrentSprite, spriteSize);
        }

        protected Vector2 FindSize(float sourceX, float sourceY, float windowX, float windowY) {
            float multipleX = windowX / sourceX;
            float multipleY = windowY / sourceY;
            Multiple = Mathf.Min(multipleX, multipleY);
            return new Vector2(sourceX, sourceY) * Multiple;
        }

        public void DrawTexture(Rect pos, Sprite sprite, Vector2 size) {
            Rect spriteRect = new Rect(
                sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            Graphics.DrawTexture(
                new Rect(pos.x, pos.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect, 0,
                0, 0, 0);
        }

        public void DrawTexture(Rect pos, Texture texture) {
//            Vector2 actualSize = size;
//            Graphics.DrawTexture(
//                new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), texture, position, 0,
//                0, 0, 0);
//            EditorGUI.DrawPreviewTexture(rect, _previewTexture, _spriteCamera.PreviewMat);
            //Graphics.DrawTexture(pos, texture, _mat);
            GUI.DrawTexture(pos, texture, ScaleMode.ScaleToFit);
        }
        

        public static void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size) {
            Rect spriteRect = new Rect(
                sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(
                new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }
    }
}