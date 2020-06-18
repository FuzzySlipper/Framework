using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public enum BillboardMode : byte {
        ForceUp=0,
        CamFwd=1,
        TrUp=2,
        NoYAxis=3,
        None=4,
        CamRot=5,
        FaceCam = 6,
        FaceCamYDiff = 7,
        CamRot45,
        CamRot60,
        CamRot90,
    }

    public static class BillboardExtension {
        private static float _changeSpeed = 200;
        private static float _yMinDiff = 3;

        public static void Apply(this BillboardMode mode, Transform transform, bool backwards, ref float lastAngleHeight) {
            var position = transform.position;
            Vector3 lookPosition = Vector3.zero;
            switch (mode) {
                case BillboardMode.ForceUp:
                case BillboardMode.TrUp:
                    lookPosition = position + Game.SpriteCameraTr.rotation * Vector3.forward;
                    //transform.LookAt(backwards ? lookPos : -lookPos, mode == BillboardMode.TrUp ? transform.up : Game.SpriteCameraTr.rotation * Vector3.up);
                    break;
                case BillboardMode.CamFwd:
                    transform.forward = backwards ? -Game.SpriteCameraTr.forward : Game.SpriteCameraTr.forward;
                    break;
                case BillboardMode.NoYAxis:
                    Vector3 viewDirection = new Vector3(Game.SpriteCameraTr.forward.x, 0, Game.SpriteCameraTr.forward.z);
                    transform.LookAt(position + (backwards ? viewDirection : -viewDirection));
                    break;
                case BillboardMode.CamRot:
                    transform.rotation = backwards ?  Quaternion.Inverse(Game.SpriteCameraTr.rotation) : Game.SpriteCameraTr.rotation;
                    break;
                case BillboardMode.FaceCam:
                    lookPosition = Game.SpriteCameraTr.position;
                    lookPosition.y = position.y;
                    //transform.LookAt(position);
                    //transform.rotation = Quaternion.LookRotation(position - position);
                    break;
                case BillboardMode.FaceCamYDiff:
                    lookPosition = Game.SpriteCameraTr.position;
                    var yHeight = transform.parent.position.y;
                    var yDiff = Mathf.Abs(yHeight - lookPosition.y);
                    var targetY = yDiff < _yMinDiff ? yHeight : lookPosition.y;
                    lastAngleHeight = Mathf.MoveTowards(lastAngleHeight, targetY, _changeSpeed * TimeManager.DeltaUnscaled);
                    lookPosition.y = lastAngleHeight;
                    //transform.LookAt(camPos);
                    //transform.rotation = Quaternion.LookRotation(camPos - transform.position);
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
                    var rot = Quaternion.LookRotation(Game.SpriteCameraTr.forward) * Quaternion.Euler(factor, 0, 0);
                    transform.rotation = backwards ? Quaternion.Inverse(rot) : rot;
                    break;
            }
            
            Vector3 dir = !backwards ? (lookPosition - position) : (position - lookPosition);
            switch (mode) {
                case BillboardMode.ForceUp:
                case BillboardMode.FaceCam:
                case BillboardMode.FaceCamYDiff:
                    transform.rotation = Quaternion.LookRotation(dir);
                    break;
                case BillboardMode.TrUp:
                    transform.rotation = Quaternion.LookRotation(dir, transform.up);
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
        private float _lastAngleHeight;

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
            _billboard.Apply(_renderer.transform, _backwards, ref _lastAngleHeight);
            bool inMargin;
            var orientation = SpriteFacingControl.GetCameraSide(_facing, transform, transform.parent, 5, out inMargin);
            if (_orientation == orientation || (inMargin && (orientation.IsAdjacent(_orientation)))) {
                return;
            }
            _orientation = orientation;
            var facing = orientation;
            if (_facing.RequiresFlipping()) {
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

