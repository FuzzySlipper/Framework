using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public interface IActionProvider {
        void SetupEntity(Entity entity, SimpleDataLine lineData, DataEntry allData);
        void OnUsage(ActionEvent ae, ActionCommand cmd);
    }

    public class ActionProviderAttribute : Attribute {
        public readonly string Action;

        public ActionProviderAttribute(string action) {
            Action = action;
        }
    }
}
