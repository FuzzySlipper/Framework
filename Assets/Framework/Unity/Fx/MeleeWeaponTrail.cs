
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MeleeWeaponTrail : MonoBehaviour, IPoolEvents {

        [SerializeField] private bool _useInterpolation = true;
        [SerializeField] float _lifeTime = 1.0f;
        [SerializeField] Color[] _colors = new Color[0];
        [SerializeField] float[] _sizes = new float[0];
        [SerializeField] float _minVertexDistance = 0.1f;
        [SerializeField] float _maxVertexDistance = 10.0f;
        [SerializeField] float _maxAngle = 3.00f;
        [SerializeField] int _subdivisions = 4;
        [SerializeField] private Transform _base = null;
        [SerializeField] private Transform _tip = null;
        [SerializeField] private bool _preview = false;
        [SerializeField] private bool _removeWorldPosition = true;
        [SerializeField] private Material _material = null;

        private List<Point> _points = new List<Point>();
        private List<Point> _smoothedPoints = new List<Point>();
        private Mesh _trailMesh = null;
        private Vector3 _lastPosition;
        private float _minVertexDistanceSqr = 0.0f;
        private float _maxVertexDistanceSqr = 0.0f;
        private List<Vector3> _smoothTipList = new List<Vector3>();
        private List<Vector3> _smoothBaseList = new List<Vector3>();
        private RendererHolder _holder;
        private bool _active = false;
        //private MeshRenderer _trailRenderer = null;
        //private MeshFilter _trailFilter = null;

        public struct Point {
            public float TimeCreated;
            public Vector3 BasePosition;
            public Vector3 TipPosition;

            public Point(float timeCreated, Vector3 basePosition, Vector3 tipPosition) {
                TimeCreated = timeCreated;
                BasePosition = basePosition;
                TipPosition = tipPosition;
            }

            public Point(Point p, Vector3 basePosition, Vector3 tipPosition) {
                TimeCreated = p.TimeCreated;
                BasePosition = basePosition;
                TipPosition = tipPosition;
            }

        }

        public void OnPoolSpawned() {
            Setup();
        }

        public void OnPoolDespawned() {
            Store(_holder);
            if (_trailMesh != null) {
                _trailMesh.Clear();
                _trailMesh = null;
            }
            _holder = null;
        }

        void Awake() {
            Setup();
        }

        void OnEnable() {
            SetActive(true);
        }

        void OnDisable() {
            SetActive(false);
        }

        private void OnDestroy() {
            if (_holder != null) {
                Store(_holder);
                _holder = null;
            }
        }

        public void SetActive(bool status) {
            _active = status;
            Clear();
            if (_holder != null) {
                _holder.Renderer.enabled = status;
            }
        }

        private void Setup() {
            if (_holder == null) {
                _holder = GetRenderer(this);
            }
            SetLastPosition();
            _trailMesh = new Mesh();
            _trailMesh.name = name + "TrailMesh";
            _holder.Filter.sharedMesh = _trailMesh;

            _minVertexDistanceSqr = _minVertexDistance * _minVertexDistance;
            _maxVertexDistanceSqr = _maxVertexDistance * _maxVertexDistance;
        }

        private void Clear() {
            _points.Clear();
            _smoothedPoints.Clear();
            if (_trailMesh != null) {
                _trailMesh.Clear();
            }
        }

        void Update() {
            RunUpdate(TimeManager.DeltaTime);
        }

        private void SetLastPosition() {
            _lastPosition = _removeWorldPosition ? _holder.Tr.InverseTransformPoint(transform.position) : transform.position;
            //_lastPosition = _removeWorldPosition ? (transform.root.position - transform.position) : transform.position;
        }

        private float GetDistance() {
            //var lastPosition = _removeWorldPosition ? (transform.root.position - transform.position) : transform.position;
            var lastPosition = _removeWorldPosition ? _holder.Tr.InverseTransformPoint(transform.position) : transform.position;
            return (_lastPosition - lastPosition).sqrMagnitude;
        }

        private Point CreatePoint() {
            //return new Point(Time.time, transform.root.position - _base.position, transform.root.position - _tip.position);
            return new Point(TimeManager.TimeUnscaled, _holder.Tr.InverseTransformPoint(_base.position), _holder.Tr.InverseTransformPoint(_tip.position));
        }

        private Point CreatePoint(Point p) {
            //return new Point(p, transform.root.position - _base.position, transform.root.position - _tip.position);
            return new Point(p, _holder.Tr.InverseTransformPoint(_base.position), _holder.Tr.InverseTransformPoint(_tip.position));
        }

        public void RunUpdate(float dt) {
            if (!_active) {
                return;
            }
            //if (_autoDestruct && !CheckEmit(dt)) {
            //       return;
            //}
            if (_holder == null || _holder.Tr == null) {
                Setup();
            }
            if (_holder.Tr.parent != transform.root) {
                _holder.Tr.SetParentResetPos(transform.root);
            }
            if (!_holder.Renderer.enabled) {
                _holder.Renderer.enabled = true;
            }
            // if we have moved enough, create a new vertex and make sure we rebuild the mesh
            float theDistanceSqr = GetDistance();
            if (theDistanceSqr > _minVertexDistanceSqr) {
                bool make = false;
                if (_points.Count < 3) {
                    make = true;
                }
                else {
                    //Vector3 l1 = _points[_points.Count - 2].basePosition - _points[_points.Count - 3].basePosition;
                    //Vector3 l2 = _points[_points.Count - 1].basePosition - _points[_points.Count - 2].basePosition;
                    Vector3 l1 = _points[_points.Count - 2].TipPosition - _points[_points.Count - 3].TipPosition;
                    Vector3 l2 = _points[_points.Count - 1].TipPosition - _points[_points.Count - 2].TipPosition;
                    if (Vector3.Angle(l1, l2) > _maxAngle || theDistanceSqr > _maxVertexDistanceSqr)
                        make = true;
                }
                if (make) {
                    Point p = CreatePoint();
                    _points.Add(p);
                    SetLastPosition();
                    if (_useInterpolation) {
                        if (_points.Count == 1) {
                            _smoothedPoints.Add(p);
                        }
                        else if (_points.Count > 1) {
                            // add 1+subdivisions for every possible pair in the _points
                            for (int n = 0; n < 1 + _subdivisions; ++n)
                                _smoothedPoints.Add(p);
                        }

                        // we use 4 control points for the smoothing
                        if (_points.Count >= 4) {
                            Vector3[] tipPoints = new Vector3[4];
                            tipPoints[0] = _points[_points.Count - 4].TipPosition;
                            tipPoints[1] = _points[_points.Count - 3].TipPosition;
                            tipPoints[2] = _points[_points.Count - 2].TipPosition;
                            tipPoints[3] = _points[_points.Count - 1].TipPosition;

                            //IEnumerable<Vector3> smoothTip = Interpolate.NewBezier(Interpolate.Ease(Interpolate.EaseType.Linear), tipPoints, subdivisions);
                            IEnumerable<Vector3> smoothTip = Interpolate.NewCatmullRom(tipPoints, _subdivisions, false);
                            Vector3[] basePoints = new Vector3[4];
                            basePoints[0] = _points[_points.Count - 4].BasePosition;
                            basePoints[1] = _points[_points.Count - 3].BasePosition;
                            basePoints[2] = _points[_points.Count - 2].BasePosition;
                            basePoints[3] = _points[_points.Count - 1].BasePosition;

                            //IEnumerable<Vector3> smoothBase = Interpolate.NewBezier(Interpolate.Ease(Interpolate.EaseType.Linear), basePoints, subdivisions);
                            IEnumerable<Vector3> smoothBase = Interpolate.NewCatmullRom(basePoints, _subdivisions, false);
                            _smoothTipList.Clear();
                            _smoothTipList.AddRange(smoothTip);
                            _smoothBaseList.Clear();
                            _smoothBaseList.AddRange(smoothBase);
                            float firstTime = _points[_points.Count - 4].TimeCreated;
                            float secondTime = _points[_points.Count - 1].TimeCreated;

                            //Debug.Log(" smoothTipList.Count: " + smoothTipList.Count);
                            for (int n = 0; n < _smoothTipList.Count; ++n) {
                                int idx = _smoothedPoints.Count - (_smoothTipList.Count - n);
                                // there are moments when the _smoothedPoints are lesser
                                // than what is required, when elements from it are removed
                                if (idx > -1 && idx < _smoothedPoints.Count) {
                                    Point sp = new Point(Mathf.Lerp(firstTime, secondTime, (float) n / _smoothTipList.Count), _smoothBaseList[n], _smoothTipList[n]);
                                    _smoothedPoints[idx] = sp;
                                }
                            }
                        }
                    }
                }
                else {
                    _points[_points.Count - 1] = CreatePoint(_points[_points.Count - 1]);
                    //_points[_points.Count - 1].timeCreated = Time.time;
                    if (_useInterpolation) {
                        _smoothedPoints[_smoothedPoints.Count - 1] = CreatePoint(_smoothedPoints[_smoothedPoints.Count - 1]);
                    }
                }
            }
            else {
                if (_points.Count > 0) {
                    _points[_points.Count - 1] = CreatePoint(_points[_points.Count - 1]);
                    //_points[_points.Count - 1].timeCreated = Time.time;
                }
                if (_useInterpolation) {
                    if (_smoothedPoints.Count > 0) {
                        _smoothedPoints[_smoothedPoints.Count - 1] = CreatePoint(_smoothedPoints[_smoothedPoints.Count - 1]);
                    }
                }
            }
            RemoveOldPoints(_points);
            if (_points.Count == 0) {
                _trailMesh.Clear();
            }

            if (_useInterpolation) {
                RemoveOldPoints(_smoothedPoints);
                if (_smoothedPoints.Count == 0) {
                    _trailMesh.Clear();
                }
            }

            List<Point> pointsToUse = _useInterpolation ? _smoothedPoints : _points;

            if (pointsToUse.Count > 1) {
                Vector3[] newVertices = new Vector3[pointsToUse.Count * 2];
                Vector2[] newUV = new Vector2[pointsToUse.Count * 2];
                int[] newTriangles = new int[(pointsToUse.Count - 1) * 6];
                Color[] newColors = new Color[pointsToUse.Count * 2];

                for (int n = 0; n < pointsToUse.Count; ++n) {
                    Point p = pointsToUse[n];
                    float time = (Time.time - p.TimeCreated) / _lifeTime;

                    Color color = Color.Lerp(Color.white, Color.clear, time);
                    if (_colors != null && _colors.Length > 0) {
                        float colorTime = time * (_colors.Length - 1);
                        float min = Mathf.Floor(colorTime);
                        float max = Mathf.Clamp(Mathf.Ceil(colorTime), 1, _colors.Length - 1);
                        float lerp = Mathf.InverseLerp(min, max, colorTime);
                        if (min >= _colors.Length)
                            min = _colors.Length - 1;
                        if (min < 0)
                            min = 0;
                        if (max >= _colors.Length)
                            max = _colors.Length - 1;
                        if (max < 0)
                            max = 0;
                        color = Color.Lerp(_colors[(int) min], _colors[(int) max], lerp);
                    }

                    float size = 0f;
                    if (_sizes != null && _sizes.Length > 0) {
                        float sizeTime = time * (_sizes.Length - 1);
                        float min = Mathf.Floor(sizeTime);
                        float max = Mathf.Clamp(Mathf.Ceil(sizeTime), 1, _sizes.Length - 1);
                        float lerp = Mathf.InverseLerp(min, max, sizeTime);
                        if (min >= _sizes.Length)
                            min = _sizes.Length - 1;
                        if (min < 0)
                            min = 0;
                        if (max >= _sizes.Length)
                            max = _sizes.Length - 1;
                        if (max < 0)
                            max = 0;
                        size = Mathf.Lerp(_sizes[(int) min], _sizes[(int) max], lerp);
                    }

                    Vector3 lineDirection = p.TipPosition - p.BasePosition;

                    newVertices[n * 2] = p.BasePosition - (lineDirection * (size * 0.5f));
                    newVertices[(n * 2) + 1] = p.TipPosition + (lineDirection * (size * 0.5f));

                    newColors[n * 2] = newColors[(n * 2) + 1] = color;

                    float uvRatio = (float) n / pointsToUse.Count;
                    newUV[n * 2] = new Vector2(uvRatio, 0);
                    newUV[(n * 2) + 1] = new Vector2(uvRatio, 1);

                    if (n > 0) {
                        newTriangles[(n - 1) * 6] = (n * 2) - 2;
                        newTriangles[((n - 1) * 6) + 1] = (n * 2) - 1;
                        newTriangles[((n - 1) * 6) + 2] = n * 2;

                        newTriangles[((n - 1) * 6) + 3] = (n * 2) + 1;
                        newTriangles[((n - 1) * 6) + 4] = n * 2;
                        newTriangles[((n - 1) * 6) + 5] = (n * 2) - 1;
                    }
                }

                _trailMesh.Clear();
                _trailMesh.vertices = newVertices;
                _trailMesh.colors = newColors;
                _trailMesh.uv = newUV;
                _trailMesh.triangles = newTriangles;
            }
        }

        void RemoveOldPoints(List<Point> pointList) {
            //List<Point> remove = new List<Point>();
            //for (var i = 0; i < pointList.Count; i++) {
            //    Point p = pointList[i];
            //    if (Time.time - p.TimeCreated > _lifeTime) {
            //        remove.Add(p);
            //    }
            //}
            for (int i = pointList.Count - 1; i >= 0; i--) {
                Point p = pointList[i];
                if (TimeManager.TimeUnscaled - p.TimeCreated > _lifeTime) {
                    pointList.RemoveAt(i);
                }
            }
            //for (var i = 0; i < remove.Count; i++) {
            //    Point p = remove[i];
            //    pointList.Remove(p);
            //}
        }

        private void OnDrawGizmos() {
            if (_tip != null) {
                Gizmos.DrawSphere(_tip.position, 0.025f);
            }
            if (_preview && gameObject.activeInHierarchy) {
                RunUpdate(TimeManager.DeltaUnscaled);
            }
        }

        //private static List<RendererHolder> _renderers = new List<RendererHolder>();

        private static Queue<RendererHolder> _renderers = new Queue<RendererHolder>(2);

        private static RendererHolder GetRenderer(MeleeWeaponTrail trail) {
            RendererHolder renderer;
            if (_renderers.Count > 0) {
                renderer = _renderers.Dequeue();
            }
            else {
                renderer = new RendererHolder();
            }
            if (renderer.Tr == null) {
                renderer = new RendererHolder();
            }
            renderer.Tr.gameObject.SetActive(true);
            renderer.Tr.SetParentResetPos(trail.transform.root);
            renderer.Tr.gameObject.layer = trail.gameObject.layer;
            renderer.Renderer.material = trail._material;
            return renderer;
        }

        private static void Store(RendererHolder renderer) {
            renderer.Filter.sharedMesh = null;
            renderer.Tr.SetParent(null);
            renderer.Tr.gameObject.SetActive(false);
            _renderers.Enqueue(renderer);
        }

        private class RendererHolder {
            public MeshFilter Filter { get; }
            public MeshRenderer Renderer { get; }
            public Transform Tr { get; }

            public RendererHolder() {
                var go = new GameObject("Trail Renderer");
                go.hideFlags = HideFlags.DontSave;
                Filter = go.AddComponent<MeshFilter>();
                Renderer = go.AddComponent<MeshRenderer>();
                Tr = go.transform;
            }
        }
    }
}
/**
 * Interpolation utility functions: easing, bezier, and catmull-rom.
 * Consider using Unity's Animation curve editor and AnimationCurve class
 * before scripting the desired behaviour using this utility.
 *
 * Interpolation functionality available at different levels of abstraction.
 * Low level access via individual easing functions (ex. EaseInOutCirc),
 * Bezier(), and CatmullRom(). High level access using sequence generators,
 * NewEase(), NewBezier(), and NewCatmullRom().
 *
 * Sequence generators are typically used as follows:
 *
 * IEnumerable<Vector3> sequence = Interpolate.New[Ease|Bezier|CatmulRom](configuration);
 * foreach (Vector3 newPoint in sequence) {
 *   transform.position = newPoint;
 *   yield return WaitForSeconds(1.0f);
 * }
 *
 * Or:
 *
 * IEnumerator<Vector3> sequence = Interpolate.New[Ease|Bezier|CatmulRom](configuration).GetEnumerator();
 * function Update() {
 *   if (sequence.MoveNext()) {
 *     transform.position = sequence.Current;
 *   }
 * }
 *
 * The low level functions work similarly to Unity's built in Lerp and it is
 * up to you to track and pass in elapsedTime and duration on every call. The
 * functions take this form (or the logical equivalent for Bezier() and CatmullRom()).
 *
 * transform.position = ease(start, distance, elapsedTime, duration);
 *
 * For convenience in configuration you can use the Ease(EaseType) function to
 * look up a concrete easing function:
 * 
 *  [SerializeField]
 *  Interpolate.EaseType easeType; // set using Unity's property inspector
 *  Interpolate.Function ease; // easing of a particular EaseType
 * function Awake() {
 *   ease = Interpolate.Ease(easeType);
 * }
 *
 * @author Fernando Zapata (fernando@cpudreams.com)
 * @Traduzione Andrea85cs (andrea85cs@dynematica.it)
 */

