namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class GridUI : Image {


		// Ser
		[SerializeField] private int m_X = 8;
		[SerializeField] private int m_Y = 8;
		[SerializeField] private float m_Size = 2f;
		[SerializeField] private bool m_Soft = true;

		// Data
		private readonly static UIVertex[] VertexCache = { default, default, default, default, };


		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {
			toFill.Clear();
			var rect = GetPixelAdjustedRect();
			float halfSize = m_Size / 2f;
			if (m_Soft) {
				// Soft
				var clear = color;
				clear.a = 0f;
				VertexCache[0].color = VertexCache[1].color = clear;
				VertexCache[2].color = VertexCache[3].color = color;
				VertexCache[0].position.y = VertexCache[3].position.y = rect.yMin;
				VertexCache[1].position.y = VertexCache[2].position.y = rect.yMax;
				for (int i = 0; i <= m_X; i++) {
					VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) - halfSize;
					VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X);
					toFill.AddUIVertexQuad(VertexCache);
					VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) + halfSize;
					VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X);
					toFill.AddUIVertexQuad(VertexCache);
				}
				VertexCache[0].color = VertexCache[3].color = clear;
				VertexCache[1].color = VertexCache[2].color = color;
				VertexCache[0].position.x = VertexCache[1].position.x = rect.xMin;
				VertexCache[2].position.x = VertexCache[3].position.x = rect.xMax;
				for (int i = 0; i <= m_Y; i++) {
					VertexCache[0].position.y = VertexCache[3].position.y = Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) - halfSize;
					VertexCache[1].position.y = VertexCache[2].position.y = Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y);
					toFill.AddUIVertexQuad(VertexCache);
					VertexCache[0].position.y = VertexCache[3].position.y = Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) + halfSize;
					VertexCache[1].position.y = VertexCache[2].position.y = Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y);
					toFill.AddUIVertexQuad(VertexCache);
				}
			} else {
				// Hard
				VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = color;
				VertexCache[0].position.y = VertexCache[3].position.y = rect.yMin;
				VertexCache[1].position.y = VertexCache[2].position.y = rect.yMax;
				for (int i = 0; i <= m_X; i++) {
					VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) - halfSize;
					VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(rect.xMin, rect.xMax, (float)i / m_X) + halfSize;
					toFill.AddUIVertexQuad(VertexCache);
				}
				VertexCache[0].position.x = VertexCache[1].position.x = rect.xMin;
				VertexCache[2].position.x = VertexCache[3].position.x = rect.xMax;
				for (int i = 0; i <= m_Y; i++) {
					VertexCache[0].position.y = VertexCache[3].position.y = Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) - halfSize;
					VertexCache[1].position.y = VertexCache[2].position.y = Mathf.Lerp(rect.yMin, rect.yMax, (float)i / m_Y) + halfSize;
					toFill.AddUIVertexQuad(VertexCache);
				}
			}

		}


	}
}



#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEngine;
	using UnityEditor;
	using UI;
	[CustomEditor(typeof(GridUI))]
	public class GridUIInspector : Editor {
		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged","m_Sprite","m_Type","m_PreserveAspect",
			"m_FillCenter","m_FillMethod","m_FillAmount","m_FillClockwise","m_FillOrigin","m_UseSpriteMesh",
			"m_PixelsPerUnitMultiplier",
		};
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif