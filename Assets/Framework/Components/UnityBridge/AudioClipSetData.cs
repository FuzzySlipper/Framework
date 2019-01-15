using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AudioClipSetData : IComponent, ISignalReceiver {

        private int _owner = -1;
        private CachedComponent<PositionComponent> _cachedPosition;
        public int Owner {
            get {
                return _owner;
            }
            set {
                _owner = value;
                if (value >= 0) {
                    var entity = EntityController.GetEntity(value);
                    if (entity != null) {
                        entity.AddObserver(this);
                        _cachedPosition = new CachedComponent<PositionComponent>(entity);
                    }
                }
            }
        }

        public AudioClipSet Set { get; }

        public AudioClipSetData(AudioClipSet set) {
            Set = set;
        }

        public void Handle(int signal) {
            Set.PlayAudio(signal, _cachedPosition.c?.Position ?? Vector3.zero);
        }
    }
}
