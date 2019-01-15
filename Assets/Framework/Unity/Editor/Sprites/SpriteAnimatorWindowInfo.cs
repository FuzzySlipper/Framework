using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PixelComrades {
    public partial class SpriteAnimatorWindow {

        private ReorderableList _framesReorderableList;

        private Vector2 _scrollPosition = Vector2.zero;

        private void ChangeFrameRate(float newFramerate, bool preserveTiming) {
            Undo.RecordObject(_clip, "Change Animation Framerate");

            // Scale each frame (if preserving timing) and clamp to closest sample time
            var minFrameTime = 1.0f/newFramerate;
            var scale = preserveTiming ? 1.0f : _clip.FramesPerSecond/newFramerate;
            foreach (var frame in _frames) {
                frame.RealLength = MathEx.Max(Utils.Snap(frame.RealLength*scale, minFrameTime), minFrameTime);
            }

            _clip.FramesPerSecond = newFramerate;
            RecalcFrameTimes();
            ApplyChanges();
        }

        private void ChangeLooping(bool looping) {
            Undo.RecordObject(_clip, "Change Animation Looping");
            //var settings = AnimationUtility.GetAnimationClipSettings(_clip);
            //settings.loopTime = looping;
            //AnimationUtility.SetAnimationClipSettings(_clip, settings);
            _clip.Looping = looping;
            // NB: When hitting play directly after this change, the looping state will be undone. So have to call ApplyChanges() afterwards even though frame data hasn't changed.
            ApplyChanges();
        }

        private void InitialiseFramesReorderableList() {
            _framesReorderableList = new ReorderableList(_frames, typeof(AnimFrame), true, true, true, true);
            _framesReorderableList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Frames");
                EditorGUI.LabelField(new Rect(rect) {
                    x = rect.width - 37,
                    width = 45
                }, "Length");
            };
            _framesReorderableList.drawElementCallback = LayoutFrameListFrame;
            _framesReorderableList.onSelectCallback = list => { SelectFrame(_frames[_framesReorderableList.index]); };
        }

        private void LayoutFrameListFrame(Rect rect, int index, bool isActive, bool isFocused) {
            if (_frames == null || index < 0 || index >= _frames.Count)
                return;
            var frame = _frames[index];

            EditorGUI.BeginChangeCheck();
            rect = new Rect(rect) {
                height = rect.height - 4,
                y = rect.y + 2
            };

            // frame ID
            var xOffset = rect.x;
            var width = Styles.INFOPANEL_LABEL_RIGHTALIGN.CalcSize(new GUIContent(index.ToString())).x;
            EditorGUI.LabelField(new Rect(rect) {
                x = xOffset,
                width = width
            }, index.ToString(), Styles.INFOPANEL_LABEL_RIGHTALIGN);

            // Frame Sprite
            xOffset += width + 5;
            width = rect.xMax - 5 - 28 - xOffset;

            // Sprite thingy
            var spriteFieldRect = new Rect(rect) {
                x = xOffset,
                width = width,
                height = 16
            };
            var oldTex = frame.Sprite != null ? frame.Sprite.texture : null;
            var tex = EditorGUI.ObjectField(spriteFieldRect, oldTex, typeof(Texture2D), false) as Texture2D;
            if (tex != oldTex) {
                var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(tex));
                var subAsset = Array.Find(assets, item => item is Sprite);
                if (subAsset != null) {
                    frame.Sprite = (Sprite) subAsset;
                }
            }

            // Frame length (in samples)
            xOffset += width + 5;
            width = 28;
            GUI.SetNextControlName("FrameLen");
            //var frameLen = Mathf.RoundToInt(frame.RealLength/GetMinFrameTime());
            frame.LengthMulti = EditorGUI.FloatField(new Rect(rect) {
                x = xOffset,
                width = width
            }, frame.LengthMulti);
            SetFrameLength(frame, frame.LengthMulti*GetFrameTime());

            if (EditorGUI.EndChangeCheck()) {
                // Apply events
                ApplyChanges();
            }
        }

        private void LayoutInfoPanel(Rect rect) {
            GUILayout.BeginArea(rect, EditorStyles.inspectorFullWidthMargins);
            GUILayout.Space(20);

            // Animation length
            EditorGUILayout.LabelField(
                string.Format("Length: {0:0.00} sec  {1:D} samples", _clip.LengthTime,
                    Mathf.RoundToInt(_clip.LengthTime/GetFrameTime())), new GUIStyle(EditorStyles.miniLabel) {
                        normal = {
                            textColor = Color.gray
                        }
                    });

            // Speed/Framerate
            GUI.SetNextControlName("Framerate");
            var newFramerate = EditorGUILayout.DelayedFloatField("Sample Rate", _clip.FramesPerSecond);
            if (Mathf.Approximately(newFramerate, _clip.FramesPerSecond) == false) {
                ChangeFrameRate(newFramerate, true);
            }
            GUI.SetNextControlName("Length");
            var oldLength = Utils.Snap(_clip.LengthTime, 0.05f);
            var newLength = Utils.Snap(EditorGUILayout.FloatField("Length (sec)", oldLength), 0.05f);
            if (Mathf.Approximately(newLength, oldLength) == false && newLength > 0) {
                newFramerate = MathEx.Max(Utils.Snap(_clip.FramesPerSecond*(_clip.LengthTime/newLength), 1), 1);
                ChangeFrameRate(newFramerate, false);
            }

            // Looping tickbox
            var looping = EditorGUILayout.Toggle("Looping", _clip.Looping);
            if (looping != _clip.Looping) {
                ChangeLooping(looping);
            }

            // UI Image option- Done as an enum to be clearer
            var animSpriteType =
                (eAnimSpriteType)
                    EditorGUILayout.EnumPopup("Animated Sprite Type",
                        _uiImage ? eAnimSpriteType.UIImage : eAnimSpriteType.Sprite);
            SetIsUIImage(animSpriteType == eAnimSpriteType.UIImage);

            GUILayout.Space(10);

            // Frames list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
            EditorGUI.BeginChangeCheck();
            _framesReorderableList.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) {
                RecalcFrameTimes();
                Repaint();
                ApplyChanges();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void SetIsUIImage(bool uiImage) {
            if (_uiImage != uiImage) {
                _uiImage = uiImage;

                // Remove old curve binding
                //if (_curveBinding.propertyName == PROPERTYNAME_SPRITE) {
                //    AnimationUtility.SetObjectReferenceCurve(_clip, _curveBinding, null);
                //}

                //// Create new curve binding - 
                //CreateCurveBinding(); // no can't do this, because it'll create an extra duplicate curve binding
                ApplyChanges();
            }
        }
    }

}