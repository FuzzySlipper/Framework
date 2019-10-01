using System;

namespace PixelComrades.Debugging
{
    public class FailedToConvertException : Exception
    {
        public FailedToConvertException(string message) : base(message)
        {

        }
    }
}