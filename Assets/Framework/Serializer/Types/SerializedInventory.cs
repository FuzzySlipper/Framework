//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.Serialization;

//namespace PixelComrades {
//    [System.Serializable]
//    public class SerializedInventory : ISerializable {

//        [SerializeField] private SerializedItem[] _data;

//        public SerializedInventory(SerializationInfo info, StreamingContext context) {
//            _data = (SerializedItem[]) info.GetValue("Data", typeof(SerializedItem[]));
//        }

//        public SerializedInventory(ItemInventory inventory) {
//            _data = new SerializedItem[inventory.ActiveItems.Count];
//            for (int i = 0; i < inventory.ActiveItems.Count; i++) {
//                _data[i] = new SerializedItem(inventory.ActiveItems[i]);
//            }
//        }

//        public void FillInventory(ItemInventory inventory) {
//            if (inventory == null) {
//                return;
//            }
//            for (int i = 0; i < _data.Length; i++) {
//                var item = _data[i].GetItem();
//                if (item != null) {
//                    inventory.AddItem(item);
//                }
//            }
//        }

//        public void GetObjectData(SerializationInfo info, StreamingContext context) {
//            info.AddValue("Data", _data, typeof(SerializedItem[]));
//        }
//    }
//}
