using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MarkovNameGenerator {

    //private static MarkovNameGenerator _instance;
    //private static IEnumerable<string> _hinduStrings = new List<string>() {
    //    " ANANTA"," ANIL"," ANIRUDDHA"," ARJUNA"," ARUNA"," ARUNDHATI"," BALA"," BALADEVA"," BHARATA"," BHASKARA"," BRAHMA"," BRIJESHA"," CHANDRA"," DAMAYANTI"," DAMODARA"," DEVARAJA"," DEVI"," DILIPA"," DIPAKA"," DRAUPADI"," DRUPADA"," DURGA"," GANESHA"," GAURI"," GIRISHA"," GOPALA"," GOPINATHA"," GOTAMA"," GOVINDA"," HARI"," HARISHA"," INDIRA"," INDRA"," INDRAJIT"," INDRANI"," JAGANNATHA"," JAYA"," JAYANTI"," KALI"," KALYANI"," KAMA"," KAMALA"," KANTI"," KAPILA"," KARNA"," KRISHNA"," KUMARA"," KUMARI"," LAKSHMANA"," LAKSHMI"," LALITA"," MADHAVA"," MADHAVI"," MAHESHA"," MANI"," MANU"," MAYA"," MINA"," MOHANA"," MOHINI"," MUKESHA"," MURALI"," NALA"," NANDA"," NARAYANA"," PADMA"," PADMAVATI"," PANKAJA"," PARTHA"," PARVATI"," PITAMBARA"," PRABHU"," PRAMODA"," PRITHA"," PRIYA"," PURUSHOTTAMA"," RADHA"," RAGHU"," RAJANI"," RAMA"," RAMACHANDRA"," RAMESHA"," RATI"," RAVI"," REVA"," RUKMINI"," SACHIN"," SANDHYA"," SANJAYA"," SARASWATI"," SATI"," SAVITR"," SAVITRI"," SHAILAJA"," SHAKTI"," SHANKARA"," SHANTA"," SHANTANU"," SHIVA"," SHIVALI"," SHRI"," SHRIPATI"," SHYAMA"," SITA"," SRI"," SUMATI"," SUNDARA"," SUNITA"," SURESHA"," SURYA"," SUSHILA"," TARA"," UMA"," USHA"," USHAS"," VALLI"," VASANTA"," VASU"," VIDYA"," VIJAYA"," VIKRAMA"," VISHNU"," YAMA"," YAMI"
    //};
        private const int LoopLimit = 5500;

        private Dictionary<string, List<char>> _chains = new Dictionary<string, List<char>>();
        private List<string> _samples = new List<string>();
        private List<string> _used = new List<string>();
        private System.Random _rnd = new System.Random();
        private int _order;
        private int _minLength;
        private WhileLoopLimiter _outerLoop = new WhileLoopLimiter(LoopLimit);
        private WhileLoopLimiter _innerLoop = new WhileLoopLimiter(LoopLimit);
        private char GetLetter(string token) {
            if (!_chains.ContainsKey(token)) {
                return '?';
            }

            List<char> letters = _chains[token];
            int n = _rnd.Next(letters.Count);
            return letters[n];
        }

        public MarkovNameGenerator(string[] tokens, int order, int minLength) {
            //fix parameter values
            if (order < 1) {
                order = 1;
            }
            if (minLength < 1) {
                minLength = 1;
            }
            _order = order;
            _minLength = minLength;

            //split comma delimited lines
            //for (var i = 0; i < sampleNames.Length; i++) {
            //    string line = sampleNames[i];
            //    string[] tokens = line.Split(',');
            //    for (var index = 0; index < tokens.Length; index++) {
            //        string word = tokens[index];
            //        string upper = word.Trim().ToUpper();
            //        if (upper.Length < order + 1)
            //            continue;
            //        _samples.Add(upper);
            //    }
            //}

            for (var index = 0; index < tokens.Length; index++) {
                string word = tokens[index];
                string upper = word.Trim().ToUpper();
                if (upper.Length < order + 1) {
                    continue;
                }

                _samples.Add(upper);
            }

            //Build chains            
            for (var i = 0; i < _samples.Count; i++) {
                string word = _samples[i];
                for (int letter = 0; letter < word.Length - order; letter++) {
                    string token = word.Substring(letter, order);
                    List<char> entry = null;
                    if (_chains.ContainsKey(token)) {
                        entry = _chains[token];
                    }
                    else {
                        entry = new List<char>();
                        _chains[token] = entry;
                    }
                    entry.Add(word[letter + order]);
                }
            }
        }

        //Get the next random name
        public string NextName() {
            //get a random token somewhere in middle of sample word                
            string s = "";
            _outerLoop.Reset();
            while (_used.Contains(s) || s.Length < _minLength) {
                if (!_outerLoop.Advance()) {
                    break;
                }
                int n = _rnd.Next(_samples.Count);
                int nameLength = _samples[n].Length;
                s = _samples[n].Substring(_rnd.Next(0, _samples[n].Length - _order), _order);
                _innerLoop.Reset();
                while (s.Length < nameLength) {
                    if (!_innerLoop.Advance()) {
                        break;
                    }
                    string token = s.Substring(s.Length - _order, _order);
                    char c = GetLetter(token);
                    if (c != '?') {
                        s += GetLetter(token);
                    }
                    else {
                        break;
                    }
                }

                if (s.Contains(" ")) {
                    string[] tokens = s.Split(' ');
                    s = "";
                    for (int t = 0; t < tokens.Length; t++) {
                        if (tokens[t] == "") {
                            continue;
                        }

                        if (tokens[t].Length == 1) {
                            tokens[t] = tokens[t].ToUpper();
                        }
                        else {
                            tokens[t] = tokens[t].Substring(0, 1) + tokens[t].Substring(1).ToLower();
                        }

                        if (s != "") {
                            s += " ";
                        }

                        s += tokens[t];
                    }
                }
                else {
                    s = s.Substring(0, 1) + s.Substring(1).ToLower();
                }
            }
            _used.Add(s);
            return s;
        }

        //Reset the used names
        public void Reset() {
            _used.Clear();
        }

    }
}