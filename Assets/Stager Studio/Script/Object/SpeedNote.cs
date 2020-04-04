namespace StagerStudio.Object {
	using global::StagerStudio.Data;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class SpeedNote : MonoBehaviour {




		#region --- VAR ---



		// Const
		private const float HEIGHT = 0.618f;
		private const float WIDTH_MIN = 0.2f;
		private const float WIDTH_MAX = 4f;

		// Api
		public static Beatmap Beatmap { get; set; } = null;
		public static float GameSpeedMuti { get; set; } = 1f;
		public static float MusicTime { get; set; } = 0f;
		public static bool MusicPlaying { get; set; } = false;
		public static (Vector3 min, Vector3 max, float size, float ratio) ZoneMinMax { get; set; } = (default, default, 0f, 1f);

		// Short
		private float Time { get; set; } = 0f;
		private int Speed { get; set; } = 1000;
		private Vector2? ColSize { get; set; } = null;
		private Quaternion? ColRot { get; set; } = null;

		// Ser
		[SerializeField] private SpriteRenderer m_MainRenderer = null;
		[SerializeField] private SpriteRenderer m_Highlight = null;
		[SerializeField] private BoxCollider m_Col = null;
		[SerializeField] private Transform m_Scaler = null;
		[SerializeField] private Color m_PositiveTint = Color.white;
		[SerializeField] private Color m_NegativeTint = Color.red;

		// Data
		private static byte CacheDirtyID = 1;
		private byte LocalCacheDirtyID = 0;
		private float HighlightScaleMuti = 0f;
		private Vector2 PrevColSize = Vector3.zero;
		private Quaternion PrevColRot = Quaternion.identity;
		private Vector3 RendererScale = Vector3.one;
		private Beatmap.SpeedNote LateNote = null;

		#endregion




		#region --- MSG ---


		private void Awake () {
			RendererScale = m_MainRenderer.transform.localScale;
			ColRot = Quaternion.identity;
		}


		private void Update () {

			ColSize = null;
			LateNote = null;

			// Get Data
			int noteIndex = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && noteIndex < Beatmap.SpeedNotes.Count ? Beatmap.SpeedNotes[noteIndex] : null;
			if (noteData is null) {
				SetRendererEnable(false);
				Update_Gizmos(null, false);
				return;
			}

			bool oldSelecting = noteData.Selecting;
			noteData.Selecting = false;

			// Update
			Update_Cache(noteData);
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
			if (Speed != noteData.m_Speed) {
				Speed = noteData.m_Speed;
			}
			if (noteData.NoteDropPos < 0f) {
				noteData.NoteDropPos = noteData.Time * GameSpeedMuti;
			}
		}


		private void Update_Gizmos (Beatmap.SpeedNote noteData, bool selecting) {

			bool active = !(noteData is null) && noteData.Active;

			// Highlight
			if (m_Highlight != null) {
				m_Highlight.enabled = !MusicPlaying && active && selecting;
			}

		}


		private void LateUpdate_Movement (Beatmap.SpeedNote noteData) {
			if (noteData == null || !noteData.Active) { return; }
			float noteY01 = noteData.NoteDropPos - MusicTime * noteData.SpeedMuti;
			var size = new Vector2(
				Mathf.LerpUnclamped(WIDTH_MIN, WIDTH_MAX, Speed_to_UI01(Speed)),
				HEIGHT
			);
			m_MainRenderer.size = size;
			transform.position = Util.Vector3Lerp3(ZoneMinMax.min, ZoneMinMax.max, 0f, noteY01) +
				(Vector3)(RendererScale * size * 0.5f);
			ColSize = RendererScale * size;
			m_Scaler.localScale = ColSize.Value;
			var tint = Speed >= 0 ? m_PositiveTint : m_NegativeTint;
			tint.a = Mathf.Clamp01(16f - noteY01 * 16f);
			m_MainRenderer.color = tint;
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
			if (ColSize.HasValue && m_Highlight.enabled) {
				m_Highlight.size = Vector2.Max(ColSize.Value * HighlightScaleMuti, Vector2.one * 0.36f) + Vector2.one * Mathf.PingPong(UnityEngine.Time.time / 6f, 0.1f);
			}
		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => CacheDirtyID++;


		public static float Speed_to_UI01 (int speed) {
			speed = Mathf.Abs(speed);
			if (speed < 2000) {
				return speed / 4000f;
			} else if (speed < 16000) {
				return Util.Remap(2000, 16000, 0.5f, 0.75f, speed);
			} else {
				return Util.Remap(16000, 128000, 0.75f, 1f, speed);
			}
		}


		public static int UI01_to_Speed (float uiSpeed) {
			uiSpeed = Mathf.Max(uiSpeed, 0f);
			if (uiSpeed < 0.5f) {
				return Mathf.RoundToInt(uiSpeed * 4000f);
			} else if (uiSpeed < 0.75f) {
				return Mathf.RoundToInt(Util.Remap(0.5f, 0.75f, 2000f, 16000f, uiSpeed));
			} else {
				return Mathf.RoundToInt(Util.Remap(0.75f, 1f, 16000f, 128000f, uiSpeed));
			}
		}


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