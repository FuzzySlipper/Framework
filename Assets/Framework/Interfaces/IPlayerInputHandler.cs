using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public interface IPlayerInputHandler {
        Ray GetLookTargetRay { get; }
        Vector2 LookInput { get; set; }
        Vector2 MoveInput { get; set; }
        bool IsCursorOverUI { get; }
        void RunUpdate();
        Vector3 GetMousePosition(float range = 500);
        bool GetButtonDown(string action);
        bool GetButton(string action);
        bool GetButtonUp(string action);
        float GetAxis(string axis);
        float GetAxisRaw(string axis);
        bool GetKeyDown(Key key);
    }
}
