using System;

namespace PixelComrades.Debugging
{
    public class ObjectConverter : Converter
    {
        public override Type Type
        {
            get
            {
                return typeof(object);
            }
        }

        public ObjectConverter() { }

        public override object Convert(string value)
        {
            return value as object;
        }
    }
}