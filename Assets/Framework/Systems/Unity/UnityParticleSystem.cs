using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class UnityParticleSystem : SystemBase, IMainSystemUpdate, IReceive<CollisionEvent>, IReceive<DeathEvent>, 
        IReceive<EnvironmentCollisionEvent>, IReceive<PerformedCollisionEvent>, IReceive<ProjectileSpawned>, IReceive<ProjectileDespawned> {
        
        public UnityParticleSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(SpriteDissolveParticlesComponent), typeof(HitParticlesComponent), typeof(ParticleTrailComponent),
            }));
        }

        private List<ParticleTrailComponent> _trailComponents = new List<ParticleTrailComponent>();
        public List<ParticleTrailComponent> TrailComponents { get => _trailComponents; }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            for (int i = _trailComponents.Count - 1; i >= 0; i--) {
                var tc = _trailComponents[i];
                tc.LastTime -= dt;
                if (tc.LastTime < 0) {
                    UnityFxManager.main.EmitParticles(tc.GetEntity().GetPosition(), tc.Color, tc.Amount, tc.Gravity);
                    tc.LastTime = tc.Frequency;
                }
            }
        }

        public void Handle(CollisionEvent arg) {
            var dissolve = arg.Target.Get<SpriteDissolveParticlesComponent>();
            if (dissolve != null) {
                dissolve.LastCollision = arg;
            }
            var hitParticles = arg.Target.Get<HitParticlesComponent>();
            if (hitParticles != null) {
                UnityFxManager.main.EmitHitParticles(arg.HitPoint, arg.HitNormal, hitParticles.Color);
            }

        }

        public void Handle(DeathEvent arg) {
            var dissolve = arg.Target.Get<SpriteDissolveParticlesComponent>();
            if (dissolve != null) {
                UnityFxManager.StartFx(arg.Target, dissolve.LastCollision, dissolve.SpriteRenderer, arg.OverKill);
            }
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            var hitParticles = arg.EntityHit.Get<HitParticlesComponent>();
            if (hitParticles != null) {
                UnityFxManager.main.EmitHitParticles(arg.HitPoint, arg.HitNormal, hitParticles.Color);
                UnityFxManager.main.EmitDecalParticles(arg.HitPoint, arg.HitNormal, hitParticles.Color);
            }
            
        }

        public void Handle(PerformedCollisionEvent arg) {
            var hitParticles = arg.Origin.Get<HitParticlesComponent>();
            if (hitParticles != null) {
                UnityFxManager.main.EmitHitParticles(arg.HitPoint, arg.HitNormal, hitParticles.Color);
            }
        }

        public void Handle(ProjectileSpawned arg) {
            var particleTrail = arg.Entity.Get<ParticleTrailComponent>();
            if (particleTrail != null) {
                TrailComponents.Add(particleTrail);
            }
        }

        public void Handle(ProjectileDespawned arg) {
            var particleTrail = arg.Entity.Get<ParticleTrailComponent>();
            if (particleTrail != null) {
                TrailComponents.Remove(particleTrail);
                particleTrail.LastTime = -1;
            }
        }
    }
}
