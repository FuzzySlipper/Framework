using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PixelComrades {

    public class MeshRaycastHit {
        public Vector2 barycentricCoordinate;
        public float distance;
        public Vector3 point;
        public Vector2 textureCoord;
        public Transform transform;

        public MeshRaycastHit() {
            distance = 0f;
            transform = null;
            textureCoord = Vector2.zero;
            barycentricCoordinate = Vector2.zero;
            point = Vector3.zero;
        }
    }

    public class MeshRaycast {

        private static Vector3 _edge1;
        private static Vector3 _edge2;
        private static Vector3 _tVec;
        private static Vector3 _pVec;
        private static Vector3 _qVec;

        private static float _det;
        private static float _invDet;
        private static float _u;
        private static float _v;

        private static float _epsilon = 0.0000001f;
        private static Stopwatch _stopWatch;

        private static CircularBuffer<MeshRaycastHit> _hitsBuffer = new CircularBuffer<MeshRaycastHit>(50, ClearHit);

        public static bool DebugCast = false;
        public static string IntersectionErrorType = "";

        private static void ClearHit(MeshRaycastHit hit) {
            hit.distance = 0f;
            hit.transform = null;
            hit.textureCoord = Vector2.zero;
            hit.barycentricCoordinate = Vector2.zero;
            hit.point = Vector3.zero;
        }

        private static MeshRaycastHit GetHit(Transform transform, float distance, Vector2 barycentricCoordinate) {
            var hit = _hitsBuffer.New();
            hit.distance = distance;
            hit.transform = transform;
            hit.barycentricCoordinate = barycentricCoordinate;
            hit.textureCoord = Vector2.zero;
            hit.point = Vector3.zero;
            return hit;
        }

        private static MeshRaycastHit BuildRaycastHit(Triangle hitTriangle, float distance, Vector2 barycentricCoordinate) {
            var returnedHit = GetHit(hitTriangle.Tr, distance, barycentricCoordinate);
            returnedHit.textureCoord = hitTriangle.Uv0 + (hitTriangle.Uv1 - hitTriangle.Uv0) * barycentricCoordinate.x + (hitTriangle.Uv2 - hitTriangle.Uv0) * barycentricCoordinate.y;
            returnedHit.point = hitTriangle.Pt0 + (hitTriangle.Pt1 - hitTriangle.Pt0) * barycentricCoordinate.x + (hitTriangle.Pt2 - hitTriangle.Pt0) * barycentricCoordinate.y;
            return returnedHit;
        }

        private static List<MeshRaycastHit> CheckRaycastAll(Ray ray, Octree octree) {
            if (DebugCast) {
                _stopWatch = new Stopwatch();
                _stopWatch.Start();
            }
            var hits = new List<MeshRaycastHit>();
            RecurseOctreeBounds(octree, ray, ref hits);
            hits = SortResults(hits);
            if (DebugCast) {
                _stopWatch.Stop();
                UnityEngine.Debug.Log("Search Time: " + _stopWatch.ElapsedMilliseconds + " ms");
            }
            return hits;
        }

        private static void RecurseOctreeBounds(Octree octree, Ray ray, ref List<MeshRaycastHit> hits) {
            if (octree.Bounds.IntersectRay(ray) || octree.Bounds.Contains(ray.origin)) {
                for (int i = 0; i < octree.Triangles.Count; i++) {
                    float dist;
                    Vector2 baryCoord;
                    if (TestIntersection(octree.Triangles[i], ray, out dist, out baryCoord)) {
                        hits.Add(BuildRaycastHit(octree.Triangles[i], dist, baryCoord));
                    }
                }
            }
            for (int i = 0; i < octree.Children.Count; i++) {
                RecurseOctreeBounds(octree.Children[i], ray, ref hits);
            }
        }

        private static List<MeshRaycastHit> SortResults(List<MeshRaycastHit> input) {
            for (int write = 0; write < input.Count; write++) {
                for (int sort = 0; sort < input.Count - 1; sort++) {
                    if (input[sort].distance > input[sort + 1].distance) {
                        var lesser = input[sort + 1];
                        input[sort + 1] = input[sort];
                        input[sort] = lesser;
                    }
                }
            }
            return input;
        }

        private static bool TestIntersection(Triangle triangle, Ray ray, out float dist, out Vector2 baryCoord) {
            baryCoord = Vector2.zero;
            dist = Mathf.Infinity;
            _edge1 = triangle.Pt1 - triangle.Pt0;
            _edge2 = triangle.Pt2 - triangle.Pt0;
            _pVec = Vector3.Cross(ray.direction, _edge2);
            _det = Vector3.Dot(_edge1, _pVec);
            if (_det < _epsilon) {
                IntersectionErrorType = "Failed Epsilon";
                return false;
            }
            _tVec = ray.origin - triangle.Pt0;
            _u = Vector3.Dot(_tVec, _pVec);
            if (_u < 0 || _u > _det) {
                IntersectionErrorType = "Failed Dot1";
                return false;
            }
            _qVec = Vector3.Cross(_tVec, _edge1);
            _v = Vector3.Dot(ray.direction, _qVec);
            if (_v < 0 || _u + _v > _det) {
                IntersectionErrorType = "Failed Dot2";
                return false;
            }
            dist = Vector3.Dot(_edge2, _qVec);
            _invDet = 1 / _det;
            dist *= _invDet;
            baryCoord.x = _u * _invDet;
            baryCoord.y = _v * _invDet;
            return true;
        }

        public static MeshRaycastHit Raycast(Ray ray, Octree parent) {
            var hits = CheckRaycastAll(ray, parent);
            hits = SortResults(hits);
            if (hits.Count > 0) {
                return hits[0];
            }
            return null;
        }

        public static MeshRaycastHit[] RaycastAll(Ray ray, Octree parentOctree) {
            return CheckRaycastAll(ray, parentOctree).ToArray();
        }

        public static MeshRaycastHit[] RaycastAll(Ray ray, float dist, LayerMask mask, Octree parentOctree) {
            var hits = CheckRaycastAll(ray, parentOctree);
            for (var i = 0; i < hits.Count; i++) {
                if (hits[i].distance > dist)
                    hits.RemoveAt(i);
                if (((1 << hits[i].transform.gameObject.layer) & mask.value) != 1 << hits[i].transform.gameObject.layer)
                    hits.RemoveAt(i);
            }
            return hits.ToArray();
        }
    }
}
