using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerPartySystem : SystemBase<PlayerPartySystem>, IReceive<DeathEvent> {

        public PlayerCharacterTemplate this[int index] {
            get {
                return Party[index];
            }
        }
        public int Length { get { return Party.Length; } }
        
        public static PlayerCharacterTemplate[] Party { get; set; }
        private GameOptions.CachedInt _partySize = new GameOptions.CachedInt("PartySize");

        public PlayerPartySystem() {
            Party = new PlayerCharacterTemplate[_partySize];
        }

        public void ClearParty() {
            for (int i = 0; i < Party.Length; i++) {
                if (Party[i] != null) {
                    Party[i].Entity.RemoveObserver(this);
                }
                Party[i] = null;
            }
        }

        public void GenerateRandomTurnBasedParty() {
            ClearParty();
            Party = new PlayerCharacterTemplate[_partySize];
            HashSet<int> pickedPortraits = new HashSet<int>();
            for (int i = 0; i < Party.Length; i++) {
                var data = PlayerFactory.GetTurnBasedRandom();
                while (true) {
                    int index = SpriteDatabase.Portraits.RandomIndex();
                    if (pickedPortraits.Contains(index)) {
                        continue;
                    }
                    pickedPortraits.Add(index);
                    data.Get<PortraitComponent>().Portrait = SpriteDatabase.Portraits[index];
                    break;
                }
                SetCharacter(i, data.GetTemplate<PlayerCharacterTemplate>());
            }
        }
        
        public void Handle(DeathEvent arg) {
            
        }

        public PlayerCharacterTemplate GetActor(string actorName) {
            for (int i = 0; i < Party.Length; i++) {
                if (Party[i].Label.Text.CompareCaseInsensitive(actorName)) {
                    return Party[i];
                }
            }
            return null;
        }

        public void FindNextActivePlayer() {
            int idx = 0;
            for (int i = 0; i < Party.Length; i++) {
                if (Party[i] == Player.SelectedActor) {
                    idx = i;
                    break;
                }
            }
            for (int i = idx; i < Party.Length; i++) {
                if (Party[i].Stats.GetVital(Stats.Recovery).IsMax) {
                    Player.SelectedActor = Party[i];
                    return;
                }
            }
            for (int i = 0; i < Party.Length; i++) {
                if (Party[i].Stats.GetVital(Stats.Recovery).IsMax) {
                    Player.SelectedActor = Party[i];
                    return;
                }
            }
        }
        
        public void SetCharacter(int i, PlayerCharacterTemplate character) {
            if (Party == null) {
                Party = new PlayerCharacterTemplate[_partySize];
            }
            if (Party[i] != null) {
                Party[i].Entity.RemoveObserver(this);
                Party[i].Entity.Remove<TransformComponent>();
                UnityToEntityBridge.Unregister(Party[i].Entity);
                Party[i] = null;
            }
            Party[i] = character;
            if (character == null) {
                MessageKit.post(Messages.PlayerCharactersChanged);
                return;
            }
            character.Entity.Name = character.Label.Text;
            if (character.Entity.Tags.Contain(EntityTags.NewCharacter)) {
                character.Entity.Tags.Remove(EntityTags.NewCharacter);
            }
            Party[i].Entity.AddObserver(this);
            MessageKit.post(Messages.PlayerCharactersChanged);
        }

        public void SetVitalMax() {
            for (int i = 0; i < Party.Length; i++) {
                if (Party[i].IsDead) {
                    continue;
                }
                Party[i].Stats.SetMax();
            }
        }

        public void AddExperience(float amount) {
            for (int i = 0; i < Party.Length; i++) {
                Party[i].PlayerLevel.Xp.TotalXp.AddToValue(amount);
            }
        }

        public void SetExperience(float amount) {
            for (int i = 0; i < Party.Length; i++) {
                Party[i].PlayerLevel.Xp.TotalXp.ChangeValue(amount);
            }
        }

        public bool CanSee(MinimapObjectType objType) {
            for (int i = 0; i < Party.Length; i++) {
                if (RpgSettings.CanSee(Party[i].Entity, objType)) {
                    return true;
                }
            }
            return false;
        }

        public bool IsDead() {
            for (int i = 0; i < Party.Length; i++) {
                if (Party[i] == null) {
                    continue;
                }
                if (Party[i].Entity != null && !Party[i].IsDead) {
                    return false;
                }
            }
            return true;
        }
    }
}
