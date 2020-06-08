using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class WeaponBobNode : StateGraphNode {
        public float VerticalSwayAmount = 0.025f;
        public float HorizontalSwayAmount = 0.075f;
        public float SwaySpeed = 3f;
        protected override Vector2 GetNodeSize { get { return new Vector2(DefaultNodeSize.x, DefaultNodeSize.y * 1.25f); } }
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.LabelField("Vertical", textStyle);
            UnityEditor.EditorGUILayout.Slider(so.FindProperty(nameof(VerticalSwayAmount)), 0, 0.1f, GUIContent.none);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.LabelField("Horizontal", textStyle);
            UnityEditor.EditorGUILayout.Slider(so.FindProperty(nameof(HorizontalSwayAmount)), 0, 0.1f, GUIContent.none);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.LabelField("Sway", textStyle);
            UnityEditor.EditorGUILayout.Slider(so.FindProperty(nameof(SwaySpeed)), 0, 15f, GUIContent.none);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            so.ApplyModifiedProperties();
#endif
            return false;
        }

        public override string Title { get { return "WeaponBobNode"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        private class RuntimeNode : RuntimeStateNode {
            private GameOptions.CachedBool _useWeaponBob = new GameOptions.CachedBool("UseWeaponBob");
            
            private WeaponBobNode _node;
            private WeaponBobComponent _component;
            
            public RuntimeNode(WeaponBobNode node, RuntimeStateGraph graph) : base(node, graph) {
                _node = node;
                _component = graph.Entity.Get<WeaponBobComponent>();
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                if (_component == null) {
                    _component = Graph.Entity.Get<WeaponBobComponent>();
                }
            }

            public override void OnExit() {
                base.OnExit();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (!_useWeaponBob) {
                    return false;
                }
                _component.BobTime += dt;
                var velocity = PlayerControllerSystem.FirstPersonController.VelocityPercent;
                var y = _node.VerticalSwayAmount * Mathf.Sin((_node.SwaySpeed * 2) * _component.BobTime) * velocity;
                var x = _node.HorizontalSwayAmount * Mathf.Sin(_node.SwaySpeed * _component.BobTime) * velocity;
                _component.ArmsPivot.localPosition = _component.ResetPoint + new Vector3(x, y, 0);
                return false;
            }
        }
    }
    // public sealed class WeaponBobNode : StateGraphNode {
    //     
    //     [Range(1f, 3f)] [SerializeField] private float _headBobFrequency = 1.5f;
    //     [Range(.1f, 2f)] [SerializeField] private float _bobStrideSpeedLengthen = .35f;
    //     [Range(.1f, 2f)] [SerializeField] private float _headBobSwayAngle = .5f;
    //     [Range(.1f, 5f)] [SerializeField] private float _jumpLandMove = 2f;
    //     [Range(10f, 100f)] [SerializeField] private float _jumpLandTilt = 35f;
    //     [Range(.1f, 4f)] [SerializeField] private float _springElastic = 1.25f;
    //     [Range(.1f, 2f)] [SerializeField] private float _springDampen = .77f;
    //     [Header("Position")]
    //     [Range(-1, 1f)] [SerializeField] private float _headBobHeight = .35f;
    //     [Range(0, 0.5f)] [SerializeField] private float _headBobSideMovement = .075f;
    //     [Range(.1f, 2f)] [SerializeField] private float _bobHeightSpeedMultiplier = .35f;
    //
    //     public class MeshWeaponBob : RuntimeStateNode {
    //
    //
    //     private Transform _tr;
    //     private float _springPos, _springVelocity, _headBobFade;
    //     private Vector3 _velocity, _velocityChange, _prevPosition, _prevVelocity;
    //
    //     public static float HeadBobCycle { get; private set; }
    //     public static float XPos { get; private set; }
    //     public static float YPos { get; private set; }
    //     public static float XTilt { get; private set; }
    //     public static float YTilt { get; private set; }
    //     public bool Unscaled { get { return false; } }
    //
    //     private void Awake() {
    //         _tr = transform;
    //         ResetValues();
    //         MessageKit.addObserver(Messages.PlayerTeleported, ResetValues);
    //     }
    //
    //     private void ResetValues() {
    //         _velocity = _velocityChange = _prevVelocity = Vector3.zero;
    //         XPos = YPos = HeadBobCycle = XTilt = YTilt = _springPos = _springVelocity = _headBobFade = 0f;
    //         _prevPosition = _tr.position;
    //     }
    //
    //     public void OnSystemUpdate(float delta) {
    //         if (Math.Abs(delta) < 0.00001f) {
    //             return;
    //         }
    //         _velocity = (_tr.position - _prevPosition) / delta;
    //         _velocityChange = _velocity - _prevVelocity;
    //         _prevPosition = _tr.position;
    //         _prevVelocity = _velocity;
    //         if (Player.FirstPersonController.CurrentMovement != FPMovementAction.Climbing) {
    //             _velocity.y = 0f;
    //         }
    //         if (float.IsNaN(_springVelocity)) {
    //             ResetValues();
    //         }
    //         _springVelocity -= _velocityChange.y;
    //         _springVelocity -= _springPos * _springElastic;
    //         _springVelocity *= _springDampen;
    //         _springPos += _springVelocity * delta;
    //         _springPos = Mathf.Clamp(_springPos, -.32f, .32f);
    //
    //         if (Mathf.Abs(_springVelocity) < .05f && Mathf.Abs(_springPos) < .05f) {
    //             _springVelocity = _springPos = 0f;
    //         }
    //
    //         var flatVelocity = _velocity.magnitude;
    //
    //         if (Player.FirstPersonController.CurrentMovement == FPMovementAction.Climbing) {
    //             flatVelocity *= 4f;
    //         }
    //         else if (Player.FirstPersonController.CurrentMovement != FPMovementAction.Climbing && !Player.FirstPersonController.Grounded) {
    //             flatVelocity /= 4f;
    //         }
    //
    //         var strideLengthen = 1f + flatVelocity * _bobStrideSpeedLengthen;
    //         HeadBobCycle += flatVelocity / strideLengthen * (delta / _headBobFrequency);
    //
    //         var bobSwayFactor = Mathf.Sin(HeadBobCycle * Mathf.PI * 2f + Mathf.PI * .5f);
    //         var bobFactor = Mathf.Sin(HeadBobCycle * Mathf.PI * 2f);
    //         bobFactor = 1f - (bobFactor * .5f + 1f);
    //         bobFactor *= bobFactor;
    //         _headBobFade = Mathf.Lerp(_headBobFade, _velocity.magnitude < .1f ? 0f : 1f, delta);
    //         var speedHeightFactor = 1f + flatVelocity * _bobHeightSpeedMultiplier;
    //
    //         XPos = -_headBobSideMovement * bobSwayFactor * _headBobFade;
    //         YPos = _springPos * _jumpLandMove + bobFactor * _headBobHeight * _headBobFade * speedHeightFactor;
    //         XTilt = _springPos * _jumpLandTilt;
    //         YTilt = bobSwayFactor * _headBobSwayAngle * _headBobFade;
    //     }
    //     }
    // }
}
