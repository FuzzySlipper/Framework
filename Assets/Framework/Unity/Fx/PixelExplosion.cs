using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace PixelComrades {
    public class PixelExplosion : MonoBehaviour {
        private const float MinimumSpawnTime = 0.1f;


        [SerializeField] private ParticleSystem _particleSystem = null;
        [SerializeField] private int _maxParticles = 1500;
        [SerializeField] private float _maxForceMagnitude = 10;
        [SerializeField] private FloatRange _extraForce = new FloatRange(2, 5);
        [SerializeField] private MeshFilter _testFilter = null;
        [SerializeField] private SkinnedMeshRenderer _testSkinned = null;
        [SerializeField] private Vector3 _testVelocity = Vector3.one;

        private ParticleSystem.Particle[] _particles;

        public bool IsAlive() {
            return _particleSystem.IsAlive();
        }

        //[Inspect, Method(MethodDisplay.Button)]
        public void Test() {
            if (_testFilter != null) {
                StartExplosion(_testFilter, _testVelocity);
            }
            else if (_testSkinned != null) {
                StartExplosion(_testSkinned, _testVelocity);
            }
        }

        public void StartExplosion(MeshFilter filter, Vector3 velocity) {
            var verts = filter.sharedMesh.vertices;
            var colors = filter.sharedMesh.colors;
            transform.position = filter.transform.position;
            transform.rotation = filter.transform.rotation;
            for (int i = 0; i < verts.Length; i++) {
                verts[i] = filter.transform.TransformPoint(verts[i]);
            }
            ProcessVertices(verts, colors, velocity);
        }

        public void StartExplosion(PrefabEntity entity, Vector3 velocity) {
            var totalCount = 0;
            for (int i = 0; i < entity.Renderers.Length; i++) {
                if (entity.Renderers[i] is SkinnedMeshRenderer skinned) {
                    totalCount += skinned.sharedMesh.vertexCount;
                }
                else if (entity.Renderers[i] is MeshRenderer mesh) {
                    var mf = entity.Renderers[i].GetComponent<MeshFilter>();
                    if (mf != null) {
                        totalCount += mf.sharedMesh.vertexCount;
                    }
                    
                }
            }
            var verts = new List<Vector3>(totalCount);
            var colors = new List<Color>(totalCount);
            for (int i = 0; i < entity.Renderers.Length; i++) {
                var entityRenderer = entity.Renderers[i];
                Mesh newMesh = null;
                if (entityRenderer is SkinnedMeshRenderer skinned) {
                    newMesh = new Mesh();
                    skinned.BakeMesh(newMesh);
                }
                else {
                    var mf = entityRenderer.GetComponent<MeshFilter>();
                    if (mf != null) {
                        newMesh = mf.sharedMesh;
                    }
                }
                if (newMesh == null) {
                    continue;
                }
                var newColors = newMesh.colors;
                if (newColors.Length == 0) {
                    continue;
                }
                var newVerts = newMesh.vertices;
                for (int v = 0; v < newVerts.Length; v++) {
                    //newVerts[v] = entityRenderer.transform.TransformPoint(verts[v]);
                    verts.Add(entityRenderer.transform.TransformPoint(newVerts[v]));
                    colors.Add(newColors[v]);
                }
                //verts.AddRange(newVerts);
                //colors.AddRange(newColors);
            }
            transform.position = entity.transform.position;
            transform.rotation = entity.transform.rotation;
            ProcessVertices(verts.ToArray(), colors.ToArray(), velocity);
        }

        public void StartExplosion(SkinnedMeshRenderer skinnedRenderer, Vector3 velocity) {
            Mesh newMesh = new Mesh();
            skinnedRenderer.BakeMesh(newMesh);
            transform.position = skinnedRenderer.transform.position;
            transform.rotation = skinnedRenderer.transform.rotation;
            var verts = newMesh.vertices;
            var colors = newMesh.colors;
            for (int i = 0; i < verts.Length; i++) {
                verts[i] = skinnedRenderer.transform.TransformPoint(verts[i]);
            }
            ProcessVertices(verts, colors, velocity);
        }

        public void StartExplosion(SpriteRenderer spriteRenderer, Vector3 velocity) {
            transform.position = spriteRenderer.transform.position;
            transform.rotation = spriteRenderer.transform.rotation;
            ushort[] tris = spriteRenderer.sprite.triangles;
            Vector2[] uvs = spriteRenderer.sprite.uv;
            Vector2[] verts = spriteRenderer.sprite.vertices;
            var finalVerts = new Vector3[spriteRenderer.sprite.uv.Length];
            var finalColors = new Color[spriteRenderer.sprite.uv.Length];
            for (int i = 0; i < uvs.Length; i++) {
                finalVerts[i] = UvTo3D(uvs[i], tris, uvs, verts);
                finalColors[i] = spriteRenderer.sprite.texture.GetPixel((int) uvs[i].x, (int) uvs[i].y);
            }
            ProcessVertices(finalVerts, finalColors, velocity);
        }

        Vector3 UvTo3D(Vector2 uv, ushort[] tris, Vector2[] uvs, Vector2[] verts) {
            for (int i = 0; i < tris.Length; i += 3) {
                Vector2 u1 = uvs[tris[i]]; // get the triangle UVs
                Vector2 u2 = uvs[tris[i + 1]];
                Vector2 u3 = uvs[tris[i + 2]];
                // calculate triangle area - if zero, skip it
                float a = Area(u1, u2, u3);
                if (a == 0) {
                    continue;
                }
                // calculate barycentric coordinates of u1, u2 and u3
                // if anyone is negative, point is outside the triangle: skip it
                float a1 = Area(u2, u3, uv) / a;
                if (a1 < 0) {
                    continue;
                }

                float a2 = Area(u3, u1, uv) / a;
                if (a2 < 0) {
                    continue;
                }

                float a3 = Area(u1, u2, uv) / a;
                if (a3 < 0) {
                    continue;
                }
                // point inside the triangle - find mesh position by interpolation...
                Vector3 p3D = a1 * verts[tris[i]] + a2 * verts[tris[i + 1]] + a3 * verts[tris[i + 2]];
                // and return it in world coordinates:
                return transform.TransformPoint(p3D);
            }
            // point outside any uv triangle: return Vector3.zero
            return Vector3.zero;
        }

        // calculate signed triangle area using a kind of "2D cross product":
        float Area(Vector2 p1, Vector2 p2, Vector2 p3) {
            Vector2 v1 = p1 - p3;
            Vector2 v2 = p2 - p3;
            return (v1.x * v2.y - v1.y * v2.x) / 2;
        }


        public void StartExplosion(Vector3[] verts, Color[] colors, Vector3 velocity) {
            ProcessVertices(verts, colors, velocity);
        }

        private void ProcessVertices(Vector3[] verts, Color[] colors, Vector3 velocity) {
            if (verts == null || colors == null) {
                Debug.Log("no verts");
                return;
            }
            velocity = Vector3.ClampMagnitude(velocity, _maxForceMagnitude);
            CheckParticles();
            var vertexMulti = Mathf.Clamp(_maxParticles / verts.Length, 1, 2);
            var emitCount = Mathf.Clamp(verts.Length * vertexMulti, 100, _maxParticles);
            //_particleSystem.main.maxParticles = _maxParticles;
            _particleSystem.Emit(emitCount);
            var currentParticlesCount = _particleSystem.GetParticles(_particles);
            //if (currentParticlesCount != emitCount && !Application.isPlaying) {
            //    Debug.LogError(string.Format("Particle count {0} emit {1}", currentParticlesCount, emitCount));
            //}
            var vertexStep = verts.Length / _maxParticles;
            if (vertexStep < 1) {
                vertexStep = 1;
            }
            var vertIndex = 0;
            float jitter = _extraForce.Get();
            float posJitter = 0;
            //var octaves = 1;
            for (int i = 0; i < currentParticlesCount; i++) {
                var spherePos = UnityEngine.Random.insideUnitSphere;
                _particles[i].position = verts[vertIndex] + spherePos * posJitter; //targetTr.TransformPoint(verts[vertIndex]); //verts[vertIndex];//
                _particles[i].startColor = colors[vertIndex];
                var noiseOffset = Mathf.PerlinNoise(_particles[i].position.x, _particles[i].position.y); //Perlin.Fbm(verts[vertIndex], octaves);
                _particles[i].velocity = velocity + spherePos * jitter * noiseOffset;
                _particles[i].angularVelocity3D = velocity + spherePos * jitter * noiseOffset;
                vertIndex += vertexStep;
                if (vertIndex >= verts.Length) {
                    //octaves++;
                    vertIndex = 0;
                    posJitter += _particleSystem.main.startSize.Evaluate(0.5f);
                    jitter += _extraForce.Get();
                }
            }
            _particleSystem.SetParticles(_particles, emitCount);
            if (Application.isPlaying) {
                TimeManager.StartTask(DespawnParticles());
            }
        }

        //private IEnumerator WaitFrameForParticles(Vector3[] verts, Color[] colors, Vector3 velocity, int emitCount) {
        //    yield return null;
        //    var currentParticlesCount = _particleSystem.GetParticles(_particles);
        //    if (currentParticlesCount != emitCount && !Application.isPlaying) {
        //        Debug.LogError(string.Format("Particle count {0} emit {1}", currentParticlesCount, emitCount));
        //    }
        //    var vertexStep = verts.Length / _maxParticles;
        //    if (vertexStep < 1) {
        //        vertexStep = 1;
        //    }
        //    var vertIndex = 0;
        //    float jitter = _extraForce.GetNewValue();
        //    float posJitter = 0;
        //    //var octaves = 1;
        //    for (int i = 0; i < currentParticlesCount; i++) {
        //        var spherePos = UnityEngine.Random.insideUnitSphere;
        //        _particles[i].position = verts[vertIndex] + spherePos * posJitter; //targetTr.TransformPoint(verts[vertIndex]); //verts[vertIndex];//
        //        _particles[i].startColor = colors[vertIndex];
        //        var noiseOffset = Mathf.PerlinNoise(_particles[i].position.x, _particles[i].position.y); //Perlin.Fbm(verts[vertIndex], octaves);
        //        _particles[i].velocity = velocity + spherePos * jitter * noiseOffset;
        //        _particles[i].angularVelocity3D = velocity + spherePos * jitter * noiseOffset;
        //        vertIndex += vertexStep;
        //        if (vertIndex >= verts.Length) {
        //            //octaves++;
        //            vertIndex = 0;
        //            posJitter += _particleSystem.startSize;
        //            jitter += _extraForce.GetNewValue();
        //        }
        //    }
        //    _particleSystem.SetParticles(_particles, emitCount);
        //    if (Application.isPlaying) {
        //        TimeManager.Start(DespawnParticles());
        //    }
        //}

        private void CheckParticles() {
            if (_particles== null || _particles.Length != _maxParticles) {
                _particles = new ParticleSystem.Particle[_maxParticles];
            }
        }


        IEnumerator DespawnParticles() {
            yield return MinimumSpawnTime;
            while (_particleSystem.IsAlive()) {
                yield return MinimumSpawnTime;
            }
            ItemPool.Despawn(gameObject);
        }
    }

}

