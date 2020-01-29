namespace StagerStudio.UI {
	using global::StagerStudio.Data;
	using Object;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


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

		// Data
		private readonly static List<(float, float)> StageList = new List<(float, float)>();
		private readonly static List<(float, float)> TrackList = new List<(float, float)>();
		private readonly static List<(float, float)> NoteList = new List<(float, float)>();
		private bool IsDirty = false;
		private UIVertex[] VertexCache = new UIVertex[4] { default, default, default, default };


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
			StageList.Clear();
			TrackList.Clear();
			NoteList.Clear();

			var map = GetBeatmap();
			if (map is null) { return; }

			foreach (var stage in map.Stages) {
				if (stage is null) { continue; }
				StageList.Add((GetMusicTime01(stage.Time), GetMusicTime01(stage.Duration)));
			}
			foreach (var track in map.Tracks) {
				if (track is null) { continue; }
				TrackList.Add((GetMusicTime01(track.Time), GetMusicTime01(track.Duration)));
			}
			foreach (var note in map.Notes) {
				if (note is null) { continue; }
				NoteList.Add((GetMusicTime01(note.Time), GetMusicTime01(Mathf.Max(note.Duration, 0.04f))));
			}
			var rect = GetPixelAdjustedRect();
			Fill(StageList, 0f, rect.height * 0.05f, m_StageColor);
			Fill(TrackList, rect.height * 0.1f, rect.height * 0.05f, m_TrackColor);
			Fill(NoteList, rect.height * 0.2f, rect.height * 0.6f, m_NoteColor);
			// Final
			StageList.Clear();
			TrackList.Clear();
			NoteList.Clear();
			// Func
			void Fill (List<(float, float)> objs, float y, float height, Color tint) {
				VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = tint;
				VertexCache[0].position.y = VertexCache[3].position.y = y;
				VertexCache[1].position.y = VertexCache[2].position.y = y + height;
				float left = rect.x;
				float right = rect.x + rect.width;
				foreach (var pair in objs) {
					VertexCache[0].position.x = VertexCache[1].position.x = Mathf.Lerp(left, right, pair.Item1);
					VertexCache[2].position.x = VertexCache[3].position.x = Mathf.Lerp(left, right, pair.Item1 + pair.Item2);
					toFill.AddUIVertexQuad(VertexCache);
				}
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