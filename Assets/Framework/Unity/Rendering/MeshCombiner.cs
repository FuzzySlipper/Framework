using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.Rendering;

namespace PixelComrades {

    public static class MeshCombineUtility {
        public const int LodGroupIndex = 0;

        public static void CombineMesh(Transform source, Transform target, bool deleteOriginal, bool splitSectors) {
            var originalPos = source.transform.position;
            source.transform.position = Vector3.zero;
            MeshFilter[] allFilters = source.GetComponentsInChildren<MeshFilter>();
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            for(int i = 0; i < allFilters.Length; i++) {
                if (allFilters[i] == null || allFilters[i].transform.parent.GetComponent<LODGroup>() != null ||
                    !allFilters[i].gameObject.isStatic) {
		            continue;
		        }
                meshFilters.Add(allFilters[i]);
		    }
            Dictionary<Point3, List<MeshCombiner>> sectorMeshCombines = new Dictionary<Point3, List<MeshCombiner>>();
            List<MeshCombiner> meshCombines = null;
            MeshCombiner current = null;
            if (!splitSectors) {
                meshCombines = new List<MeshCombiner>();
                current = new MeshCombiner();
                meshCombines.Add(current);
            }
            int errorCnt = 0;
            for (int i = 0; i < meshFilters.Count; i++) {
		        var meshFilter = meshFilters[i];
				MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
		        if (meshRenderer == null) {
		            continue;
		        }
		        meshRenderer.enabled = !deleteOriginal;
		        if (splitSectors) {
		            var secPos = Game.WorldToSector(meshFilter.transform.position);
		            if (!sectorMeshCombines.TryGetValue(secPos, out meshCombines)) {
		                meshCombines = new List<MeshCombiner>();
		                sectorMeshCombines.Add(secPos, meshCombines);
		                current = new MeshCombiner();
		                meshCombines.Add(current);
		            }
		            else {
		                current = meshCombines.LastElement();
		            }
		        }
		        var mesh = Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh;
                if (mesh.GetInstanceID() < 0) { //is already combined
                    continue;
                }
                for (int m = 0; m < mesh.subMeshCount; m++) {
                    var matList = Application.isPlaying ? meshRenderer.materials : meshRenderer.sharedMaterials;
                    if (m >= matList.Length || m >= mesh.subMeshCount) {
                        if (errorCnt < 50) {
                            errorCnt++;
                            Debug.LogError(
                                string.Format(
                                    "MeshCombine: GameObject {0} is missing a material",
                                    meshRenderer.name));
                        }
                        continue;
                    }
                    Material mat = matList[m];
                    if (!current.CanAdd(mesh.vertexCount)) {
                        current = new MeshCombiner();
                        meshCombines.Add(current);
                    }
                    current.AddMesh(meshFilter, mat, m);
                }
		    }
            if (splitSectors) {
                foreach (var combine in sectorMeshCombines) {
                    var created =BuildCombineList(combine.Value, target, deleteOriginal, combine.Key.ToString());
                    for (int i = 0; i < created.Count; i++) {
                        World.Get<CullingSystem>().Add(combine.Key, created[i]);
                    }
                }
            }
            else {
                BuildCombineList(meshCombines, target, deleteOriginal, source.name);
            }
            source.transform.position = originalPos;
            if (!deleteOriginal) {
                return;
            }
            for (int i = 0; i < meshFilters.Count; i++) {
                if (meshFilters[i] != null && meshFilters[i].gameObject.isStatic) {
                    UnityEngine.Object.DestroyImmediate(meshFilters[i].gameObject);
                }
            }
            var childTr = source.GetComponentsInChildren<Transform>();
            for (int i = 0; i < childTr.Length; i++) {
                if (childTr[i] == null ||
                    childTr[i].transform.childCount > 0 || !childTr[i].CompareTag(StringConst.TagDummy)) {
                    continue;
                }
                UnityEngine.Object.DestroyImmediate(childTr[i].gameObject);
            }
        }

