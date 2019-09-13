using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class AudioClipSetData : IComponent, ISignalReceiver {

        public AudioClipSet Set { get; }

        public AudioClipSetData(AudioClipSet set) {
            Set = set;
        }

        public void Handle(int signal) {
            Set.PlayAudio(signal, this.GetEntity().GetPosition());
        }

        public AudioClipSetData(SerializationInfo info, StreamingContext context) {
            Set = ItemPool.LoadAsset<AudioClipSet>(info.GetValue(nameof(Set), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Set), ItemPool.GetAssetLocation(Set));
        }
    }
}
