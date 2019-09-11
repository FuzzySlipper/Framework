using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class DecalRenderer : MonoBehaviour {

        private const string _bufferBaseName = "Decalicious - ";
        private const string _bufferDeferredName = _bufferBaseName + "Deferred";
        private const string _bufferUnlitName = _bufferBaseName + "Unlit";
        private const string _bufferLimitToName = _bufferBaseName + "Limit To Game Objects";
        private const CameraEvent _camEventDeferred = CameraEvent.BeforeReflections;
        private const CameraEvent _camEventUnlit = CameraEvent.BeforeImageEffectsOpaque;
        private const CameraEvent _camEventLimitTo = CameraEvent.AfterGBuffer;
        private const float _noLimitToValue = 0.0f;

        public static DecalRenderer Main;
        
        private static Vector4[] _avCoeff = new Vector4[7];
        private RenderTargetIdentifier[] _albedoRenderTarget;
        private CommandBuffer _bufferDeferred;
        private CommandBuffer _bufferLimitTo;
        private CommandBuffer _bufferUnlit;
        private Camera _camera;
        private bool _camLastKnownHDR;
        private Vector4[] _colors;
        private List<Decal> _decalComponent;
        private SortedDictionary<int, Dictionary<Material, HashSet<Decal>>> _deferredDecals;
        private MaterialPropertyBlock _directBlock;
        private float[] _fadeValues;
        private MaterialPropertyBlock _instancedBlock;
        private HashSet<GameObject> _limitToGameObjects;
        private List<MeshRenderer> _limitToMeshRenderers;
        private float[] _limitToValues;
        
        private Matrix4x4[] _matrices;
        private List<MeshFilter> _meshFilterComponent;
        private RenderTargetIdentifier[] _normalRenderTarget;
        private SortedDictionary<int, Dictionary<Material, HashSet<Decal>>> _unlitDecals;
        private int _copy1ID;
        private int _copy2ID;
        private int _limitToId;

        public bool UseInstancing = true;

        [SerializeField] private Mesh _cubeMesh = null;
        [SerializeField] private Material _materialLimitToGameObjects;

        private void Awake() {
            Main = this;
        }
        
        private void OnEnable() {
            Main = this;
            _deferredDecals = new SortedDictionary<int, Dictionary<Material, HashSet<Decal>>>();
            _unlitDecals = new SortedDictionary<int, Dictionary<Material, HashSet<Decal>>>();
            _limitToMeshRenderers = new List<MeshRenderer>();
            _limitToGameObjects = new HashSet<GameObject>();
            _decalComponent = new List<Decal>();
            _meshFilterComponent = new List<MeshFilter>();
            _matrices = new Matrix4x4[1023];
            _fadeValues = new float[1023];
            _limitToValues = new float[1023];
            _colors = new Vector4[1023];
            _instancedBlock = new MaterialPropertyBlock();
            _directBlock = new MaterialPropertyBlock();
            _camera = GetComponent<Camera>();
            _normalRenderTarget = new RenderTargetIdentifier[] {BuiltinRenderTextureType.GBuffer1, BuiltinRenderTextureType.GBuffer2};
            _copy1ID = Shader.PropertyToID("_CameraGBufferTexture1Copy");
            _copy2ID = Shader.PropertyToID("_CameraGBufferTexture2Copy");
            _limitToId = Shader.PropertyToID("_DecaliciousLimitToGameObject");
        }

        private void OnDisable() {
            if (_bufferDeferred != null) {
                GetComponent<Camera>().RemoveCommandBuffer(_camEventDeferred, _bufferDeferred);
                _bufferDeferred = null;
            }
            if (_bufferUnlit != null) {
                GetComponent<Camera>().RemoveCommandBuffer(_camEventUnlit, _bufferUnlit);
                _bufferUnlit = null;
            }
            if (_bufferLimitTo != null) {
                GetComponent<Camera>().RemoveCommandBuffer(_camEventLimitTo, _bufferLimitTo);
                _bufferLimitTo = null;
            }
        }

        private void OnPreRender() {
            if (!SystemInfo.supportsInstancing) {
                UseInstancing = false;
            }
            if (_albedoRenderTarget == null || _camera.allowHDR != _camLastKnownHDR) {
                _camLastKnownHDR = _camera.allowHDR;
                _albedoRenderTarget = new RenderTargetIdentifier[] {
                    BuiltinRenderTextureType.GBuffer0,
                    _camLastKnownHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3
                };
            }

            // Make sure that command buffers are created
            CreateBuffer(ref _bufferDeferred, _camera, _bufferDeferredName, _camEventDeferred);
            CreateBuffer(ref _bufferUnlit, _camera, _bufferUnlitName, _camEventUnlit);
            CreateBuffer(ref _bufferLimitTo, _camera, _bufferLimitToName, _camEventLimitTo);

            // Render Game Objects that are special decal targets
            _bufferLimitTo.Clear();
            DrawLimitToGameObjects(_camera);

            // Prepare command buffer for deferred decals
            _bufferDeferred.Clear();
            DrawDeferredDecals_Albedo();
            DrawDeferredDecals_NormSpecSmooth();

            // Prepare command buffer for unlit decals
            _bufferUnlit.Clear();
            DrawUnlitDecals();

            // TODO: Materials that are no longer used will never be removed from the dictionary -
            //   which should not be a big thing, but anyway.

            // Clear deferred decal list for next frame
            var decalEnum = _deferredDecals.GetEnumerator();
            while (decalEnum.MoveNext()) {
                decalEnum.Current.Value.Clear();
            }
            decalEnum.Dispose();
            // Clear unlit decal list for next frame
            decalEnum = _unlitDecals.GetEnumerator();
            while (decalEnum.MoveNext()) {
                decalEnum.Current.Value.Clear();
            }
            decalEnum.Dispose();

            // Clear limit to targets for next frame
            _limitToGameObjects.Clear();
        }

        public void Add(Decal decal, GameObject limitTo) {
            if (limitTo) {
                _limitToGameObjects.Add(limitTo);
            }
            switch (decal.RenderMode) {
                case Decal.DecalRenderMode.Deferred:
                    AddDeferred(decal);
                    break;
                case Decal.DecalRenderMode.Unlit:
                    AddUnlit(decal);
                    break;
            }
        }

        private void CreateBuffer(ref CommandBuffer buffer, Camera cam, string name, CameraEvent evt) {
            if (buffer == null) {
                // See if the camera already has a command buffer to avoid duplicates
                foreach (CommandBuffer existingCommandBuffer in cam.GetCommandBuffers(evt)) {
                    if (existingCommandBuffer.name == name) {
                        buffer = existingCommandBuffer;
                        break;
                    }
                }

                // Not found? Create a new command buffer
                if (buffer == null) {
                    buffer = new CommandBuffer();
                    buffer.name = name;
                    cam.AddCommandBuffer(evt, buffer);
                }
            }
        }

        private void AddDeferred(Decal decal) {
            if (!_deferredDecals.ContainsKey(decal.RenderOrder)) {
                _deferredDecals.Add(decal.RenderOrder, new Dictionary<Material, HashSet<Decal>>());
            }
            var dict = _deferredDecals[decal.RenderOrder];
            if (!dict.ContainsKey(decal.Material)) {
                dict.Add(decal.Material, new HashSet<Decal> {decal});
            }
            else {
                dict[decal.Material].Add(decal);
            }
        }

        private void AddUnlit(Decal decal) {
            if (!_unlitDecals.ContainsKey(decal.RenderOrder)) {
                _unlitDecals.Add(decal.RenderOrder, new Dictionary<Material, HashSet<Decal>>());
            }
            var dict = _unlitDecals[decal.RenderOrder];
            if (!dict.ContainsKey(decal.Material)) {
                dict.Add(decal.Material, new HashSet<Decal> {decal});
            }
            else {
                dict[decal.Material].Add(decal);
            }
        }

        private void DrawDeferredDecals_Albedo() {
            if (_deferredDecals.Count == 0) {
                return;
            }

            // Render first pass: albedo
            _bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

            // Traverse over decal render order values
            foreach (var deferredDecal in _deferredDecals) {
                foreach (var dictDecal in deferredDecal.Value) {
                    Material material = dictDecal.Key;
                    HashSet<Decal> decals = dictDecal.Value;
                    int n = 0;
                    foreach (var decal in decals) {
                        if (decal != null && decal.DrawAlbedo) {
                            DrawAlbedo(decal, material, ref n);
                        }
                    }
                    if (UseInstancing && n > 0) {
                        DrawInstanceAlbedo(material, n);
                    }
                }
            }
        }

        private void DrawDeferredDecals_NormSpecSmooth() {
            if (_deferredDecals.Count == 0) {
                return;
            }

            _bufferDeferred.GetTemporaryRT(_copy1ID, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            _bufferDeferred.GetTemporaryRT(_copy2ID, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            foreach (var deferredDecal in _deferredDecals) {
                // Traverse over decal render order values
                foreach (var allRenderOrder in deferredDecal.Value) {
                    // Render second pass: specular / smoothness and normals
                    Material material = allRenderOrder.Key;
                    HashSet<Decal> decals = allRenderOrder.Value;
                    int n = 0;
                    foreach (var decal in decals) {
                        if (decal != null && decal.DrawNormalAndGloss) {
                            DrawNormal(decal, material, ref n);
                        }
                    }
                    if (UseInstancing && n > 0) {
                        DrawInstanceNormal(material, n);
                    }
                }
            }
        }

        private void DrawAlbedo(Decal decal, Material material, ref int n) {
            if (UseInstancing && !decal.UseLightProbes) {
                _matrices[n] = decal.transform.localToWorldMatrix;
                _fadeValues[n] = decal.Fade;
                _limitToValues[n] = decal.LimitTo ? decal.LimitTo.GetInstanceID() : _noLimitToValue;
                _colors[n] = decal.Color;
                ++n;
                if (n == 1023) {
                    _instancedBlock.Clear();
                    _instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
                    _instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
                    _instancedBlock.SetVectorArray("_Color", _colors);
                    SetLightProbeOnBlock(RenderSettings.ambientProbe, _instancedBlock);
                    _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _instancedBlock);
                    n = 0;
                }
            }
            else {
                // Fall back to non-instanced rendering
                _directBlock.Clear();
                _directBlock.SetFloat("_MaskMultiplier", decal.Fade);
                _directBlock.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : _noLimitToValue);
                _directBlock.SetVectorArray("_Color", _colors);
                // Interpolate a light probe for this probe, if requested
                if (decal.UseLightProbes) {
                    LightProbes.GetInterpolatedProbe(decal.transform.position, decal.Mr, out var probe);
                    SetLightProbeOnBlock(probe, _directBlock);
                }
                _bufferDeferred.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 0, _directBlock);
            }
        }

        private void DrawNormal(Decal decal, Material material,ref int n) {
            if (decal.HighQualityBlending) {
                // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
                _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, _copy1ID);
                _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, _copy2ID);
                _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
                _instancedBlock.Clear();
                _instancedBlock.SetFloat("_MaskMultiplier", decal.Fade);
                _instancedBlock.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : _noLimitToValue);
                _instancedBlock.SetVectorArray("_Color", _colors);
                _bufferDeferred.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 1, _instancedBlock);
            }
            else {
                if (UseInstancing) {
                    // Instanced drawing
                    _matrices[n] = decal.transform.localToWorldMatrix;
                    _fadeValues[n] = decal.Fade;
                    _limitToValues[n] = decal.LimitTo ? decal.LimitTo.GetInstanceID() : _noLimitToValue;
                    _colors[n] = decal.Color;
                    ++n;
                    if (n == 1023) {
                        // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
                        _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, _copy1ID);
                        _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, _copy2ID);
                        _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
                        _instancedBlock.Clear();
                        _instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
                        _instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
                        _instancedBlock.SetVectorArray("_Color", _colors);
                        _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n, _instancedBlock);
                        n = 0;
                    }
                }
                else {
                    if (n == 0) {
                        // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
                        _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, _copy1ID);
                        _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, _copy2ID);
                    }
                    _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
                    _instancedBlock.Clear();
                    _instancedBlock.SetFloat("_MaskMultiplier", decal.Fade);
                    _instancedBlock.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : _noLimitToValue);
                    _instancedBlock.SetVectorArray("_Color", _colors);
                    _bufferDeferred.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 1, _instancedBlock);
                    ++n;
                }
            }
        }

        private void DrawInstanceAlbedo(Material material, int n) {
            _instancedBlock.Clear();
            _instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
            _instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
            _instancedBlock.SetVectorArray("_Color", _colors);
            SetLightProbeOnBlock(RenderSettings.ambientProbe, _instancedBlock);
            _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _instancedBlock);

        }

        private void DrawInstanceNormal(Material material, int n) {
            // Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
            _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, _copy1ID);
            _bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, _copy2ID);
            _bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
            _instancedBlock.Clear();
            _instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
            _instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
            _instancedBlock.SetVectorArray("_Color", _colors);
            _bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n, _instancedBlock);
        }

        private void DrawLimitToGameObjects(Camera cam) {
            if (_limitToGameObjects.Count == 0) {
                return;
            }
            if (_materialLimitToGameObjects == null) {
                _materialLimitToGameObjects = new Material(Shader.Find("Hidden/Decalicious Game Object ID"));
            }
            _bufferLimitTo.GetTemporaryRT(_limitToId, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
            _bufferLimitTo.SetRenderTarget(_limitToId, BuiltinRenderTextureType.CameraTarget);
            _bufferLimitTo.ClearRenderTarget(false, true, Color.black);

            // Loop over all game objects used for limiting decals
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
            foreach (GameObject go in _limitToGameObjects) {
                _bufferLimitTo.SetGlobalFloat("_ID", go.GetInstanceID());

                // Draw all mesh renderers...
                _limitToMeshRenderers.Clear();
                go.GetComponentsInChildren(_limitToMeshRenderers);
                foreach (MeshRenderer mr in _limitToMeshRenderers) {
                    // ...if they are not decals themselves...
                    // NOTE: We're using this trick because GetComponent() does some GC allocs
                    //       when the component is null (for some warning string or whatever).
                    _decalComponent.Clear();
                    mr.GetComponents(_decalComponent);
                    if (_decalComponent.Count == 0) {
                        // Cull meshes that are outside the camera's frustum
                        if (!GeometryUtility.TestPlanesAABB(frustumPlanes, mr.bounds)) {
                            continue;
                        }

                        // ...and have a mesh filter
                        _meshFilterComponent.Clear();
                        mr.GetComponents(_meshFilterComponent);
                        if (_meshFilterComponent.Count == 1) {
                            MeshFilter mf = _meshFilterComponent[0];
                            if (mr.isPartOfStaticBatch) {
                                // If the renderer is statically batched, the mesh is in world space (i.e. we use identity matrix)
                                _bufferLimitTo.DrawMesh(mf.sharedMesh, Matrix4x4.identity, _materialLimitToGameObjects, mr.subMeshStartIndex);
                            }
                            else {
                                _bufferLimitTo.DrawMesh(mf.sharedMesh, mr.transform.localToWorldMatrix, _materialLimitToGameObjects, mr.subMeshStartIndex);
                            }
                        }
                    }
                }
            }
        }

        private void DrawUnlitDecals() {
            if (_unlitDecals.Count == 0) {
                return;
            }

            // Render third pass: unlit decals
            _bufferUnlit.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            foreach (var unlitDecal in _unlitDecals) {
                // Traverse over decal render order values
                foreach (var allRenderOrder in unlitDecal.Value) {
                    // Render second pass: specular / smoothness and normals
                    Material material = allRenderOrder.Key;
                    HashSet<Decal> decals = allRenderOrder.Value;
                    int n = 0;
                    foreach (var decal in decals) {
                        if (decal != null) {
                            if (UseInstancing) {
                                _matrices[n] = decal.transform.localToWorldMatrix;
                                _fadeValues[n] = decal.Fade;
                                _limitToValues[n] = decal.LimitTo ? decal.LimitTo.GetInstanceID() : _noLimitToValue;
                                ++n;
                                if (n == 1023) {
                                    _instancedBlock.Clear();
                                    _instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
                                    _instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
                                    _instancedBlock.SetVectorArray("_Color", _colors);
                                    _bufferUnlit.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _instancedBlock);
                                    n = 0;
                                }
                            }
                            else {
                                _instancedBlock.Clear();
                                _instancedBlock.SetFloat("_MaskMultiplier", decal.Fade);
                                _instancedBlock.SetFloat("_LimitTo", decal.LimitTo ? decal.LimitTo.GetInstanceID() : _noLimitToValue);
                                _instancedBlock.SetVectorArray("_Color", _colors);
                                _bufferUnlit.DrawMesh(_cubeMesh, decal.transform.localToWorldMatrix, material, 0, 0, _instancedBlock);
                            }
                        }
                    }
                    if (UseInstancing && n > 0) {
                        _instancedBlock.Clear();
                        _instancedBlock.SetFloatArray("_MaskMultiplier", _fadeValues);
                        _instancedBlock.SetFloatArray("_LimitTo", _limitToValues);
                        _instancedBlock.SetVectorArray("_Color", _colors);
                        _bufferUnlit.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _instancedBlock);
                    }
                }
            }
        }

        private void SetLightProbeOnBlock(SphericalHarmonicsL2 probe, MaterialPropertyBlock block) {
            // Kudos to Bas-Smit for this. I couldn't make sense of it for the life of me.
            // https://forum.unity3d.com/threads/getinterpolatedlightprobe-interpreting-the-coefficients.209223/
            for (int iC = 0; iC < 3; iC++) {
                _avCoeff[iC].x = probe[iC, 3];
                _avCoeff[iC].y = probe[iC, 1];
                _avCoeff[iC].z = probe[iC, 2];
                _avCoeff[iC].w = probe[iC, 0] - probe[iC, 6];
            }
            for (int iC = 0; iC < 3; iC++) {
                _avCoeff[iC + 3].x = probe[iC, 4];
                _avCoeff[iC + 3].y = probe[iC, 5];
                _avCoeff[iC + 3].z = 3.0f * probe[iC, 6];
                _avCoeff[iC + 3].w = probe[iC, 7];
            }
            _avCoeff[6].x = probe[0, 8];
            _avCoeff[6].y = probe[1, 8];
            _avCoeff[6].z = probe[2, 8];
            _avCoeff[6].w = 1.0f;
            block.SetVector("unity_SHAr", _avCoeff[0]);
            block.SetVector("unity_SHAg", _avCoeff[1]);
            block.SetVector("unity_SHAb", _avCoeff[2]);
            block.SetVector("unity_SHBr", _avCoeff[3]);
            block.SetVector("unity_SHBg", _avCoeff[4]);
            block.SetVector("unity_SHBb", _avCoeff[5]);
            block.SetVector("unity_SHC", _avCoeff[6]);
        }
    }
}