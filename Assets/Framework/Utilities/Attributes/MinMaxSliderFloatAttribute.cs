using System;
using UnityEngine;

public class MinMaxSliderFloatAttribute : PropertyAttribute {

	public readonly float max;
	public readonly float min;

	public MinMaxSliderFloatAttribute (float min, float max) {
		this.min = min;
		this.max = max;
	}
}
