namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;



	public class DottedRingUI : Image {



		// Ser
		[SerializeField] private float m_Size = 2f;
		[SerializeField] private float m_DotDensity = 0.1f;


		// Data
		private readonly static UIVertex[] CacheVertex = {
			new UIVertex(){ position = Vector3.zero, },
			new UIVertex(){ position = Vector3.zero, },
			new UIVertex(){ position = Vector3.zero, },
			new UIVertex(){ position = Vector3.zero, },
		};


		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {
			toFill.Clear();
			CacheVertex[0].color = color;
			CacheVertex[1].color = color;
			CacheVertex[2].color = color;
			CacheVertex[3].color = color;
			var rect = GetPixelAdjustedRect();
			float rectL = rect.xMin;
			float rectR = rect.xMax;
			float rectD = rect.yMin;
			float rectU = rect.yMax;
			AddQuad(rectL, rectL + m_Size, rectD, rectU, true);
			AddQuad(rectR - m_Size, rectR, rectD, rectU, true);
			AddQuad(rectL + m_Size, rectR - m_Size, rectD, rectD + m_Size, false);
			AddQuad(rectL + m_Size, rectR - m_Size, rectU - m_Size, rectU, false);
			// Func
			void AddQuad (float l, float r, float d, float u, bool uvVerticle) {
				int uvA = uvVerticle ? 0 : 1;
				int uvB = 1 - uvA;
				// Pos
				CacheVertex[0].position.x = l;
				CacheVertex[0].position.y = d;
				CacheVertex[1].position.x = l;
				CacheVertex[1].position.y = u;
				CacheVertex[2].position.x = r;
				CacheVertex[2].position.y = u;
				CacheVertex[3].position.x = r;
				CacheVertex[3].position.y = d;
				// UV
				CacheVertex[0].uv0[uvA] = 0;
				CacheVertex[0].uv0[uvB] = 0;
				CacheVertex[1].uv0[uvA] = 0;
				CacheVertex[1].uv0[uvB] = (u - d) * m_DotDensity;
				CacheVertex[2].uv0[uvA] = (r - l) * m_DotDensity;
				CacheVertex[2].uv0[uvB] = (u - d) * m_DotDensity;
				CacheVertex[3].uv0[uvA] = (r - l) * m_DotDensity;
				CacheVertex[3].uv0[uvB] = 0;
				// Final
				toFill.AddUIVertexQuad(CacheVertex);
			}
		}


		// API
		public void Refresh () => SetVerticesDirty();


	}
}



#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using UI;



	[CustomEditor(typeof(DottedRingUI))]
	public class DottedRingUIInspector : Editor {
		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged","m_Type","m_PreserveAspect",
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