using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class EditorHelpers {
    public static void StringBox(string label, string content) {
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PrefixLabel(label);
        GUILayout.Label(content);
        EditorGUILayout.EndVertical();
    }
}

public static class UtilitiesShortcuts
{

    [MenuItem("Window/Delete EditorStatesSceneHelper")]
    public static void DeleteEditorSceneHelper() {
        GameObject helper = GameObject.Find("UMotion_EditorStatesSceneHelper");
        if (helper != null) {
            Object.DestroyImmediate(helper);
        }
    }

    [MenuItem("Utilities/Delete All Animation Events")]
    public static void GetSelections() {
        var selectedAsset = Selection.objects;
        foreach (var obj in selectedAsset) {
            if (obj == null)
                continue;
            var importer = obj as ModelImporter;
            if (importer == null) {
                importer = (ModelImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
                if (importer == null) {
                    continue;
                }
            }
            var animations = importer.clipAnimations;
            //AnimationClip[] animations = AnimationUtility.GetAnimationClips(obj as ModelImporter);
            for (int i = 0; i < animations.Length; i++) {
                var clip = animations[i];
                if (clip != null) {
                    clip.events = null;
                    //UnityEditor.AnimationUtility.SetAnimationEvents(clip., null);
                    UnityEditor.EditorUtility.SetDirty(obj);
                    Debug.LogFormat("Delete clip {0}", clip.name);
                }
            }
            //foreach (var animationClip in texture.runtimeAnimatorController.animationClips) {
            //    UnityEditor.AnimationUtility.SetAnimationEvents(animationClip.);
            //}
        }
    }
    
    [MenuItem("Utilities/Shortcuts/Unload Unusued Assets")]
	static void UnloadUnusedAssets() {
        Resources.UnloadUnusedAssets();
    }

    [MenuItem("Utilities/Shortcuts/Reload Assemblies")]
	static void ReloadAssemblies() {
         MonoScript cMonoScript = MonoImporter.GetAllRuntimeMonoScripts()[ 0 ];
        MonoImporter.SetExecutionOrder( cMonoScript, MonoImporter.GetExecutionOrder( cMonoScript ) );
    }

    [MenuItem("Utilities/Shortcuts/Clear Console %#c")] // CTRL/CMD + SHIFT + C
	public static void ClearConsole()
	{
		try
		{
			var logEntries = Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
			if(logEntries != null)
			{
				var method = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
				if(method != null)
				{
					method.Invoke(null, null);
				}
			}
		}
		catch(Exception exception)
		{
			Debug.LogError("Failed to clear the console: " + exception.ToString());
		}
	}

	//[MenuItem("Utilities/Shortcuts/Save project &%s")] // ALT + CTRL + S
	static void SaveProject()
	{
		Debug.Log("Saved assets to disk.");
		AssetDatabase.SaveAssets();
	}

	//[MenuItem("Utilities/Shortcuts/Toggle Inspector Debug %#d")] // CTRL/CMD + SHIFT + C
	public static void ToggleInspectorDebug()
	{
		try
		{
			var type = Type.GetType("UnityEditor.InspectorWindow,UnityEditor");
			if(type != null)
			{
				var window = EditorWindow.GetWindow(type);
				var field = type.GetField("m_InspectorMode", BindingFlags.Instance | BindingFlags.Public);
				if(field != null)
				{
					var mode = (InspectorMode)field.GetValue(window);
					var newMode = mode == InspectorMode.Debug ? InspectorMode.Normal : InspectorMode.Debug;

					var method = type.GetMethod("SetMode", BindingFlags.Instance | ~BindingFlags.Public);
					if(method != null)
					{
						method.Invoke(window, new object[] { newMode });
					}
				}
			}
		}
		catch(Exception exception)
		{
			Debug.LogError("Failed to toggle inspector debug: " + exception.ToString());
		}
	}

	//[MenuItem("Utilities/Shortcuts/Toggle GameView maximized %#m")] // CTRL/CMD + SHIFT + M
	public static void ToggleGameViewMaximized()
	{
		try
		{
			var type = Type.GetType("UnityEditor.GameView,UnityEditor");
			if(type != null)
			{
				var window = EditorWindow.GetWindow(type);
				var property = type.GetProperty("maximized", BindingFlags.Instance | BindingFlags.Public);
				if(property != null)
				{
					var isMaximized = (bool)property.GetValue(window, null);
					property.SetValue(window, !isMaximized, null);
				}
			}
		}
		catch(Exception exception)
		{
			Debug.LogError("Failed to toggle GameView maximized: " + exception.ToString());
		}
	}

    [MenuItem("Utilities/Shortcuts/Toggle Inspector Lock %#l")] // CTRL/CMD + SHIFT + L
	public static void ToggleInspectorLock()
	{
		try
		{
			var type = Type.GetType("UnityEditor.InspectorWindow,UnityEditor");
			if(type != null)
			{
				var window = EditorWindow.GetWindow(type);
				
				var method = type.GetMethod("FlipLocked", BindingFlags.Instance | ~BindingFlags.Public);
				if(method != null)
				{
					method.Invoke(window, null);
				}	
			}
		}
		catch(Exception exception)
		{
			Debug.LogError("Failed to toggle inspector debug: " + exception.ToString());
		}
	}

	//public delegate void ApplyOrRevertDelegate(GameObject inInstance, UnityEngine.Object inPrefab, ReplacePrefabOptions inReplaceOptions);

	//[MenuItem("Utilities/Shortcuts/Apply all selected prefabs %#e")] // CTRL/CMD + SHIFT + E
	//static void ApplyPrefabs()
	//{
	//	var count = SearchPrefabConnections((inInstance, inPrefab, inReplaceOptions) =>
	//		{
	//			PrefabUtility.ReplacePrefab(inInstance, inPrefab, inReplaceOptions);
	//		},
	//		"apply"
	//	);
	//	if(count > 0)
	//		SaveProject();
	//}

	//[MenuItem("Utilities/Shortcuts/Revert all selected prefabs &#r")] // ALT + SHIFT + R
	//static void RevertPrefabs()
	//{
	//	SearchPrefabConnections((inInstance, inPrefab, inReplaceOptions) =>
	//		{
	//			PrefabUtility.ReconnectToLastPrefab(inInstance);
	//			PrefabUtility.RevertPrefabInstance(inInstance);
	//		},
	//		"revert"
	//	);
	//}

	//static int SearchPrefabConnections(ApplyOrRevertDelegate inDelegate, string inDescriptor)
	//{
	//	var count = 0;
	//	if(inDelegate != null)
	//	{
	//		var selectedGameObjects = Selection.gameObjects;
	//		if(selectedGameObjects.Length > 0)
	//		{
	//			foreach(var gameObject in selectedGameObjects)
	//			{
	//				var prefabType = PrefabUtility.GetPrefabType(gameObject);

	//				// Is the selected GameObject a prefab?
	//				if(prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
	//				{
	//					// Get the prefab root.
	//					var prefabParent = ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(gameObject));
	//					var prefabRoot = prefabParent.transform.root.gameObject;
						
	//					var currentGameObject = gameObject;
	//					var hasFoundTopOfHierarchy = false;
	//					var canApply = true;
						
	//					// We go up in the hierarchy until we locate a GameObject that doesn't have the same GetPrefabParent return value.
	//					while(currentGameObject.transform.parent && !hasFoundTopOfHierarchy)
	//					{
	//						// Same prefab?
	//						prefabParent = ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(currentGameObject.transform.parent.gameObject));
	//						if(prefabParent && prefabRoot == prefabParent.transform.root.gameObject)
	//						{
	//							// Continue upwards.
	//							currentGameObject = currentGameObject.transform.parent.gameObject;
	//						}
	//						else
	//						{
	//							// The gameobject parent is another prefab, we stop here.
	//							hasFoundTopOfHierarchy = true;
	//							if(prefabRoot != ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(currentGameObject)))
	//							{
	//								// Gameobject is part of another prefab.
	//								canApply = false;
	//							}
	//						}
	//					}

	//					if(canApply)
	//					{
	//						count++;
	//						var parent = PrefabUtility.GetCorrespondingObjectFromSource(currentGameObject);
	//						inDelegate(currentGameObject, parent, ReplacePrefabOptions.ConnectToPrefab);
	//						var assetPath = AssetDatabase.GetAssetPath(parent);
	//						Debug.Log(assetPath + " " + inDescriptor, parent);
	//					}
	//				}
	//			}
	//			Debug.Log(count + " prefab" + (count > 1 ? "s" : "") + " updated");
	//		}
	//	}

	//	return count;
	//}
    /// <summary>
    /// The rotation to restore when going back to perspective view. If we don't have anything,
    /// default to the 'Front' view. This avoids the problem of an invalid rotation locking out
    /// any further mouse rotation
    /// </summary>
    static Quaternion sPerspectiveRotation = Quaternion.Euler(0, 0, 0);
 
    /// <summary>
    /// Whether the camera should tween between views or snap directly to them
    /// </summary>
    static bool sShouldTween = true;
 
 
    /// <summary>
    /// When switching from a perspective view to an orthographic view, record the rotation so
    /// we can restore it later
    /// </summary>
    static private void StorePerspective()
    {
        if (SceneView.lastActiveSceneView.orthographic == false)
        {
            sPerspectiveRotation = SceneView.lastActiveSceneView.rotation;
        }
    }
 
    /// <summary>
    /// Apply an orthographic view to the scene views camera. This stores the previously active
    /// perspective rotation if required
    /// </summary>
    /// <param name="newRotation">The new rotation for the orthographic camera</param>
    private static void ApplyOrthoRotation(Quaternion newRotation)
    {
        StorePerspective();
 
        SceneView.lastActiveSceneView.orthographic = true;
 
        if (sShouldTween)
        {
            SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot, newRotation);
        }
        else
        {
            SceneView.lastActiveSceneView.LookAtDirect(SceneView.lastActiveSceneView.pivot, newRotation);
        }
 
        SceneView.lastActiveSceneView.Repaint();
    }

