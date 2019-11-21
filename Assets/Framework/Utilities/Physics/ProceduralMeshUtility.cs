using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class ProceduralMeshUtility {
        public static void BuildBox(Vector3[] verts, ref List<Vector3> allVerts, ref List<int> allTri) {
            
            int[] triangles = {
                allVerts.Count + 0, allVerts.Count + 2, allVerts.Count + 1, //face front
                allVerts.Count + 0, allVerts.Count + 3, allVerts.Count + 2,
                allVerts.Count + 2, allVerts.Count + 3, allVerts.Count + 4, //face top
                allVerts.Count + 2, allVerts.Count + 4, allVerts.Count + 5,
                allVerts.Count + 1, allVerts.Count + 2, allVerts.Count + 5, //face right
                allVerts.Count + 1, allVerts.Count + 5, allVerts.Count + 6,
                allVerts.Count + 0, allVerts.Count + 7, allVerts.Count + 4, //face left
                allVerts.Count + 0, allVerts.Count + 4, allVerts.Count + 3,
                allVerts.Count + 5, allVerts.Count + 4, allVerts.Count + 7, //face back
                allVerts.Count + 5, allVerts.Count + 7, allVerts.Count + 6,
                allVerts.Count + 0, allVerts.Count + 6, allVerts.Count + 7, //face bottom
                allVerts.Count + 0, allVerts.Count + 1, allVerts.Count + 6
            };

            //Vector2[] uvs = {
            //    new Vector2(0, 0.66f),
            //    new Vector2(0.25f, 0.66f),
            //    new Vector2(0, 0.33f),
            //    new Vector2(0.25f, 0.33f),

            //    new Vector2(0.5f, 0.66f),
            //    new Vector2(0.5f, 0.33f),
            //    new Vector2(0.75f, 0.66f),
            //    new Vector2(0.75f, 0.33f),

            //    new Vector2(1, 0.66f),
            //    new Vector2(1, 0.33f),

            //    new Vector2(0.25f, 1),
            //    new Vector2(0.5f, 1),

            //    new Vector2(0.25f, 0),
            //    new Vector2(0.5f, 0),
            //};	
            allVerts.AddRange(verts);
            allTri.AddRange(triangles);
            //allUvs.AddRange(uvs);
        }

        private static float _pi = Mathf.PI;
        private static float _2Pi = _pi * 2f;
        private const int SphereLong = 24;
        private const int SphereLat = 16;

        public static void BuildSphere(Vector3 center, float radius, ref List<Vector3> allVerts, ref List<int> allTri) {
            Vector3[] verts = new Vector3[(SphereLong+1) * SphereLat + 2];
            verts[0] = Vector3.up * radius;
            for( int lat = 0; lat < SphereLat; lat++ ) {
	            float a1 = _pi * (float)(lat+1) / (SphereLat+1);
	            float sin1 = Mathf.Sin(a1);
	            float cos1 = Mathf.Cos(a1);
             
	            for( int lon = 0; lon <= SphereLong; lon++ ) {
		            float a2 = _2Pi * (float)(lon == SphereLong ? 0 : lon) / SphereLong;
		            float sin2 = Mathf.Sin(a2);
		            float cos2 = Mathf.Cos(a2);
             
		            verts[ lon + lat * (SphereLong + 1) + 1] = center + new Vector3( sin1 * cos2, cos1, sin1 * sin2 ) * radius;
	            }
            }
            verts[verts.Length-1] = center + Vector3.up * -radius;
            //Vector3[] normales = new Vector3[vertices.Length];
            //for( int n = 0; n < vertices.Length; n++ )
	           // normales[n] = vertices[n].normalized;
             
            //Vector2[] uvs = new Vector2[vertices.Length];
            //uvs[0] = Vector2.up;
            //uvs[uvs.Length-1] = Vector2.zero;
            //for( int lat = 0; lat < nbLat; lat++ )
	           // for( int lon = 0; lon <= nbLong; lon++ )
		          //  uvs[lon + lat * (nbLong + 1) + 1] = new Vector2( (float)lon / nbLong, 1f - (float)(lat+1) / (nbLat+1) );
             
            int nbFaces = verts.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[ nbIndexes ];
            var triStart = allVerts.Count;
            //Top Cap
            int i = 0;
            for( int lon = 0; lon < SphereLong; lon++ ) {
	            triangles[i++] = triStart + lon+2;
	            triangles[i++] = triStart + lon+1;
	            triangles[i++] = triStart + 0;
            }
             
            //Middle
            for( int lat = 0; lat < SphereLat - 1; lat++ ) {
	            for( int lon = 0; lon < SphereLong; lon++ ) {
		            int current = lon + lat * (SphereLong + 1) + 1;
		            int next = current + SphereLong + 1;
             
		            triangles[i++] = triStart + current;
		            triangles[i++] = triStart + current + 1;
		            triangles[i++] = triStart + next + 1;
             
		            triangles[i++] = triStart + current;
		            triangles[i++] = triStart + next + 1;
		            triangles[i++] = triStart + next;
	            }
            }
            //Bottom Cap
            for( int lon = 0; lon < SphereLong; lon++ ) {
	            triangles[i++] = triStart + verts.Length - 1;
	            triangles[i++] = triStart + verts.Length - (lon+2) - 1;
	            triangles[i++] = triStart + verts.Length - (lon+1) - 1;
            }
            allVerts.AddRange(verts);
            allTri.AddRange(triangles);
        }

        // make segments an even number
        private const int CapsuleSegments = 24;

        public static void BuildCapsule(Transform tr, float height, float radius, ref List<Vector3> allVerts, ref List<int> allTri) {
		    int points = CapsuleSegments + 1;
            var triStart = allVerts.Count;
		    // calculate points around a circle
		    float[] pX = new float[ points ];
		    float[] pZ = new float[ points ];
		    float[] pY = new float[ points ];
		    float[] pR = new float[ points ];
		    
		    float calcH = 0f;
		    float calcV = 0f;
		    
		    for ( int i = 0; i < points; i ++ ) {
			    pX[ i ] = Mathf.Sin( calcH * Mathf.Deg2Rad ); 
			    pZ[ i ] = Mathf.Cos( calcH * Mathf.Deg2Rad );
			    pY[ i ] = Mathf.Cos( calcV * Mathf.Deg2Rad ); 
			    pR[ i ] = Mathf.Sin( calcV * Mathf.Deg2Rad ); 
			    
			    calcH += 360f / (float)CapsuleSegments;
			    calcV += 180f / (float)CapsuleSegments;
		    }
		    
		    Vector3[] vertices = new Vector3[ points * ( points + 1 ) ];
		    //Vector2[] uvs = new Vector2[ vertices.Length ];
		    int ind = 0;
		    
		    // Y-offset is half the height minus the diameter
		    float yOff = ( height - ( radius * 2f ) ) * 0.5f;
		    if ( yOff < 0 )
			    yOff = 0;
		    
		    // uv calculations
		    //float stepX = 1f / ( (float)(points - 1) );
		    //float uvX, uvY;
		    
		    // Top Hemisphere
		    int top = Mathf.CeilToInt( (float)points * 0.5f );
		    
		    for ( int y = 0; y < top; y ++ ) {
			    for ( int x = 0; x < points; x ++ ) {
				    vertices[ ind ] = new Vector3( pX[ x ] * pR[ y ], pY[ y ], pZ[ x ] * pR[ y ] ) * radius;
				    vertices[ ind ].y = yOff + vertices[ ind ].y;
				    //uvX = 1f - ( stepX * (float)x );
				    //uvY = ( vertices[ ind ].y + ( height * 0.5f ) ) / height;
				    //uvs[ ind ] = new Vector2( uvX, uvY );
				    ind ++;
			    }
		    }
		    
		    // Bottom Hemisphere
		    int btm = Mathf.FloorToInt( (float)points * 0.5f );
		    
		    for ( int y = btm; y < points; y ++ ) {
			    for ( int x = 0; x < points; x ++ ) {
				    vertices[ ind ] = new Vector3( pX[ x ] * pR[ y ], pY[ y ], pZ[ x ] * pR[ y ] ) * radius;
				    vertices[ ind ].y = -yOff + vertices[ ind ].y;
				    //uvX = 1f - ( stepX * (float)x );
				    //uvY = ( vertices[ ind ].y + ( height * 0.5f ) ) / height;
				    //uvs[ ind ] = new Vector2( uvX, uvY );
				    ind ++;
			    }
		    }

            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = tr.TransformPoint(vertices[i]);
            }

		    int[] triangles = new int[ ( CapsuleSegments * (CapsuleSegments + 1) * 2 * 3 ) ];
		    for ( int y = 0, t = 0; y < CapsuleSegments + 1; y ++ ) {
			    for ( int x = 0; x < CapsuleSegments; x ++, t += 6 ) {
				    triangles[ t + 0 ] = triStart + ( (y + 0) * ( CapsuleSegments + 1 ) ) + x + 0;
				    triangles[ t + 1 ] = triStart + ( (y + 1) * ( CapsuleSegments + 1 ) ) + x + 0;
				    triangles[ t + 2 ] = triStart + ( (y + 1) * ( CapsuleSegments + 1 ) ) + x + 1;
				    triangles[ t + 3 ] = triStart + ( (y + 0) * ( CapsuleSegments + 1 ) ) + x + 1;
				    triangles[ t + 4 ] = triStart + ( (y + 0) * ( CapsuleSegments + 1 ) ) + x + 0;
				    triangles[ t + 5 ] = triStart + ( (y + 1) * ( CapsuleSegments + 1 ) ) + x + 1;
			    }
		    }
            allVerts.AddRange(vertices);
            allTri.AddRange(triangles);
        }


        public static void AddMeshVerts(Transform tr, Mesh mesh, ref List<Vector3> allVerts, ref List<int> allTri) {
            var verts = mesh.vertices;
            var tri = mesh.triangles;
            var triStart = allVerts.Count;
            for (int i = 0; i < verts.Length; i++) {
                allVerts.Add(tr.TransformPoint(verts[i]));
            }
            for (int i = 0; i < tri.Length; i++) {
                allTri.Add(triStart + tri[i]);
            }
        }

        private static Vector3 _quadNormal = -Vector3.forward;
        private static Vector4 _flatTangent = new Vector4(1f, 0f, 0f, -1f);
        
        public static Mesh GenerateQuad(Vector2 size, Vector2 pivot) {
            Vector2 scaledPivot = size * pivot;
            Vector3[] vertices = {
                new Vector3(size.x - scaledPivot.x, size.y - scaledPivot.y, 0),
                new Vector3(size.x - scaledPivot.x, -scaledPivot.y, 0),
                new Vector3(-scaledPivot.x, -scaledPivot.y, 0),
                new Vector3(-scaledPivot.x, size.y - scaledPivot.y, 0),
            };

            Vector2[] uv = {
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1)
            };

            Vector3[] normals = {
                _quadNormal,
                _quadNormal,
                _quadNormal,
                _quadNormal
            };
            
            Vector4[] tangents = {
                _flatTangent,
                _flatTangent,
                _flatTangent,
                _flatTangent
            };

            int[] triangles = {
                0, 1, 2,
                2, 3, 0
            };
            
            var mesh = new Mesh {
                vertices = vertices,
                uv = uv,
                triangles = triangles,
                normals = normals,
                tangents = tangents
            };
            mesh.RecalculateBounds();
            var boundsSize = mesh.bounds.size;
            boundsSize.z = Mathf.Max(boundsSize.z, 1);
            mesh.bounds = new Bounds(mesh.bounds.center, boundsSize * 3);
            return mesh;
        }

        public static Mesh GenerateDoubleSidedQuad(Vector2 size, Vector2 pivot) {
            Vector2 scaledPivot = size * pivot;
            Vector3[] vertices = {
                new Vector3(size.x - scaledPivot.x, size.y - scaledPivot.y, 0),
                new Vector3(size.x - scaledPivot.x, -scaledPivot.y, 0),
                new Vector3(-scaledPivot.x, -scaledPivot.y, 0),
                new Vector3(-scaledPivot.x, size.y - scaledPivot.y, 0),
                new Vector3(size.x - scaledPivot.x, size.y - scaledPivot.y, 0),
                new Vector3(size.x - scaledPivot.x, -scaledPivot.y, 0),
                new Vector3(-scaledPivot.x, -scaledPivot.y, 0),
                new Vector3(-scaledPivot.x, size.y - scaledPivot.y, 0),
            };

            Vector2[] uv = {
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1)
            };

            Vector3[] normals = {
                _quadNormal,
                _quadNormal,
                _quadNormal,
                _quadNormal,
                -_quadNormal,
                -_quadNormal,
                -_quadNormal,
                -_quadNormal
            };

            Vector4[] tangents = {
                _flatTangent,
                _flatTangent,
                _flatTangent,
                _flatTangent,
                -_flatTangent,
                -_flatTangent,
                -_flatTangent,
                -_flatTangent
            };

            int[] triangles = {
                0, 1, 2,
                2, 3, 0,
                4, 5, 6,
                6, 7, 4
            };

            return new Mesh {
                vertices = vertices,
                uv = uv,
                triangles = triangles,
                normals = normals,
                tangents = tangents
            };

        }

        public static Mesh BuildQuadMesh(int width, int height) {
            var mesh = new Mesh();
            var vertices = new Vector3[4];

            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(width, 0, 0);
            vertices[2] = new Vector3(0, height, 0);
            vertices[3] = new Vector3(width, height, 0);

            mesh.vertices = vertices;

            var tri = new int[6];

            tri[0] = 0;
            tri[1] = 2;
            tri[2] = 1;

            tri[3] = 2;
            tri[4] = 3;
            tri[5] = 1;

            mesh.triangles = tri;

            var normals = new Vector3[4];

            normals[0] = -Vector3.forward;
            normals[1] = -Vector3.forward;
            normals[2] = -Vector3.forward;
            normals[3] = -Vector3.forward;

            mesh.normals = normals;

            var uv = new Vector2[4];

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);

            mesh.uv = uv;
            return mesh;
        }
    }
    static public class MeshColliderTools {
     
        public static void SnapToGrid(this Mesh mesh, float gridDelta) {
            if (gridDelta < 1e-5f)
                return;
            float inverse = 1f / gridDelta;
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++) {
                verts[i].x = Mathf.RoundToInt(verts[i].x*inverse) / inverse;
                verts[i].y = Mathf.RoundToInt(verts[i].y*inverse) / inverse;
                verts[i].z = Mathf.RoundToInt(verts[i].z*inverse) / inverse;
            }
            mesh.vertices = verts;
        }
        public static void Weld (this Mesh mesh, float bucketStep) {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            int[] old2New = new int[oldVertices.Length];
            int newSize = 0;
     
            // Find AABB
            Vector3 min = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3 (float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < oldVertices.Length; i++) {
                if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
                if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
                if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
                if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
                if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
                if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
            }
            min -= Vector3.one * 0.111111f;
            max += Vector3.one * 0.899999f;
     
            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt ((max.x - min.x) / bucketStep) + 1;
            int bucketSizeY = Mathf.FloorToInt ((max.y - min.y) / bucketStep) + 1;
            int bucketSizeZ = Mathf.FloorToInt ((max.z - min.z) / bucketStep) + 1;
            if (bucketSizeX + bucketSizeY + bucketSizeZ < -14000) {
                return;
            }
            List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];
     
            // Make new vertices
            for (int i = 0; i < oldVertices.Length; i++) {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt ((oldVertices[i].x - min.x) / bucketStep);
                int y = Mathf.FloorToInt ((oldVertices[i].y - min.y) / bucketStep);
                int z = Mathf.FloorToInt ((oldVertices[i].z - min.z) / bucketStep);
     
                // Check to see if it's already been added
                if (buckets[x, y, z] == null)
                    buckets[x, y, z] = new List<int> (); // Make buckets lazily
     
                for (int j = 0; j < buckets[x, y, z].Count; j++) {
                    Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                    if (Vector3.SqrMagnitude (to) < 0.001f) {
                        old2New[i] = buckets[x, y, z][j];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }
     
                // Add new vertex
                newVertices[newSize] = oldVertices[i];
                buckets[x, y, z].Add (newSize);
                old2New[i] = newSize;
                newSize++;
     
                skip:;
            }
     
            // Make new triangles
            int[] oldTris = mesh.triangles;
            int[] newTris = new int[oldTris.Length];
            for (int i = 0; i < oldTris.Length; i++) {
                newTris[i] = old2New[oldTris[i]];
            }
     
            Vector3[] finalVertices = new Vector3[newSize];
            for (int i = 0; i < newSize; i++)
                finalVertices[i] = newVertices[i];
     
            mesh.Clear();
            mesh.vertices = finalVertices;
            mesh.triangles = newTris;
     
            // Debug.LogFormat("Weld vert count: {0} vs. {1}", newSize, oldVertices.Length);
        }
     
     
        public static void Simplify(this MeshCollider meshCollider) { meshCollider.sharedMesh.Simplify(); }
        public static void Simplify(this Mesh mesh) {
            var verts = mesh.vertices;
            var origNumVerts = verts.Length;
     
            var workingSet = new List<Vertice>(origNumVerts);
            for (int i = 0; i < origNumVerts; i++) {
                var r = new Vertice();
                r.position = verts[i];
                workingSet.Add(r);
            }
     
            var tris = mesh.triangles;
            var triLength = tris.Length;
            for (int i = 0; i < triLength; i+=3)
                Face.AddFace(workingSet, tris, i);
     
            for (int i = 0; i < origNumVerts; i++)
                workingSet[i].AssignLinearPosition();
     
     
            /*********************************
             *                               *
             *         Simplify mesh!        *
             *                               *
             ********************************/
     
            HashSet<Vertice> candidates;
            HashSet<Vertice> nextCandidates  = new HashSet<Vertice>();
     
            foreach (Vertice v in workingSet) if (!v.IsStatic)
                nextCandidates.Add(v);
     
            while (nextCandidates.Count != 0) {
                candidates = nextCandidates;
                nextCandidates = new HashSet<Vertice>();
                foreach (Vertice a in candidates) {
                    if (a.edges != null) foreach (Edge ac in a.edges) {
                        Vertice c;
                        if (a.CanFollow(ac, out c)) {
                            foreach (Face f in ac.faces)
                                Edge.Collapse(f.GetOpposite(c), f.GetOpposite(a), f);
                            foreach (Edge edge_of_a in a.edges) {
                                if (edge_of_a != ac) {
                                    var o = edge_of_a.GetOpposite(a);
                                    if (!o.IsStatic) nextCandidates.Add(o);
                                    edge_of_a.Reconnect(a, c);
                                }
                            }
                            if (!c.IsStatic) nextCandidates.Add(c);
     
                            c.DisconnectFrom(ac);
                            a.Disconnect();
                            ac.DisconnectIncludingFaces();
                            break;
                        }
                    }
                }
            }
     
            var simplifiedVerts = new List<Vertice>();
            foreach (Vertice v in workingSet) if (v.edges != null)
                simplifiedVerts.Add(v);
     
            var simplifiedNumVerts = simplifiedVerts.Count;
            var newPositions = new Vector3[simplifiedNumVerts];
            //var resultColors = new Color[simplifiedNumVerts];
            for (int i = 0; i < simplifiedNumVerts; i++) {
                simplifiedVerts[i].finalIndex = i;
                newPositions[i] = simplifiedVerts[i].position;
            }
     
            var resultTris = new List<int>();
            var triSet = new HashSet<Face>();
            foreach (Vertice v in simplifiedVerts) {
                foreach (Edge e in v.edges) {
                    foreach (Face f in e.faces) if (!triSet.Contains(f)) {
                        triSet.Add(f);
                        f.GetIndexes(resultTris);
                    }
                }
            }
     
            mesh.Clear();
            mesh.vertices = newPositions;
            mesh.triangles = resultTris.ToArray();
     
            mesh.RecalculateBounds();
            //mesh.Optimize();
     
            // Debug.LogFormat("Simplify vert count: {0} vs. {1}", simplifiedNumVerts, origNumVerts);
        }
     
        private class Vertice {
            public List<Edge> edges = new List<Edge>();
            public Vector3 position;
     
            /// <summary>A cache used for identifying a vertice during mesh reconstruction.
            public int finalIndex;
     
            /// <summary>
            ///  - Null if vertice is internal in a plane
            ///  - A non-zero vector if vertice is internal in a line
            ///  - Vector3.zero otherwise.
            /// </summary>
            public Vector3? linearPosition;
     
            public void AssignLinearPosition() {
                for (int i = 0; i < edges.Count; i++) {
                    var edge = edges[i];
                    if (!edge.HasEqualPlanes()) {
                        if (linearPosition == null)
                            linearPosition = edge.vertices[1].position - edge.vertices[0].position;
                        else if (!edge.IsParallel(linearPosition)) {
                            linearPosition = Vector3.zero;
                            break;
                        }
                    }
                }
            }
     
            public bool IsStatic { get {
                return linearPosition == Vector3.zero;
            } }
     
            public Edge GetExistingConnectingEdge(Vertice v) {
                foreach (Edge e in edges) {
                    if (e.vertices[0] == this && e.vertices[1] == v) return e;
                    if (e.vertices[1] == this && e.vertices[0] == v) return e;
                }
                return null;
            }
     
            public Edge GetConnectingEdge(Vertice v) {
                Edge result = GetExistingConnectingEdge(v);
                if (result == null) {
                    result = new Edge(this, v);
                    edges.Add(result);
                    v.edges.Add(result);
                }
                return result;
            }
     
            /// <summary>When collapsing an edge a->c, the linear space must
            /// be respected, and all faces, not connected with a->c,
            /// must not flip.
            /// </summary>
            public bool CanFollow(Edge transportEdge, out Vertice opposite) {
                if (IsStatic) { opposite = default(Vertice); return false; }
                if (linearPosition != null && !transportEdge.IsParallel(linearPosition)) { opposite = default(Vertice); return false; }
     
                var localTris = new HashSet<Face>();
                foreach (Edge e in edges) foreach (Face f in e.faces)
                    localTris.Add(f);
     
                localTris.ExceptWith(transportEdge.faces);
     
                opposite = transportEdge.GetOpposite(this);
                var targetPos = opposite.position;
                var lTriEnum = localTris.GetEnumerator();
                try {
                    while (lTriEnum.MoveNext())
                        if (lTriEnum.Current.MoveWouldFlip(this, targetPos))
                            return false;
                } finally { lTriEnum.Dispose(); }
                return true;
            }
     
            public void DisconnectFrom(Edge e) {
                edges.Remove(e);
            }
     
            public void Disconnect() {
                edges.Clear();
                edges = null;
            }
     
        }
     
        private class Edge {
            public List<Vertice> vertices;
            public List<Face> faces;
     
            public static void Collapse(Edge moved, Edge target, Face f) {
                Face faceOutsideMoved;
                try { faceOutsideMoved = moved.GetOpposite(f); }
                catch (Exception e) {
                    throw new Exception(e.Message + "\n" + moved.vertices[0].position);
                }
                faceOutsideMoved.Replace(moved, target);
                target.Replace(f, faceOutsideMoved);
                foreach (Vertice v in moved.vertices) {
                    v.edges.Remove(moved);
                }
            }
     
            public Edge(Vertice v0, Vertice v1) {
                vertices = new List<Vertice>(2);
                vertices.Add(v0);
                vertices.Add(v1);
                faces = new List<Face>(2);
            }
     
            public Vertice GetOpposite(Vertice v) {
                var v0 = vertices[0];
                return v != v0 ? v0 : vertices[1];
            }
     
            public Face GetOpposite(Face v) {
                if (faces.Count != 2) throw new Exception("Collapsing an edge with only 1 face into another. This is not supported.");
                var face0 = faces[0];
                return face0 == v ? faces[1] : face0;
            }
     
            public bool HasEqualPlanes() {
                if (faces.Count != 2) return false;
                var f0   = faces[0];
                var f0e0 = faces[0].edges[0];
     
                var f1   = faces[1];
                var f1e0 = faces[1].edges[0];
     
                var e0 = f0e0 != this ? f0e0 : f0.edges[1];
                var e1 = f1e0 != this ? f1e0 : f1.edges[1];
     
                var v0 =    vertices[1].position -    vertices[0].position;
                var v1 = e0.vertices[1].position - e0.vertices[0].position;
                var v2 = e1.vertices[1].position - e1.vertices[0].position;
     
                var n0 = Vector3.Cross(v0, v1);
     
                var dot = Vector3.Dot(n0, v2);
                return -5e-3 < dot && dot < 5e-3;
            }
     
            public void Replace (Face oldFace, Face newFace) {
                for (int j = 0; j < faces.Count; j++) {
                    if (faces[j] == oldFace) {
                        faces[j] = newFace;
                        return;
                    }
                }
            }
     
            public void Reconnect(Vertice oldVertice, Vertice newVertice) {
                if (vertices[0] == oldVertice) vertices[0] = newVertice;
                else                           vertices[1] = newVertice;
                newVertice.edges.Add(this);
            }
     
            public bool Contains(Vertice v) {
                return v == vertices[0] || v == vertices[1];
            }
     
            public bool IsParallel(Vector3? nv) {
                var v0 = vertices[0].position;
                var v1 = vertices[1].position;
                float cross = Vector3.Cross(v1 - v0, nv.Value).sqrMagnitude;
                return -5e-6f < cross && cross < 5e-6f;
            }
     
            public void DisconnectIncludingFaces() {
                vertices.Clear();
                vertices = null;
                foreach (Face f in faces)
                    f.Disconnect();
                faces.Clear();
                faces = null;
            }
        }
     
        private class Face {
            public Edge[] edges;
     
            /*
             * <paramref name="e0">Edge between v0 and v1</paramref>
             * <paramref name="e1">Edge between v0 and v2</paramref>
             * <paramref name="e2">Edge between v1 and v2</paramref>
             */
            private Face(Edge e0, Edge e1, Edge e2) {
                edges = new Edge[] { e0, e1, e2 };
            }
     
            /*
             * <paramref name="allVertices">List of vertices, in the same order as 'verts' from the mesh.</paramref>
             * <paramref name="tris">The tris array from the mesh.</paramref>
             * <paramref name="triIndex">The index of the first vertex in this triangle.</paramref>
             */
            public static void AddFace(List<Vertice> allVertices, int[] tris, int triIndex) {
                var v0 = allVertices[tris[triIndex+0]];
                var v1 = allVertices[tris[triIndex+1]];
                var v2 = allVertices[tris[triIndex+2]];
     
                var e0 = v0.GetConnectingEdge(v1);
                var e1 = v0.GetConnectingEdge(v2);
                var e2 = v1.GetConnectingEdge(v2);
     
                var face = new Face(e0, e1, e2);
     
                e0.faces.Add(face);
                e1.faces.Add(face);
                e2.faces.Add(face);
            }
     
            public Edge GetOpposite(Vertice v) {
                Edge e0, e1;
                if (!(e0 = edges[0]).Contains(v)) return e0;
                if (!(e1 = edges[1]).Contains(v)) return e1;
                return edges[2];
            }
     
            public Vertice GetOpposite(Edge o) {
                var o0 = o.vertices[0];
                var o1 = o.vertices[1];
                for (int i = 0; i < 3; i++) { // Will never reach 3 because there will be an unequal vertice before the third edge
                    Edge e = edges[i];
                    Vertice e0 = e.vertices[0];
                    if (e0 != o0 && e0 != o1) return e0;
                    Vertice e1 = e.vertices[1];
                    if (e1 != o0 && e1 != o1) return e1;
                }
                throw new Exception("A face seems to have three edges that all share a vertice with a given edge.");
            }
     
            public void Replace (Edge oldEdge, Edge newEdge) {
                if      (edges[0] == oldEdge) edges[0] = newEdge;
                else if (edges[1] == oldEdge) edges[1] = newEdge;
                else                          edges[2] = newEdge;
            }
     
            public bool MoveWouldFlip(Vertice v, Vector3 p) {
                Edge oppositeEdge = GetOpposite(v);
                var ov0 = oppositeEdge.vertices[0].position;
                var ov  = oppositeEdge.vertices[1].position - ov0;
                var ot = p          - ov0;
                var ct = v.position - ov0;
     
                var cross0 = Vector3.Cross(ot, ov);
                var c0SqrMagnitude = cross0.sqrMagnitude;
                if (c0SqrMagnitude < 0.0001f) return true;
     
                var cross1 = Vector3.Cross(ct, ov);
                var c1SqrMagnitude = cross1.sqrMagnitude;
                if (c1SqrMagnitude < 0.0001f) return true;
     
                if (Mathf.Sign(Vector3.Dot(cross0, cross1)) < 0f) return true;
     
                return false;
            }
     
            /*
             * <summary>Adds the finalIndex for the verts in order: v0, v1, v2.</summary>
             */
            public void GetIndexes(List<int> results) {
                results.Add(GetOpposite(edges[2]).finalIndex);
                results.Add(GetOpposite(edges[1]).finalIndex);
                results.Add(GetOpposite(edges[0]).finalIndex);
            }
     
            public void Disconnect() {
                edges = null;
            }
        }
    }
}
