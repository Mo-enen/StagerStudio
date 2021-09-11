namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class PreviewUI : Image {




		#region --- SUB ---


		public delegate float FLoatFLoatHandler (float time);
		public delegate Beatmap BeatmapHandler ();

		#endregion




		#region --- VAR ---

		// Handler
		public static FLoatFLoatHandler GetMusicTime01 { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;

		// Ser
		[SerializeField] private Color m_StageColor = new Color(0.5f, 0.11f, 0.65f);
		[SerializeField] private Color m_TrackColor = new Color(0.6f, 0.61f, 0.1f);
		[SerializeField] private Color m_NoteColor = new Color(0.11f, 0.65f, 0.52f);
		[SerializeField] private Color m_TimingColor = new Color(0.65f, 0.11f, 0.52f);

		// Data
		private static readonly UIVertex[] VertexCache = new UIVertex[4] { default, default, default, default };
		private bool IsDirty = false;


		#endregion




		#region --- MSG ---


		private void LateUpdate () {
			if (IsDirty) {
				IsDirty = false;
				SetVerticesDirty();
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
			if (map is null) { return; }

			var rect = GetPixelAdjustedRect();
			float left = rect.xMin;
			float right = rect.xMax;

			// Stage
			VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = m_StageColor;
			VertexCache[0].position.y = VertexCache[3].position.y = 0f;
			VertexCache[1].position.y = VertexCache[2].position.y = rect.height * 0.05f;
			foreach (var stage in map.Stages) {
				if (stage is null) { continue; }
				float time01 = GetMusicTime01(stage.Time);
				float duration01 = GetMusicTime01(stage.Duration);
				VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(left, right, time01);
				VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(left, right, time01 + duration01);
				toFill.AddUIVertexQuad(VertexCache);
			}

			// Track
			VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = m_TrackColor;
			VertexCache[0].position.y = VertexCache[3].position.y = rect.height * 0.1f;
			VertexCache[1].position.y = VertexCache[2].position.y = rect.height * 0.1f + rect.height * 0.05f;
			foreach (var track in map.Tracks) {
				if (track is null) { continue; }
				float time01 = GetMusicTime01(track.Time);
				float duration01 = GetMusicTime01(track.Duration);
				VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(left, right, time01);
				VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(left, right, time01 + duration01);
				toFill.AddUIVertexQuad(VertexCache);
			}

			// Note
			VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = m_NoteColor;
			VertexCache[0].position.y = VertexCache[3].position.y = rect.height * 0.2f;
			VertexCache[1].position.y = VertexCache[2].position.y = rect.height * 0.2f + rect.height * 0.6f;
			foreach (var note in map.Notes) {
				if (note is null) { continue; }
				float time01 = GetMusicTime01(note.Time);
				float duration01 = GetMusicTime01(Mathf.Max(note.Duration, 0.1f));
				VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(left, right, time01);
				VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(left, right, time01 + duration01);
				toFill.AddUIVertexQuad(VertexCache);
			}

			// Timing
			VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = m_TimingColor;
			VertexCache[0].position.y = VertexCache[3].position.y = rect.height * 0.2f;
			VertexCache[1].position.y = VertexCache[2].position.y = rect.height * 0.2f + rect.height * 0.3f;
			float timingDuration = GetMusicTime01(0.4f);
			foreach (var timing in map.Timings) {
				if (timing is null) { continue; }
				float time01 = GetMusicTime01(timing.Time);
				VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(left, right, time01);
				VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(left, right, time01 + timingDuration);
				toFill.AddUIVertexQuad(VertexCache);
			}

		}


		#endregion




		#region --- API ---


		public void Show (bool show) => gameObject.SetActive(show);


		public void SetDirty () => IsDirty = true;


		#endregion





	}
}



#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEditor;
	using UI;
	[CustomEditor(typeof(PreviewUI))]
	public class PreviewUIInspector : Editor {
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