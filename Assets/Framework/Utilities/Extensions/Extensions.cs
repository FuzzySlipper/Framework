using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Random = System.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PixelComrades {

    public static class TextureUtilities {
        public static Texture2D MakeTexture(int width, int height, Color col) {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; ++i)
                pix[i] = col;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static Texture2D MakeTexture(int width, int height, Color col, Color border) {
            List<Color> pix = new List<Color>();
            for (int w = 0; w < width; w++) {
                pix.Add(w == 0 || w == width-1 ? border : col);
                for (int h = 0; h < height; h++) {
                    pix.Add(h == 0 || h == height - 1 ? border : col);
                }
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pix.ToArray());
            result.Apply();
            return result;
        }

        public static Texture2D MakeTexture(int width, int height, Color textureColor, RectOffset border, Color borderColor) {
            int widthInner = width;
            width += border.left;
            width += border.right;
            Color[] pix = new Color[width * (height + border.top + border.bottom)];
            for (int i = 0; i < pix.Length; i++) {
                if (i < (border.bottom * width))
                    pix[i] = borderColor;
                else if (i >= ((border.bottom * width) + (height * width))) //Border Top
                    pix[i] = borderColor;
                else { //Center of Texture

                    if ((i % width) < border.left) // Border left
                        pix[i] = borderColor;
                    else if ((i % width) >= (border.left + widthInner)) //Border right
                        pix[i] = borderColor;
                    else
                        pix[i] = textureColor; //Color texture
                }
            }
            Texture2D result = new Texture2D(width, height + border.top + border.bottom);
            result.SetPixels(pix);
            result.Apply();


            return result;
        }
    }
    public static class AnimationCurveExtension {
        public static AnimationCurve ClipBoardAnimationCurve;
    }
    
    [System.Serializable]
    public class GenericAssetHolder<T,TV> where TV : UnityEngine.Object where T : AssetReferenceEntry<TV> {
        public List<T> Objects = new List<T>();
        public AnimationCurve Curve = new AnimationCurve();

        public void Load() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            for (int i = 0; i < Objects.Count; i++) {
                Objects[i].LoadAssetAsync();
            }
        }

        public void Unload() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            for (int i = 0; i < Objects.Count; i++) {
                Objects[i].ReleaseAsset();
            }
        }

        public TV Get(int index) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return Objects[index].Asset as TV;
            }
#endif
            if (Objects[index].Asset == null) {
                var op = Objects[index].LoadAssetAsync();
                if (!op.IsDone) {
                    //Debug.LogFormat("Failed to load {0} {1} {2}", Objects[0].ToString(), Objects[0].RuntimeKeyIsValid(), op.PercentComplete);
                    //op.Completed += handle => Debug.LogFormat("Finished Loading {0} {1}", handle.IsDone, handle.Result);
                }
                else {
                    return op.Result;
                }
            }
            return Objects[index].Asset as TV;
        }

        public TV Get() {
            if (Objects.Count == 0) {
                return null;
            }
            if (Objects.Count == 1) {
                return Get(0);
            }
            //return Prefabs.SafeAccess((int) (Prefabs.Length * Curve.Evaluate(UnityEngine.Random.value)));
            return Get((int) (Objects.Count * Curve.Evaluate(Game.LevelRandom.NextFloat(0, 1))));
        }

        public int GetIndex() {
            return (int) (Objects.Count * Curve.Evaluate(Game.LevelRandom.NextFloat(0, 1)));
        }
    }

    [System.Serializable]
    public class RandomObjectHolder {
        public List<GameObjectReference> Objects = new List<GameObjectReference>();
        public AnimationCurve Curve = new AnimationCurve();

        public void Load() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            for (int i = 0; i < Objects.Count; i++) {
                Objects[i].LoadAssetAsync();
            }
        }

        public void Unload() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            for (int i = 0; i < Objects.Count; i++) {
                if (Objects[i].Asset == null) {
                    continue;
                }
                Objects[i].ReleaseAsset();
            }
        }

        public GameObject Get(int index) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return Objects[index].Asset as GameObject;
            }
