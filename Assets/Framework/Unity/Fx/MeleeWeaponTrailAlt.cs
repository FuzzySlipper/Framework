using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class MeleeWeaponTrailAlt : MonoBehaviour {
        [System.Serializable]
        private class TrailSection {
            public Vector3 Point;
            public Vector3 UpDir;
            public float Time;

            public TrailSection() {
            }

            public TrailSection(Vector3 p, float t) {
                Point = p;
                Time = t;
            }
        }
        
        [SerializeField] private float _height = 2.0f;
        [SerializeField] private float _time = 2.0f;
        [SerializeField] private bool _alwaysUp = false;
        [SerializeField] private float _minDistance = 0.1f;
	    [SerializeField] private float _lifeTime = 2.0f;
        [SerializeField] private Color _startColor = Color.white;
        [SerializeField] private Color _endColor = new Color(1, 1, 1, 0);
        [SerializeField] private MeshFilter _filter = null;
        [SerializeField] private float _animationIncrement = 0.003f;
        
        private Vector3 _position;
        private TrailSection _currentSection;
        private Matrix4x4 _localSpaceTransform;
        private Mesh _mesh;
        private Vector3[] _vertices;
        private Color[] _colors;
        private Vector2[] _uv;
        private List<TrailSection> _sections = new List<TrailSection>();

        public void StartTrail() {
            _time = 0.01f;
        }
        
        private void Iterate(float iterateTime) {
            _position = transform.position;
            if (_sections.Count == 0 || (_sections[0].Point - _position).sqrMagnitude > _minDistance * _minDistance) {
                TrailSection section = new TrailSection();
                section.Point = _position;
                if (_alwaysUp)
                    section.UpDir = Vector3.up;
                else
                    section.UpDir = transform.TransformDirection(Vector3.up);
                 
                section.Time = iterateTime;
                _sections.Insert(0, section);
            }
        }
        
        public void UpdateTrail(float currentTime, float deltaTime) {
            var dt = Mathf.Clamp(deltaTime, 0, 0.066f);
            var iterator = 0f;
            while (iterator < dt) {
                iterator += _animationIncrement;
                Iterate(currentTime - dt + iterator);
                
            }
            if (_mesh == null) {
                _mesh = new Mesh();
                _filter.sharedMesh = _mesh;
            }
            _mesh.Clear();
            while (_sections.Count > 0 && currentTime > _sections[_sections.Count - 1].Time + _lifeTime) {
                _sections.RemoveAt(_sections.Count - 1);
            }
            if (_sections.Count < 2)
                return;
            _vertices = new Vector3[_sections.Count * 2];
            _colors = new Color[_sections.Count * 2];
            _uv = new Vector2[_sections.Count * 2];
            _currentSection = _sections[0];
            
            // Use matrix instead of transform.TransformPoint for performance reasons
            _localSpaceTransform = transform.worldToLocalMatrix;

            for (var i = 0; i < _sections.Count; i++) {
			    //
                _currentSection = _sections[i];
                // Calculate u for texture uv and color interpolation
                float u = 0.0f;
                if (i != 0)
                    u = Mathf.Clamp01((currentTime - _currentSection.Time) / _time);
                Vector3 upDir = _currentSection.UpDir;
                _vertices[i * 2 + 0] = _localSpaceTransform.MultiplyPoint(_currentSection.Point);
                _vertices[i * 2 + 1] = _localSpaceTransform.MultiplyPoint(_currentSection.Point + upDir * _height);

                _uv[i * 2 + 0] = new Vector2(u, 0);
                _uv[i * 2 + 1] = new Vector2(u, 1);
                Color interpolatedColor = Color.Lerp(_startColor, _endColor, u);
                _colors[i * 2 + 0] = interpolatedColor;
                _colors[i * 2 + 1] = interpolatedColor;
            }

            int[] triangles = new int[(_sections.Count - 1) * 2 * 3];
            for (int i = 0; i < triangles.Length / 6; i++) {
                triangles[i * 6 + 0] = i * 2;
                triangles[i * 6 + 1] = i * 2 + 1;
                triangles[i * 6 + 2] = i * 2 + 2;

                triangles[i * 6 + 3] = i * 2 + 2;
                triangles[i * 6 + 4] = i * 2 + 1;
                triangles[i * 6 + 5] = i * 2 + 3;
            }

            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.uv = _uv;
            _mesh.triangles = triangles;
            _time = currentTime;
        }
        
        public void ClearTrail() {
		    _time = 0;
            if (_mesh != null) {
                _mesh.Clear();
                _sections.Clear();
            }
        }
    }
}
