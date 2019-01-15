using System.CodeDom;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace PixelComrades {
    public class SpriteAnimationPreview {

        private SpriteAnimation _animation;
        private int _frame = 0;
        private float _nextFrameTime = 0;
        private List<Editor> _spriteEditors = new List<Editor>();
        private float _currentTime;
        private double _lastFrameEditorTime;
        private Vector2 _latestSize;
        private Dictionary<Editor, Texture> _latestPreviewTextures;

        public SpriteAnimation Animation { get { return _animation; } }
        public bool IsPlaying { get; private set; }
        public float Speed { get; set; }

        public SpriteAnimationPreview(SpriteAnimation animation) {
            _animation = animation;
            var type = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.SpriteInspector");
            for (int i = 0; i < animation.Frames.Length; i++) {
                var sprite = animation.GetSpriteFrame(i);
                if (sprite == null) {
                    continue;
                }
                Editor editor = null;
                Editor.CreateCachedEditor(sprite, type, ref editor);
                if (editor != null) {
                    _spriteEditors.Add(editor);
                }
            }
            Speed = 1;
            EditorApplication.update += Update;
        }

        public void OnDisable() {
            if (_latestPreviewTextures != null)
                foreach (var key in _latestPreviewTextures.Keys) {
                    Object.DestroyImmediate(_latestPreviewTextures[key]);
                }
            if (_spriteEditors != null)
                foreach (var spriteEditor in _spriteEditors) {
                    Object.DestroyImmediate(spriteEditor);
                }
        }

        private Sprite GetCurrentSprite() {
            if (_nextFrameTime < _currentTime) {
                _frame++;
                if (_frame >= _animation.LengthFrames) {
                    _frame = 0;
                }
                _nextFrameTime = _currentTime + (_animation.FrameTime * _animation.GetFrame(_frame).Length);
            }
            return _animation.GetSpriteFrame(_frame);
        }

        public Texture GetCurrentPreviewTexture(Rect previewRect) {
            var currentSprite = GetCurrentSprite();
            return GetPreviewTexture(previewRect, currentSprite) ?? AssetPreview.GetAssetPreview(GetCurrentSprite());
        }

        public void NextSprite() {
            _frame++;
        }

        public void PrevSprite() {
            _frame--;
        }

        public Texture GetPreviewTexture(Rect previewRect, Sprite sprite) {
            if (IsDirty(previewRect))
                RebuildPreviewTextures(previewRect);
            var spriteEditor = GetSpriteEditor(sprite);
            return _latestPreviewTextures[spriteEditor];
        }

        private void RebuildPreviewTextures(Rect previewRect) {
            _latestPreviewTextures = new Dictionary<Editor, Texture>(_spriteEditors.Capacity);
            _latestSize = new Vector2(previewRect.width, previewRect.height);
            for (int i = 0; i < _spriteEditors.Count; i++) {
                var editor = _spriteEditors[i];
                var previewTexture = editor.RenderStaticPreview("", null, (int)previewRect.width,
                    (int)previewRect.height);
                previewTexture.name = string.Format("({1,2}/{2,2}) {0}", editor.target.name, i + 1,
                    _spriteEditors.Count);
                _latestPreviewTextures.Add(editor, previewTexture);
            }
        }

        private bool IsDirty(Rect previewRect) {
            return !(_latestSize.x == previewRect.width && _latestSize.y == previewRect.height);
        }

        private Editor GetSpriteEditor(Sprite sprite) {
            return _spriteEditors.FirstOrDefault(e => e.target == sprite);
        }


        public void Update() {
            if (IsPlaying) {
                var deltaTime = (float)EditorApplication.timeSinceStartup - (float)_lastFrameEditorTime;
                _currentTime += deltaTime * Speed;
            }
            else {
                _currentTime = 0;
            }
            _lastFrameEditorTime = EditorApplication.timeSinceStartup;

        }

        public float GetCurrentTime(float startTime, float stopTime) {
            var _currentTime = Mathf.Repeat(this._currentTime, stopTime);
            _currentTime = Mathf.Clamp(_currentTime, startTime, stopTime);
            return _currentTime;
        }

        public void Play() {
            IsPlaying = true;
            _lastFrameEditorTime = EditorApplication.timeSinceStartup;
        }

        public void Pause() {
            IsPlaying = false;
        }
    }
}
