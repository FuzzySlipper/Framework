using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    [AutoRegister]
    public sealed class GameLogSystem : SystemBase {

        public static Color DamageColor = new Color(1f, 0.53f, 0.04f);
        public static Color DeathColor = new Color(0.54f, 0f, 0.05f);
        public static Color HealColor = new Color(0.04f, 0.54f, 0.04f);
        public static Color NormalColor = new Color(0.47f, 0.44f, 0.47f);

        private StringBuilder _logMsg = new StringBuilder(100);
        private StringBuilder _hoverMsg = new StringBuilder(100);
        
        public GameLogSystem() {
            
        }

        public void StartNewMessage(out StringBuilder logMsg, out StringBuilder hoverMsg) {
            _logMsg.Clear();
            _hoverMsg.Clear();
            logMsg = _logMsg;
            hoverMsg = _hoverMsg;
        }

        public void PostCurrentStrings(Color color) {
            MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, 
                new UINotificationWindow.Msg(_logMsg.ToString(),_hoverMsg.ToString(), color));
        }
    }
}
