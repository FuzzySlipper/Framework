using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class SpriteCollider : MonoBehaviour, IOnCreate, IPoolEvents {

        private const float ColliderDepth = 0.5f;
        [SerializeField] private SpriteRenderer _spriteRenderer = null;
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private MeshCollider _mc;
        [SerializeField] private MeshCollider _detailCollider = null;
        
        private Vector3[] _meshColliderVerts;
        private Mesh _mesh;
        
        private Vector2 _bottomLeft, _bottomRight, _topLeft, _topRight;

        public Collider UnityCollider { get; private set; }
        public MeshCollider DetailCollider { get => _detailCollider; }

        public void OnCreate(PrefabEntity entity) {
            if (_mc == null) {
                _mc = GetComponent<MeshCollider>();
            }
            if (_boxCollider == null) {
                _boxCollider = GetComponent<BoxCollider>();
            }
            if (_mc != null) {
                _mesh = new Mesh();
                UnityCollider = _mc;
            }
            else {
                UnityCollider = _boxCollider;
            }
        }

        public void OnPoolSpawned() {
            if (_detailCollider != null) {
                var detailTr = _detailCollider.transform;
                detailTr.SetParent(null);
                detailTr.position = new Vector3(Random.Range(-100,100), -100, 0);
                detailTr.rotation = Quaternion.identity;
            }
        }

        public void OnPoolDespawned() {
            if (_detailCollider != null) {
                _detailCollider.transform.SetParent(transform);
            }
        }

        public void UpdateSpriteRendererCollider() {
            if (!_spriteRenderer || !_spriteRenderer.sprite) {
                return;
            }
            UpdateOffsets(_spriteRenderer.sprite, _spriteRenderer.transform.localScale, _spriteRenderer.flipX);
            UpdateCollider();
        }

        public void UpdateSprite(Sprite sprite, bool flipX) {
            UpdateOffsets(sprite, transform.localScale, flipX);
            UpdateCollider();
        }

        private void UpdateCollider() {
            if (_mc != null) {
                UpdateMeshCollider();
            }
            else if (_boxCollider != null) {
                UpdateBoxCollider();
            }
        }

        public void AltUpdateSprite(Sprite sprite, bool flipX) {
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var width = (sprite.rect.width / 2) / pixelsPerUnit;
            var height = (sprite.rect.height / 2) / pixelsPerUnit;
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);

            float leftPadding = (padding.x / pixelsPerUnit);
            float leftOffset = width - leftPadding;
            float rightPadding = (padding.z / pixelsPerUnit);
            float rightOffset = width - rightPadding;
            float bottomPadding = (padding.y / pixelsPerUnit);
            float bottomOffset = height - bottomPadding;
            float topPadding = (padding.w / pixelsPerUnit);
            float topOffset = height - topPadding;
            if (flipX) {
                var tempLeft = leftOffset;
                leftOffset = rightOffset;
                rightOffset = tempLeft;
            }
            var center = new Vector2(0, height);
            _bottomLeft = center + new Vector2(-leftOffset, -bottomOffset);
            _bottomRight = center + new Vector2(rightOffset, -bottomOffset);
            _topLeft = center + new Vector2(-leftOffset, topOffset);
            _topRight = center + new Vector2(rightOffset, topOffset);
            UpdateCollider();
        }
        
        private void UpdateOffsets(Sprite sprite, Vector3 localScale, bool flipX) {
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var width = (sprite.rect.width / 2) / pixelsPerUnit;
            var height = (sprite.rect.height / 2) / pixelsPerUnit;
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);
            
            float leftPadding = (padding.x / pixelsPerUnit) * localScale.x;
            float leftOffset = width - leftPadding;
            float rightPadding = (padding.z / pixelsPerUnit) * localScale.x;
            float rightOffset = width - rightPadding;
            float bottomPadding = (padding.y / pixelsPerUnit) * localScale.x;
            float bottomOffset = height - bottomPadding;
            float topPadding = (padding.w / pixelsPerUnit) * localScale.x;
            float topOffset = height - topPadding;
            if (flipX) {
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

        public void UpdateCollider(SavedSpriteCollider spriteCollider) {
            _mesh.Clear();
            if (spriteCollider == null) {
                return;
            }
            _mesh.SetVertices(spriteCollider.CollisionVertices);
            _mesh.SetTriangles(spriteCollider.CollisionIndices, 0, true);
            _mc.sharedMesh = _mesh;
            if (_detailCollider != null) {
                _detailCollider.sharedMesh = _mesh;
            }
        }

        [Button]
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
            if (_mc == null) {
                InitMeshCollider();
            }
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
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.Encapsulate(_bottomLeft);
            bounds.Encapsulate(_bottomRight);
            bounds.Encapsulate(_topLeft);
            bounds.Encapsulate(_topRight);

            _boxCollider.center = bounds.center;
            _boxCollider.size = new Vector3(bounds.size.x, bounds.size.y, ColliderDepth);
        }
    }
}
