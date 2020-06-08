using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static partial class Player {

        private static Transform _tr;
        private static PlayerSaveData _playerSaveData = new PlayerSaveData();

        public static Camera Cam { get { return PlayerCamera.Cam; } }
        public static Camera MinimapCamera { get;set; }
        
        private static FloatValueCollection _currencies = new FloatValueCollection();

        public static FloatValueHolder DefaultCurrencyHolder { get { return _currencies.GetHolder(Game.DefaultCurrencyId); } }
        public static Transform Tr {
            get {
                if (_tr == null) {
                    _tr = GameObject.FindGameObjectWithTag(StringConst.TagPlayer).transform;
                }
                return _tr;
            }
            set { _tr = value; }
        }
        public static PlayerSaveData Data { get { return _playerSaveData; } set { _playerSaveData = value; } }
        public static ItemInventory MainInventory { get; set; }
        public static Entity MainEntity { get; set; }

        public static int HighestCurrentLevel {
            get {
                int level = 1;
                for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                    if (PlayerPartySystem.Party[i] == null) {
                        continue;
                    }
                    level = MathEx.Max(PlayerPartySystem.Party[i].Get<EntityLevelComponent>().Value, level);
                }
                return level;
            }
        }

        public static FloatValueHolder GetCurrency(string id) {
            return _currencies.GetHolder(id);
        }


        private static PlayerActorSaveData _savePartyData = new PlayerActorSaveData();
        private static PlayerCharacterTemplate _selectedActor = null;

        public static PlayerActorSaveData SavedPartyData { get => _savePartyData; }
        public static IntValueHolder Supplies { get; set; }
        public static PlayerCharacterTemplate SelectedActor {
            get {
                if (_selectedActor == null) {
                    FindValidSelected();
                }
                return _selectedActor;
            }
            set {
                if (_selectedActor == value || (value != null && value.Entity == null)) {
                    return;
                }
                //if (_selectedActor != null) {
                //    _selectedActor.Selected(false);
                //}
                _selectedActor = value;
                MessageKit.post(Messages.SelectedActorChanged);
                //if (_selectedActor != null) {
                //    _selectedActor.Selected(true);
                //}
            }
        }

        public static void FindValidSelected() {
            for (int i = 0; i < PlayerPartySystem.Get.Length; i++) {
                if (!PlayerPartySystem.Get[i].IsDead && PlayerPartySystem.Get[i].Entity != null) {
                    SelectedActor = PlayerPartySystem.Get[i];
                    break;
                }
            }
        }

        public static void AddFactionRep(RiftFactions faction, float amtToAdd) {
            Data.FactionRep[(int) faction] += amtToAdd;
        }

        public static void SetFactionRep(RiftFactions faction, float amtToSet) {
            Data.FactionRep[(int) faction] = amtToSet;
        }
    }
}
