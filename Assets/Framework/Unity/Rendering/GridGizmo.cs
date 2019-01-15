using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GridGizmo : MonoBehaviour {

        [SerializeField] private Color _gridColor = Color.grey;
        [SerializeField] private bool _active = false;
        [SerializeField] private int _maxGridSize = 25;
        [SerializeField] private float _gridSize = 3;
        [SerializeField] private float _height = 0;

#if UNITY_EDITOR
        private void DrawSceneGrid() {
            UnityEditor.Handles.color = _gridColor;
            var max = (_maxGridSize + 1) * _gridSize;
            var min = -(_maxGridSize * _gridSize);
            var offset = new Vector3( _gridSize * 0.5f, 0, _gridSize * 0.5f);
            for (int x = -_maxGridSize; x <= _maxGridSize + 1; x++) {
                var xPos = x * _gridSize;
                UnityEditor.Handles.DrawLine(new Vector3(xPos, _height, min) + offset,
                    new Vector3(xPos, _height, max) + offset);
            }
            for (int z = -_maxGridSize; z <= _maxGridSize + 1; z++) {
                var zPos = z * _gridSize;
                UnityEditor.Handles.DrawLine(new Vector3(min, _height, zPos) + offset,
                    new Vector3(max, _height, zPos) + offset);
            }
        }

        void OnDrawGizmos() {
            if (_active && !Application.isPlaying) {
                DrawSceneGrid();
            }
        }
#endif
    }
}
