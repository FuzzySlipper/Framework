using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class SpriteSystem : SystemBase, IMainSystemUpdate, IReceive<TakeDamageEvent>, IReceive<TagChangeEvent>,
        IReceive<ConfusionEvent> {
        
        private const float Duration = 0.5f;
        private static GameOptions.CachedColor _stunColor = new GameOptions.CachedColor("Stunned");
        private static GameOptions.CachedColor _confusedColor = new GameOptions.CachedColor("Confused");
        private static GameOptions.CachedColor _frozenColor = new GameOptions.CachedColor("Frozen");

        private GenericPool<TweenFloat> _floatPool = new GenericPool<TweenFloat>(0, null, SetupTween);
        private BufferedList<SpriteColorWatch> _colorList = new BufferedList<SpriteColorWatch>();
        private ManagedArray<SpriteColorWatch>.RefDelegate _del;
        private struct SpriteColorWatch {
            public SpriteColorComponent ColorComponent;
            public TweenFloat Tween;
            public int Stage;
            public Vector3 Scale;

            public SpriteColorWatch(SpriteColorComponent colorComponent, TweenFloat tween, Vector3 scale) {
                ColorComponent = colorComponent;
                Tween = tween;
                Scale = scale;
                Stage = 0;
            }

            public SpriteColorWatch(SpriteColorComponent colorComponent, TweenFloat tween, Vector3 scale, int stage) {
                ColorComponent = colorComponent;
                Tween = tween;
                Scale = scale;
                Stage = stage;
            }
        } 
        
        public SpriteSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(SpriteColorComponent)
            }));
            _del = UpdateSprite;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _colorList.Run(_del);
        }

        private void UpdateSprite(ref SpriteColorWatch colorStage) {
            if (colorStage.ColorComponent == null) {
                _colorList.Remove(colorStage);
                return;
            }
            colorStage.ColorComponent.Renderer.transform.localScale = new Vector3(
                colorStage.Scale.x * colorStage.Tween.Get(),
                colorStage.Scale.y, colorStage.Scale.z);
            if (colorStage.Stage == 1) {
                colorStage.ColorComponent.UpdateCurrentColor(
                    Color.Lerp(
                        Color.red, colorStage.ColorComponent.BaseColor,
                        colorStage.Tween.Get()));
            }
            if (colorStage.Tween.Active) {
                return;
            }
            if (colorStage.Stage == 0) {
                colorStage.Tween.Restart(colorStage.ColorComponent.DmgMaxScale, 1);
                colorStage.Stage = 1;
            }
            else {
                colorStage.ColorComponent.AnimatingColor = false;
                colorStage.ColorComponent.UpdateBaseColor();
                _colorList.Remove(colorStage);
            }
        }

        private static void SetupTween(TweenFloat tween) {
            tween.EasingConfig = EasingTypes.BounceInOut;
            tween.UnScaled = false;
            tween.Length = Duration;
        }

        public void Handle(TakeDamageEvent arg) {
            if (arg.Amount <= 0) {
                return;
            }
            var colorComponent = arg.Target.Get<SpriteColorComponent>();
            if (colorComponent == null) {
                return;
            }
            if (!colorComponent.AnimatingColor && colorComponent.Renderer != null) {
                StartColorDamageTween(colorComponent);
            }
            else if (colorComponent.AnimatingColor && colorComponent.Renderer != null) {
                for (int i = 0; i < _colorList.Count; i++) {
                    if (_colorList[i].ColorComponent == colorComponent) {
                        _colorList[i].Tween.Restart(colorComponent.DmgMaxScale, 1);
                    }
                }
            }
        }
        
        private void StartColorDamageTween(SpriteColorComponent colorComponent) {
            colorComponent.AnimatingColor = true;
            colorComponent.UpdateCurrentColor(Color.red);
            var tween = _floatPool.New();
            tween.Restart(1, colorComponent.DmgMaxScale);
            _colorList.Add(new SpriteColorWatch(colorComponent, tween, colorComponent.Renderer.transform.localScale));
        }

        public void Handle(TagChangeEvent arg) {
            CheckTagColor(arg.Entity, arg.Entity.Get<SpriteColorComponent>(), arg.Active ? TagTypes.Slow : TagTypes.None);
        }

        public void Handle(ConfusionEvent arg) {
            CheckTagColor(arg.Entity, arg.Entity.Get<SpriteColorComponent>(), arg.Active ? TagTypes.Confuse : TagTypes.None);
        }

        private void CheckTagColor(Entity entity, SpriteColorComponent colorComponent, TagTypes tagType) {
            if (colorComponent == null) {
                return;
            }
            if (entity.Tags.IsStunned || tagType == TagTypes.Stun) {
                colorComponent.BaseColor = _stunColor;
            }
            else if (entity.Tags.IsConfused || tagType == TagTypes.Confuse) {
                colorComponent.BaseColor = _confusedColor;
            }
            else if (entity.Tags.IsSlowed || tagType == TagTypes.Slow) {
                colorComponent.BaseColor = _frozenColor;
            }
            else {
                colorComponent.BaseColor = Color.white;
            }
            if (colorComponent.AnimatingColor) {
                return;
            }
            colorComponent.UpdateBaseColor();
        }

        enum TagTypes {
            None,
            Stun,
            Slow,
            Confuse
        }
    }
}