        private static List<GameObject> BuildCombineList(List<MeshCombiner> meshCombines, Transform target, bool deleteOriginal, string combineName) {
            List<GameObject> created = new List<GameObject>();
            for (int i = 0; i < meshCombines.Count; i++) {
                var combiner = meshCombines[i];
                var sceneObj = new GameObject();
                created.Add(sceneObj);
                var newName = meshCombines.Count > 1 ? string.Format("{0}-{1}", combineName, i) : combineName;
                sceneObj.name = newName;
                sceneObj.transform.parent = target;
                sceneObj.transform.localPosition = Vector3.zero;
                sceneObj.layer = LayerMasks.NumberEnvironment;
                sceneObj.isStatic = true;
                var mf = sceneObj.AddComponent<MeshFilter>();
                var meshes = new Mesh[combiner.Combines.Count];
                var finalCombines = new CombineInstance[combiner.Combines.Count];
                var mats = combiner.Combines.Keys.ToArray();
                for (int m = 0; m < mats.Length; m++) {
                    meshes[m] = new Mesh();
                    meshes[m].CombineMeshes(combiner.Combines[mats[m]].ToArray(), true, true, true);
                    finalCombines[m] = new CombineInstance() {
                        mesh = meshes[m],
                        subMeshIndex = 0
                    };
                }
                mf.sharedMesh = new Mesh();
                if (deleteOriginal) {
                    mf.hideFlags = HideFlags.DontSave;
                }
                mf.sharedMesh.CombineMeshes(finalCombines, false, false);
                var render = sceneObj.AddComponent<MeshRenderer>();
                // maybe need to add specific tag for objects that cast shadows and put them on their own renderer
                render.shadowCastingMode = ShadowCastingMode.Off;
                render.materials = mats;
                mf.sharedMesh.name = newName;
                StaticBatchingUtility.Combine(mf.gameObject);
                sceneObj.AddComponent<MeshCollider>();
                for (int m = 0; m < mats.Length; m++) {
                    meshes[m].Clear();
                    UnityEngine.Object.DestroyImmediate(meshes[m]);
                }
            }
            return created;
        }
    }


    public class MeshCombiner {
        public const int VertLimit = 45000;

        public Dictionary<Material, List<CombineInstance>> Combines = new Dictionary<Material, List<CombineInstance>>();
        //public List<Material> Mats = new List<Material>();
        //public List<CombineInstance> CombineList = new List<CombineInstance>();
        public int VertCount = 0;

        public bool CanAdd(int vertCount) {
            if (vertCount + VertCount > VertLimit) {
                return false;
            }
            return true;
        }

        //private int FindSubMeshIndex(Material mat) {
        //    for (int i = 0; i < Mats.Count; i++) {
        //        if (Mats[i] == mat) {
        //            return i;
        //        }
        //    }
        //    return -1;
        //}

        public void AddMesh(MeshFilter filter, Material mat, int subMeshIndex) {
            if (mat == null) {
                return;
            }
            //List<CombineInstance> list;
            VertCount += filter.sharedMesh.vertexCount;
            List<CombineInstance> list;
            if (!Combines.TryGetValue(mat, out list)) {
                list = new List<CombineInstance>();
                Combines.Add(mat, list);
            }
            //if (!Mats.Contains(mat)) {
            //    Mats.Add(mat);
            //}
            //subMeshIndex = FindSubMeshIndex(mat);
            //if (subMeshIndex < 0) {
            //    subMeshIndex = Mats.Count;
            //    Mats.Add(mat);
            //}
            //if (!Combine.TryGetValue(mat, out list)) {
            //    list = new List<CombineInstance>();
            //    Combine.Add(mat, list);
            //    subMeshIndex = Mats.Count;
            //    Mats.Add(mat);
            //}
            //else {
            //    subMeshIndex = FindSubMeshIndex(mat);
            //}
            list.Add(new CombineInstance() {
                mesh = filter.sharedMesh,
                subMeshIndex = subMeshIndex,
                transform = filter.transform.localToWorldMatrix
            });
        }
    }
}

