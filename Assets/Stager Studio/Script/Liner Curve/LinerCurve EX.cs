namespace StagerStudio.LinerCurve {
	using UnityEngine;

	// Float
	public class LinerFloat : LinerCurve<float> {
		protected override float ItemToFloat (float item) => item;
		protected override float Lerp (float l, float r, float t) => Mathf.LerpUnclamped(l, r, t);
		protected override float ItemMuti (float item, float muti) => item * muti;
		public LinerFloat () { }
		public LinerFloat (params (float, float)[] items) {
			foreach (var (key, value) in items) {
				Add(key, value);
			}
		}
	}

	// Color
	public class LinerColor : LinerCurve<Color> {
		protected override float ItemToFloat (Color item) => item.a;
		protected override Color Lerp (Color l, Color r, float t) => Color.LerpUnclamped(l, r, t);
		protected override Color ItemMuti (Color item, float muti) => item * muti;
	}

}