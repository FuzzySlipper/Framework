using System;

namespace PixelComrades.Debugging
{
    [Serializable]
    public class CategoryAttribute : Attribute
    {
        public string name = "";

        public CategoryAttribute(string name)
        {
            this.name = name;
        }
    }
}
