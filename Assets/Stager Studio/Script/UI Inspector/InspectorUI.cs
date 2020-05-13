namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class InspectorUI : MonoBehaviour {


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

		// Short
		private BeatmapInspectorUI BeatmapInspector => _BeatmapInspector != null ? _BeatmapInspector : (_BeatmapInspector = m_Container.GetChild(0).GetComponent<BeatmapInspectorUI>());
		private StageInspectorUI StageInspector => _StageInspector != null ? _StageInspector : (_StageInspector = m_Container.GetChild(1).GetComponent<StageInspectorUI>());
		private TrackInspectorUI TrackInspector => _TrackInspector != null ? _TrackInspector : (_TrackInspector = m_Container.GetChild(2).GetComponent<TrackInspectorUI>());
		private NoteInspectorUI NoteInspector => _NoteInspector != null ? _NoteInspector : (_NoteInspector = m_Container.GetChild(3).GetComponent<NoteInspectorUI>());
		private TimingInspectorUI TimingInspector => _TimingInspector != null ? _TimingInspector : (_TimingInspector = m_Container.GetChild(4).GetComponent<TimingInspectorUI>());

		// Ser
		[SerializeField] private Text m_Header = null;
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



		private void Start () {
			Start_Beatmap();
			Start_Stage();
			Start_Track();
			Start_Note();
			Start_Timing();
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
				if (type >= 0 && type < m_Container.childCount && GetSelectingIndex() >= 0) {
					switch (type) {
						case 0: // Stage
							m_Header.text = GetLanguage(HEADER_STAGE);
							SetInspectorActive(1);
							RefreshStageInspector();
							break;
						case 1: // Track
							m_Header.text = GetLanguage(HEADER_TRACK);
							SetInspectorActive(2);
							RefreshTrackInspector();
							break;
						case 2: // Note
							m_Header.text = GetLanguage(HEADER_NOTE);
							SetInspectorActive(3);
							RefreshNoteInspector();
							break;
						case 3: // Timing
							m_Header.text = GetLanguage(HEADER_TIMING);
							SetInspectorActive(4);
							RefreshTimingInspector();
							break;
					}
				} else {
					m_Header.text = GetLanguage(HEADER_BEATMAP);
					SetInspectorActive(0);
					RefreshBeatmapInspector();
				}
			} else {
				m_Header.text = "";
				SetInspectorActive(-1);
			}
		}


		public void StartEditMotion_Stage (int motion) {
			m_MotionPainter.TypeIndex = 0;
			m_MotionPainter.MotionIndex = motion;
			m_MotionPainter.SetVerticesDirty();
			PlayMotionAnimation(true, true);
		}


		public void StartEditMotion_Track (int motion) {
			m_MotionPainter.TypeIndex = 1;
			m_MotionPainter.MotionIndex = motion;
			m_MotionPainter.SetVerticesDirty();
			PlayMotionAnimation(true, true);
		}


		public void StopEditMotion (bool useAnimation) {
			m_MotionPainter.TypeIndex = -1;
			m_MotionPainter.MotionIndex = -1;
			m_MotionPainter.SetVerticesDirty();
			PlayMotionAnimation(false, useAnimation);
		}


		// Init
		private void Start_Beatmap () {
			// Language
			foreach (var label in BeatmapInspector.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			// BPM
			BeatmapInspector.BpmIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.BPM = BeatmapInspector.GetBpm();
				OnBeatmapEdited();
				RefreshBeatmapInspector();
			});
			// Shift
			BeatmapInspector.ShiftIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.m_Shift = BeatmapInspector.GetShift();
				OnBeatmapEdited();
				RefreshBeatmapInspector();
			});
			// Ratio
			BeatmapInspector.RatioIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.Ratio = BeatmapInspector.GetRatio();
				OnBeatmapEdited();
				RefreshBeatmapInspector();
			});
			// Level
			BeatmapInspector.LevelIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.Level = BeatmapInspector.GetLevel();
				OnBeatmapEdited();
				RefreshBeatmapInspector();
			});
			// Tag
			BeatmapInspector.TagIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.Tag = BeatmapInspector.GetTag();
				OnBeatmapEdited();
				RefreshBeatmapInspector();
			});
		}


		private void Start_Stage () {
			// Language
			foreach (var label in StageInspector.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			// Time
			StageInspector.TimeIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(0, GetSelectingIndex(), StageInspector.GetTime());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Beat
			StageInspector.BeatIF.OnEndEdit = () => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(0, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(StageInspector.GetBeat(), map.BPM, map.Shift), 0f));
				OnItemEdited();
				RefreshStageInspector();
			};
			// Type
			StageInspector.TypeIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetItemType(0, GetSelectingIndex(), StageInspector.GetItemType());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Duration
			StageInspector.DurationIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetDuration(0, GetSelectingIndex(), StageInspector.GetDuration());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Speed
			StageInspector.SpeedIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetStageSpeed(GetSelectingIndex(), StageInspector.GetSpeed());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Pivot
			StageInspector.PivotIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetStagePivot(GetSelectingIndex(), StageInspector.GetPivot());
				OnItemEdited();
				RefreshStageInspector();
			});
			// X
			StageInspector.PosXIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetX(0, GetSelectingIndex(), StageInspector.GetPosX());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Y
			StageInspector.PosYIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetStageY(GetSelectingIndex(), StageInspector.GetPosY());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Rot
			StageInspector.RotIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetStageRotation(GetSelectingIndex(), StageInspector.GetRot());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Width
			StageInspector.WidthIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetStageWidth(GetSelectingIndex(), StageInspector.GetWidth());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Height
			StageInspector.HeightIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetStageHeight(GetSelectingIndex(), StageInspector.GetHeight());
				OnItemEdited();
				RefreshStageInspector();
			});
			// Color
			StageInspector.ColorIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetStageColor(GetSelectingIndex(), StageInspector.GetColor());
				OnItemEdited();
				RefreshStageInspector();
			});
		}


		private void Start_Track () {
			// Language
			foreach (var label in TrackInspector.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			// Time
			TrackInspector.TimeIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(1, GetSelectingIndex(), TrackInspector.GetTime());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// Beat
			TrackInspector.BeatIF.OnEndEdit = () => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(1, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(TrackInspector.GetBeat(), map.BPM, map.Shift), 0f));
				OnItemEdited();
				RefreshTrackInspector();
			};
			// Type
			TrackInspector.TypeIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetItemType(1, GetSelectingIndex(), TrackInspector.GetItemType());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// Duration
			TrackInspector.DurationIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetDuration(1, GetSelectingIndex(), TrackInspector.GetDuration());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// X
			TrackInspector.PosXIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetX(1, GetSelectingIndex(), TrackInspector.GetPosX());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// Width
			TrackInspector.WidthIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTrackWidth(GetSelectingIndex(), TrackInspector.GetWidth());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// Angle
			TrackInspector.AngleIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTrackAngle(GetSelectingIndex(), TrackInspector.GetAngle());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// Color
			TrackInspector.ColorIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTrackColor(GetSelectingIndex(), TrackInspector.GetColor());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// Tray
			TrackInspector.TrayTG.onValueChanged.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTrackTray(GetSelectingIndex(), TrackInspector.GetTray());
				OnItemEdited();
				RefreshTrackInspector();
			});
			// Stage Index
			TrackInspector.IndexIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTrackStageIndex(GetSelectingIndex(), Mathf.Clamp(TrackInspector.GetIndex(), 0, map.Stages.Count - 1));
				OnItemEdited();
				RefreshTrackInspector();
			});
		}


		private void Start_Note () {
			// Language
			foreach (var label in NoteInspector.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			// Time
			NoteInspector.TimeIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(2, GetSelectingIndex(), NoteInspector.GetTime());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Beat
			NoteInspector.BeatIF.OnEndEdit = () => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(2, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(NoteInspector.GetBeat(), map.BPM, map.Shift), 0f));
				OnItemEdited();
				RefreshNoteInspector();
			};
			// Type
			NoteInspector.TypeIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetItemType(2, GetSelectingIndex(), NoteInspector.GetItemType());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Duration
			NoteInspector.DurationIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetDuration(2, GetSelectingIndex(), NoteInspector.GetDuration());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// X
			NoteInspector.PosXIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetX(2, GetSelectingIndex(), NoteInspector.GetPosX());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Width
			NoteInspector.WidthIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteWidth(GetSelectingIndex(), NoteInspector.GetWidth());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Track
			NoteInspector.IndexIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteTrackIndex(GetSelectingIndex(), Mathf.Clamp(NoteInspector.GetIndex(), 0, map.Tracks.Count - 1));
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Link
			NoteInspector.LinkIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteLinkedIndex(GetSelectingIndex(), Mathf.Clamp(NoteInspector.GetLink(), -1, map.Notes.Count - 1));
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Z
			NoteInspector.PosZIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteZ(GetSelectingIndex(), NoteInspector.GetPosZ());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Click
			NoteInspector.ClickIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteClickIndex(GetSelectingIndex(), NoteInspector.GetClick());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Sfx
			NoteInspector.SfxIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteSfxIndex(GetSelectingIndex(), NoteInspector.GetSfx());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Param A
			NoteInspector.SfxParamAIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteParamA(GetSelectingIndex(), NoteInspector.GetSfxParamA());
				OnItemEdited();
				RefreshNoteInspector();
			});
			// Param B
			NoteInspector.SfxParamBIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetNoteParamB(GetSelectingIndex(), NoteInspector.GetSfxParamB());
				OnItemEdited();
				RefreshNoteInspector();
			});
		}


		private void Start_Timing () {
			// Language
			foreach (var label in TimingInspector.LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
			// Time
			TimingInspector.TimeIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(3, GetSelectingIndex(), TimingInspector.GetTime());
				OnItemEdited();
				RefreshTimingInspector();
			});
			// Beat
			TimingInspector.BeatIF.OnEndEdit = () => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTime(3, GetSelectingIndex(), Mathf.Max(Util.Beat_to_Time(TimingInspector.GetBeat(), map.BPM, map.Shift), 0f));
				OnItemEdited();
				RefreshTimingInspector();
			};
			// Duration
			TimingInspector.DurationIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetDuration(3, GetSelectingIndex(), TimingInspector.GetDuration());
				OnItemEdited();
				RefreshTimingInspector();
			});
			// Speed
			TimingInspector.SpeedIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTimingSpeed(GetSelectingIndex(), TimingInspector.GetSpeed());
				OnItemEdited();
				RefreshTimingInspector();
			});
			// Sfx
			TimingInspector.SfxIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTimingSfxIndex(GetSelectingIndex(), TimingInspector.GetSfx());
				OnItemEdited();
				RefreshTimingInspector();
			});
			// Param A
			TimingInspector.SfxParamAIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTimingParamA(GetSelectingIndex(), TimingInspector.GetSfxParamA());
				OnItemEdited();
				RefreshTimingInspector();
			});
			// Param B
			TimingInspector.SfxParamBIF.onEndEdit.AddListener((_) => {
				var map = GetBeatmap();
				if (!UIReady || map == null) { return; }
				map.SetTimingParamB(GetSelectingIndex(), TimingInspector.GetSfxParamB());
				OnItemEdited();
				RefreshTimingInspector();
			});
		}


		// Refresh
		private void RefreshBeatmapInspector () {
			var map = GetBeatmap();
			if (map == null) { return; }
			UIReady = false;
			try {
				BeatmapInspector.SetBpm(map.BPM);
				BeatmapInspector.SetShift(map.m_Shift);
				BeatmapInspector.SetRatio(map.Ratio);
				BeatmapInspector.SetLevel(map.Level);
				BeatmapInspector.SetTag(map.Tag);
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
				StageInspector.SetTime(stage.Time);
				StageInspector.SetBeat(Util.Time_to_Beat(stage.Time, GetBPM(), GetShift()));
				StageInspector.SetItemType(stage.ItemType);
				StageInspector.SetDuration(stage.Duration);
				StageInspector.SetSpeed(stage.Speed);
				StageInspector.SetPivot(stage.PivotY);
				StageInspector.SetPosX(stage.X);
				StageInspector.SetPosY(stage.Y);
				StageInspector.SetRot(stage.Rotation);
				StageInspector.SetWidth(stage.Width);
				StageInspector.SetHeight(stage.Height);
				StageInspector.SetColor(stage.Color);
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
				TrackInspector.SetTime(track.Time);
				TrackInspector.SetBeat(Util.Time_to_Beat(track.Time, GetBPM(), GetShift()));
				TrackInspector.SetItemType(track.ItemType);
				TrackInspector.SetDuration(track.Duration);
				TrackInspector.SetPosX(track.X);
				TrackInspector.SetWidth(track.Width);
				TrackInspector.SetAngle(track.Angle);
				TrackInspector.SetColor(track.Color);
				TrackInspector.SetTray(track.HasTray);
				TrackInspector.SetIndex(track.StageIndex);
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
				NoteInspector.SetTime(note.Time);
				NoteInspector.SetBeat(Util.Time_to_Beat(note.Time, GetBPM(), GetShift()));
				NoteInspector.SetItemType(note.ItemType);
				NoteInspector.SetDuration(note.Duration);
				NoteInspector.SetPosX(note.X);
				NoteInspector.SetWidth(note.Width);
				NoteInspector.SetIndex(note.TrackIndex);
				NoteInspector.SetLink(note.LinkedNoteIndex);
				NoteInspector.SetPosZ(note.Z);
				NoteInspector.SetClick(note.ClickSoundIndex);
				NoteInspector.SetSfx(note.SoundFxIndex);
				NoteInspector.SetSfxParamA(note.SoundFxParamA);
				NoteInspector.SetSfxParamB(note.SoundFxParamB);
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
				TimingInspector.SetTime(timing.Time);
				TimingInspector.SetBeat(Util.Time_to_Beat(timing.Time, GetBPM(), GetShift()));
				TimingInspector.SetDuration(timing.Duration);
				TimingInspector.SetSpeed(timing.m_X);
				TimingInspector.SetSfx(timing.SoundFxIndex);
				TimingInspector.SetSfxParamA(timing.SoundFxParamA);
				TimingInspector.SetSfxParamB(timing.SoundFxParamB);
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
				motionRT.anchoredPosition = new Vector2(toX, conPosY);
				m_Container.gameObject.SetActive(!show);
				m_MotionInspector.gameObject.SetActive(show);
				MotionInspectorCor = null;
			}
		}


	}
}