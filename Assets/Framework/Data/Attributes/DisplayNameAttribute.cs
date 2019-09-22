using UnityEngine;
using System.Collections;

namespace PixelComrades {
	public class DisplayNameAttribute : PropertyAttribute {
        public string Varname;
        public DisplayNameAttribute(string elementTitleVar) {
            Varname = elementTitleVar;
        }
    }
}

