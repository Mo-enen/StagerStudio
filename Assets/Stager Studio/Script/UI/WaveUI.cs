namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class WaveUI : Image {


		// Const
		private const int SAMPLE_PER_WAVE = 64;

		// API
		public float Length01 {
			get => _Length;
			set {
				_Length = Mathf.Clamp01(value);
				SetVerticesDirty();
			}
		}

		public float Time01 {
			get => _Time;
			set {
				_Time = Mathf.Clamp01(value);
				SetVerticesDirty();
			}
		}


		// Data
		private readonly UIVertex[] VertexCache = new UIVertex[4] { default, default, default, default };
		private Coroutine AlphaCor = null;
		private float[] Waves = null;
		private float _Length = 1f / 600f;
		private float _Time = 0f;
		private float MaxWave = 0f;


		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {
			toFill.Clear();
			if (Waves == null || Waves.Length == 0 || MaxWave <= 0f || Length01 <= 0f) { return; }
			var rect = GetPixelAdjustedRect();
			int len = Waves.Length;
			VertexCache[0].color = color;
			VertexCache[1].color = color;
			VertexCache[2].color = color;
			VertexCache[3].color = color;
			float x, y;
			int fromIndex = (int)(Time01 * len);
			int toIndex = (int)((Time01 + Length01) * len);
			int maxToIndex = Mathf.Min(len, fromIndex + 6500);
			float prevX = GetX(0);
			float prevY = GetY(0);
			for (int i = fromIndex; i < toIndex && i < maxToIndex; i++) {
				x = GetX(i);
				y = GetY(i);
				VertexCache[0].position.x = prevX;
				VertexCache[1].position.x = x;
				VertexCache[2].position.x = 0;
				VertexCache[3].position.x = 0;
				VertexCache[0].position.y = prevY;
				VertexCache[1].position.y = y;
				VertexCache[2].position.y = y;
				VertexCache[3].position.y = prevY;
				prevX = x;
				prevY = y;
				toFill.AddUIVertexQuad(VertexCache);
			}
			VertexCache[0].position = new Vector3(prevX, prevY);
			VertexCache[1].position = new Vector3(0, rect.yMax);
			VertexCache[2].position = new Vector3(0, rect.yMax);
			VertexCache[3].position = new Vector3(0, prevY);
			toFill.AddUIVertexQuad(VertexCache);
			float GetX (int index) => -Waves[index] / MaxWave * rect.width;
			float GetY (int index) => Mathf.Lerp(rect.yMin, rect.yMax, (float)(index - fromIndex) / (toIndex - fromIndex));
		}


		// API
		public void LoadWave (AudioClip clip) {
			SetVerticesDirty();
			MaxWave = 0f;
			if (!clip || clip.samples * clip.channels < SAMPLE_PER_WAVE) { Waves = null; return; }
			var oWaves = new float[clip.samples * clip.channels];
			bool success = clip.GetData(oWaves, 0);
			if (!success) { Waves = null; return; }
			Waves = new float[oWaves.Length / SAMPLE_PER_WAVE];
			for (int i = 0; i < Waves.Length; i++) {
				float w = GetWave(i * SAMPLE_PER_WAVE);
				Waves[i] = w;
				MaxWave = Mathf.Max(MaxWave, w);
			}
			float GetWave (int startIndex) {
				if (startIndex >= oWaves.Length) { return 0; }
				float min = float.MaxValue;
				float max = float.MinValue;
				for (int i = startIndex; i < startIndex + SAMPLE_PER_WAVE && i < oWaves.Length; i++) {
					min = Mathf.Min(min, oWaves[i]);
					max = Mathf.Max(max, oWaves[i]);
				}
				return (max - min) * 0.5f;
			}
		}


		public void SetAlpha (float a) {
			const float NORMAL_ALPHA = 0.12f;
			if (AlphaCor != null) {
				StopCoroutine(AlphaCor);
			}
			if (!gameObject.activeSelf) {
				gameObject.SetActive(a > 0.01f);
			}
			if (gameObject.activeInHierarchy) {
				AlphaCor = StartCoroutine(Alphaing());
			} else {
				var c = color;
				c.a = Mathf.Lerp(0f, NORMAL_ALPHA, a);
				color = c;
				gameObject.SetActive(a > 0.01f);
			}
			// === Func ===
			IEnumerator Alphaing () {
				a = Mathf.Lerp(0f, NORMAL_ALPHA, a);
				var c = color;
				float from = c.a;
				if (a > 0.01f) {
					gameObject.SetActive(true);
				}
				for (float t = 0f; t < 0.4f; t += Time.deltaTime) {
					c.a = Mathf.Lerp(from, a, t / 0.4f);
					color = c;
					yield return new WaitForEndOfFrame();
				}
				c.a = a;
				color = c;
				gameObject.SetActive(a > 0.01f);
				AlphaCor = null;
			}
		}


	}
}