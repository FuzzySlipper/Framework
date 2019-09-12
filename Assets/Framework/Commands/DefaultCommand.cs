using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class DefaultCommand : IComponent {
        public Command Default { get; }
        public Command Loaded;
        public Command Get { get { return Loaded ?? Default; } }

        public DefaultCommand(Command @default) {
            Default = @default;
        }

        public DefaultCommand(SerializationInfo info, StreamingContext context) {
            Default = info.GetValue(nameof(Default), Default);
            Loaded = info.GetValue(nameof(Loaded), Loaded);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Default), Default);
            info.AddValue(nameof(Loaded), Loaded);
        }
    }
}
