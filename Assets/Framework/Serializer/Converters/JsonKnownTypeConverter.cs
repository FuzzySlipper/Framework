using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    /// <summary>
    /// From: https://stackoverflow.com/questions/8030538/how-to-implement-custom-jsonconverter-in-json-net-to-deserialize-a-list-of-base/8031283#8031283
    /// Use KnownType Attribute to match a derived class based on the class given to the serializer
    /// Selected class will be the first class to match all properties in the json object.
    /// </summary>
    public sealed class JsonKnownTypeConverter : JsonConverter {
        public override bool CanConvert( Type objectType ) {
            return System.Attribute.GetCustomAttributes( objectType ).Any( v => v is KnownTypeAttribute );
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
            // Load JObject from stream
            JObject jObject = JObject.Load( reader );

            // Create target object based on JObject
            System.Attribute[ ] attrs = System.Attribute.GetCustomAttributes( objectType );  // Reflection. 

            // check known types for a match. 
            foreach( var attr in attrs.OfType<KnownTypeAttribute>( ) ) {
                object target = Activator.CreateInstance( attr.Type );

                JObject jTest;
                using( var writer = new StringWriter( ) ) {
                    using( var jsonWriter = new JsonTextWriter( writer ) ) {
                        serializer.Serialize( jsonWriter, target );
                        string json = writer.ToString( );
                        jTest = JObject.Parse( json );
                    }
                }

                var jO = this.GetKeys( jObject ).Select( k => k.Key ).ToList( );
                var jT = this.GetKeys( jTest ).Select( k => k.Key ).ToList( );

                if( jO.Count == jT.Count && jO.Intersect( jT ).Count( ) == jO.Count ) {
                    serializer.Populate( jObject.CreateReader( ), target );
                    return target;
                }
            }

            throw new SerializationException( string.Format( "Could not convert base class {0}", objectType ) );
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) {
            throw new NotImplementedException( );
        }

        private IEnumerable<KeyValuePair<string, JToken>> GetKeys( JObject obj ) {
            var list = new List<KeyValuePair<string, JToken>>( );
            foreach( var t in obj ) {
                list.Add( t );
            }
            return list;
        }
    }
}
