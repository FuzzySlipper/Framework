﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    public class JsonMatrixConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(Matrix4x4).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var o = JObject.Load(reader);

            var matrix = Matrix4x4.identity;

            matrix[0, 0] = (float)o.GetValue("00");
            matrix[0, 1] = (float)o.GetValue("01");
            matrix[0, 2] = (float)o.GetValue("02");
            matrix[0, 3] = (float)o.GetValue("03");

            matrix[1, 0] = (float)o.GetValue("10");
            matrix[1, 1] = (float)o.GetValue("11");
            matrix[1, 2] = (float)o.GetValue("12");
            matrix[1, 3] = (float)o.GetValue("13");

            matrix[2, 0] = (float)o.GetValue("20");
            matrix[2, 1] = (float)o.GetValue("21");
            matrix[2, 2] = (float)o.GetValue("22");
            matrix[2, 3] = (float)o.GetValue("23");

            matrix[3, 0] = (float)o.GetValue("30");
            matrix[3, 1] = (float)o.GetValue("31");
            matrix[3, 2] = (float)o.GetValue("32");
            matrix[3, 3] = (float)o.GetValue("33");

            return matrix;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var o = new JObject();

            o.Add("$type", value.GetType().AssemblyQualifiedName);

            var matrix = (Matrix4x4)value;

            o.Add("00", matrix[0, 0]);
            o.Add("01", matrix[0, 1]);
            o.Add("02", matrix[0, 2]);
            o.Add("03", matrix[0, 3]);

            o.Add("10", matrix[1, 0]);
            o.Add("11", matrix[1, 1]);
            o.Add("12", matrix[1, 2]);
            o.Add("13", matrix[1, 3]);

            o.Add("20", matrix[2, 0]);
            o.Add("21", matrix[2, 1]);
            o.Add("22", matrix[2, 2]);
            o.Add("23", matrix[2, 3]);

            o.Add("30", matrix[3, 0]);
            o.Add("31", matrix[3, 1]);
            o.Add("32", matrix[3, 2]);
            o.Add("33", matrix[3, 3]);

            o.WriteTo(writer, serializer.Converters.ToArray());
        }
    }
}
