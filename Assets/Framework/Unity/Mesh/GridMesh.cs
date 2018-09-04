//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//namespace PixelComrades {
//    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter)), ExecuteInEditMode]
//    public class GridMesh : MonoBehaviour {
//        public static void Create(bool withCollision)
//    {
//            GameObject gridObject = new GameObject();
//            gridObject.name = "Grid";
//            gridObject.transform.position = Vector3.zero;
 
//            List<int> triangles = new List<int>();
//            List<Vector3> vertices = new List<Vector3>();
//            List<Vector2> uvs = new List<Vector2>();
 
//            List<int> collision_triangles = new List<int>();
//            List<Vector3> collision_vertices = new List<Vector3>();
 
//            int vertexIndex = 0;
//            int collisionVertexIndex = 0;
 
//            int count = 64; // n+1 grid lines (n = even number)
//            int n = count / 2; // halve count for +/- iteration
 
//            float w = 0.05f; //line width
//            float s = 1.0f; //width of space
 
//            Vector3 v1;
//            Vector3 v2;
//            Vector3 v3;
//            Vector3 v4;
 
 
//            //Collision mesh
 
//            if (withCollision)
//            {
//                v1 = new Vector3(-n, 0, -n);
//                v2 = new Vector3(-n, 0, n);
//                v3 = new Vector3(n, 0, n);
//                v4 = new Vector3(n, 0, -n);
 
//                collision_vertices.Add(v1);
//                collision_vertices.Add(v2);
//                collision_vertices.Add(v3);
//                collision_vertices.Add(v4);
 
//                collision_triangles.Add(collisionVertexIndex);
//                collision_triangles.Add((collisionVertexIndex + 1));
//                collision_triangles.Add((collisionVertexIndex + 2));
//                collision_triangles.Add((collisionVertexIndex + 2));
//                collision_triangles.Add((collisionVertexIndex + 3));
//                collision_triangles.Add(collisionVertexIndex);
 
//            }
 
 
//            //Grid mesh
//            for (int x = -n; x < n + 1; x++)
//            {
 
//                v1 = new Vector3((x * s), 0, -n);
//                v2 = new Vector3((x * s), 0, n);
//                v3 = new Vector3((x * s) + w, 0, n);
//                v4 = new Vector3((x * s) + w, 0, -n);
 
//                vertexIndex = vertices.Count;
 
//                vertices.Add(v1);
//                vertices.Add(v2);
//                vertices.Add(v3);
//                vertices.Add(v4);
 
//                triangles.Add(vertexIndex);
//                triangles.Add((vertexIndex + 1));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 3));
//                triangles.Add(vertexIndex);
 
//                uvs.AddRange(Mats.Generic);
 
//                //back face
//                vertexIndex = vertices.Count;
 
//                vertices.Add(v4);
//                vertices.Add(v3);
//                vertices.Add(v2);
//                vertices.Add(v1);
 
//                triangles.Add(vertexIndex);
//                triangles.Add((vertexIndex + 1));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 3));
//                triangles.Add(vertexIndex);
 
//                uvs.AddRange(Mats.Generic);
 
//            }
 
 
//            for (int z = -n; z < n + 1; z++)
//            {
 
//                v1 = new Vector3(-n, 0, (z * s));
//                v2 = new Vector3(n, 0, (z * s));
//                v3 = new Vector3(n, 0, (z * s) + w);
//                v4 = new Vector3(-n, 0, (z * s) + w);
 
//                vertexIndex = vertices.Count;
 
//                vertices.Add(v1);
//                vertices.Add(v2);
//                vertices.Add(v3);
//                vertices.Add(v4);
 
//                triangles.Add(vertexIndex);
//                triangles.Add((vertexIndex + 1));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 3));
//                triangles.Add(vertexIndex);
 
//                uvs.AddRange(Mats.Generic);
 
//                //back face
//                vertexIndex = vertices.Count;
 
//                vertices.Add(v4);
//                vertices.Add(v3);
//                vertices.Add(v2);
//                vertices.Add(v1);
 
//                triangles.Add(vertexIndex);
//                triangles.Add((vertexIndex + 1));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 2));
//                triangles.Add((vertexIndex + 3));
//                triangles.Add(vertexIndex);
 
//                uvs.AddRange(Mats.Generic);
 
//            }
 
//            Mesh mesh = new Mesh();
//            mesh.vertices = vertices.ToArray();
//            mesh.triangles = triangles.ToArray();
//            mesh.uv = uvs.ToArray();
 
//            mesh.RecalculateNormals();
//            ;
 
//            MeshFilter meshFilter = gridObject.AddComponent<MeshFilter>();
//            MeshRenderer meshRenderer = gridObject.AddComponent<MeshRenderer>();
 
//            if (withCollision)
//            {
//                MeshCollider meshCollider = gridObject.AddComponent<MeshCollider>();
//                Mesh collision_mesh = new Mesh();
//                collision_mesh.vertices = collision_vertices.ToArray();
//                collision_mesh.triangles = collision_triangles.ToArray();
//                meshCollider.sharedMesh = collision_mesh;
//            }
 
//            meshRenderer.material = Mats.Grid();
//            meshFilter.sharedMesh = mesh;
 
//        }
//    }
//}
