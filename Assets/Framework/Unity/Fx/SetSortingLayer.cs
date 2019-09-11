using UnityEngine;
using System.Collections;
using TMPro;
namespace PixelComrades {
    public class SetSortingLayer : MonoBehaviour, IOnCreate {

        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private int _sortingId = 1;
        [SerializeField] private int _sortExtra = 25;

        public void OnCreate(PrefabEntity entity) {
            SetSortLayer();
        }

        public void SetSortLayer() {
            if (_renderer == null) {
                _renderer = GetComponent<Renderer>();
            }
            if (_renderer != null) {
                _renderer.sortingLayerID = SortingLayer.layers[_sortingId].id;
                _renderer.sortingOrder += _sortExtra;
            }
        }
    }
}
