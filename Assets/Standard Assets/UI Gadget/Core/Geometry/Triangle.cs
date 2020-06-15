namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;



	[AddComponentMenu("UIGadget/Triangle")]
	public class Triangle : Image {




		// Ser
		[Header("Triangle")]
		[SerializeField] private Vector2 m_A = new Vector2(0f, 0f);
		[SerializeField] private Vector2 m_B = new Vector2(1f, 0f);
		[SerializeField] private Vector2 m_C = new Vector2(0.5f, 1f);
		[SerializeField] private int m_Detail = 48;
		[SerializeField] private float m_Round = 8;
		[Header("Border"), SerializeField] private float m_Border = 4f;
		[SerializeField] private Color m_BorderColor = Color.black;



		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {
			toFill.Clear();

			var rect = GetPixelAdjustedRect();
			if (color.a < GadgetUtil.FLOAT_GAP || rect.width <= GadgetUtil.FLOAT_GAP || rect.height <= GadgetUtil.FLOAT_GAP) { return; }

			float border = Mathf.Max(m_Border, 0f);
			float round = Mathf.Max(m_Round, 0f);
			int detail = Mathf.Clamp(m_Detail, 3, 1024);
			Vector2 a = new Vector2(
				Mathf.LerpUnclamped(rect.xMin, rect.xMax, m_A.x),
				Mathf.LerpUnclamped(rect.yMin, rect.yMax, m_A.y)
			);
			Vector2 b = new Vector2(
				Mathf.LerpUnclamped(rect.xMin, rect.xMax, m_B.x),
				Mathf.LerpUnclamped(rect.yMin, rect.yMax, m_B.y)
			);
			Vector2 c = new Vector2(
				Mathf.LerpUnclamped(rect.xMin, rect.xMax, m_C.x),
				Mathf.LerpUnclamped(rect.yMin, rect.yMax, m_C.y)
			);

			if (Mathf.Repeat(Quaternion.FromToRotation(b - a, c - a).eulerAngles.z, 360f) > 180f) {
				var temp = b;
				b = c;
				c = temp;
			}

			Vector2 deltaAB = Vector3.Cross(a - b, Vector3.back).normalized;
			Vector2 deltaBC = Vector3.Cross(b - c, Vector3.back).normalized;
			Vector2 deltaCA = Vector3.Cross(c - a, Vector3.back).normalized;
			float rotAB = Mathf.Repeat(Quaternion.FromToRotation(deltaAB, Vector2.up).eulerAngles.z, 360f);
			float rotBC = Mathf.Repeat(Quaternion.FromToRotation(deltaBC, Vector2.up).eulerAngles.z, 360f);
			float rotCA = Mathf.Repeat(Quaternion.FromToRotation(deltaCA, Vector2.up).eulerAngles.z, 360f);

			// Fill
			if (color.a > GadgetUtil.FLOAT_GAP) {
				GadgetUtil.SetCacheColor(color);
				GadgetUtil.FillTriangle(toFill, a, b, c);
				if (round > GadgetUtil.FLOAT_GAP) {
					GadgetUtil.FillQuad(toFill, a, b, b + deltaAB * round, a + deltaAB * round);
					GadgetUtil.FillQuad(toFill, b, c, c + deltaBC * round, b + deltaBC * round);
					GadgetUtil.FillQuad(toFill, c, a, a + deltaCA * round, c + deltaCA * round);
					GadgetUtil.FillDisc(toFill,
						rotAB < rotCA ? rotAB : rotAB - 360f,
						rotCA,
						detail, round, a
					);
					GadgetUtil.FillDisc(toFill,
						rotBC < rotAB ? rotBC : rotBC - 360f,
						rotAB,
						detail, round, b
					);
					GadgetUtil.FillDisc(toFill,
						rotCA < rotBC ? rotCA : rotCA - 360f,
						rotBC,
						detail, round, c
					);
				}
			}

			// Border
			if (m_BorderColor.a > GadgetUtil.FLOAT_GAP && border > GadgetUtil.FLOAT_GAP) {
				GadgetUtil.SetCacheColor(m_BorderColor);
				GadgetUtil.FillQuad(toFill,
					a + deltaAB * round,
					b + deltaAB * round,
					b + deltaAB * (round + border),
					a + deltaAB * (round + border)
				);
				GadgetUtil.FillQuad(toFill,
					b + deltaBC * round,
					c + deltaBC * round,
					c + deltaBC * (round + border),
					b + deltaBC * (round + border)
				);
				GadgetUtil.FillQuad(toFill,
					c + deltaCA * round,
					a + deltaCA * round,
					a + deltaCA * (round + border),
					c + deltaCA * (round + border)
				);
				GadgetUtil.FillDisc(toFill,
						rotAB < rotCA ? rotAB : rotAB - 360f,
						rotCA,
						detail, round, round + border, a
					);
				GadgetUtil.FillDisc(toFill,
					rotBC < rotAB ? rotBC : rotBC - 360f,
					rotAB,
					detail, round, round + border, b
				);
				GadgetUtil.FillDisc(toFill,
					rotCA < rotBC ? rotCA : rotCA - 360f,
					rotBC,
					detail, round, round + border, c
				);
			}





		}



	}
}