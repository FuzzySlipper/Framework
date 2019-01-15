using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CommandHolder {
        public int LastRoll { get; private set; }
        public int Chance; // this needs to be in range 0 -100
        public Command Command;
        public AdvancedTargeting Targeting;

        public CommandHolder(int chanceNotNormalized, Command command, AdvancedTargeting targeting) {
            Chance = chanceNotNormalized;
            Command = command;
            Targeting = targeting;
            LastRoll = 0;
        }

        public void Roll() {
            LastRoll = Game.Random.Next(0, Chance);
        }
    }
}
