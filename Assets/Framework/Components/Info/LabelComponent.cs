using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class LabelComponent : IComponent {
        public int Owner { get; set; }
        public string Text;

        public LabelComponent(string text){
            Text = text;
        }
    }
}