public class Interpolate {


    /**
 * Different methods of easing interpolation.
 */
    public enum EaseType {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc
    }

    /**
    * Sequence of eleapsedTimes until elapsedTime is >= duration.
    *
    * Note: elapsedTimes are calculated using the value of Time.deltatTime each
    * time a value is requested.
    */
    static Vector3 Identity(Vector3 v) {
        return v;
    }

    static Vector3 TransformDotPosition(Transform t) {
        return t.position;
    }


    static IEnumerable<float> NewTimer(float duration) {
        float elapsedTime = 0.0f;
        while (elapsedTime < duration) {
            yield return elapsedTime;
            elapsedTime += Time.deltaTime;
            // make sure last value is never skipped
            if (elapsedTime >= duration) {
                yield return elapsedTime;
            }
        }
    }

    public delegate Vector3 ToVector3<T>(T v);
    public delegate float Function(float a, float b, float c, float d);

    /**
     * Generates sequence of integers from start to end (inclusive) one step
     * at a time.
     */
    static IEnumerable<float> NewCounter(int start, int end, int step) {
        for (int i = start; i <= end; i += step) {
            yield return i;
        }
    }

    /**
     * Returns sequence generator from start to end over duration using the
     * given easing function. The sequence is generated as it is accessed
     * using the Time.deltaTime to calculate the portion of duration that has
     * elapsed.
     */
    public static IEnumerator NewEase(Function ease, Vector3 start, Vector3 end, float duration) {
        IEnumerable<float> timer = Interpolate.NewTimer(duration);
        return NewEase(ease, start, end, duration, timer);
    }

