namespace StagerStudio.Object {
	using global::StagerStudio.Data;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class SpeedNote : MonoBehaviour {




		#region --- VAR ---



		// Api
		public static Beatmap Beatmap { get; set; } = null;
		public static float GameSpeedMuti { get; set; } = 1f;
		public static float MusicTime { get; set; } = 0f;

		// Short
		private float Time { get; set; } = 0f;
		private float Speed { get; set; } = 1f;
		private Vector2? ColSize { get; set; } = null;
		private Quaternion? ColRot { get; set; } = null;

		// Ser
		[SerializeField] private SpriteRenderer m_MainRenderer = null;
		[SerializeField] private BoxCollider m_Col = null;
		[SerializeField] private SpriteRenderer m_Highlight = null;

		// Data
		private static byte CacheDirtyID = 1;
		private byte LocalCacheDirtyID = 0;
		private float HighlightScaleMuti = 0f;
		private Vector2 PrevColSize = Vector3.zero;
		private Quaternion PrevColRot = Quaternion.identity;
		private Beatmap.SpeedNote LateNote = null;


		#endregion




		#region --- MSG ---


		private void Update () {

			ColSize = null;
			LateNote = null;

			// Get Data
			int noteIndex = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && noteIndex < Beatmap.SpeedNotes.Count ? Beatmap.SpeedNotes[noteIndex] : null;
			if (noteData is null) {
				SetRendererEnable(false);
				return;
			}

			// Update
			Update_Cache(noteData);

			noteData.Active = GetActive(noteData, noteData.AppearTime);
			if (!noteData.Active) {
				SetRendererEnable(false);
				return;
			}

			// Final
			LateNote = noteData;
			SetRendererEnable(true);

		}


		private void LateUpdate () {
			LateUpdate_Movement(LateNote);
			LateUpdate_Col_Highlight();
		}


		private void Update_Cache (Beatmap.SpeedNote noteData) {
			if (LocalCacheDirtyID != CacheDirtyID) {
				LocalCacheDirtyID = CacheDirtyID;
				Time = -1f;
			}
			if (GameSpeedMuti != noteData.SpeedMuti) {
				noteData.SpeedMuti = GameSpeedMuti;
				Time = -1f;
			}
			if (Time != noteData.Time) {
				Time = noteData.Time;
				noteData.AppearTime = noteData.Time - 1f / GameSpeedMuti;
				noteData.NoteDropPos = -1f;
			}
			if (Speed != noteData.Speed) {
				Speed = noteData.Speed;
			}
			if (noteData.NoteDropPos < 0f) {
				noteData.NoteDropPos = noteData.Time * GameSpeedMuti;
			}
		}


		private void LateUpdate_Movement (Beatmap.SpeedNote noteData) {
			if (noteData == null || !noteData.Active) { return; }















		}


		private void LateUpdate_Col_Highlight () {
			if (m_Col == null) { return; }
			if (m_Col.enabled != ColSize.HasValue) {
				m_Col.enabled = ColSize.HasValue;
			}
			if (ColSize.HasValue && (PrevColSize != ColSize.Value || (ColRot.HasValue && ColRot.Value != PrevColRot))) {
				var size = new Vector3(ColSize.Value.x, ColSize.Value.y, 0.01f);
				var offset = new Vector3(0f, ColSize.Value.y * 0.5f, 0f);
				PrevColSize = m_Col.size = size;
				m_Col.center = offset;
				m_Highlight.transform.localPosition = offset;

				if (ColRot.HasValue) {
					PrevColRot = m_Col.transform.rotation = ColRot.Value;
				}
			}
			if (m_Highlight.enabled) {
				m_Highlight.size = Vector2.Max(ColSize.Value * HighlightScaleMuti, Vector2.one * 0.36f) + Vector2.one * Mathf.PingPong(UnityEngine.Time.time / 6f, 0.1f);
			}
		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => CacheDirtyID++;


		#endregion




		#region --- LGC ---


		private static bool GetActive (Beatmap.SpeedNote data, float appearTime) => MusicTime >= appearTime && MusicTime <= data.Time;


		private void SetRendererEnable (bool enable) {
			if (enable != m_MainRenderer.enabled) {
				m_MainRenderer.enabled = enable;
			}
		}


		#endregion




	}
}