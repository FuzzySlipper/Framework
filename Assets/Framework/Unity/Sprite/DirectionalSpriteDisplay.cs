using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public enum BillboardMode {
        ForceUp,
        CamFwd,
        TrUp,
        NoYAxis,
        None,
        CamRot = 10,
        CamRot45 = 12,
        CamRot60 = 13,
        CamRot90 = 14,
    }

    public static class BillboardExtension {
        public static void Apply(this BillboardMode mode, Transform transform, bool backwards) {
            switch (mode) {
                case BillboardMode.ForceUp:
                case BillboardMode.TrUp:
                    var lookPos = transform.position + Game.SpriteCamera.transform.rotation * Vector3.forward;
                    transform.LookAt(backwards ? lookPos : -lookPos, mode == BillboardMode.TrUp ? transform.up : Game.SpriteCamera.transform.rotation * Vector3.up);
                    break;
                case BillboardMode.CamFwd:
                    transform.forward = backwards ? -Game.SpriteCamera.transform.forward : Game.SpriteCamera.transform.forward;
                    break;
                case BillboardMode.NoYAxis:
                    Vector3 viewDirection = new Vector3(Game.SpriteCamera.transform.forward.x, 0, Game.SpriteCamera.transform.forward.z);
                    transform.LookAt(transform.position + (backwards ? viewDirection : -viewDirection));
                    break;
                case BillboardMode.CamRot:
                    transform.rotation = backwards ?  Quaternion.Inverse(Game.SpriteCamera.transform.rotation) : Game.SpriteCamera.transform.rotation;
                    break;
                case BillboardMode.CamRot45:
                case BillboardMode.CamRot90:
                case BillboardMode.CamRot60:
                    float factor = -45f;
                    switch (mode) {
                        case BillboardMode.CamRot90:
                            factor = -90f;
                            break;
                        case BillboardMode.CamRot60:
                            factor = -60f;
                            break;
                    }
                    var rot = Quaternion.LookRotation(Game.SpriteCamera.transform.forward) * Quaternion.Euler(factor, 0, 0);
                    transform.rotation = backwards ? Quaternion.Inverse(rot) : rot;
                    break;
            }
        }
    }

    [ExecuteInEditMode]
    public class DirectionalSpriteDisplay : MonoBehaviour, IOnCreate {

        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;
        [SerializeField] private bool _backwards = false;
        [SerializeField] private SpriteFacing _facing = SpriteFacing.Fourway;
        [SerializeField] private DirectionalSprite _sprite = null;
        [SerializeField] private SpriteRenderer _renderer = null;

        private DirectionsEight _orientation = DirectionsEight.Top;
        private bool _active = true;

        public void OnCreate(PrefabEntity entity) {
            MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialBlock);
            _renderer.sprite = _sprite.GetFacingSprite(_orientation);
            materialBlock.SetTexture("_BumpMap", _sprite.NormalMap);
            materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
            _renderer.SetPropertyBlock(materialBlock);
        }

        void OnBecameVisible() {
            _active = true;
        }

        void OnBecameInvisible() {
            _active = false;
        }

        void Update() {
            if (!_active) {
                return;
            }
            _billboard.Apply(transform, _backwards);
            bool inMargin;
            var orientation = SpriteFacingControl.GetCameraSide(_facing, transform, transform.parent, 5, out inMargin);
            if (_orientation == orientation || (inMargin && (orientation.IsAdjacent(_orientation)))) {
                return;
            }
            _orientation = orientation;
            var facing = orientation;
            if (_facing == SpriteFacing.EightwayFlipped) {
                facing = _orientation.GetFlippedSide();
                _renderer.flipX = _orientation.IsFlipped();
            }
            var sprite = _sprite.GetFacingSprite(facing);
            if (sprite != null) {
                _renderer.sprite = sprite;
            }
        }
    }
}