    /**
     * Instead of easing based on time, generate n interpolated points (slices)
     * between the start and end positions.
     */
    public static IEnumerator NewEase(Function ease, Vector3 start, Vector3 end, int slices) {
        IEnumerable<float> counter = Interpolate.NewCounter(0, slices + 1, 1);
        return NewEase(ease, start, end, slices + 1, counter);
    }



    /**
     * Generic easing sequence generator used to implement the time and
     * slice variants. Normally you would not use this function directly.
     */
    static IEnumerator NewEase(Function ease, Vector3 start, Vector3 end, float total, IEnumerable<float> driver) {
        Vector3 distance = end - start;
        foreach (float i in driver) {
            yield return Ease(ease, start, distance, i, total);
        }
    }

    /**
     * Vector3 interpolation using given easing method. Easing is done independently
     * on all three vector axis.
     */
    static Vector3 Ease(Function ease, Vector3 start, Vector3 distance, float elapsedTime, float duration) {
        start.x = ease(start.x, distance.x, elapsedTime, duration);
        start.y = ease(start.y, distance.y, elapsedTime, duration);
        start.z = ease(start.z, distance.z, elapsedTime, duration);
        return start;
    }

    /**
     * Returns the static method that implements the given easing type for scalars.
     * Use this method to easily switch between easing interpolation types.
     *
     * All easing methods clamp elapsedTime so that it is always <= duration.
     *
     * var ease = Interpolate.Ease(EaseType.EaseInQuad);
     * i = ease(start, distance, elapsedTime, duration);
     */
    public static Function Ease(EaseType type) {
        // Source Flash easing functions:
        // http://gizma.com/easing/
        // http://www.robertpenner.com/easing/easing_demo.html
        //
        // Changed to use more friendly variable names, that follow my Lerp
        // conventions:
        // start = b (start value)
        // distance = c (change in value)
        // elapsedTime = t (current time)
        // duration = d (time duration)

        Function f = null;
        switch (type) {
            case EaseType.Linear: f = Interpolate.Linear; break;
            case EaseType.EaseInQuad: f = Interpolate.EaseInQuad; break;
            case EaseType.EaseOutQuad: f = Interpolate.EaseOutQuad; break;
            case EaseType.EaseInOutQuad: f = Interpolate.EaseInOutQuad; break;
            case EaseType.EaseInCubic: f = Interpolate.EaseInCubic; break;
            case EaseType.EaseOutCubic: f = Interpolate.EaseOutCubic; break;
            case EaseType.EaseInOutCubic: f = Interpolate.EaseInOutCubic; break;
            case EaseType.EaseInQuart: f = Interpolate.EaseInQuart; break;
            case EaseType.EaseOutQuart: f = Interpolate.EaseOutQuart; break;
            case EaseType.EaseInOutQuart: f = Interpolate.EaseInOutQuart; break;
            case EaseType.EaseInQuint: f = Interpolate.EaseInQuint; break;
            case EaseType.EaseOutQuint: f = Interpolate.EaseOutQuint; break;
            case EaseType.EaseInOutQuint: f = Interpolate.EaseInOutQuint; break;
            case EaseType.EaseInSine: f = Interpolate.EaseInSine; break;
            case EaseType.EaseOutSine: f = Interpolate.EaseOutSine; break;
            case EaseType.EaseInOutSine: f = Interpolate.EaseInOutSine; break;
            case EaseType.EaseInExpo: f = Interpolate.EaseInExpo; break;
            case EaseType.EaseOutExpo: f = Interpolate.EaseOutExpo; break;
            case EaseType.EaseInOutExpo: f = Interpolate.EaseInOutExpo; break;
            case EaseType.EaseInCirc: f = Interpolate.EaseInCirc; break;
            case EaseType.EaseOutCirc: f = Interpolate.EaseOutCirc; break;
            case EaseType.EaseInOutCirc: f = Interpolate.EaseInOutCirc; break;
        }
        return f;
    }

