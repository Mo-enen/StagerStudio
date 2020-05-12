namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class MotionPainterUI : Image {





		#region --- VAR ---


		// Handler
		public delegate Beatmap BeatmapHandler ();
		public static BeatmapHandler GetBeatmap { get; set; } = null;

		// Api
		public int TypeIndex { get; set; } = -1; // -1: None   0:Stage   1:Track
		public int MotionIndex { get; set; } = -1; // Stage: Pos Rot Width Height Color // Track: X Angle Width Color

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Data
		private static readonly UIVertex[] VertexCache = new UIVertex[4] { default, default, default, default };
		private Camera _Camera = null;
		private (bool active, bool down, Vector3 pos, Vector2 pos01) Mouse = default;


		#endregion




		#region --- MSG ---



		private void Update () {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			var rt = transform as RectTransform;
			var pos01 = rt.Get01Position(Input.mousePosition, Camera);
			if (pos01.x >= 0f && pos01.x <= 1f && pos01.y >= 0f && pos01.y <= 1f) {
				bool getLeftDown = Input.GetMouseButton(0);
				if (!Mouse.active || Input.mousePosition != Mouse.pos || getLeftDown != Mouse.down) {
					SetVerticesDirty();
					Mouse = (true, getLeftDown, Input.mousePosition, pos01);
				}
			} else {
				if (Mouse.active || Mouse.down) {
					SetVerticesDirty();
					Mouse.active = false;
					Mouse.down = false;
				}
			}
		}


		protected override void OnPopulateMesh (VertexHelper toFill) {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) {
				toFill.Clear();
				return;
			}
#endif
			toFill.Clear();
			var map = GetBeatmap();
			if (map == null || TypeIndex < 0 || MotionIndex < 0) { return; }










		}


		#endregion



	}
}




#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEditor;
	using UI;
	[CustomEditor(typeof(MotionPainterUI))]
	public class MotionPainterUIInspector : Editor {
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