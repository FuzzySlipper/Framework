using System;
using System.Collections.Generic;

namespace PixelComrades {
    /// <summary>
    ///     Mutable String class, optimized for speed and memory allocations while retrieving the final result as a string.
    ///     Similar use than StringBuilder, but avoid a lot of allocations done by StringBuilder (conversion of int and float
    ///     to string, frequent capacity change, etc.)
    ///     Author: Nicolas Gadenne contact@gaddygames.com
    /// </summary>
    public class FastString {
        ///<summary>Working mutable string</summary>
        private char[] _mBuffer;
        private int _mBufferPos;
        private int _mCharsCapacity;
        ///<summary>Is m_stringGenerated is up to date ?</summary>
        private bool _mIsStringGenerated;
        ///<summary>Temporary string used for the Replace method</summary>
        private List<char> _mReplacement;
        ///<summary>Immutable string. Generated at last moment, only if needed</summary>
        private string _mStringGenerated = "";
        private object _mValueControl;
        private int _mValueControlInt = int.MinValue;

        public FastString(int initialCapacity = 32) {
            _mBuffer = new char[_mCharsCapacity = initialCapacity];
        }

        private void ReallocateIfn(int nbCharsToAdd) {
            if (_mBufferPos + nbCharsToAdd > _mCharsCapacity) {
                _mCharsCapacity = Math.Max(_mCharsCapacity + nbCharsToAdd, _mCharsCapacity * 2);
                char[] newChars = new char[_mCharsCapacity];
                _mBuffer.CopyTo(newChars, 0);
                _mBuffer = newChars;
            }
        }

        ///<summary>Append a string without memory allocation</summary>
        public FastString Append(string value) {
            if (string.IsNullOrEmpty(value)) {
                return this;
            }
            ReallocateIfn(value.Length);
            int n = value.Length;
            for (int i = 0; i < n; i++) {
                _mBuffer[_mBufferPos + i] = value[i];
            }
            _mBufferPos += n;
            _mIsStringGenerated = false;
            return this;
        }

        ///<summary>Append an object.ToString(), allocate some memory</summary>
        public FastString Append(object value) {
            Append(value.ToString());
            return this;
        }

        ///<summary>Append an int without memory allocation</summary>
        public FastString Append(int value) {
            // Allocate enough memory to handle any int number
            ReallocateIfn(16);

            // Handle the negative case
            if (value < 0) {
                value = -value;
                _mBuffer[_mBufferPos++] = '-';
            }

            // Copy the digits in reverse order
            int nbChars = 0;
            do {
                _mBuffer[_mBufferPos++] = (char) ('0' + value % 10);
                value /= 10;
                nbChars++;
            }
            while (value != 0);

            // Reverse the result
            for (int i = nbChars / 2 - 1; i >= 0; i--) {
                char c = _mBuffer[_mBufferPos - i - 1];
                _mBuffer[_mBufferPos - i - 1] = _mBuffer[_mBufferPos - nbChars + i];
                _mBuffer[_mBufferPos - nbChars + i] = c;
            }
            _mIsStringGenerated = false;
            return this;
        }

        ///<summary>Append a float without memory allocation.</summary>
        public FastString Append(float valueF) {
            double value = valueF;
            _mIsStringGenerated = false;
            ReallocateIfn(32); // Check we have enough buffer allocated to handle any float number

            // Handle the 0 case
            if (value == 0) {
                _mBuffer[_mBufferPos++] = '0';
                return this;
            }

            // Handle the negative case
            if (value < 0) {
                value = -value;
                _mBuffer[_mBufferPos++] = '-';
            }

            // Get the 7 meaningful digits as a long
            int nbDecimals = 0;
            while (value < 1000000) {
                value *= 10;
                nbDecimals++;
            }
            long valueLong = (long) Math.Round(value);

            // Parse the number in reverse order
            int nbChars = 0;
            bool isLeadingZero = true;
            while (valueLong != 0 || nbDecimals >= 0) {
                // We stop removing leading 0 when non-0 or decimal digit
                if (valueLong % 10 != 0 || nbDecimals <= 0) {
                    isLeadingZero = false;
                }

                // Write the last digit (unless a leading zero)
                if (!isLeadingZero) {
                    _mBuffer[_mBufferPos + nbChars++] = (char) ('0' + valueLong % 10);
                }

                // Add the decimal point
                if (--nbDecimals == 0 && !isLeadingZero) {
                    _mBuffer[_mBufferPos + nbChars++] = '.';
                }
                valueLong /= 10;
            }
            _mBufferPos += nbChars;

            // Reverse the result
            for (int i = nbChars / 2 - 1; i >= 0; i--) {
                char c = _mBuffer[_mBufferPos - i - 1];
                _mBuffer[_mBufferPos - i - 1] = _mBuffer[_mBufferPos - nbChars + i];
                _mBuffer[_mBufferPos - nbChars + i] = c;
            }
            return this;
        }

