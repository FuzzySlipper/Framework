using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class Action : IComponent {

        [SerializeField] private Dictionary<string, List<IActionEventHandler>> _events = new Dictionary<string, List<IActionEventHandler>>();
        
        public List<ICommandCost> Costs = new List<ICommandCost>();
        public string ActionType;
        public float Range;
        public ActionFx Fx;
        public string EquipVariable;
        public string AnimationTrigger;
        public bool Primary;
        public int EquippedSlot = -1;
        private CachedComponent<AmmoComponent> _ammo = new CachedComponent<AmmoComponent>();

        public AmmoComponent Ammo {
            get {
                return _ammo;
            }
            set {
                _ammo.Set(value);
            }
        }
        public Entity Entity { get { return this.GetEntity(); } }
        public Action() {}

        public Action(SerializationInfo info, StreamingContext context) {
            _events = info.GetValue(nameof(_events), _events);
            Costs = info.GetValue(nameof(Costs), Costs);
            Range = info.GetValue(nameof(Range), Range);
            Primary = info.GetValue(nameof(Primary), Primary);
            EquippedSlot = info.GetValue(nameof(EquippedSlot), EquippedSlot);
            _ammo = info.GetValue(nameof(_ammo), _ammo);
            Fx = ItemPool.LoadAsset<ActionFx>(info.GetValue(nameof(Fx), ""));
        }

        public void AddEvent(string eventName, IActionEventHandler eventHandler) {
            if (!_events.TryGetValue(eventName, out var list)) {
                list = new List<IActionEventHandler>();
                _events.Add(eventName, list);
            }
            list.Add(eventHandler);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_events), _events);
            info.AddValue(nameof(Costs), Costs);
            info.AddValue(nameof(Range), Range);
            info.AddValue(nameof(Primary), Primary);
            info.AddValue(nameof(EquippedSlot), EquippedSlot);
            info.AddValue(nameof(_ammo), _ammo);
            info.AddValue(nameof(Fx), ItemPool.GetAssetLocation(Fx));
        }

        public void PostAnimationEvent(ActionEvent ae, string eventName) {
            if (_events.TryGetValue(eventName, out var animationList)) {
                for (int i = 0; i < animationList.Count; i++) {
                    animationList[i].Trigger(ae, eventName);
                }
            }
            ActionState state = AnimationEvents.ToStateEvent(eventName);
            if (state != ActionState.None) {
                ae.Origin.Entity.Post(new ActionEvent(ae.Origin.Entity, ae.Origin.Entity, ae.Position, ae.Rotation, state));
            }
            if (state == ActionState.None) {
                return;
            }
            var stateEvent = new ActionEvent(ae.Origin.Entity, Entity, ae.Position, ae.Rotation, state);
            if (Fx != null) {
                Fx.TriggerEvent(stateEvent);
            }
        }
    }

}
