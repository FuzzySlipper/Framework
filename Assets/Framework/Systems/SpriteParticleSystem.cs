using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteParticleSystem : SystemBase, IMainSystemUpdate {
        //private const BillboardMode Billboard = BillboardMode.NoYAxis;
        //private const bool Reversed = true;

        private List<ParticlePlayer> _current = new List<ParticlePlayer>();
        private Transform _poolPivot;
        private Stack<ParticlePlayer> _pool = new Stack<ParticlePlayer>();

        private Transform Pivot {
            get {
                if (_poolPivot == null) {
                    _poolPivot = Game.GetMainChild("ParticlePool");
                }
                return _poolPivot;
            }
        }

        public static ParticlePlayer PlayParticle(SpriteParticle particle, Vector3 pos, Quaternion rot) {
            var ps = World.Get<SpriteParticleSystem>();
            var player = ps.GetPlayer();
            player.Tr.position = pos;
            player.Tr.rotation = rot;
            player.PlayAnimation(particle);
            ps._current.Add(player);
            return player;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_current.Count == 0) {
                return;
            }
            float time = TimeManager.Time;
            for (int i = _current.Count - 1; i >= 0; i--) {
                var value = _current[i];
                //Billboard.Apply(value.Renderer.transform, Reversed);
                if (value.NextUpdateTime > time) {
                    continue;
                }
                value.CurrentFrameIndex+=1;
                if (value.CurrentFrameIndex >= value.Animation.Frames.Length) {
                    StorePlayer(value);
                    _current.RemoveAt(i);
                    continue;
                }
                var currentFrame = value.CurrentFrame;
                value.NextUpdateTime = value.Animation.FrameTime * currentFrame.Length + time;
                value.Renderer.sprite = value.Animation.GetSprite(value.CurrentFrameIndex);
            }
        }

        private void StorePlayer(ParticlePlayer player) {
            player.Renderer.enabled = false;
            player.Tr.SetParent(Pivot);
            _pool.Push(player);
        }

        private ParticlePlayer GetPlayer() {
            ParticlePlayer player = null;
            if (_pool.Count > 0) {
                player = _pool.Pop();
                player.Renderer.enabled = true;
            }
            else {
                var newPlayer = UnityEngine.Object.Instantiate(SimpleGameSpecificDb.Main.ParticleHolder);
                player = new ParticlePlayer(newPlayer.GetComponent<SpriteRenderer>());
            }
            player.Tr.SetParent(null);
            return player;
        }


        public class ParticlePlayer {
            public Transform Tr { get; }
            public SpriteRenderer Renderer;
            public SpriteAnimation Animation;
            public float NextUpdateTime;
            public short CurrentFrameIndex;

            private MaterialPropertyBlock _matBlock;

            public ParticlePlayer(SpriteRenderer renderer) {
                Renderer = renderer;
                Tr = renderer.transform;
                _matBlock = new MaterialPropertyBlock();
                Renderer.GetPropertyBlock(_matBlock);
            }

            public AnimationFrame CurrentFrame { get { return Animation.GetFrame(CurrentFrameIndex); } }
            public bool Active { get { return CurrentFrameIndex >= 0; } }

            public void PlayAnimation(SpriteParticle particle) {
                Animation = particle.Animation;
                CurrentFrameIndex = 0;
                NextUpdateTime = Animation.FrameTime * CurrentFrame.Length + TimeManager.Time;
                _matBlock.SetColor("_TintColor", Color.white * particle.Glow);
                Renderer.SetPropertyBlock(_matBlock);
                Renderer.color = particle.Color;
                Renderer.sprite = Animation.GetSprite(CurrentFrameIndex);
                Renderer.gameObject.name = particle.Animation.name;
            }
        }
    }
}
