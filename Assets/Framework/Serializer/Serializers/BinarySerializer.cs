//using System;
//using System.IO;
//using System.Runtime.Remoting.Messaging;
//using System.Runtime.Serialization.Formatters.Binary;

//namespace PixelComrades {
//    public class BinarySerializer : ISerializer {

//        public HeaderHandler HeaderHandler;
//        public Header[] Headers = new Header[0];

//        public string Serialize(object obj) {
//            using (MemoryStream stream = new MemoryStream()) {
//                new BinaryFormatter().Serialize(stream, obj, Headers);
//                return Convert.ToBase64String(stream.ToArray());
//            }
//        }

//        public T Deserialize<T>(string str) {
//            byte[] bytes = Convert.FromBase64String(str);
//            using (MemoryStream stream = new MemoryStream(bytes)) {
//                return (T) new BinaryFormatter().Deserialize(stream, HeaderHandler);
//            }
//        }
//    }
//}