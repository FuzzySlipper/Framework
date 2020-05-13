using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;

namespace PixelComrades {
    [CustomPropertyDrawer(typeof(AnimationCurve))]
    public class AnimationCurveDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            AnimationCurveGUI.OnGUI(position, property, label);
        }
    }

    public class AnimationCurvePopupMenu
    {
        

        public static void Show(Rect popupRect, AnimationCurve animationCurve, SerializedProperty property)
        {
            if (GUI.Button(popupRect, GUIContent.none, "ShurikenDropdown"))
            {
                GUIContent content = new GUIContent("Copy");
                GUIContent content2 = new GUIContent("Paste");
                GUIContent content3 = new GUIContent("Clear");
                GenericMenu genericMenu = new GenericMenu();

                if (property != null)
                {
                    genericMenu.AddItem(content, false, AnimationCurveCallbackCopy, property);
                    genericMenu.AddItem(content2, false, AnimationCurveCallbackPaste, property);
                    genericMenu.AddItem(content3, false, AnimationCurveCallbackClear, property);
                }
                else
                {
                    genericMenu.AddItem(content, false, AnimationCurveCallback2Copy, animationCurve);
                    genericMenu.AddItem(content2, false, AnimationCurveCallback2Paste, animationCurve);
                    genericMenu.AddItem(content3, false, AnimationCurveCallback2Clear, animationCurve);
                }

                if (!HasClipBoardAnimationCurve())
                {
                    genericMenu.AddDisabledItem(content2);
                }
                genericMenu.DropDown(popupRect);
                Event.current.Use();
            }
        }

        public static void AnimationCurvePreviewCacheClearCache()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ReorderableList));
            System.Type type = assembly.GetType("UnityEditorInternal.AnimationCurvePreviewCache");
            MethodInfo clearCache = type.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
            if (clearCache != null)
            {
                clearCache.Invoke(null, null);
            }
        }

        private static bool HasClipBoardAnimationCurve()
        {
            return AnimationCurveExtension.ClipBoardAnimationCurve != null;
        }

        private static void AnimationCurveCallbackCopy(object obj)
        {
            SerializedProperty property = (SerializedProperty)obj;
            AnimationCurveExtension.ClipBoardAnimationCurve = property.animationCurveValue;
        }

        private static void AnimationCurveCallbackPaste(object obj)
        {
            if (AnimationCurveExtension.ClipBoardAnimationCurve == null)
            {
                return;
            }
            SerializedProperty property = (SerializedProperty)obj;
            property.serializedObject.Update();
            property.animationCurveValue = AnimationCurveExtension.ClipBoardAnimationCurve;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void AnimationCurveCallbackClear(object obj)
        {
            SerializedProperty property = (SerializedProperty)obj;
            property.serializedObject.Update();
            property.animationCurveValue = new AnimationCurve();
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void AnimationCurveCallback2Copy(object obj)
        {
            AnimationCurve animationCurve = (AnimationCurve)obj;
            AnimationCurveExtension.ClipBoardAnimationCurve = animationCurve;
        }

        private static void AnimationCurveCallback2Paste(object obj)
        {
            if (AnimationCurveExtension.ClipBoardAnimationCurve == null)
            {
                return;
            }
            AnimationCurve animationCurve = (AnimationCurve)obj;
            animationCurve.keys = AnimationCurveExtension.ClipBoardAnimationCurve.keys;
            animationCurve.postWrapMode = AnimationCurveExtension.ClipBoardAnimationCurve.postWrapMode;
            animationCurve.preWrapMode = AnimationCurveExtension.ClipBoardAnimationCurve.preWrapMode;
            AnimationCurvePreviewCacheClearCache();
        }

        private static void AnimationCurveCallback2Clear(object obj)
        {
            AnimationCurve animationCurve = (AnimationCurve)obj;
            if (animationCurve != null)
            {
                for (int i = animationCurve.length - 1; i >= 0; i--)
                {
                    animationCurve.RemoveKey(i);
                }
                AnimationCurvePreviewCacheClearCache();
            }
        }
    }
    public class AnimationCurveGUI
    {
        private static Rect SubtractPopupWidth(Rect position)
        {
            position.width -= 18f;
            return position;
        }

        private static Rect GetPopupRect(Rect position)
        {
            position.xMin = position.xMax - 13f;
            return position;
        }

        #region EditorGUILayout
        public static AnimationCurve CurveField(AnimationCurve value, params GUILayoutOption[] options)
        {
            Rect position = EditorGUILayout.GetControlRect(false, 16f, EditorStyles.colorField, options);
            return CurveField(position, value);
        }

        public static AnimationCurve CurveField(string label, AnimationCurve value, params GUILayoutOption[] options)
        {
            Rect position = EditorGUILayout.GetControlRect(true, 16f, EditorStyles.colorField, options);
            return CurveField(position, label, value);
        }

        public static AnimationCurve CurveField(GUIContent label, AnimationCurve value, params GUILayoutOption[] options)
        {
            Rect position = EditorGUILayout.GetControlRect(true, 16f, EditorStyles.colorField, options);
            return CurveField(position, label, value);
        }
        #endregion

        #region EditorGUI
        public static AnimationCurve CurveField(Rect position, AnimationCurve value)
        {
            AnimationCurve animationCurve = EditorGUI.CurveField(SubtractPopupWidth(position), value);
            AnimationCurvePopupMenu.Show(GetPopupRect(position), animationCurve, null);
            return animationCurve;
        }

        public static AnimationCurve CurveField(Rect position, string label, AnimationCurve value)
        {
            return CurveField(position, new GUIContent(label), value);
        }

        public static AnimationCurve CurveField(Rect position, GUIContent label, AnimationCurve value)
        {
            AnimationCurve animationCurve = EditorGUI.CurveField(SubtractPopupWidth(position), label, value);
            AnimationCurvePopupMenu.Show(GetPopupRect(position), animationCurve, null);
            return animationCurve;
        }
        #endregion

        public static void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.CurveField(EditorGUI.PrefixLabel(SubtractPopupWidth(position), label), property, Color.green, default(Rect));
            AnimationCurvePopupMenu.Show(GetPopupRect(position), null, property);
            EditorGUI.EndProperty();
        }
    }
}
