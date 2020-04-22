using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;

namespace PixelComrades {
    public class ScriptableDatabaseTable {
        [TableList(IsReadOnly = true, AlwaysExpanded = true), ShowInInspector]
        private readonly List<ScriptableObjectWrapper> _allObjs;
        
        public ScriptableObjectWrapper this[int index] { get { return _allObjs[index]; } }

        public ScriptableDatabaseTable(List<ScriptableObjectWrapper> allObjs) {
            _allObjs = allObjs;
        }
    }

    public abstract class ScriptableObjectWrapper {

        public ScriptableDatabase Database { get; }
        public UnityEngine.Object Obj { get; }

        public abstract Texture Icon { get; }

        protected ScriptableObjectWrapper(ScriptableDatabase db, UnityEngine.Object obj) {
            Obj = obj;
            Database = db;
        }
    }

}