    static private void ApplyPersRotation()
    {
 
        if (sShouldTween) {
            SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot, sPerspectiveRotation);
        }
        else
        {
            SceneView.lastActiveSceneView.LookAtDirect(SceneView.lastActiveSceneView.pivot, sPerspectiveRotation);
        }
 
        SceneView.lastActiveSceneView.orthographic = false;
 
        SceneView.lastActiveSceneView.Repaint();
    }
 
 
    [MenuItem("Utilities/Cam Top")]
    public static void TopCamera()
    {
        ApplyOrthoRotation(Quaternion.Euler(90, 0, 0));
    }
    
    [MenuItem("Utilities/Cam Bottom")]
    static void BottomCamera()
    {
        ApplyOrthoRotation(Quaternion.Euler(-90, 0, 0));
    }
 
 
    [MenuItem("Utilities/Cam Left")]
    static void LeftCamera()
    {
        ApplyOrthoRotation(Quaternion.Euler(0, 90, 0));
    }
 
 
    [MenuItem("Utilities/Cam Right")]
    static void RightCamera()
    {
        ApplyOrthoRotation(Quaternion.Euler(0, -90, 0));
    }
 
 
    [MenuItem("Utilities/Cam Front")]
    static void FrontCamera()
    {
        ApplyOrthoRotation(Quaternion.Euler(0, 0, 0));
    }
 
    [MenuItem("Utilities/Cam Back")]
    static void BackCamera()
    {
        ApplyOrthoRotation(Quaternion.Euler(0, 180, 0));
    }
 
 
    [MenuItem("Utilities/Cam Persp Toggle")]
    static void PerspCamera()
    {
        if (SceneView.lastActiveSceneView.camera.orthographic) {
            ApplyPersRotation();
        }
        else {
            ApplyOrthoRotation(SceneView.lastActiveSceneView.rotation);
        }
    }

    [MenuItem("Tools/Simulate Physics in Editor - Start")]
    static void editorSimStart() {
        Physics.autoSimulation = false;
        EditorApplication.update += PhysicsUpdate;
    }

    [MenuItem("Tools/Simulate Physics in Editor - Stop")]
    static void editorSimStop() {
        Physics.autoSimulation = true;
        EditorApplication.update -= PhysicsUpdate;
    }

    static void PhysicsUpdate() {
        Physics.Simulate(Time.fixedDeltaTime);
    }
}