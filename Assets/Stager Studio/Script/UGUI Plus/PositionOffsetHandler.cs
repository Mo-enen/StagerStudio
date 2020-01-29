namespace StagerStudio.UGUIPlus {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	[System.Serializable]
	public class PositionOffsetHandler {


		// Short Cut
		public bool UsePositionOffset {
			get {
				return m_UsePositionOffset;
			}

			set {
				m_UsePositionOffset = value;
			}
		}

		public Vector2 TopLeft {
			get {
				return m_TopLeft;
			}

			set {
				m_TopLeft = value;
			}
		}

		public Vector2 TopRight {
			get {
				return m_TopRight;
			}

			set {
				m_TopRight = value;
			}
		}

		public Vector2 BottomLeft {
			get {
				return m_BottomLeft;
			}

			set {
				m_BottomLeft = value;
			}
		}

		public Vector2 BottomRight {
			get {
				return m_BottomRight;
			}

			set {
				m_BottomRight = value;
			}
		}


		// Ser
		[SerializeField] private bool m_UsePositionOffset = false;
		[SerializeField] private Vector2 m_TopLeft = Vector2.zero;
		[SerializeField] private Vector2 m_TopRight = Vector2.zero;
		[SerializeField] private Vector2 m_BottomLeft = Vector2.zero;
		[SerializeField] private Vector2 m_BottomRight = Vector2.zero;






		public void PopulateMesh (VertexHelper toFill, RectTransform rectTransform) {
			if (UsePositionOffset) {
				Vector2 min = rectTransform.pivot;
				min.Scale(-rectTransform.rect.size);
				Vector2 max = rectTransform.rect.size + min;
				int len = toFill.currentVertCount;
				for (int i = 0; i < len; i++) {
					UIVertex v = new UIVertex();
					toFill.PopulateUIVertex(ref v, i);
					v.position += RemapPositionOffset(min, max, v.position);
					toFill.SetUIVertex(v, i);
				}
			}
		}




		private Vector3 RemapPositionOffset (Vector2 min, Vector2 max, Vector2 pos) {
			float x01 = max.x == min.x ? 0f : ((pos.x - min.x) / (max.x - min.x));
			float y01 = max.y == min.y ? 0f : ((pos.y - min.y) / (max.y - min.y));
			return Vector3.Lerp(
				Vector3.Lerp(BottomLeft, BottomRight, x01),
				Vector3.Lerp(TopLeft, TopRight, x01),
				y01
			);
		}



	}
}
