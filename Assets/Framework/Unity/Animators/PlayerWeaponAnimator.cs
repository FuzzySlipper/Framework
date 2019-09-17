using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class PlayerWeaponAnimator : EntityIdentifier, IOnCreate, IAnimator, IModelComponent, ISystemUpdate {

        public System.Action<PlayerWeaponAnimator, string> OnAnimatorEvent;

        [SerializeField] private Transform _armsPivot = null;
        [Range(0, 0.1f)] [SerializeField] private float _verticalSwayAmount = 0.04f;
        [Range(0, 0.1f)] [SerializeField] private float _horizontalSwayAmount = 0.108f;
        [Range(0, 15f)] [SerializeField] private float _swaySpeed = 3.54f;
        [SerializeField] private Transform _primaryPivot = null;
        [SerializeField] private Transform _secondaryPivot = null;

        private IWeaponModel _weaponModel;
        private MaterialPropertyBlock[] _blocks;
        private Renderer[] _renderers;
        private PrefabEntity _entity;
        private Vector3 _lastEventPosition;
        private Quaternion _lastEventRotation;
        private Vector3 _resetPoint;
        private float _bobTime = 0;
        private Action _primary;
        private bool _reloading;
        protected Task ProceduralAnimation;
        protected Action AnimationAction;

        public bool Unscaled { get { return false; } }
        public string CurrentAnimationEvent { get; protected set; }
        public IWeaponModel WeaponModel { get => _weaponModel; }
        public Renderer[] Renderers { get { return _renderers; } }
        public Transform Tr { get { return transform; } }
        public Transform PrimaryPivot { get => _weaponModel != null ? _weaponModel.Spawn : _primaryPivot; }
        public Transform SecondaryPivot { get => _secondaryPivot; }
        public virtual Vector3 GetEventPosition {
            get {
                if (AnimationAction != null && AnimationAction.Primary) {
                    return _weaponModel?.Spawn.position ?? _lastEventPosition;
                }
                return _lastEventPosition;
            }
        }
        public virtual Quaternion GetEventRotation {
            get {
                if (AnimationAction != null && AnimationAction.Primary) {
                    return _weaponModel?.Spawn.rotation ?? _lastEventRotation;
                }
                return _lastEventRotation;
            }
        }

        private GameOptions.CachedBool _useWeaponBob = new GameOptions.CachedBool("UseWeaponBob");
        public MaterialPropertyBlock[] MaterialBlocks {
            get {
                if (_blocks != null) {
                    return _blocks;
                }
                _blocks = new MaterialPropertyBlock[_renderers.Length];
                for (int i = 0; i < _blocks.Length; i++) {
                    _blocks[i] = new MaterialPropertyBlock();
                    _renderers[i].GetPropertyBlock(_blocks[i]);
                }
                return _blocks;
            }
        }

        public virtual void OnCreate(PrefabEntity entity) {
            _entity = entity;
            List<Renderer> renders = new List<Renderer>();
            for (int i = 0; i < entity.Renderers.Length; i++) {
                if (entity.Renderers[i] == null || entity.Renderers[i].transform.CompareTag(StringConst.TagConvertedTexture) ||
                    entity.Renderers[i] is SpriteRenderer) {
                    continue;
                }
                renders.Add(entity.Renderers[i]);
            }
            _renderers = renders.ToArray();
            _resetPoint = _armsPivot.localPosition;
        }

        public virtual void OnSystemUpdate(float dt) {
            if (_useWeaponBob && !PlayingAnimation) {
                //if (_useWeaponBob && !PlayingAnimation) {
                _bobTime += dt;
                var velocity = Player.FirstPersonController.VelocityPercent;
                var y = _verticalSwayAmount * Mathf.Sin((_swaySpeed * 2) * _bobTime) * velocity;
                var x = _horizontalSwayAmount * Mathf.Sin(_swaySpeed * _bobTime) * velocity;
                _armsPivot.localPosition = _resetPoint + new Vector3(x, y, 0);
            }
        }

        protected virtual void ProcessEvent(string eventName) {
            CurrentAnimationEvent = eventName;
            switch (eventName) {
                case AnimationEvents.Default:
                    ClipEventTriggered();
                    break;
                case AnimationEvents.FxOn:
                    _weaponModel?.SetFx(true);
                    break;
                case AnimationEvents.FxOff:
                    _weaponModel?.SetFx(false);
                    break;
                default:
                    FirstPersonCamera.PlaySpringAnimation(eventName);
                    break;
            }
            OnAnimatorEvent?.Invoke(this, eventName);
        }

        public virtual void SetWeaponModel(IWeaponModel model) {
            _weaponModel = model;
            _weaponModel.Tr.SetParent(_primaryPivot);
            _weaponModel.Setup();
            _weaponModel.SetFx(false);
        }

        private void ClearAnimation() {
            ProceduralAnimation = null;
        }

        public void ReloadWeapon() {
            if (_primary == null || !_primary.Ammo.CanLoadAmmo() || PlayingAnimation) {
                return;
            }
            ProceduralAnimation = TimeManager.StartTask(ReloadWeaponProcess(), false, ClearAnimation);
        }

        public void StopReload() {
            _reloading = false;
        }

        public void ChangeAction(Action newAction) {
            if (newAction != null) {
                ProceduralAnimation = TimeManager.StartTask(SwapWeapons(newAction), ClearAnimation);
            }
            else if (_primary != null && newAction == null) {
                ProceduralAnimation = TimeManager.StartTask(LowerWeapon(true), ClearAnimation);
            }
            //else {
            //    SetupAction(newAction);
            //    ProceduralAnimation = TimeManager.StartTask(RaiseWeapon());
            //}
        }

        private void SetupAction(Action usable) {
            _primary = usable;
            if (!string.IsNullOrEmpty(usable.WeaponModel)) {
                var weaponModel = ItemPool.Spawn(UnityDirs.Weapons, usable.WeaponModel, Vector3.zero, Quaternion.identity, false, false);
                if (weaponModel != null) {
                    usable.Entity.Add(new TransformComponent(weaponModel.transform));
                    SetWeaponModel(weaponModel.GetComponent<IWeaponModel>());
                }
            }
            //PlayAnimation(usable.IdleAnimation, false);
        }

        protected virtual void ClearWeaponModel() {
            if (_primary != null && WeaponModel != null) {
                var primaryTr = _primary.Entity.Get<TransformComponent>()?.Value;
                if (primaryTr == WeaponModel.Tr) {
                    _primary.Entity.Remove<TransformComponent>();
                }
            }
            if (_weaponModel != null) {
                ItemPool.Despawn(_weaponModel.Tr.gameObject);
            }
            _weaponModel = null;
        }

        public void SetVisible(bool status) {
            _entity.SetVisible(status);
        }

        public virtual void ClipEventTriggered() {
            _lastEventPosition = AnimationAction != null && !AnimationAction.Primary? SecondaryPivot.position : PrimaryPivot.position;
            _lastEventRotation = AnimationAction != null && !AnimationAction.Primary ? SecondaryPivot.rotation : PrimaryPivot.rotation;
        }

        public void SetMaterialKeyword(string keyword, bool status) {
            for (int i = 0; i < _renderers.Length; i++) {
                if (status) {
                    _renderers[i].material.EnableKeyword(keyword);
                }
                else {
                    _renderers[i].material.DisableKeyword(keyword);
                }
            }
        }

        public void ApplyMaterialBlocks(MaterialPropertyBlock[] blocks) {
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].SetPropertyBlock(blocks[i]);
            }
        }

        private IEnumerator LowerWeapon(bool removing) {
            StopCurrentAnimation();
            yield return LowerArms();
            if (removing) {
                ClearWeaponModel();
            }
        }

        private IEnumerator RaiseWeapon() {
            //StopCurrentAnimation();
            SetVisible(true);
            yield return RaiseArms();
            CheckFinish();
        }

        private IEnumerator SwapWeapons(Action newAction) {
            yield return LowerWeapon(true);
            SetupAction(newAction);
            if (_weaponModel != null && _weaponModel.IdlePose != null) {
                yield return TransitionToPose(_weaponModel.IdlePose);
            }
            else {
                yield return RaiseWeapon();
            }
        }

        private IEnumerator ReloadWeaponProcess() {
            yield return LowerWeapon(false);
            _reloading = true;
            UIChargeCircle.ManualStart(_primary.Ammo.Template.ReloadText);
            var reloadPerAmmo = _primary.Ammo.ReloadSpeed / _primary.Ammo.Amount.MaxValue;
            var totalAmmo = _primary.Ammo.Amount.MaxValue - _primary.Ammo.Amount.Value;
            int current = 0;
            while (true) {
                if (_primary == null) {
                    break;
                }
                current++;
                UIChargeCircle.ManualSetPercent((float) current/totalAmmo);
                if (!_primary.Ammo.TryLoadOneAmmo(Player.Controller.Entity) || !_reloading) {
                    break;
                }
                yield return reloadPerAmmo;
            }
            UIChargeCircle.StopCharge();
            yield return RaiseWeapon();
        }
        public void PlayAnimation(string clip, bool overrideClip) {
            PlayAnimation(clip, overrideClip, null);
        }

        protected abstract IEnumerator LowerArms();
        protected abstract IEnumerator RaiseArms();
        protected abstract IEnumerator TransitionToPose(MusclePose pose);
        protected abstract void CheckFinish();
        public abstract void StopCurrentAnimation();
        public abstract bool PlayingAnimation { get; }
        public abstract string CurrentAnimation { get; }
        public abstract float CurrentAnimationLength { get; }
        public abstract float CurrentAnimationRemaining { get; }
        public abstract bool IsAnimationComplete(string clip);
        public abstract bool IsAnimationEventComplete(string clip);
        public abstract void PlayAnimation(string clip, bool overrideClip, Action action);
    }
}
