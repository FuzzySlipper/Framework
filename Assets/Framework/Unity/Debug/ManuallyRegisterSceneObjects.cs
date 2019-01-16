using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace PixelComrades {
    public class ManuallyRegisterSceneObjects : MonoBehaviour {

        void Awake() {
            var scene = SceneManager.GetActiveScene();
            ItemPool.RegisterSceneEntities(scene);
        }
    }
}
