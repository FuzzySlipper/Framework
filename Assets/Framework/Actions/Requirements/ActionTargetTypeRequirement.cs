using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ActionTargetTypeRequirement : IActionRequirement {
        private TargetType _targetType;

        public ActionTargetTypeRequirement(TargetType targetType) {
            _targetType = targetType;
        }

        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return string.Format("Requires: {0}", _targetType);
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            switch (_targetType) {
                default:
                case TargetType.Any:
                    return true;
                case TargetType.Self:
                    return character == target;
                case TargetType.Enemy:
                    return World.Get<FactionSystem>().AreEnemies(character, target);
                case TargetType.Friendly:
                    return World.Get<FactionSystem>().AreFriends(character, target);
            }
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            switch (_targetType) {
                default:
                case TargetType.Any:
                    return true;
                case TargetType.Self:
                    return character == target;
                case TargetType.Enemy:
                    return World.Get<FactionSystem>().AreEnemies(character, target);
                case TargetType.Friendly:
                    return World.Get<FactionSystem>().AreFriends(character, target);
            }
        }
    }
}
