namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	[RequireComponent(typeof(LineRenderer))]
	public class CurveUI : Image {


		// API
		public AnimationCurve Curve {
			get => m_Curve;
			set {
				m_Curve = value;
				SetVerticesDirty();
			}
		}

		// Short
		private LineRenderer Renderer => _Renderer != null ? _Renderer : (_Renderer = GetComponent<LineRenderer>());

		// Ser
		[SerializeField] private AnimationCurve m_Curve = null;
		[SerializeField] private Color m_DotColor = Color.white;
		[SerializeField] private int m_Step = 24;
		[SerializeField] private float m_DotSize = 4f;

		// Data
		private readonly static UIVertex[] CacheVertex = { default, default, default, default, };
		private LineRenderer _Renderer = null;
		private Mesh CacheMesh = null;

		// MSG
		protected override void Awake () {
			base.Awake();
			Renderer.enabled = false;
		}


		protected override void OnPopulateMesh (VertexHelper toFill) {
			toFill.Clear();
			if (m_Curve is null || m_Curve.length == 0) { return; }
			CacheVertex[0].position.z = CacheVertex[1].position.z = CacheVertex[2].position.z = CacheVertex[3].position.z = -1f;
			var size = rectTransform.rect.size;
			Vector3 pivotOffset = -size * rectTransform.pivot;

			// Curve
			if (CacheMesh is null) {
				CacheMesh = new Mesh();
				CacheMesh.MarkDynamic();
			}
			RefreshRenderer();
			if (Renderer != null) {
				Renderer.BakeMesh(CacheMesh);
				Renderer.enabled = false;
			}
			var uv = Vector2.zero;
			foreach (var vert in CacheMesh.vertices) {
				toFill.AddVert(vert + pivotOffset, color, uv);
			}
			var tris = CacheMesh.triangles;
			for (int i = 0; i < tris.Length; i += 3) {
				toFill.AddTriangle(tris[i], tris[i + 1], tris[i + 2]);
			}

			// Dots
			if (m_DotSize > 0.01f) {
				CacheVertex[0].color = CacheVertex[1].color = CacheVertex[2].color = CacheVertex[3].color = m_DotColor;
				float dotSize = m_DotSize / 2f;
				foreach (var key in m_Curve.keys) {
					float x = key.time * size.x;
					float y = key.value * size.y;
					CacheVertex[0].position.x = x - dotSize + pivotOffset.x;
					CacheVertex[1].position.x = x - dotSize + pivotOffset.x;
					CacheVertex[2].position.x = x + dotSize + pivotOffset.x;
					CacheVertex[3].position.x = x + dotSize + pivotOffset.x;
					CacheVertex[0].position.y = y - dotSize + pivotOffset.y;
					CacheVertex[1].position.y = y + dotSize + pivotOffset.y;
					CacheVertex[2].position.y = y + dotSize + pivotOffset.y;
					CacheVertex[3].position.y = y - dotSize + pivotOffset.y;
					toFill.AddUIVertexQuad(CacheVertex);
				}
			}
		}


		private void RefreshRenderer () {
			if (m_Curve is null || Renderer is null || m_Curve.length == 0) { return; }
			int len = m_Curve.length;
			Vector2 size = (transform as RectTransform).rect.size;
			if (len == 1) {
				float value = m_Curve[0].value;
				Renderer.positionCount = 2;
				Renderer.SetPosition(0, new Vector2(0f, value * size.y));
				Renderer.SetPosition(1, new Vector2(size.x, value * size.y));
			} else {
				Renderer.positionCount = m_Step * (len - 1) + 1;
				float time, endTime, timeStep;
				for (int index = 0; index < len - 1; index++) {
					// Pos
					time = m_Curve[index].time;
					endTime = m_Curve[index + 1].time;
					timeStep = (endTime - time) / m_Step;
					for (int i = 0; i < m_Step; i++) {
						Renderer.SetPosition(index * m_Step + i, new Vector2(
						   time * size.x,
						  //Mathf.Clamp01(m_Curve.Evaluate(time)) * size.y
						  m_Curve.Evaluate(time) * size.y
						));
						time += timeStep;
					}
				}
				time = m_Curve[len - 1].time;
				Renderer.SetPosition(m_Step * (len - 1), new Vector2(time * size.x, Mathf.Clamp01(m_Curve.Evaluate(time)) * size.y));
			}
		}


	}
}


#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEngine;
	using UnityEditor;
	using UIGadget;
	[CustomEditor(typeof(CurveUI))]
	public class CurveUIInspector : Editor {
		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged","m_Sprite","m_Type","m_PreserveAspect",
			"m_FillCenter","m_FillMethod","m_FillAmount","m_FillClockwise","m_FillOrigin","m_UseSpriteMesh",
			"m_PixelsPerUnitMultiplier",
		};
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				(target as CurveUI).SetVerticesDirty();
			}
		}
	}
}
#endif