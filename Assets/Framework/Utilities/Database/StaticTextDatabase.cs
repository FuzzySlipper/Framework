using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class StaticTextDatabase : ScriptableObject {

        private static StaticTextDatabase _main;

        public static StaticTextDatabase Main {
            get {
                if (_main == null) {
                    _main = Resources.Load<StaticTextDatabase>("StaticTextDatabase");
                }
                return _main;
            }
        }

        public string CustomSearchPath = "GameData\\Text\\";

        [SerializeField] private List<StaticTextHolder> _text = new List<StaticTextHolder>();
        [SerializeField] private MarkovHolder[] _markovHolders = new MarkovHolder[0];
        [SerializeField] private MarkovHolder _playerMaleName = null;
        [SerializeField] private MarkovHolder _playerFemaleName = null;
        [SerializeField] private MarkovHolder _spellName = null;
        [SerializeField] private MarkovHolder _nodeName = null;

        private Dictionary<string, StaticTextHolder> _labelDict = new Dictionary<string, StaticTextHolder>();
        private Dictionary<string, StaticTextHolder> _nameDict = new Dictionary<string, StaticTextHolder>();
        private Dictionary<string, MarkovHolder> _markovDict = new Dictionary<string, MarkovHolder>();

        public List<StaticTextHolder> AllText { get { return _text; } }
        public Dictionary<string, StaticTextHolder> LabelDict {
            get {
                if (_labelDict.Count == 0) {
                    for (int i = 0; i < _text.Count; i++) {
                        if (_labelDict.ContainsKey(_text[i].Label)) {
                            continue;
                        }
                        _labelDict.Add(_text[i].Label, _text[i]);
                    }
                }
                return _labelDict;
            }
        }

        public Dictionary<string, StaticTextHolder> NameDict {
            get {
                if (_nameDict.Count == 0) {
                    for (int i = 0; i < _text.Count; i++) {
                        if (_nameDict.ContainsKey(_text[i].name)) {
                            continue;
                        }
                        _nameDict.Add(_text[i].name, _text[i]);
                    }
                }
                return _nameDict;
            }
        }

        public Dictionary<string, MarkovHolder> MarkovDict {
            get {
                if (_markovDict.Count == 0) {
                    for (int i = 0; i < _markovHolders.Length; i++) {
                        if (_markovDict.ContainsKey(_markovHolders[i].Label)) {
                            continue;
                        }
                        _markovDict.Add(_markovHolders[i].Label, _markovHolders[i]);
                    }
                }
                return _markovDict;
            }
        }

        public static StaticTextHolder GetText(string searchString, bool isLabel) {
            Dictionary<string, StaticTextHolder> dict = isLabel ? Main.LabelDict : Main.NameDict;
            StaticTextHolder text;
            return dict.TryGetValue(searchString, out text) ? text : null;
        }

        public static string GetName(string label) {
            MarkovHolder holder;
            if (Main.MarkovDict.TryGetValue(label, out holder)) {
                return holder.GetName();
            }
            holder = Main._markovHolders.RandomElement();
            return holder.GetName();
        }

        public static string RandomPlayerMaleName() {
            return Main._playerMaleName.GetName();
        }

        public static string RandomPlayerFemaleName() {
            return Main._playerFemaleName.GetName();
        }

        public static string RandomSpellName() {
            return Main._spellName.GetName();
        }

        public static string RandomNodeName() {
            return Main._nodeName.GetName();
        }
    }
}
