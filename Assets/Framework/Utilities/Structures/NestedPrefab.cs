using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NestedPrefab : MonoBehaviour {

    public Vector3 GridSize;

    [SerializeField] private List<PlacedPrefab> _prefabs = new List<PlacedPrefab>();

#if UNITY_EDITOR
    void OnDrawGizmos() {
        DrawGizmos(transform, Vector3.zero);
    }

    public void DrawGizmos(Transform tr, Vector3 pos) {
        if (_prefabs == null || _prefabs.Count == 0) {
            return;
        }
        Gizmos.color = Color.grey;
        Gizmos.DrawCube(tr.position, new Vector3(tr.localScale.x * GridSize.x, 0.1f,
            tr.localScale.z * GridSize.z));
        for (int i = 0; i < _prefabs.Count; i++) {
            if (_prefabs[i].Prefab == null) {
                continue;
            }
            var rootPos = tr.TransformPoint(pos + _prefabs[i].RootPosition);
            _prefabs[i].Display(this, rootPos, i);
        }
    }
#endif
    public void PlacePrefab() {
        for (int p = 0; p < _prefabs.Count; p++) {
            var pos = transform.TransformPoint(_prefabs[p].RootPosition);
            //if (dungeon != null) {
            //    var gridPosition = MathUtils.ToIntVector(pos);
            //    if (dungeon.Model.GetGridCellLookup(gridPosition.x, gridPosition.z).ContainsDoor) {
            //        continue;
            //    }
            //}
            for (int c = 0; c < _prefabs[p].ChildPositions.Length; c++) {
                var newPrefab = ItemPool.Spawn(_prefabs[p].Prefab, pos + _prefabs[p].ChildPositions[c],
                    Quaternion.Euler(_prefabs[p].Rotation), true, true);
                var nested = newPrefab.GetComponent<NestedPrefab>();
                if (nested != null) {
                    nested.transform.localScale = Vector3.one;
                    nested.PlacePrefab();
                    ItemPool.Despawn(nested.gameObject);
                }
            }
        }
    }


    [System.Serializable]
    public class PlacedPrefab {
        public PrefabEntity Prefab;
        public Vector3 RootPosition;
        public Vector3 Rotation;
        public Vector3[] ChildPositions = new Vector3[1];
        public Color Color = Color.white;

        [SerializeField, HideInInspector] private NestedPrefab _parent;

        public override string ToString() {return Prefab != null ? Prefab.name : "Empty";}

#if UNITY_EDITOR

        public void Duplicate() {
            if (_parent == null) {
                return;
            }
            var newPlaced = new PlacedPrefab();
            newPlaced.RootPosition = RootPosition;
            newPlaced.Rotation = Rotation;
            newPlaced.Prefab = Prefab;
            newPlaced.Color = Color;
            newPlaced.ChildPositions = ChildPositions;
            _parent._prefabs.Add(newPlaced);
        }

        public void Display(NestedPrefab parent, Vector3 localToWorld, int index) {
            _parent = parent;
            Gizmos.color = Color;
            var mesh = Prefab.GetComponent<MeshFilter>();
            //Handles.Label(pos + Vector3.up, string.Format("{0} {1}", Prefab.name, index));
            NestedPrefab nestedPrefab = null;
            if (mesh == null) {
                nestedPrefab = Prefab.GetComponent<NestedPrefab>();
            }
            for (int i = 0; i < ChildPositions.Length; i++) {
                if (mesh != null) {
                    Gizmos.DrawWireMesh(mesh.sharedMesh, localToWorld + ChildPositions[i], Quaternion.Euler(Rotation), Prefab.transform.localScale);
                }
                else if (nestedPrefab != null) {
                    nestedPrefab.DrawGizmos(parent.transform, RootPosition + ChildPositions[i]);
                }
                else {
                    var cubeSize = Vector3.one;
                    var collider = Prefab.GetComponent<Collider>();
                    if (collider != null) {
                        cubeSize = collider.bounds.size;
                    }
                    Gizmos.DrawWireCube(localToWorld, cubeSize);
                }
            }
        }
#endif

    }
}

