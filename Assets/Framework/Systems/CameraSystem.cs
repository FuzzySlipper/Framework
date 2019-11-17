using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [AutoRegister]
    public sealed class CameraSystem : SystemBase, IReceive<CollisionEvent>, IReceiveGlobal<CameraPositionForceEvent>,
        IReceiveGlobal<CameraRotationForceEvent>, IReceiveGlobal<CameraZoomForceEvent>, IReceive<EntityJumped>, IReceive<EntityLanded> {
        
        private static Vector3 _topDirection = new Vector3(-1, 0, 0);
        private static Vector3 _bottomDirection = new Vector3(1, 0, 0);
        private static Vector3 _rightDirection = new Vector3(0, 1, 0);
        private static Vector3 _leftDirection = new Vector3(0, -1, 0);
        
        public CameraSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(CameraShakeOnDamage), typeof(PlayerCameraComponent)
            }));
            BuildEventDictionary();
        }

        private GameOptions.CachedInt _shakeFrames = new GameOptions.CachedInt("ShakeFrames");
        private GameOptions.CachedInt _shakeStrength = new GameOptions.CachedInt("ShakeStrength");
        private GameOptions.CachedInt _pullFrames = new GameOptions.CachedInt("PullFrames");
        private GameOptions.CachedInt _pullStrength = new GameOptions.CachedInt("PullStrength");

        private Dictionary<string, SpringEventConfig> _eventDict = new Dictionary<string, SpringEventConfig>();
        private PlayerCameraComponent _singleton;

        public void Set(PlayerCameraComponent component) {
            _singleton = component;
            InitSprings();
        }

        private void InitSprings() {
            _singleton.FovSpring.Initialize(false);
            _singleton.FovSpring.Reset();
            _singleton.MoveSpring.Initialize(false);
            _singleton.MoveSpring.Reset();
            _singleton.RotationSpring.Initialize(true);
            _singleton.RotationSpring.Reset();
        }
        
        public void Handle(CollisionEvent arg) {
            var cameraShakeOnDamage = arg.Target.Get<CameraShakeOnDamage>();
            if (cameraShakeOnDamage == null || _singleton == null) {
                return;
            }
            AddRotationForce(-arg.HitNormal * cameraShakeOnDamage.IntensityMulti);
        }

        public void HandleGlobal(CameraPositionForceEvent arg) {
            AddForce(arg.Force, arg.Frames);
        }

        public void HandleGlobal(CameraRotationForceEvent arg) {
            AddRotationForce(arg.Force, arg.Frames);
        }

        public void HandleGlobal(CameraZoomForceEvent arg) {
            ZoomForce(arg.Force, arg.Frames);
        }

        public void ZoomForce(float force, int frames = 4) {
            _singleton.FovSpring.AddForce(new Vector3(0, 0, force), frames);
        }

        public void AddForce(Vector3 force, int frames = 4) {
            _singleton.MoveSpring.AddForce(force, frames);
        }

        public void AddRotationForce(Vector3 force, int frames = 4) {
            _singleton.RotationSpring.AddForce(force, frames);
        }
        
        public void PlaySpringAnimation(string animationEvent) {
            var config = GetConfig(animationEvent);
            if (config != null) {
                config.Trigger();
            }
        }

        public void Handle(EntityJumped arg) {
            GetConfig(AnimationEvents.PullTop).Trigger();
        }

        public void Handle(EntityLanded arg) {
            GetConfig(AnimationEvents.ShakeBottom).Trigger();
        }
        
        private void BuildEventDictionary() {
            _eventDict.Clear();
            _eventDict.Add(AnimationEvents.ShakeRightTop, new SpringEventConfig(AnimationEvents.ShakeRightTop, 
                (_topDirection + _rightDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullRightTop, new SpringEventConfig(AnimationEvents.PullRightTop,
                    (_topDirection + _rightDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeRightMiddle, new SpringEventConfig(AnimationEvents.ShakeRightMiddle, 
                (_rightDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullRightMiddle, new SpringEventConfig(AnimationEvents.PullRightMiddle,
                    (_rightDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeRightBottom, new SpringEventConfig(AnimationEvents.ShakeRightBottom, 
                (_bottomDirection + _rightDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullRightBottom, new SpringEventConfig(AnimationEvents.PullRightBottom,
                    (_bottomDirection + _rightDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeTop, new SpringEventConfig(AnimationEvents.ShakeTop, 
                (_topDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullTop, new SpringEventConfig(AnimationEvents.PullTop,
                    (_topDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeBottom, new SpringEventConfig(AnimationEvents.ShakeBottom, 
                (_bottomDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullBottom, new SpringEventConfig(AnimationEvents.PullBottom,
                    (_bottomDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeLeftTop, new SpringEventConfig(AnimationEvents.ShakeLeftTop, 
                (_topDirection + _leftDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullLeftTop, new SpringEventConfig(AnimationEvents.PullLeftTop,
                    (_topDirection + _leftDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeLeftMiddle, new SpringEventConfig(AnimationEvents.ShakeLeftMiddle, 
                (_leftDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullLeftMiddle, new SpringEventConfig(AnimationEvents.PullLeftMiddle,
                    (_leftDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeLeftBottom, new SpringEventConfig(AnimationEvents.ShakeLeftBottom, 
                (_bottomDirection + _leftDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullLeftBottom, new SpringEventConfig(AnimationEvents.PullLeftBottom,
                    (_bottomDirection + _leftDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));
        }

        private SpringEventConfig GetConfig(string animationEvent) {
            return _eventDict.TryGetValue(animationEvent, out var config) ? config : null;
        }

        [System.Serializable]
        public class SpringEventConfig {
            [ValueDropdown("SignalsList")] public string AnimationEvent;
            public Vector3 Force;
            public int Frames;
            public SpringType Type;

            public SpringEventConfig() {
            }

            public SpringEventConfig(string animationEvent, Vector3 force, int frames, SpringType type) {
                AnimationEvent = animationEvent;
                Force = force;
                Frames = frames;
                Type = type;
            }

            public enum SpringType {
                Position,
                Rotation,
                Fov
            }

            private ValueDropdownList<string> SignalsList() {
                return AnimationEvents.GetDropdownList();
            }

            public void Trigger() {
                switch (Type) {
                    case SpringType.Fov:
                        World.Get<CameraSystem>().ZoomForce(Force.AbsMax(), Frames);
                        break;
                    case SpringType.Rotation:
                        World.Get<CameraSystem>().AddRotationForce(Force, Frames);
                        break;
                    case SpringType.Position:
                        World.Get<CameraSystem>().AddForce(Force, Frames);
                        break;
                }
            }
        }

    }

    public struct CameraPositionForceEvent : IEntityMessage {
        public Vector3 Force { get; }
        public int Frames { get; }

        public CameraPositionForceEvent(Vector3 force, int frames = 4) {
            Force = force;
            Frames = frames;
        }
    }

    public struct CameraZoomForceEvent : IEntityMessage {
        public float Force { get; }
        public int Frames { get; }

        public CameraZoomForceEvent(float force, int frames = 4) {
            Force = force;
            Frames = frames;
        }
    }

    public struct CameraRotationForceEvent : IEntityMessage {
        public Vector3 Force { get; }
        public int Frames { get; }

        public CameraRotationForceEvent(Vector3 force, int frames = 4) {
            Force = force;
            Frames = frames;
        }
    }

    public struct EntityJumped : IEntityMessage {
        public Entity Entity { get; }

        public EntityJumped(Entity entity) {
            Entity = entity;
        }
    }

    public struct EntityLanded : IEntityMessage {
        public Entity Entity { get; }

        public EntityLanded(Entity entity) {
            Entity = entity;
        }
    }
}