    /**
     * Returns sequence generator from the first node to the last node over
     * duration time using the points in-between the first and last node
     * as control points of a bezier curve used to generate the interpolated points
     * in the sequence. If there are no control points (ie. only two nodes, first
     * and last) then this behaves exactly the same as NewEase(). In other words
     * a zero-degree bezier spline curve is just the easing method. The sequence
     * is generated as it is accessed using the Time.deltaTime to calculate the
     * portion of duration that has elapsed.
     */
    public static IEnumerable<Vector3> NewBezier(Function ease, Transform[] nodes, float duration) {
        IEnumerable<float> timer = Interpolate.NewTimer(duration);
        return NewBezier<Transform>(ease, nodes, TransformDotPosition, duration, timer);
    }

    /**
     * Instead of interpolating based on time, generate n interpolated points
     * (slices) between the first and last node.
     */
    public static IEnumerable<Vector3> NewBezier(Function ease, Transform[] nodes, int slices) {
        IEnumerable<float> counter = NewCounter(0, slices + 1, 1);
        return NewBezier<Transform>(ease, nodes, TransformDotPosition, slices + 1, counter);
    }

    /**
     * A Vector3[] variation of the Transform[] NewBezier() function.
     * Same functionality but using Vector3s to define bezier curve.
     */
    public static IEnumerable<Vector3> NewBezier(Function ease, Vector3[] points, float duration) {
        IEnumerable<float> timer = NewTimer(duration);
        return NewBezier<Vector3>(ease, points, Identity, duration, timer);
    }

