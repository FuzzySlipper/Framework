using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UIKit.Editor
{
	/// <summary>
	/// Sprite Sheet Editor. Use Case is primarly creation
	/// of Sprite Sheets from even numbered amounts of PNG
	/// image Sequences into a Power of 2 Spritesheet for
	/// Animation. 
	/// </summary>
	public class SpriteSheetEditor : EditorWindow {
		//static private SpriteSheetEditor singleWindow;
		private static int windowWidth = 350;
		private bool detachable;
		private string folderpath, foldername = "Spritesheet";

		private int xAxis = 4;
		
		#region Window Settings
		[MenuItem ("Window/Essential UI Kit/Create Spritesheet from Sequence")]
		static void  ShowWindow () {
			SpriteSheetEditor window = EditorWindow.GetWindow<SpriteSheetEditor>(EditorPrefs.GetBool(typeof(SpriteSheetEditor).Name + "Detachable", false) ,"Create Sprite Sheet", true);
			window.minSize = new Vector2(windowWidth, 50);
			window.maxSize = new Vector2(windowWidth, 400);
			window.Repaint();
		}
		void OnEnable()
		{
			detachable = EditorPrefs.GetBool(typeof(SpriteSheetEditor).Name  + "Detachable", false);
			xAxis = EditorPrefs.GetInt(typeof(SpriteSheetEditor).Name  + "xAxisPreference", 4);
		}
		void OnDisable()
		{
			EditorPrefs.SetBool(typeof(SpriteSheetEditor).Name  + "Detachable", detachable);
			EditorPrefs.SetInt(typeof(SpriteSheetEditor).Name  + "xAxisPreference", xAxis);
		}
		void OnGUI () {
			//Header
			GUILayout.BeginArea(new Rect(7,0,this.position.width - 14,this.position.height));
			GUILayout.Space(5);
			GUILayout.Label("Create Sprite Sheet from Sequence", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			detachable = EditorGUILayout.Toggle("Detachable", detachable, GUILayout.Height(20));
			if(GUI.changed)
			{
				EditorPrefs.SetBool("detachable", detachable);
				this.Close();
				SpriteSheetEditor.ShowWindow();
			}
			if(GUILayout.Button("Choose Folder with Sequence", GUILayout.Height(25)) )
			{
				folderpath = EditorUtility.OpenFolderPanel("Folder where PNG Sequence is located", getSelectedPath() , "");	
			}

			GUILayout.Space(5);
			if(string.IsNullOrEmpty(folderpath))
			{
				EditorGUILayout.HelpBox("No folderpath given!", MessageType.Warning);
			}else
			{
				// just a little reminder that textures have to be advanced to be read (The Unity Exception thrown says pretty much the same thing but still)
				EditorGUILayout.HelpBox("Textures have to be Advanced & Read/Write has to be enabled! Texture Files have to be in PNG format.", MessageType.Info);
				xAxis = EditorGUILayout.IntField("#Sprites on X-Axis:", xAxis, GUILayout.Height(20));
				if(GUILayout.Button("Create Spritesheet", GUILayout.Height(25)) )
				{
					createSpriteSheetFromPNGSequence(folderpath);
				}
			}

			
			GUILayout.Space(5);
			//Resize window
			if(Event.current.type == EventType.Repaint)
			{	
				Rect rect = GUILayoutUtility.GetLastRect();
				this.minSize = new Vector2(windowWidth,rect.y + 4);
				this.maxSize = new Vector2(windowWidth,rect.y + 4);
			}
			EditorGUI.EndChangeCheck();
			GUILayout.EndArea();
		}
		#endregion
		
		#region Helper Functions
		
		void OnInspectorUpdate()
		{
			this.Repaint();
		}
		private void createSpriteSheetFromPNGSequence(string path)
		{
			string[] files = Directory.GetFiles(path);
			//Order is not guaranteed with Directory.GetFiles
			Array.Sort(files);
			List<Texture2D> sourceTextures = new List<Texture2D>();
			foreach(string file in files)
			{
				if(!file.EndsWith(".png")) continue;
				Texture2D tex = AssetDatabase.LoadAssetAtPath(getUnityPath(file), typeof(Texture2D)) as Texture2D;

				sourceTextures.Add(tex);
			}
			if(sourceTextures.Count <2) Debug.Log("Found fewer than 2 PNG files in given folder. No Point in creating a Sprite Sheet");
			else texturesToSpriteSheet(sourceTextures.ToArray());
		}
		private string getUnityPath(string absolutePath)
		{
			string[] folders = absolutePath.Split(Path.DirectorySeparatorChar);
			foldername = folders[folders.Length-2];
			for(int i = 0; i< folders.Length; i++)
			{	
				if(folders[i].Contains("Assets"))
				{
					int length = folders.Length-i;
					string[] unityFolders = new string[length];
					Array.Copy(folders, i, unityFolders,0, length);
					//All paths in Unity use forward slashes
					absolutePath = string.Join("/", unityFolders);
					break;
				}
			}
			return absolutePath;
		}
		//packs the texture into a bigger spritesheet
		private void texturesToSpriteSheet(Texture2D[] textures)
		{
			Vector2 spritesheetSize = calculateSizeForTextureArray(textures);
			Texture2D destinationTexture = new Texture2D((int) spritesheetSize.x, (int)spritesheetSize.y);
			int xOffset = 0, yOffset = 0, lineYOffset = 0, line = 1;
			for(int i = 0; i<textures.Length; i++)
			{
				int index = (line*xAxis) - (i%xAxis) - 1;
				if(index >= textures.Length) continue;
				Texture2D tex = textures[index];

				destinationTexture.SetPixels(xOffset, yOffset, tex.width, tex.height, tex.GetPixels() );
				xOffset += tex.width;
				lineYOffset = (tex.height > lineYOffset) ? tex.height : lineYOffset;

				if(i%xAxis == xAxis-1)
				{
					line++;
					yOffset += lineYOffset;
					lineYOffset = xOffset = 0;
				}
			}
			destinationTexture.Apply();
			byte[] pngData =  destinationTexture.EncodeToPNG();
			DestroyImmediate(destinationTexture);
			string path = EditorUtility.SaveFilePanel( "Save Spritesheet as PNG",
				getSelectedPath(),
				foldername+".png",
				"png");
			if(!string.IsNullOrEmpty(path))
			{
				File.WriteAllBytes(path, pngData);
				//Let Unity know that there is a new asset
				AssetDatabase.Refresh();
			}
		}
		//do a dry run to get the size needed for a spritesheet with the given Amount of Pictures on the XAxis
		private Vector2 calculateSizeForTextureArray(Texture2D[] textures)
		{
			Vector2 linesize = Vector2.zero, maxSize = Vector2.zero;
			for(int i = 0; i<textures.Length; i++)
			{
				Vector2 textureSize = new Vector2(textures[i].width, textures[i].height);
				linesize.x += textureSize.x;
				linesize.y = (textureSize.y < linesize.y) ? linesize.y : textureSize.y; 
				if(i%xAxis == xAxis-1)
				{
					maxSize.x = (maxSize.x < linesize.x) ? linesize.x : maxSize.x;
					maxSize.y += linesize.y;
					linesize = Vector2.zero;
				}
			}
			return maxSize;
		}
		//a little helper for giving us the selected folder
		private string getSelectedPath()
		{
			string path;
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
			{
				path = AssetDatabase.GetAssetPath(obj);
				if ( !string.IsNullOrEmpty(path) && File.Exists(path) ) 
				{
					return Path.GetDirectoryName(path);
				}
			}
			//fallback
			return "Assets";
		}
		#endregion
	}
}
