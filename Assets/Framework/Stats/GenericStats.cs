using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GenericStats : ComponentContainer<BaseStat> {

        public GenericStats(BaseStat[] values) : base(values) {}

        public BaseStat Get(string id) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Id == id) {
                    return this[i];
                }
            }
            return null;
        }

        public float GetValue(string id) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Id == id) {
                    return this[i].Value;
                }
            }
            return 0;
        }

        public bool GetValue(string id, out float value) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Id == id) {
                    value = this[i].Value;
                    return true;
                }
            }
            value = 0f;
            return false;
        }

        public GenericStats Clone() {
            return new GenericStats(List.ToArray());
        }

        public void ClearMods() {
            for (int i = 0; i < Count; i++) {
                this[i].ClearMods();
            }
        }

        public void Reset() {
            for (int i = 0; i < Count; i++) {
                this[i].Reset();
            }
        }
    }
}