        // Append methods, to build the string without allocation

        ///<summary>Reset the m_char array</summary>
        public FastString Clear() {
            _mBufferPos = 0;
            _mIsStringGenerated = false;
            return this;
        }

        public bool IsEmpty() {
            return _mIsStringGenerated ? _mStringGenerated == null : _mBufferPos == 0;
        }

        // Value controls methods: use a value to check if the string has to be regenerated.

        ///<summary>Return true if the valueControl has changed (and update it)</summary>
        public bool IsModified(int newControlValue) {
            bool changed = newControlValue != _mValueControlInt;
            if (changed) {
                _mValueControlInt = newControlValue;
            }
            return changed;
        }

        ///<summary>Return true if the valueControl has changed (and update it)</summary>
        public bool IsModified(object newControlValue) {
            bool changed = !newControlValue.Equals(_mValueControl);
            if (changed) {
                _mValueControl = newControlValue;
            }
            return changed;
        }

        ///<summary>Replace all occurences of a string by another one</summary>
        public FastString Replace(string oldStr, string newStr) {
            if (_mBufferPos == 0) {
                return this;
            }
            if (_mReplacement == null) {
                _mReplacement = new List<char>();
            }

            // Create the new string into m_replacement
            for (int i = 0; i < _mBufferPos; i++) {
                bool isToReplace = false;
                if (_mBuffer[i] == oldStr[0]) // If first character found, check for the rest of the string to replace
                {
                    int k = 1;
                    while (k < oldStr.Length && _mBuffer[i + k] == oldStr[k]) {
                        k++;
                    }
                    isToReplace = k >= oldStr.Length;
                }
                if (isToReplace) // Do the replacement
                {
                    i += oldStr.Length - 1;
                    if (newStr != null) {
                        for (int k = 0; k < newStr.Length; k++) {
                            _mReplacement.Add(newStr[k]);
                        }
                    }
                }
                else // No replacement, copy the old character
                {
                    _mReplacement.Add(_mBuffer[i]);
                }
            }

            // Copy back the new string into m_chars
            ReallocateIfn(_mReplacement.Count - _mBufferPos);
            for (int k = 0; k < _mReplacement.Count; k++) {
                _mBuffer[k] = _mReplacement[k];
            }
            _mBufferPos = _mReplacement.Count;
            _mReplacement.Clear();
            _mIsStringGenerated = false;
            return this;
        }

        // Set methods: 

        ///<summary>Set a string, no memorry allocation</summary>
        public void Set(string str) {
            // We fill the m_chars list to manage future appends, but we also directly set the final stringGenerated
            Clear();
            Append(str);
            _mStringGenerated = str;
            _mIsStringGenerated = true;
        }

        ///<summary>Caution, allocate some memory</summary>
        public void Set(object str) {
            Set(str.ToString());
        }

        ///<summary>Append several params: no memory allocation unless params are of object type</summary>
        public void Set<T1, T2>(T1 str1, T2 str2) {
            Clear();
            Append(str1);
            Append(str2);
        }

        public void Set<T1, T2, T3>(T1 str1, T2 str2, T3 str3) {
            Clear();
            Append(str1);
            Append(str2);
            Append(str3);
        }

        public void Set<T1, T2, T3, T4>(T1 str1, T2 str2, T3 str3, T4 str4) {
            Clear();
            Append(str1);
            Append(str2);
            Append(str3);
            Append(str4);
        }

        ///<summary>Allocate a little memory (20 byte)</summary>
        public void Set(params object[] str) {
            Clear();
            for (int i = 0; i < str.Length; i++) {
                Append(str[i]);
            }
        }

        ///<summary>Return the string</summary>
        public override string ToString() {
            if (!_mIsStringGenerated) // Regenerate the immutable string if needed
            {
                _mStringGenerated = new string(_mBuffer, 0, _mBufferPos);
                _mIsStringGenerated = true;
            }
            return _mStringGenerated;
        }
    }
}