    /**
     * A Vector3[] variation of the Transform[] NewBezier() function.
     * Same functionality but using Vector3s to define bezier curve.
     */
    public static IEnumerable<Vector3> NewBezier(Function ease, Vector3[] points, int slices) {
        IEnumerable<float> counter = NewCounter(0, slices + 1, 1);
        return NewBezier<Vector3>(ease, points, Identity, slices + 1, counter);
    }

    /**
     * Generic bezier spline sequence generator used to implement the time and
     * slice variants. Normally you would not use this function directly.
     */
    static IEnumerable<Vector3> NewBezier<T>(Function ease, IList nodes, ToVector3<T> toVector3, float maxStep, IEnumerable<float> steps) {
        // need at least two nodes to spline between
        if (nodes.Count >= 2) {
            // copy nodes array since Bezier is destructive
            Vector3[] points = new Vector3[nodes.Count];

            foreach (float step in steps) {
                // re-initialize copy before each destructive call to Bezier
                for (int i = 0; i < nodes.Count; i++) {
                    points[i] = toVector3((T)nodes[i]);
                }
                yield return Bezier(ease, points, step, maxStep);
                // make sure last value is always generated
            }
        }
    }

    /**
     * A Vector3 n-degree bezier spline.
     *
     * WARNING: The points array is modified by Bezier. See NewBezier() for a
     * safe and user friendly alternative.
     *
     * You can pass zero control points, just the start and end points, for just
     * plain easing. In other words a zero-degree bezier spline curve is just the
     * easing method.
     *
     * @param points start point, n control points, end point
     */
    static Vector3 Bezier(Function ease, Vector3[] points, float elapsedTime, float duration) {
        // Reference: http://ibiblio.org/e-notes/Splines/Bezier.htm
        // Interpolate the n starting points to generate the next j = (n - 1) points,
        // then interpolate those n - 1 points to generate the next n - 2 points,
        // continue this until we have generated the last point (n - (n - 1)), j = 1.
        // We store the next set of output points in the same array as the
        // input points used to generate them. This works because we store the
        // result in the slot of the input point that is no longer used for this
        // iteration.
        for (int j = points.Length - 1; j > 0; j--) {
            for (int i = 0; i < j; i++) {
                points[i].x = ease(points[i].x, points[i + 1].x - points[i].x, elapsedTime, duration);
                points[i].y = ease(points[i].y, points[i + 1].y - points[i].y, elapsedTime, duration);
                points[i].z = ease(points[i].z, points[i + 1].z - points[i].z, elapsedTime, duration);
            }
        }
        return points[0];
    }

