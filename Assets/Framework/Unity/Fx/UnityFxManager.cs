using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public enum ParticleGravityStatus : byte {
        Default,
        None,
        Anti
    }
    public class UnityFxManager : MonoSingleton<UnityFxManager> {

        //private struct Decal {
        //    //public Matrix4x4 Matrix;
        //    public Vector3 Position;
        //    public Quaternion Rotation;
        //    public MaterialPropertyBlock Block;
        //}

        //[SerializeField] private GameObject _particlePrefab = null;
        //[SerializeField] private int _particleCount = 5000;
        //[SerializeField] private float _extraForce = 3f;
        [SerializeField] private float _maxDamage = 1.5f;
        //[SerializeField] private float _damageMulti = 0.25f;
        [SerializeField] private float _minDamage = 0.3f;
        [SerializeField] private float _maxFinish = 5f;
        [SerializeField] private ParticleSystem _gravityParticles = null;
        [SerializeField] private ParticleSystem _noGravityParticles = null;
        [SerializeField] private ParticleSystem _antiGravityParticles = null;
        //[SerializeField] private ParticleSystem _decalParticles = null;
        [SerializeField] private float _distance = 0.05f;
        [SerializeField] private float _maxVelocityShift = 0.2f;
        [SerializeField] private float _velocityPower = 3f;
        [SerializeField] private int _emitCnt = 25;
        [SerializeField] private float _hitSize = 0.01f;
        [SerializeField] private float _otherSize = 0.1f;
        [SerializeField] private float _hOffset = 0.05f;
        [SerializeField] private float _cOffset = 0.25f;
        [SerializeField] private int _maxDecals = 100;
        //[SerializeField] private int _decalSize = 1;
        //[SerializeField] private Material _decalMaterial = null;
        [SerializeField] private GameObject _decalPrefab = null;
        [SerializeField] private NormalizedFloatRange _decalRange = new NormalizedFloatRange(0.1f, 0.4f);
        [SerializeField] private NormalizedFloatRange _decalBrightness = new NormalizedFloatRange(0.05f, 0.3f);

        private ParticleSystem.Particle[] _particles;
        private MaterialPropertyBlock _block;
        //private List<Decal> _decals = new List<Decal>();
        private List<Decal> _decals = new List<Decal>();
        private int _decalIndex = 0;
        //private Mesh _decalMesh;
        
        void Awake() {
            _block = new MaterialPropertyBlock();
            //_decalMesh = ProceduralMeshUtility.BuildQuadMesh(_decalSize, _decalSize);
        }

//        void Update() {
//            for (int i = 0; i < _decals.Count; i++) {
//                //Graphics.DrawMesh(_decalMesh, _decals[i].Matrix, _decalMaterial, 0, PlayerCamera.Cam, 0, _decals[i].Block);
//                Graphics.DrawMesh(_decalMesh, _decals[i].Position, _decals[i].Rotation, _decalMaterial, 0, PlayerCamera.Cam, 0, _decals[i].Block);
//            }
//        }
        
//#if UNITY_EDITOR

//        private void OnDrawGizmosSelected() {
//            for (int i = 0; i < _decals.Count; i++) {
//                //Gizmos.matrix = _decals[i].Matrix;
//                DebugExtension.DrawArrow(_decals[i].Position, _decals[i].Rotation.eulerAngles, Color.white);
//            }
//        }

//#endif

        [Button]
        private void TestColor() {
            Debug.DrawRay(transform.position + Random.insideUnitSphere, transform.forward *5, Randomize(Color.red), 5);
        }

        [Button]
        private void TestHit() {
            EmitHitParticles(transform.position + Random.insideUnitSphere, transform.forward, Color.red);
        }

        private Color Randomize(Color color) {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            return Color.HSVToRGB(
                h + Random.Range(-_hOffset, _hOffset),
                Mathf.Clamp(s + Random.Range(-_cOffset, _cOffset), 0.25f, 1f),
                Mathf.Clamp(v + Random.Range(-_cOffset, _cOffset), 0.1f, 1f), true);
        }

        //public void EmitDecalParticles(Vector3 position, Vector3 normal, Color color) {
        //    var startSize = _decalRange.Get(Game.Random);
        //    var decalBlock = new MaterialPropertyBlock();
        //    decalBlock.SetVector("_Color", Randomize(color));
        //    var decal = new Decal {
        //        Block =  decalBlock,
        //        Position = position,
        //        Rotation = Quaternion.LookRotation(normal)
        //        //Matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(normal), new Vector3(startSize, startSize, startSize))
        //    };
        //    if (_decals.Count < _maxDecals) {
        //        _decals.Add(decal);
        //    }
        //    else {
        //        _decals[_decalIndex] = decal;
        //        _decalIndex++;
        //        if (_decalIndex >= _decals.Count) {
        //            _decalIndex = 0;
        //        }
        //    }
        //}
        public void EmitDecalParticles(Vector3 position, Vector3 normal, Color color) {
            Decal decal;
            if (_decals.Count < _maxDecals) {
                decal = Instantiate(_decalPrefab).GetComponent<Decal>();
                decal.transform.SetParent(transform);
                _decals.Add(decal);
            }
            else {
                decal = _decals[_decalIndex];
                _decalIndex++;
                if (_decalIndex >= _decals.Count) {
                    _decalIndex = 0;
                }
            }
            decal.transform.position = position;
            decal.transform.up = normal;
            decal.transform.Rotate(Vector3.up, Random.Range(0, 360), Space.Self);
            var decalSize = _decalRange.Get(Game.Random);
            decal.transform.localScale = new Vector3(decalSize, decal.transform.localScale.y, decalSize);
            Color.RGBToHSV(color, out var h, out var s, out var v);
            var finalColor = Color.HSVToRGB(
                h + Random.Range(-_hOffset, _hOffset),
                Mathf.Clamp(s + Random.Range(-_cOffset, _cOffset), 0.25f, 1f),
                _decalBrightness.Get(Game.Random), true);
            decal.Color = finalColor;
        }

        public void EmitHitParticles(Vector3 position, Vector3 normal, Color color) {
            for (int i = 0; i < _emitCnt; i++) {
                var emitParams = new ParticleSystem.EmitParams();
                emitParams.position = position + UnityEngine.Random.insideUnitSphere * _distance;
                emitParams.velocity = (normal + new Vector3(Random.Range(-_maxVelocityShift, _maxVelocityShift), Random.Range(-_maxVelocityShift, _maxVelocityShift), Random.Range(-_maxVelocityShift, _maxVelocityShift))) * _velocityPower;
                emitParams.startColor = Randomize(color);
                emitParams.startSize = _hitSize;
                _gravityParticles.Emit(emitParams, 1);
            }
        }

        public void EmitParticles(Vector3 position, Color color, int amount, ParticleGravityStatus status) {
            ParticleSystem ps;
            switch (status) {
                default:
                case ParticleGravityStatus.Default:
                    ps = _gravityParticles;
                    break;
                case ParticleGravityStatus.Anti:
                    ps = _antiGravityParticles;
                    break;
                case ParticleGravityStatus.None:
                    ps = _noGravityParticles;
                    break;
                    
            }
            EmitParticles(position, color, amount, ps);
        }

        private void EmitParticles(Vector3 position, Color color, int amount, ParticleSystem ps) {
            for (int i = 0; i < amount; i++) {
                var emitParams = new ParticleSystem.EmitParams();
                emitParams.position = position;
                emitParams.startColor = color;
                emitParams.startSize = _otherSize;
                emitParams.position = position + UnityEngine.Random.insideUnitSphere * _distance;
                ps.Emit(emitParams, 1);
            }
        }

        public static void StartFx(Entity entity, CollisionEvent collisionEvent, SpriteRenderer sprite, float amt) {
            TimeManager.StartUnscaled(main.DissolveFx(entity, collisionEvent, sprite, amt));
        }

        private IEnumerator DissolveFx(Entity owner, CollisionEvent collisionEvent, SpriteRenderer sprite, float amt) {
            var power = Mathf.Clamp(amt, _minDamage, _maxDamage);
            var tr = owner.Get<TransformComponent>();
            sprite.GetPropertyBlock(_block);
            var hitPnt = collisionEvent.HitPoint;
            var localPosition = tr != null ? tr.InverseTransformPoint(hitPnt) : hitPnt;
            //DebugExtension.DebugWireSphere(hitPnt, power, 2.5f);
            _block.SetVector("_DissolveMaskPosition", hitPnt);
            _block.SetFloat("_DissolveMaskRadius", power);
            sprite.SetPropertyBlock(_block);
            var start = TimeManager.TimeUnscaled;
            bool started = false;
            if (owner.IsDestroyed()) {
                yield break;
            }
            var modelComponent = owner.Get<RenderingComponent>();
            var animator = owner.Get<AnimatorComponent>()?.Value;
            if (animator == null) {
                owner.Destroy();
                yield break;
            }
            var startFx = 0f;
            var fxTime = 0f;
            owner.Tags.Remove(EntityTags.CanUnityCollide);
            if (animator.CurrentAnimation != AnimationIds.Death) {
                var loopLimiter = new WhileLoopLimiter(500);
                int lastTried = 0;
                while (loopLimiter.Advance()) {
                    if (animator.CurrentAnimation == AnimationIds.Death) {
                        break;
                    } 
                    if (lastTried > 5) {
                        animator.PlayAnimation(AnimationIds.Death, false, null);
                        lastTried = 0;
                    }
                    lastTried++;
                    yield return null;
                }
                if (loopLimiter.HitLimit) {
                    Debug.LogFormat("{0} could not start Death animation", owner.Name);
                    owner.Destroy();
                    yield break;
                }
            }
            var timeOut = animator.CurrentAnimationLength * 2.5f;
            while (TimeManager.TimeUnscaled < start + timeOut) {
                var worldPnt = tr != null ? tr.TransformPoint(localPosition) : hitPnt;
                //DebugExtension.DebugWireSphere(worldPnt, power, 0.5f);
                sprite.GetPropertyBlock(_block);
                _block.SetVector("_DissolveMaskPosition", worldPnt);
                if (!started) {
                    if (animator.IsAnimationEventComplete("Death")) {
                        started = true;
                        startFx = TimeManager.TimeUnscaled;
                        fxTime = animator.CurrentAnimationRemaining * 1.5f;
                    }
                    else {
                        sprite.SetPropertyBlock(_block);
                        yield return null;
                        continue;
                    }
                }
                var percent = (TimeManager.TimeUnscaled - startFx) / fxTime;
                _block.SetFloat("_DissolveMaskRadius", Mathf.Lerp(power, _maxFinish, percent));
                sprite.SetPropertyBlock(_block);
                yield return null;
            }
            _block.SetFloat("_DissolveMaskRadius", 0);
            sprite.SetPropertyBlock(_block);
            modelComponent.SetVisible(RenderingMode.None);
            owner.Destroy();
        }

        //private IEnumerator ExplodeFx(Entity entity, CollisionEvent collisionEvent, SpriteRenderer sprite) {
        //    var holder = GetParticles();
        //    holder.Tr.position = entity.Tr.position;
        //    holder.Tr.rotation = entity.Tr.rotation;
        //    var shape = holder.Particles.shape;
        //    shape.sprite = sprite.sprite;
        //    shape.texture = sprite.sprite.texture;
        //    holder.ForceTr.position = collisionEvent.HitPoint;
        //    holder.ForceTr.rotation = Quaternion.identity;

        //    var force = -collisionEvent.HitNormal;
        //    DebugExtension.DebugArrow(collisionEvent.HitPoint, force, Color.red, 2f);

        //    var dirX = holder.Force.directionX;
        //    dirX.constant = force.x;
        //    holder.Force.directionX = dirX;
        //    var dirY = holder.Force.directionY;
        //    dirY.constant = force.y;
        //    holder.Force.directionY = dirY;
        //    var dirZ = holder.Force.directionZ;
        //    dirZ.constant = force.z;
        //    holder.Force.directionZ = dirZ;

        //    var endTime = TimeManager.Time +  holder.Particles.main.duration;
        //    holder.Particles.Emit(_particleCount);
        //    //var controller = entity.Get<KinematicControllerComponent>();
        //    //if (controller != null) {
        //        //var force = controller.Controller.Motor.Velocity + controller.InternalVelocityAdd;
        //        if (_particles == null || _particles.Length != _particleCount) {
        //            _particles = new ParticleSystem.Particle[_particleCount];
        //        }
        //        holder.Particles.GetParticles(_particles);
        //        for (int i = 0; i < _particles.Length; i++) {
        //            _particles[i].velocity = force * _extraForce;
        //        }
        //        holder.Particles.SetParticles(_particles);
        //    //}
        //    sprite.enabled = false;
        //    while (TimeManager.Time < endTime) {
        //        yield return null;
        //    }
        //    Store(holder);
        //    entity.Destroy();
        //}
        //private static Queue<ParticleHolder> _renderers = new Queue<ParticleHolder>(2);
        //private ParticleHolder GetParticles() {
        //    ParticleHolder particle;
        //    if (_renderers.Count > 0) {
        //        particle = _renderers.Dequeue();
        //    }
        //    else {
        //        particle = new ParticleHolder(_particlePrefab);
        //    }
        //    if (particle.Tr == null) {
        //        particle = new ParticleHolder(_particlePrefab);
        //    }
        //    particle.Tr.SetParent(null);
        //    particle.Tr.gameObject.SetActive(true);
        //    return particle;
        //}

        //private void Store(ParticleHolder particle) {
        //    particle.Tr.SetParent(transform);
        //    particle.Tr.gameObject.SetActive(false);
        //    _renderers.Enqueue(particle);
        //}

        //private class ParticleHolder {
        //    public ParticleSystemForceField Force { get; }
        //    public ParticleSystem Particles { get; }
        //    public Transform Tr { get; }
        //    public Transform ForceTr { get; }

        //    public ParticleHolder(GameObject particlePrefab) {
        //        var go = Instantiate(particlePrefab);
        //        Particles = go.GetComponent<ParticleSystem>();
        //        Tr = go.transform;
        //        Force = go.GetComponentInChildren<ParticleSystemForceField>();
        //        ForceTr = Force.transform;
        //    }
        //}
    }
}