//    private int _mipLevel = 2;
//    private Action _onFinished;
//    private WorldEntity _target;
//    private Camera _cam;

//    public void Atomize(WorldEntity unit, Action onFinished) {
//        _onFinished = onFinished;
//        _target = unit;
//        for (int i = 0; i < _target.Renderers.Length; i++) {
//            _target.Renderers[i].ChangeLayer(LayerMasks.NumberCameraCapture);
//        }
//        Snapshot snapshot = null;
//        RenderTexture.active = _cam.targetTexture;
//        _cam.Render();
//        //CanvasRenderer canvasRenderer = _target.GetComponentInChildren<CanvasRenderer>();
//        //if (canvasRenderer != null) {
//        //    snapshot = Snapshot.GetCanvasObjectSnapshot(_target, GetComponent<Camera>());
//        //}
//        snapshot = Snapshot.GetObjectSnapshot(_target, _cam);
//        BuildEffect(snapshot);
//    }
//    private void BuildEffect(Snapshot snapshot) {
//        Color32[] validPixels = GetValidPixels(snapshot.Image);

//        int imageResizeRatio = Mathf.RoundToInt(Mathf.Pow(2.0f, _mipLevel));
//        if (imageResizeRatio < 1) {
//            imageResizeRatio = 1;
//        }

