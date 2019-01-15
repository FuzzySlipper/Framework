using System;
using UnityEngine;

namespace PixelComrades {
    [AttributeUsage(AttributeTargets.Field)] 
    public class RequireInterfaceAttribute : PropertyAttribute {
        public readonly System.Type TargetType;

        public RequireInterfaceAttribute(System.Type value) {
            if (!value.IsInterface) {
                throw new Exception("Type must be an interface!");
            }
            TargetType = value;
        }
    }
}