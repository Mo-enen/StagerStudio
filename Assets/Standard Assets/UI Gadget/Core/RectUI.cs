namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Sprites;
	using UnityEngine.UI;


	public class RectUI : Image {




		#region --- VAR ---


		// Ser
		[SerializeField] private bool m_Sliced = false;
		[SerializeField] private List<Rect> m_Rects = new List<Rect>();

		// Data
		private static readonly UIVertex[] VertexCache = { UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert, };
		private static readonly Vector2[] s_VertScratch = new Vector2[4];
		private static readonly Vector2[] s_UVScratch = new Vector2[4];


		#endregion




		#region --- MSG ---


		protected override void OnPopulateMesh (VertexHelper toFill) {
			if (sprite != null && m_Sliced) {
				PopulateSlicedMesh(toFill);
			} else {
				PopulateSimpleMesh(toFill);
			}
		}


		private void PopulateSimpleMesh (VertexHelper toFill) {
			toFill.Clear();
			if (color.a < 0.01f || m_Rects.Count == 0) { return; }
			var pRect = GetPixelAdjustedRect();
			VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = color;
			VertexCache[0].uv0 = sprite.uv[2];
			VertexCache[1].uv0 = sprite.uv[0];
			VertexCache[2].uv0 = sprite.uv[1];
			VertexCache[3].uv0 = sprite.uv[3];
			foreach (var r in m_Rects) {
				VertexCache[0].position.x = Mathf.LerpUnclamped(pRect.xMin, pRect.xMax, r.xMin);
				VertexCache[0].position.y = Mathf.LerpUnclamped(pRect.yMin, pRect.yMax, r.yMin);
				VertexCache[1].position.x = Mathf.LerpUnclamped(pRect.xMin, pRect.xMax, r.xMin);
				VertexCache[1].position.y = Mathf.LerpUnclamped(pRect.yMin, pRect.yMax, r.yMax);
				VertexCache[2].position.x = Mathf.LerpUnclamped(pRect.xMin, pRect.xMax, r.xMax);
				VertexCache[2].position.y = Mathf.LerpUnclamped(pRect.yMin, pRect.yMax, r.yMax);
				VertexCache[3].position.x = Mathf.LerpUnclamped(pRect.xMin, pRect.xMax, r.xMax);
				VertexCache[3].position.y = Mathf.LerpUnclamped(pRect.yMin, pRect.yMax, r.yMin);
				toFill.AddUIVertexQuad(VertexCache);
			}
		}


		private void PopulateSlicedMesh (VertexHelper toFill) {
			toFill.Clear();
			if (color.a < 0.01f || m_Rects.Count == 0) { return; }
			var pRect = GetPixelAdjustedRect();
			foreach (var r in m_Rects) {
				GenerateSlicedSprite(toFill, pRect,
					new Rect(
						Mathf.LerpUnclamped(pRect.xMin, pRect.xMax, r.x),
						Mathf.LerpUnclamped(pRect.yMin, pRect.yMax, r.y),
						pRect.width * Mathf.Clamp(r.width, 0.18f, 1f),
						pRect.height * Mathf.Clamp(r.height, 0.18f, 1f)
					)
				);
			}
		}


		#endregion




		#region --- API ---


		public void Add (Rect rect) {
			m_Rects.Add(rect);
			SetVerticesDirty();
		}


		public void Clear () {
			m_Rects.Clear();
			SetVerticesDirty();
		}


		#endregion




		#region --- LGC ---


		private Vector4 GetAdjustedBorders (Vector4 border, Rect adjustedRect) {
			Rect originalRect = rectTransform.rect;
			for (int axis = 0; axis <= 1; axis++) {
				float borderScaleRatio;
				if (originalRect.size[axis] != 0) {
					borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
					border[axis] *= borderScaleRatio;
					border[axis + 2] *= borderScaleRatio;
				}
				float combinedBorders = border[axis] + border[axis + 2];
				if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0) {
					borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
					border[axis] *= borderScaleRatio;
					border[axis + 2] *= borderScaleRatio;
				}
			}
			return border;
		}



		private void GenerateSlicedSprite (VertexHelper toFill, Rect pRect, Rect rect) {

			Vector4 outer = DataUtility.GetOuterUV(sprite);
			Vector4 inner = DataUtility.GetInnerUV(sprite);
			Vector4 padding = DataUtility.GetPadding(sprite);
			Vector4 adjustedBorders = GetAdjustedBorders(sprite.border / multipliedPixelsPerUnit, pRect);
			
			padding /= multipliedPixelsPerUnit;

			s_VertScratch[0] = new Vector2(padding.x, padding.y);
			s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

			s_VertScratch[1].x = adjustedBorders.x;
			s_VertScratch[1].y = adjustedBorders.y;

			s_VertScratch[2].x = rect.width - adjustedBorders.z;
			s_VertScratch[2].y = rect.height - adjustedBorders.w;

			for (int i = 0; i < 4; ++i) {
				s_VertScratch[i].x += rect.x;
				s_VertScratch[i].y += rect.y;
			}


			s_UVScratch[0] = new Vector2(outer.x, outer.y);
			s_UVScratch[1] = new Vector2(inner.x, inner.y);
			s_UVScratch[2] = new Vector2(inner.z, inner.w);
			s_UVScratch[3] = new Vector2(outer.z, outer.w);


			for (int x = 0; x < 3; ++x) {
				int x2 = x + 1;
				for (int y = 0; y < 3; ++y) {
					int y2 = y + 1;
					AddQuad(
						toFill,
						new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
						new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
						color,
						new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
						new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y)
					);
				}
			}
			// Func
			void AddQuad (VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax) {
				if (posMin.x >= posMax.x || posMin.y >= posMax.y) { return; }
				int startIndex = vertexHelper.currentVertCount;
				vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
				vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
				vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
				vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));
				vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
				vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
			}
		}



		#endregion




	}
}



#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEngine;
	using UnityEditor;
	using UIGadget;
	[CustomEditor(typeof(RectUI))]
	public class RectUIInspector : Editor {
		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged","m_Type","m_PreserveAspect",
			"m_FillMethod","m_FillAmount","m_FillClockwise","m_FillOrigin","m_UseSpriteMesh",
		};
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				(target as RectUI).SetVerticesDirty();
			}
		}
	}
}
#endif