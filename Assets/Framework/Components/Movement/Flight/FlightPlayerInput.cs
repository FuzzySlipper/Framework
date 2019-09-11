using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightPlayerInput : ComponentBase {

        public PlayerInput Input;
        public string Boost;
        public string ThrustAxis;
        public string StrafeHorizontalAxis;
        public string StrafeVerticalAxis;
        public string PitchAxis;
        public string YawAxis;
        public string RollAxis;

        public FlightPlayerInput(PlayerInput input, string boost, string thrustAxis, string strafeHorizontalAxis, string strafeVerticalAxis, string pitchAxis, string yawAxis, string rollAxis) {
            Input = input;
            Boost = boost;
            ThrustAxis = thrustAxis;
            StrafeHorizontalAxis = strafeHorizontalAxis;
            StrafeVerticalAxis = strafeVerticalAxis;
            PitchAxis = pitchAxis;
            YawAxis = yawAxis;
            RollAxis = rollAxis;
        }

        public void UpdateControl(FlightControl control) {
            control.GotoPos = GetMousePosition();
            if (!string.IsNullOrEmpty(Boost) && Input.GetKeyDown(Boost)) {
                control.Boost = 1;
            }
            else {
                control.Boost = 0;
            }
            if (!string.IsNullOrEmpty(ThrustAxis)) {
                control.Thrust = Input.GetAxis(ThrustAxis);
            }
            if (!string.IsNullOrEmpty(StrafeHorizontalAxis)) {
                control.StrafeHorizontal = Input.GetAxis(StrafeHorizontalAxis);
            }
            if (!string.IsNullOrEmpty(StrafeVerticalAxis)) {
                control.StrafeVertical = Input.GetAxis(StrafeVerticalAxis);
            }
            if (control.Config.UseMouse) {
                if (control.Config.UseDirectControl) {
                    var mousePosition = UnityEngine.Input.mousePosition;
                    Vector3 mousePos = new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, 0) - new Vector3(0.5f, 0.5f, 0f);
                    mousePos = mousePos * 2; // Go from -1 to 1 left to right of screen

                    // Adjust the mouse distance taking into account the dead radius
                    float mouseDist = Vector3.Magnitude(mousePos);
                    mouseDist = Mathf.Max(mouseDist - control.Config.MouseDeadRadius, 0);
                    mousePos = mousePos.normalized * mouseDist;

                    control.Pitch = Mathf.Clamp((control.Config.MouseVerticalInverted? 1 : -1) * mousePos.y * control.Config.MousePitchSensitivity, -1f, 1f);

                    if (control.Config.LinkYawAndRoll) {
                        control.Roll = Mathf.Clamp(-mousePos.x * control.Config.MouseRollSensitivity, -1f, 1f);
                        control.Yaw = Mathf.Clamp(-control.Roll * control.Config.YawRollRatio, -1f, 1f);
                    }
                    else {
                        if (!string.IsNullOrEmpty(RollAxis)) {
                            control.Roll = Input.GetAxis(RollAxis);
                        }
                        control.Yaw = Mathf.Clamp(mousePos.x * control.Config.MouseYawSensitivity, -1f, 1f);
                    }
                }
                else {
                    control.TurnTowardsPoint(control.Entity.Tr, control.GotoPos);
                }
            }
            else {
                if (!string.IsNullOrEmpty(PitchAxis)) {
                    control.Pitch = Input.GetAxis(PitchAxis);
                }
                if (!string.IsNullOrEmpty(YawAxis)) {
                    control.Yaw = Input.GetAxis(YawAxis);
                }
                if (!string.IsNullOrEmpty(RollAxis)) {
                    control.Roll = Input.GetAxis(RollAxis);
                }
            }
        }

        protected Vector3 GetMousePosition() {
            Vector3 mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = 1000f;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }
    }
}
