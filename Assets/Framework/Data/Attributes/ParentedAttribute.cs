using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace PixelComrades {
    public class DropdownListAttribute : PropertyAttribute {
        public readonly string[] List;

        public DropdownListAttribute(string[] list) {
            List = list;
        }

        public DropdownListAttribute(System.Type type, string methodName) {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            if (method != null) {
                List = method.Invoke(null, null) as string[];
            }
            else {
                Debug.LogError("NO SUCH METHOD " + methodName + " FOR " + type);
            }
        }
    }
}
