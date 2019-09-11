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
                var mesh = meshFilter.sharedMesh;
		        //var mesh = Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh;
                if (mesh.GetInstanceID() < 0) { //is already combined
                    continue;
                }
                for (int m = 0; m < mesh.subMeshCount; m++) {
                    var matList = meshRenderer.sharedMaterials;
                    //var matList = Application.isPlaying ? meshRenderer.materials : meshRenderer.sharedMaterials;
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
            Transform createPivot = null;
            if (splitSectors) {
                foreach (var combine in sectorMeshCombines) {
                    var created =BuildCombineList(combine.Value, target, deleteOriginal, combine.Key.ToString());
                    for (int i = 0; i < created.Count; i++) {
                        World.Get<CullingSystem>().Add(combine.Key, created[i]);
                    }
                    createPivot = created[0].transform;
                }
            }
            else {
                createPivot = BuildCombineList(meshCombines, target, deleteOriginal, source.name)[0].transform;
            }
            source.position = originalPos;
            if (!deleteOriginal || createPivot == null) {
                return;
            }
            var childTr = source.GetComponentsInChildren<Transform>(true);
            Dictionary<Point3, Dictionary<int, Transform>> colliderDict = new Dictionary<Point3, Dictionary<int, Transform>>();
            colliderDict.Add(createPivot.rotation.eulerAngles.toPoint3(), new Dictionary<int, Transform>() {
                {createPivot.gameObject.layer, createPivot}
            });
            for (int i = 0; i < meshFilters.Count; i++) {
                if (meshFilters[i] == null) {
                    continue;
                }
                if (meshFilters[i].gameObject.isStatic) {
                    DeleteObject(createPivot, meshFilters[i].gameObject, colliderDict);
                }
            }
            for (int i = 0; i < childTr.Length; i++) {
                if (childTr[i] == null) {
                    continue;   
                }
                if (childTr[i].transform.childCount > 0 || childTr[i].CompareTag(StringConst.TagCreated) || childTr[i].CompareTag(StringConst.TagDoNotDisable)) {
                    continue;
                }
                if (childTr[i].CompareTag(StringConst.TagDeleteObject)) {
                    DeleteObject(createPivot, childTr[i].gameObject, colliderDict);
                    continue;
                }
                childTr[i].GetComponents<Component>(_monoList);
                bool foundScripts = false;
                for (int j = 0; j < _monoList.Count; j++) {
                    var mono = _monoList[j];
                    if (mono is Transform) {
                        continue;
                    }
                    foundScripts = true;
                    break;
                }
                if (foundScripts) {
                    continue;
                }
                DeleteObject(createPivot, childTr[i].gameObject, colliderDict);
            }
        }

        private static List<Collider> _colliderList = new List<Collider>(5);
        private static List<Component> _monoList = new List<Component>(15);

        private static void DeleteObject(Transform combined, GameObject checkedObj, Dictionary<Point3, Dictionary<int, Transform>> colliderDict) {
            checkedObj.GetComponentsInChildren<Collider>(true, _colliderList);
            for (int i = 0; i < _colliderList.Count; i++) {
                if (_colliderList[i] == null) {
                    continue;
                }
                var coll = _colliderList[i];
                var p3 = coll.transform.rotation.eulerAngles.WorldToGenericGrid(1);
                if (!colliderDict.TryGetValue(p3, out var dict)) {
                    dict = new Dictionary<int, Transform>();
                    colliderDict.Add(p3, dict);
                }
                if (!dict.TryGetValue(coll.gameObject.layer, out var target)) {
                    var rotated = new GameObject("Rotation: " + p3);
                    rotated.tag = StringConst.TagEnvironment;
                    rotated.transform.SetParentResetPos(combined);
                    rotated.transform.rotation = coll.transform.rotation;
                    rotated.layer = coll.gameObject.layer;
                    target = rotated.transform;
                    dict.Add(coll.gameObject.layer, target);
                }
                if (coll is BoxCollider boxCollider) {
                    var box = target.gameObject.AddComponent<BoxCollider>();
                    box.center = target.InverseTransformPoint(coll.transform.TransformPoint(boxCollider.center));
                    box.size = new Vector3(Mathf.Abs(boxCollider.size.x * coll.transform.lossyScale.x), Mathf.Abs(boxCollider.size.y * coll.transform.lossyScale.y), Mathf.Abs(boxCollider.size.z * coll.transform.lossyScale.z));
                }
                if (coll is SphereCollider sphereCollider) {
                    var sphere = target.gameObject.AddComponent<SphereCollider>();
                    sphere.center = target.InverseTransformPoint(coll.transform.TransformPoint(sphereCollider.center));
                    sphere.radius = sphereCollider.radius * coll.transform.lossyScale.AbsMax();
                }
                if (coll is MeshCollider meshCollider) {
                    var go = new GameObject(coll.transform.name + " Collider");
                    go.tag = coll.tag;
                    go.transform.SetParent(target);
                    go.transform.position = coll.transform.position;
                    go.transform.rotation = coll.transform.rotation;
                    go.transform.localScale = coll.transform.lossyScale;
                    go.layer = coll.gameObject.layer;
                    var mesh = go.gameObject.AddComponent<MeshCollider>();
                    mesh.sharedMesh = meshCollider.sharedMesh;
                    mesh.convex = meshCollider.convex;
                }
                if (coll is CapsuleCollider capsuleCollider) {
                    var capsule = target.gameObject.AddComponent<CapsuleCollider>();
                    capsule.center = target.InverseTransformPoint(coll.transform.TransformPoint(capsuleCollider.center));
                    capsule.radius = capsuleCollider.radius * coll.transform.lossyScale.AbsMax();
                    capsule.height = capsuleCollider.height * coll.transform.lossyScale.AbsMax();
                }
            }
            UnityEngine.Object.DestroyImmediate(checkedObj);
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
                sceneObj.tag = StringConst.TagEnvironment;
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
                //if (deleteOriginal) {
                //    mf.hideFlags = HideFlags.DontSave;
                //}
                mf.sharedMesh.CombineMeshes(finalCombines, false, false);
                var render = sceneObj.AddComponent<MeshRenderer>();
                // maybe need to add specific tag for objects that cast shadows and put them on their own renderer
                render.shadowCastingMode = ShadowCastingMode.Off;
                render.materials = mats;
                mf.sharedMesh.name = newName;
                StaticBatchingUtility.Combine(mf.gameObject);
                //sceneObj.AddComponent<MeshCollider>();
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

