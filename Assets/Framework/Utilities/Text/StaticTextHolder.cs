using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class StaticTextHolder : ScriptableObject {

        [SerializeField] private string _label = "";
        [SerializeField, TextArea(5,100)] private string _text = "";

        public string Text { get { return _text; } }
        public string Label { get { return string.IsNullOrEmpty(_label) ? name : _label; } }
        public string ID { get { return name; } }
    }
}