#endif
            if (Objects[index].Asset == null) {
                var op = Objects[index].LoadAssetAsync();
                if (!op.IsDone) {
                    Debug.LogFormat("Failed to load {0} {1}", Objects[0].ToString(), op
                    .PercentComplete);
                    op.Completed += handle => Debug.LogFormat("Finished Loading {0} {1}", handle.IsDone, handle.Result);
                }
                else {
                    return op.Result;
                }
            }
            return Objects[index].Asset as GameObject;
        }

        public GameObject Get() {
            if (Objects.Count == 0) {
                return null;
            }
            if (Objects.Count == 1) {
                return Get(0);
            }
            //return Prefabs.SafeAccess((int) (Prefabs.Length * Curve.Evaluate(UnityEngine.Random.value)));
            return Get((int) (Objects.Count * Curve.Evaluate(Game.LevelRandom.NextFloat(0, 1))));
        }

        public int GetIndex() {
            return (int) (Objects.Count * Curve.Evaluate(Game.LevelRandom.NextFloat(0, 1)));
        }
    }

    [System.Serializable]
    public class RandomScriptableHolder {
        public UnityEngine.ScriptableObject[] Objects = new UnityEngine.ScriptableObject[0];
        public AnimationCurve Curve = new AnimationCurve();

        public UnityEngine.ScriptableObject Get() {
            if (Objects.Length == 0) {
                return null;
            }
            if (Objects.Length == 1) {
                return Objects[0];
            }
            return Objects.SafeAccess((int) (Objects.Length * Curve.Evaluate(Game.LevelRandom.NextFloat(0, 1))));
        }

        public int GetIndex() {
            return (int) (Objects.Length * Curve.Evaluate(Game.LevelRandom.NextFloat(0, 1)));
        }
    }
    public class SmoothRandom {
        private static FractalNoise s_Noise;
        private static Vector3 s_Result1;
        private static Vector3 s_Result2;
        private static FractalNoise Noise {
            get {
                if (s_Noise == null) {
                    s_Noise = new FractalNoise(1.27f, 2.04f, 8.36f);
                }
                return s_Noise;
            }
        }

        /// <summary>
        ///     Slightly refactored fractal noise class from the Procedular Examples package.
        /// </summary>
        private class FractalNoise {
            private float[] m_Exponent;
            private int m_IntOctaves;
            private float m_Lacunarity;
            private Perlin m_Noise;
            private float m_Octaves;

            public FractalNoise(float inH, float inLacunarity, float inOctaves) : this(inH, inLacunarity, inOctaves, null) {
            }

            public FractalNoise(float inH, float inLacunarity, float inOctaves, Perlin noise) {
                m_Lacunarity = inLacunarity;
                m_Octaves = inOctaves;
                m_IntOctaves = (int) inOctaves;
                m_Exponent = new float[m_IntOctaves + 1];
                float frequency = 1.0f;
                for (int i = 0; i < m_IntOctaves + 1; i++) {
                    m_Exponent[i] = (float) Math.Pow(m_Lacunarity, -inH);
                    frequency *= m_Lacunarity;
                }
                if (noise == null) {
                    m_Noise = new Perlin();
                }
                else {
                    m_Noise = noise;
                }
            }

            public float HybridMultifractal(float x, float y, float offset) {
                float weight, signal, remainder, result;
                result = (m_Noise.Noise(x, y) + offset) * m_Exponent[0];
                weight = result;
                x *= m_Lacunarity;
                y *= m_Lacunarity;
                int i;
                for (i = 1; i < m_IntOctaves; i++) {
                    if (weight > 1.0f) {
                        weight = 1.0f;
                    }
                    signal = (m_Noise.Noise(x, y) + offset) * m_Exponent[i];
                    result += weight * signal;
                    weight *= signal;
                    x *= m_Lacunarity;
                    y *= m_Lacunarity;
                }
                remainder = m_Octaves - m_IntOctaves;
                result += remainder * m_Noise.Noise(x, y) * m_Exponent[i];
                return result;
            }
        }

        /// <summary>
        ///     Slightly refactored perlin class from the Procedular Examples package.
        /// </summary>
        private class Perlin {
            // Original C code derived from 
            // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.c
            // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.h
            private const int B = 0x100;
            private const int BM = 0xff;
            private const int N = 0x1000;
            private float[] g1 = new float[B + B + 2];
            private float[,] g2 = new float[B + B + 2, 2];
            private float[,] g3 = new float[B + B + 2, 3];
            private int[] p = new int[B + B + 2];

            public Perlin() {
                int i, j, k;
                Random rnd = new Random();
                for (i = 0; i < B; i++) {
                    p[i] = i;
                    g1[i] = (float) (rnd.Next(B + B) - B) / B;
                    for (j = 0; j < 2; j++) {
                        g2[i, j] = (float) (rnd.Next(B + B) - B) / B;
                    }
                    Normalize2(ref g2[i, 0], ref g2[i, 1]);
                    for (j = 0; j < 3; j++) {
                        g3[i, j] = (float) (rnd.Next(B + B) - B) / B;
                    }
                    Normalize3(ref g3[i, 0], ref g3[i, 1], ref g3[i, 2]);
                }
                while (--i != 0) {
                    k = p[i];
                    p[i] = p[j = rnd.Next(B)];
                    p[j] = k;
                }
                for (i = 0; i < B + 2; i++) {
                    p[B + i] = p[i];
                    g1[B + i] = g1[i];
                    for (j = 0; j < 2; j++) {
                        g2[B + i, j] = g2[i, j];
                    }
                    for (j = 0; j < 3; j++) {
                        g3[B + i, j] = g3[i, j];
                    }
                }
            }

            private float At2(float rx, float ry, float x, float y) {
                return rx * x + ry * y;
            }

            private float At3(float rx, float ry, float rz, float x, float y, float z) {
                return rx * x + ry * y + rz * z;
            }

            private float Lerp(float t, float a, float b) {
                return a + t * (b - a);
            }

            private void Normalize2(ref float x, ref float y) {
                float s;
                s = (float) Math.Sqrt(x * x + y * y);
                x = y / s;
                y = y / s;
            }

            private void Normalize3(ref float x, ref float y, ref float z) {
                float s;
                s = (float) Math.Sqrt(x * x + y * y + z * z);
                x = y / s;
                y = y / s;
                z = z / s;
            }

            private float SCurve(float t) {
                return t * t * (3.0f - 2.0f * t);
            }

            private void Setup(float value, out int b0, out int b1, out float r0, out float r1) {
                float t = value + N;
                b0 = (int) t & BM;
                b1 = (b0 + 1) & BM;
                r0 = t - (int) t;
                r1 = r0 - 1.0f;
            }

            public float Noise(float arg) {
                int bx0, bx1;
                float rx0, rx1, sx, u, v;
                Setup(arg, out bx0, out bx1, out rx0, out rx1);
                sx = SCurve(rx0);
                u = rx0 * g1[p[bx0]];
                v = rx1 * g1[p[bx1]];
                return Lerp(sx, u, v);
            }

            public float Noise(float x, float y) {
                int bx0, bx1, by0, by1, b00, b10, b01, b11;
                float rx0, rx1, ry0, ry1, sx, sy, a, b, u, v;
                int i, j;
                Setup(x, out bx0, out bx1, out rx0, out rx1);
                Setup(y, out by0, out by1, out ry0, out ry1);
                i = p[bx0];
                j = p[bx1];
                b00 = p[i + by0];
                b10 = p[j + by0];
                b01 = p[i + by1];
                b11 = p[j + by1];
                sx = SCurve(rx0);
                sy = SCurve(ry0);
                u = At2(rx0, ry0, g2[b00, 0], g2[b00, 1]);
                v = At2(rx1, ry0, g2[b10, 0], g2[b10, 1]);
                a = Lerp(sx, u, v);
                u = At2(rx0, ry1, g2[b01, 0], g2[b01, 1]);
                v = At2(rx1, ry1, g2[b11, 0], g2[b11, 1]);
                b = Lerp(sx, u, v);
                return Lerp(sy, a, b);
            }

            public float Noise(float x, float y, float z) {
                int bx0, bx1, by0, by1, bz0, bz1, b00, b10, b01, b11;
                float rx0, rx1, ry0, ry1, rz0, rz1, sy, sz, a, b, c, d, t, u, v;
                int i, j;
                Setup(x, out bx0, out bx1, out rx0, out rx1);
                Setup(y, out by0, out by1, out ry0, out ry1);
                Setup(z, out bz0, out bz1, out rz0, out rz1);
                i = p[bx0];
                j = p[bx1];
                b00 = p[i + by0];
                b10 = p[j + by0];
                b01 = p[i + by1];
                b11 = p[j + by1];
                t = SCurve(rx0);
                sy = SCurve(ry0);
                sz = SCurve(rz0);
                u = At3(rx0, ry0, rz0, g3[b00 + bz0, 0], g3[b00 + bz0, 1], g3[b00 + bz0, 2]);
                v = At3(rx1, ry0, rz0, g3[b10 + bz0, 0], g3[b10 + bz0, 1], g3[b10 + bz0, 2]);
                a = Lerp(t, u, v);
                u = At3(rx0, ry1, rz0, g3[b01 + bz0, 0], g3[b01 + bz0, 1], g3[b01 + bz0, 2]);
                v = At3(rx1, ry1, rz0, g3[b11 + bz0, 0], g3[b11 + bz0, 1], g3[b11 + bz0, 2]);
                b = Lerp(t, u, v);
                c = Lerp(sy, a, b);
                u = At3(rx0, ry0, rz1, g3[b00 + bz1, 0], g3[b00 + bz1, 2], g3[b00 + bz1, 2]);
                v = At3(rx1, ry0, rz1, g3[b10 + bz1, 0], g3[b10 + bz1, 1], g3[b10 + bz1, 2]);
                a = Lerp(t, u, v);
                u = At3(rx0, ry1, rz1, g3[b01 + bz1, 0], g3[b01 + bz1, 1], g3[b01 + bz1, 2]);
                v = At3(rx1, ry1, rz1, g3[b11 + bz1, 0], g3[b11 + bz1, 1], g3[b11 + bz1, 2]);
                b = Lerp(t, u, v);
                d = Lerp(sy, a, b);
                return Lerp(sz, c, d);
            }
        }

        public static Vector3 GetVector3(float speed) {
            float time = Time.time * 0.01f * speed;
            s_Result1.Set(Noise.HybridMultifractal(time, 15.73f, 0.58f), Noise.HybridMultifractal(time, 63.94f, 0.58f), Noise.HybridMultifractal(time, 0.2f, 0.58f));
            return s_Result1;
        }

        public static Vector3 GetVector3Centered(float speed) {
            var time1 = Time.time * 0.01f * speed;
            var time2 = (Time.time - 1) * 0.01f * speed;
            s_Result1.Set(Noise.HybridMultifractal(time1, 15.73f, 0.58f), Noise.HybridMultifractal(time1, 63.94f, 0.58f), Noise.HybridMultifractal(time1, 0.2f, 0.58f));
            s_Result2.Set(Noise.HybridMultifractal(time2, 15.73f, 0.58f), Noise.HybridMultifractal(time2, 63.94f, 0.58f), Noise.HybridMultifractal(time2, 0.2f, 0.58f));
            return s_Result1 - s_Result2;
        }
    }

    public static class PhysicsExtensions {
        public static Vector3 ComputeGunLead(Vector3 targetPos, Vector3 targetVel, Vector3 ownPos, Vector3 ownVel, float muzzleVelocity) {
            // Figure out ETA for bullets to reach target.
            Vector3 predictedTargetPos = targetPos + targetVel;
            Vector3 predictedOwnPos = ownPos + ownVel;

            float range = Vector3.Distance(predictedOwnPos, predictedTargetPos);
            float timeToHit = range / muzzleVelocity;

            // Project velocity of target using the TimeToHit.
            Vector3 leadMarker = (targetVel - ownVel) * timeToHit + targetPos;

            return leadMarker;
        }
    }

    public static class RandomExtensions {

        public static float NextFloat(this Random random) {
            return random.NextFloat(0, 1);
        }

        public static float value(this Random random) {
            return random.NextFloat(0, 1);
        }

        public static float NextFloat(this Random random, double minValue, double maxValue) {
            return (float) (random.NextDouble() * (maxValue - minValue) + minValue);
        }

        public static float Range(this Random random, float min, float max) {
            return random.NextFloat(min, max);
        }

        public static int Range(this Random random, int min, int max) {
            return (int) ((max - min + 1) * (float) random.NextDouble()) + min;
        }

        public static T RandomElement<T>(this IList<T> list) {
            if (list.Count == 0) {
                return default(T);
            }
            return list[Game.Random.Next(0, list.Count)];
        }

        public static T RandomElement<T>(this IList<T> list, System.Random random) {
            if (list.Count == 0) {
                return default(T);
            }
            return list[random.Next(0, list.Count)];
        }

        public static int RandomIndex<T>(this IList<T> list) {
            if (list.Count == 0) {
                return -1;
            }
            return Game.Random.Next(0, list.Count);
        }

        public static int RandomIndex<T>(this IList<T> list, System.Random random) {
            if (list.Count == 0) {
                return -1;
            }
            return random.Next(0, list.Count);
        }

        public static int LastIndex<T>(this IList<T> list) {
            return list.Count - 1;
        }

        public static void RemoveLast<T>(this IList<T> list) {
            list.RemoveAt(list.Count - 1);
        }

        public static T PopLast<T>(this IList<T> list) {
            var element = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return element;
        }

        public static T LastElement<T>(this IList<T> list) {
            if (list.Count == 0) {
                return default(T);
            }
            return list[list.Count - 1];
        }

        public static void Shuffle<T>(this IList<T> list, Random rnd) {
            for (var i = 0; i < list.Count - 1; i++) {
                //list.Swap(i, rnd.Next(i, list.Count));
                var temp = list[i];
                var j = rnd.Next(i, list.Count);
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public static void Shuffle<T>(this IList<T> list) {
            Shuffle<T>(list, Game.Random);
        }

        public static void ShuffleArray(this Array list) {
            var rolls = new int[list.Length];
            for (var i = 0; i < list.Length; i++) {
                rolls[i] = Game.Random.DiceRoll();
            }
            System.Array.Sort(rolls, list);
        }

        public static void Swap<T>(this IList<T> list, int i, int j) {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static bool WithinRange<T>(this IList<T> list, int index) {
            return index >= 0 && index < list.Count;
        }

        public const float MaxDiceRoll = 100f;

        public static bool DiceRollSucess(this System.Random random, float chance) {
            if (chance < 1 && chance > 0) {
                chance *= 100;
            }
            var roll = random.Next(0, 101);
            return roll <= chance;
        }

        public static bool DiceRollSucess(this System.Random random, int chance) {
            var roll = random.Next(0, 101);
            return roll <= chance;
        }

        public static int DiceRoll(this System.Random random) {
            return random.Next(0, 101);
        }

        public static bool CoinFlip(this System.Random random) {
            return random.Next(0, 101) < 50;
        }

        private static FloatRange _standardYClamp = new FloatRange(-15, 15);

        public static Vector3 RandomSpherePosition(Vector3 center, float distance) {
            var pos = center + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(distance * 0.5f, distance);
            pos.y = _standardYClamp.Clamp(pos.y);
            return pos;
        }

        public static Vector2 RandomCirclePosition(Vector2 center, float distance) {
            return center + UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(distance * 0.5f, distance);
        }

        public static Vector2 RandomCirclePosition(Vector2 center, float distance, System.Random random) {
            var angle = random.NextDouble() * Math.PI * 2;
            var radius = Math.Sqrt(random.NextDouble()) * distance;
            var x = center.x + radius * Math.Cos(angle);
            var y = center.y + radius * Math.Sin(angle);
            return new Vector2((float) x, (float) y);
        }

        //public static IComparable<T> QuickSort<T>(this IComparable<T> list, int start, int end) {
        //    int lenght = end - start;
        //    if (lenght < 1) {
        //        return list;
        //    }
        //    int divider = start;
        //    int pivot = end - 1;
        //    for (int position = start; position < end; position++) {
        //    }
        //}
    }

    public static class Algorithms {
        private static void Swap<T>(ref T lhs, ref T rhs) {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// The plot function delegate
        /// </summary>
        /// <param name="x">The x co-ord being plotted</param>
        /// <param name="y">The y co-ord being plotted</param>
        /// <returns>True to continue, false to stop the algorithm</returns>
        public delegate bool PlotFunction(int x, int y);

        /// <summary>
        /// Plot the line stopping if plot returns false
        /// </summary>
        public static void Line(Point3 origin, Point3 dest, PlotFunction plot) {
            bool steep = Math.Abs(dest.z - origin.z) > Math.Abs(dest.x - origin.x);
            if (steep) {
                Swap<int>(ref origin.x, ref origin.z);
                Swap<int>(ref dest.x, ref dest.z);
            }
            if (origin.x > dest.x) {
                Swap<int>(ref origin.x, ref dest.x);
                Swap<int>(ref origin.z, ref dest.z);
            }
            int dX = (dest.x - origin.x);
            int dZ = Math.Abs(dest.z - origin.z);
            int err = (dX / 2);
            int ystep = (origin.z < dest.z ? 1 : -1), z = origin.z;
            for (int x = origin.x; x <= dest.x; ++x) {
                if (steep && !plot(z, x)) {
                    return;
                }
                if (!steep && !plot(x, z)) {
                    return;
                }
                err = err - dZ;
                if (err < 0) {
                    z += ystep;
                    err += dX;
                }
            }
        }

        public static bool CanReach(Point3 origin, Point3 dest) {
            bool steep = Math.Abs(dest.z - origin.z) > Math.Abs(dest.x - origin.x);
            if (steep) {
                Swap<int>(ref origin.x, ref origin.z);
                Swap<int>(ref dest.x, ref dest.z);
            }
            if (origin.x > dest.x) {
                Swap<int>(ref origin.x, ref dest.x);
                Swap<int>(ref origin.z, ref dest.z);
            }
            int dX = (dest.x - origin.x);
            int dZ = Math.Abs(dest.z - origin.z);
            int err = (dX / 2);
            int ystep = (origin.z < dest.z ? 1 : -1), z = origin.z;
            //var prev = origin;
            for (int x = origin.x; x <= dest.x; ++x) {
                //var pos = steep ? new Point3(z, 0, x) : new Point3(x, 0, z);
                err = err - dZ;
                if (err < 0) {
                    z += ystep;
                    err += dX;
                }
            }
            return true;
        }

        //Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
        //http://www.iquilezles.org/www/articles/minispline/minispline.htm
        public static Vector3 GetCatmullRomPosition(float t, Vector3 pre, Vector3 start, Vector3 end, Vector3 post) {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            Vector3 a = 2f * start;
            Vector3 b = end - pre;
            Vector3 c = 2f * pre - 5f * start + 4f * end - post;
            Vector3 d = -pre + 3f * start - 3f * end + post;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }
    }

    public static class CameraExtensions {
        public static Bounds OrthographicBounds(this Camera camera) {
            float screenAspect = (float) Screen.width / (float) Screen.height;
            float cameraHeight = camera.orthographicSize * 2;
            Bounds bounds = new Bounds(
                camera.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }

        public static void FaceCamera(this Camera cam, Transform tr, bool backwards) {
            tr.LookAt(
                tr.position + cam.transform.rotation * (backwards ? Vector3.back : Vector3.forward),
                cam.transform.rotation * Vector3.up);
        }

        public static Ray EditorRaycast(this Camera current) {
            return current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Camera.current.pixelHeight - Event.current.mousePosition.y, 0.0f));
        }
    }

    public static class FloatExtension {
        public static float ToFloatPercentFromBase100(this float fltVal) {
            return fltVal * 0.01f;
        }

        public static float CheckNormalized(this float current) {
            if (current > 5f) {
                //I'm assuming anything greater than normalized X00% is actually a straight percent
                return current * 0.01f;
            }
            return current;
        }

        public static float LevelAddition(this float val, float amount) {
            return val + (val * amount);
        }
    }

    public static class IntExtension {

        public static float ToFloatPercentFromBase100(this int intVal) {
            return intVal * 0.01f;
        }

        public static int LevelAddition(this int intVal, float amount) {
            return (int) (intVal + (intVal * amount));
        }

        public static string ToRomanNumeral(this int value) {
            if (value < 0) {
                throw new ArgumentOutOfRangeException("Please use a positive integer greater than zero.");
            }
            StringBuilder sb = new StringBuilder();
            int remain = value;
            while (remain > 0) {
                if (remain >= 1000) {
                    sb.Append("M");
                    remain -= 1000;
                }
                else if (remain >= 900) {
                    sb.Append("CM");
                    remain -= 900;
                }
                else if (remain >= 500) {
                    sb.Append("D");
                    remain -= 500;
                }
                else if (remain >= 400) {
                    sb.Append("CD");
                    remain -= 400;
                }
                else if (remain >= 100) {
                    sb.Append("C");
                    remain -= 100;
                }
                else if (remain >= 90) {
                    sb.Append("XC");
                    remain -= 90;
                }
                else if (remain >= 50) {
                    sb.Append("L");
                    remain -= 50;
                }
                else if (remain >= 40) {
                    sb.Append("XL");
                    remain -= 40;
                }
                else if (remain >= 10) {
                    sb.Append("X");
                    remain -= 10;
                }
                else if (remain >= 9) {
                    sb.Append("IX");
                    remain -= 9;
                }
                else if (remain >= 5) {
                    sb.Append("V");
                    remain -= 5;
                }
                else if (remain >= 4) {
                    sb.Append("IV");
                    remain -= 4;
                }
                else if (remain >= 1) {
                    sb.Append("I");
                    remain -= 1;
                }
                else {
                    throw new Exception("Unexpected error."); // <<-- shouldn't be possble to get here, but it ensures that we will never have an infinite loop (in case the computer is on crack that day).
                }
            }
            return sb.ToString();
        }
    }

    public static class GeometryExtensions {
        public static Vector3[] GetAllPoints(this Bounds bounds) {
            return new Vector3[] {
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            };
        }

        public static Vector3[] GetAllPointsVec(this Bounds bounds) {
            return new Vector3[] {
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),

                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),

                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),

                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),


                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),

                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),

                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),

                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),


                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),

                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),

                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),

                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),

            };
        }

        public static List<Vector3> MatrixMultiply(this List<Vector3> points, Matrix4x4 matrix = default(Matrix4x4)) {
            var result = new List<Vector3>(points.Count);
            for (int i = 0; i < points.Count; i++) {
                result.Add(matrix.MultiplyPoint(points[i]));
            }
            return result;
        }

        public static Bounds GetEnclosingBounds(this List<Vector3> points) {
            if (points.Count == 0) {
                return default(Bounds);
            }
            var bounds = new Bounds(points[0], Vector3.zero);
            for (var i = 1; i < points.Count; i++) {
                bounds.Encapsulate(points[i]);
            }
            return bounds;
        }
    }

    public static class RectTrExtensions {
        public static void SetAnchors(this RectTransform tr, Vector2 anchor) {
            tr.anchorMin = anchor;
            tr.anchorMax = anchor;
        }

        public static void SetAnchorsAndPivots(this RectTransform tr, Vector2 anchor) {
            tr.anchorMin = anchor;
            tr.anchorMax = anchor;
            tr.pivot = anchor;
        }

        public static RectTransform Resize(this RectTransform transform, float percentage, float arcSize) {
            RectTransform parent = transform.parent as RectTransform;
            float arcPercentage = arcSize / parent.rect.width;
            transform.sizeDelta = new Vector2(parent.rect.width * MathEx.Min(percentage, arcPercentage), parent.rect.height * MathEx.Min(percentage, arcPercentage));
            return transform;
        }

        public static RectTransform AnchorsToCorners(this RectTransform transform) {
            RectTransform t = transform;
            RectTransform pt = t.parent as RectTransform;

            if (pt == null) {
                return transform;
            }

            Vector2 newAnchorsMin = new Vector2(
                t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                t.anchorMin.y + t.offsetMin.y / pt.rect.height);
            Vector2 newAnchorsMax = new Vector2(
                t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                t.anchorMax.y + t.offsetMax.y / pt.rect.height);

            t.anchorMin = newAnchorsMin;
            t.anchorMax = newAnchorsMax;
            t.offsetMin = t.offsetMax = new Vector2(0, 0);
            return t;
        }

        public static RectTransform CornersToAnchors(this RectTransform transform) {
            RectTransform t = transform as RectTransform;

            t.offsetMin = t.offsetMax = new Vector2(0, 0);

            return t;
        }

        public static Vector2 ViewportToWorldSpaceUI(this RectTransform transform, Vector3 pos, Vector3 dir) {
            Vector2 result = Vector2.zero;

            Vector3 localPos = transform.InverseTransformPoint(pos);
            Vector3 localDir = transform.InverseTransformDirection(dir);

            result = (Vector2) localPos + ((Vector2) localDir * (Mathf.Abs(localPos.z) / localDir.z));

            return result;
        }
    }

    public static class DirectoryExtensions {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dirInfo, params string[] extensions) {
            var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            return dirInfo.EnumerateFiles().Where(f => allowedExtensions.Contains(f.Extension));
        }
    }

    public static class DictionaryExtensions {
        public static bool TryAdd<T, TV>(this Dictionary<T, TV> dict, T key, TV val) {
            if (dict.ContainsKey(key)) {
                return false;
            }
            dict.Add(key, val);
            return true;
        }

        public static void AddOrUpdate<T, TV>(this Dictionary<T, TV> dict, T key, TV val) {
            if (dict.ContainsKey(key)) {
                dict[key] = val;
            }
            else {
                dict.Add(key, val);
            }
        }

        public static TV GetOrAdd<T, TV>(this Dictionary<T, TV> dict, T key) where TV : new() {
            if (!dict.TryGetValue(key, out var value)) {
                value = new TV();
                dict.Add(key, value);
            }
            return value;
        }

        public static string EncodeToString(this Dictionary<string, string> dict) {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in dict) {
                sb.Append(pair.Key);
                sb.Append('-');
                sb.AppendEntryBreak(pair.Value);
            }
            return sb.ToString();
        }

        public static void DecodeFromString(this Dictionary<string, string> dict, string data) {
            var list = data.SplitMultiEntry();
            for (int i = 0; i < list.Count; i++) {
                var entry = list[i].Split('-');
                if (entry.Length < 2 || dict.ContainsKey(entry[0])) {
                    continue;
                }
                dict.Add(entry[0], entry[1]);
            }
        }

        public static T RetrieveEnum<T>(this Dictionary<string, string> dict, string key, T defaultVal) {
            string data;
            if (!dict.TryGetValue(key, out data)) {
                return defaultVal;
            }
            T val;
            return EnumHelper.TryParse(data, out val) ? val : defaultVal;
        }
        
        public static string Retrieve(this Dictionary<string, string> dict, string key, string defaultVal) {
            string data;
            return !dict.TryGetValue(key, out data) ? defaultVal : data;
        }

        public static Color Retrieve(this Dictionary<string, string> dict, string key, Color defaultVal) {
            string data;
            if (!dict.TryGetValue(key, out data)) {
                return defaultVal;
            }
            Color color;
            return ColorUtility.TryParseHtmlString(data, out color) ? color : defaultVal;
        }

        public static int Retrieve(this Dictionary<string, string> dict, string key, int defaultVal) {
            string data;
            if (!dict.TryGetValue(key, out data)) {
                return defaultVal;
            }
            int val;
            return int.TryParse(data, out val) ? val : defaultVal;
        }

        public static float Retrieve(this Dictionary<string, string> dict, string key, float defaultVal) {
            string data;
            if (!dict.TryGetValue(key, out data)) {
                return defaultVal;
            }
            float val;
            return float.TryParse(data, out val) ? val : defaultVal;
        }

        public static bool Retrieve(this Dictionary<string, string> dict, string key, bool defaultVal) {
            string data;
            if (!dict.TryGetValue(key, out data)) {
                return defaultVal;
            }
            bool val;
            return bool.TryParse(data, out val) ? val : defaultVal;
        }

        public static void Encode(this Dictionary<string, string> dict, string key, string value) {
            if (dict.ContainsKey(key)) {
                dict[key] = value;
            }
            else {
                dict.Add(key, value);
            }
        }

        public static void Encode(this Dictionary<string, string> dict, string key, Color color) {
            if (dict.ContainsKey(key)) {
                dict[key] = ColorUtility.ToHtmlStringRGBA(color);
            }
            else {
                dict.Add(key, ColorUtility.ToHtmlStringRGBA(color));
            }
        }

        public static void EncodeEnum<T>(this Dictionary<string, string> dict, string key, int value) where T : struct, IConvertible {
            if (dict.ContainsKey(key)) {
                dict[key] = EnumHelper.GetString<T>(value);
            }
            else {
                dict.Add(key, EnumHelper.GetString<T>(value));
            }
            
        }
    }

    public static class SerializationExtensions {
        
        public static T GetValue<T>(this SerializationInfo self, string name, T currentValue) {
            //return (T) self.GetValue(name, typeof(T));
            T value;
            try {
                value = (T) self.GetValue(name, typeof(T));
            }
            catch (Exception e) {
                Debug.LogFormat("Name {0} Target {1} Stack {2}", name, e.TargetSite.ToString(), e.StackTrace);
                value = currentValue;
            }
            return value;
        }

        public static T GetValue<T>(this SerializationInfo self, string name) {
            //return (T) self.GetValue(name, typeof(T));
            T value;
            try {
                value = (T) self.GetValue(name, typeof(T));
            }
            catch (Exception e) {
                Debug.LogFormat("Name {0} {1} {2}", name, e.TargetSite.ToString(), e.StackTrace);
                value = default(T);
            }
            return value;
        }

        public static Color GetValue(this SerializationInfo self, string name, Color currentValue) {
            //return (T) self.GetValue(name, typeof(T));
            Color value;
            try {
                string id = (string) self.GetValue(name, typeof(string));
                if (id.Length > 0 && id[0] != '#') {
                    id = id.Insert(0, "#");
                }
                if (!ColorUtility.TryParseHtmlString(id, out value)) {
                    value = currentValue;
                }
            }
            catch (Exception e) {
                Debug.LogFormat("Name {0} {1} {2}", name, e.TargetSite.ToString(), e.StackTrace);
                value = currentValue;
            }
            return value;
        }

        public static void AddValue(this SerializationInfo self, string name, Color color) {
            self.AddValue(name, ColorUtility.ToHtmlStringRGBA(color), typeof(string));
        }

        public static void AddColor(this SerializationInfo self, string name, Color color) {
            self.AddValue(name, ColorUtility.ToHtmlStringRGBA(color), typeof(string));
        }

        public static T Deserialize<T>(this SerializationInfo info, T val) {
            string name = "";
            try {
                name = nameof(val);
            }
            catch (Exception e) {
                Debug.LogFormat("{0} {1} {2}", e.Source, e.TargetSite.ToString(), e.StackTrace);
                throw;
            }
            T value = default(T);
            try {
                value = (T) info.GetValue(name, typeof(T));
            }
            catch (Exception e) {
                Debug.LogFormat("Name {0} {1} {2}", name, e.TargetSite.ToString(), e.StackTrace);
                throw;
            }
            //return (T) info.GetValue(name, typeof(T));
            return value;
        }
    }

    public static class TrExtensions {

        //public static void DeleteChildren(this Transform tr) {
        //    var children = new List<GameObject>();
        //    foreach (Transform child in tr) {
        //        children.Add(child.gameObject);
        //    }
        //    for (var i = 0; i < children.Count; i++) {
        //        DestroyImmediate(children[i]);
        //    }
        //}

        public static bool IsEnvironment(this Transform tr) {
            if (tr.CompareTag(StringConst.TagEnvironment) ||
                tr.gameObject.layer == LayerMasks.NumberWall ||
                tr.gameObject.layer == LayerMasks.NumberFloor ||
                tr.gameObject.layer == LayerMasks.NumberCeiling) {
                return true;
            }
            return false;
        }

        public static void SetParentResetPos(this Transform child, Transform parent) {
            child.SetParent(parent, true);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        public static void ResetPos(this Transform child) {
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        public static Vector3 Direction(this Transform tr, Directions dir) {
            switch (dir) {
                case Directions.Forward:
                    return tr.forward;
                case Directions.Right:
                    return tr.right;
                case Directions.Back:
                    return -tr.forward;
                case Directions.Left:
                    return -tr.right;
                case Directions.Up:
                    return tr.up;
                case Directions.Down:
                    return -tr.up;
            }
            return tr.forward;
        }

        public static Directions ForwardDirection(this Transform tr) {
            var v = tr.forward;
            v.y = 0;
            v.Normalize();
            if (Vector3.Angle(v, Vector3.forward) <= 45.0) {
                return Directions.Forward;
            }
            if (Vector3.Angle(v, Vector3.right) <= 45.0) {
                return Directions.Right;
            }
            if (Vector3.Angle(v, Vector3.back) <= 45.0) {
                return Directions.Back;
            }
            if (Vector3.Angle(v, Vector3.left) <= 45.0) {
                return Directions.Left;
            }
            v.y = tr.forward.y;
            if (Vector3.Angle(v, Vector3.up) <= 45.0) {
                return Directions.Up;
            }
            return Directions.Down;
        }

        public static string GetPath(this Transform tr) {
            string path = tr.name;
            while (tr.parent != null) {
                tr = tr.parent;
                if (tr.parent == null) {
                    break;
                }
                path += tr.name + "/";
            }
            return path;
        }
        
        public static Directions ForwardDirection2D(this Transform tr) {
            var v = tr.forward;
            v.y = 0;
            v.Normalize();
            if (Vector3.Angle(v, Vector3.forward) <= 45.0) {
                return Directions.Forward;
            }
            if (Vector3.Angle(v, Vector3.right) <= 45.0) {
                return Directions.Right;
            }
            if (Vector3.Angle(v, Vector3.back) <= 45.0) {
                return Directions.Back;
            }
            return Directions.Left;
        }

        private static WhileLoopLimiter _deleteLimiter = new WhileLoopLimiter(15000);
        
        public static void DeleteChildren(this Transform tr) {
//#if UNITY_EDITOR
//            if (Application.isPlaying) {
//                TimeManager.StartUnscaled(SlowDelete(tr));
//                return;
//            }
//#else
//            TimeManager.StartUnscaled(SlowDelete(tr));
//            return;
//#endif
            int i = 0;
            _deleteLimiter.Reset();
            while (tr.childCount > 0) {
                if (tr.childCount >= i) {
                    i = 0;
                }
                var child = tr.GetChild(i);
                i++;
                if (!_deleteLimiter.Advance()) {
                    break;
                }
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(child.gameObject);
#else
            UnityEngine.Object.Destroy(child.gameObject);
#endif
            }
        }

        private static IEnumerator SlowDelete(Transform tr) {
            int i = 0;
            var breakLimit = 0;
            _deleteLimiter.Reset();
            while (tr.childCount > 0) {
                if (tr.childCount >= i) {
                    i = 0;
                }
                var child = tr.GetChild(i);
                i++;
                breakLimit++;
                if (!_deleteLimiter.Advance()) {
                    break;
                }
                UnityEngine.Object.Destroy(child.gameObject);
                if (breakLimit > 200) {
                    breakLimit = 0;
                    yield return null;
                }
            }

        }

        public static void DespawnChildren(this Transform tr) {
            _deleteLimiter.Reset();
            while (tr.childCount > 0) {
                var child = tr.GetChild(0);
                if (child.SafeIsUnityNull() || child == null) {
                    continue;
                }
                if (!_deleteLimiter.Advance()) {
                    break;
                }
                ItemPool.Despawn(child.gameObject);
            }
        }

        public static Quaternion GetLookAtRotation(this Transform me, Transform target) {
            if (me == null || target == null) {
                return Quaternion.identity;
            }
            var targetPos = target.position;
            targetPos.y = me.position.y;
            Vector3 relativePos = (targetPos - me.position).normalized;
            return Quaternion.LookRotation(relativePos);
        }
    }

    public static class RigidbodyExtensions {

        /// <summary>
        ///     Calculate the maximum speed of this Rigidbody for a given force.
        /// </summary>
        /// <param name="rb">Rigidbody</param>
        /// <param name="force">The linear force to be used in the calculation.</param>
        /// <returns>The maximum speed.</returns>
        public static float GetSpeedFromForce(this Rigidbody rb, float force) {
            float deltaVThrust = force / rb.mass * Time.fixedDeltaTime;
            float dragFactor = Time.fixedDeltaTime * rb.drag;
            float maxSpeed = deltaVThrust / dragFactor;
            return maxSpeed;
        }

        public static void ApplyDamageForce(this Rigidbody rb, Vector3 dir, float physAmt) {
            rb.AddForce(dir * physAmt, ForceMode.Impulse);
        }

        public static void SetPhysics(this Rigidbody rb, bool enabled) {
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
        }

        public static Vector3 CalculateThrowVelocity(Vector3 origin, Vector3 target, float timeToTarget) {
            // calculate vectors
            Vector3 toTarget = target - origin;
            Vector3 toTargetXZ = toTarget;
            toTargetXZ.y = 0;

            // calculate xz and y
            float y = toTarget.y;
            float xz = toTargetXZ.magnitude;

            // calculate starting speeds for xz and y. Physics forumulase deltaX = v0 * t + 1/2 * a * t * t
            // where a is "-gravity" but only on the y plane, and a is 0 in xz plane.
            // so xz = v0xz * t => v0xz = xz / t
            // and y = v0y * t - 1/2 * gravity * t * t => v0y * t = y + 1/2 * gravity * t * t => v0y = y / t + 1/2 * gravity * t
            float t = timeToTarget;
            float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
            float v0xz = xz / t;

            // create result vector for calculated starting speeds
            Vector3 result = toTargetXZ.normalized; // get direction of xz but with magnitude 1
            result *= v0xz; // set magnitude of xz to v0xz (starting speed in xz plane)
            result.y = v0y; // set y to v0y (starting speed of y plane)

            return result;
        }

        //public static Vector3 CalculateVelocity(Vector3 origin, Vector3 target, float angle = 20f) {
        //    var range = Vector3.Distance(origin, target);
        //    Vector3 v = new Vector3(0, Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
        //    return v * Mathf.Sqrt(range * Physics.gravity.magnitude / Mathf.Sin(2*angle*Mathf.Deg2Rad));
        //}

        public static Vector3 CalculateVelocity(Vector3 origin, Vector3 target, float initialAngle) {

            float gravity = Physics.gravity.magnitude;
            // Selected angle in radians
            float angle = initialAngle * Mathf.Deg2Rad;

            // Positions of this object and the target on the same plane
            Vector3 planarTarget = new Vector3(target.x, 0, target.z);
            Vector3 planarPostion = new Vector3(origin.x, 0, origin.z);

            // Planar distance between objects
            float distance = Vector3.Distance(planarTarget, planarPostion);
            // Distance along the y axis between objects
            float yOffset = origin.y - target.y;

            float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt(
                                        (0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) +
                                                                                     yOffset));

            Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

            // Rotate our velocity to match the direction between the two objects
//        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
            float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (target.x > origin.x ? 1 : -1);

            Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
            return finalVelocity;
            // Alternative way:
            // rigid.AddForce(finalVelocity * rigid.mass, ForceMode.Impulse);
        }
        
    }

    [System.Serializable]
    public struct Vector3Pair {
        public readonly Vector3 Origin;
        public readonly Vector3 Target;

        public Vector3Pair(Vector3 origin, Vector3 target) {
            Origin = origin;
            Target = target;
        }
    }

    public static class VIntExtensions {
        public static Vector3 GenericGridToWorld(this Vector2Int position, float gridSize) {
            return new Vector3(position.x * gridSize, 0, position.y * gridSize);
        }

        public static Vector3 GenericGridToWorld(this Vector3Int position, float gridSize) {
            return new Vector3(position.x * gridSize, position.y * gridSize, position.z * gridSize);
        }

        public static Vector3Int WorldToGenericGridInt3(Vector3 position, float gridSize) {
            return new Vector3Int(
                (int) Math.Round((double) position.x / gridSize),
                (int) Math.Round((double) position.y / gridSize),
                (int) Math.Round((double) position.z / gridSize));
        }

        public static Vector2Int WorldToGenericGridInt2(Vector3 position, float gridSize) {
            return new Vector2Int(
                (int) Math.Round((double) position.x / gridSize),
                (int) Math.Round((double) position.z / gridSize));
        }
    }

    public static class QuaternionExtensions {
        public static Vector3 GetPosition(this Quaternion rotation, Vector3 position, float distance) {
            Vector3 direction = rotation * Vector3.forward;
            return position + (direction * distance);
        }
    }

    public static class V3Extensions {

        public static Point3 toPoint3(this Vector3 v3) {
            return new Point3(v3);
        }

        public static Point3 toPoint3ZeroY(this Vector3 v3) {
            return new Point3(v3.x, 0, v3.z);
        }

        public static float AbsMax(this Vector3 v3) {
            return Mathf.Max(Mathf.Abs(v3.y), Mathf.Abs(v3.x), Mathf.Abs(v3.z));
        }

        public static float ManualDot(Vector3 a, Vector3 b) {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static float ManualSqrMagnitude(Vector3 v) {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }

        public static Vector3 Multiply(this Vector3 v3, Vector3 other) {
            return new Vector3(v3.x * other.x, v3.y * other.y, v3.z * other.z);
        }

        public static Directions GetDirection(this Vector3 t1, Vector3 t2) {
            if (t1.z < t2.z) {
                return Directions.Forward;
            }

            if (t1.x < t2.x) {
                return Directions.Right;
            }

            if (t1.z > t2.z) {
                return Directions.Back;
            }

            return Directions.Left;
        }

        public static Vector3 Clamp(this Vector3 v3, float min, float max) {
            for (int i = 0; i < 3; i++) {
                v3[i] = Mathf.Clamp(v3[i], min, max);
            }
            return v3;
        }

        public static float EuclideanDistance(this Vector3 t1, Vector3 t2) {
            return Mathf.Abs(t1.x - t2.x) + Mathf.Abs(t1.y - t2.y) + Mathf.Abs(t1.z - t2.z);
        }

        public static float DistanceChebyshev(this Vector3 t1, Vector3 t2) {
            return MathEx.Max(Mathf.Abs(t1.x - t2.x), Mathf.Abs(t1.z - t2.z));
        }

        public static float SqrDistanceXz(this Vector3 t1, Vector3 t2) {
            return new Vector3(t2.x - t1.x, 0, t2.z - t1.z).sqrMagnitude;
        }

        public static float SqrDistance(this Vector3 t1, Vector3 t2) {
            return new Vector3(t2.x - t1.x, t2.y - t1.y, t2.z - t1.z).sqrMagnitude;
        }

        public static float SqrDistance(this Vector3 t1, Point3 t2) {
            return new Vector3(t2.x - t1.x, t2.y - t1.y, t2.z - t1.z).sqrMagnitude;
        }

        public static float DistanceSquared(this Vector3 a, Vector3 b) {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static Directions EulerToDirection(this Vector3 eulerRot) {
            var angle = eulerRot.y;
            if (angle >= -45 && angle < 45) {
                return Directions.Forward;
            }
            if (angle >= 45 && angle < 135) {
                return Directions.Right;
            }
            if (angle >= 135 && angle < 225) {
                return Directions.Back;
            }
            if (angle >= 225 && angle < 315) {
                return Directions.Left;
            }
            if (angle > 315) {
                return Directions.Forward;
            }
            return Directions.Left;
        }


        public static Point3 WorldToGenericGrid(this Vector3 position, float gridSize) {
            return new Point3(
                (int) Math.Round((double) position.x / gridSize),
                (int) Math.Round((double) position.y / gridSize),
                (int) Math.Round((double) position.z / gridSize));
        }

        public static Point3 WorldToGenericGridYZero(this Vector3 position, float gridSize) {
            return new Point3(
                (int) Math.Round((double) position.x / gridSize), 0,
                (int) Math.Round((double) position.z / gridSize));
        }

        public static Vector3 Rounded(this Vector3 position, float amount) {
            return new Vector3(
                Mathf.Round(position.x / amount) * amount,
                Mathf.Round(position.y / amount) * amount,
                Mathf.Round(position.z / amount) * amount);
        }

        //public override string ToStringSimple() {
        //    return UnityString.Format("({0:F1}, {1:F1}, {2:F1})", new object[]
        //    {
        //            this.x,
        //            this.y,
        //            this.z
        //    });
        //}

        public static float XZSqrMagnitude(Vector3 a, Vector3 b) {
            float dx = b.x - a.x;
            float dz = b.z - a.z;
            return dx * dx + dz * dz;
        }

        public static float SqrMagnitude(Vector3 a, Vector3 b) {
            return (a - b).sqrMagnitude;
        }

        public static float AbsDistance(Vector3 p1, Vector3 p2) {
            return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) + Mathf.Abs(p1.z - p2.z);
        }

        public static float AbsDistance(Point3 p1, Point3 p2) {
            return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) + Mathf.Abs(p1.z - p2.z);
        }

    }

    public static class CornerDirectionsExtensions {

        public static void GetAdjacentCorners(this Directions direction, out CornerDirections dir01, out CornerDirections dir02) {
            switch (direction) {
                default:
                case Directions.Forward:
                    dir01 = CornerDirections.NorthWest;
                    dir02 = CornerDirections.NorthEast;
                    break;
                case Directions.Right:
                    dir01 = CornerDirections.NorthEast;
                    dir02 = CornerDirections.SouthEast;
                    break;
                case Directions.Back:
                    dir01 = CornerDirections.SouthEast;
                    dir02 = CornerDirections.SouthWest;
                    break;
                case Directions.Left:
                    dir01 = CornerDirections.SouthWest;
                    dir02 = CornerDirections.NorthWest;
                    break;
            }
        }

        public static Vector3 GetCornerPositionV3(this CornerDirections diagonalDirection) {
            switch (diagonalDirection) {
                case CornerDirections.NorthWest:
                    return Vector3.forward + Vector3.left;
                case CornerDirections.NorthEast:
                    return Vector3.forward + Vector3.right;
                case CornerDirections.SouthEast:
                    return Vector3.back + Vector3.right;
                case CornerDirections.SouthWest:
                    return Vector3.back + Vector3.left;
                default:
                    return Vector3.zero;
            }
        }

    }

    public static class DirectionsExtensions {
        public static Directions OppositeDir(this Directions dir) {
            switch (dir) {
                case Directions.Forward:
                    return Directions.Back;
                case Directions.Right:
                    return Directions.Left;
                case Directions.Left:
                    return Directions.Right;
                case Directions.Back:
                default:
                    return Directions.Forward;
                case Directions.Up:
                    return Directions.Down;
                case Directions.Down:
                    return Directions.Up;
            }
        }

        public static DirectionsEight ToDirectionEight(this Directions dir) {
            if (dir == Directions.Forward) {
                return DirectionsEight.Front;
            }
            if (dir == Directions.Right) {
                return DirectionsEight.Right;
            }
            if (dir == Directions.Left) {
                return DirectionsEight.Left;
            }
            if (dir == Directions.Back) {
                return DirectionsEight.Rear;
            }
            if (dir == Directions.Up) {
                return DirectionsEight.Top;
            }
            if (dir == Directions.Down) {
                return DirectionsEight.Bottom;
            }
            return DirectionsEight.Front;
        }

        public static Directions ToCardinalDir(this DirectionsEight dir) {
            switch (dir) {
                case DirectionsEight.Front:
                    return Directions.Forward;
                case DirectionsEight.Right:
                    return Directions.Right;
                case DirectionsEight.Left:
                    return Directions.Left;
                case DirectionsEight.Rear:
                    return Directions.Back;
                case DirectionsEight.Top:
                    return Directions.Up;
                case DirectionsEight.Bottom:
                    return Directions.Down;
            }
            return Directions.Forward;
        }

        public static DirectionsEight OppositeDir(this DirectionsEight dir) {
            if (dir == DirectionsEight.Front) {
                return DirectionsEight.Rear;
            }
            if (dir == DirectionsEight.Right) {
                return DirectionsEight.Left;
            }
            if (dir == DirectionsEight.Left) {
                return DirectionsEight.Right;
            }
            if (dir == DirectionsEight.Rear) {
                return DirectionsEight.Front;
            }
            if (dir == DirectionsEight.Top) {
                return DirectionsEight.Bottom;
            }
            if (dir == DirectionsEight.Bottom) {
                return DirectionsEight.Top;
            }
            if (dir == DirectionsEight.FrontRight) {
                return DirectionsEight.RearLeft;
            }
            if (dir == DirectionsEight.RearLeft) {
                return DirectionsEight.FrontRight;
            }
            if (dir == DirectionsEight.RearRight) {
                return DirectionsEight.FrontLeft;
            }
            if (dir == DirectionsEight.FrontLeft) {
                return DirectionsEight.RearRight;
            }
            return DirectionsEight.Front;
        }

        public static Directions[] ShuffledArray = new Directions[] {
            Directions.Forward, Directions.Right, Directions.Back, Directions.Left, Directions.Up, Directions.Down,
        };

        public static DirectionsEight[] ShuffledDiagonalArray = new DirectionsEight[] {
            DirectionsEight.Front, DirectionsEight.FrontRight, DirectionsEight.Right,DirectionsEight.RearRight,
            DirectionsEight.Rear, DirectionsEight.RearLeft, DirectionsEight.Left, DirectionsEight.FrontLeft
        };

        public static DirectionsEight[] ShuffledDiagonalPrimeArray = new DirectionsEight[] {
            DirectionsEight.Front, DirectionsEight.Right, 
            DirectionsEight.Rear, DirectionsEight.Left,
        };

        public static Directions[] ShuffledArray2D = new Directions[] {
            Directions.Forward, Directions.Right, Directions.Back, Directions.Left
        };

        private static Directions[][] _diagonalCheckDirs = new Directions[4][] {
            new[] {Directions.Left, Directions.Forward},
            new[] {Directions.Right, Directions.Forward},
            new[] {Directions.Right, Directions.Back},
            new[] {Directions.Left, Directions.Back},
        };

        public static Directions[] GetDiagonalCheckDirs(Directions dir) {
            return _diagonalCheckDirs[(int) dir];
        }

        private static Directions[] _leftRight = new Directions[] {
            Directions.Left, Directions.Right
        };
        private static Directions[] _forwardBack = new Directions[] {
            Directions.Forward, Directions.Back
        };

        public static Directions[] Adjacent(this Directions dir) {
            switch (dir) {
                default:
                case Directions.Forward:
                case Directions.Back:
                    return _leftRight;
                case Directions.Right:
                case Directions.Left:
                    return _forwardBack;
            }
        }

        public static Directions[] GetWallDirections(this CornerDirections cornerDirection) {
            switch (cornerDirection) {
                default:
                case CornerDirections.NorthWest:
                    return new[] {Directions.Left, Directions.Forward};
                case CornerDirections.NorthEast:
                    return new[] {Directions.Forward, Directions.Right};
                case CornerDirections.SouthEast:
                    return new[] {Directions.Right, Directions.Back};
                case CornerDirections.SouthWest:
                    return new[] {Directions.Back, Directions.Left};
            }
        }

        public static CornerDirections OppositeDir(this CornerDirections cornerDirection) {
            switch (cornerDirection) {
                default:
                case CornerDirections.NorthWest:
                    return CornerDirections.SouthEast;
                case CornerDirections.NorthEast:
                    return CornerDirections.SouthWest;
                case CornerDirections.SouthEast:
                    return CornerDirections.NorthWest;
                case CornerDirections.SouthWest:
                    return CornerDirections.NorthEast;
            }
        }

        private static DirectionsEight[][] _adjacent = new DirectionsEight[8][] {
            new [] {DirectionsEight.FrontLeft, DirectionsEight.FrontRight},
            new [] {DirectionsEight.Front, DirectionsEight.Right},
            new [] {DirectionsEight.FrontRight, DirectionsEight.RearRight},
            new [] {DirectionsEight.Right, DirectionsEight.Rear},
            new [] {DirectionsEight.RearRight, DirectionsEight.RearLeft},
            new [] {DirectionsEight.Rear, DirectionsEight.Left},
            new [] {DirectionsEight.RearLeft, DirectionsEight.FrontLeft},
            new [] {DirectionsEight.Left, DirectionsEight.Front},
        };

        private static DirectionsEight[][] _adjacentInclusive = new DirectionsEight[8][] {
            new[] {DirectionsEight.Front, DirectionsEight.FrontLeft, DirectionsEight.FrontRight},
            new[] {DirectionsEight.FrontRight, DirectionsEight.Front, DirectionsEight.Right},
            new[] {DirectionsEight.Right, DirectionsEight.FrontRight, DirectionsEight.RearRight},
            new[] {DirectionsEight.RearRight, DirectionsEight.Right, DirectionsEight.Rear},
            new[] {DirectionsEight.Rear, DirectionsEight.RearRight, DirectionsEight.RearLeft},
            new[] {DirectionsEight.RearLeft, DirectionsEight.Rear, DirectionsEight.Left},
            new[] {DirectionsEight.Left, DirectionsEight.RearLeft, DirectionsEight.FrontLeft},
            new[] {DirectionsEight.FrontLeft, DirectionsEight.Left, DirectionsEight.Front},
        };

        public static DirectionsEight[] Adjacent(this DirectionsEight dir) {
            return _adjacent[(int) dir];
        }

        public static DirectionsEight[] AdjacentInclusive(this DirectionsEight dir) {
            return _adjacent[(int) dir];
        }

        public static bool IsAdjacent(this DirectionsEight dir, DirectionsEight compare) {
            switch (dir) {
                default:
                case DirectionsEight.Front:
                    return compare == DirectionsEight.FrontLeft || compare == DirectionsEight.FrontRight;
                case DirectionsEight.FrontRight:
                    return compare == DirectionsEight.Front || compare == DirectionsEight.Right;
                case DirectionsEight.Right:
                    return compare == DirectionsEight.FrontRight || compare == DirectionsEight.RearRight;
                case DirectionsEight.RearRight:
                    return compare == DirectionsEight.Right || compare == DirectionsEight.Rear;
                case DirectionsEight.Rear:
                    return compare == DirectionsEight.RearRight || compare == DirectionsEight.RearLeft;
                case DirectionsEight.RearLeft:
                    return compare == DirectionsEight.Rear || compare == DirectionsEight.Left;
                case DirectionsEight.Left:
                    return compare == DirectionsEight.FrontLeft || compare == DirectionsEight.RearLeft;
                case DirectionsEight.FrontLeft:
                    return compare == DirectionsEight.Front || compare == DirectionsEight.Left;
            }
        }

        public static bool IsCardinal(this DirectionsEight dir) {
            switch (dir) {
                case DirectionsEight.Front:
                case DirectionsEight.Right:
                case DirectionsEight.Left:
                case DirectionsEight.Rear:
                    return true;
            }
            return false;
        }

        public static bool Is2D(this DirectionsEight dir) {
            switch (dir) {
                case DirectionsEight.Top:
                case DirectionsEight.Bottom:
                    return false;
            }
            return true;
        }

        public static Directions[] GetShuffledDirectionsArray(Directions lastDir) {
            ShuffledArray.Shuffle();
            for (int i = 0; i < ShuffledArray.Length - 1; i++) {
                if (ShuffledArray[i] == lastDir) {
                    ShuffledArray[i] = ShuffledArray[ShuffledArray.Length - 1];
                    ShuffledArray[ShuffledArray.Length - 1] = lastDir;
                }
            }
            return ShuffledArray;
        }

        public static bool Is2D(this Directions dir) {
            if (dir == Directions.Up || dir == Directions.Down) {
                return false;
            }
            return true;
        }

        public static Directions ChangeFwdDirection(this Directions dir, int newDir) {
            if (newDir == 0) {
                return dir;
            }
            switch (dir) {
                case Directions.Forward:
                    switch (newDir) {
                        case 1:
                            return Directions.Right;
                        case 2:
                            return Directions.Left;
                        case 3:
                            return Directions.Up;
                        default:
                            return Directions.Down;
                    }
                case Directions.Right:
                    switch (newDir) {
                        case 1:
                            return Directions.Back;
                        case 2:
                            return Directions.Forward;
                        case 3:
                            return Directions.Up;
                        default:
                            return Directions.Down;
                    }
                case Directions.Back:
                    switch (newDir) {
                        case 1:
                            return Directions.Left;
                        case 2:
                            return Directions.Right;
                        case 3:
                            return Directions.Up;
                        default:
                            return Directions.Down;
                    }
                case Directions.Left:
                    switch (newDir) {
                        case 1:
                            return Directions.Forward;
                        case 2:
                            return Directions.Back;
                        case 3:
                            return Directions.Up;
                        default:
                            return Directions.Down;
                    }
                case Directions.Up:
                    switch (newDir) {
                        case 1:
                            return Directions.Right;
                        case 2:
                            return Directions.Left;
                        case 3:
                            return Directions.Back;
                        default:
                            return Directions.Forward;
                    }
                case Directions.Down:
                    switch (newDir) {
                        case 1:
                            return Directions.Left;
                        case 2:
                            return Directions.Right;
                        case 3:
                            return Directions.Back;
                        default:
                            return Directions.Forward;
                    }
            }
            return dir;
        }

        public static Directions TransformDir(this Directions dir, Directions newDir) {
            switch (dir) {
                case Directions.Forward:
                    switch (newDir) {
                        case Directions.Right:
                            return Directions.Right;
                        case Directions.Left:
                            return Directions.Left;
                        case Directions.Back:
                            return Directions.Back;
                    }
                    return Directions.Forward;
                case Directions.Right:
                    switch (newDir) {
                        case Directions.Right:
                            return Directions.Back;
                        case Directions.Left:
                            return Directions.Forward;
                        case Directions.Back:
                            return Directions.Left;
                    }
                    return Directions.Right;
                case Directions.Back:
                    switch (newDir) {
                        case Directions.Right:
                            return Directions.Left;
                        case Directions.Left:
                            return Directions.Right;
                        case Directions.Back:
                            return Directions.Forward;
                    }
                    return Directions.Back;
                case Directions.Left:
                    switch (newDir) {
                        case Directions.Right:
                            return Directions.Forward;
                        case Directions.Left:
                            return Directions.Back;
                        case Directions.Back:
                            return Directions.Right;
                    }
                    return Directions.Left;
            }
            return dir;
        }

        public static Vector3 ToV3(this Directions dir) {
            switch (dir) {
                case Directions.Forward:
                    return Vector3.forward;
                case Directions.Right:
                    return Vector3.right;
                case Directions.Back:
                    return Vector3.back;
                case Directions.Left:
                    return Vector3.left;
                case Directions.Up:
                    return Vector3.up;
                case Directions.Down:
                    return Vector3.down;
            }
            return Vector3.forward;
        }

        public static Vector3 ToV3XY(this Directions dir) {
            switch (dir) {
                case Directions.Forward:
                    return Vector2.up;
                case Directions.Right:
                    return Vector2.right;
                case Directions.Back:
                    return Vector2.down;
                case Directions.Left:
                    return Vector2.left;
                case Directions.Up:
                    return Vector2.up;
                case Directions.Down:
                    return Vector2.down;
            }
            return Vector3.forward;
        }

        public static float ToSimpleAngle(this Directions dir) {
            switch (dir) {
                case Directions.Forward:
                    return 0;
                case Directions.Right:
                    return 90;
                case Directions.Back:
                    return 180;
                case Directions.Left:
                    return -90;
            }
            return 0;
        }

        public static Point3 ToPoint3(this Directions dir) {
            switch (dir) {
                case Directions.Forward:
                    return Point3.forward;
                case Directions.Right:
                    return Point3.right;
                case Directions.Back:
                    return Point3.back;
                case Directions.Left:
                    return Point3.left;
                case Directions.Up:
                    return Point3.up;
                case Directions.Down:
                    return Point3.down;
            }
            return Point3.forward;
        }

        public static DirectionsEight EulerToDirectionEight(this Vector3 eulerRot, bool helpCardinal = false) {
            var angle = eulerRot.y;
            var halfRange = 45 / 2;
            for (int i = 0; i < _eulerEight.Length; i++) {
                var dirRange = halfRange;
                if (helpCardinal && ((DirectionsEight) i).IsCardinal()) {
                    dirRange += 5;
                }
                var center = _eulerEight[i].y;
                var bottom = center - dirRange;
                var top = center + dirRange;
                if (angle >= bottom && angle <= top) {
                    return (DirectionsEight) i;
                }
            }
            for (int i = 4; i < _eulerEightNeg.Length; i++) {
                var dirRange = halfRange;
                if (helpCardinal && ((DirectionsEight) i).IsCardinal()) {
                    dirRange += 5;
                }
                var center = _eulerEightNeg[i].y;
                var bottom = center - dirRange;
                var top = center + dirRange;
                if (angle >= bottom && angle <= top ||
                    (angle >= top && angle <= bottom)) {
                    return ((DirectionsEight) i);
                }
            }
            return DirectionsEight.Front;
        }

        public static int RotateRight(this Directions d, int rotation) {
            return MathEx.WrapAround(((int) d + rotation), Length2D);
        }

        public static int RotateLeft(this Directions d, int rotation) {
            return MathEx.WrapAround(((int) d - rotation), Length2D);
        }

        public static int Length = 6;
        public static int Length2D = 4;
        public static int DiagonalLength = 8;
        public static int DiagonalLength3D = 10;

        public static Vector3 ToEuler(this Directions d) {
            //return new Vector3(0, (int)d * 90, 0);
            return EulerRotations[(int) d];
        }

        public static Quaternion ToEulerRot(this Directions d) {
            //return new Vector3(0, (int)d * 90, 0);
            return Quaternion.Euler(EulerRotations[(int) d]);
        }

        //-90 or 
        public static Vector3[] EulerRotations = new Vector3[] {
            new Vector3(0, 0, 0), new Vector3(0, 90, 0), new Vector3(0, 180, 0), new Vector3(0, 270, 0),
            new Vector3(90, 0, 0), new Vector3(-90, 0, 0)
        };

        private static Color[] _dirEightColors = new Color[10] {
            Color.green, new Color32(0, 255, 255, 255), Color.blue, new Color32(128,0,255,255), 
            new Color32(191,0,255,255), new Color32(255,0,255,255), new Color32(255,0,0,255), new Color32(255,191,0,255),
            Color.yellow, Color.white
        };

        public static Color ToColor(this DirectionsEight dir) {
            return _dirEightColors[(int) dir];
        }

        private static Vector3[] _eulerEight = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, 45, 0), new Vector3(0, 90, 0), new Vector3(0, 135, 0),
            new Vector3(0, 180, 0),
            new Vector3(0, 225, 0), new Vector3(0, 270, 0), new Vector3(0, 315, 0)
        };

        private static Vector3[] _eulerEightNeg = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0),
            new Vector3(0, -180, 0),
            new Vector3(0, -135, 0), new Vector3(0, -90, 0), new Vector3(0, -45, 0)
        };

        public static Quaternion ToEulerRot(this DirectionsEight d) {
            return Quaternion.Euler(_eulerEight[(int) d]);
        }

        private static string[] _toCardinal = new string[] {
            "North", "East", "South", "West", "Up", "Down"
        };

        public static List<string> Door2DLabelList = new List<string>() {
            "Forward", "Right", "Back", "Left"
        };

        public static string ToCardinalString(this Directions dir) {
            return _toCardinal[(int) dir];
        }

        public static Directions GetTravelDirTo(this Point3 origin, Point3 newpos) {
            if (origin.y < newpos.y) {
                return Directions.Up;
            }
            if (origin.y > newpos.y) {
                return Directions.Down;
            }
            if (origin.z < newpos.z) {
                return Directions.Forward;
            }
            if (origin.x < newpos.x) {
                return Directions.Right;
            }
            if (origin.z > newpos.z) {
                return Directions.Back;
            }
            return Directions.Left;
        }

        public static Directions ToDirection(this Point3 dir) {
            if (dir.z > 0 && dir.x == 0) {
                return Directions.Forward;
            }

            if (dir.z < 0 && dir.x == 0) {
                return Directions.Back;
            }

            if (dir.x > 0 && dir.z == 0) {
                return Directions.Right;
            }

            return Directions.Left;
        }

        private static Point3[] _diagonalPosition = {
            new Point3(0, 0, 1),
            new Point3(1, 0, 1), new Point3(1, 0, 0), new Point3(1, 0, -1),
            new Point3(0, 0, -1),
            new Point3(-1, 0, -1), new Point3(-1, 0, 0), new Point3(-1, 0, 1),
            new Point3(0, 1, 0), new Point3(0, -1, 0)
        };

        public static Point3 ToPoint3(this DirectionsEight dir) {
            return _diagonalPosition[(int) dir];
        }

        private static Vector3[] _diagonalPositionV3 = {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(-1, 0, 1),
            new Vector3(0, 1, 0), new Vector3(0, -1, 0)
        };

        public static Vector3 ToV3(this DirectionsEight dir) {
            return _diagonalPositionV3[(int) dir];
        }
    }

    
    public static class ActionExtensions {
        public static void SafeInvoke(this System.Action callback) {
            if (callback != null) {
                callback();
            }
        }

        public static void SafeInvoke<T>(this System.Action<T> callback, T param) {
            if (callback != null) {
                callback(param);
            }
        }
    }

    public static class SortHelper {
        /// <summary>
        /// (i, i1) => i > i1 is ascending
        /// (i, i1) => i < i1 is descending
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="isHigher"></param>
        public static void BubbleSort<T>(this IList<T> arr, Func<T,T, bool> isHigher) {
            T temp;
            for (int write = 0; write < arr.Count; write++) {
                for (int sort = 0; sort < arr.Count - 1; sort++) {
                    if (isHigher(arr[sort], arr[sort + 1])) {
                        temp = arr[sort + 1];
                        arr[sort + 1] = arr[sort];
                        arr[sort] = temp;
                    }
                }
            }
            
        }
    }

    public static class RaycastHitExtensions {

        public class RaycastSorter : Comparer<RaycastHit> {

            private bool _descending;

            public RaycastSorter(bool descending) {
                _descending = descending;
            }

            public override int Compare(RaycastHit x, RaycastHit y) {
                if (x.collider == null && y.collider == null) {
                    return 0;
                }
                if (x.collider == null) {
                    return 1;
                }
                if (y.collider == null) {
                    return -1;
                }
                if (_descending) {
                    return -1 * x.distance.CompareTo(y.distance);
                }
                return x.distance.CompareTo(y.distance);
            }
        }

        private static RaycastSorter _descending = new RaycastSorter(true);
        private static RaycastSorter _ascending = new RaycastSorter(false);

        public static void SortByDistanceDesc(this RaycastHit[] rayHits, int hitLimit) {
            //The lazy bubble sort is actually faster
            //System.Array.Sort(rayHits, _descending);
            for (int write = 0; write < hitLimit; write++) {
                for (int sort = 0; sort < hitLimit - 1; sort++) {
                    if (rayHits[sort].distance < rayHits[sort + 1].distance) {
                        var greater = rayHits[sort + 1];
                        rayHits[sort + 1] = rayHits[sort];
                        rayHits[sort] = greater;
                    }
                }
            }
        }

        public static void SortByDistanceAsc(this RaycastHit[] rayHits, int hitLimit) {
            //System.Array.Sort(rayHits, _ascending);
            for (int write = 0; write < hitLimit; write++) {
                for (int sort = 0; sort < hitLimit - 1; sort++) {
                    if (rayHits[sort].distance > rayHits[sort + 1].distance) {
                        var lesser = rayHits[sort + 1];
                        rayHits[sort + 1] = rayHits[sort];
                        rayHits[sort] = lesser;
                    }
                }
            }
        }

        public static void SortByHitPointAsc(this RaycastHit[] rayHits, int hitLimit, Vector3 origin) {
            //System.Array.Sort(rayHits, _ascending);
            for (int write = 0; write < hitLimit; write++) {
                for (int sort = 0; sort < hitLimit - 1; sort++) {
                    var dist1 = Vector3.Distance(rayHits[sort].point, origin);
                    var dist2 = Vector3.Distance(rayHits[sort+1].point, origin);
                    if (dist1 > dist2) {
                        var lesser = rayHits[sort + 1];
                        rayHits[sort + 1] = rayHits[sort];
                        rayHits[sort] = lesser;
                    }
                }
            }
        }

        public static void SortByDistanceAsc(this List<Entity> list, Point3 center) {
            for (int write = 0; write < list.Count; write++) {
                for (int sort = 0; sort < list.Count - 1; sort++) {
                    if (list[sort].Get<GridPosition>().Position.SqrDistance(center) > list[sort + 1].Get<GridPosition>().Position.SqrDistance(center)) {
                        var lesser = list[sort + 1];
                        list[sort + 1] = list[sort];
                        list[sort] = lesser;
                    }
                }
            }
        }

        public static void SortByDistanceAsc(this List<Vector3> list, Vector3 center) {
            for (int write = 0; write < list.Count; write++) {
                for (int sort = 0; sort < list.Count - 1; sort++) {
                    if (list[sort].SqrDistance(center) > list[sort + 1].SqrDistance(center)) {
                        var lesser = list[sort + 1];
                        list[sort + 1] = list[sort];
                        list[sort] = lesser;
                    }
                }
            }
        }

        public static void SortByDistanceAsc(this List<Point3> list, Point3 center) {
            for (int write = 0; write < list.Count; write++) {
                for (int sort = 0; sort < list.Count - 1; sort++) {
                    if (list[sort].SqrDistance(center) > list[sort + 1].SqrDistance(center)) {
                        var lesser = list[sort + 1];
                        list[sort + 1] = list[sort];
                        list[sort] = lesser;
                    }
                }
            }
        }

        public static void SortByDistanceAsc(this List<WatchTarget> list, Point3 center) {
            for (int write = 0; write < list.Count; write++) {
                for (int sort = 0; sort < list.Count - 1; sort++) {
                    if (list[sort].LastSensedPos.SqrDistance(center) > list[sort + 1].LastSensedPos.SqrDistance(center)) {
                        var lesser = list[sort + 1];
                        list[sort + 1] = list[sort];
                        list[sort] = lesser;
                    }
                }
            }
        }
    }

    public static class Point3Extensions {
        public static int DistanceSquared(this Point3 a, Point3 b) {
            int dx = b.x - a.x;
            int dy = b.y - a.y;
            int dz = b.z - a.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static int XZDistanceSquared(this Point3 a, Point3 b) {
            int dx = b.x - a.x;
            int dz = b.z - a.z;
            return dx * dx + dz * dz;
        }


        public static Point3 Reverse(this Point3 pos) {
            Point3 newPos = Point3.zero;
            for (int i = 0; i < 3; i++) {
                if (newPos[i] == 0) {
                    continue;
                }
                newPos[i] = pos[i] * -1;
            }
            return newPos;
        }

        public static Vector3 GenericGridToWorld(this Point3 position, float gridSize) {
            return new Vector3(position.x * gridSize, position.y * gridSize, position.z * gridSize);
        }

        public static bool OnAxis2D(this Point3 p, Point3 other) {
            if (p.z == other.z || p.x == other.x) {
                return true;
            }
            return false;
        }

        public static int TileDistance2D(this Point3 p, Point3 other) {
            return Mathf.Abs(p.x - other.x) + Mathf.Abs(p.z - other.z);
        }
    }

    public static class CustomStringExtension {
        public static bool CompareCaseInsensitive(this string data, string other) {
            return string.Equals(data, other, StringComparison.OrdinalIgnoreCase);
        }
        
        public static List<string> SplitMultiEntry(this string targetLine) {
            return StringUtilities.SplitString(targetLine, StringConst.MultiEntryBreak);
        }

        public static List<string>[] SplitLinesIntoData(this string text, char splitChar) {
            return StringUtilities.SplitStringWithLines(text, splitChar);
        }

        public static List<string>[] SplitLinesIntoData(this string text) {
            return StringUtilities.SplitStringWithLines(text, StringConst.MultiEntryBreak);
        }

        public static string[] SplitIntoLines(this string text) {
            return text.Split('\n');
        }

        public static string[] SplitIntoWords(this string text) {
            return text.Split(' ');
        }

        public static string[] SplitFromEntryBreak(this string text) {
            if (text == null) {
                return null;
            }
            return text.Split(StringConst.MultiEntryBreak);
        }

        public static string EncodeWithEntryBreak(this IList<string> text) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Count; i++) {
                sb.AppendEntryBreak(text[i]);
            }
            return sb.ToString();
        }
    }

    public class Point3Comparer : IEqualityComparer<Point3> {
        public int GetHashCode(Point3 p) {
            unchecked {
                return (p.x.GetHashCode() * 397) ^ p.z.GetHashCode() ^ p.y.GetHashCode();
            }
            //return p.x ^ p.y << 2 ^ p.z >> 2;
            //return p.x.GetHashCode() ^ p.y.GetHashCode() << 2 ^ p.z.GetHashCode() >> 2;
            //return p.x + p.y + p.z;
        }

        public bool Equals(Point3 p1, Point3 p2) {
            return p1.x == p2.x &&
                   p1.y == p2.y &&
                   p1.z == p2.z;
        }
    }

    public static class QuanternionExtensions {
        public static Vector3 RotatePointAroundPivot(this Quaternion angles, Vector3 point, Vector3 axis) {
            var dir = point - axis;
            dir = angles * dir;
            point = dir + axis;
            return point;
        }

        public static Quaternion Rotate2D(Vector3 start, Vector3 end) {
            Vector3 diff = start - end;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, rot_z - 90f);
        }

    }


    public static class CanvasGroupExtension {
        public static void SetActive(this CanvasGroup canvasGroup, bool active) {
            canvasGroup.alpha = active ? 1 : 0;
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
    }

    public static class GameObjectExtensions {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
            var newOrExistingComponent = gameObject.GetComponent<T>();
            if (!(newOrExistingComponent != null)) {
                newOrExistingComponent = gameObject.AddComponent<T>();
            }
            //var newOrExistingComponent = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
            return newOrExistingComponent;
        }

        public static GameObject GetClosest(this IList<GameObject> objList, Vector3 position) {
            GameObject go = null;
            float lowDistance = float.MaxValue;
            for (int i = 0; i < objList.Count; i++) {
                var dist = objList[i].transform.position.SqrDistance(position);
                if (dist < lowDistance) {
                    lowDistance = dist;
                    go = objList[i];
                }
            }
            return go;
        }

        public static void SetActive(this IList<GameObject> objList, bool active) {
            for (int i = 0; i < objList.Count; i++) {
                objList[i].SetActive(active);
            }
        }

        public static void DebugDuplicate(this GameObject go) {
            var components = go.GetComponents<Component>();
            if (components == null) {
                return;
            }
            List<Type> uniqueTypes = new List<Type>();
            List<Type> dupes = new List<Type>();
            for (int j = 0; j < components.Length; j++) {
                var type = components[j].GetType();
                if (!uniqueTypes.Contains(type)) {
                    uniqueTypes.Add(type);
                }
                else if (!dupes.Contains(type)) {
                    dupes.Add(type);
                }
            }
            if (dupes.Count == 0) {
                return;
            }
            Debug.LogFormat("Object {0}: Components {1} Duplicates {2}", go.name, components.Length, dupes.Count);
            //for (int j = 0; j < dupes.Count; j++) {
            //    Debug.Log(dupes[j].Name);
            //}
        }
    }

    public static class FloatExtensions {
        //public static float Remap (this float value, float from1, float to1, float from2, float to2) {
        //    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        //}

        public static float Remap(this float value, float fromMinValue, float fromMaxValue, float toMinValue, float toMaxValue) {
            try {
                return (value - fromMinValue) * (toMaxValue - toMinValue) / (fromMaxValue - fromMinValue) + toMinValue;
            }
            catch {
                return float.NaN;
            }
        }

        public static float ToPercent(this float value) {
            return value * 100;
        }

        public static float ToPercentNormalized(this float value) {
            if (value < 1) {
                return value;
            }
            return value * .001f;
        }
    }

    public static class MathEx {

        public static int Min(int a, int b) {
            return a < b ? a : b;
        }

        public static int Max(int a, int b) {
            return a > b ? a : b;
        }

        public static int Max(int a, int b, int c) {
            if (c > a && c > b) {
                return c;
            }
            return a > b ? a : b;
        }

        public static float Min(float a, float b) {
            return a < b ? a : b;
        }

        public static float Max(float a, float b) {
            return a > b ? a : b;
        }

        /// <summary>
        /// Max is exclusive
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float WrapAround(float val, float min, float max) {
            val = val - (float) Math.Round((val - min) / (max - min)) * (max - min);
            if (val < 0)
                val = val + max - min;
            return val;
        }

        /// <summary>
        /// Max is inclusive
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int WrapClamp(int val, int min, int max) {
            if (val < min) {
                return max;
            }
            return val > max ? min : val;
        }

        /// <summary>
        /// Max is exclusive, will warp to max-1 below 0
        /// </summary>
        /// <param name="input"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int WrapAround(int input, int max) {
            return (input % max + max) % max;
        }


        public static double RoundDown(double value) {
            value = System.Math.Floor(value);
            return value * Math.Pow(10, 0);
        }

        //This function returns a point which is a projection from a point to a line segment.
        //If the projected point lies outside of the line segment, the projected point will 
        //be clamped to the appropriate line edge.
        //If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
        public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {

            Vector3 vector = linePoint2 - linePoint1;

            Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

            int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

            //The projected point is on the line segment
            if (side == 0) {

                return projectedPoint;
            }

            if (side == 1) {

                return linePoint1;
            }

            if (side == 2) {

                return linePoint2;
            }

            //output is invalid
            return Vector3.zero;
        }

        //This function returns a point which is a projection from a point to a line.
        //The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
        public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point) {

            //get vector from point on line to point in space
            Vector3 linePointToPoint = point - linePoint;

            float t = Vector3.Dot(linePointToPoint, lineVec);

            return linePoint + lineVec * t;
        }

        //This function finds out on which side of a line segment the point is located.
        //The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
        //the line segment, project it on the line using ProjectPointOnLine() first.
        //Returns 0 if point is on the line segment.
        //Returns 1 if point is outside of the line segment and located on the side of linePoint1.
        //Returns 2 if point is outside of the line segment and located on the side of linePoint2.
        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {

            Vector3 lineVec = linePoint2 - linePoint1;
            Vector3 pointVec = point - linePoint1;

            float dot = Vector3.Dot(pointVec, lineVec);

            //point is on side of linePoint2, compared to linePoint1
            if (dot > 0) {

                //point is on the line segment
                if (pointVec.magnitude <= lineVec.magnitude) {

                    return 0;
                }

                //point is not on the line segment and it is on the side of linePoint2
                else {

                    return 2;
                }
            }

            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
            else {

                return 1;
            }
        }

        public static Vector3 ClosestPointOnLine(Vector3 start, Vector3 end, Vector3 pos) {
            var localPos = pos - start;
            var lineDir = (end - start).normalized;

            var lineLength = Vector3.Distance(start, end);
            var projAngle = Vector3.Dot(lineDir, localPos);
            //projAngle = Mathf.Clamp(projAngle, 0, lineLength);
            if (projAngle <= 0) {
                return start;
            }

            if (projAngle >= lineLength) {
                return end;
            }

            var localPntOnLine = lineDir * projAngle;
            var worldSpaceOnLine = start + localPntOnLine;
            return worldSpaceOnLine;
        }
    }

    public static class BaseItemExtensions {

        //public static T New<T>(this T item) where T : Entity {
        //    var baseItem = ItemPool.SpawnPrefab<T>(item.gameObject);
        //    //baseItem.Setup(UnityEngine.Random.Range(1,item.MaxStack+1), 
        //    //    UnityEngine.Random.Range(1, PlayerGameStats.LevelReached));
        //    return baseItem;
        //}
    }

    public class ClassLog {
        public static void Log(string msg, UnityEngine.Object originClass = null) {
            if (originClass != null) {
                Debug.Log(originClass.ToString() + " :: " + msg);
            }
            else {
                Debug.Log(msg);
            }
        }
    }

    public static class ScrollRectExtensions {
        public static void ScrollToTop(this ScrollRect scrollRect) {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        public static void ScrollToBottom(this ScrollRect scrollRect) {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }

    public static class ArrayExtensions {
        public static bool Contains(this IList array1, IList array2) {
            for (int a = 0; a < array1.Count; a++) {
                for (int b = 0; b < array2.Count; b++) {
                    if (array1[a].Equals(array2[b])) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Fill<T>(this T[] array, T[] values) {
            if (array.Length < values.Length) {
                System.Array.Resize(ref array, values.Length);
            }
            for (int i = 0; i < values.Length; i++) {
                array[i] = values[i];
            }
        }

        public static bool Contains<T>(this IList<T> array1, T item) {
            for (int a = 0; a < array1.Count; a++) {
                if (array1[a] == null) {
                    continue;
                }
                if (array1[a].Equals(item)) {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains<T>(this List<T> array1, T item) {
            for (int a = 0; a < array1.Count; a++) {
                if (array1[a] == null) {
                    continue;
                }
                if (array1[a].Equals(item)) {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains<T>(this T[] array1, T item) {
            for (int a = 0; a < array1.Length; a++) {
                if (array1[a] == null) {
                    continue;
                }
                if (array1[a].Equals(item)) {
                    return true;
                }
            }
            return false;
        }

        public static bool HasIndex<T>(this IList<T> array, int index) {
            return index >= 0 && index < array.Count;
        }

        public static T Clamp<T>(this IList<T> array, int index) {
            if (index <= 0) {
                return array[0];
            }
            if (index >= array.Count) {
                return array[array.Count - 1];
            }
            return array[index];
        }

        public static T SafeAccess<T>(this IList<T> array, int index) {
            return array[Mathf.Clamp(index, 0, array.Count - 1)];
        }

        public static int FindIndex(this IList<string> array, string target) {
            for (int i = 0; i < array.Count; i++) {
                if (array[i] == null) {
                    continue;
                }
                if (target.CompareCaseInsensitive(array[i])) {
                    return i;
                }
            }
            return -1;
        }

        public static int FindIndex<T>(this IList<T> array1, T item) {
            for (int a = 0; a < array1.Count; a++) {
                if (array1[a] == null) {
                    continue;
                }
                if (array1[a].Equals(item)) {
                    return a;
                }
            }
            return -1;
        }

        public static int FindIndex<T>(this List<T> array1, T item) {
            for (int a = 0; a < array1.Count; a++) {
                if (array1[a] == null) {
                    continue;
                }
                if (array1[a].Equals(item)) {
                    return a;
                }
            }
            return -1;
        }

        public static int FindIndex<T>(this T[] array1, T item) {
            for (int a = 0; a < array1.Length; a++) {
                if (array1[a] == null) {
                    continue;
                }
                if (array1[a].Equals(item)) {
                    return a;
                }
            }
            return -1;
        }

    }

    public static class TextAssetExtensionMethods {
        public static List<string> TextAssetToLineList(this TextAsset ta) {
            return new List<string>(ta.text.Split('\n'));
        }
    }

    public static class LayerMaskExtensions {
        public static bool ContainsLayer(this LayerMask layerMask, int layer) {
            return layerMask == (layerMask | (1 << layer));
        }
    }

    public static class AssetTypeExtensions {
        private static Dictionary<Type, string> _typeToExtensions = new Dictionary<Type, string>() {
            {typeof(GameObject), ".prefab"}, {typeof(ScriptableObject), ".asset" },
        };

        public static string GetExtensionFromType<T>() {
            if (_typeToExtensions.TryGetValue(typeof(T), out var extension)) {
                return extension;
            }
            return ".asset";
        }
    }

    public static class ParseUtilities {

        private static List<string> _namespaces = new List<string>();

        public static System.Type ParseType(string typeName) {
            var type = System.Type.GetType(typeName, false, false);
            if (type == null) {
                type = System.Type.GetType("PixelComrades." + typeName, false, false);
            }
            if (type == null) {
                if (_namespaces.Count == 0) {
                    CollectNamespaces();
                }
                for (int i = 0; i < _namespaces.Count; i++) {
                    type = System.Type.GetType(_namespaces[i] + "." + typeName, false, false);
                    if (type != null) {
                        break;
                    }
                }
            }
            return type;
        }

        private static void CollectNamespaces() {
            _namespaces.Clear();
            int i = 0;
            while (i < 99) {
                var nameSpaceString = GameOptions.Get("LoadedNamespace" + i, "");
                if (string.IsNullOrEmpty(nameSpaceString)) {
                    break;
                }
                _namespaces.Add(nameSpaceString);
                i++;
            }
        } 

        public static int TryParseInt(this IList<string> lines, ref int parseIndex) {
            return lines.TryParse(ref parseIndex, 0);
        }

        public static float TryParseFloat(this IList<string> lines, ref int parseIndex) {
            return lines.TryParse(ref parseIndex, 0f);
        }

        public static int TryParse(this IList<string> lines, ref int parseIndex, int defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseInt out of range at {0}", parseIndex);
                parseIndex++;
                return defValue;
            }
            int value;
            if (!int.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            parseIndex++;
            return value;
        }

        public static float TryParse(this IList<string> lines, ref int parseIndex, float defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseFloat out of range at {0}", parseIndex);
                parseIndex++;
                return defValue;
            }
            float value;
            if (!float.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            parseIndex++;
            return value;
        }

        public static bool TryParse(this IList<string> lines, ref int parseIndex, bool defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseBool out of range at {0}", parseIndex);
                parseIndex++;
                return defValue;
            }
            bool value;
            if (!bool.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            parseIndex++;
            return value;
        }

        public static LeveledFloat TryParse(this IList<string> lines, ref int parseIndex, LeveledFloat defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseBool out of range at {0}", parseIndex);
                parseIndex++;
                return defValue;
            }
            LeveledFloat value = LeveledFloat.Parse(lines[parseIndex]);
            parseIndex++;
            return value == null ? defValue : value;
        }

        public static string ParseString(this IList<string> lines, ref int parseIndex) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("ParseString out of range at {0}", parseIndex);
                parseIndex++;
                return null;
            }
            var value = lines[parseIndex];
            parseIndex++;
            return value;
        }

        public static Color TryParse(this IList<string> lines, ref int parseIndex, Color defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseColor out of range at {0}", parseIndex);
                parseIndex++;
                return defValue;
            }
            Color color;
            if (ColorUtility.TryParseHtmlString(lines[parseIndex], out color)) {
                parseIndex++;
                return color;
            }
            parseIndex++;
            return defValue;
        }

        public static string TryParseString(this IList<string> lines, ref int parseIndex, string defaultVal) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseString out of range at {0}", parseIndex);
                parseIndex++;
                return defaultVal;
            }
            var value = lines[parseIndex];
            parseIndex++;
            return value;
        }

        public static T ParseEnum<T>(this IList<string> lines, ref int parseIndex) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("ParseEnum out of range at {0}", parseIndex);
                parseIndex++;
                return default(T);
            }
            var value = EnumHelper.ForceParse<T>(lines[parseIndex]);
            parseIndex++;
            return value;
        }

        public static T TryParse<T>(this IList<string> lines, ref int parseIndex, T defaultValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParse out of range at {0}", parseIndex);
                parseIndex++;
                return defaultValue;
            }
            T value;
            if (!EnumHelper.TryParse(lines[parseIndex], out value)) {
                value = defaultValue;
            }
            parseIndex++;
            return value;
        }

        public static LeveledFloatRange TryParseLeveledFloatWithMulti(this IList<string> lines, ref int parseIndex) {
            if (!lines.HasIndex(parseIndex+1)) {
                Debug.LogFormat("TryParse out of range at {0}", parseIndex);
                parseIndex++;
                return null;
            }
            LeveledFloat lvledFloat = LeveledFloat.Parse(lines[parseIndex]);
            if (lvledFloat == null) {
                parseIndex++;
                return null;
            }
            parseIndex++;
            float multi = lines.TryParse(ref parseIndex, 1f);
            return new LeveledFloatRange(lvledFloat.BaseValue, lvledFloat.PerLevel, multi);
        }

        public static string[] SplitStringMultiEntries(this IList<string> lines, ref int parseIndex) {
            return lines.SplitStringMultiEntries(StringConst.MultiEntryBreak, ref parseIndex);
        }

        public static string[] SplitStringMultiEntries(this IList<string> lines, char splitChar, ref int parseIndex) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("SplitStringMultiEnties out of range at {0}", parseIndex);
                parseIndex++;
                return null;
            }
            string[] value = lines[parseIndex].Split(splitChar);
            parseIndex++;
            return value;
        }

        public static string[] SplitString(this IList<string> lines, ref int parseIndex, char splitChar = ' ') {
            var value = lines[parseIndex].Split(splitChar);
            parseIndex++;
            return value;
        }

        public static int TryParse(ref IList<string> lines, ref int parseIndex, int defValue) {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return defValue;
            }
            int value;
            if (!int.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            parseIndex++;
            return value;
        }

        public static float TryParse(ref IList<string> lines, ref int parseIndex, float defValue) {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return defValue;
            }
            float value;
            if (!float.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            parseIndex++;
            return value;
        }

        public static bool TryParseBool(ref IList<string> lines, ref int parseIndex, bool defValue) {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return defValue;
            }
            bool value;
            if (!bool.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            parseIndex++;
            return value;
        }

        public static string TryParseString(ref IList<string> lines, ref int parseIndex, string defaultVal) {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return defaultVal;
            }
            var value = lines[parseIndex];
            parseIndex++;
            return value;
        }

        public static T ParseEnum<T>(ref IList<string> lines, ref int parseIndex) {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return default(T);
            }
            var value = EnumHelper.ForceParse<T>(lines[parseIndex]);
            parseIndex++;
            return value;
        }

        public static T TryParse<T>(ref IList<string> lines, ref int parseIndex, T defaultValue) {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return defaultValue;
            }
            T value;
            if (!EnumHelper.TryParse(lines[parseIndex], out value)) {
                value = defaultValue;
            }
            parseIndex++;
            return value;
        }

        public static string[] SplitStringMultiEntries(ref IList<string> lines, ref int parseIndex) {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return null;
            }
            var value = lines[parseIndex].Split('-');
            parseIndex++;
            return value;
        }

        public static string[] SplitString(ref IList<string> lines, ref int parseIndex, char splitChar = ' ') {
            if (lines.Count <= parseIndex) {
                parseIndex++;
                return null;
            }
            var value = lines[parseIndex].Split(splitChar);
            parseIndex++;
            return value;
        }

        public static IntRange TryParseRange(this IList<string> lines, ref int parseIndex, IntRange defaultVal) {
            if (!lines.HasIndex(parseIndex)) {
                parseIndex++;
                return defaultVal;
            }
            var numbers = lines[parseIndex].Split('-');
            parseIndex++;
            if (numbers.Length < 2) {
                return defaultVal;
            }
            int num1, num2;
            if (!int.TryParse(numbers[0], out num1)) {
                return defaultVal;
            }
            if (!int.TryParse(numbers[1], out num2)) {
                return defaultVal;
            }
            return new IntRange(num1, num2);
        }

        public static int TryParse(string data, int defaultValue) {
            return int.TryParse(data, out var value) ? value : defaultValue;
        }

        public static float TryParse(string data, float defaultValue) {
            return float.TryParse(data, out var value) ? value : defaultValue;
        }

        public static Vector3 TryParse(string data, Vector3 defaultValue) {
            data = data.Trim();
            var v3 = data.Split(',');
            if (v3.Length < 3) {
                return defaultValue;
            }
            if (float.TryParse(v3[0], out var x) &&
                float.TryParse(v3[1], out var y) &&
                float.TryParse(v3[2], out var z)) {
                return new Vector3(x, y, z);
            }
            return defaultValue;
        }

        public static bool TryParse(string data, out Vector3 value) {
            data = data.Trim();
            var v3 = data.Split(',');
            if (v3.Length < 3) {
                value = Vector3.zero;
                return false;
            }
            if (float.TryParse(v3[0], out var x) &&
                float.TryParse(v3[1], out var y) &&
                float.TryParse(v3[2], out var z)) {
                value = new Vector3(x, y, z);
                return true;
            }
            value = Vector3.zero;
            return false;
        }

        public static string EncodeV3(Vector3 vector) {
            return $"{vector.x},{vector.y},{vector.z}";
        }

        public static bool TryParse(string data, out Quaternion value) {
            data = data.Trim();
            var qt = data.Split(',');
            if (qt.Length < 3) {
                value = Quaternion.identity;
                return false;
            }
            if (float.TryParse(qt[0], out var x) &&
                float.TryParse(qt[1], out var y) &&
                float.TryParse(qt[2], out var z)) {
                value = Quaternion.Euler(x,y,z);
                return true;
            }
            value = Quaternion.identity;
            return false;
        }

        public static string EncodeEulerQuaternion(Quaternion vector) {
            return $"{vector.x},{vector.y},{vector.z}";
        }
        
        public static T TryParseEnum<T>(string data, T defaultValue) {
            //var type = typeof(T);
            //if (type.GetEnumUnderlyingType() == typeof(int)) {
            //    if (!Int32.TryParse(data, out var intEnumValue)) {
            //        return defaultValue;
            //    }
            //    return (T) Enum.ToObject(typeof(T), intEnumValue);
            //}
            //if (type.GetEnumUnderlyingType() == typeof(byte)) {
            //    if (!byte.TryParse(data, out var byteEnumValue)) {
            //        return defaultValue;
            //    }
            //    return (T) Enum.ToObject(typeof(T), byteEnumValue);
            //}
            //return defaultValue;
            if (EnumHelper.TryParse(data, out T value)) {
                return value;
            }
            return defaultValue;
        }

        public static int TryParse(this IList<string> lines, int parseIndex, int defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseInt out of range at {0}", parseIndex);
                return defValue;
            }
            int value;
            if (!int.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            return value;
        }

        public static float TryParse(this IList<string> lines, int parseIndex, float defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseFloat out of range at {0}", parseIndex);
                return defValue;
            }
            float value;
            if (!float.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            return value;
        }

        public static bool TryParse(this IList<string> lines, int parseIndex, bool defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseBool out of range at {0}", parseIndex);
                return defValue;
            }
            bool value;
            if (!bool.TryParse(lines[parseIndex], out value)) {
                value = defValue;
            }
            return value;
        }

        public static LeveledFloat TryParse(this IList<string> lines, int parseIndex, LeveledFloat defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseBool out of range at {0}", parseIndex);
                return defValue;
            }
            LeveledFloat value = LeveledFloat.Parse(lines[parseIndex]);
            return value == null ? defValue : value;
        }

        public static Color TryParse(this IList<string> lines, int parseIndex, Color defValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParseBool out of range at {0}", parseIndex);
                return defValue;
            }
            Color color;
            return ColorUtility.TryParseHtmlString(lines[parseIndex], out color) ? color : defValue;
        }

        public static string TryParseString(this IList<string> lines, int parseIndex, string defaultVal) {
            if (!lines.HasIndex(parseIndex)) {
                return defaultVal;
            }
            var value = lines[parseIndex];
            return value;
        }

        public static IntRange TryParseRange(this IList<string> lines, int parseIndex, IntRange defaultVal) {
            if (!lines.HasIndex(parseIndex)) {
                return defaultVal;
            }
            var numbers = lines[parseIndex].Split('-');
            if (numbers.Length < 3) {
                return defaultVal;
            }
            int num1, num2;
            if (!int.TryParse(numbers[0], out num1)) {
                return defaultVal;
            }
            if (!int.TryParse(numbers[1], out num2)) {
                return defaultVal;
            }
            return new IntRange(num1, num2);
        }

        public static T TryParse<T>(this IList<string> lines, int parseIndex, T defaultValue) {
            if (!lines.HasIndex(parseIndex)) {
                Debug.LogFormat("TryParse out of range at {0}", parseIndex);
                return defaultValue;
            }
            T value;
            if (!EnumHelper.TryParse(lines[parseIndex], out value)) {
                value = defaultValue;
            }
            return value;
        }

        public static List<string> TryChildSplit(this IList<string> lines, int parseIndex) {
            if (!lines.HasIndex(parseIndex)) {
                return null;
            }
            return StringUtilities.SplitChildMultiEntry(lines[parseIndex]);
            
        }
    }

    public static class PronounExtension {
        public static string NewPlayerName(this PlayerPronouns pronouns) {
            switch (pronouns) {
                case PlayerPronouns.He:
                    return StaticTextDatabase.RandomPlayerMaleName();
                case PlayerPronouns.She:
                    return StaticTextDatabase.RandomPlayerFemaleName();
            }
            return Game.CoinFlip() ? StaticTextDatabase.RandomPlayerMaleName() : StaticTextDatabase.RandomPlayerFemaleName();
        }
    }

    public static class GridExtension {
        public static Vector3 GridPositionPlace(this Point3 gridPos) {
            var ray = new Ray(gridPos.CellToWorldV3(), Vector3.down);
            if (Physics.Raycast(ray, out var hit, Game.MapCellSize * 5, LayerMasks.Floor)) {
                return hit.point;
            }
            return ray.origin;
        }


        public static Vector3 CellToWorldV3(this Point3 p) {
            return Game.GridToWorld(p);
        }

        public static Point3 ToCellGridP3(this Vector3 p) {
            return Game.WorldToGrid(p);
        }

        public static Point3 ToCellGridP3ZeroY(this Vector3 p) {
            return Game.WorldToGrid(new Vector3(p.x, 0, p.z));
        }
        /*
        [16][15][14][13][12]
        [17][ 4][ 3][ 2][11]
        [18][ 5][ 0][ 1][10]
        [19][ 6][ 7][ 8][ 9]
        [20][21][22][23][24]
        */
        public static Vector3 GridSpiral(int n) {
            float k = Mathf.Ceil((Mathf.Sqrt(n) - 1.0f) / 2.0f);
            float t = 2.0f * k;
            float m = (t + 1f) * (t + 1f);
            if (n >= m - t) {
                return new Vector3(k -(m - n), 0f, -k);
            }
            m = m - t;
            if (n >= m - t) {
                return new Vector3(-k, 0f, -k + (m - n));
            }
            m = m - t;
            if (n >= m - t) {
                return new Vector3(-k + (m - n), 0f, k);
            }
            return new Vector3(k, 0f, k -(m - n - t));
        }

        public static Point3 GridSpiralP3(int n) {
            float k = Mathf.Ceil((Mathf.Sqrt(n) - 1.0f) / 2.0f);
            float t = 2.0f * k;
            float m = (t + 1f) * (t + 1f);
            if (n >= m - t) {
                return new Point3(k - (m - n), 0f, -k);
            }
            m = m - t;
            if (n >= m - t) {
                return new Point3(-k, 0f, -k + (m - n));
            }
            m = m - t;
            if (n >= m - t) {
                return new Point3(-k + (m - n), 0f, k);
            }
            return new Point3(k, 0f, k - (m - n - t));
        }
    }

    public static class TextureScaler {
        public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
                Rect texR = new Rect(0,0,width,height);
                GpuScale(src,width,height,mode);
               
                //Get rendered data back to a new texture
                Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
                result.Resize(width, height);
                result.ReadPixels(texR,0,0,true);
                return result;                 
        }
       
        /// <summary>
        /// Scales the texture data of the given texture.
        /// </summary>
        /// <param name="tex">Texure to scale</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="mode">Filtering mode</param>
        public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
                Rect texR = new Rect(0,0,width,height);
                GpuScale(tex,width,height,mode);
               
                // Update new texture
                tex.Resize(width, height);
                tex.ReadPixels(texR,0,0,true);
                tex.Apply(true);        //Remove this if you hate us applying textures for you :)
        }
               
        // Internal unility that renders the source texture into the RTT - the scaling method itself.
        private static void GpuScale(Texture2D src, int width, int height, FilterMode fmode)
        {
                //We need the source texture in VRAM because we render with it
                src.filterMode = fmode;
                src.Apply(true);       
                               
                //Using RTT for best quality and performance. Thanks, Unity 5
                RenderTexture rtt = new RenderTexture(width, height, 32);
               
                //Set the RTT in order to render to it
                Graphics.SetRenderTarget(rtt);
               
                //Setup 2D matrix in range 0..1, so nobody needs to care about sized
                GL.LoadPixelMatrix(0,1,1,0);
               
                //Then clear & draw the texture to fill the entire RTT.
                GL.Clear(true,true,new Color(0,0,0,0));
                Graphics.DrawTexture(new Rect(0,0,1,1),src);
        }
    }

    public static class ActionTypeExtensions {
        public static string GetPowerFromSource(this ActionSource source) {
            switch (source) {
                case ActionSource.Melee:
                    return Stats.BonusPowerMelee;
                case ActionSource.Ranged:
                    return Stats.BonusPowerRanged;
                default:
                    return Stats.BonusPowerMagic;
            }
        }

        public static string GetToHitFromSource(this ActionSource source) {
            switch (source) {
                case ActionSource.Melee:
                    return Stats.BonusToHitMelee;
                case ActionSource.Ranged:
                    return Stats.BonusToHitRanged;
                default:
                    return Stats.BonusToHitMagic;
            }
        }

        public static string GetCritFromSource(this ActionSource source) {
            switch (source) {
                case ActionSource.Melee:
                    return Stats.BonusCritMelee;
                case ActionSource.Ranged:
                    return Stats.BonusCritRanged;
                default:
                    return Stats.BonusCritMagic;
            }
        }
    }


#if UNITY_EDITOR
    public static class SceneExtensions {
        public static ValueDropdownList<int> GetBuildSceneList() {
            var list = new ValueDropdownList<int>();
            var sceneList = EditorBuildSettings.scenes;
            Regex regex = new Regex(@"([^/]*/)*([\w\d\-]*)\.unity");
            for (int i = 0; i < sceneList.Length; i++) {
                list.Add(new ValueDropdownItem<int>(regex.Replace(sceneList[i].path, "$2"), i));
            }
            return list;
        }
    }
#endif
}