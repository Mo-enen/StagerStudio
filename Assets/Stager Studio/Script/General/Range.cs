namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[System.Serializable]
	public struct Range {

		// API
		public readonly static Range Unlimited = new Range(float.MinValue, float.MaxValue, false, false);
		public readonly static Range GreaterThanZero = new Range(0f, float.MaxValue, true, false);
		public readonly static Range LessThanZero = new Range(float.MinValue, 0f, false, true);
		public readonly static Range ZeroToOne = new Range(0f, 1f, true, true);
		public readonly static Range Zero = new Range(0f, 0f, true, true);
		public readonly static Range Half = new Range(0.5f, 0.5f, true, true);
		public readonly static Range One = new Range(1f, 1f, true, true);

		// Ser-API
		public float Min;
		public float Max;
		public bool HasMin;
		public bool HasMax;

		// API
		public Range (float? min, float? max) : this(min ?? float.MinValue, max ?? float.MaxValue, min.HasValue, max.HasValue) { }

		public Range (float fixedValue) : this(fixedValue, fixedValue, true, true) { }

		public Range (float min, float max, bool hasMin, bool hasMax) {
			HasMin = hasMin;
			HasMax = hasMax;
			Min = min;
			Max = max;
		}

		public float Clamp (float value) => Mathf.Clamp(value, HasMin ? Min : float.MinValue, HasMax ? Max : float.MaxValue);

		public float Lerp (float lerp) => HasMin && HasMax ? Mathf.Lerp(Min, Max, lerp) : lerp;

		public float LerpUnclamped (float lerp) => HasMin && HasMax ? Mathf.LerpUnclamped(Min, Max, lerp) : lerp;

	}

}