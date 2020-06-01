namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UI;
	using Stage;
	using Object;
	using Rendering;
	using Data;
	using Saving;
	using UndoRedo;
	using DebugLog;


	public partial class StagerStudio : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable]
		public struct CursorData {
			public Texture2D Cursor;
			public Vector2 Offset;
		}


		[System.Serializable]
		private class UndoData {
			public Beatmap Map;
			public float MusicTime;
			public bool[] ContainerActive;
		}


		// Handler
		public delegate SkinData SkinDataStringHandler (string name);
		public delegate Beatmap BeatmapHandler ();
		public delegate void VoidHandler ();
		public delegate bool BoolHandler ();
		public delegate float FloatHandler ();
		public delegate string StringStringHandler (string str);
		public delegate int IntIntHandler (int i);
		public delegate void VoidIntHandler (int i);
		public delegate (float value, float index, float width) AbreastHandler ();
		public delegate (float a, float b) FloatFloatHandler ();


		#endregion




		#region --- VAR ---


		// Const
		private const string UI_QuitConfirm = "Menu.UI.QuitConfirm";
		private const string UI_SelectorStageMenu = "Menu.UI.SelectorStage";
		private const string UI_SelectorTrackMenu = "Menu.UI.SelectorTrack";
		private const string UI_OpenWebMSG = "Dialog.OpenWebMSG";
		private const string Confirm_DeleteProjectPal = "ProjectInfo.Dialog.DeletePal";
		private const string Confirm_DeleteProjectSound = "ProjectInfo.Dialog.DeleteSound";
		private const string Confirm_DeleteProjectTween = "ProjectInfo.Dialog.DeleteTween";
		private const string Hint_CommandDone = "Command.Hint.CommandDone";
		private const string Hint_Volume = "Music.Hint.Volume";

		// Handler
		private SkinDataStringHandler GetSkinFromDisk { get; set; } = null;
		private StringStringHandler GetLanguage { get; set; } = null;
		private FloatHandler GetDropSpeed { get; set; } = null;
		private VoidIntHandler ProjectRemoveClickSound { get; set; } = null;
		private VoidIntHandler ProjectRemoveTweenAt { get; set; } = null;
		private VoidIntHandler ProjectRemovePaletteAt { get; set; } = null;
		private AbreastHandler GetAbreastData { get; set; } = null;
		private IntIntHandler GetGameItemCount { get; set; } = null;
		private FloatHandler GetMusicTime { get; set; } = null;
		private VoidHandler MusicPause { get; set; } = null;

		// Ser
		[Header("Stage")]
		[SerializeField] private StageMusic m_Music = null;
		[SerializeField] private StageProject m_Project = null;
		[SerializeField] private StageGame m_Game = null;
		[SerializeField] private StageSoundFX m_SoundFX = null;
		[SerializeField] private StageEditor m_Editor = null;
		[SerializeField] private StageLanguage m_Language = null;
		[SerializeField] private StageSkin m_Skin = null;
		[SerializeField] private StageShortcut m_Shortcut = null;
		[SerializeField] private StageMenu m_Menu = null;
		[SerializeField] private StageState m_State = null;
		[SerializeField] private StageEffect m_Effect = null;
		[SerializeField] private StageEasterEgg m_EasterEgg = null;
		[Header("Misc")]
		[SerializeField] private Transform m_CanvasRoot = null;
		[SerializeField] private RectTransform m_DirtyMark = null;
		[SerializeField] private Text m_BeatmapSwiperLabel = null;
		[SerializeField] private Text m_SkinSwiperLabel = null;
		[SerializeField] private Image m_AbreastTGMark = null;
		[SerializeField] private Toggle m_GridTG = null;
		[SerializeField] private Text m_VersionLabel = null;
		[SerializeField] private GridRenderer m_GridRenderer = null;
		[SerializeField] private RectTransform m_PitchWarningBlock = null;
		[SerializeField] private RectTransform m_PitchTrebleClef = null;
		[SerializeField] private RectTransform m_PitchBassClef = null;
		[SerializeField] private Transform m_CameraTF = null;
		[SerializeField] private Transform m_TutorialBoard = null;
		[SerializeField] private Text m_TipLabelA = null;
		[SerializeField] private Text m_TipLabelB = null;
		[SerializeField] private RectTransform m_MotionInspector = null;
		[SerializeField] private RectTransform m_SelectBrushMark = null;
		[SerializeField] private RectTransform m_EraseBrushMark = null;
		[SerializeField] private RectTransform m_GlobalBrushMark = null;
		[Header("UI")]
		[SerializeField] private BackgroundUI m_Background = null;
		[SerializeField] private ProgressUI m_Progress = null;
		[SerializeField] private HintBarUI m_Hint = null;
		[SerializeField] private ZoneUI m_Zone = null;
		[SerializeField] private PreviewUI m_Preview = null;
		[SerializeField] private WaveUI m_Wave = null;
		[SerializeField] private TimingPreviewUI m_TimingPreview = null;
		[SerializeField] private AxisHandleUI m_Axis = null;
		[SerializeField] private InspectorUI m_Inspector = null;
		[SerializeField] private MotionPainterUI m_MotionPainter = null;
		[SerializeField] private KeypressUI m_Keypress = null;
		[Header("Data")]
		[SerializeField] private TextSpriteSheet m_TextSheet = null;
		[SerializeField] private Text[] m_LanguageTexts = null;
		[SerializeField] private CursorData[] m_Cursors = null;

		// Saving
		private SavingBool TutorialOpened = new SavingBool("StagerStudio.TutorialOpened", false);
		private SavingBool SoloOnEditMotion = new SavingBool("StagerStudio.SoloOnEditMotion", true);

		// Data
		private bool WillUndo = false;
		private bool WillRedo = false;


		#endregion




		#region --- MSG ---


		private void Awake () {

			GetSkinFromDisk = m_Skin.GetSkinFromDisk;
			MusicPause = m_Music.Pause;
			GetMusicTime = () => m_Music.Time;
			ProjectRemoveClickSound = m_Project.RemoveClickSound;
			ProjectRemoveTweenAt = m_Project.RemoveTweenAt;
			ProjectRemovePaletteAt = m_Project.RemovePaletteAt;
			GetAbreastData = () => (m_Game.AbreastValue, m_Game.AbreastIndex, m_Game.AbreastWidth);
			GetDropSpeed = () => m_Game.GameDropSpeed;
			GetGameItemCount = m_Game.GetItemCount;
			GetLanguage = m_Language.Get;

			Awake_Message();
			Awake_Quit();
			Awake_Setting();
			Awake_Setting_UI();
			Awake_Menu();
			Awake_Object();
			Awake_Project();
			Awake_Game();
			Awake_Music();
			Awake_Sfx();
			Awake_Editor();
			Awake_Skin();
			Awake_Undo();
			Awake_ProjectInfo();
			Awake_Inspector();
			Awake_Misc();

		}


		private void Start () {
			LoadAllSettings();
			UI_RemoveUI();
			if (Screen.fullScreen) {
				Screen.fullScreen = false;
			}
			QualitySettings.vSyncCount = 0;
			m_Background.SetBackground(null, false);
			m_SelectBrushMark.gameObject.TrySetActive(m_Editor.SelectingBrushIndex == -1);
			m_EraseBrushMark.gameObject.TrySetActive(m_Editor.SelectingBrushIndex == -2);
			m_GlobalBrushMark.gameObject.TrySetActive(m_Editor.UseGlobalBrushScale.Value);
			DebugLog_Start();
		}


		private void Update () {

			var (aValue, aIndex, aWidth) = GetAbreastData();
			var dropSpeed = GetDropSpeed();
			var map = m_Project.Beatmap;
			float musicTime = GetMusicTime();
			CursorUI.GlobalUpdate();
			if (!Input.anyKey) {
				UndoRedo.RegisterUndoIfDirty();
			}
			StageObject.ZoneMinMax = ObjectTimer.ZoneMinMax = m_Zone.GetZoneMinMax();
			StageObject.Abreast = (aIndex, aValue, aWidth);
			StageObject.ScreenZoneMinMax = m_Zone.GetScreenZoneMinMax();
			StageObject.MusicTime = musicTime;
			StageObject.ShowGridOnPlay = m_Game.ShowGridTimerOnPlay.Value;
			StageObject.Solo = (
				SoloOnEditMotion.Value && m_MotionPainter.ItemType >= 0,
				m_MotionPainter.ItemType == 0 ? m_MotionPainter.ItemIndex :
				map != null ? map.GetParentIndex(1, m_MotionPainter.ItemIndex) : -1,
				m_MotionPainter.ItemType == 1 ? m_MotionPainter.ItemIndex : -1
			);
			Object.Stage.StageCount = GetGameItemCount(0);
			Note.CameraWorldPos = m_CameraTF.position;
			TimingNote.ZoneMinMax = m_Zone.GetZoneMinMax(true);
			TimingNote.GameSpeedMuti = dropSpeed;
			TimingNote.MusicTime = musicTime;
			ObjectTimer.MusicTime = musicTime;
			ObjectTimer.SpeedMuti = dropSpeed;
			if (WillUndo) {
				UndoRedo.Undo();
				WillUndo = false;
			}
			if (WillRedo) {
				UndoRedo.Redo();
				WillRedo = false;
			}
		}


		private void OnApplicationQuit () {
			DebugLog.CloseLogStream();
		}


		private void Awake_Message () {
			// Language
			StageProject.GetLanguage = m_Language.Get;
			StageGame.GetLanguage = m_Language.Get;
			StageMenu.GetLanguage = m_Language.Get;
			StageState.GetLanguage = m_Language.Get;
			StageSkin.GetLanguage = m_Language.Get;
			DialogUtil.GetLanguage = m_Language.Get;
			HomeUI.GetLanguage = m_Language.Get;
			ProjectInfoUI.GetLanguage = m_Language.Get;
			TooltipUI.GetLanguage = m_Language.Get;
			ColorPickerUI.GetLanguage = m_Language.Get;
			DialogUI.GetLanguage = m_Language.Get;
			TweenEditorUI.GetLanguage = m_Language.Get;
			ProjectCreatorUI.GetLanguage = m_Language.Get;
			SkinEditorUI.GetLanguage = m_Language.Get;
			SettingUI.GetLanguage = m_Language.Get;
			StageEditor.GetLanguage = m_Language.Get;
			InspectorUI.GetLanguage = m_Language.Get;
			CommandUI.GetLanguage = m_Language.Get;
			TimingInspectorUI.GetLanguage = m_Language.Get;
			NoteInspectorUI.GetLanguage = m_Language.Get;
			// Misc
			TooltipUI.SetTip = (tip) => {
				m_TipLabelA.text = tip;
				m_TipLabelB.text = tip;
			};
			StageProject.LogHint = m_Hint.SetHint;
			StageGame.LogHint = m_Hint.SetHint;
			StageEditor.LogHint = m_Hint.SetHint;
			StageLanguage.OnLanguageLoaded = () => {
				TryRefreshSetting();
				foreach (var text in m_LanguageTexts) {
					if (text != null) {
						text.text = m_Language.Get(text.name);
					}
				}
			};
			DialogUtil.GetRoot = () => m_DialogRoot;
			DialogUtil.GetPrefab = () => m_DialogPrefab;
			TooltipUI.GetHotKey = m_Shortcut.GetHotkeyLabel;
			DebugLog.Init(Util.CombinePaths(Util.GetParentPath(Application.dataPath), "Log"));
		}


		private void Awake_Quit () {
			bool willQuit = false;
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) { return true; }
#endif
				if (willQuit) {
					return true;
				} else {
					DialogUtil.Dialog_OK_Cancel(UI_QuitConfirm, DialogUtil.MarkType.Info, () => {
						willQuit = true;
						Application.Quit();
					});
					return false;
				}
			};
			// Reload Language Texts
			foreach (var text in m_LanguageTexts) {
				if (text != null) {
					text.text = m_Language.Get(text.name);
				}
			}
		}


		private void Awake_Menu () {
			// Grid
			m_Menu.AddCheckerFunc("Menu.Grid.x00", () => m_Game.GridCountX0 == 1);
			m_Menu.AddCheckerFunc("Menu.Grid.x01", () => m_Game.GridCountX0 == 7);
			m_Menu.AddCheckerFunc("Menu.Grid.x02", () => m_Game.GridCountX0 == 15);

			m_Menu.AddCheckerFunc("Menu.Grid.x10", () => m_Game.GridCountX1 == 1);
			m_Menu.AddCheckerFunc("Menu.Grid.x11", () => m_Game.GridCountX1 == 7);
			m_Menu.AddCheckerFunc("Menu.Grid.x12", () => m_Game.GridCountX1 == 15);

			m_Menu.AddCheckerFunc("Menu.Grid.x20", () => m_Game.GridCountX2 == 1);
			m_Menu.AddCheckerFunc("Menu.Grid.x21", () => m_Game.GridCountX2 == 7);
			m_Menu.AddCheckerFunc("Menu.Grid.x22", () => m_Game.GridCountX2 == 15);

			m_Menu.AddCheckerFunc("Menu.Grid.y0", () => m_Game.GridCountY == 1);
			m_Menu.AddCheckerFunc("Menu.Grid.y1", () => m_Game.GridCountY == 2);
			m_Menu.AddCheckerFunc("Menu.Grid.y2", () => m_Game.GridCountY == 4);
			m_Menu.AddCheckerFunc("Menu.Grid.y3", () => m_Game.GridCountY == 8);
			// Auto Save
			m_Menu.AddCheckerFunc("Menu.AutoSave.0", () => Mathf.Abs(m_Project.UI_AutoSaveTime - 30f) < 1f);
			m_Menu.AddCheckerFunc("Menu.AutoSave.1", () => Mathf.Abs(m_Project.UI_AutoSaveTime - 120f) < 1f);
			m_Menu.AddCheckerFunc("Menu.AutoSave.2", () => Mathf.Abs(m_Project.UI_AutoSaveTime - 300f) < 1f);
			m_Menu.AddCheckerFunc("Menu.AutoSave.3", () => Mathf.Abs(m_Project.UI_AutoSaveTime - 600f) < 1f);
			m_Menu.AddCheckerFunc("Menu.AutoSave.Off", () => m_Project.UI_AutoSaveTime < 0f);
			// Beat per Section
			m_Menu.AddCheckerFunc("Menu.Grid.bps0", () => m_Game.BeatPerSection == 2);
			m_Menu.AddCheckerFunc("Menu.Grid.bps1", () => m_Game.BeatPerSection == 3);
			m_Menu.AddCheckerFunc("Menu.Grid.bps2", () => m_Game.BeatPerSection == 4);
			// Abreast Width
			m_Menu.AddCheckerFunc("Menu.Abreast.Width0", () => m_Game.AbreastWidthIndex == 0);
			m_Menu.AddCheckerFunc("Menu.Abreast.Width1", () => m_Game.AbreastWidthIndex == 1);
			m_Menu.AddCheckerFunc("Menu.Abreast.Width2", () => m_Game.AbreastWidthIndex == 2);
			m_Menu.AddCheckerFunc("Menu.Abreast.Width3", () => m_Game.AbreastWidthIndex == 3);
			// Command
			m_Menu.AddCheckerFunc("Menu.Command.Target.None", () => CommandUI.TargetIndex == 0);
			m_Menu.AddCheckerFunc("Menu.Command.Target.Stage", () => CommandUI.TargetIndex == 1);
			m_Menu.AddCheckerFunc("Menu.Command.Target.Track", () => CommandUI.TargetIndex == 2);
			m_Menu.AddCheckerFunc("Menu.Command.Target.TrackInside", () => CommandUI.TargetIndex == 3);
			m_Menu.AddCheckerFunc("Menu.Command.Target.Note", () => CommandUI.TargetIndex == 4);
			m_Menu.AddCheckerFunc("Menu.Command.Target.NoteInside", () => CommandUI.TargetIndex == 5);
			m_Menu.AddCheckerFunc("Menu.Command.Target.Timing", () => CommandUI.TargetIndex == 6);
			m_Menu.AddCheckerFunc("Menu.Command.Command.None", () => CommandUI.CommandIndex == 0);
			m_Menu.AddCheckerFunc("Menu.Command.Command.Time", () => CommandUI.CommandIndex == 1);
			m_Menu.AddCheckerFunc("Menu.Command.Command.TimeAdd", () => CommandUI.CommandIndex == 2);
			m_Menu.AddCheckerFunc("Menu.Command.Command.X", () => CommandUI.CommandIndex == 3);
			m_Menu.AddCheckerFunc("Menu.Command.Command.XAdd", () => CommandUI.CommandIndex == 4);
			m_Menu.AddCheckerFunc("Menu.Command.Command.Width", () => CommandUI.CommandIndex == 5);
			m_Menu.AddCheckerFunc("Menu.Command.Command.WidthAdd", () => CommandUI.CommandIndex == 6);
			m_Menu.AddCheckerFunc("Menu.Command.Command.Delete", () => CommandUI.CommandIndex == 7);

		}


		private void Awake_Object () {
			StageObject.TweenEvaluate = (x, index) => m_Project.Tweens[Mathf.Clamp(index, 0, m_Project.Tweens.Count - 1)].curve.Evaluate(x);
			StageObject.PaletteColor = (index) => index < 0 ? new Color32(0, 0, 0, 0) : m_Project.Palette[Mathf.Min(index, m_Project.Palette.Count - 1)];
			StageObject.MaterialZoneID = Shader.PropertyToID("_ZoneMinMax");
			Note.GetFilledTime = m_Game.FillTime;
			Note.GetDropSpeedAt = m_Game.GetDropSpeedAt;
			Note.GetGameDropOffset = (muti) => m_Game.AreaBetween(0f, m_Music.Time, muti);
			Note.GetDropOffset = (time, muti) => m_Game.AreaBetween(0f, time, muti);
			Note.PlayClickSound = m_Music.PlayClickSound;
			Note.PlaySfx = m_SoundFX.PlayFX;
			TimingNote.PlaySfx = m_SoundFX.PlayFX;
			MotionItem.GetZoneMinMax = () => m_Zone.GetZoneMinMax();
			MotionItem.GetBeatmap = () => m_Project.Beatmap;
			MotionItem.GetMusicTime = () => m_Music.Time;
			MotionItem.OnMotionChanged = m_MotionPainter.RefreshFieldUI;
			MotionItem.GetSpeedMuti = () => m_Game.GameDropSpeed;
			MotionItem.GetGridEnabled = () => m_Game.ShowGrid;
			// Sorting Layer ID
			StageObject.SortingLayerID_Gizmos = SortingLayer.NameToID("Gizmos");
			Object.Stage.SortingLayerID_Stage = SortingLayer.NameToID("Stage");
			Object.Stage.SortingLayerID_Judge = SortingLayer.NameToID("Judge");
			Track.SortingLayerID_TrackTint = SortingLayer.NameToID("TrackTint");
			Track.SortingLayerID_Track = SortingLayer.NameToID("Track");
			Track.SortingLayerID_Tray = SortingLayer.NameToID("Tray");
			Note.SortingLayerID_Pole_Front = SortingLayer.NameToID("Pole Front");
			Note.SortingLayerID_Pole_Back = SortingLayer.NameToID("Pole Back");
			Note.SortingLayerID_Note_Hold = SortingLayer.NameToID("HoldNote");
			Note.SortingLayerID_Note = SortingLayer.NameToID("Note");
			TimingNote.SortingLayerID_UI = SortingLayer.NameToID("UI");
			Luminous.SortingLayerID_Lum = SortingLayer.NameToID("Luminous");
			Note.LayerID_Note = LayerMask.NameToLayer("Note");
			Note.LayerID_Note_Hold = LayerMask.NameToLayer("HoldNote");
		}


		private void Awake_Project () {

			// Project
			StageProject.OnProjectLoadingStart = () => {
				m_Music.SetClip(null);
				m_SoundFX.SetClip(null);
				m_Game.SetSpeedCurveDirty();
				m_Preview.SetDirty();
				UI_RemoveUI();
				UndoRedo.ClearUndo();
				StageObject.Beatmap = TimingNote.Beatmap = ObjectTimer.Beatmap = null;
				m_Game.SetAbreastIndex(0);
				m_Game.SetUseAbreastView(false);
				m_Game.SetGameDropSpeed(1f);
				m_Inspector.RefreshUI();
				m_EasterEgg.CheckEasterEggs();
			};
			StageProject.OnProjectLoaded = () => {
				m_Game.SetSpeedCurveDirty();
				m_Music.Pitch = 1f;
				m_Music.Seek(0f);
				UI_RemoveUI();
				RefreshLoading(-1f);
				DebugLog_Project("Loaded");
			};
			StageProject.OnProjectSavingStart = () => {
				StartCoroutine(SaveProgressing());
			};
			StageProject.OnProjectClosed = () => {
				m_Game.SetSpeedCurveDirty();
				m_Game.SetUseAbreastView(false);
				m_Game.SetGameDropSpeed(1f);
				m_Music.Pitch = 1f;
				m_Music.SetClip(null);
				m_SoundFX.SetClip(null);
				UndoRedo.ClearUndo();
				StageObject.Beatmap = TimingNote.Beatmap = ObjectTimer.Beatmap = null;
				m_Preview.SetDirty();
				m_Editor.ClearSelection();
				m_Inspector.RefreshUI();
				UI_RemoveUI();
			};

			// Beatmap
			StageProject.OnBeatmapOpened = (map, key) => {
				if (!(map is null)) {
					m_Game.BPM = map.BPM;
					m_Game.Shift = map.Shift;
					m_Game.Ratio = map.Ratio;
					m_BeatmapSwiperLabel.text = map.Tag;
					StageObject.Beatmap = TimingNote.Beatmap = ObjectTimer.Beatmap = map;
				}
				TryRefreshProjectInfo();
				RefreshLoading(-1f);
				m_Editor.ClearSelection();
				m_Game.SetSpeedCurveDirty();
				m_Game.ClearAllContainers();
				m_Music.Pitch = 1f;
				m_Music.Seek(0f);
				UndoRedo.ClearUndo();
				UndoRedo.SetDirty();
				m_Preview.SetDirty();
				Resources.UnloadUnusedAssets();
				RefreshGridRenderer();
				RefreshOnItemChange();
				m_Inspector.RefreshUI();
				DebugLog_Beatmap("Open");
			};
			StageProject.OnBeatmapRemoved = () => {
				TryRefreshProjectInfo();
				m_Inspector.RefreshUI();
			};
			StageProject.OnBeatmapCreated = () => {
				TryRefreshProjectInfo();
				m_Inspector.RefreshUI();
			};

			// Assets
			StageProject.OnMusicLoaded = (clip) => {
				m_Music.SetClip(clip);
				m_SoundFX.SetClip(clip);
				TryRefreshProjectInfo();
				m_Wave.LoadWave(clip);
				m_Music.Pitch = 1f;
				m_Music.Seek(0f);
			};
			StageProject.OnBackgroundLoaded = (sprite) => {
				try {
					m_Background.SetBackground(sprite);
				} catch { }
				TryRefreshProjectInfo();

			};
			StageProject.OnCoverLoaded = (sprite) => {
				TryRefreshProjectInfo();

			};
			StageProject.OnClickSoundsLoaded = (clips) => {
				m_Music.SetClickSounds(clips);
				TryRefreshProjectInfo();

			};

			// Misc
			StageProject.OnDirtyChanged = m_DirtyMark.gameObject.SetActive;
			StageProject.OnLoadProgress = RefreshLoading;

			// Func
			IEnumerator SaveProgressing () {
				float pg = 0f;
				m_Hint.SetProgress(0f);
				while (m_Project.SavingProject) {
					yield return new WaitUntil(() => m_Project.SavingProgress != pg);
					pg = m_Project.SavingProgress;
					m_Hint.SetProgress(pg);
				}
				m_Hint.SetProgress(-1f);
			}

		}


		private void Awake_Game () {
			StageGame.OnItemCountChanged = () => {
				m_Preview.SetDirty();
				m_TimingPreview.SetDirty();
				m_Editor.ClearSelection();
			};
			StageGame.OnAbreastChanged = () => {
				m_AbreastTGMark.enabled = m_Game.UseAbreast;
				m_Wave.SetAlpha(m_Game.AbreastValue);
				m_TimingPreview.SetDirty();
				if (m_Editor.SelectingBrushIndex != -1) {
					m_Editor.SetBrush(-1);
				}
				if (m_Editor.SelectingItemIndex != -1) {
					m_Editor.ClearSelection();
				}
				Note.SetCacheDirty();
				TimingNote.SetCacheDirty();
			};
			StageGame.OnSpeedChanged = () => {
				if (!m_Game.UseDynamicSpeed) {
					m_Wave.Length01 = 1f / m_Game.GameDropSpeed / m_Music.Duration;
				}
				m_TimingPreview.SetDirty();
				Note.SetCacheDirty();
				TimingNote.SetCacheDirty();
				RefreshGridRenderer();
			};
			StageGame.OnGridChanged = () => {
				m_GridTG.isOn = m_Game.ShowGrid;
				m_GridRenderer.SetShow(m_Game.ShowGrid);
				m_TimingPreview.SetDirty();
				Track.BeatPerSection = m_Game.BeatPerSection;
				StageObject.ShowGrid = m_Game.ShowGrid;
				RefreshGridRenderer();
			};
			StageGame.OnRatioChanged = (ratio) => {
				m_Zone.SetFitterRatio(ratio);
				m_TimingPreview.SetDirty();
				RefreshGridRenderer();
				if (!m_Game.UseDynamicSpeed) {
					m_Wave.Length01 = 1f / m_Game.GameDropSpeed / m_Music.Duration;
				}
			};
			StageGame.GetBeatmap = () => m_Project.Beatmap;
			StageGame.GetMusicTime = () => m_Music.Time;
			StageGame.MusicSeek = m_Music.Seek;
			StageGame.MusicIsPlaying = () => m_Music.IsPlaying;
			StageGame.GetPitch = () => m_Music.Pitch;
			StageGame.SetPitch = (p) => m_Music.Pitch = p;
			StageGame.MusicPlay = m_Music.Play;
			StageGame.MusicPause = m_Music.Pause;
			StageGame.GetItemLock = m_Editor.GetItemLock;
		}


		private void Awake_Music () {
			StageMusic.OnMusicPlayPause = (playing) => {
				m_Progress.RefreshControlUI();
				m_TimingPreview.SetDirty();
				m_GridRenderer.MusicTime = m_Music.Time;
				StageObject.MusicPlaying = playing;
				TimingNote.MusicPlaying = playing;
				m_SoundFX.StopAllFx();
			};
			StageMusic.OnMusicTimeChanged = (time, duration) => {
				m_Progress.SetProgress(time);
				m_Wave.Time01 = time / duration;
				m_GridRenderer.MusicTime = time;
				m_TimingPreview.SetDirty();
				m_MotionPainter.TrySetDirty();
				StageObject.MusicDuration = duration;
			};
			StageMusic.OnMusicClipLoaded = () => {
				m_GridRenderer.MusicTime = m_Music.Time;
				m_Progress.RefreshControlUI();
				m_TimingPreview.SetDirty();
			};
			StageMusic.OnPitchChanged = () => {
				m_PitchWarningBlock.gameObject.SetActive(m_Music.Pitch < 0.05f);
				m_PitchTrebleClef.gameObject.SetActive(Mathf.Abs(m_Music.Pitch) >= 0.999f);
				m_PitchBassClef.gameObject.SetActive(!m_PitchTrebleClef.gameObject.activeSelf);
				m_SoundFX.StopAllFx();
			};
		}


		private void Awake_Sfx () {
			StageSoundFX.GetMusicPlaying = () => m_Music.IsPlaying;
			StageSoundFX.GetMusicTime = () => m_Music.Time;
			StageSoundFX.GetMusicVolume = () => SliderItemMap[SliderType.MusicVolume].saving.Value / 12f;
			StageSoundFX.SetMusicVolume = (volume) => m_Music.Volume = SliderItemMap[SliderType.MusicVolume].saving.Value / 12f * volume;
			StageSoundFX.GetMusicPitch = () => m_Music.Pitch;
			StageSoundFX.GetMusicMute = () => m_Music.Mute;
			StageSoundFX.SetMusicMute = (mute) => m_Music.Mute = mute;
			StageSoundFX.GetSecondPerBeat = () => m_Game.SPB;
			StageSoundFX.OnUseFxChanged = () => m_Music.UseMixer(m_SoundFX.UseFX);

		}


		private void Awake_Editor () {
			StageEditor.GetZoneMinMax = () => m_Zone.GetZoneMinMax();
			StageEditor.GetRealZoneMinMax = () => m_Zone.GetZoneMinMax(true);
			StageEditor.OnSelectionChanged = () => {
				m_Preview.SetDirty();
				m_Inspector.RefreshUI();
				if (m_MotionPainter.ItemType >= 0) {
					if (m_Editor.SelectingItemType >= 0 && m_MotionPainter.ItemType == m_Editor.SelectingItemType) {
						m_MotionPainter.ItemIndex = m_Editor.SelectingItemIndex;
						m_MotionPainter.SetVerticesDirty();
					} else {
						m_Inspector.StopEditMotion(false);
					}
				}
			};
			StageEditor.OnBrushChanged = () => {
				m_SelectBrushMark.gameObject.TrySetActive(m_Editor.SelectingBrushIndex == -1);
				m_EraseBrushMark.gameObject.TrySetActive(m_Editor.SelectingBrushIndex == -2);
				m_GlobalBrushMark.gameObject.TrySetActive(m_Editor.UseGlobalBrushScale.Value);
			};
			StageEditor.OnLockEyeChanged = () => {
				m_Editor.SetSelection(m_Editor.SelectingItemType, m_Editor.SelectingItemIndex, m_Editor.SelectingItemSubIndex);
				m_Editor.SetBrush(m_Editor.SelectingBrushIndex);
				m_TimingPreview.SetDirty();
			};
			StageEditor.GetBeatmap = () => m_Project.Beatmap;
			StageEditor.GetEditorActive = () =>
				m_Project.Beatmap != null &&
				!m_Music.IsPlaying &&
				!m_MotionInspector.gameObject.activeSelf;
			StageEditor.GetUseDynamicSpeed = () => m_Game.UseDynamicSpeed;
			StageEditor.GetUseAbreast = () => m_Game.UseAbreast;
			StageEditor.GetMoveAxisHovering = m_Axis.GetEntering;
			StageEditor.BeforeObjectEdited = (editType, itemType, itemIndex) => {
				if (editType == StageEditor.EditType.Delete) {
					m_Effect.SpawnDeleteEffect(itemType, itemIndex);
				}
			};
			StageEditor.OnObjectEdited = (editType, itemType, itemIndex) => {
				RefreshOnItemChange();
				UndoRedo.SetDirty();
				m_Inspector.RefreshAllInspectors();
				m_MotionPainter.TrySetDirty();
				if (editType == StageEditor.EditType.Create) {
					m_Effect.SpawnCreateEffect(itemType, itemIndex);
				}
			};
			StageEditor.GetFilledTime = m_Game.FillTime;
			StageEditor.SetAbreastIndex = m_Game.SetAbreastIndex;
			StageEditor.LogAxisMessage = m_Axis.LogAxisMessage;
			StageEditor.GetMusicTime = () => m_Music.Time;
			StageEditor.GetMusicDuration = () => m_Music.Duration;
			StageEditor.GetSnapedTime = m_Game.SnapTime;
		}


		private void Awake_Skin () {
			StageSkin.OnSkinLoaded = (data) => {
				TryRefreshSetting();
				StageObject.LoadSkin(data);
				Luminous.SetLuminousSkin(data);
				Resources.UnloadUnusedAssets();
				m_Game.ClearAllContainers();
				m_SkinSwiperLabel.text = StageSkin.Data.Name;
			};
			StageSkin.OnSkinDeleted = () => {
				TryRefreshSetting();
				m_Game.ClearAllContainers();
			};
		}


		private void Awake_Undo () {
			UndoRedo.GetStepData = () => new UndoData() {
				Map = m_Project.Beatmap,
				MusicTime = m_Music.Time,
				ContainerActive = new bool[4] {
					m_Editor.GetContainerActive(0),
					m_Editor.GetContainerActive(1),
					m_Editor.GetContainerActive(2),
					m_Editor.GetContainerActive(3),
				},
			};
			UndoRedo.OnUndoRedo = (stepObj) => {
				var step = stepObj as UndoData;
				if (step == null || step.Map == null) { return; }
				// Map
				m_Project.Beatmap.LoadFromOtherMap(step.Map);
				// Music Time
				m_Music.Seek(step.MusicTime);
				// Container Active
				for (int i = 0; i < step.ContainerActive.Length; i++) {
					m_Editor.SetContainerActive(i, step.ContainerActive[i]);
				}
				// Final
				m_Editor.ClearSelection();
				RefreshOnItemChange();
			};
		}


		private void Awake_ProjectInfo () {

			ProjectInfoUI.MusicStopClickSounds = m_Music.StopClickSounds;
			ProjectInfoUI.MusicPlayClickSound = m_Music.PlayClickSound;
			ProjectInfoUI.OpenMenu = m_Menu.OpenMenu;
			ProjectInfoUI.ProjectImportPalette = m_Project.UI_ImportPalette;
			ProjectInfoUI.ProjectSaveProject = m_Project.SaveProject;
			ProjectInfoUI.ProjectSetDirty = m_Project.SetDirty;
			ProjectInfoUI.ProjectNewBeatmap = m_Project.NewBeatmap;
			ProjectInfoUI.ProjectImportBeatmap = m_Project.UI_ImportBeatmap;
			ProjectInfoUI.ProjectAddPaletteColor = m_Project.UI_AddPaletteColor;
			ProjectInfoUI.ProjectExportPalette = m_Project.UI_ExportPalette;
			ProjectInfoUI.ProjectImportClickSound = m_Project.ImportClickSound;
			ProjectInfoUI.ProjectAddTween = m_Project.UI_AddTween;
			ProjectInfoUI.ProjectImportTween = m_Project.UI_ImportTween;
			ProjectInfoUI.ProjectExportTween = m_Project.UI_ExportTween;
			ProjectInfoUI.GetBeatmapMap = () => m_Project.BeatmapMap;
			ProjectInfoUI.ProjectSetPaletteColor = m_Project.SetPaletteColor;
			ProjectInfoUI.GetProjectPalette = () => m_Project.Palette;
			ProjectInfoUI.GetProjectTweens = () => m_Project.Tweens;
			ProjectInfoUI.SetProjectTweenCurve = m_Project.SetTweenCurve;
			ProjectInfoUI.GetProjectClickSounds = () => m_Project.ClickSounds;
			ProjectInfoUI.GetProjectInfo = () => (
				m_Project.ProjectName, m_Project.ProjectDescription,
				m_Project.BeatmapAuthor, m_Project.MusicAuthor, m_Project.BackgroundAuthor,
				m_Project.Background.sprite, m_Project.FrontCover.sprite, m_Project.Music.data
			);
			ProjectInfoUI.GetBeatmapKey = () => m_Project.BeatmapKey;
			ProjectInfoUI.ProjectImportBackground = m_Project.ImportBackground;
			ProjectInfoUI.ProjectImportCover = m_Project.ImportCover;
			ProjectInfoUI.ProjectImportMusic = m_Project.ImportMusic;
			ProjectInfoUI.ProjectRemoveBackground = m_Project.RemoveBackground;
			ProjectInfoUI.ProjectRemoveCover = m_Project.RemoveCover;
			ProjectInfoUI.ProjectRemoveMusic = m_Project.RemoveMusic;
			ProjectInfoUI.SetProjectInfo_Name = (name) => m_Project.ProjectName = name;
			ProjectInfoUI.SetProjectInfo_Description = (des) => m_Project.ProjectDescription = des;
			ProjectInfoUI.SetProjectInfo_BgAuthor = (author) => m_Project.BackgroundAuthor = author;
			ProjectInfoUI.SetProjectInfo_MusicAuthor = (author) => m_Project.MusicAuthor = author;
			ProjectInfoUI.SetProjectInfo_MapAuthor = (author) => m_Project.BeatmapAuthor = author;
			ProjectInfoUI.SpawnColorPicker = SpawnColorPicker;
			ProjectInfoUI.SpawnTweenEditor = SpawnTweenEditor;
			ProjectInfoUI.OnBeatmapInfoChanged = () => {
				RefreshOnBeatmapInfoChange();
				Invoke("TryRefreshProjectInfo", 0.01f);
			};
			ProjectInfoUI.OnProjectInfoChanged = () => {
				TryRefreshProjectInfo();
			};

		}


		private void Awake_Setting () {
			SettingUI.ResetAllSettings = () => {
				ResetAllSettings();
				LoadAllSettings();
			};
			SettingUI.SkinRefreshAllSkinNames = m_Skin.RefreshAllSkinNames;
			SettingUI.LanguageGetDisplayName = m_Language.GetDisplayName;
			SettingUI.LanguageGetDisplayName_Language = m_Language.GetDisplayName;
			SettingUI.GetAllLanguages = () => m_Language.AllLanguages;
			SettingUI.GetAllSkinNames = () => m_Skin.AllSkinNames;
			SettingUI.GetSkinName = () => StageSkin.Data.Name;
			SettingUI.SkinLoadSkin = m_Skin.LoadSkin;
			SettingUI.SkinDeleteSkin = m_Skin.UI_DeleteSkin;
			SettingUI.SkinNewSkin = m_Skin.UI_NewSkin;
			SettingUI.OpenMenu = m_Menu.OpenMenu;
			SettingUI.ShortcutCount = () => m_Shortcut.Datas.Length;
			SettingUI.GetShortcutAt = (i) => {
				var data = m_Shortcut.Datas[i];
				return (data.Name, data.Key, data.Ctrl, data.Shift, data.Alt);
			};
			SettingUI.SaveShortcut = () => {
				m_Shortcut.SaveToFile();
				m_Shortcut.ReloadMap();
			};
			SettingUI.SetShortcut = (index, key, ctrl, shift, alt) => {
				var data = m_Shortcut.Datas[index];
				data.Key = key;
				data.Ctrl = ctrl;
				data.Shift = shift;
				data.Alt = alt;
				return index;
			};
			SettingUI.SpawnSkinEditor = SpawnSkinEditor;
			SettingUI.LoadLanguage = m_Language.LoadLanguage;

		}


		private void Awake_Inspector () {

			// Stage
			StageCommand.OnCommandDone = () => {
				RefreshOnItemChange();
				UndoRedo.SetDirty();
			};

			// Inspector
			InspectorUI.GetSelectingType = () => {
				if (m_Editor.SelectingItemType == 4) {
					return 0;
				}
				if (m_Editor.SelectingItemType == 5) {
					return 1;
				}
				return m_Editor.SelectingItemType;
			};
			InspectorUI.GetSelectingIndex = () => m_Editor.SelectingItemIndex;
			InspectorUI.GetBeatmap = () => m_Project.Beatmap;
			InspectorUI.GetBPM = () => m_Game.BPM;
			InspectorUI.GetShift = () => m_Game.Shift;
			InspectorUI.OnItemEdited = () => {
				RefreshOnItemChange();
				UndoRedo.SetDirty();
			};
			InspectorUI.OnBeatmapEdited = () => RefreshOnBeatmapInfoChange();
			InspectorUI.GetProjectTweens = () => m_Project.Tweens;

			// CMD
			CommandUI.DoCommand = (type, command, index, value) => {
				bool success = StageCommand.DoCommand(
					m_Project.Beatmap,
					(StageCommand.TargetType)type,
					(StageCommand.CommandType)command,
					index, value
				);
				if (success) {
					m_Hint.SetHint(m_Language.Get(Hint_CommandDone), true);
				}
			};
			CommandUI.OpenMenu = m_Menu.OpenMenu;

			// Motion 
			MotionPainterUI.GetBeatmap = () => m_Project.Beatmap;
			MotionPainterUI.GetMusicTime = () => m_Music.Time;
			MotionPainterUI.GetMusicDuration = () => m_Music.Duration;
			MotionPainterUI.GetBPM = () => m_Game.BPM;
			MotionPainterUI.GetBeatPerSection = () => m_Game.BeatPerSection.Value;
			MotionPainterUI.OnItemEdit = () => {
				RefreshOnItemChange();
				UndoRedo.SetDirty();
			};
			MotionPainterUI.OnSelectionChanged = () => {
				m_Inspector.CloseTweenSelector();
			};
			MotionPainterUI.GetSprite = m_TextSheet.Char_to_Sprite;
			MotionPainterUI.GetPaletteCount = () => m_Project.Palette.Count;
			MotionPainterUI.SeekMusic = m_Music.Seek;

		}


		private void Awake_Misc () {

			m_EasterEgg.Workspace = m_Project.Workspace;

			CursorUI.GetCursorTexture = (index) => (
				index >= 0 ? m_Cursors[index].Cursor : null,
				index >= 0 ? m_Cursors[index].Offset : Vector2.zero
			);

			ProgressUI.GetSnapTime = (time, step) => m_Game.SnapTime(time, step);

			GridRenderer.GetAreaBetween = (timeA, timeB, muti, ignoreDy) => ignoreDy ? Mathf.Abs(timeA - timeB) * muti : m_Game.AreaBetween(timeA, timeB, muti);
			GridRenderer.GetSnapedTime = m_Game.SnapTime;

			TrackSectionRenderer.GetAreaBetween = m_Game.AreaBetween;
			TrackSectionRenderer.GetSnapedTime = m_Game.SnapTime;

			BeatmapSwiperUI.GetBeatmapMap = () => m_Project.BeatmapMap;
			BeatmapSwiperUI.TriggerSwitcher = (key) => {
				m_Project.SaveProject();
				m_Project.OpenBeatmap(key);
			};

			HomeUI.GotoEditor = m_State.GotoEditor;
			HomeUI.GetWorkspace = () => m_Project.Workspace;
			HomeUI.OpenMenu = m_Menu.OpenMenu;
			HomeUI.SpawnProjectCreator = SpawnProjectCreator;

			ProgressUI.GetDuration = () => m_Music.Duration;
			ProgressUI.GetReadyPlay = () => (m_Music.IsReady, m_Music.IsPlaying);
			ProgressUI.PlayMusic = m_Music.Play;
			ProgressUI.PauseMusic = m_Music.Pause;
			ProgressUI.SeekMusic = m_Music.Seek;
			ProgressUI.GetBPM = () => m_Game.BPM;
			ProgressUI.GetShift = () => m_Game.Shift;

			PreviewUI.GetMusicTime01 = (time) => time / m_Music.Duration;
			PreviewUI.GetBeatmap = () => m_Project.Beatmap;

			ProjectCreatorUI.ImportMusic = () => {
				m_Project.ImportMusic((data, _) => {
					if (data is null) { return; }
					ProjectCreatorUI.MusicData = data;
					ProjectCreatorUI.SetMusicSizeDirty();
				});
			};
			ProjectCreatorUI.GotoEditor = m_State.GotoEditor;

			SkinEditorUI.MusicSeek_Add = (add) => m_Music.Seek(m_Music.Time + add);
			SkinEditorUI.SkinReloadSkin = m_Skin.ReloadSkin;
			SkinEditorUI.OpenMenu = m_Menu.OpenMenu;
			SkinEditorUI.SkinGetPath = m_Skin.GetPath;
			SkinEditorUI.SkinSaveSkin = m_Skin.SaveSkin;
			SkinEditorUI.SpawnSetting = UI_SpawnSetting;
			SkinEditorUI.RemoveUI = UI_RemoveUI;

			SkinSwiperUI.GetAllSkinNames = () => m_Skin.AllSkinNames;
			SkinSwiperUI.SkinLoadSkin = m_Skin.LoadSkin;

			TimingPreviewUI.GetBeatmap = () => m_Project.Beatmap;
			TimingPreviewUI.GetMusicTime = () => m_Music.Time;
			TimingPreviewUI.GetSpeedMuti = () => m_Game.GameDropSpeed;
			TimingPreviewUI.ShowPreview = () => m_Game.ShowGrid && m_Editor.GetContainerActive(3);

			AxisHandleUI.GetZoneMinMax = () => m_Zone.GetZoneMinMax(true);

			SelectorUI.GetBeatmap = () => m_Project.Beatmap;
			SelectorUI.SelectStage = (index) => {
				m_Editor.SetSelection(0, index);
				var map = m_Project.Beatmap;
				if (map != null && !map.GetActive(0, index)) {
					m_Music.Seek(map.GetTime(0, index));
				}
			};
			SelectorUI.SelectTrack = (index) => {
				m_Editor.SetSelection(1, index);
				var map = m_Project.Beatmap;
				if (map != null && !map.GetActive(1, index)) {
					m_Music.Seek(map.GetTime(1, index));
				}
			};
			SelectorUI.OpenItemMenu = (rt, type) => m_Menu.OpenMenu(type == 0 ? UI_SelectorStageMenu : UI_SelectorTrackMenu, rt);
			SelectorUI.GetSelectionType = () => m_Editor.SelectingItemType;
			SelectorUI.GetSelectionIndex = () => m_Editor.SelectingItemIndex;

			TextRenderer.GetSprite = m_TextSheet.Char_to_Sprite;

			m_GridRenderer.SetSortingLayer(SortingLayer.NameToID("Gizmos"), 0);
			m_VersionLabel.text = $"v{Application.version}";
			m_TextSheet.Init();

			// Tutorial
			if (TutorialOpened.Value) {
				Destroy(m_TutorialBoard.gameObject);
			} else {
				m_TutorialBoard.gameObject.SetActive(true);
				///////////////// No Tutorial Yet ///////////////
				Destroy(m_TutorialBoard.gameObject);
				/////////////////////////////////////////////////
			}

		}


		#endregion




		#region --- API ---


		public void Quit () => Application.Quit();


		public void About () => DialogUtil.Open(
			$"<size=38><b>Stager Studio</b> v{Application.version}</size>\n" +
			"<size=20>" +
			"\nCreated by 楠瓜Moenen\n\n" +
			//"Home     www.stager.studio\n" +
			"Email     moenen6@gmail.com\n" +
			"Twitter   _Moenen\n" +
			"QQ        754100943" +
			"</size>",
			DialogUtil.MarkType.Info, 324, () => { }
		);


		public void GotoWeb () {
			Application.OpenURL("http://www.stager.studio");
			DialogUtil.Open(
				string.Format(GetLanguage(UI_OpenWebMSG), "www.stager.studio"),
				DialogUtil.MarkType.Info,
				() => { }, null, null, null, null
			);
		}


		public void GotoTutorial (bool open) {
			TutorialOpened.Value = true;
			if (m_TutorialBoard) {
				Destroy(m_TutorialBoard.gameObject);
			}
			if (open) {
				Application.OpenURL("");
				DialogUtil.Open(
					string.Format(GetLanguage(UI_OpenWebMSG), ""),
					DialogUtil.MarkType.Info,
					() => { }, null, null, null, null
				);
			}
		}


		public void AddVolume (float delta) {
			SetMusicVolume(Mathf.Clamp01(Util.Snap(m_Music.Volume + delta, 10f)));
			try {
				m_Hint.SetHint(string.Format(m_Language.Get(Hint_Volume), Mathf.RoundToInt(m_Music.Volume * 100f)));
			} catch { }
		}


		public void Undo () => WillUndo = true;


		public void Redo () => WillRedo = true;


		#endregion




		#region --- LGC ---


		// Change
		private void RefreshOnItemChange () {
			Note.SetCacheDirty();
			TimingNote.SetCacheDirty();
			m_Game.SetSpeedCurveDirty();
			m_Project.SetDirty();
			m_Preview.SetDirty();
			m_TimingPreview.SetDirty();
			m_Game.ForceUpdateZone();
		}


		private void RefreshOnBeatmapInfoChange () { ///////////////// Invoking /////////////////
			if (m_Project.Beatmap != null) {
				m_Game.BPM = m_Project.Beatmap.BPM;
				m_Game.Shift = m_Project.Beatmap.Shift;
				m_Game.Ratio = m_Project.Beatmap.Ratio;
				m_BeatmapSwiperLabel.text = m_Project.Beatmap.Tag;
			}
			m_Inspector.RefreshUI();
			RefreshGridRenderer();
		}


		// Try Refresh UI
		private void TryRefreshSetting () {
			var setting = m_SettingRoot.childCount > 0 ? m_SettingRoot.GetChild(0).GetComponent<SettingUI>() : null;
			if (!(setting is null)) {
				setting.Refresh();
			}
		}


		private void TryRefreshProjectInfo () {
			var pInfo = m_ProjectInfoRoot.childCount > 0 ? m_ProjectInfoRoot.GetChild(0).GetComponent<ProjectInfoUI>() : null;
			if (!(pInfo is null)) {
				pInfo.Refresh();
			}
		}


		private void RefreshLoading (float progress01, string hint = "") {
			var loading = m_LoadingRoot.childCount > 0 ? m_LoadingRoot.GetChild(0).GetComponent<LoadingUI>() : null;
			if (loading is null) {
				RemoveLoading();
				loading = Util.SpawnUI(m_LoadingPrefab, m_LoadingRoot, "Loading");
			}
			if (progress01 <= 0f || progress01 >= 1f) {
				RemoveLoading();
			} else {
				loading.SetProgress(progress01, hint);
			}
		}


		private void RefreshGridRenderer () {
			m_GridRenderer.SetCountX(0, m_Game.GridCountX0);
			m_GridRenderer.SetCountX(1, m_Game.GridCountX1);
			m_GridRenderer.SetCountX(2, m_Game.GridCountX2);
			m_GridRenderer.TimeGap = 60f / m_Game.BPM / m_Game.GridCountY;
			m_GridRenderer.TimeOffset = m_Game.Shift;
			m_GridRenderer.GameSpeedMuti = m_Game.GameDropSpeed;
		}


		// Debug Log
		private void DebugLog_Start () {
			if (!DebugLog.UseLog) { return; }
			DebugLog.LogFormat("System", "Start", true);
		}


		private void DebugLog_Project (string type) {
			if (!DebugLog.UseLog) { return; }
			DebugLog.LogFormat(
				"Project", type, true,
				("ProjectName", m_Project.ProjectName),
				("ProjectPath", m_Project.ProjectPath),
				("Workspace", m_Project.Workspace)
			);
		}


		private void DebugLog_Beatmap (string type) {
			if (!DebugLog.UseLog) { return; }
			var map = m_Project.Beatmap;
			if (map != null) {
				DebugLog.LogFormat(
					"Beatmap", type, true,
					("map", map),
					("BeatmapKey", m_Project.BeatmapKey),
					("CreatedTime", map.CreatedTime),
					("Tag", map.tag),
					("Bpm", map.bpm)
				);
			} else {
				DebugLog.LogFormat("Beatmap", type, false, ("Map is Null", null));
			}
		}


		#endregion




	}
}


#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using UndoRedo;


	[CustomEditor(typeof(StagerStudio))]
	public class StagerStudio_Inspector : Editor {



		public override void OnInspectorGUI () {
			if (EditorApplication.isPlaying) {
				LayoutH(() => {
					GUIRect(0, 18);
					if (GUI.Button(GUIRect(48, 18), "⇦")) {
						UndoRedo.Undo();
					}
					Space(2);
					if (GUI.Button(GUIRect(48, 18), "⇨")) {
						UndoRedo.Redo();
					}
					GUIRect(0, 18);
				});
				Space(4);
			}
			base.OnInspectorGUI();
		}



		// UTL
		private Rect GUIRect (float width, float height) => GUILayoutUtility.GetRect(
			width, height,
			GUILayout.ExpandWidth(width == 0),
			GUILayout.ExpandHeight(height == 0)
		);


		private void LayoutH (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = GUI.skin.box;
			}
			if (style != null) {
				GUILayout.BeginHorizontal(style);
			} else {
				GUILayout.BeginHorizontal();
			}
			action();
			GUILayout.EndHorizontal();
		}


		private void Space (float space = 4f) => GUILayout.Space(space);


	}
}
#endif