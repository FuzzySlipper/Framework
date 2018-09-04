using System;

namespace PixelComrades {
    [Serializable]
    public class DataTemplate : IComponent {
        public Template Template;

        public void Dispose() {
            Template = null;
        }

        public T Get<T>() where T : class {
            return Template.Get<T>();
        }

        public int Owner { get; set; }
    }
}