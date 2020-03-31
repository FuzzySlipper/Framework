using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PixelComrades {

    public class Enums {
        
        private Dictionary<string, FakeEnum> _enums = new Dictionary<string, FakeEnum>();

        public FakeEnum this[string index] { get { return _enums.TryGetValue(index, out var fakeEnum) ? fakeEnum : null; } }

        public void Add(string name, FakeEnum fakeEnum) {
            _enums.AddOrUpdate(name, fakeEnum);
        }

        public bool TryGetEnumIndex(string fullName, out int index) {
            return GetEnumIndex(fullName, out index) != null;
        }

        public FakeEnum GetEnumIndex(string fullName, out int index) {
            FakeEnum fakeEnum = GetFakeEnum(fullName);
            if (fakeEnum == null) {
                index = -1;
                return null;
            }
            var splitName = fullName.Split('.');
            fakeEnum.TryParse(splitName.Length > 1 ? splitName[1] : fullName, out index);
            return fakeEnum;
        }

        public FakeEnum GetFakeEnum(string fullName) {
            var splitName = fullName.Split('.');
            if (splitName.Length < 2) {
                return _enums.TryGetValue(fullName, out var fullEnum) ? fullEnum : null;
            }
            return !_enums.TryGetValue(splitName[0], out var splitEnum) ? null : splitEnum;
        }
    }
}
