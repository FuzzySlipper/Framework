using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using GameObject = UnityEngine.GameObject;

namespace PixelComrades
{
	public class UIAnimatorWindow : EditorWindow
	{
		//static private UIAnimatorWindow singleWindow;
		private static int windowWidth = 350;
		private int keyframes = 100;
		private float keyframeReductionTolerance = 0f;
		private int startkeyframe, endkeyframe;
		private string propertyName;
		private AnimationCurve originalCurve, proposedCurve;
		private string[] properties, displayableProperties;
		private int propertiesindex = 0;
		private EasingTypes easeType = EasingTypes.Linear;
		private AnimationClip clip, oldclip = null;
		private List<EditorCurveBinding> bindings = new List<EditorCurveBinding>();
		private bool detachable = true, valid = false;
		
		[MenuItem ("Window/Essential UI Kit/Add Easing to Animation")]
		static void  ShowWindow () {
			UIAnimatorWindow window = EditorWindow.GetWindow<UIAnimatorWindow>(EditorPrefs.GetBool("UIAnimationWindowDetachable", false) ,"Animation Easing", true);
			window.minSize = new Vector2(windowWidth, 50);
			window.maxSize = new Vector2(windowWidth, 400);
		}
		void OnEnable()
		{
			detachable = EditorPrefs.GetBool("UIAnimationWindowDetachable", false);
		}
		void OnDisable()
		{
			EditorPrefs.SetBool("UIAnimationWindowDetachable", detachable);
		}

		void OnGUI () {

			GUILayout.BeginArea(new Rect(7,0,this.position.width - 14,this.position.height));
			EditorGUI.BeginChangeCheck();
			GUILayout.Space(5);
			GUILayout.Label("Add easing to UI Animation", EditorStyles.boldLabel);

			detachable = EditorGUILayout.Toggle("Detachable", detachable, GUILayout.Height(20));
			if(GUI.changed)
			{
				EditorPrefs.SetBool("UIAnimationWindowDetachable", detachable);
				this.Close();
				UIAnimatorWindow.ShowWindow();
			}
			clip = EditorGUILayout.ObjectField("Animation Clip:", clip, typeof(AnimationClip), true, GUILayout.Height(16)) as AnimationClip;
			GUILayout.Space(5);
			if(clip == null)
			{
				EditorGUILayout.HelpBox("Select An Animation Clip!", MessageType.Info);
				valid = false;
				End();
				return;
			}
			//if the user changed the clip get the new bindings
			if(clip != oldclip)
			{
				refreshClipData(clip);
			}
			if(properties.Length > 0)
			{
				valid = true;
				//getting the selected Property from the user
				propertiesindex = EditorGUILayout.Popup("Property:", propertiesindex, displayableProperties, GUILayout.Height(25));
				propertyName = properties[propertiesindex];

				originalCurve = EditorGUILayout.CurveField("CurrentRope Curve: ", curveByPropertyName(propertyName), Color.magenta, new Rect() , GUILayout.Height(windowWidth/3));

				startkeyframe = EditorGUILayout.IntSlider("Start from Keyframe:" ,startkeyframe, 0, originalCurve.keys.Length -1, GUILayout.Height(17));
				endkeyframe = EditorGUILayout.IntSlider("End with Keyframe:" ,endkeyframe, 0, originalCurve.keys.Length -1, GUILayout.Height(17));
				if(startkeyframe >= endkeyframe)
				{
					EditorGUILayout.HelpBox("No Keyframes would be affected!", MessageType.Warning);
					valid = false;
				}
				GUILayout.Space(5);
				GUILayout.Label("Easing:", EditorStyles.label);
				easeType = (EasingTypes)EditorGUILayout.EnumPopup(easeType, GUILayout.Height(18));
				GUILayout.Space(5);
				keyframes = EditorGUILayout.IntField("Number of Keyframes:", keyframes, GUILayout.Height(17));
				if(keyframes <2)
				{
					EditorGUILayout.HelpBox("At Least 2 Keyframes are needed!", MessageType.Error);
					valid = false;
				}
				keyframeReductionTolerance = EditorGUILayout.FloatField("Reduction Tolerance:", keyframeReductionTolerance, GUILayout.Height(17));

				if(valid)
				{
					proposedCurve = modifiedCurve(originalCurve);
					//Showing the proposed Curve can cause the window to not draw until dragged when reopening. 
					proposedCurve = EditorGUILayout.CurveField("New Curve: ", proposedCurve, Color.green, new Rect(), GUILayout.Height(windowWidth/3));
				}
			}else
			{
				EditorGUILayout.HelpBox("No Modifiable Curves on this Clip", MessageType.Warning);	
			}
			if(valid)
			{
				if(GUILayout.Button("Modify Animation", GUILayout.Height(25)) )
				{
					applyModifications();
				}
			}
			End();
		}

		
		#region Helper Functions

