using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpriteDissolveParticlesComponent : IComponent {

        public float Length { get; }
        public SpriteDissolveParticlesComponent(float length) {
            Length = length;
        }

        public SpriteDissolveParticlesComponent(SerializationInfo info, StreamingContext context) {
            Length = info.GetValue(nameof(Length), Length);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Length), Length);
        }
    }

    [System.Serializable]
	public sealed class HitParticlesComponent : IComponent {

        public Color Color { get; }

        public HitParticlesComponent(Color color) {
            Color = color;
        }

        public HitParticlesComponent(SerializationInfo info, StreamingContext context) {
            Color = info.GetValue(nameof(Color), Color);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Color), Color);
        }
    }

    [System.Serializable]
	public sealed class ParticleTrailComponent : IComponent {
        public int Amount { get; }
        public float Frequency { get; }
        public Color Color { get; }
        public ParticleGravityStatus Gravity { get; }

        public float LastTime;

        public ParticleTrailComponent(int amount, float frequency, Color color, ParticleGravityStatus gravity) {
            Amount = amount;
            Frequency = frequency;
            Color = color;
            Gravity = gravity;
            LastTime = -1;
        }

        public ParticleTrailComponent(SerializationInfo info, StreamingContext context) {
            Amount = info.GetValue(nameof(Amount), Amount);
            Frequency = info.GetValue(nameof(Frequency), Frequency);
            Color = info.GetValue(nameof(Color), Color);
            Gravity = info.GetValue(nameof(Gravity), Gravity);
            LastTime = info.GetValue(nameof(LastTime), LastTime);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Amount), Amount);
            info.AddValue(nameof(Frequency), Frequency);
            info.AddValue(nameof(Color), Color);
            info.AddValue(nameof(Gravity), Gravity);
            info.AddValue(nameof(LastTime), LastTime);
        }
    }

}
