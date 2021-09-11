namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	[AddComponentMenu("UIGadget/Circle")]
	public class Circle : Image {



		// SUB
		public enum AdaptMode {
			FitIn = 0,
			Envelope = 1,
		}


		// Api
		public AdaptMode Adapt {
			get => m_Adapt;
			set {
				if (m_Adapt != value) {
					m_Adapt = value;
					SetVerticesDirty();
				}
			}
		}
		public int Detail {
			get => m_Detail;
			set {
				if (m_Detail != value) {
					m_Detail = value;
					SetVerticesDirty();
				}
			}
		}
		public float Border {
			get => m_Border;
			set {
				if (m_Border != value) {
					m_Border = value;
					SetVerticesDirty();
				}
			}
		}
		public float BorderOffset {
			get => m_BorderOffset;
			set {
				if (m_BorderOffset != value) {
					m_BorderOffset = value;
					SetVerticesDirty();
				}
			}
		}
		public Color FillColor {
			get => m_FillColor;
			set {
				if (m_FillColor != value) {
					m_FillColor = value;
					SetVerticesDirty();
				}
			}
		}
		public Color BorderColor {
			get => m_BorderColor;
			set {
				if (m_BorderColor != value) {
					m_BorderColor = value;
					SetVerticesDirty();
				}
			}
		}
		public float StartAngle {
			get => m_StartAngle;
			set {
				if (m_StartAngle != value) {
					m_StartAngle = value;
					SetVerticesDirty();
				}
			}
		}
		public float EndAngle {
			get => m_EndAngle;
			set {
				if (m_EndAngle != value) {
					m_EndAngle = value;
					SetVerticesDirty();
				}
			}
		}
		public float RingOffset {
			get => m_RingOffset;
			set {
				if (m_RingOffset != value) {
					m_RingOffset = value;
					SetVerticesDirty();
				}
			}
		}

		// Ser
		[Header("Circle"), SerializeField] private AdaptMode m_Adapt = AdaptMode.FitIn;
		[SerializeField] private int m_Detail = 48;
		[SerializeField] private float m_StartAngle = 0f;
		[SerializeField] private float m_EndAngle = 360f;
		[Range(0f, 1f), SerializeField] private float m_RingOffset = 0f;
		[SerializeField] private Color m_FillColor = Color.white;
		[Header("Border"), SerializeField] private float m_Border = 4f;
		[Range(0f, 1f), SerializeField] private float m_BorderOffset = 0.5f;
		[SerializeField] private Color m_BorderColor = Color.black;



		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {

			toFill.Clear();
			var rect = GetPixelAdjustedRect();
			if (color.a < GadgetUtil.FLOAT_GAP || rect.width <= GadgetUtil.FLOAT_GAP || rect.height <= GadgetUtil.FLOAT_GAP) { return; }
			float radius = m_Adapt == AdaptMode.FitIn ? Mathf.Min(rect.width, rect.height) / 2f : Mathf.Max(rect.width, rect.height) / 2f;
			float border = Mathf.Max(m_Border, 0f);
			float borderOffset = Mathf.Clamp01(border > GadgetUtil.FLOAT_GAP ?
				Mathf.Max(m_BorderOffset, 1f - rect.height / border / 2f, 1f - rect.width / border / 2f) :
				m_BorderOffset
			);
			int detail = Mathf.Clamp(m_Detail, 3, 1024);
			float borderGap = (1f - borderOffset) * border;
			Vector3 center = Vector3.Lerp(rect.min, rect.max, 0.5f);
			float ringOffset = Mathf.Clamp01(m_RingOffset);

			// Fill
			if (m_FillColor.a > GadgetUtil.FLOAT_GAP && borderGap < radius) {
				GadgetUtil.SetCacheColor(m_FillColor * color);
				GadgetUtil.FillDisc(toFill, m_StartAngle, m_EndAngle, detail, (radius - borderGap) * ringOffset, radius - borderGap, center);
			}

			// Border
			if (m_BorderColor.a > GadgetUtil.FLOAT_GAP && border > GadgetUtil.FLOAT_GAP) {
				GadgetUtil.SetCacheColor(m_BorderColor * color);
				GadgetUtil.FillDisc(toFill, m_StartAngle, m_EndAngle, detail, radius - borderGap, radius - borderGap + border, center);
			}

		}




	}
}