﻿using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;

namespace PixelComrades {
    public static class CreateSpriteAnimation {

        private const int DefaultFrameRate = 12;

        [MenuItem("Assets/Create/Unity Animation From Sprite")]
        public static void CreateAnimation() {
            var texture = Selection.objects[0] as Texture2D;
            if (texture == null) {
                return;
            }
            var texturePath = AssetDatabase.GetAssetPath(texture);
            Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();
            if (allSprites.Length == 0) {
                Debug.LogError("no anim sprite");
                return;
            }
            Sprite[] animSprites = new Sprite[allSprites.Length];
            for (int i = 0; i < allSprites.Length; i++) {
                animSprites[i] = allSprites[i];
            }
            AnimationClip animClip = new AnimationClip();
            animClip.frameRate = DefaultFrameRate;
            var spriteBinding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[animSprites.Length];
            for (int i = 0; i < spriteKeyFrames.Length; i++) {
                spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                spriteKeyFrames[i].time = (float)i / DefaultFrameRate;
                spriteKeyFrames[i].value = animSprites[i];
            }
            AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);
            var path = (texturePath.Remove(texturePath.Length - 3)) + "anim";
            AssetDatabase.CreateAsset(animClip, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        [MenuItem("Assets/Create/Unity Animation From Sprite", true)]
        public static bool CreateAssetFromSelectedScript_Validator() {
            if (Selection.objects == null || Selection.objects.Length == 0) {
                return false;
            }
            if (Selection.objects.Length != 1) {
                return false;
            }
            var texture = Selection.objects[0] as Texture2D;
            if (texture == null) {
                return false;
            }
            Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().ToArray();
            if (allSprites == null || allSprites.Length == 0) {
                return false;
            }
            return true;
        }

        [MenuItem("Assets/Create/Simple Animation From Sprite", true, -100)]
        public static bool CreateSimpleAssetFromSelectedScript_Validator() {
            if (Selection.objects == null || Selection.objects.Length == 0) {
                return false;
            }
            var texture = Selection.objects[0] as Texture2D;
            if (texture == null) {
                return false;
            }
            Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().ToArray();
            if (allSprites == null || allSprites.Length == 0) {
                return false;
            }
            return true;
        }

        [MenuItem("Assets/Create/Simple Animation From Sprite", false, -100)]
        public static void CreateSimpleAnimation() {
            
            for (int i = 0; i < Selection.objects.Length; i++) {
                var texture = Selection.objects[i] as Texture2D;
                if (texture != null) {
                    CreateSimpleAnimation(texture);
                }
            }
        }

        [MenuItem("GameObject/Delete Children", true, -15)]
        public static bool DeleteChildren_Validator() {
            if (Selection.activeGameObject == null || Selection.gameObjects == null || Selection.gameObjects.Length == 0) {
                return false;
            }
            if (Selection.activeGameObject != null && (!Selection.activeGameObject.scene.IsValid() || Selection.activeGameObject.transform.childCount == 0)) {
                return false;
            }
            for (int i = 0; i < Selection.gameObjects.Length; i++) {
                if (!Selection.gameObjects[i].scene.IsValid()) {
                    return false;
                }
                if (Selection.gameObjects[i].transform.childCount == 0) {
                    return false;
                }
            }
            return true;
        }

        [MenuItem("GameObject/Delete Children", false, -15)]
        public static void DeleteChildren() {
            foreach (var gameObject in Selection.gameObjects) {
                gameObject.transform.DeleteChildren();
            }
        }

        private static void CreateSimpleAnimation(Texture2D texture) {
            if (texture == null) {
                return;
            }
            var texturePath = AssetDatabase.GetAssetPath(texture);
            Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();
            if (allSprites.Length == 0) {
                return;
            }
            Sprite[] animSprites = new Sprite[allSprites.Length];
            for (int i = 0; i < allSprites.Length; i++) {
                animSprites[i] = allSprites[i];
            }
            var path = (texturePath.Remove(texturePath.Length - 3));
            var holder = CreateAnimationHolder(path);
            holder.Frames = new AnimationFrame[allSprites.Length];
            for (var i = 0; i < allSprites.Length; i++) {
                holder.Frames[i] = new AnimationFrame {
                    Length = 1,
                };
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            holder.Sprites = allSprites;
            EditorUtility.SetDirty(holder);
        }

        private static SimpleAnimation CreateAnimationHolder(string path) {
            var scriptableObject = ScriptableObject.CreateInstance<SimpleAnimation>();
            AssetDatabase.CreateAsset(scriptableObject, path + "asset");
            AssetDatabase.SaveAssets();
            return scriptableObject;
        }
    }
}
