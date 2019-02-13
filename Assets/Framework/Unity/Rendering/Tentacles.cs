using UnityEngine;

namespace PixelComrades {
    [ExecuteInEditMode] public class Tentacles : MonoBehaviour {

        public enum TailMode {
            CurlUpLeft,
            CurlUpRight,
            Wiggle
        }

        public enum TailType {
            Thin,
            Fish
        }

        public const int SegmentCount = 30; //How many parts does the tentacle have
#pragma warning disable 649
        [SerializeField] private bool _alive = true;
        [SerializeField] private float _length = 3;
        [SerializeField] private Material _material;
        [SerializeField] private float _speed = 2.5f;
        [SerializeField] private TailMode _tailMode = TailMode.Wiggle;
        [SerializeField] private TailType _tailtype = TailType.Thin;
#pragma warning restore 649
        //private float _angle;
        private Mesh _mesh = null;
        private float _offset = 0;
        private float[] _amplitude = new float[SegmentCount];
        private Color[] _colours = new Color[SegmentCount * 2 + 2];
        private float[] _frequency = new float[SegmentCount];
        private int[] _indices = new int[SegmentCount * 6];
        private Vector3[] _normals = new Vector3[SegmentCount * 2 + 2];
        private float[] _segments = new float[SegmentCount];
        private float[] _targets = new float[SegmentCount];
        private Vector2[] _uvs = new Vector2[SegmentCount * 2 + 2];
        private Vector3[] _vertices = new Vector3[SegmentCount * 2 + 2];

        void Awake() {
            BuildMesh();
        }
        
        //private void SetTargetAngle(float angle) {
        //    for (int i = 0; i < segmentCount; ++i) {
        //        _targets[i] = angle;
        //    }
        //}

        void Update() {
            if (_mesh == null) {
                BuildMesh();
            }
            if (_tailMode == TailMode.CurlUpRight) {
                for (int i = 0; i < SegmentCount; ++i) {
                    float delta = _targets[i] - _segments[i];
                    float turnSpeed = 0.14f * Time.deltaTime;
                    if (delta > 0 && delta > turnSpeed) {
                        delta = turnSpeed;
                    }
                    else if (delta < 0 && delta < turnSpeed) {
                        delta = turnSpeed;
                    }

                    _segments[i] += delta;
                }
            }
            if (_tailMode == TailMode.CurlUpLeft) {
                for (int i = 0; i < SegmentCount; ++i) {
                    float delta = _targets[i] - _segments[i];
                    float turnSpeed = -0.07f * Time.deltaTime;
                    if (delta > 0 && delta > turnSpeed) {
                        delta = turnSpeed;
                    }
                    else if (delta < 0 && delta < turnSpeed) {
                        delta = turnSpeed;
                    }

                    _segments[i] += delta;
                }
            }
            else if (_tailMode == TailMode.Wiggle) {
                if (!_alive) {
                    Die();
                }
                for (int i = 0; i < SegmentCount; ++i) {
                    _segments[i] = Mathf.Sin(Time.time * _frequency[i]) * _amplitude[i] / _speed;
                }
            }

            //Based on  new information make, set the tails values for drawing
            Vector3 currentPos = new Vector3(0, 0, 0);
            float currentAngle = 0;

            Vector3 dir;
            Vector3 perp;

            for (int i = 0; i < SegmentCount; ++i) {
                dir = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);
                perp = new Vector3(dir.y / (i + 1.5f), -dir.x / (i + 1.5f), 0);

                _vertices[i * 2] = currentPos - perp;
                _vertices[i * 2 + 1] = currentPos + perp;
                _normals[i * 2] = new Vector3(0, 0, 1);
                _normals[i * 2 + 1] = new Vector3(0, 0, 1);
                _uvs[i * 2] = new Vector2((float) i / SegmentCount, 0);
                _uvs[i * 2 + 1] = new Vector2((float) i / SegmentCount, 1);
                currentPos = currentPos + dir * _length;
                currentAngle = currentAngle + _segments[i] + _offset;
            }

            _mesh.vertices = _vertices;

            _mesh.uv = _uvs;
            _mesh.triangles = _indices;
            _mesh.colors = _colours;
            _mesh.normals = _normals;
            _mesh.RecalculateBounds();

            Graphics.DrawMesh(_mesh, transform.localToWorldMatrix, _material, gameObject.layer);
        }

        private void Die() {
            int curl = Random.Range(0, 2);
            _tailMode = curl == 0 ? TailMode.CurlUpRight : TailMode.CurlUpLeft;
        }

        public void BuildMesh() {
            _mesh = new Mesh();
            for (int i = 0; i < SegmentCount; ++i) {
                _segments[i] = 0.0f;
                _targets[i] = 0.14f;
                _frequency[i] = Random.Range(0.1f, 1.0f * 2);
                _amplitude[i] = Random.Range(0.2f, 0.4f * 2);
            }

            for (int i = 0; i < SegmentCount - 1; ++i) {
                if (_tailtype == TailType.Thin) {
                    _indices[i * 6] = i * 2 + 0;
                    _indices[i * 6 + 1] = i * 2 + 3;
                    _indices[i * 6 + 2] = i * 2 + 1;
                    _indices[i * 6 + 4] = i * 2 + 2;
                    _indices[i * 6 + 3] = i * 2 + 0;
                    _indices[i * 6 + 5] = i * 2 + 3;
                }
                else if (_tailtype == TailType.Fish) {
                    _indices[i * 6] = i * 1;
                    _indices[i * 6 + 1] = i * 2 + 3;
                    _indices[i * 6 + 2] = i * 2 + 1;
                    _indices[i * 6 + 4] = i * 2 + 2;
                    _indices[i * 6 + 3] = i * 2 + 0;
                    _indices[i * 6 + 5] = i * 1;
                }
            }
        }
    }
}