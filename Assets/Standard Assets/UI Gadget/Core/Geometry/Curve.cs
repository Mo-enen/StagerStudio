namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	[AddComponentMenu("UIGadget/Curve")]
	public class Curve : Image {



		// API
		public AnimationCurve CurveData {
			get => m_Curve;
			set {
				m_Curve = value;
				SetVerticesDirty();
			}
		}
		public Color DotColor {
			get => m_DotColor;
			set {
				if (m_DotColor != value) {
					m_DotColor = value;
					SetVerticesDirty();
				}
			}
		}
		public Color LineColor {
			get => m_LineColor;
			set {
				if (m_LineColor != value) {
					m_LineColor = value;
					SetVerticesDirty();
				}
			}
		}
		public int LineDetail {
			get => m_LineDetail;
			set {
				if (m_LineDetail != value) {
					m_LineDetail = value;
					SetVerticesDirty();
				}
			}
		}
		public int CornerDetail {
			get => m_CornerDetail;
			set {
				if (m_CornerDetail != value) {
					m_CornerDetail = value;
					SetVerticesDirty();
				}
			}
		}
		public float CurveSize {
			get => m_CurveSize;
			set {
				if (m_CurveSize != value) {
					m_CurveSize = value;
					SetVerticesDirty();
				}
			}
		}
		public float DotSize {
			get => m_DotSize;
			set {
				if (m_DotSize != value) {
					m_DotSize = value;
					SetVerticesDirty();
				}
			}
		}

		// Ser
		[SerializeField] private AnimationCurve m_Curve = null;
		[SerializeField] private Color m_DotColor = Color.red;
		[SerializeField] private Color m_LineColor = Color.white;
		[SerializeField] private int m_LineDetail = 16;
		[SerializeField] private int m_CornerDetail = 16;
		[SerializeField] private float m_CurveSize = 4f;
		[SerializeField] private float m_DotSize = 8f;



		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {

			toFill.Clear();
			if (m_Curve is null || m_Curve.length == 0) { return; }

			var rect = GetPixelAdjustedRect();
			int length = m_Curve.length;
			float timeL = m_Curve[0].time;
			float timeR = m_Curve[length - 1].time;
			float valueD = float.MaxValue;
			float valueU = float.MinValue;
			int lineDetail = Mathf.Clamp(m_LineDetail, 0, 1024);
			int cornerDetail = Mathf.Clamp(m_CornerDetail, 0, 1024);
			float curveSize = Mathf.Max(m_CurveSize, 0f);

			foreach (var key in m_Curve.keys) {
				valueD = Mathf.Min(valueD, key.value);
				valueU = Mathf.Max(valueU, key.value);
			}

			// Line
			if (lineDetail > 0 && m_LineColor.a > GadgetUtil.FLOAT_GAP && curveSize > GadgetUtil.FLOAT_GAP) {
				var keyL = m_Curve[0];
				float prevAngle = 0f;
				GadgetUtil.SetCacheColor(color * m_LineColor);
				float xl = rect.xMin;
				float vl = RemapUnclamped(valueD, valueU, rect.yMin, rect.yMax, m_Curve[0].value);
				for (int index = 0; index < length - 1; index++) {
					var keyR = m_Curve[index + 1];
					float l = RemapUnclamped(timeL, timeR, rect.xMin, rect.xMax, keyL.time);
					float r = RemapUnclamped(timeL, timeR, rect.xMin, rect.xMax, keyR.time);
					for (int i = 0; i < lineDetail; i++) {
						float step01 = (float)(i + 1) / lineDetail;
						float xr = Mathf.Lerp(l, r, step01);
						float vr = RemapUnclamped(
							valueD, valueU, rect.yMin, rect.yMax,
							m_Curve.Evaluate(Mathf.Lerp(keyL.time, keyR.time, step01))
						);
						var a = new Vector3(xl, vl, 0f);
						var b = new Vector3(xr, vr, 0f);
						// Line
						GadgetUtil.FillLine(toFill, a, b, curveSize);
						if (cornerDetail > 0) {
							float angle = Vector2.SignedAngle(b - a, Vector2.right);
							if (index != 0 || i != 0) {
								// Disc
								GadgetUtil.FillDisc(
									toFill,
									prevAngle, angle,
									Mathf.CeilToInt(Mathf.Abs(angle - prevAngle) / (360f / cornerDetail)),
									curveSize / 2f, a
								);
								GadgetUtil.FillDisc(
									toFill,
									prevAngle + 180f, angle + 180f,
									Mathf.CeilToInt(Mathf.Abs(angle - prevAngle) / (360f / cornerDetail)),
									curveSize / 2f, a
								);
							}
							prevAngle = angle;
						}
						vl = vr;
						xl = xr;
					}
					keyL = keyR;
				}
			}

			// Dots
			if (m_DotSize > GadgetUtil.FLOAT_GAP && m_DotColor.a > GadgetUtil.FLOAT_GAP) {
				GadgetUtil.SetCacheColor(color * m_DotColor);
				float dotSize = m_DotSize / 2f;
				foreach (var key in m_Curve.keys) {
					float x = RemapUnclamped(timeL, timeR, rect.xMin, rect.xMax, key.time);
					float y = RemapUnclamped(valueD, valueU, rect.yMin, rect.yMax, key.value);
					GadgetUtil.FillQuad(toFill, x - dotSize, x + dotSize, y - dotSize, y + dotSize);
				}
			}
		}



		// UTL
		private float RemapUnclamped (float l, float r, float newL, float newR, float t) {
			return l == r ? l : Mathf.LerpUnclamped(
				newL, newR,
				(t - l) / (r - l)
			);
		}


	}
}
