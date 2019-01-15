using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class Octree {

        public Bounds Bounds;
        public List<Octree> Children;
        public Octree Parent;
        public List<Triangle> Triangles;
        public Dictionary<int, List<Triangle>> MeshFilterTriangles = new Dictionary<int, List<Triangle>>();

        public Octree() {
            Children = new List<Octree>();
            Triangles = new List<Triangle>();
            Parent = null;
        }

        public Octree(Bounds parentBounds, int generations) {
            Bounds = parentBounds;
            Children = new List<Octree>();
            Triangles = new List<Triangle>();
            Parent = null;
            CreateChildren(this, generations);
        }

        protected int ClearOctree(Octree o) {
            var count = 0;
            for (var i = 0; i < o.Children.Count; i++) {
                count += ClearOctree(o.Children[i]);
            }
            o.Triangles.Clear();
            o.Triangles.TrimExcess();
            o.Parent = null;
            o.Children.Clear();
            o.Children.TrimExcess();
            count++;
            return count;
        }


        protected void CreateChildren(Octree parent, int generations) {
            Children = new List<Octree>();
            var c = parent.Bounds.center;
            var u = parent.Bounds.extents.x * 0.5f;
            var v = parent.Bounds.extents.y * 0.5f;
            var w = parent.Bounds.extents.z * 0.5f;
            var childrenSize = parent.Bounds.extents;
            Vector3[] childrenCenters = {
                new Vector3(c.x + u, c.y + v, c.z + w),
                new Vector3(c.x + u, c.y + v, c.z - w),
                new Vector3(c.x + u, c.y - v, c.z + w),
                new Vector3(c.x + u, c.y - v, c.z - w),
                new Vector3(c.x - u, c.y + v, c.z + w),
                new Vector3(c.x - u, c.y + v, c.z - w),
                new Vector3(c.x - u, c.y - v, c.z + w),
                new Vector3(c.x - u, c.y - v, c.z - w)
            };

            for (var i = 0; i < childrenCenters.Length; i++) {
                var o = new Octree();
                o.Parent = parent;
                o.Bounds = new Bounds(childrenCenters[i], childrenSize);
                Children.Add(o);
                if (generations > 0) {
                    o.CreateChildren(o, generations - 1);
                }
            }
        }

        public void AddTriangle(MeshFilter mf, Triangle t) {
            Triangles.Add(t);
            List<Triangle> triList;
            if (!MeshFilterTriangles.TryGetValue(mf.GetInstanceID(), out triList)) {
                triList = new List<Triangle>();
                MeshFilterTriangles.Add(mf.GetInstanceID(), triList);
            }
            triList.Add(t);
        }

        public void RemoveTriangles(MeshFilter mf) {
            List<Triangle> triList;
            if (MeshFilterTriangles.TryGetValue(mf.GetInstanceID(), out triList)) {
                for (int i = 0; i < triList.Count; i++) {
                    Triangles.Remove(triList[i]);
                }
                triList.Clear();
            }
            for (int i = 0; i < Children.Count; i++) {
                Children[i].RemoveTriangles(mf);
            }
        }

        public void Clear() {
            ClearOctree(this);
        }

        public bool ContainsTriangle(Triangle triangle) {
            return Bounds.Contains(triangle.Pt0) &&
                   Bounds.Contains(triangle.Pt1) &&
                   Bounds.Contains(triangle.Pt2);
        }


        public Octree GetOctree(Triangle triangle) {
            return GetOctree(this, triangle);
        }

        public Octree GetOctree(Octree parentNode, Triangle triangle) {
            // Compute triangle bounds (not using the param version of MathEx.Min() to avoid array allocation)
            var minX = MathEx.Min(triangle.Pt0.x, MathEx.Min(triangle.Pt1.x, triangle.Pt2.x));
            var minY = MathEx.Min(triangle.Pt0.y, MathEx.Min(triangle.Pt1.y, triangle.Pt2.y));
            var minZ = MathEx.Min(triangle.Pt0.z, MathEx.Min(triangle.Pt1.z, triangle.Pt2.z));

            var maxX = MathEx.Max(triangle.Pt0.x, MathEx.Max(triangle.Pt1.x, triangle.Pt2.x));
            var maxY = MathEx.Max(triangle.Pt0.y, MathEx.Max(triangle.Pt1.y, triangle.Pt2.y));
            var maxZ = MathEx.Max(triangle.Pt0.z, MathEx.Max(triangle.Pt1.z, triangle.Pt2.z));

            Octree finalNode = null;
            var currentNode = parentNode;
            while (currentNode != null && finalNode == null) {
                var boundsCenterX = currentNode.Bounds.center.x;
                var boundsCenterY = currentNode.Bounds.center.y;
                var boundsCenterZ = currentNode.Bounds.center.z;

                // Test if the triangle crosses any of the mid planes of the node
                if (minX < boundsCenterX && maxX >= boundsCenterX || minY < boundsCenterY && maxY >= boundsCenterY || minZ < boundsCenterZ && maxZ >= boundsCenterZ) {
                    // The triangle must be in the current node
                    finalNode = currentNode;
                }
                else {
                    // The triangle can be inside one of our children, if we have any
                    if (currentNode.Children != null && currentNode.Children.Count > 0) {
                        // Figure out which child based on which side of each mid plane the triangle sits on
                        var childIndex = 0;
                        if (minX < boundsCenterX)
                            childIndex |= 4;
                        if (minY < boundsCenterY)
                            childIndex |= 2;
                        if (minZ < boundsCenterZ)
                            childIndex |= 1;
                        // Continue iteration with the child node that contains the triangle
                        currentNode = currentNode.Children[childIndex];
                    }
                    else {
                        // Since we don't have children, even though the triangle *would* fit in one of our potential child,
                        // we're the node that has to own the triangle.
                        // Arguably, if you hit this code a lot, you could benefit from using more depth in your octree...
                        finalNode = currentNode;
                    }
                }
            }
            return finalNode;
        }
    }

    public class Triangle {

        public Vector3 Pt0;
        public Vector3 Pt1;
        public Vector3 Pt2;
        public Transform Tr;

        public Vector2 Uv0;
        public Vector2 Uv1;
        public Vector2 Uv2;

        private static Vector2 DefaultUV = new Vector2(0,1);

        public Triangle(Vector3 pt0, Vector3 pt1, Vector3 pt2, Vector2 uvPt0, Vector2 uvPt1, Vector2 uvPt2, Transform tr) {
            Pt0 = pt0;
            Pt1 = pt1;
            Pt2 = pt2;
            Uv0 = uvPt0;
            Uv1 = uvPt1;
            Uv2 = uvPt2;
            Tr = tr;
            UpdateVerts();
        }

        public void UpdateVerts() {
            Pt0 = Tr.TransformPoint(Pt0);
            Pt1 = Tr.TransformPoint(Pt1);
            Pt2 = Tr.TransformPoint(Pt2);
        }

        public static Triangle[] GetTriangles(MeshFilter mf) {
            var mesh = mf.sharedMesh;
            var tris = mesh.triangles;
            var verts = mesh.vertices;
            var uvs = mesh.uv;
            var triangleList = new List<Triangle>();
            var i = 0;
            while (i < tris.Length) {
                if (i + 2 >= tris.Length) {
                    break;
                }
                var idx1 = tris[i + 0];
                var idx2 = tris[i + 1];
                var idx3 = tris[i + 2];
                if (uvs == null || uvs.Length == 0) {
                    triangleList.Add(
                        new Triangle(
                            verts[Mathf.Clamp(idx1, 0, verts.Length - 1)],
                            verts[Mathf.Clamp(idx2, 0, verts.Length - 1)],
                            verts[Mathf.Clamp(idx3, 0, verts.Length - 1)],
                            DefaultUV,
                            DefaultUV,
                            DefaultUV,
                            mf.transform));
                }
                else {
                    triangleList.Add(
                        new Triangle(
                            verts[Mathf.Clamp(idx1, 0, verts.Length - 1)],
                            verts[Mathf.Clamp(idx2, 0, verts.Length - 1)],
                            verts[Mathf.Clamp(idx3, 0, verts.Length - 1)],
                            uvs[Mathf.Clamp(idx1, 0, uvs.Length - 1)],
                            uvs[Mathf.Clamp(idx2, 0, uvs.Length - 1)],
                            uvs[Mathf.Clamp(idx3, 0, uvs.Length - 1)],
                            mf.transform));
                }
                i += 3;
            }
            return triangleList.ToArray();
        }
    }
}
