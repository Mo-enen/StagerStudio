namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;



	public class TimingNote : MonoBehaviour {




		#region --- SUB ---


		public delegate void SfxHandler (byte type, int duration, int a, int b);


		#endregion




		#region --- VAR ---


		// Handler
		public static SfxHandler PlaySfx { get; set; } = null;

		// Api
		public static Beatmap Beatmap { get; set; } = null;
		public static int SortingLayerID_UI { get; set; } = -1;
		public static float GameSpeedMuti { get; set; } = 1f;
		public static float MusicTime { get; set; } = 0f;
		public static bool MusicPlaying { get; set; } = false;
		public static (Vector3 min, Vector3 max, float size, float ratio) ZoneMinMax { get; set; } = (default, default, 0f, 1f);

		// Ser
		[SerializeField] private SpriteRenderer m_MainRenderer = null;
		[SerializeField] private TextRenderer m_LabelRenderer = null;
		[SerializeField] private Transform m_Col = null;
		[SerializeField] private Color m_PositiveTint = Color.white;
		[SerializeField] private Color m_NegativeTint = Color.red;

		// Data
		private bool PrevClicked = false;
		private Beatmap.Timing LateNote = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			m_LabelRenderer.SetSortingLayer(SortingLayerID_UI, 4);
		}


		private void Update () {

			LateNote = null;

			// Get Data
			int noteIndex = transform.GetSiblingIndex();
			var timingData = !(Beatmap is null) && noteIndex < Beatmap.Timings.Count ? Beatmap.Timings[noteIndex] : null;
			if (timingData is null) {
				SetRendererEnable(false);
				Update_Gizmos(null);
				return;
			}

			// Update
			Update_SoundFx(timingData);
			Update_Gizmos(timingData);

			timingData._Active = GetActive(timingData);
			if (!timingData._Active) {
				SetRendererEnable(false);
				gameObject.SetActive(false);
				return;
			}

			// Final
			LateNote = timingData;
			SetRendererEnable(true);

		}


		private void LateUpdate () {
			// Active
			if (m_Col.gameObject.activeSelf != m_MainRenderer.enabled) {
				m_Col.gameObject.SetActive(m_MainRenderer.enabled);
			}
			// Late Note
			if (LateNote == null || !LateNote._Active) { return; }
			float noteY01 = LateNote._NoteDropPos - MusicTime * LateNote._SpeedMuti;
			transform.position = Util.Vector3Lerp3(ZoneMinMax.min, ZoneMinMax.max, 0f, noteY01);
			var tint = LateNote.x >= 0 ? m_PositiveTint : m_NegativeTint;
			tint.a = Mathf.Clamp01(16f - noteY01 * 16f);
			m_MainRenderer.color = tint;
		}


		public static void Update_Cache (Beatmap.Timing timingData) {
			if (timingData._LocalCacheDirtyID != Beatmap.Timing._CacheDirtyID) {
				timingData._LocalCacheDirtyID = Beatmap.Timing._CacheDirtyID;
				timingData._CacheTime = -1f;
			}
			if (GameSpeedMuti != timingData._SpeedMuti) {
				timingData._SpeedMuti = GameSpeedMuti;
				timingData._CacheTime = -1f;
			}
			if (timingData._CacheTime != timingData.Time) {
				timingData._CacheTime = timingData.Time;
				timingData._AppearTime = timingData.Time - 1f / GameSpeedMuti;
				timingData._NoteDropPos = -1f;
			}
			if (timingData._NoteDropPos < 0f) {
				timingData._NoteDropPos = timingData.Time * GameSpeedMuti;
			}
			Beatmap.Timing._CacheMaxTimingIndex = (byte)Mathf.Max(255, Mathf.Max(timingData.TimingID, Beatmap.Timing._CacheMaxTimingIndex));
		}


		private void Update_Gizmos (Beatmap.Timing timingData) {

			bool active = !(timingData is null) && timingData._Active;

			// Label
			if (m_LabelRenderer.gameObject.activeSelf != active) {
				m_LabelRenderer.gameObject.SetActive(active);
			}
			if (active) {
				m_LabelRenderer.Text = $"{timingData.x}%";
			}

		}


		private void Update_SoundFx (Beatmap.Timing noteData) {
			bool clicked = MusicTime > noteData.Time;
			if (MusicPlaying && clicked && !PrevClicked) {
				PlaySfx(noteData.SoundFxIndex, noteData.duration, noteData.SoundFxParamA, noteData.SoundFxParamB);
			}
			PrevClicked = clicked;
		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => Beatmap.Timing._CacheDirtyID++;

		public static bool GetActive (Beatmap.Timing data) => MusicTime >= data._AppearTime && MusicTime <= data.Time;


		#endregion




		#region --- LGC ---


		private void SetRendererEnable (bool enable) {
			if (enable != m_MainRenderer.enabled) {
				m_MainRenderer.enabled = enable;
			}
		}


		#endregion




	}
}