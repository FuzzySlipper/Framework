using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class CharacterRectSystem : SystemBase<CharacterRectSystem> {
        private Dictionary<int, ICharacterRect> _entityRect = new Dictionary<int, ICharacterRect>();
        public Dictionary<int, ICharacterRect> EntityRect { get => _entityRect; }

        public void Remove(int index) {
            _entityRect.Remove(index);
        }

        public void Add(int index, ICharacterRect rect) {
            if (_entityRect.ContainsKey(index)) {
                _entityRect[index] = rect;
            }
            else {
                _entityRect.Add(index, rect);
            }
        }

        public ICharacterRect GetEntityRect(int entity) {
            return _entityRect.TryGetValue(entity, out var rect) ? rect : null;
        }

        public T GetEntityRect<T>(int entity) where T : class, ICharacterRect {
            return _entityRect.TryGetValue(entity, out var rect) ? (T) rect : null;
        }
    }

    public interface ICharacterRect {
        RectTransform RectTr { get; }
    }
}