//        int imageHeight = snapshot.Image.height / imageResizeRatio;
//        int imageWidth = snapshot.Image.width / imageResizeRatio;
//        int pixelsLength = validPixels.Length;
//        int validPixelCount = 0;

//        for (int i = 0; i < pixelsLength; ++i) {
//            if (validPixels[i].a > 0 && validPixels[i].r > 0 &&
//                validPixels[i].g > 0 && validPixels[i].b > 0) {
//                ++validPixelCount;
//            }
//        }
//        var emitParams = new ParticleSystem.EmitParams();
//        _particleSystem.Emit(emitParams, 1);
//        ParticleSystem.Particle[] particles = EmitParticles(validPixelCount);

//        int validPixelIndex = 0;
//        float adjustedParticleSize = ParticleSize;
//        if (false == ForceParticleSize) {
//            adjustedParticleSize = ParticleSize * imageResizeRatio;
//            adjustedParticleSize = Mathf.Clamp(adjustedParticleSize, 0.01f, 1.0f);
//        }

//        float minDistance = Vector3.Distance(_cam.transform.position, atomizerTarget.transform.position);
//        Color32 color = new Color32(0, 0, 0, 0);
//        Vector3 screenPoint = new Vector3(0.0f, 0.0f, minDistance);

//        for (int i = 0; i < imageHeight; ++i) {
//            float screenPointY = snapshot.ScreenRect.yMin + i * imageResizeRatio;
//            screenPoint.y = screenPointY;
//            for (int j = 0; j < imageWidth; ++j) {
//                int index = j + i * imageWidth;
//                if (index < pixelsLength && validPixels[index].a > 0 && validPixels[index].r > 0 &&
//                    validPixels[index].g > 0 && validPixels[index].b > 0) {
//                    ParticleSystem.Particle particle = particles[validPixelIndex];
//                    color = validPixels[index];
//                    color.a = 255;
//                    particle.color = color;
//                    particle.size = adjustedParticleSize;
//                    screenPoint.x = snapshot.ScreenRect.xMin + j * imageResizeRatio;
//                    particle.position = _cam.ScreenToWorldPoint(screenPoint);
//                    particles[validPixelIndex] = particle;
//                    ++validPixelIndex;
//                }
//            }
//        }