		private void End()
		{
			GUILayout.Space(5);
			//Resize window
			if(Event.current.type == EventType.Repaint)
			{	
				Rect rect = GUILayoutUtility.GetLastRect();
				this.minSize = new Vector2(windowWidth,rect.y + 8);
				this.maxSize = new Vector2(windowWidth,rect.y +8);
			}
			GUILayout.EndArea();
			EditorGUI.EndChangeCheck();
		}
		void OnInspectorUpdate()
		{
			this.Repaint();
		}
		private void refreshClipData(AnimationClip clip)
		{
			propertiesindex = 0;
			bindings = new List<EditorCurveBinding>(AnimationUtility.GetCurveBindings(clip));
			oldclip = clip;
			getClipProperties();
		}
		
		private void getClipProperties()
		{
			properties = bindings.GetNameArray((x)=>{ return x.propertyName;});
			displayableProperties = bindings.GetNameArray((x)=>{ return ObjectNames.NicifyVariableName(x.propertyName);});
		}

		private AnimationCurve curveByPropertyName(string name)
		{
			foreach(EditorCurveBinding b in bindings)
			{
				if(b.propertyName == name)
				{
					return AnimationUtility.GetEditorCurve(clip, b);
				}
			}
			return null;
		}
		private EditorCurveBinding bindingByName(string name)
		{
			foreach(EditorCurveBinding b in bindings)
			{
				if(b.propertyName == name)
				{
					return b;
				}
			}
			return default(EditorCurveBinding);
		}
		private AnimationCurve modifiedCurve(AnimationCurve original)
		{
			List<Keyframe> tmp = new List<Keyframe>();
			float from, to, length, offset;
			from = original.keys[startkeyframe].value;
			to = original.keys[endkeyframe].value;
			offset = original.keys[startkeyframe].time;
			length = Mathf.Abs(original.keys[endkeyframe].time - original.keys[startkeyframe].time);
			tmp.AddRange(original.keys.Slice(0, startkeyframe));
			tmp.AddRange(EasingExtensionMethods.easedKeys(Easing.Function(easeType),keyframes,from, to, keyframeReductionTolerance,length, offset ));
			tmp.AddRange(original.keys.Slice(endkeyframe, original.keys.Length));
			AnimationCurve c = new AnimationCurve(tmp.ToArray());
			c.smoothAllTangents(0f);
			return c;
		}
		private void applyModifications()
		{
			EditorCurveBinding binding = bindingByName(propertyName);
			Undo.RecordObject (clip, "Added Easing to Clip");
			AnimationUtility.SetEditorCurve(clip, binding, proposedCurve);
		}
		#endregion
	}
#if UNITY_EDITOR
    
    public static class EditorListExtension{

	public static string[] GetNameArray<T>(this List<T> list, Func<T, string> nameFunction)
	{
		string[] names = new string[list.Count];

		for(var i = 0; i < list.Count; i++)
		{
			names[i] = nameFunction(list[i]);
		}
		return names;
	}

    public static class EditorSave {

        public static void SetDirty(UnityEngine.Object obj) {
            EditorUtility.SetDirty(obj);
            if (obj is GameObject go) {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
            }
        }
    }
}
#endif

}
