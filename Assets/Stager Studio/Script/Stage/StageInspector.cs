namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;
	using Object;



	public class StageInspector : MonoBehaviour {


		// SUB
		public delegate int IntHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate float FloatHandler ();
		public delegate void VoidHandler ();
		public delegate string StringStringHandler (string str);

		// Const
		private const string HEADER_BEATMAP = "Inspector.Header.Beatmap";
		private const string HEADER_STAGE = "Inspector.Header.Stage";
		private const string HEADER_TRACK = "Inspector.Header.Track";
		private const string HEADER_NOTE = "Inspector.Header.Note";
		private const string HEADER_TIMING = "Inspector.Header.Timing";

		// Handler
		public static IntHandler GetSelectingType { get; set; } = null;
		public static IntHandler GetSelectingIndex { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetBPM { get; set; } = null;
		public static FloatHandler GetShift { get; set; } = null;
		public static VoidHandler OnBeatmapEdited { get; set; } = null;
		public static VoidHandler OnItemEdited { get; set; } = null;
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Api
		public static (int stage, int track, int note) TypeCount { get; set; } = (0, 0, 0);

		// Short
		private BeatmapInspectorUI InspectorBeatmap => _BeatmapInspector != null ? _BeatmapInspector : (_BeatmapInspector = m_Container.GetChild(0).GetComponent<BeatmapInspectorUI>());
		private StageInspectorUI InspectorStage => _StageInspector != null ? _StageInspector : (_StageInspector = m_Container.GetChild(1).GetComponent<StageInspectorUI>());
		private TrackInspectorUI InspectorTrack => _TrackInspector != null ? _TrackInspector : (_TrackInspector = m_Container.GetChild(2).GetComponent<TrackInspectorUI>());
		private NoteInspectorUI InspectorNote => _NoteInspector != null ? _NoteInspector : (_NoteInspector = m_Container.GetChild(3).GetComponent<NoteInspectorUI>());
		private TimingInspectorUI InspectorTiming => _TimingInspector != null ? _TimingInspector : (_TimingInspector = m_Container.GetChild(4).GetComponent<TimingInspectorUI>());

		// Ser
		[SerializeField] private Text m_Header = null;
		[SerializeField] private Text m_Index = null;
		[SerializeField] private RectTransform m_Container = null;
		[SerializeField] private RectTransform m_MotionInspector = null;
		[SerializeField] private MotionPainterUI m_MotionPainter = null;

		// Data
		private BeatmapInspectorUI _BeatmapInspector = null;
		private StageInspectorUI _StageInspector = null;
		private TrackInspectorUI _TrackInspector = null;
		private NoteInspectorUI _NoteInspector = null;
		private TimingInspectorUI _TimingInspector = null;
		private Coroutine MotionInspectorCor = null;
		private bool UIReady = true;


		// MSG
		private void Start () {
			foreach (var label in InspectorBeatmap.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			foreach (var label in InspectorStage.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			foreach (var label in InspectorTrack.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			foreach (var label in InspectorNote.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			foreach (var label in InspectorTiming.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			RefreshUI();
		}


		// API
		public void RefreshAllInspectors () {
			RefreshBeatmapInspector();
			RefreshStageInspector();
			RefreshTrackInspector();
			RefreshNoteInspector();
			RefreshTimingInspector();
		}


		public void RefreshUI () {
			int type = GetSelectingType();
			if (GetBeatmap() != null) {
				int selectingIndex = GetSelectingIndex();
				if (type >= 0 && type < m_Container.childCount && selectingIndex >= 0) {
					switch (type) {
						case 0: // Stage
							m_Header.text = GetLanguage(HEADER_STAGE);
							m_Index.text = selectingIndex.ToString("00");
							m_Index.transform.parent.gameObject.TrySetActive(true);
							SetInspectorActive(1);
							RefreshStageInspector();
							break;
						case 1: // Track
							m_Header.text = GetLanguage(HEADER_TRACK);
							m_Index.text = selectingIndex.ToString("00");
							m_Index.transform.parent.gameObject.TrySetActive(true);
							SetInspectorActive(2);
							RefreshTrackInspector();
							break;
						case 2: // Note
							m_Header.text = GetLanguage(HEADER_NOTE);
							m_Index.text = selectingIndex.ToString("00");
							m_Index.transform.parent.gameObject.TrySetActive(true);
							SetInspectorActive(3);
							RefreshNoteInspector();
							break;
						case 3: // Timing
							m_Header.text = GetLanguage(HEADER_TIMING);
							m_Index.text = selectingIndex.ToString("00");
							m_Index.transform.parent.gameObject.TrySetActive(true);
							SetInspectorActive(4);
							RefreshTimingInspector();
							break;
					}
				} else {
					m_Header.text = GetLanguage(HEADER_BEATMAP);
					m_Index.transform.parent.gameObject.TrySetActive(false);
					SetInspectorActive(0);
					RefreshBeatmapInspector();
				}
			} else {
				m_Header.text = "";
				SetInspectorActive(-1);
			}
		}


		public void StartEditMotion (int motion) {
			m_MotionPainter.ItemIndex = GetSelectingIndex();
			m_MotionPainter.MotionType = motion;
			m_MotionPainter.ScrollValue = 0f;
			m_MotionPainter.SetVerticesDirty();
			MotionItem.SelectingMotionIndex = -1;
			PlayMotionAnimation(true, true);
		}


		public void StopEditMotion (bool useAnimation) {
			m_MotionPainter.MotionType = -1;
			m_MotionPainter.ItemIndex = -1;
			m_MotionPainter.ScrollValue = 0f;
			m_MotionPainter.SetVerticesDirty();
			MotionItem.SelectingMotionIndex = -1;
			PlayMotionAnimation(false, useAnimation);
		}


		public void UI_SfxSetted () {
			var map = GetBeatmap();
			if (!UIReady || map == null) { return; }
			if (InspectorNote.gameObject.activeSelf) {
				map.SetNoteSfxIndex(GetSelectingIndex(), InspectorNote.GetSfx());
				OnItemEdited();
				RefreshNoteInspector();
			}
			if (InspectorTiming.gameObject.activeSelf) {
				map.SetTimingSfxIndex(GetSelectingIndex(), InspectorTiming.GetSfx());
				OnItemEdited();
				RefreshTimingInspector();
			}
		}


		public void UI_SwitchItemType () {
			int selectingType = GetSelectingType();
			int selectingIndex = GetSelectingIndex();
			var map = GetBeatmap();
			if (map == null || selectingType < 0 || selectingType > 2 || selectingIndex < 0) { return; }
			int typeCount = selectingType == 0 ? TypeCount.stage : selectingType == 1 ? TypeCount.track : TypeCount.note;
			int itemType = map.GetItemType(selectingType, selectingIndex);
			itemType = (itemType + 1) % typeCount;
			map.SetItemType(selectingType, selectingIndex, itemType);
			OnItemEdited();
			RefreshAllInspectors();
		}


		public void UI_SetItemType (int value) {
			int selectingType = GetSelectingType();
			int selectingIndex = GetSelectingIndex();
			var map = GetBeatmap();
			if (map == null || selectingType < 0 || selectingIndex < 0) { return; }
			map.SetItemType(selectingType, selectingIndex, value);
			OnItemEdited();
			RefreshAllInspectors();
		}


		public void UI_OnEndEdit_Beatmap (string id) {
			var map = GetBeatmap();
			if (!UIReady || map == null) { return; }
			switch (id.ToLower()) {
				case "bpm":
					map.BPM = InspectorBeatmap.GetBpm();
					break;
				case "shift":
					map.shift = InspectorBeatmap.GetShift();
					break;
				case "ratio":
					map.Ratio = InspectorBeatmap.GetRatio();
					break;
				case "level":
					map.Level = InspectorBeatmap.GetLevel();
					break;
				case "tag":
					map.Tag = InspectorBeatmap.GetTag();
					break;
				default:
					Debug.LogWarning("wrong id: " + id.ToLower());
					break;
			}
			OnBeatmapEdited();
			RefreshBeatmapInspector();
		}


		public void UI_OnEndEdit_Stage (string id) {
			var map = GetBeatmap();
			if (!UIReady || map == null) { return; }
			switch (id.ToLower()) {
				case "time":
					map.SetTime(0, GetSelectingIndex(), InspectorStage.GetTime());
					break;
				case "beat":
					map.SetTime(0, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(InspectorStage.GetBeat(), map.BPM, map.Shift), 0f));
					break;
				case "type":
					map.SetItemType(0, GetSelectingIndex(), InspectorStage.GetItemType());
					break;
				case "duration":
					map.SetDuration(0, GetSelectingIndex(), InspectorStage.GetDuration());
					break;
				case "speed":
					map.SetSpeed(0, GetSelectingIndex(), InspectorStage.GetSpeed());
					break;
				case "pivot":
					map.SetStagePivot(GetSelectingIndex(), InspectorStage.GetPivot());
					break;
				case "pivot.top":
					map.SetStagePivot(GetSelectingIndex(), 1f);
					break;
				case "pivot.mid":
					map.SetStagePivot(GetSelectingIndex(), 0.5f);
					break;
				case "pivot.bottom":
					map.SetStagePivot(GetSelectingIndex(), 0f);
					break;
				case "x":
					map.SetX(0, GetSelectingIndex(), InspectorStage.GetPosX());
					break;
				case "y":
					map.SetStageY(GetSelectingIndex(), InspectorStage.GetPosY());
					break;
				case "rot":
					map.SetStageRotation(GetSelectingIndex(), InspectorStage.GetRot());
					break;
				case "width":
					map.SetStageWidth(GetSelectingIndex(), InspectorStage.GetWidth());
					break;
				case "height":
					map.SetStageHeight(GetSelectingIndex(), InspectorStage.GetHeight());
					break;
				case "color":
					map.SetStageColor(GetSelectingIndex(), InspectorStage.GetColor());
					break;
				default:
					Debug.LogWarning("wrong id: " + id.ToLower());
					break;
			}
			OnItemEdited();
			RefreshStageInspector();
		}


		public void UI_OnEndEdit_Track (string id) {
			var map = GetBeatmap();
			if (!UIReady || map == null) { return; }
			switch (id.ToLower()) {
				case "time":
					map.SetTime(1, GetSelectingIndex(), InspectorTrack.GetTime());
					break;
				case "beat":
					map.SetTime(1, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(InspectorTrack.GetBeat(), map.BPM, map.Shift), 0f));
					break;
				case "type":
					map.SetItemType(1, GetSelectingIndex(), InspectorTrack.GetItemType());
					break;
				case "duration":
					map.SetDuration(1, GetSelectingIndex(), InspectorTrack.GetDuration());
					break;
				case "speed":
					map.SetSpeed(1, GetSelectingIndex(), InspectorTrack.GetSpeed());
					break;
				case "x":
					map.SetX(1, GetSelectingIndex(), InspectorTrack.GetPosX());
					break;
				case "width":
					map.SetTrackWidth(GetSelectingIndex(), InspectorTrack.GetWidth());
					break;
				case "angle":
					map.SetTrackAngle(GetSelectingIndex(), InspectorTrack.GetAngle());
					break;
				case "color":
					map.SetTrackColor(GetSelectingIndex(), InspectorTrack.GetColor());
					break;
				case "tray":
					map.SetTrackTray(GetSelectingIndex(), InspectorTrack.GetTray());
					break;
				case "stageindex":
					map.SetTrackStageIndex(GetSelectingIndex(), Mathf.Clamp(InspectorTrack.GetIndex(), 0, map.Stages.Count - 1));
					break;
				default:
					Debug.LogWarning("wrong id: " + id.ToLower());
					break;
			}
			OnItemEdited();
			RefreshTrackInspector();
		}


		public void UI_OnEndEdit_Note (string id) {
			var map = GetBeatmap();
			if (!UIReady || map == null) { return; }
			switch (id.ToLower()) {
				case "time":
					map.SetTime(2, GetSelectingIndex(), InspectorNote.GetTime());
					break;
				case "beat":
					map.SetTime(2, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(InspectorNote.GetBeat(), map.BPM, map.Shift), 0f));
					break;
				case "type":
					map.SetItemType(2, GetSelectingIndex(), InspectorNote.GetItemType());
					break;
				case "duration":
					map.SetDuration(2, GetSelectingIndex(), InspectorNote.GetDuration());
					break;
				case "speed":
					map.SetSpeed(2, GetSelectingIndex(), InspectorNote.GetSpeed());
					break;
				case "x":
					map.SetX(2, GetSelectingIndex(), InspectorNote.GetPosX());
					break;
				case "width":
					map.SetNoteWidth(GetSelectingIndex(), InspectorNote.GetWidth());
					break;
				case "trackindex":
					map.SetNoteTrackIndex(GetSelectingIndex(), Mathf.Clamp(InspectorNote.GetIndex(), 0, map.Tracks.Count - 1));
					break;
				case "link":
					map.SetNoteLinkedIndex(GetSelectingIndex(), Mathf.Clamp(InspectorNote.GetLink(), -1, map.Notes.Count - 1));
					break;
				case "z":
					map.SetNoteZ(GetSelectingIndex(), InspectorNote.GetPosZ());
					break;
				case "click":
					map.SetNoteClickIndex(GetSelectingIndex(), InspectorNote.GetClick());
					break;
				case "parama":
					map.SetNoteParamA(GetSelectingIndex(), InspectorNote.GetSfxParamA());
					break;
				case "paramb":
					map.SetNoteParamB(GetSelectingIndex(), InspectorNote.GetSfxParamB());
					break;
				default:
					Debug.LogWarning("wrong id: " + id.ToLower());
					break;
			}
			OnItemEdited();
			RefreshNoteInspector();
		}


		public void UI_OnEndEdit_Timing (string id) {
			var map = GetBeatmap();
			if (!UIReady || map == null) { return; }
			switch (id.ToLower()) {
				case "time":
					map.SetTime(3, GetSelectingIndex(), InspectorTiming.GetTime());
					break;
				case "beat":
					map.SetTime(3, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(InspectorTiming.GetBeat(), map.BPM, map.Shift), 0f));
					break;
				case "duration":
					map.SetDuration(3, GetSelectingIndex(), InspectorTiming.GetDuration());
					break;
				case "speed":
					map.SetTimingX(GetSelectingIndex(), InspectorTiming.GetSpeed());
					break;
				case "parama":
					map.SetTimingParamA(GetSelectingIndex(), InspectorTiming.GetSfxParamA());
					break;
				case "paramb":
					map.SetTimingParamB(GetSelectingIndex(), InspectorTiming.GetSfxParamB());
					break;
				default:
					Debug.LogWarning("wrong id: " + id.ToLower());
					break;
			}
			OnItemEdited();
			RefreshTimingInspector();
		}


		// Refresh
		private void RefreshBeatmapInspector () {
			var map = GetBeatmap();
			if (map == null) { return; }
			UIReady = false;
			try {
				InspectorBeatmap.SetBpm(map.BPM);
				InspectorBeatmap.SetShift(map.shift);
				InspectorBeatmap.SetRatio(map.Ratio);
				InspectorBeatmap.SetLevel(map.Level);
				InspectorBeatmap.SetTag(map.Tag);
			} catch { }
			UIReady = true;
		}


		private void RefreshStageInspector () {
			var map = GetBeatmap();
			int type = GetSelectingType();
			int index = GetSelectingIndex();
			if (map == null || type != 0 || index < 0 || index >= map.Stages.Count) { return; }
			UIReady = false;
			try {
				var stage = map.Stages[index];
				InspectorStage.SetTime(stage.Time);
				InspectorStage.SetBeat(Util.Time_to_Beat(stage.Time, GetBPM(), GetShift()));
				InspectorStage.SetItemType(stage.ItemType);
				InspectorStage.SetDuration(stage.Duration);
				InspectorStage.SetSpeed(stage.Speed);
				InspectorStage.SetPivot(stage.PivotY);
				InspectorStage.SetPosX(stage.X);
				InspectorStage.SetPosY(stage.Y);
				InspectorStage.SetRot(stage.Rotation);
				InspectorStage.SetWidth(stage.Width);
				InspectorStage.SetHeight(stage.Height);
				InspectorStage.SetColor(stage.Color);
			} catch { }
			UIReady = true;
		}


		private void RefreshTrackInspector () {
			var map = GetBeatmap();
			int type = GetSelectingType();
			int index = GetSelectingIndex();
			if (map == null || type != 1 || index < 0 || index >= map.Tracks.Count) { return; }
			UIReady = false;
			try {
				var track = map.Tracks[index];
				InspectorTrack.SetTime(track.Time);
				InspectorTrack.SetBeat(Util.Time_to_Beat(track.Time, GetBPM(), GetShift()));
				InspectorTrack.SetItemType(track.ItemType);
				InspectorTrack.SetSpeed(track.Speed);
				InspectorTrack.SetDuration(track.Duration);
				InspectorTrack.SetPosX(track.X);
				InspectorTrack.SetWidth(track.Width);
				InspectorTrack.SetAngle(track.Angle);
				InspectorTrack.SetColor(track.Color);
				InspectorTrack.SetTray(track.HasTray);
				InspectorTrack.SetIndex(track.StageIndex);
			} catch { }
			UIReady = true;
		}


		private void RefreshNoteInspector () {
			var map = GetBeatmap();
			int type = GetSelectingType();
			int index = GetSelectingIndex();
			if (map == null || type != 2 || index < 0 || index >= map.Notes.Count) { return; }
			UIReady = false;
			try {
				var note = map.Notes[index];
				InspectorNote.SetTime(note.Time);
				InspectorNote.SetBeat(Util.Time_to_Beat(note.Time, GetBPM(), GetShift()));
				InspectorNote.SetItemType(note.ItemType);
				InspectorNote.SetDuration(note.Duration);
				InspectorNote.SetSpeed(note.Speed);
				InspectorNote.SetPosX(note.X);
				InspectorNote.SetWidth(note.Width);
				InspectorNote.SetIndex(note.TrackIndex);
				InspectorNote.SetLink(note.LinkedNoteIndex);
				InspectorNote.SetPosZ(note.Z);
				InspectorNote.SetClick(note.ClickSoundIndex);
				InspectorNote.SetSfx(note.SoundFxIndex);
				InspectorNote.SetSfxParamA(note.SoundFxParamA);
				InspectorNote.SetSfxParamB(note.SoundFxParamB);
			} catch { }
			UIReady = true;
		}


		private void RefreshTimingInspector () {
			var map = GetBeatmap();
			int type = GetSelectingType();
			int index = GetSelectingIndex();
			if (map == null || type != 3 || index < 0 || index >= map.Timings.Count) { return; }
			UIReady = false;
			try {
				var timing = map.Timings[index];
				InspectorTiming.SetTime(timing.Time);
				InspectorTiming.SetBeat(Util.Time_to_Beat(timing.Time, GetBPM(), GetShift()));
				InspectorTiming.SetDuration(timing.Duration);
				InspectorTiming.SetSpeed(timing.x);
				InspectorTiming.SetSfx(timing.SoundFxIndex);
				InspectorTiming.SetSfxParamA(timing.SoundFxParamA);
				InspectorTiming.SetSfxParamB(timing.SoundFxParamB);
			} catch { }
			UIReady = true;
		}


		private void SetInspectorActive (int index) {
			int count = m_Container.childCount;
			for (int i = 0; i < count; i++) {
				m_Container.GetChild(i).gameObject.SetActive(i == index);
			}
		}


		// Motion
		private void PlayMotionAnimation (bool showMotion, bool useAnimation) {
			if (MotionInspectorCor != null) {
				StopCoroutine(MotionInspectorCor);
			}
			MotionInspectorCor = StartCoroutine(ShowingMotion(showMotion));
			IEnumerator ShowingMotion (bool show) {
				const float POS_LEFT = 0f;
				const float POS_RIGHT = 106f;
				var motionRT = m_MotionInspector.transform as RectTransform;
				float conPosY = m_Container.anchoredPosition.y;
				float motionPosY = motionRT.anchoredPosition.y;
				float fromX = show ? POS_RIGHT : POS_LEFT;
				float toX = show ? POS_LEFT : POS_RIGHT;
				m_Container.gameObject.SetActive(true);
				m_MotionInspector.gameObject.SetActive(true);
				for (float lerp = 0f; useAnimation && lerp < 0.999f; lerp = Mathf.Lerp(lerp, 1f, Time.deltaTime * 20f)) {
					m_Container.anchoredPosition = new Vector2(Mathf.Lerp(toX, fromX, lerp), conPosY);
					motionRT.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, lerp), motionPosY);
					yield return new WaitForEndOfFrame();
				}
				m_Container.anchoredPosition = new Vector2(fromX, conPosY);
				motionRT.anchoredPosition = new Vector2(toX, motionPosY);
				m_Container.gameObject.SetActive(!show);
				m_MotionInspector.gameObject.SetActive(show);
				MotionInspectorCor = null;
			}
		}


	}
}