//        Destroy(snapshot.Image);

//        SetParticles(particles);
//    }

//    private Color32[] GetValidPixels(Texture2D image) {
//        _mipLevel = image.mipmapCount > 0 ? image.mipmapCount - 1 : 0;
//        Color32[] pixels = image.GetPixels32(_mipLevel);

//        Color32[] validPixels = pixels;
//        while (pixels.Length < MaxParticleCount && _mipLevel > 0) {
//            --_mipLevel;
//            validPixels = pixels;
//            pixels = image.GetPixels32(_mipLevel);
//            if (pixels.Length > MaxParticleCount) {
//                ++_mipLevel;
//                break;
//            }
//            if (_mipLevel == 0) {
//                validPixels = pixels;
//            }
//        }

//        return validPixels;
//    }

//    //private IEnumerator OnPostRender() {
//    //    CanvasRenderer canvasRenderer = unit.GetComponentInChildren<CanvasRenderer>();
//    //    if (canvasRenderer != null) {
//    //        yield return new WaitForEndOfFrame();
//    //    }
//    //    BuildEffect();
//    //    yield return null;
//    //}


//}

//public class Snapshot {
//    public Texture2D Image { get; private set; }

//    public Rect ScreenRect { get; private set; }

//    public Texture2D ColourData { get; private set; }

//    public bool ReflectOnYAxis { get; private set; }

//    public static Snapshot GetObjectSnapshot(WorldEntity entity, Camera camera) {
//        var renderers = entity.Renderers;
//        if (renderers.Length == 0) {
//            return null;
//        }

//        Bounds bounds = GeometryHelper.GetRendererBounds(renderers);
//        Rect screenRect = GeometryHelper.GetScreenRect(GeometryHelper.GetWorldPoints(bounds), camera);

//        if (screenRect.width < 1 || screenRect.height < 1) {
//            Debug.LogWarning("Atomizer: Attempting to atomize an off-screen object.");
//            return null;
//        }

//        Texture2D image = new Texture2D(Mathf.FloorToInt(screenRect.width), Mathf.FloorToInt(screenRect.height), TextureFormat.ARGB32, true);
//        image.ReadPixels(screenRect, 0, 0);

//        Snapshot snapshot = new Snapshot();
//        snapshot.Image = image;
//        snapshot.ScreenRect = screenRect;

//        return snapshot;
//    }

//    public static Snapshot GetCanvasObjectSnapshot(GameObject unit, Camera camera) {
//        RectTransform rectTransform = unit.GetComponentInChildren<RectTransform>();

//        Vector3[] worldCorners = new Vector3[4];
//        rectTransform.GetWorldCorners(worldCorners);
//        Rect screenRect = GeometryHelper.GetScreenRect(worldCorners, camera);

//        if (screenRect.width < 1 || screenRect.height < 1) {
//            Debug.LogWarning("Atomizer: Attempting to atomize an off-screen object.");
//            return null;
//        }

