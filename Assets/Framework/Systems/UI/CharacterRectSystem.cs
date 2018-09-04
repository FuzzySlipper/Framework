using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CharacterRectSystem : SystemBase {
        private Dictionary<int, ICharacterRect> _entityRect = new Dictionary<int, ICharacterRect>();
        public Dictionary<int, ICharacterRect> EntityRect { get => _entityRect; }

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
