namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class VectorGrid : Image {


		// Ser
		[SerializeField] private int m_X = 8;
		[SerializeField] private int m_Y = 8;
		[SerializeField] private float m_Size = 2f;
		[SerializeField] private bool m_Soft = true;
		[SerializeField] private bool m_Corner = true;


		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {
			if (m_Soft) {
				PopulateSoftMesh(toFill);
			} else {
				PopulateHardMesh(toFill);
			}
		}


		private void PopulateSoftMesh (VertexHelper toFill) {

			toFill.Clear();
			var rect = GetPixelAdjustedRect();
			float halfSize = m_Size / 2f;
			var clear = color;
			clear.a = 0f;
			GadgetUtil.SetCacheColor(clear, clear, color, color);

			// H
			for (int i = 0; i <= m_X; i++) {
				GadgetUtil.FillQuad(
					toFill,
					Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) - halfSize,
					Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X),
					rect.yMin, rect.yMax
				);
				GadgetUtil.FillQuad(
					toFill,
					Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) + halfSize,
					Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X),
					rect.yMin, rect.yMax
				);
			}

			// V
			GadgetUtil.SetCacheColor(clear, color, color, clear);
			for (int i = 0; i <= m_Y; i++) {
				GadgetUtil.FillQuad(
					toFill,
					rect.xMin, rect.xMax,
					Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) - halfSize,
					Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y)
				);
				GadgetUtil.FillQuad(
					toFill,
					rect.xMin, rect.xMax,
					Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) + halfSize,
					Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y)
				);
			}

			// Corner
			if (m_Corner) {
				GadgetUtil.SetCacheColor(color, clear, clear, default);
				// UL
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMin, rect.yMax, 0f),
					new Vector3(rect.xMin - halfSize, rect.yMax, 0f),
					new Vector3(rect.xMin, rect.yMax + halfSize, 0f)
				);
				// UR
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMax, rect.yMax, 0f),
					new Vector3(rect.xMax, rect.yMax + halfSize, 0f),
					new Vector3(rect.xMax + halfSize, rect.yMax, 0f)
				);
				// DL
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMin, rect.yMin, 0f),
					new Vector3(rect.xMin, rect.yMin - halfSize, 0f),
					new Vector3(rect.xMin - halfSize, rect.yMin, 0f)
				);
				// DR
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMax, rect.yMin, 0f),
					new Vector3(rect.xMax + halfSize, rect.yMin, 0f),
					new Vector3(rect.xMax, rect.yMin - halfSize, 0f)
				);
			}
		}


		private void PopulateHardMesh (VertexHelper toFill) {

			toFill.Clear();
			var rect = GetPixelAdjustedRect();
			float halfSize = m_Size / 2f;
			GadgetUtil.SetCacheColor(color);

			// H
			for (int i = 0; i <= m_X; i++) {
				GadgetUtil.FillQuad(
					toFill,
					Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) - halfSize,
					Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) + halfSize,
					rect.yMin, rect.yMax
				);
			}

			// V
			for (int i = 0; i <= m_Y; i++) {
				GadgetUtil.FillQuad(
					toFill, rect.xMin, rect.xMax,
					Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) - halfSize,
					Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) + halfSize
				);
			}

			// Corner
			if (m_Corner) {
				GadgetUtil.SetCacheColor(color);
				// UL
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMin, rect.yMax, 0f),
					new Vector3(rect.xMin - halfSize, rect.yMax, 0f),
					new Vector3(rect.xMin, rect.yMax + halfSize, 0f)
				);
				// UR
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMax, rect.yMax, 0f),
					new Vector3(rect.xMax, rect.yMax + halfSize, 0f),
					new Vector3(rect.xMax + halfSize, rect.yMax, 0f)
				);
				// DL
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMin, rect.yMin, 0f),
					new Vector3(rect.xMin, rect.yMin - halfSize, 0f),
					new Vector3(rect.xMin - halfSize, rect.yMin, 0f)
				);
				// DR
				GadgetUtil.FillTriangle(
					toFill,
					new Vector3(rect.xMax, rect.yMin, 0f),
					new Vector3(rect.xMax + halfSize, rect.yMin, 0f),
					new Vector3(rect.xMax, rect.yMin - halfSize, 0f)
				);
			}
		}


	}
}
