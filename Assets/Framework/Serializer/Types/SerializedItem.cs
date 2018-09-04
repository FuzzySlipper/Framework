using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class SerializedItem : ISerializable {

        private int _level;
        private ItemModifier _prefix;
        private ItemModifier _suffix;
        private ItemTemplate _template;
        private string _otherData;

        public SerializedItem(SerializationInfo info, StreamingContext context) {
            _level = (int) info.GetValue("Level", typeof(int));
            _template = ItemDatabase.GetTemplate((string) info.GetValue("Template", typeof(string)));
            _prefix = ItemDatabase.GetMod((string) info.GetValue("Prefix", typeof(string)));
            _suffix = ItemDatabase.GetMod((string) info.GetValue("Suffix", typeof(string)));
            _otherData = (string) info.GetValue("OtherData", typeof(string));
        }

        public SerializedItem(Entity item) {
            _level = item.Get<EntityLevelComponent>().Level;
            //_template = equipment?.Template;
            //_prefix = equipment?.PrefixEquip;
            //_suffix = equipment?.SuffixEquip;
            //_otherData = item.GetOtherData();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Template", _template != null ? _template.Id : "", typeof(string));
            info.AddValue("Level", _level, typeof(int));
            info.AddValue("Prefix", _prefix != null ? _prefix.Id : "", typeof(string));
            info.AddValue("Suffix", _suffix != null ? _suffix.Id : "", typeof(string));
            info.AddValue("OtherData", _otherData, typeof(string));
        }

        public Entity GetItem() {
            if (_template == null) {
                return null;
            }
            var item = _template.New(_level, _prefix, _suffix);
            //item.RestoreOtherData(_otherData);
            return item;
        }
    }
}
