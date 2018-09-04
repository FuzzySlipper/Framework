using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DefaultCommand : IComponent {
        public int Owner { get; set; }
        public Command Default { get; }
        public Command Alternative;
        public Command Get { get { return Alternative ?? Default; } }

        public DefaultCommand(Command @default) {
            Default = @default;
        }
    }
}
