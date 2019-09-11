using System;
using UnityEngine;

namespace SensorToolkit {
    /*
     * A parametric shape for creating field of view cones that work with the trigger sensor. Requires a MeshCollider
     * component on the same gameobject. When the script starts it will dynamically create a mesh for the fov cone and
     * assign it to this MeshCollider component.
     */
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public class FOVCollider : MonoBehaviour {

        [SerializeField] private float _baseSize = 0.5f;
        [SerializeField, Range(1f, 180f)] private float _elevationAngle = 50f;
        [SerializeField, Range(1f, 180f)] private float _fovAngle = 90f;
        [SerializeField] private float _length = 15f;
        [SerializeField, Range(0, 8)] private int _resolution = 0; // The number of vertices used to approximate the arc of the fov cone. Ideally this should be as low as possible.
        [SerializeField] private MeshFilter _renderFilter = null;

        private MeshCollider _mc;
        private Vector3[] _pts;
        private int[] _triangles;
        private Mesh _mesh;

        private void OnEnable() {
            _mc = GetComponent<MeshCollider>();
            CreateCollider();
        }

        private void OnValidate() {
            _length = Mathf.Max(0f, _length);
            _baseSize = Mathf.Max(0f, _baseSize);
        }

        private void ReleaseMesh() {
            if (_mc.sharedMesh != null && _mc.sharedMesh.name == "FOVColliderPoints") {
                DestroyImmediate(_mc.sharedMesh, true);
            }
        }

        public void CreateCollider() {
            _pts = new Vector3[4 + (2 + _resolution) * (2 + _resolution)];
            // There are 2 triangles on the base
            var baseTriangleIndices = 2 * 3;
            // The arc is (Resolution+2) vertices to each side, making (Resolution+1)*(Resolution+1) boxes of 2 tris each
            var arcTriangleIndices = (_resolution + 1) * (_resolution + 1) * 2 * 3;
            // There are 4 sides to the cone, and each side has Resolution+2 triangles
            var sideTriangleIndices = (_resolution + 2) * 3;
            _triangles = new int[baseTriangleIndices + arcTriangleIndices + sideTriangleIndices * 4];

            // Base points
            _pts[0] = new Vector3(-_baseSize / 2f, -_baseSize / 2f, 0f); // Bottom Left
            _pts[1] = new Vector3(_baseSize / 2f, -_baseSize / 2f, 0f); // Bottom Right
            _pts[2] = new Vector3(_baseSize / 2f, _baseSize / 2f, 0f); // Top Right
            _pts[3] = new Vector3(-_baseSize / 2f, _baseSize / 2f, 0f); // Top Left
            _triangles[0] = 2;
            _triangles[1] = 1;
            _triangles[2] = 0;
            _triangles[3] = 3;
            _triangles[4] = 2;
            _triangles[5] = 0;
            for (int y = 0; y < 2 + _resolution; y++) {
                for (int x = 0; x < 2 + _resolution; x++) {
                    int i = 4 + y * (2 + _resolution) + x;
                    float ay = Mathf.Lerp(-_fovAngle / 2f, _fovAngle / 2f, x / (float) (_resolution + 1));
                    float ax = Mathf.Lerp(-_elevationAngle / 2f, _elevationAngle / 2f, y / (float) (_resolution + 1));
                    Vector3 p = Quaternion.Euler(ax, ay, 0f) * Vector3.forward * _length;
                    _pts[i] = p;
                    if (x < 1 + _resolution && y < 1 + _resolution) {
                        var ti = baseTriangleIndices + (y * (_resolution + 1) + x) * 3 * 2;
                        _triangles[ti] = i + 1 + 2 + _resolution; // top right
                        _triangles[ti + 1] = i + 1; // bottom right
                        _triangles[ti + 2] = i; // bottom left
                        _triangles[ti + 3] = i + 2 + _resolution; // top left
                        _triangles[ti + 4] = i + 2 + _resolution + 1; // top right
                        _triangles[ti + 5] = i; // bottom left
                    }
                }
            }

            // Top and bottom side triangles
            for (int x = 0; x < 2 + _resolution; x++) {
                var iTop = 4 + x;
                var iBottom = 4 + (1 + _resolution) * (2 + _resolution) + x;
                var tiTop = baseTriangleIndices + arcTriangleIndices + x * 3;
                var tiBottom = tiTop + sideTriangleIndices;
                if (x == 0) {
                    _triangles[tiTop] = 2;
                    _triangles[tiTop + 1] = 3;
                    _triangles[tiTop + 2] = iTop;
                    _triangles[tiBottom] = 0;
                    _triangles[tiBottom + 1] = 1;
                    _triangles[tiBottom + 2] = iBottom;
                }
                else {
                    _triangles[tiTop] = iTop;
                    _triangles[tiTop + 1] = 2;
                    _triangles[tiTop + 2] = iTop - 1;
                    _triangles[tiBottom] = 1;
                    _triangles[tiBottom + 1] = iBottom;
                    _triangles[tiBottom + 2] = iBottom - 1;
                }
            }

            // Left and right side triangles
            var yIncr = 2 + _resolution;
            for (int y = 0; y < 2 + _resolution; y++) {
                var iLeft = 4 + y * (2 + _resolution);
                var iRight = iLeft + 1 + _resolution;
                var tiLeft = baseTriangleIndices + arcTriangleIndices + sideTriangleIndices * 2 + y * 3;
                var tiRight = tiLeft + sideTriangleIndices;
                if (y == 0) {
                    _triangles[tiLeft] = 3;
                    _triangles[tiLeft + 1] = 0;
                    _triangles[tiLeft + 2] = iLeft;
                    _triangles[tiRight] = 1;
                    _triangles[tiRight + 1] = 2;
                    _triangles[tiRight + 2] = iRight;
                }
                else {
                    _triangles[tiLeft] = 0;
                    _triangles[tiLeft + 1] = iLeft;
                    _triangles[tiLeft + 2] = iLeft - yIncr;
                    _triangles[tiRight] = iRight;
                    _triangles[tiRight + 1] = 1;
                    _triangles[tiRight + 2] = iRight - yIncr;
                }
            }
            ReleaseMesh();
            _mesh = new Mesh();
            _mc.sharedMesh = _mesh;
            _mesh.vertices = _pts;
            _mesh.triangles = _triangles;
            _mesh.name = "FOVColliderPoints";
            _mc.convex = true;
            _mc.isTrigger = true;
            if (_renderFilter != null) {
                _renderFilter.sharedMesh = _mesh;
            }
        }
#if UNITY_EDITOR

        private class StoredSetting {
            public float ElevationAngle;
            public float Angle;
            public float Length;
            public int Resolution;
            public float BaseSize;
        }

        private StoredSetting _storedSettings = new StoredSetting();
        private static float _tolerance = 0.000f;


        private void OnDrawGizmosSelected() {
            if (Math.Abs(_storedSettings.ElevationAngle - _elevationAngle) > _tolerance ||
                Math.Abs(_storedSettings.Angle - _fovAngle) > _tolerance ||
                Math.Abs(_storedSettings.Length - _length) > _tolerance ||
                Math.Abs(_storedSettings.BaseSize - _baseSize) > _tolerance ||
                _storedSettings.Resolution != _resolution) {
                CreateCollider();
                _storedSettings.ElevationAngle = _elevationAngle;
                _storedSettings.Angle = _fovAngle;
                _storedSettings.Length = _length;
                _storedSettings.BaseSize = _baseSize;
                _storedSettings.Resolution = _resolution;
            }
            Gizmos.color = Color.green;
            foreach (Vector3 p in _pts) {
                Gizmos.DrawSphere(transform.TransformPoint(p), 0.1f);
            }
        }
#endif

    }
}