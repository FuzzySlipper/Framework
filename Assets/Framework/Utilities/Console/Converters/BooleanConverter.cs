using System;

namespace PixelComrades.Debugging
{
    public class BooleanConverter : Converter
    {
        public override Type Type
        {
            get
            {
                return typeof(bool);
            }
        }

        public BooleanConverter() { }

        public override object Convert(string value)
        {
            return value == "1" || value.ToLower() == "true";
        }
    }
}