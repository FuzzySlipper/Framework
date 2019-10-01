using System;

namespace PixelComrades.Debugging {
    [Serializable]
    public class AliasAttribute : Attribute
    {
        public string name = "";

        public AliasAttribute(string name)
        {
            this.name = name;
        }
    }
}
