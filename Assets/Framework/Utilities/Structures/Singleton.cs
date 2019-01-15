using System.Diagnostics.CodeAnalysis;

namespace PixelComrades {
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public class Singleton<T> where T : class, new() {
        private static T _instance;
        // ReSharper disable once StaticMemberInGenericType
        private static object _lock = new object();
        public static T Instance {
            get {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = new T();
                    }
                    return _instance;
                }
            }
        }
    }

}