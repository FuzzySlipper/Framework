using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightPhysicsSystem : SystemBase, IMainFixedUpdate {

        private TemplateList<FlyingTemplate> _flyingList;
        private ManagedArray<FlyingTemplate>.RefDelegate _del;
        
        public FlightPhysicsSystem() {
            TemplateFilter<FlyingTemplate>.Setup(FlyingTemplate.GetTypes());
            _flyingList = EntityController.GetTemplateList<FlyingTemplate>();
            _del = UpdateNode;
        }

        public override void Dispose() {
            base.Dispose();
            if (_flyingList != null) {
                _flyingList.Clear();
            }
        }

        public void OnFixedSystemUpdate(float dt) {
            _flyingList.Run(_del);
        }

        private void UpdateNode(ref FlyingTemplate template) {
            if (template.Control.CurrentMode == FlightControl.Mode.Disabled) {
                return;
            }
            template.UpdateControl();
            //Calculate the current speed by using the dot product. This tells us
            //how much of the ship's velocity is in the "forward" direction 
            template.Control.Speed = Vector3.Dot(template.Rigidbody.velocity, template.Tr.forward);
            switch (template.Control.CurrentMode) {
                case FlightControl.Mode.Flying:
                    if (template.Engine != null) {
                        UpdateFlightEngine(template);
                    }
                    break;
                case FlightControl.Mode.FakeFlying:
                    if (template.FakeFlight != null) {
                        CalculateFakeFlightMovement(template);
                    }
                    break;
                case FlightControl.Mode.Hovering:
                    if (template.Hover != null) {
                        CalculateHover(template);
                        CalculatePropulsion(template);
                    }
                    break;
            }
            if (template.Banking != null) {
                CalculateCosmeticBanking(template);
            }
        }

        private void UpdateFlightEngine(FlyingTemplate template) {
            AutoLevel(template);
            var steeringValues = new Vector3(template.Control.Pitch, template.Control.Yaw, template.Control.Roll);
            steeringValues = steeringValues.Clamp(-1, 1);
            template.Rigidbody.AddRelativeTorque(Vector3.Scale(steeringValues,  template.Engine.AvailableRotationForces), ForceMode.Acceleration);

            var nextTranslationThrottleValues = new Vector3(template.Control.StrafeHorizontal, template.Control.StrafeVertical, template.Control.Thrust);
            nextTranslationThrottleValues = nextTranslationThrottleValues.Clamp(-1, 1);
            var boostThrottleValues = new Vector3(0, 0, Mathf.Clamp(template.Control.Boost, -1, 1));

            Vector3 nextForces = Vector3.Scale(nextTranslationThrottleValues, template.Engine.AvailableTranslationForces) +
                                 Vector3.Scale(boostThrottleValues, template.Engine.AvailableBoostForces);

            nextForces = Vector3.Min(nextForces, template.Engine.MaxTranslationForces);
            template.Rigidbody.AddRelativeForce(nextForces);
        }


        private void CalculateFakeFlightMovement(FlyingTemplate template) {
            var position = template.Rigidbody.position;
            //position.x = Mathf.Clamp(position.x + (template.Control.Yaw * template.FakeFlight.Config.FakeFlightSpeed) + (template.Control.StrafeHorizontal * template.FakeFlight.Config.FakeFlightStrafe), -template.FakeFlight.Config.FakeFlightLimitX, template.FakeFlight.Config.FakeFlightLimitX);
            //position.y = Mathf.Clamp(position.y + (-template.Control.Pitch * template.FakeFlight.Config.FakeFlightSpeed), -template.FakeFlight.Config.FakeFlightLimitY, template.FakeFlight.Config.FakeFlightLimitY);
            //template.Rigidbody.MovePosition(position);
            position.x = template.FakeFlight.Config.FakeFlightLimitX.Clamp(position.x + (template.Control.Yaw * template.FakeFlight.Config.FakeFlightSpeed) + (template.Control.StrafeHorizontal * template.FakeFlight.Config.FakeFlightStrafe));
            position.y = template.FakeFlight.Config.FakeFlightLimitY.Clamp(position.y + (-template.Control.Pitch * template.FakeFlight.Config.FakeFlightSpeed));
            //template.Rigidbody.MovePosition(position);
            //template.Entity.Tr.position = position;
            var pitchBank = template.FakeFlight.Config.AngleOfPitch * template.Control.Pitch;
            var pitchRotation = Quaternion.Lerp(template.Rigidbody.rotation, Quaternion.Euler(pitchBank, 0f, 0), Time.deltaTime * template.FakeFlight.Config.PitchSpeed);
            //template.Rigidbody.MoveRotation(pitchRotation);
            //template.Entity.Tr.rotation = pitchRotation;
            template.Entity.Post(new ChangePositionEvent(template.Entity, position, pitchRotation));
        }

        private void AutoLevel(FlyingTemplate template) {
            if (!template.Control.Config.AutoLevel) {
                template.Control.Roll = 0;
                return;
            }
            if (template.Control.Config.AltOrient) {
                var angleOffTarget = Vector3.Angle(template.Tr.forward, template.Control.GotoPos - template.Tr.position);
                var aggressiveRoll = Mathf.Clamp(template.Control.GotoPos.x, -1f, 1f);
                var wingsLevelRoll = template.Tr.right.y;
                // Blend between auto level and banking into the target.
                var wingsLevelInfluence = Mathf.InverseLerp(0f, template.Control.Config.HorizonOrientAngle, angleOffTarget);
                template.Control.Roll = -Mathf.Lerp(wingsLevelRoll, aggressiveRoll, wingsLevelInfluence);
            }
            else {
                var flatForward = template.Tr.forward;
                var flatRight = Vector3.Cross(Vector3.up, flatForward);
                var localFlatRight = template.Tr.InverseTransformDirection(flatRight);
                var rollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
                template.Control.Roll = -rollAngle * template.Control.Config.AutoRollSpeed;
            }
        }

        private void CalculateHover(FlyingTemplate template) {
            FindGround(template, template.Hover.Config.MaxGroundDist);
            if (template.Hover.IsOnGround) {
                float forcePercent = template.Hover.HoverPid.Seek(template.Hover.Config.HoverHeight, template.Hover.Height, Time.fixedDeltaTime);
                Vector3 force = template.Hover.GroundNormal * template.Hover.Config.HoverForce * forcePercent;
                Vector3 gravity = -template.Hover.GroundNormal * template.Hover.Config.HoverGravity * template.Hover.Height;
                template.Rigidbody.AddForce(force + gravity, ForceMode.Acceleration);
            }
            else {
                Vector3 gravity = -template.Hover.GroundNormal * template.Hover.Config.FallGravity;
                template.Rigidbody.AddForce(gravity, ForceMode.Acceleration);
            }
            MatchGroundRotation(template);
        }

        private void MatchGroundRotation(FlyingTemplate template) {
            //Calculate the amount of pitch and roll the ship needs to match its orientation
            //with that of the ground. This is done by creating a projection and then calculating
            //the rotation needed to face that projection
            Vector3 projection = Vector3.ProjectOnPlane(template.Tr.forward, template.Hover.GroundNormal);
            Quaternion rotation = Quaternion.LookRotation(projection, template.Hover.GroundNormal);
            if (template.Control.CurrentMode == FlightControl.Mode.Flying) {
                rotation.x = template.Rigidbody.rotation.x;
                rotation.y = template.Rigidbody.rotation.y;
            }
            template.Rigidbody.MoveRotation(Quaternion.Lerp(template.Rigidbody.rotation, rotation, Time.deltaTime * template.Hover.Config.OrientSpeed));
        }

        private void CalculatePropulsion(FlyingTemplate template) {
            float yawTorque = (template.Control.Yaw * template.Hover.Config.TurnForce) - template.Rigidbody.angularVelocity.y;
            float pitchTorque = (template.Control.Pitch * template.Hover.Config.PitchForce);
            float jumpForce = 0f;
            if (template.Hover.Jumping) {
                jumpForce = template.Hover.IsOnGround && template.Hover.Jumping ? template.Hover.Config.JumpForce : 0;
                template.Hover.Jumping = false;
            }
            template.Rigidbody.AddRelativeTorque(pitchTorque, yawTorque, 0f, ForceMode.VelocityChange);
            template.Rigidbody.AddRelativeForce(new Vector3(template.Control.StrafeHorizontal * template.Hover.Config.StrafeForce, jumpForce, 0), ForceMode.Force);
            float sidewaysSpeed = Vector3.Dot(template.Rigidbody.velocity, template.Tr.right);
            Vector3 sideFriction = -template.Tr.right * (sidewaysSpeed / Time.fixedDeltaTime);
            template.Rigidbody.AddForce(sideFriction, ForceMode.Acceleration);
            if (template.Control.Thrust <= 0f) {
                template.Rigidbody.velocity *= template.Hover.Config.SlowingVelFactor;
            }
            if (!template.Hover.IsOnGround) {
                return;
            }
            if (template.Control.Boost < 0) {
                template.Rigidbody.velocity *= template.Hover.Config.BrakingVelFactor;
            }
            float propulsion = template.Hover.Config.DriveForce * template.Control.Thrust - template.Hover.Drag * Mathf.Clamp(template.Control.Speed, 0f, template.Hover.Config.MaxForwardSpeed);
            template.Rigidbody.AddForce(template.Tr.forward * propulsion, ForceMode.Acceleration);
        }

        private void FindGround(FlyingTemplate template, float distance) {
            Ray ray = new Ray(template.Tr.position, -template.Tr.up);
            template.Hover.IsOnGround = Physics.Raycast(ray, out var hitInfo, distance, template.Hover.Config.GroundLayer);
            var newGround = Vector3.MoveTowards(template.Hover.GroundNormal, template.Hover.IsOnGround ? hitInfo.normal.normalized : Vector3.up, template.Hover.Config.UpOrientationSpeed * Time.deltaTime);
            if (template.Hover.IsOnGround) {
                template.Hover.Height = hitInfo.distance;
            }
            template.Hover.GroundNormal = newGround;
        }

        private void CalculateCosmeticBanking(FlyingTemplate template) {
            var yawBank = template.Banking.Config.AngleOfRoll * -template.Control.Yaw;
            Quaternion bodyRotation = template.Tr.rotation * Quaternion.Euler(0, 0f, yawBank);
            template.Banking.BankTransform.rotation = Quaternion.Lerp(template.Banking.BankTransform.rotation, bodyRotation, Time.deltaTime * template.Banking.Config.RollSpeed);
        }

        public Vector3 GetMaxSpeedByAxis(FlyingTemplate template, bool withBoost) {
            Vector3 maxForces = template.Engine.AvailableTranslationForces + (withBoost ? template.Engine.AvailableBoostForces : Vector3.zero);
            maxForces = Vector3.Min(maxForces, template.Engine.MaxTranslationForces);
            return new Vector3(
                template.Rigidbody.GetSpeedFromForce(maxForces.x),
                template.Rigidbody.GetSpeedFromForce(maxForces.y),
                template.Rigidbody.GetSpeedFromForce(maxForces.z));
        }
    }

    public static class FlyingExtensions {

        public static void TurnTowardsPoint(this FlightControl control, TransformComponent nodeTr, Vector3 gotoPos) {
            Vector3 localGotoPos = nodeTr.InverseTransformVector(gotoPos - nodeTr.position).normalized;
            control.Pitch = Mathf.Clamp(-localGotoPos.y * control.Config.PitchSensitivity, -1f, 1f);
            control.Yaw = Mathf.Clamp(localGotoPos.x * control.Config.YawSensitivity, -1f, 1f);
            control.GotoPos = gotoPos;
            //_pitch = _turnPitchPID.Seek(-localGotoPos.y, _pitch, Time.deltaTime);
            //_yaw = _turnYawPID.Seek(localGotoPos.x, _yaw, Time.deltaTime);
        }
    }
}
