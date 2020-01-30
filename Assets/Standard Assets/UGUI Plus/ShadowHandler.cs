namespace UGUIPlus {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	[System.Serializable]
	public class ShadowHandler {



		// Cache
		private static UIVertex[] cache_Quads = new UIVertex[4];


		// Short Cut
		public bool UseShadow {
			get {
				return m_UseShadow;
			}

			set {
				m_UseShadow = value;
			}
		}

		public bool SharpCorner {
			get {
				return m_SharpCorner;
			}

			set {
				m_SharpCorner = value;
			}
		}

		public Color Color {
			get {
				return m_Color;
			}

			set {
				m_Color = value;
			}
		}

		public Color ClearColor {
			get {
				return m_ClearColor;
			}

			set {
				m_ClearColor = value;
			}
		}

		public float Left {
			get {
				return m_Left;
			}

			set {
				m_Left = value;
			}
		}

		public float Right {
			get {
				return m_Right;
			}

			set {
				m_Right = value;
			}
		}

		public float Top {
			get {
				return m_Top;
			}

			set {
				m_Top = value;
			}
		}

		public float Bottom {
			get {
				return m_Bottom;
			}

			set {
				m_Bottom = value;
			}
		}


		// Ser
		[SerializeField] private bool m_UseShadow = false;
		[SerializeField] private bool m_SharpCorner = false;
		[SerializeField] private float m_Left = 5f;
		[SerializeField] private float m_Right = 5f;
		[SerializeField] private float m_Top = 5f;
		[SerializeField] private float m_Bottom = 5f;
		[SerializeField] private Color m_Color = new Color(0, 0, 0, 0.5f);
		[SerializeField] private Color m_ClearColor = new Color(0, 0, 0, 0f);



		// API
		public void PopulateMesh (VertexHelper toFill, RectTransform rectTransform) {
			if (m_UseShadow) {
				Vector2 min = rectTransform.pivot;
				min.Scale(-rectTransform.rect.size);
				Vector2 max = rectTransform.rect.size + min;
				Vector2 minmin = min - new Vector2(m_Left, m_Bottom);
				Vector2 maxmax = max + new Vector2(m_Right, m_Top);
				// d
				SetQuadCacheColorUD(m_Color, m_ClearColor);
				SetQuadCachePosition(min.x, minmin.y, max.x, min.y);
				toFill.AddUIVertexQuad(cache_Quads);
				// u
				SetQuadCacheColorUD(m_ClearColor, m_Color);
				SetQuadCachePosition(min.x, max.y, max.x, maxmax.y);
				toFill.AddUIVertexQuad(cache_Quads);
				// l
				SetQuadCacheColorLR(m_ClearColor, m_Color);
				SetQuadCachePosition(minmin.x, min.y, min.x, max.y);
				toFill.AddUIVertexQuad(cache_Quads);
				// r
				SetQuadCacheColorLR(m_Color, m_ClearColor);
				SetQuadCachePosition(max.x, min.y, maxmax.x, max.y);
				toFill.AddUIVertexQuad(cache_Quads);
				// dl
				SetQuadCacheColorLR(m_ClearColor, m_ClearColor);
				cache_Quads[m_SharpCorner ? 2 : 1].color = m_Color;
				if (m_SharpCorner) {
					SetQuadCachePosition(minmin.x, minmin.y, min.x, min.y);
				} else {
					SetQuadCachePositionAlt(minmin.x, minmin.y, min.x, min.y);
				}
				toFill.AddUIVertexQuad(cache_Quads);
				// dr
				SetQuadCacheColorLR(m_ClearColor, m_ClearColor);
				cache_Quads[m_SharpCorner ? 0 : 1].color = m_Color;
				if (m_SharpCorner) {
					SetQuadCachePositionAlt(max.x, minmin.y, maxmax.x, min.y);
				} else {
					SetQuadCachePosition(max.x, minmin.y, maxmax.x, min.y);
				}
				toFill.AddUIVertexQuad(cache_Quads);
				// ul
				SetQuadCacheColorLR(m_ClearColor, m_ClearColor);
				cache_Quads[m_SharpCorner ? 2 : 3].color = m_Color;
				if (m_SharpCorner) {
					SetQuadCachePositionAlt(minmin.x, max.y, min.x, maxmax.y);
				} else {
					SetQuadCachePosition(minmin.x, max.y, min.x, maxmax.y);
				}
				toFill.AddUIVertexQuad(cache_Quads);
				// ur
				SetQuadCacheColorLR(m_ClearColor, m_ClearColor);
				cache_Quads[m_SharpCorner ? 0 : 3].color = m_Color;
				if (m_SharpCorner) {
					SetQuadCachePosition(max.x, max.y, maxmax.x, maxmax.y);
				} else {
					SetQuadCachePositionAlt(max.x, max.y, maxmax.x, maxmax.y);
				}
				toFill.AddUIVertexQuad(cache_Quads);
			}
		}




		// LGC
		private void SetQuadCachePosition (float minX, float minY, float maxX, float maxY) {
			cache_Quads[0].position = new Vector2(minX, minY);
			cache_Quads[3].position = new Vector2(maxX, minY);
			cache_Quads[1].position = new Vector2(minX, maxY);
			cache_Quads[2].position = new Vector2(maxX, maxY);
			cache_Quads[0].uv0 = Vector2.zero;
			cache_Quads[3].uv0 = Vector2.right;
			cache_Quads[1].uv0 = Vector2.up;
			cache_Quads[2].uv0 = Vector2.one;
		}


		private void SetQuadCachePositionAlt (float minX, float minY, float maxX, float maxY) {
			cache_Quads[3].position = new Vector2(minX, minY);
			cache_Quads[2].position = new Vector2(maxX, minY);
			cache_Quads[0].position = new Vector2(minX, maxY);
			cache_Quads[1].position = new Vector2(maxX, maxY);
			cache_Quads[3].uv0 = Vector2.zero;
			cache_Quads[2].uv0 = Vector2.right;
			cache_Quads[0].uv0 = Vector2.up;
			cache_Quads[1].uv0 = Vector2.one;
		}
		

		private void SetQuadCacheColorLR (Color l, Color r) {
			cache_Quads[0].color = l;
			cache_Quads[3].color = r;
			cache_Quads[1].color = l;
			cache_Quads[2].color = r;
		}


		private void SetQuadCacheColorUD (Color u, Color d) {
			cache_Quads[0].color = d;
			cache_Quads[3].color = d;
			cache_Quads[1].color = u;
			cache_Quads[2].color = u;
		}




	}
}
