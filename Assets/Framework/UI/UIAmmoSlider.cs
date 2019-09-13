using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIAmmoSlider : MonoBehaviour, IReceive<EntityDisposed>, IReceive<CurrentActionsChanged> {

        [SerializeField] private Slider _statSlider = null;
        [SerializeField] private int _targetUsable = 0;

        private CharacterNode _actor;
        private IntValueHolder _currentAmmo;

        public void SetNewTarget(CharacterNode actor) {
            if (_actor != null) {
                RemoveActor();
            }
            _actor = actor;
            if (_actor == null) {
                return;
            }
            _actor.Entity.AddObserver(this);
            var action = _actor.CurrentActions.Value.GetAction(_targetUsable);
            if (action != null) {
                _currentAmmo = action.Ammo.Amount;
                _currentAmmo.OnResourceChanged += CheckAmmo;
                _statSlider.value = _currentAmmo.CurrentPercent;
            }
        }

        public void RemoveActor() {
            if (_actor != null) {
                _actor.Entity.RemoveObserver(this);
            }
            _actor = null;
            RemoveAmmo();
        }

        private void RemoveAmmo() {
            _statSlider.value = 0;
            if (_currentAmmo != null) {
                _currentAmmo.OnResourceChanged -= CheckAmmo;
            }
            _currentAmmo = null;
        }

        private void CheckAmmo() {
            _statSlider.value = _currentAmmo.CurrentPercent;
        }

        public void Handle(EntityDisposed arg) {
            RemoveActor();
        }

        public void Handle(CurrentActionsChanged arg) {
            if (arg.Index != _targetUsable) {
                return;
            }
            RemoveAmmo();
            var usable = arg.Action;
            if (usable != null) {
                _currentAmmo = usable.Ammo.Amount;
                _currentAmmo.OnResourceChanged += CheckAmmo;
            }
        }
    }
}