    /**
     * Returns sequence generator from the first node, through each control point,
     * and to the last node. N points are generated between each node (slices)
     * using Catmull-Rom.
     */
    public static IEnumerable<Vector3> NewCatmullRom(Transform[] nodes, int slices, bool loop) {
        return NewCatmullRom<Transform>(nodes, TransformDotPosition, slices, loop);
    }

    /**
     * A Vector3[] variation of the Transform[] NewCatmullRom() function.
     * Same functionality but using Vector3s to define curve.
     */
    public static IEnumerable<Vector3> NewCatmullRom(Vector3[] points, int slices, bool loop) {
        return NewCatmullRom<Vector3>(points, Identity, slices, loop);
    }

    /**
     * Generic catmull-rom spline sequence generator used to implement the
     * Vector3[] and Transform[] variants. Normally you would not use this
     * function directly.
     */
    static IEnumerable<Vector3> NewCatmullRom<T>(IList nodes, ToVector3<T> toVector3, int slices, bool loop) {
        // need at least two nodes to spline between
        if (nodes.Count >= 2) {

            // yield the first point explicitly, if looping the first point
            // will be generated again in the step for loop when interpolating
            // from last point back to the first point
            yield return toVector3((T)nodes[0]);

            int last = nodes.Count - 1;
            for (int current = 0; loop || current < last; current++) {
                // wrap around when looping
                if (loop && current > last) {
                    current = 0;
                }
                // handle edge cases for looping and non-looping scenarios
                // when looping we wrap around, when not looping use start for previous
                // and end for next when you at the ends of the nodes array
                int previous = (current == 0) ? ((loop) ? last : current) : current - 1;
                int start = current;
                int end = (current == last) ? ((loop) ? 0 : current) : current + 1;
                int next = (end == last) ? ((loop) ? 0 : end) : end + 1;

                // adding one guarantees yielding at least the end point
                int stepCount = slices + 1;
                for (int step = 1; step <= stepCount; step++) {
                    yield return CatmullRom(toVector3((T)nodes[previous]),
                                     toVector3((T)nodes[start]),
                                     toVector3((T)nodes[end]),
                                     toVector3((T)nodes[next]),
                                     step, stepCount);
                }
            }
        }
    }

