using System;
using UnityEngine;

public class MinMaxSliderIntAttribute : PropertyAttribute {

	public readonly int max;
	public readonly int min;

	public MinMaxSliderIntAttribute(int min, int max) {
		this.min = min;
		this.max = max;
	}
}
