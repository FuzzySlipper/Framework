using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class RendererTracker : MonoBehaviour {

        private Renderer _render;
        private Mesh _mesh = null;
        private SkinnedMeshRenderer _skinnedMesh = null;

        public Mesh Mesh { get { return _mesh; } }
        public SkinnedMeshRenderer SkinnedMesh { get { return _skinnedMesh; } }
        public Renderer Renderer { get { return _render; } }

        public void Setup(Renderer render) {
            _render = render;
            if (render is SkinnedMeshRenderer) {
                _skinnedMesh = _render as SkinnedMeshRenderer;
            }
            else {
                var filter = GetComponent<MeshFilter>();
                if (filter != null) {
                    _mesh = filter.sharedMesh;
                }
            }
        }

        public void SetVisible(bool isVisible) {
            if (_render != null) {
                _render.enabled = isVisible;
            }
        }

    }
}