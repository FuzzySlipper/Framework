using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class SceneMeshUtilities {
        public static Bounds GetSceneBounds(LayerMask mask) {
            List<ExtraMesh> meshes;
            CollectMeshes(out meshes, new Bounds(Vector3.zero,
                new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)), mask);

            if (meshes.Count == 0) {
                return new Bounds(Vector3.zero, Vector3.one);
            }
            var bounds = meshes[0].bounds;

            for (int i = 1; i < meshes.Count; i++) {
                bounds.Encapsulate(meshes[i].bounds);
            }
            return new Bounds(bounds.center, bounds.size);
        }

        private static bool CollectMeshes(out List<ExtraMesh> extraMeshes, Bounds bounds, LayerMask mask) {
            extraMeshes = new List<ExtraMesh>();
            GetSceneMeshes(bounds, mask, extraMeshes);
            if (extraMeshes.Count == 0) {
                Debug.LogWarning("No MeshFilters were found contained in the layers specified by the 'mask' variables");
                return false;
            }

            return true;
        }

        private static void GetSceneMeshes(Bounds bounds, LayerMask layerMask, List<ExtraMesh> meshes) {
            var filters = GameObject.FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];

            var filteredFilters = new List<MeshFilter>(filters.Length / 3);

            for (int i = 0; i < filters.Length; i++) {
                MeshFilter filter = filters[i];
                Renderer rend = filter.GetComponent<Renderer>();

                if (rend != null && filter.sharedMesh != null && rend.enabled &&
                    (((1 << filter.gameObject.layer) & layerMask) != 0)) {
                    filteredFilters.Add(filter);
                }
            }

            var cachedVertices = new Dictionary<Mesh, Vector3[]>();
            var cachedTris = new Dictionary<Mesh, int[]>();

            for (int i = 0; i < filteredFilters.Count; i++) {
                MeshFilter filter = filteredFilters[i];

                // Note, guaranteed to have a renderer
                Renderer rend = filter.GetComponent<Renderer>();

                //Workaround for statically batched meshes
                if (!rend.isPartOfStaticBatch) {
                    //Only include it if it intersects with the graph
                    if (rend.bounds.Intersects(bounds)) {
                        Mesh mesh = filter.sharedMesh;
                        var smesh = new ExtraMesh();
                        smesh.matrix = rend.localToWorldMatrix;
                        smesh.original = filter;
                        if (cachedVertices.ContainsKey(mesh)) {
                            smesh.vertices = cachedVertices[mesh];
                            smesh.triangles = cachedTris[mesh];
                        }
                        else {
                            smesh.vertices = mesh.vertices;
                            smesh.triangles = mesh.triangles;
                            cachedVertices[mesh] = smesh.vertices;
                            cachedTris[mesh] = smesh.triangles;
                        }

                        smesh.bounds = rend.bounds;

                        meshes.Add(smesh);
                    }
                }
            }
        }

        public struct ExtraMesh {
            /** Source of the mesh.
             * May be null if the source was not a mesh filter
             */
            public MeshFilter original;

            public int area;
            public Vector3[] vertices;
            public int[] triangles;

            /** World bounds of the mesh. Assumed to already be multiplied with the matrix */
            public Bounds bounds;

            public Matrix4x4 matrix;

            public ExtraMesh(Vector3[] v, int[] t, Bounds b) {
                matrix = Matrix4x4.identity;
                vertices = v;
                triangles = t;
                bounds = b;
                original = null;
                area = 0;
            }

            public ExtraMesh(Vector3[] v, int[] t, Bounds b, Matrix4x4 matrix) {
                this.matrix = matrix;
                vertices = v;
                triangles = t;
                bounds = b;
                original = null;
                area = 0;
            }

            /** Recalculate the bounds based on vertices and matrix */
            public void RecalculateBounds() {
                Bounds b = new Bounds(matrix.MultiplyPoint3x4(vertices[0]), Vector3.zero);

                for (int i = 1; i < vertices.Length; i++) {
                    b.Encapsulate(matrix.MultiplyPoint3x4(vertices[i]));
                }
                //Assigned here to avoid changing bounds if vertices would happen to be null
                bounds = b;
            }
        }


        //private void ScanDungeonNavMesh() {
        //    Status = "Building Nav Mesh";
        //    _dungeonNavMesh.Build();
        //    Progress = 1;
        //}
    }
}