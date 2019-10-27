using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class SpriteMeshUtilities {

        public static void GenerateColliders(DirectionalAnimation dirAnim, float distance, float quality) {
            var spriteRenderer = new GameObject("SpriteTester").AddComponent<SpriteRenderer>();
            spriteRenderer.transform.ResetPos();
            for (int d = 0; d < dirAnim.DirectionalFrames.Count; d++) {
                var frames = dirAnim.DirectionalFrames[d].Frames;
                dirAnim.DirectionalFrames[d].Colliders = new SavedSpriteCollider[frames.Length];
                for (int f = 0; f < frames.Length; f++) {
                    spriteRenderer.sprite = frames[f];
                    var spriteCollider = spriteRenderer.gameObject.AddComponent<PolygonCollider2D>();
                    dirAnim.DirectionalFrames[d].Colliders[f] = GenerateSavedCollider(spriteCollider.points, distance, quality);
                    UnityEngine.Object.DestroyImmediate(spriteCollider);
                }
            }
            UnityEngine.Object.DestroyImmediate(spriteRenderer.gameObject);
        }

        public static void GenerateColliders(SimpleAnimation anim, float distance, float quality) {
            var spriteRenderer = new GameObject("SpriteTester").AddComponent<SpriteRenderer>();
            spriteRenderer.transform.ResetPos();
            anim.Colliders = new SavedSpriteCollider[anim.Sprites.Length];
            for (int f = 0; f < anim.Sprites.Length; f++) {
                spriteRenderer.sprite = anim.Sprites[f];
                var spriteCollider = spriteRenderer.gameObject.AddComponent<PolygonCollider2D>();
                anim.Colliders[f] = GenerateSavedCollider(spriteCollider.points, distance, quality);
                UnityEngine.Object.DestroyImmediate(spriteCollider);
            }
            UnityEngine.Object.DestroyImmediate(spriteRenderer.gameObject);
        }

        private static SavedSpriteCollider GenerateSavedCollider(Vector2[] poly, float distance, float quality) {
            var collider = new SavedSpriteCollider();
            ExtractMesh(poly, distance, out var verts, out var indices, out collider.HighPoint);
            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices,0, true);
            mesh.RecalculateNormals();
            mesh.Optimize();
            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            meshSimplifier.Initialize(mesh);
            meshSimplifier.SimplifyMesh(quality);
            var destMesh = meshSimplifier.ToMesh();
            collider.CollisionIndices = new List<int>();
            collider.CollisionVertices = new List<Vector3>();
            collider.CollisionVertices.AddRange(destMesh.vertices);
            collider.CollisionIndices.AddRange(destMesh.triangles);
            return collider;
        }

        public static void ExtractMesh(Vector2[] poly, float distance, out List<Vector3> verts, out List<int> indices, 
        out Vector3 highPoint) {

            // convert polygon to triangles
            Triangulator triangulator = new Triangulator(poly);
            var tris = triangulator.Triangulate();
            Vector3[] vertices = new Vector3[poly.Length * 2];
            for (int i = 0; i < poly.Length; i++) {
                vertices[i].x = poly[i].x;
                vertices[i].y = poly[i].y;
                vertices[i].z = -distance; // front vertex
                vertices[i + poly.Length].x = poly[i].x;
                vertices[i + poly.Length].y = poly[i].y;
                vertices[i + poly.Length].z = distance; // back vertex    
            }
            highPoint = Vector3.one;
            for (int i = 0; i < vertices.Length; i++) {
                if (vertices[i].y > highPoint.y) {
                    highPoint = vertices[i];
                }
            }
            int[] triangles = new int[tris.Count * 2 + poly.Length * 6];
            int countTris = 0;
            for (int i = 0; i < tris.Count; i += 3) {
                triangles[i] = tris[i];
                triangles[i + 1] = tris[i + 1];
                triangles[i + 2] = tris[i + 2];
            } // front vertices
            countTris += tris.Count;
            for (int i = 0; i < tris.Count; i += 3) {
                triangles[countTris + i] = tris[i + 2] + poly.Length;
                triangles[countTris + i + 1] = tris[i + 1] + poly.Length;
                triangles[countTris + i + 2] = tris[i] + poly.Length;
            } // back vertices
            countTris += tris.Count;
            for (int i = 0; i < poly.Length; i++) {
                // triangles around the perimeter of the object
                int n = (i + 1) % poly.Length;
                triangles[countTris] = i;
                triangles[countTris + 1] = n;
                triangles[countTris + 2] = i + poly.Length;
                triangles[countTris + 3] = n;
                triangles[countTris + 4] = n + poly.Length;
                triangles[countTris + 5] = i + poly.Length;
                countTris += 6;
            }
            verts = new List<Vector3>();
            verts.AddRange(vertices);
            indices = new List<int>();
            indices.AddRange(triangles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Source: http://wiki.unity3d.com/index.php?title=Triangulator </remarks>
        public class Triangulator {
            private List<Vector2> _points;

            public Triangulator(Vector2[] points) {
                _points = new List<Vector2>(points);
            }

            public List<int> Triangulate() {
                List<int> indices = new List<int>();
                int n = _points.Count;
                if (n < 3)
                    return indices;
                int[] V = new int[n];
                if (Area() > 0) {
                    for (int v = 0; v < n; v++)
                        V[v] = v;
                }
                else {
                    for (int v = 0; v < n; v++)
                        V[v] = (n - 1) - v;
                }
                int nv = n;
                int count = 2 * nv;
                for (int m = 0, v = nv - 1; nv > 2;) {
                    if ((count--) <= 0)
                        return indices;
                    int u = v;
                    if (nv <= u)
                        u = 0;
                    v = u + 1;
                    if (nv <= v)
                        v = 0;
                    int w = v + 1;
                    if (nv <= w)
                        w = 0;
                    if (Snip(u, v, w, nv, V)) {
                        int a, b, c, s, t;
                        a = V[u];
                        b = V[v];
                        c = V[w];
                        indices.Add(a);
                        indices.Add(b);
                        indices.Add(c);
                        m++;
                        for (s = v, t = v + 1; t < nv; s++, t++)
                            V[s] = V[t];
                        nv--;
                        count = 2 * nv;
                    }
                }
                indices.Reverse();
                return indices;
            }

            private float Area() {
                int n = _points.Count;
                float A = 0.0f;
                for (int p = n - 1, q = 0; q < n; p = q++) {
                    Vector2 pval = _points[p];
                    Vector2 qval = _points[q];
                    A += pval.x * qval.y - qval.x * pval.y;
                }
                return (A * 0.5f);
            }

            private bool Snip(int u, int v, int w, int n, int[] V) {
                int p;
                Vector2 A = _points[V[u]];
                Vector2 B = _points[V[v]];
                Vector2 C = _points[V[w]];
                if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                    return false;
                for (p = 0; p < n; p++) {
                    if ((p == u) || (p == v) || (p == w))
                        continue;
                    Vector2 P = _points[V[p]];
                    if (InsideTriangle(A, B, C, P))
                        return false;
                }
                return true;
            }

            private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
                float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
                float cCROSSap, bCROSScp, aCROSSbp;
                ax = C.x - B.x;
                ay = C.y - B.y;
                bx = A.x - C.x;
                by = A.y - C.y;
                cx = B.x - A.x;
                cy = B.y - A.y;
                apx = P.x - A.x;
                apy = P.y - A.y;
                bpx = P.x - B.x;
                bpy = P.y - B.y;
                cpx = P.x - C.x;
                cpy = P.y - C.y;
                aCROSSbp = ax * bpy - ay * bpx;
                cCROSSap = cx * apy - cy * apx;
                bCROSScp = bx * cpy - by * cpx;
                return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
            }
        }
    }
}