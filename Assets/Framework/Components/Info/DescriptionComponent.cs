using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DescriptionComponent : IComponent {
        public int Owner { get; set; }
        public string Text;

        public DescriptionComponent(string text){
            Text = text;
        }
    }
}
