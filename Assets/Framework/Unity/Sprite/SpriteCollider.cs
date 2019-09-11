using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteCollider : MonoBehaviour, IOnCreate {

        private const float ColliderDepth = 0.5f;

        private Vector3[] _meshColliderVerts;
        private Mesh _mesh;
        private BoxCollider _boxCollider;
        private MeshCollider _mc;
        private SpriteRenderer _renderer;
        private Vector2 _bottomLeft, _bottomRight, _topLeft, _topRight;

        public Collider UnityCollider { get; private set; }

        public void OnCreate(PrefabEntity entity) {
            _renderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider>();
            _mc = GetComponent<MeshCollider>();
            if (_mc != null) {
                UnityCollider = _mc;
                InitMeshCollider();
            }
            else {
                UnityCollider = _boxCollider;
            }
        }

        public void UpdateCollider() {
            if (!_renderer || !_renderer.sprite) {
                return;
            }
            UpdateOffsets();
            if (_mc != null) {
                UpdateMeshCollider();
            }
            else if (_boxCollider != null) {
                UpdateBoxCollider();
            }
        }

        private void UpdateOffsets() {
            var pixelsPerUnit = _renderer.sprite.pixelsPerUnit;
            var width = (_renderer.sprite.rect.width / 2) / pixelsPerUnit;
            var height = (_renderer.sprite.rect.height / 2) / pixelsPerUnit;

            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(_renderer.sprite);

            float leftPadding = (padding.x / _renderer.sprite.pixelsPerUnit) * _renderer.transform.localScale.x;
            float leftOffset = width - leftPadding;
            float rightPadding = (padding.z / _renderer.sprite.pixelsPerUnit) * _renderer.transform.localScale.x;
            float rightOffset = width - rightPadding;
            float bottomPadding = (padding.y / _renderer.sprite.pixelsPerUnit) * _renderer.transform.localScale.x;
            float bottomOffset = height - bottomPadding;
            float topPadding = (padding.w / _renderer.sprite.pixelsPerUnit) * _renderer.transform.localScale.x;
            float topOffset = height - topPadding;

            if (_renderer.flipX) {
                var tempLeft = leftOffset;
                leftOffset = rightOffset;
                rightOffset = tempLeft;
            }

            var center = new Vector2(0, height);
            _bottomLeft = center + new Vector2(-leftOffset, -bottomOffset);
            _bottomRight = center + new Vector2(rightOffset, -bottomOffset);
            _topLeft = center + new Vector2(-leftOffset, topOffset);
            _topRight = center + new Vector2(rightOffset, topOffset);
        }

        private void InitMeshCollider() {
            _meshColliderVerts = new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, 0, 1),
                new Vector3(0, 0, 1),
            };

            int[] triangles = {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };

            _mesh = new Mesh();
            _mesh.Clear();
            _mesh.vertices = _meshColliderVerts;
            _mesh.triangles = triangles;
            _mc.sharedMesh = _mesh;
        }

        

        private void UpdateMeshCollider() {
            _meshColliderVerts[0] = new Vector3(_bottomLeft.x, _bottomLeft.y, -ColliderDepth);
            _meshColliderVerts[1] = new Vector3(_bottomRight.x, _bottomRight.y, -ColliderDepth);
            _meshColliderVerts[2] = new Vector3(_topRight.x, _topRight.y, -ColliderDepth);
            _meshColliderVerts[3] = new Vector3(_topLeft.x, _topLeft.y, -ColliderDepth);
            _meshColliderVerts[4] = new Vector3(_topLeft.x, _topLeft.y, ColliderDepth);
            _meshColliderVerts[5] = new Vector3(_topRight.x, _topRight.y, ColliderDepth);
            _meshColliderVerts[6] = new Vector3(_bottomRight.x, _bottomRight.y, ColliderDepth);
            _meshColliderVerts[7] = new Vector3(_bottomLeft.x, _bottomLeft.y, ColliderDepth);

            _mesh.vertices = _meshColliderVerts;
            _mc.sharedMesh = _mesh;
        }

        private void UpdateBoxCollider() {
            //var bounds = new Bounds(_renderer.transform.position, Vector3.zero);
            //bounds.Encapsulate(_renderer.transform.TransformPoint(_bottomLeft));
            //bounds.Encapsulate(_renderer.transform.TransformPoint(_bottomRight));
            //bounds.Encapsulate(_renderer.transform.TransformPoint(_topLeft));
            //bounds.Encapsulate(_renderer.transform.TransformPoint(_topRight));

            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.Encapsulate(_bottomLeft);
            bounds.Encapsulate(_bottomRight);
            bounds.Encapsulate(_topLeft);
            bounds.Encapsulate(_topRight);

            _boxCollider.center = bounds.center;
            _boxCollider.size = new Vector3(bounds.size.x, bounds.size.y, ColliderDepth);
            //var reducedHeight = topOffset * 2;
            //var finalWidth = Mathf.Max(leftOffset, rightOffset) * 2;
            //_boxCollider.center = new Vector3(0, reducedHeight * 0.5f, 0);
            //_boxCollider.size = new Vector3(finalWidth, reducedHeight , ColliderDepth*2);
        }
    }
}
