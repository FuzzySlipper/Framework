using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static partial class Player {

        private static Transform _tr;
        private static PlayerSaveData _playerSaveData = new PlayerSaveData();

        public static Camera Cam { get { return PlayerCamera.Cam; } }
        public static Camera MinimapCamera { get;set; }
        public static IntValueHolder Currency { get; set; }
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
        public static Rigidbody Rb { get; set; }
        public static Entity[] Entities { get; set; }

        public static int HighestCurrentLevel {
            get {
                int level = 1;
                for (int i = 0; i < Player.Entities.Length; i++) {
                    if (Player.Entities[i] == null) {
                        continue;
                    }
                    level = MathEx.Max(Player.Entities[i].Get<EntityLevelComponent>().Level, level);
                }
                return level;
            }
        }
    }
}
