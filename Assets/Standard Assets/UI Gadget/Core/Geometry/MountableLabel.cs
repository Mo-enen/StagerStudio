namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	[AddComponentMenu("UIGadget/MountableLabel")]
	public class MountableLabel : Text {



		// Ser
		[SerializeField] private RectTransform m_MountLeft = null;
		[SerializeField] private RectTransform m_MountRight = null;
		[SerializeField] private RectTransform m_MountDown = null;
		[SerializeField] private RectTransform m_MountUp = null;

		// Short
		private bool HasMount => m_MountLeft || m_MountRight || m_MountDown || m_MountUp;

		// Data
		private Vector4? MountOffset = null;


		// MSG
		private void Update () {
			if (MountOffset.HasValue) {
				// L
				if (m_MountLeft) {
					var pos = m_MountLeft.anchoredPosition;
					pos.x = MountOffset.Value.x;
					m_MountLeft.anchoredPosition = pos;
				}
				// R
				if (m_MountRight) {
					var pos = m_MountRight.anchoredPosition;
					pos.x = MountOffset.Value.z;
					m_MountRight.anchoredPosition = pos;
				}

				// D
				if (m_MountDown) {
					var pos = m_MountDown.anchoredPosition;
					pos.y = MountOffset.Value.y;
					m_MountDown.anchoredPosition = pos;
				}
				// U
				if (m_MountUp) {
					var pos = m_MountUp.anchoredPosition;
					pos.y = MountOffset.Value.w;
					m_MountUp.anchoredPosition = pos;
				}

				MountOffset = null;
			}
		}


		protected override void OnPopulateMesh (VertexHelper toFill) {
			base.OnPopulateMesh(toFill);
			int count = toFill.currentVertCount;
			if (!HasMount || count == 0) { return; }
			UIVertex v = default;
			var rect = GetPixelAdjustedRect();
			Vector4 result = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			for (int i = 0; i < count; i++) {
				toFill.PopulateUIVertex(ref v, i);
				var pos = v.position;
				result.x = Mathf.Min(result.x, pos.x - rect.x);
				result.y = Mathf.Min(result.y, pos.y - rect.y);
				result.z = Mathf.Max(result.z, pos.x - rect.x - rect.width);
				result.w = Mathf.Max(result.w, pos.y - rect.y - rect.height);
			}
			MountOffset = result;
		}





	}
}