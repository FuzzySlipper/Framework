using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.Utilities.Editor;
using Sirenix.Serialization;
using UnityEditor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public sealed class ShaderKeywordsTool : OdinEditorWindow {
        
        [MenuItem("Tools/Shader Keywords Tool")]
        public static void ShowWindow() {
            var window = GetWindow<ShaderKeywordsTool>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1200, 800);
            window.Show();
        }

        [System.Serializable]

        public class ShaderKeywords {
            public string ShaderName;
            public string Path;
            public List<string> KeywordsUsedList = new List<string>();
            public bool Expand;
            public bool Selected;
        }
        
        public string[] SkipKeys = {"#pragma", "shader_feature", "multi_compile", "addshadow", "fullforwardshadows", "tessellate:TessFunction", "exclude_path:deferred", "exclude_path:forward", "exclude_path:prepass", "noshadow", "noambient", "novertexlights", "nolightmap", "nodynlightmap", "nodirlightmap", "nofog", "nometa", "noforwardadd", "softvegetation", "interpolateview", "halfasview", "approxview", "dualforward"};
        public string[] SkipProperties = {"surface", "ARB_precision_hint_fastest", "lambert", "surf", "skip_variants", ".", "target", "vert", "frag", "vertex", "gles", "glsl", "BlinnPhong", "exclude_renderers", "nomrt", "fragment", "geometry", "hull", "domain", "only_renderers", "enable_d3d11_debug_symbols"};
        public string GlobalKeywords;
        public List<string> GlobalKeywordsFound = new List<string>();
        public int MaterialsFound;
        public List<ShaderKeywords> ShaderKeywordsList = new List<ShaderKeywords>();
        public bool ShowProjectKeywords;
        public bool ShowProjectShaders;
        public int MaterialKeywordsFound;

        private List<string> _sharedKeywordsList = new List<string>();
        private ShaderKeywords _shaderKeywords;
        private Color _selectedColor = new Color(.54f, .54f, .42f, 1f);
        private List<string> _keywordsFound = new List<string>();

        public void BeginSearch() {
            int count = SkipProperties.Length;
            for (int p = 0; p < count; p++) {
                SkipProperties[p] = SkipProperties[p].ToLower();
            }
            count = SkipKeys.Length;
            for (int k = 0; k < count; k++) {
                SkipKeys[k] = SkipKeys[k].ToLower();
            }
        }

        protected override void OnGUI() {
            base.OnGUI();
            int count;
            if (GUILayout.Button("\r\nScan Project for Shader Keywords\r\n")) {
                ScanForKeywords();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);

            if (GlobalKeywordsFound.Count > 0)
            {
                GUILayout.Label("Keywords found in project:" + GlobalKeywordsFound.Count);
                GUILayout.Space(10);


                if (!ShowProjectKeywords)
                {
                    if (GUILayout.Button("Show Project Keywords"))
                    {
                        ShowProjectKeywords = true;
                    }

                }
                else
                {
                    if (GUILayout.Button("Hide Project Keywords"))
                    {
                        ShowProjectKeywords = false;
                    }
                    GUILayout.Space(10);

                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.TextArea(GlobalKeywords);
                }
                GUILayout.Space(10);
                GUILayout.Label("Shaders found in project:" + ShaderKeywordsList.Count);
                GUILayout.Space(10);
                if (!ShowProjectShaders)
                {
                    if (GUILayout.Button("Show Project Shaders"))
                    {
                        ShowProjectShaders = true;
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    if (GUILayout.Button("Hide Project Shaders"))
                    {
                        ShowProjectShaders = false;
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.Space(10);

                    count = ShaderKeywordsList.Count;

                    for (int i = 0; i < count; i++)
                    {
                        _shaderKeywords = ShaderKeywordsList[i];

                        if (_shaderKeywords.Selected)
                        {
                            GUI.backgroundColor = _selectedColor;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(" Show Shader", GUILayout.Width(90)))
                            {
                                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(_shaderKeywords.Path));
                            }

                            if (_shaderKeywords.Selected)
                            {
                                if (GUILayout.Button(_shaderKeywords.ShaderName + " ( Shared Keywords " + _sharedKeywordsList.Count + " )"))
                                {
                                    _shaderKeywords.Selected = false;
                                    CalculateSharedKeywordList();
                                }

                            }
                            else
                            {
                                if (GUILayout.Button(_shaderKeywords.ShaderName + " ( Keywords " + _shaderKeywords.KeywordsUsedList.Count + " )"))
                                {
                                    _shaderKeywords.Selected = true;
                                    CalculateSharedKeywordList();
                                }
                            }

                            if (!_shaderKeywords.Expand)
                            {
                                if (GUILayout.Button("More info...", GUILayout.Width(70)))
                                {
                                    _shaderKeywords.Expand = true;
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Less info...", GUILayout.Width(70)))
                                {
                                    _shaderKeywords.Expand = false;
                                }
                            }

                            if (_shaderKeywords.Selected)
                            {
                                if (GUILayout.Button("◄", GUILayout.Width(20)))
                                {
                                    count = ShaderKeywordsList.Count;

                                    for (int s = 0; s < count; s++)
                                    {
                                        ShaderKeywordsList[s].Selected = false;
                                    }

                                    _sharedKeywordsList.Clear();
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUI.backgroundColor = Color.white;

                        if (_shaderKeywords.Expand)
                        {
                            GUILayout.Space(5);
                            GUILayout.BeginHorizontal();
                            {

                                GUILayout.Label(_shaderKeywords.Path);
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5);

                            if (_shaderKeywords.Selected)
                            {
                                foreach (string keyword in _sharedKeywordsList)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label("     " + keyword);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            else
                            {

                                foreach (string keyword in _shaderKeywords.KeywordsUsedList)
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label("     " + keyword);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }

                            GUILayout.Space(10);
                        }
                    }
                }

                GUILayout.Space(10);
                GUILayout.Label("Unused Keywords found in materials:" + MaterialKeywordsFound);
                GUILayout.Space(10);

                if (MaterialKeywordsFound > 0)
                {
                    if (GUILayout.Button("\r\nRemove " + MaterialKeywordsFound + " Unused keywords found in " + MaterialsFound + " Materials\r\n(Recommend backing up your project first as this cannot be undone)\r\n")) {
                        RemoveMaterialKeywords();
                    }
                }
            }
        }

        private void CalculateSharedKeywordList() {
            _sharedKeywordsList.Clear();
            int count = ShaderKeywordsList.Count;

            for (int i = 0; i < count; i++) {
                _shaderKeywords = ShaderKeywordsList[i];
                int keyword_count = _shaderKeywords.KeywordsUsedList.Count;

                if (_shaderKeywords.Selected) {
                    for (int k = 0; k < keyword_count; k++) {
                        string keyword = _shaderKeywords.KeywordsUsedList[k];

                        if (!_sharedKeywordsList.Contains(keyword)) {
                            _sharedKeywordsList.Add(keyword);
                        }
                    }
                }
            }
        }
        
        private void FindKeywords(string file) {
            string[] filedata = File.ReadAllLines(file);
            int count = filedata.Length;
            int keywords_count;
            string[] keywords;
            string line;
            string s;
            int pragma_Index;
            _keywordsFound.Clear();
            for (int i = 0; i < count; i++) {
                line = filedata[i];
                pragma_Index = line.IndexOf("#pragma");
                if (pragma_Index != -1) {
                    s = line.Substring(pragma_Index, line.Length - pragma_Index);
                    s = s.TrimStart();
                    s = s.TrimEnd();
                    if (!CheckSkipProperties(s)) {
                        keywords = s.Split(' ');
                        keywords_count = keywords.Length;
                        for (int k = 0; k < keywords_count; k++) {
                            if (!CheckSkipKeys(keywords[k])) {
                                if (!_keywordsFound.Contains(keywords[k])) {
                                    _keywordsFound.Add(keywords[k]);
                                }
                                if (!GlobalKeywordsFound.Contains(keywords[k])) {
                                    GlobalKeywordsFound.Add(keywords[k]);
                                }
                            }
                        }
                    }
                }
            }
            if (_keywordsFound.Count > 0) {
                ShaderKeywords shaderKeywords = new ShaderKeywords();
                ShaderKeywordsList.Add(shaderKeywords);
                int lastSlashIndex = file.LastIndexOf("\\") + 1;
                shaderKeywords.ShaderName = file.Substring(lastSlashIndex, file.Length - lastSlashIndex - 7);
                int assetPathIndex = file.IndexOf("/Assets") + 1;
                shaderKeywords.Path = file.Substring(assetPathIndex, file.Length - assetPathIndex);

                //Debug.LogError("Filename:"+shaderKeywords.shaderName+":"+keywordsFound.Count);
                keywords_count = _keywordsFound.Count;
                for (int k = 0; k < keywords_count; k++) {
                    //Debug.Log(keywordsFound[k]);
                    shaderKeywords.KeywordsUsedList.Add(_keywordsFound[k]);
                }
            }
        }

        private void CheckMaterials(string file, bool write) {
            string materialReplacementText;
            bool updateFile = false;
            string[] keywords;
            string[] filedata = File.ReadAllLines(file);
            int count = filedata.Length;
            for (int i = 0; i < count; i++) {
                if (filedata[i].IndexOf("m_ShaderKeywords") != -1) {
                    int last = filedata[i].IndexOf(":");
                    if (last != -1 && filedata[i].Length > last + 1) {
                        materialReplacementText = filedata[i].Remove(last + 1);
                        keywords = filedata[i].TrimStart().TrimEnd().Split(' ');
                        int keywordCount = keywords.Length;
                        for (int k = 1; k < keywordCount; k++) {
                            if (keywords[k] != "" || keywords[k] != " ") {
                                if (GlobalKeywordsFound.Contains(keywords[k])) {
                                    materialReplacementText += " " + keywords[k];
                                }
                                else {
                                    updateFile = true;
                                    MaterialKeywordsFound++;
                                }
                            }
                        }
                        //filedata[i] = filedata[i].Remove(last + 1);
                        //Debug.Log(file);
                        //Debug.Log(filedata[i]);
                        if (updateFile) {
                            MaterialsFound++;
                            //Debug.LogError(materialReplacementText);
                            if (write) {
                                filedata[i] = materialReplacementText;
                            }
                        }
                    }
                }
            }
            if (write) {
                File.WriteAllLines(file, filedata);
            }
        }

        private void CountMaterialKeywords() {
            List<string> fileList = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories).ToList();
            int count = fileList.Count;
            for (int i = 0; i < count; i++) {
                CheckMaterials(fileList[i], false);
            }
        }

        private void RemoveMaterialKeywords() {
            List<string> fileList = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories).ToList();
            int count = fileList.Count;
            for (int i = 0; i < count; i++) {
                CheckMaterials(fileList[i], true);
            }
            MaterialsFound = 0;
            MaterialKeywordsFound = 0;
        }

        private void ScanForKeywords() {
            GlobalKeywords = "";
            GlobalKeywordsFound.Clear();
            _keywordsFound.Clear();
            ShaderKeywordsList.Clear();
            MaterialsFound = 0;
            MaterialKeywordsFound = 0;
            List<string> fileList = Directory.GetFiles(Application.dataPath, "*.shader", SearchOption.AllDirectories).ToList();
            int count = fileList.Count;
            for (int i = 0; i < count; i++) {
                FindKeywords(fileList[i]);
            }
            count = GlobalKeywordsFound.Count;
            //Debug.LogError("Global Keywords found:"+count);
            for (int g = 0; g < count; g++) {
                GlobalKeywords += GlobalKeywordsFound[g] + " ";
            }

            //Debug.Log(globalKeywords);
            CountMaterialKeywords();
        }

        private bool CheckSkipKeys(string s) {
            if (s == "_" || s == "__" || s == "___") {
                return true;
            }
            int count = SkipKeys.Length;
            for (int k = 0; k < count; k++) {
                //Debug.LogError(s.ToLower()+":"+_skipKeys[k]);
                if (s.ToLower().IndexOf(SkipKeys[k]) != -1) {
                    return true;
                }
            }
            return false;
        }

        private bool CheckSkipProperties(string s) {
            int count = SkipProperties.Length;
            for (int p = 0; p < count; p++) {
                if (s.ToLower().IndexOf(SkipProperties[p]) != -1) {
                    return true;
                }
            }
            return false;
        }
    }
}