    /**
     * A Vector3 Catmull-Rom spline. Catmull-Rom splines are similar to bezier
     * splines but have the useful property that the generated curve will go
     * through each of the control points.
     *
     * NOTE: The NewCatmullRom() functions are an easier to use alternative to this
     * raw Catmull-Rom implementation.
     *
     * @param previous the point just before the start point or the start point
     *                 itself if no previous point is available
     * @param start generated when elapsedTime == 0
     * @param end generated when elapsedTime >= duration
     * @param next the point just after the end point or the end point itself if no
     *             next point is available
     */
    static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, 
                                float elapsedTime, float duration) {
        // References used:
        // p.266 GemsV1
        //
        // tension is often set to 0.5 but you can use any reasonable value:
        // http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
        //
        // bias and tension controls:
        // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

        float percentComplete = elapsedTime / duration;
        float percentCompleteSquared = percentComplete * percentComplete;
        float percentCompleteCubed = percentCompleteSquared * percentComplete;

        return previous * (-0.5f * percentCompleteCubed +
                                   percentCompleteSquared -
                            0.5f * percentComplete) +
                start   * ( 1.5f * percentCompleteCubed +
                           -2.5f * percentCompleteSquared + 1.0f) +
                end     * (-1.5f * percentCompleteCubed +
                            2.0f * percentCompleteSquared +
                            0.5f * percentComplete) +
                next    * ( 0.5f * percentCompleteCubed -
                            0.5f * percentCompleteSquared);
    }




    /**
     * Linear interpolation (same as Mathf.Lerp)
     */
    static float Linear(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * (elapsedTime / duration) + start;
    }

    /**
     * quadratic easing in - accelerating from zero velocity
     */
    static float EaseInQuad(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime + start;
    }

    /**
     * quadratic easing out - decelerating to zero velocity
     */
    static float EaseOutQuad(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return -distance * elapsedTime * (elapsedTime - 2) + start;
    }

    /**
     * quadratic easing in/out - acceleration until halfway, then deceleration
     */
    static float EaseInOutQuad(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime + start;
        elapsedTime--;
        return -distance / 2 * (elapsedTime * (elapsedTime - 2) - 1) + start;
    }

    /**
     * cubic easing in - accelerating from zero velocity
     */
    static float EaseInCubic(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime * elapsedTime + start;
    }

    /**
     * cubic easing out - decelerating to zero velocity
     */
    static float EaseOutCubic(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return distance * (elapsedTime * elapsedTime * elapsedTime + 1) + start;
    }

    /**
     * cubic easing in/out - acceleration until halfway, then deceleration
     */
    static float EaseInOutCubic(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime * elapsedTime + start;
        elapsedTime -= 2;
        return distance / 2 * (elapsedTime * elapsedTime * elapsedTime + 2) + start;
    }

    /**
     * quartic easing in - accelerating from zero velocity
     */
    static float EaseInQuart(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
    }

    /**
     * quartic easing out - decelerating to zero velocity
     */
    static float EaseOutQuart(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return -distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1) + start;
    }

    /**
     * quartic easing in/out - acceleration until halfway, then deceleration
     */
    static float EaseInOutQuart(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
        elapsedTime -= 2;
        return -distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2) + start;
    }


    /**
     * quintic easing in - accelerating from zero velocity
     */
    static float EaseInQuint(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
    }

    /**
     * quintic easing out - decelerating to zero velocity
     */
    static float EaseOutQuint(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 1) + start;
    }

    /**
     * quintic easing in/out - acceleration until halfway, then deceleration
     */
    static float EaseInOutQuint(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2f);
        if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
        elapsedTime -= 2;
        return distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 2) + start;
    }

    /**
     * sinusoidal easing in - accelerating from zero velocity
     */
    static float EaseInSine(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return -distance * Mathf.Cos(elapsedTime / duration * (Mathf.PI / 2)) + distance + start;
    }

    /**
     * sinusoidal easing out - decelerating to zero velocity
     */
    static float EaseOutSine(float start, float distance, float elapsedTime, float duration) {
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * Mathf.Sin(elapsedTime / duration * (Mathf.PI / 2)) + start;
    }

    /**
     * sinusoidal easing in/out - accelerating until halfway, then decelerating
     */
    static float EaseInOutSine(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return -distance / 2 * (Mathf.Cos(Mathf.PI * elapsedTime / duration) - 1) + start;
    }

    /**
     * exponential easing in - accelerating from zero velocity
     */
    static float EaseInExpo(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * Mathf.Pow(2, 10 * (elapsedTime / duration - 1)) + start;
    }

    /**
     * exponential easing out - decelerating to zero velocity
     */
    static float EaseOutExpo(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * (-Mathf.Pow(2, -10 * elapsedTime / duration) + 1) + start;
    }

    /**
     * exponential easing in/out - accelerating until halfway, then decelerating
     */
    static float EaseInOutExpo(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 *  Mathf.Pow(2, 10 * (elapsedTime - 1)) + start;
        elapsedTime--;
        return distance / 2 * (-Mathf.Pow(2, -10 * elapsedTime) + 2) + start;
    }

    /**
     * circular easing in - accelerating from zero velocity
     */
    static float EaseInCirc(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return -distance * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
    }

    /**
     * circular easing out - decelerating to zero velocity
     */
    static float EaseOutCirc(float start, float distance, float elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return distance * Mathf.Sqrt(1 - elapsedTime * elapsedTime) + start;
    }

    /**
     * circular easing in/out - acceleration until halfway, then deceleration
     */
    static float EaseInOutCirc(float start, float distance, float
                         elapsedTime, float duration) {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return -distance / 2 * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
        elapsedTime -= 2;
        return distance / 2 * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) + 1) + start;
    }
}
