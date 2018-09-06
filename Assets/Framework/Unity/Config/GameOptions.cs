using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public partial class GameOptions : ScriptableSingleton<GameOptions> {

        [SerializeField] private float _lookSmooth = .721f;
        [SerializeField] private float _lookSensitivity = 2.5f;
        [SerializeField] private bool _useHeadBob = true;
        [SerializeField] private bool _mouseLook = true;
        [SerializeField] private bool _turnbased = false;
        [SerializeField] private bool _shaking = true;
        [SerializeField] private bool _painFlash = true;
        [SerializeField] private bool _readyNotice = true;
        [SerializeField] private bool _pauseForInput = true;
        [SerializeField] private bool _verboseInventory = true;
        [SerializeField] private bool _logAllDamage = true;
        [SerializeField] private bool _showMiss = false;
        [SerializeField] private bool _useCulling = true;
        [SerializeField] private bool _debugMode = true;
        
        public static bool UseShaking { get { return Main._shaking; } set { Main._shaking = value; } }
        public static bool UsePainFlash { get { return Main._painFlash; } set { Main._painFlash = value; } }
        public static float LookSmooth { get { return Main._lookSmooth; } set { Main._lookSmooth = value; } }
        public static float LookSensitivity { get { return Main._lookSensitivity; } set { Main._lookSensitivity = value; } }
        public static bool UseHeadBob { get { return Main._useHeadBob; } set { Main._useHeadBob = value; } }
        public static bool VerboseInventory { get { return Main._verboseInventory; } set { Main._verboseInventory = value; } }
        public static bool LogAllDamage { get { return Main._logAllDamage; } set { Main._logAllDamage = value; } }
        public static bool ShowMiss { get { return Main._showMiss; } set { Main._showMiss = value; } }
        public static bool PauseForInput { get { return Main._pauseForInput; } set { Main._pauseForInput = value; } }
        public static bool ReadyNotice { get { return Main._readyNotice; } set { Main._readyNotice = value; } }
        public static bool UseCulling { get { return Main._useCulling; } set { Main._useCulling = value; } }
        public static bool DebugMode { get { return Main._debugMode; } }

        public static bool MouseLook {
            get { return Main._mouseLook; }
            set {
                if (Main._mouseLook == value) {
                    return;
                }
                Main._mouseLook = value;
                if (Main._mouseLook && !Game.CursorUnlocked) {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else if (!Main._mouseLook) {
                    Cursor.lockState = CursorLockMode.None;
                    Player.Cam.transform.localRotation = Quaternion.identity;
                    //CameraMouseLook.main.Pivot.localRotation = Player.Controller.ActorPivot.localRotation;
                }
            }
        }

        public static bool TurnBased {
            get { return Main._turnbased; }
            set {
                Main._turnbased = value;
                MessageKit.post(Messages.TurnBasedChanged);
            }
        }
    }
}
