using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

namespace PixelComrades{
public static class EasingExtensionMethods{
	public static Task MoveTo(this Transform trans, Vector3 target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                          Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                          System.Action onComplete = null) {
			Vector3 start = trans.position;
			return new Tween((Vector3 pos)=>{
				trans.position = pos;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
		public static Task LocalMoveTo(this Transform trans, Vector3 target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                               Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                               System.Action onComplete = null)
		{
			Vector3 start = trans.localPosition;
			return new Tween((Vector3 pos)=>{
				trans.localPosition = pos;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
		public static Task WarpAndMoveTo(this Transform trans, Vector3 start, Vector3 target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                          Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                          System.Action onComplete = null)
		{
			return new Tween((Vector3 pos)=>{
				trans.position = pos;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
			
		}
		public static Task ScaleTo(this Transform trans, Vector3 target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                          Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                          System.Action onComplete = null)
		{
			Vector3 start = trans.localScale;
			return new Tween((Vector3 scale)=>{
				trans.localScale = scale;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
		public static Task RotateTo(this Transform trans, Vector3 target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                          Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                          System.Action onComplete = null)
		{
			Vector3 start = trans.localEulerAngles;
			return new Tween((Vector3 rot)=>{
				trans.localEulerAngles = rot;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
		public static Task FadeTo(this CanvasGroup cgroup, float target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                            Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                            System.Action onComplete = null) {
			float start = cgroup.alpha;
			return new Tween((float newAlpha)=>{
				cgroup.alpha = newAlpha;
                cgroup.interactable = newAlpha > 0.5f;
                cgroup.blocksRaycasts = newAlpha > 0.5f;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
        public static Task FadeTo(this TextMeshProUGUI cgroup, float target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                            Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                            System.Action onComplete = null)
		{
			float start = cgroup.alpha;
			return new Tween((float newAlpha)=>{
				cgroup.alpha = newAlpha;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
		public static Task EaseFill(this Image image, float target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                          Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                          System.Action onComplete = null)
		{
			float start = image.fillAmount;
			return new Tween((float newFill)=>{
				image.fillAmount = newFill;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
		public static Task EaseLayoutMinValues(this LayoutElement layout, Vector2 target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                                 Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                                 System.Action onComplete = null)
		{
			Vector2 start = new Vector2(layout.minWidth, layout.minHeight);
			return new Tween((Vector2 newDimensions)=>{
				layout.minWidth = newDimensions.x;
				layout.minHeight = newDimensions.y;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}
		public static Task jump(this ScrollRect scrollRect, Vector2 target, float length, EasingTypes easingChoice, bool unscaled = false, 
		                                            Tween.TweenRepeat repeat = Tween.TweenRepeat.Once,
		                                            System.Action onComplete = null)
		{
			Vector2 start = scrollRect.normalizedPosition;
			return new Tween((Vector2 newPosition)=>{
				scrollRect.normalizedPosition = newPosition;
			}, start, target, length, easingChoice, unscaled, repeat, onComplete).Play();
		}

		public static float Difference(this float f, float a, float b){
			if(a > b) {
                return -Mathf.Abs(a-b);
            }

            return Mathf.Abs(a-b);
		}
		public static Color ease(this Color c, Func<float, float, float, float>easingFunction, Color from, Color to, float t){
			float newR = easingFunction(from.r, to.r, t);
			float newG = easingFunction(from.g, to.g, t);
			float newB = easingFunction(from.b, to.b, t);
			float newA = easingFunction(from.a, to.a, t);
			
			return new Color(newR, newG, newB, newA);
		}
		public static Vector3 ease(this Vector3 v3, Func<float, float, float, float>easingFunction, Vector3 from, Vector3 to, float t){
			float newX = easingFunction(from.x, to.x, t);
			float newY = easingFunction(from.y, to.y, t);
			float newZ = easingFunction(from.z, to.z, t);

			return new Vector3(newX, newY, newZ);
		}
		public static Vector2 ease(this Vector2 v2, Func<float, float, float, float>easingFunction, Vector2 from, Vector2 to, float t){
			float newX = easingFunction(from.x, to.x, t);
			float newY = easingFunction(from.y, to.y, t);

			return new Vector2(newX, newY);
		}
		public static AnimationCurve graph(this AnimationCurve graph, Func<float, float, float, float> EasingFunction , int steps, float from, float to, float length = 1f)
		{
			graph = new AnimationCurve();
			for(int i = 0; i <= steps; i++)
			{
				float time = (float)i/(float)steps;
				float value = EasingFunction(from,to,time);
				graph.AddKey(new Keyframe(time * length, value));
				graph.SmoothTangents(i, 0f);
			}
			return graph;
		}
		public static AnimationCurve smoothAllTangents(this AnimationCurve graph, float weigth)
		{
			for(int i = 0; i < graph.keys.Length; i++)
			{
				graph.SmoothTangents(i, weigth);
			}
			return graph;
		}
		public static Keyframe[] easedKeys(Func<float, float, float, float> EasingFunction , int steps, float from, float to, float tolerance = 0f, float length = 1f, float offset = 0f)
		{
			Keyframe[] newkeys = new Keyframe[steps+1];
			for(int i = 0; i <= steps; i++)
			{
				float time = (float)i/(float)steps;
				float value = EasingFunction(from,to,time);
				newkeys[i] = new Keyframe(offset + (time * length), value);
			}
			if(!Mathf.Approximately(tolerance, 0f))
			{
				return DouglasPeuckerReduction(new List<Keyframe>(newkeys), (double) tolerance).ToArray();
			}else
			{
				return newkeys;
			}
		}
		public static T[] Slice<T>(this T[] source, int start, int end)
		{
			// Handles negative ends.
			if (end < 0)
			{
				end = source.Length + end;
			}
			int len = end - start;
			
			// Return new array.
			T[] res = new T[len];
			for (int i = 0; i < len; i++)
			{
				res[i] = source[i + start];
			}
			return res;
		}

		/// <summary>
		/// Reduces the keyframes of the AnimationCurve with the given tolerance using doug
		/// </summary>
		/// <param name="graph">Graph.</param>
		/// <param name="tolerance">Tolerance.</param>
		public static void reduceKeyframes(this AnimationCurve graph, Double tolerance)
		{
			if(Mathf.Approximately(0f, (float) tolerance) || tolerance < 0) {
                return;
            }

            List<Keyframe> keyframes = new List<Keyframe>(graph.keys);
			keyframes = DouglasPeuckerReduction(keyframes, tolerance);
			graph.keys = keyframes.ToArray();
		}

		// c# implementation of the Ramer-Douglas-Peucker-Algorithm by Craig Selbert slightly adapted for Unity Keyframes
		//http://www.codeproject.com/Articles/18936/A-Csharp-Implementation-of-Douglas-Peucker-Line-Ap
		public static List<Keyframe> DouglasPeuckerReduction
			(List<Keyframe> Points, Double Tolerance)
		{
			if (Points == null || Points.Count < 3) {
                return Points;
            }

            Int32 firstPoint = 0;
			Int32 lastPoint = Points.Count - 1;
			List<Int32> pointIndexsToKeep = new List<Int32>();
			
			//Add the first and last index to the keepers
			pointIndexsToKeep.Add(firstPoint);
			pointIndexsToKeep.Add(lastPoint);
			
			//The first and the last point cannot be the same
			while (Points[firstPoint].Equals(Points[lastPoint]))
			{
				lastPoint--;
			}
			
			DouglasPeuckerReduction(Points, firstPoint, lastPoint, 
			                        Tolerance, ref pointIndexsToKeep);
			
			List<Keyframe> returnPoints = new List<Keyframe>();
			pointIndexsToKeep.Sort();
			foreach (Int32 index in pointIndexsToKeep)
			{
				returnPoints.Add(Points[index]);
			}
			
			return returnPoints;
		}

		private static void DouglasPeuckerReduction(List<Keyframe> 
		                                            points, Int32 firstPoint, Int32 lastPoint, Double tolerance, 
		                                            ref List<Int32> pointIndexsToKeep)
		{
			Double maxDistance = 0;
			Int32 indexFarthest = 0;
			
			for (Int32 index = firstPoint; index < lastPoint; index++)
			{
				Double distance = (Double)PerpendicularDistance
					(points[firstPoint], points[lastPoint], points[index]);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					indexFarthest = index;
				}
			}
			
			if (maxDistance > tolerance && indexFarthest != 0)
			{
				//Add the largest point that exceeds the tolerance
				pointIndexsToKeep.Add(indexFarthest);
				
				DouglasPeuckerReduction(points, firstPoint, 
				                        indexFarthest, tolerance, ref pointIndexsToKeep);
				DouglasPeuckerReduction(points, indexFarthest, 
				                        lastPoint, tolerance, ref pointIndexsToKeep);
			}
		}

		public static float PerpendicularDistance
			(Keyframe Point1, Keyframe Point2, Keyframe Point)
		{
			float area = Mathf.Abs(.5f * (Point1.time * Point2.value + Point2.time * 
			                             Point.value + Point.time * Point1.value - Point2.time * Point1.value - Point.time * 
			                             Point2.value - Point1.time * Point.value));
			float bottom = Mathf.Sqrt(Mathf.Pow(Point1.time - Point2.time, 2f) + 
			                          Mathf.Pow(Point1.value - Point2.value, 2f));
			float height = area / bottom * 2f;
			
			return height;

		}
}
}
