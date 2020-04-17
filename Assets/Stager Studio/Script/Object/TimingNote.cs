namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;



	public class TimingNote : MonoBehaviour {




		#region --- VAR ---


		// Handler
		public delegate void SfxHandler (byte type, int duration, int a, int b);
		public static SfxHandler PlaySfx { get; set; } = null;

		// Api
		public static int SortingLayerID_UI { get; set; } = -1;
		public static Beatmap Beatmap { get; set; } = null;
		public static float GameSpeedMuti { get; set; } = 1f;
		public static float MusicTime { get; set; } = 0f;
		public static bool MusicPlaying { get; set; } = false;
		public static (Vector3 min, Vector3 max, float size, float ratio) ZoneMinMax { get; set; } = (default, default, 0f, 1f);

		// Short
		private float Time { get; set; } = 0f;
		private int Speed { get; set; } = 100;

		// Ser
		[SerializeField] private SpriteRenderer m_MainRenderer = null;
		[SerializeField] private SpriteRenderer m_Highlight = null;
		[SerializeField] private TextRenderer m_LabelRenderer = null;
		[SerializeField] private Transform m_Col = null;
		[SerializeField] private Color m_PositiveTint = Color.white;
		[SerializeField] private Color m_NegativeTint = Color.red;

		// Data
		private static byte CacheDirtyID = 1;
		private byte LocalCacheDirtyID = 0;
		private bool PrevClicked = false;
		private Beatmap.TimingNote LateNote = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			m_LabelRenderer.SetSortingLayer(SortingLayerID_UI, 4);
		}


		private void Update () {

			LateNote = null;

			// Get Data
			int noteIndex = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && noteIndex < Beatmap.TimingNotes.Count ? Beatmap.TimingNotes[noteIndex] : null;
			if (noteData is null) {
				SetRendererEnable(false);
				Update_Gizmos(null, false);
				return;
			}

			bool oldSelecting = noteData.Selecting;
			noteData.Selecting = false;

			// Update
			Update_Cache(noteData);
			Update_SoundFx(noteData);
			Update_Gizmos(noteData, oldSelecting);

			noteData.Active = GetActive(noteData, noteData.AppearTime);
			if (!noteData.Active) {
				SetRendererEnable(false);
				return;
			}

			// Final
			noteData.Selecting = oldSelecting;
			LateNote = noteData;
			SetRendererEnable(true);

		}


		private void LateUpdate () {
			// Active
			if (m_Col.gameObject.activeSelf != m_MainRenderer.enabled) {
				m_Col.gameObject.SetActive(m_MainRenderer.enabled);
			}
			// Late Note
			if (LateNote == null || !LateNote.Active) { return; }
			float noteY01 = LateNote.NoteDropPos - MusicTime * LateNote.SpeedMuti;
			transform.position = Util.Vector3Lerp3(ZoneMinMax.min, ZoneMinMax.max, 0f, noteY01);
			var tint = Speed >= 0 ? m_PositiveTint : m_NegativeTint;
			tint.a = Mathf.Clamp01(16f - noteY01 * 16f);
			m_MainRenderer.color = tint;
		}


		private void Update_Cache (Beatmap.TimingNote noteData) {
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
			if (Speed != noteData.m_Speed) {
				Speed = noteData.m_Speed;
			}
			if (noteData.NoteDropPos < 0f) {
				noteData.NoteDropPos = noteData.Time * GameSpeedMuti;
			}
		}


		private void Update_Gizmos (Beatmap.TimingNote noteData, bool selecting) {

			bool active = !(noteData is null) && noteData.Active;

			// Highlight
			bool hEnabled = !MusicPlaying && active && selecting;
			if (m_Highlight.gameObject.activeSelf != hEnabled) {
				m_Highlight.gameObject.SetActive(hEnabled);
			}
			if (m_Highlight.gameObject.activeSelf) {
				m_Highlight.size = Vector2.one * 0.4f;
			}

			// Label
			if (m_LabelRenderer.gameObject.activeSelf != active) {
				m_LabelRenderer.gameObject.SetActive(active);
			}
			if (active) {
				m_LabelRenderer.Text = $"{Speed}%";
			}

		}


		private void Update_SoundFx (Beatmap.TimingNote noteData) {
			bool clicked = MusicTime > noteData.Time;
			if (MusicPlaying && clicked && !PrevClicked) {
				PlaySfx(noteData.SoundFxIndex, noteData.SoundFxDuration, noteData.SoundFxParamA, noteData.SoundFxParamB);
			}
			PrevClicked = clicked;
		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => CacheDirtyID++;


		#endregion




		#region --- LGC ---


		private static bool GetActive (Beatmap.TimingNote data, float appearTime) => MusicTime >= appearTime && MusicTime <= data.Time;


		private void SetRendererEnable (bool enable) {
			if (enable != m_MainRenderer.enabled) {
				m_MainRenderer.enabled = enable;
			}
		}


		#endregion




	}
}