//        Texture2D image = new Texture2D(Mathf.FloorToInt(screenRect.width), Mathf.FloorToInt(screenRect.height), TextureFormat.ARGB32, false);
//        image.ReadPixels(screenRect, 0, 0);

//        Sprite sprite = unit.GetComponent<Image>().sprite;
//        Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.ARGB32, false);

//        Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
//            (int)sprite.textureRect.y,
//            (int)sprite.textureRect.width,
//            (int)sprite.textureRect.height);

//        croppedTexture.SetPixels(pixels);
//        croppedTexture.Apply();

//        Snapshot snapshot = new Snapshot();
//        snapshot.Image = image;
//        if (sprite.rect.width > screenRect.width) {
//            snapshot.ColourData = image;
//        }
//        else {
//            snapshot.ColourData = croppedTexture;
//        }
//        snapshot.ScreenRect = screenRect;

//        // Might need to reflect on the Y axis.
//        snapshot.ReflectOnYAxis = (Mathf.Approximately(rectTransform.eulerAngles.y, 180.0f));

//        return snapshot;
//    }
//}

//public static class GeometryHelper {
//    public static Bounds GetRendererBounds(RendererTracker[] renderers) {
//        if (renderers.Length == 0) {
//            return new Bounds(Vector3.zero, Vector3.zero);
//        }

//        Bounds inclusiveBounds = renderers[0].Renderer.bounds;

//        for (int i = 1; i < renderers.Length; i++) {
//            inclusiveBounds.Encapsulate(renderers[i].Renderer.bounds);
//        }

//        return inclusiveBounds;
//    }

//    private static Rect BuildScreenRect(List<Vector2> list) {
//        list.Sort((a, b) => (a.x).CompareTo( (b.x)) );
//        float minX = list[0].x;
//        float maxX = list.LastElement().x;
//        list.Sort((a, b) => (a.y).CompareTo((b.y)));
//        float minY = list[0].y;
//        float maxY = list.LastElement().y;
//        Rect screenRect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
//        screenRect.xMin = Mathf.Clamp(screenRect.xMin, 1.0f, Screen.width - 1);
//        screenRect.xMax = Mathf.Clamp(screenRect.xMax, 1.0f, Screen.width - 1);
//        screenRect.yMin = Mathf.Clamp(screenRect.yMin, 1.0f, Screen.height - 1);
//        screenRect.yMax = Mathf.Clamp(screenRect.yMax, 1.0f, Screen.height - 1);
//        return screenRect;
//    }

//    public static Rect GetScreenRect(Vector3[] worldPoints, Camera camera) {
//        List<Vector2> screenPoints = new List<Vector2>(worldPoints.Length);
//        for (int i = 0; i < worldPoints.Length; i++) {
//            screenPoints.Add(camera.WorldToScreenPoint(worldPoints[i]));
//        }

//        return BuildScreenRect(screenPoints);
//    }

//    public static Rect GetScreenRect(Vector2[] worldPoints, Camera camera) {
//        List<Vector2> screenPoints = new List<Vector2>(worldPoints.Length);
//        for (int i = 0; i < worldPoints.Length; i++) {
//            screenPoints.Add(camera.WorldToScreenPoint(worldPoints[i]));
//        }

//        return BuildScreenRect(screenPoints);
//    }

//    public static Vector3[] GetWorldPoints(Bounds bounds) {
//        Vector3[] worldPoints = new Vector3[8];

//        Vector3 boundsCenter = bounds.center;
//        Vector3 boundsExtents = bounds.extents;

//        float plusX = boundsCenter.x + boundsExtents.x;
//        float minusX = boundsCenter.x - boundsExtents.x;
//        float plusY = boundsCenter.y + boundsExtents.y;
//        float minusY = boundsCenter.y - boundsExtents.y;
//        float plusZ = boundsCenter.z + boundsExtents.z;
//        float minusZ = boundsCenter.z - boundsExtents.z;

//        worldPoints[0] = new Vector3(plusX, plusY, plusZ);
//        worldPoints[1] = new Vector3(minusX, plusY, plusZ);
//        worldPoints[2] = new Vector3(plusX, minusY, plusZ);
//        worldPoints[3] = new Vector3(minusX, minusY, plusZ);
//        worldPoints[4] = new Vector3(plusX, plusY, minusZ);
//        worldPoints[5] = new Vector3(minusX, plusY, minusZ);
//        worldPoints[6] = new Vector3(plusX, minusY, minusZ);
//        worldPoints[7] = new Vector3(minusX, minusY, minusZ);

//        return worldPoints;
//    }
//}
