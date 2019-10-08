using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightPhysicsSystem : SystemBase, IMainFixedUpdate {

        private NodeList<FlyingNode> _flyingList;
        private ManagedArray<FlyingNode>.RefDelegate _del;
        
        public FlightPhysicsSystem() {
            NodeFilter<FlyingNode>.Setup(FlyingNode.GetTypes());
            _flyingList = EntityController.GetNodeList<FlyingNode>();
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

        private void UpdateNode(ref FlyingNode node) {
            if (node.Control.CurrentMode == FlightControl.Mode.Disabled) {
                return;
            }
            node.UpdateControl();
            //Calculate the current speed by using the dot product. This tells us
            //how much of the ship's velocity is in the "forward" direction 
            node.Control.Speed = Vector3.Dot(node.Rigidbody.velocity, node.Tr.forward);
            switch (node.Control.CurrentMode) {
                case FlightControl.Mode.Flying:
                    if (node.Engine != null) {
                        UpdateFlightEngine(node);
                    }
                    break;
                case FlightControl.Mode.FakeFlying:
                    if (node.FakeFlight != null) {
                        CalculateFakeFlightMovement(node);
                    }
                    break;
                case FlightControl.Mode.Hovering:
                    if (node.Hover != null) {
                        CalculateHover(node);
                        CalculatePropulsion(node);
                    }
                    break;
            }
            if (node.Banking != null) {
                CalculateCosmeticBanking(node);
            }
        }

        private void UpdateFlightEngine(FlyingNode node) {
            AutoLevel(node);
            var steeringValues = new Vector3(node.Control.Pitch, node.Control.Yaw, node.Control.Roll);
            steeringValues = steeringValues.Clamp(-1, 1);
            node.Rigidbody.AddRelativeTorque(Vector3.Scale(steeringValues,  node.Engine.AvailableRotationForces), ForceMode.Acceleration);

            var nextTranslationThrottleValues = new Vector3(node.Control.StrafeHorizontal, node.Control.StrafeVertical, node.Control.Thrust);
            nextTranslationThrottleValues = nextTranslationThrottleValues.Clamp(-1, 1);
            var boostThrottleValues = new Vector3(0, 0, Mathf.Clamp(node.Control.Boost, -1, 1));

            Vector3 nextForces = Vector3.Scale(nextTranslationThrottleValues, node.Engine.AvailableTranslationForces) +
                                 Vector3.Scale(boostThrottleValues, node.Engine.AvailableBoostForces);

            nextForces = Vector3.Min(nextForces, node.Engine.MaxTranslationForces);
            node.Rigidbody.AddRelativeForce(nextForces);
        }


        private void CalculateFakeFlightMovement(FlyingNode node) {
            var position = node.Rigidbody.position;
            //position.x = Mathf.Clamp(position.x + (node.Control.Yaw * node.FakeFlight.Config.FakeFlightSpeed) + (node.Control.StrafeHorizontal * node.FakeFlight.Config.FakeFlightStrafe), -node.FakeFlight.Config.FakeFlightLimitX, node.FakeFlight.Config.FakeFlightLimitX);
            //position.y = Mathf.Clamp(position.y + (-node.Control.Pitch * node.FakeFlight.Config.FakeFlightSpeed), -node.FakeFlight.Config.FakeFlightLimitY, node.FakeFlight.Config.FakeFlightLimitY);
            //node.Rigidbody.MovePosition(position);
            position.x = node.FakeFlight.Config.FakeFlightLimitX.Clamp(position.x + (node.Control.Yaw * node.FakeFlight.Config.FakeFlightSpeed) + (node.Control.StrafeHorizontal * node.FakeFlight.Config.FakeFlightStrafe));
            position.y = node.FakeFlight.Config.FakeFlightLimitY.Clamp(position.y + (-node.Control.Pitch * node.FakeFlight.Config.FakeFlightSpeed));
            //node.Rigidbody.MovePosition(position);
            //node.Entity.Tr.position = position;
            var pitchBank = node.FakeFlight.Config.AngleOfPitch * node.Control.Pitch;
            var pitchRotation = Quaternion.Lerp(node.Rigidbody.rotation, Quaternion.Euler(pitchBank, 0f, 0), Time.deltaTime * node.FakeFlight.Config.PitchSpeed);
            //node.Rigidbody.MoveRotation(pitchRotation);
            //node.Entity.Tr.rotation = pitchRotation;
            node.Entity.Post(new ChangePositionEvent(node.Entity, position, pitchRotation));
        }

        private void AutoLevel(FlyingNode node) {
            if (!node.Control.Config.AutoLevel) {
                node.Control.Roll = 0;
                return;
            }
            if (node.Control.Config.AltOrient) {
                var angleOffTarget = Vector3.Angle(node.Tr.forward, node.Control.GotoPos - node.Tr.position);
                var aggressiveRoll = Mathf.Clamp(node.Control.GotoPos.x, -1f, 1f);
                var wingsLevelRoll = node.Tr.right.y;
                // Blend between auto level and banking into the target.
                var wingsLevelInfluence = Mathf.InverseLerp(0f, node.Control.Config.HorizonOrientAngle, angleOffTarget);
                node.Control.Roll = -Mathf.Lerp(wingsLevelRoll, aggressiveRoll, wingsLevelInfluence);
            }
            else {
                var flatForward = node.Tr.forward;
                var flatRight = Vector3.Cross(Vector3.up, flatForward);
                var localFlatRight = node.Tr.InverseTransformDirection(flatRight);
                var rollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
                node.Control.Roll = -rollAngle * node.Control.Config.AutoRollSpeed;
            }
        }

        private void CalculateHover(FlyingNode node) {
            FindGround(node, node.Hover.Config.MaxGroundDist);
            if (node.Hover.IsOnGround) {
                float forcePercent = node.Hover.HoverPid.Seek(node.Hover.Config.HoverHeight, node.Hover.Height, Time.fixedDeltaTime);
                Vector3 force = node.Hover.GroundNormal * node.Hover.Config.HoverForce * forcePercent;
                Vector3 gravity = -node.Hover.GroundNormal * node.Hover.Config.HoverGravity * node.Hover.Height;
                node.Rigidbody.AddForce(force + gravity, ForceMode.Acceleration);
            }
            else {
                Vector3 gravity = -node.Hover.GroundNormal * node.Hover.Config.FallGravity;
                node.Rigidbody.AddForce(gravity, ForceMode.Acceleration);
            }
            MatchGroundRotation(node);
        }

        private void MatchGroundRotation(FlyingNode node) {
            //Calculate the amount of pitch and roll the ship needs to match its orientation
            //with that of the ground. This is done by creating a projection and then calculating
            //the rotation needed to face that projection
            Vector3 projection = Vector3.ProjectOnPlane(node.Tr.forward, node.Hover.GroundNormal);
            Quaternion rotation = Quaternion.LookRotation(projection, node.Hover.GroundNormal);
            if (node.Control.CurrentMode == FlightControl.Mode.Flying) {
                rotation.x = node.Rigidbody.rotation.x;
                rotation.y = node.Rigidbody.rotation.y;
            }
            node.Rigidbody.MoveRotation(Quaternion.Lerp(node.Rigidbody.rotation, rotation, Time.deltaTime * node.Hover.Config.OrientSpeed));
        }

        private void CalculatePropulsion(FlyingNode node) {
            float yawTorque = (node.Control.Yaw * node.Hover.Config.TurnForce) - node.Rigidbody.angularVelocity.y;
            float pitchTorque = (node.Control.Pitch * node.Hover.Config.PitchForce);
            float jumpForce = 0f;
            if (node.Hover.Jumping) {
                jumpForce = node.Hover.IsOnGround && node.Hover.Jumping ? node.Hover.Config.JumpForce : 0;
                node.Hover.Jumping = false;
            }
            node.Rigidbody.AddRelativeTorque(pitchTorque, yawTorque, 0f, ForceMode.VelocityChange);
            node.Rigidbody.AddRelativeForce(new Vector3(node.Control.StrafeHorizontal * node.Hover.Config.StrafeForce, jumpForce, 0), ForceMode.Force);
            float sidewaysSpeed = Vector3.Dot(node.Rigidbody.velocity, node.Tr.right);
            Vector3 sideFriction = -node.Tr.right * (sidewaysSpeed / Time.fixedDeltaTime);
            node.Rigidbody.AddForce(sideFriction, ForceMode.Acceleration);
            if (node.Control.Thrust <= 0f) {
                node.Rigidbody.velocity *= node.Hover.Config.SlowingVelFactor;
            }
            if (!node.Hover.IsOnGround) {
                return;
            }
            if (node.Control.Boost < 0) {
                node.Rigidbody.velocity *= node.Hover.Config.BrakingVelFactor;
            }
            float propulsion = node.Hover.Config.DriveForce * node.Control.Thrust - node.Hover.Drag * Mathf.Clamp(node.Control.Speed, 0f, node.Hover.Config.MaxForwardSpeed);
            node.Rigidbody.AddForce(node.Tr.forward * propulsion, ForceMode.Acceleration);
        }

        private void FindGround(FlyingNode node, float distance) {
            Ray ray = new Ray(node.Tr.position, -node.Tr.up);
            node.Hover.IsOnGround = Physics.Raycast(ray, out var hitInfo, distance, node.Hover.Config.GroundLayer);
            var newGround = Vector3.MoveTowards(node.Hover.GroundNormal, node.Hover.IsOnGround ? hitInfo.normal.normalized : Vector3.up, node.Hover.Config.UpOrientationSpeed * Time.deltaTime);
            if (node.Hover.IsOnGround) {
                node.Hover.Height = hitInfo.distance;
            }
            node.Hover.GroundNormal = newGround;
        }

        private void CalculateCosmeticBanking(FlyingNode node) {
            var yawBank = node.Banking.Config.AngleOfRoll * -node.Control.Yaw;
            Quaternion bodyRotation = node.Tr.rotation * Quaternion.Euler(0, 0f, yawBank);
            node.Banking.BankTransform.rotation = Quaternion.Lerp(node.Banking.BankTransform.rotation, bodyRotation, Time.deltaTime * node.Banking.Config.RollSpeed);
        }

        public Vector3 GetMaxSpeedByAxis(FlyingNode node, bool withBoost) {
            Vector3 maxForces = node.Engine.AvailableTranslationForces + (withBoost ? node.Engine.AvailableBoostForces : Vector3.zero);
            maxForces = Vector3.Min(maxForces, node.Engine.MaxTranslationForces);
            return new Vector3(
                node.Rigidbody.GetSpeedFromForce(maxForces.x),
                node.Rigidbody.GetSpeedFromForce(maxForces.y),
                node.Rigidbody.GetSpeedFromForce(maxForces.z));
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
