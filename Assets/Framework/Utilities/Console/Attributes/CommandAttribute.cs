using System;

namespace PixelComrades
{
    [Serializable]
    public class CommandAttribute : Attribute
    {
        public string name = "";
        public string description = "";

        public CommandAttribute(string name, string description = "")
        {
            this.name = name;
            this.description = description;
        }
    }
}