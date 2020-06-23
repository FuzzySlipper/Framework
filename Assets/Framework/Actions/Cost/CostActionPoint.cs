using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class CostActionPoint : CommandCost {

        public int StandardActions;
        public int MinorActions;
        public int MoveActions;
        public bool ClearAll;
        
        private bool _postStatusUpdates;

        public CostActionPoint(int standardActions, int minorActions, int moveActions, bool clearAll = false, bool postStatusUpdates = false) {
            StandardActions = standardActions;
            MinorActions = minorActions;
            MoveActions = moveActions;
            ClearAll = clearAll;
            _postStatusUpdates = postStatusUpdates;
        }

        public CostActionPoint(string type) {
            StandardActions = MinorActions = MoveActions = 0;
            ClearAll = false;
            switch (type) {
              case ActionPointTypes.Standard:
                  StandardActions = 1;
                  break;
              case ActionPointTypes.Minor:
                  MinorActions = 1;
                  break;
              case ActionPointTypes.Move:
                  MoveActions = 1;
                  break;
            }
        }

        public override void ProcessCost(ActionTemplate action, CharacterTemplate owner) {
            var component = owner.Get<TurnBasedComponent>();
            if (component == null) {
                return;
            }
            if (ClearAll) {
                component.StandardActions = component.MinorActions = component.MoveActions = 0;
                return;
            }
            component.StandardActions -= StandardActions;
            component.MinorActions -= MinorActions;
            component.MoveActions -= MoveActions;
        }

        public override bool CanAct(ActionTemplate action, CharacterTemplate owner) {
            if (ClearAll) {
                return true;
            }
            var component = owner.Get<TurnBasedComponent>();
            if (component == null) {
                if (_postStatusUpdates) {
                    owner.Post(new StatusUpdate(owner, "No TB component", Color.yellow));
                }
                return false;
            }
            if (StandardActions > 0 && component.StandardActions <= 0) {
                if (_postStatusUpdates) {
                    owner.Post(new StatusUpdate(owner, "Not enough standard AP", Color.yellow));
                }
                return false;
            }
            if (MinorActions > 0 && component.MinorActions <= 0) {
                if (_postStatusUpdates) {
                    owner.Post(new StatusUpdate(owner, "Not enough minor AP", Color.yellow));
                }
                return false;
            }
            if (MoveActions > 0 && component.MoveActions <= 0) {
                if (_postStatusUpdates) {
                    owner.Post(new StatusUpdate(owner, "Not enough move AP", Color.yellow));
                }
                return false;
            }
            return true;
        }
    }

    public class ActionPointTypes : StringEnum<ActionPointTypes> {
        public const string Standard = "Standard";
        public const string Minor = "Minor";
        public const string Free = "Free";
        public const string Move = "Move";
        public const string Interrupt = "Interrupt";
        public const string Reaction = "Reaction";
    }
}
