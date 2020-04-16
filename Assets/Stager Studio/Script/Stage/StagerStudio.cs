namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Audio;
	using UI;
	using Stage;
	using Object;
	using Rendering;
	using Data;



	public partial class StagerStudio : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable]
		public struct CursorData {
			public Texture2D Cursor;
			public Vector2 Offset;
		}


		[System.Serializable]
		private class UndoData {
			public byte[] Beatmap;
			public float MusicTime;
			public bool[] ContainerActive;
		}


		// Handler
		public delegate SkinData SkinDataStringHandler (string name);
		public delegate Beatmap BeatmapHandler ();
		public delegate void VoidHandler ();
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
		private const string UI_OpenWebMSG = "Dialog.OpenWebMSG";
		private const string UI_InfoLabel = "UI.InfoLabel";
		private const string Confirm_DeleteProjectPal = "ProjectInfo.Dialog.DeletePal";
		private const string Confirm_DeleteProjectSound = "ProjectInfo.Dialog.DeleteSound";
		private const string Confirm_DeleteProjectTween = "ProjectInfo.Dialog.DeleteTween";
		private const string Dialog_SelectingLayerLocked = "Dialog.Library.SelectingLayerLocked";

		// Handler
		private SkinDataStringHandler GetSkinFromDisk { get; set; } = null;
		private StringStringHandler GetLanguage { get; set; } = null;
		private FloatFloatHandler GetDropMapSpeed { get; set; } = null;
		private VoidIntHandler ProjectRemoveClickSound { get; set; } = null;
		private VoidIntHandler ProjectRemoveTweenAt { get; set; } = null;
		private VoidIntHandler ProjectRemovePaletteAt { get; set; } = null;
		private AbreastHandler GetAbreastData { get; set; } = null;
		private IntIntHandler GetGameItemCount { get; set; } = null;
		private FloatHandler GetMusicTime { get; set; } = null;
		private VoidHandler MusicPause { get; set; } = null;
		private BeatmapHandler GetBeatmap { get; set; } = null;

		// Ser
		[Header("Misc")]
		[SerializeField] private Transform m_CanvasRoot = null;
		[SerializeField] private RectTransform m_DirtyMark = null;
		[SerializeField] private Text m_BeatmapSwiperLabel = null;
		[SerializeField] private Text m_SkinSwiperLabel = null;
		[SerializeField] private Image m_AbreastTGMark = null;
		[SerializeField] private Toggle m_GridTG = null;
		[SerializeField] private Text m_AuthorLabel = null;
		[SerializeField] private Text m_InfoLabel = null;
		[SerializeField] private Text m_VersionLabel = null;
		[SerializeField] private GridRenderer m_GridRenderer = null;
		[SerializeField] private RectTransform m_PitchWarningBlock = null;
		[SerializeField] private RectTransform m_Keypress = null;
		[SerializeField] private Transform m_CameraTF = null;
		[Header("UI")]
		[SerializeField] private BackgroundUI m_Background = null;
		[SerializeField] private ProgressUI m_Progress = null;
		[SerializeField] private HintBarUI m_Hint = null;
		[SerializeField] private AbreastSwitcherUI m_AbreastSwitcherUI = null;
		[SerializeField] private ZoneUI m_Zone = null;
		[SerializeField] private PreviewUI m_Preview = null;
		[SerializeField] private WaveUI m_Wave = null;
		[SerializeField] private TimingPreviewUI m_TimingPreview = null;
		[Header("Data")]
		[SerializeField] private Text[] m_LanguageTexts = null;
		[SerializeField] private CursorData[] m_Cursors = null;


		#endregion




		#region --- MSG ---


		private void Awake () {

			var music = FindObjectOfType<StageMusic>();
			var project = FindObjectOfType<StageProject>();
			var game = FindObjectOfType<StageGame>();
			var sfx = FindObjectOfType<StageSoundFX>();
			var editor = FindObjectOfType<StageEditor>();
			var library = FindObjectOfType<StageLibrary>();
			var language = FindObjectOfType<StageLanguage>();
			var skin = FindObjectOfType<StageSkin>();
			var shortcut = FindObjectOfType<StageShortcut>();
			var menu = FindObjectOfType<StageMenu>();
			var state = FindObjectOfType<StageState>();

			GetSkinFromDisk = skin.GetSkinFromDisk;
			MusicPause = music.Pause;
			GetMusicTime = () => music.Time;
			ProjectRemoveClickSound = project.RemoveClickSound;
			ProjectRemoveTweenAt = project.RemoveTweenAt;
			ProjectRemovePaletteAt = project.RemovePaletteAt;
			GetAbreastData = () => (game.AbreastValue, game.AbreastIndex, game.AbreastWidth);
			GetDropMapSpeed = () => (game.GameDropSpeed, game.MapDropSpeed);
			GetGameItemCount = game.GetItemCount;
			GetBeatmap = () => project.Beatmap;
			GetLanguage = language.Get;

			Awake_Message(language, shortcut);
			Awake_Setting(skin, language, shortcut, menu);
			Awake_Setting_UI(sfx, music, game, editor);
			Awake_Menu(menu, game, project);
			Awake_Object(project, game, music, sfx);
			Awake_Project(project, editor, game, music, sfx);
			Awake_Game(project, game, music, library);
			Awake_Music(game, music, sfx);
			Awake_Sfx(music, game, sfx);
			Awake_Editor(editor, game, library, project, music);
			Awake_Library(project, editor, library, menu, game, music);
			Awake_Skin(game);
			Awake_Tutorial(state);
			Awake_Undo(project, editor, music);
			Awake_ProjectInfo(project, game, music, menu);
			Awake_UI(project, game, editor, skin, state, menu, music);

		}


		private void Start () {
			LoadAllSettings();
			UI_RemoveUI();
			if (Screen.fullScreen) {
				Screen.fullScreen = false;
			}
			QualitySettings.vSyncCount = 0;
		}


		private void Update () {
			var (aValue, aIndex, aWidth) = GetAbreastData();
			var (dropSpeed, mapSpeed) = GetDropMapSpeed();
			CursorUI.GlobalUpdate();
			StageUndo.GlobalUpdate();
			StageObject.ZoneMinMax = m_Zone.GetZoneMinMax();
			StageObject.Abreast = (aIndex, aValue, aWidth);
			StageObject.ScreenZoneMinMax = m_Zone.GetScreenZoneMinMax();
			StageObject.GameSpeedMuti = dropSpeed * mapSpeed;
			StageObject.MusicTime = GetMusicTime();
			Note.CameraWorldPos = m_CameraTF.position;
			TimingNote.ZoneMinMax = m_Zone.GetZoneMinMax(true);
			TimingNote.GameSpeedMuti = dropSpeed * mapSpeed;
			TimingNote.MusicTime = GetMusicTime();
			Object.Stage.StageCount = GetGameItemCount(0);
		}


		private void Awake_Message (StageLanguage language, StageShortcut shortcut) {
			// Language
			StageProject.GetLanguage = language.Get;
			StageGame.GetLanguage = language.Get;
			StageMenu.GetLanguage = language.Get;
			StageState.GetLanguage = language.Get;
			StageSkin.GetLanguage = language.Get;
			DialogUtil.GetLanguage = language.Get;
			HomeUI.GetLanguage = language.Get;
			ProjectInfoUI.GetLanguage = language.Get;
			TooltipUI.GetLanguage = language.Get;
			ColorPickerUI.GetLanguage = language.Get;
			DialogUI.GetLanguage = language.Get;
			TweenEditorUI.GetLanguage = language.Get;
			ProjectCreatorUI.GetLanguage = language.Get;
			SkinEditorUI.GetLanguage = language.Get;
			SettingUI.GetLanguage = language.Get;
			StageLibrary.GetLanguage = language.Get;
			StageEditor.GetLanguage = language.Get;
			// Misc
			TooltipUI.SetTip = m_Hint.SetTip;
			HomeUI.LogHint = m_Hint.SetHint;
			StageProject.LogHint = m_Hint.SetHint;
			StageLibrary.LogHint = m_Hint.SetHint;
			StageGame.LogHint = m_Hint.SetHint;
			StageEditor.LogHint = m_Hint.SetHint;
			StageLanguage.OnLanguageLoaded = () => {
				TryRefreshSetting();
				foreach (var text in m_LanguageTexts) {
					text.text = language.Get(text.name);
				}
			};
			DialogUtil.GetRoot = () => m_DialogRoot;
			DialogUtil.GetPrefab = () => m_DialogPrefab;
			TooltipUI.GetHotKey = shortcut.GetHotkeyLabel;
			// Quit
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
				text.text = language.Get(text.name);
			}
		}


		private void Awake_Menu (StageMenu menu, StageGame game, StageProject project) {
			// Grid
			menu.AddCheckerFunc("Menu.Grid.x00", () => game.GridCountX0 == 1);
			menu.AddCheckerFunc("Menu.Grid.x01", () => game.GridCountX0 == 7);
			menu.AddCheckerFunc("Menu.Grid.x02", () => game.GridCountX0 == 15);

			menu.AddCheckerFunc("Menu.Grid.x10", () => game.GridCountX1 == 1);
			menu.AddCheckerFunc("Menu.Grid.x11", () => game.GridCountX1 == 7);
			menu.AddCheckerFunc("Menu.Grid.x12", () => game.GridCountX1 == 15);

			menu.AddCheckerFunc("Menu.Grid.x20", () => game.GridCountX2 == 1);
			menu.AddCheckerFunc("Menu.Grid.x21", () => game.GridCountX2 == 7);
			menu.AddCheckerFunc("Menu.Grid.x22", () => game.GridCountX2 == 15);

			menu.AddCheckerFunc("Menu.Grid.y0", () => game.GridCountY == 1);
			menu.AddCheckerFunc("Menu.Grid.y1", () => game.GridCountY == 2);
			menu.AddCheckerFunc("Menu.Grid.y2", () => game.GridCountY == 4);
			menu.AddCheckerFunc("Menu.Grid.y3", () => game.GridCountY == 8);
			// Auto Save
			menu.AddCheckerFunc("Menu.AutoSave.0", () => Mathf.Abs(project.UI_AutoSaveTime - 30f) < 1f);
			menu.AddCheckerFunc("Menu.AutoSave.1", () => Mathf.Abs(project.UI_AutoSaveTime - 120f) < 1f);
			menu.AddCheckerFunc("Menu.AutoSave.2", () => Mathf.Abs(project.UI_AutoSaveTime - 300f) < 1f);
			menu.AddCheckerFunc("Menu.AutoSave.3", () => Mathf.Abs(project.UI_AutoSaveTime - 600f) < 1f);
			menu.AddCheckerFunc("Menu.AutoSave.Off", () => project.UI_AutoSaveTime < 0f);
			// Beat per Section
			menu.AddCheckerFunc("Menu.Grid.bps0", () => game.BeatPerSection == 2);
			menu.AddCheckerFunc("Menu.Grid.bps1", () => game.BeatPerSection == 3);
			menu.AddCheckerFunc("Menu.Grid.bps2", () => game.BeatPerSection == 4);
		}


		private void Awake_Object (StageProject project, StageGame game, StageMusic music, StageSoundFX sfx) {
			StageObject.TweenEvaluate = (x, index) => project.Tweens[Mathf.Clamp(index, 0, project.Tweens.Count - 1)].curve.Evaluate(x);
			StageObject.PaletteColor = (index) => index < 0 ? new Color32(0, 0, 0, 0) : project.Palette[Mathf.Min(index, project.Palette.Count - 1)];
			StageObject.MaterialZoneID = Shader.PropertyToID("_ZoneMinMax");
			Note.GetFilledTime = game.FillDropTime;
			Note.GetDropSpeedAt = game.GetDropSpeedAt;
			Note.GetGameDropOffset = (muti) => game.AreaBetweenDrop(music.Time, muti);
			Note.GetDropOffset = game.AreaBetweenDrop;
			Note.PlayClickSound = music.PlayClickSound;
			Note.PlaySfx = sfx.PlayFX;
			TimingNote.PlaySfx = sfx.PlayFX;
			// Sorting Layer ID
			StageObject.SortingLayerID_Gizmos = SortingLayer.NameToID("Gizmos");
			Object.Stage.SortingLayerID_Stage = SortingLayer.NameToID("Stage");
			Object.Stage.SortingLayerID_Judge = SortingLayer.NameToID("Judge");
			Track.SortingLayerID_TrackTint = SortingLayer.NameToID("TrackTint");
			Track.SortingLayerID_Track = SortingLayer.NameToID("Track");
			Track.SortingLayerID_Tray = SortingLayer.NameToID("Tray");
			Note.SortingLayerID_Pole = SortingLayer.NameToID("Pole");
			Note.SortingLayerID_Note_Hold = SortingLayer.NameToID("HoldNote");
			Note.SortingLayerID_Note = SortingLayer.NameToID("Note");
			Note.LayerID_Note = LayerMask.NameToLayer("Note");
			Note.LayerID_Note_Hold = LayerMask.NameToLayer("HoldNote");
			Note.SortingLayerID_Arrow = SortingLayer.NameToID("Arrow");
			TimingNote.SortingLayerID_UI = SortingLayer.NameToID("UI");
			MotionNote.SortingLayerID_Motion = SortingLayer.NameToID("Motion");
			Luminous.SortingLayerID_Lum = SortingLayer.NameToID("Luminous");
		}


		private void Awake_Project (StageProject project, StageEditor editor, StageGame game, StageMusic music, StageSoundFX sfx) {

			// Project
			StageProject.OnProjectLoadingStart = () => {
				music.SetClip(null);
				sfx.SetClip(null);
				game.SetSpeedCurveDirty();
				m_Preview.SetDirty();
				UI_RemoveUI();
				StageUndo.ClearUndo();
				StageObject.Beatmap = TimingNote.Beatmap = null;
				game.SetAbreastIndex(0);
				game.SetUseAbreastView(false);
				game.SetGameDropSpeed(1f);
				RefreshAuthorLabel();
			};
			StageProject.OnProjectLoaded = () => {
				game.SetSpeedCurveDirty();
				music.Pitch = 1f;
				music.Seek(0f);
				UI_RemoveUI();
				RefreshLoading(-1f);
				RefreshAuthorLabel();
				if (ShowWelcome) {
					SpawnWelcome();
				}
			};
			StageProject.OnProjectSavingStart = () => {
				StartCoroutine(SaveProgressing());
			};
			StageProject.OnProjectClosed = () => {
				game.SetSpeedCurveDirty();
				game.SetUseAbreastView(false);
				game.SetGameDropSpeed(1f);
				music.Pitch = 1f;
				music.SetClip(null);
				sfx.SetClip(null);
				StageUndo.ClearUndo();
				m_Preview.SetDirty();
				editor.ClearSelection();
				StageObject.Beatmap = TimingNote.Beatmap = null;
			};

			// Beatmap
			StageProject.OnBeatmapOpened = (map, key) => {
				if (!(map is null)) {
					game.BPM = map.BPM;
					game.Shift = map.Shift;
					game.MapDropSpeed = map.DropSpeed;
					game.Ratio = map.Ratio;
					m_BeatmapSwiperLabel.text = map.Tag;
					StageObject.Beatmap = TimingNote.Beatmap = map;
				}
				TryRefreshProjectInfo();
				RefreshLoading(-1f);
				editor.ClearSelection();
				game.SetSpeedCurveDirty();
				game.ClearAllContainers();
				music.Pitch = 1f;
				music.Seek(0f);
				StageUndo.ClearUndo();
				StageUndo.RegisterUndo();
				m_Preview.SetDirty();
				Resources.UnloadUnusedAssets();
				RefreshGridRenderer(game);
				RefreshInfoLabel();
			};
			StageProject.OnBeatmapRemoved = () => {
				TryRefreshProjectInfo();
			};
			StageProject.OnBeatmapCreated = () => {
				TryRefreshProjectInfo();
			};

			// Assets
			StageProject.OnMusicLoaded = (clip) => {
				music.SetClip(clip);
				sfx.SetClip(clip);
				TryRefreshProjectInfo();
				m_Wave.LoadWave(clip);
				music.Pitch = 1f;
				music.Seek(0f);
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
				music.SetClickSounds(clips);
				TryRefreshProjectInfo();

			};

			// Misc
			StageProject.OnDirtyChanged = m_DirtyMark.gameObject.SetActive;
			StageProject.OnLoadProgress = RefreshLoading;

			// Func
			IEnumerator SaveProgressing () {
				float pg = 0f;
				m_Hint.SetProgress(0f);
				while (project.SavingProject) {
					yield return new WaitUntil(() => project.SavingProgress != pg);
					pg = project.SavingProgress;
					m_Hint.SetProgress(pg);
				}
				m_Hint.SetProgress(-1f);
			}

		}


		private void Awake_Game (StageProject project, StageGame game, StageMusic music, StageLibrary library) {
			StageGame.OnStageObjectChanged = () => {
				m_Preview.SetDirty();
				m_TimingPreview.SetVerticesDirty();
				RefreshInfoLabel();
			};
			StageGame.OnAbreastChanged = () => {
				m_AbreastTGMark.enabled = game.UseAbreast;
				m_Wave.SetAlpha(game.AbreastValue);
				m_AbreastSwitcherUI.SetBarUIDirty();
				m_AbreastSwitcherUI.RefreshHighlightUI();
				m_TimingPreview.SetVerticesDirty();
				if (library.SelectingItemTypeIndex.index != -1) {
					library.UI_SetSelection(-1);
				}
				Note.SetCacheDirty();
				TimingNote.SetCacheDirty();
			};
			StageGame.OnSpeedChanged = () => {
				if (!game.UseDynamicSpeed) {
					m_Wave.Length01 = game.GameDropSpeed * game.MapDropSpeed > 0f ?
						1f / (game.GameDropSpeed * game.MapDropSpeed) / music.Duration :
					0f;
				}
				m_TimingPreview.SetVerticesDirty();
				Note.SetCacheDirty();
				TimingNote.SetCacheDirty();
				RefreshGridRenderer(game);
				RefreshInfoLabel();
			};
			StageGame.OnGridChanged = () => {
				m_GridTG.isOn = game.ShowGrid;
				m_GridRenderer.SetShow(game.ShowGrid);
				m_TimingPreview.SetVerticesDirty();
				Track.BeatPerSection = game.BeatPerSection;
				StageObject.ShowGrid = game.ShowGrid;
				RefreshGridRenderer(game);
			};
			StageGame.OnRatioChanged = (ratio) => {
				m_Zone.SetFitterRatio(ratio);
				m_TimingPreview.SetVerticesDirty();
				Beatmap.DEFAULT_STAGE.Height = 1f / ratio;
			};
			StageGame.GetBeatmap = () => project.Beatmap;
		}


		private void Awake_Music (StageGame game, StageMusic music, StageSoundFX sfx) {
			StageMusic.OnMusicPlayPause = (playing) => {
				m_Progress.RefreshControlUI();
				m_TimingPreview.SetVerticesDirty();
				StageObject.MusicPlaying = playing;
				TimingNote.MusicPlaying = playing;
				sfx.StopAllFx();
			};
			StageMusic.OnMusicTimeChanged = (time, duration) => {
				m_Progress.SetProgress(time, game.BPM);
				m_Wave.Time01 = time / duration;
				m_GridRenderer.MusicTime = time;
				m_TimingPreview.SetVerticesDirty();
				StageObject.MusicDuration = duration;
			};
			StageMusic.OnMusicClipLoaded = () => {
				m_Progress.RefreshControlUI();
				m_TimingPreview.SetVerticesDirty();
			};
			StageMusic.OnPitchChanged = () => {
				m_PitchWarningBlock.gameObject.SetActive(music.Pitch < 0.05f);
				sfx.StopAllFx();
			};
		}


		private void Awake_Sfx (StageMusic music, StageGame game, StageSoundFX sfx) {
			StageSoundFX.GetMusicPlaying = () => music.IsPlaying;
			StageSoundFX.GetMusicTime = () => music.Time;
			StageSoundFX.GetMusicVolume = () => SliderItemMap[SliderType.MusicVolume].saving.Value / 12f;
			StageSoundFX.SetMusicVolume = (volume) => music.Volume = SliderItemMap[SliderType.MusicVolume].saving.Value / 12f * volume;
			StageSoundFX.GetMusicPitch = () => music.Pitch;
			StageSoundFX.GetMusicMute = () => music.Mute;
			StageSoundFX.SetMusicMute = (mute) => music.Mute = mute;
			StageSoundFX.GetSecondPerBeat = () => game.SPB;
			StageSoundFX.OnUseFxChanged = () => music.UseMixer(sfx.UseFX);

		}


		private void Awake_Editor (StageEditor editor, StageGame game, StageLibrary library, StageProject project, StageMusic music) {
			StageEditor.GetZoneMinMax = () => m_Zone.GetZoneMinMax(true);
			StageEditor.OnSelectionChanged = () => {
				library.RefreshAddButton(editor.SelectingCount > 0 && editor.SelectingType == 2);
				m_Preview.SetDirty();
			};
			StageEditor.OnLockEyeChanged = () => {
				editor.ClearSelection();
				m_TimingPreview.SetVerticesDirty();
			};
			StageEditor.GetBrushTypeIndex = () => library.SelectingItemTypeIndex;
			StageEditor.GetBeatmap = () => project.Beatmap;
			StageEditor.GetEditorActive = () => project.Beatmap != null && !music.IsPlaying;
			StageEditor.GetUseDynamicSpeed = () => game.UseDynamicSpeed;
			StageEditor.GetUseAbreast = () => game.UseAbreast;
			StageEditor.GetDefaultStageBrush = () => library.GetDefaultStage;
			StageEditor.GetDefaultTrackBrush = () => library.GetDefaultTrack;
			StageEditor.GetNotesBrushAt = library.GetNotesAt;
		}


		private void Awake_Library (StageProject project, StageEditor editor, StageLibrary library, StageMenu menu, StageGame game, StageMusic music) {
			StageLibrary.OnSelectionChanged = () => {
				var (type, index) = library.SelectingItemTypeIndex;
				if (index >= 0) {
					editor.ClearSelection();
					music.Pause();
				}
				if (index >= 0 && editor.GetItemLock(type)) {
					m_Hint.SetHint(GetLanguage(Dialog_SelectingLayerLocked));
				}
			};
			StageLibrary.GetSelectingObjects = () => {
				int finalType = -1;
				var finalList = new List<object>();
				editor.ForAddSelectingObjects((index) => {
					int type = editor.SelectingType;
					var map = project.Beatmap;
					if (map == null || type >= 3 || index < 0) { return; }
					if (type > finalType) {
						finalType = type;
						finalList.Clear();
					}
					if (type == 0) {
						if (map.Stages != null && index < map.Stages.Count) {
							finalList.Add(map.Stages[index]);
						}
					} else if (type == 1) {
						if (map.Tracks != null && index < map.Tracks.Count) {
							finalList.Add(map.Tracks[index]);
						}
					} else if (type == 2) {
						if (map.Notes != null && index < map.Notes.Count) {
							finalList.Add(map.Notes[index]);
						}
					}
				});
				return (finalType, finalList);
			};
			StageLibrary.GetSelectionCount = () => editor.SelectingCount;
			StageLibrary.OpenMenu = menu.OpenMenu;
			StageLibrary.GetBPM = () => game.BPM;
			StageLibrary.GetAbreastValue = () => game.AbreastValue;
		}


		private void Awake_Skin (StageGame game) {
			StageSkin.OnSkinLoaded = (data) => {
				TryRefreshSetting();
				StageObject.LoadSkin(data);
				Luminous.SetLuminousSkin(data);
				Resources.UnloadUnusedAssets();
				game.ClearAllContainers();
				m_SkinSwiperLabel.text = StageSkin.Data.Name;
			};
			StageSkin.OnSkinDeleted = () => {
				TryRefreshSetting();
				game.ClearAllContainers();
			};
		}


		private void Awake_Tutorial (StageState state) {
			StageTutorial.OpenProjectAt = state.GotoEditor;

		}


		private void Awake_Undo (StageProject project, StageEditor editor, StageMusic music) {
			StageUndo.GetObjectData = () => new UndoData() {
				Beatmap = Util.ObjectToBytes(project.Beatmap),
				MusicTime = music.Time,
				ContainerActive = new bool[5] {
					editor.GetContainerActive(0),
					editor.GetContainerActive(1),
					editor.GetContainerActive(2),
					editor.GetContainerActive(3),
					editor.GetContainerActive(4),
				},
			};
			StageUndo.OnUndo = (bytes) => {
				if (bytes == null) { return; }
				var step = Util.BytesToObject(bytes) as UndoData;
				if (step is null || step.Beatmap is null) { return; }
				// Map
				project.Beatmap.LoadFromBytes(step.Beatmap);
				// Music Time
				music.Seek(step.MusicTime);
				// Container Active
				for (int i = 0; i < step.ContainerActive.Length; i++) {
					editor.SetContainerActive(i, step.ContainerActive[i]);
				}
				// Final
				editor.ClearSelection();
			};
		}


		private void Awake_ProjectInfo (StageProject project, StageGame game, StageMusic music, StageMenu menu) {

			ProjectInfoUI.MusicStopClickSounds = music.StopClickSounds;
			ProjectInfoUI.MusicPlayClickSound = music.PlayClickSound;
			ProjectInfoUI.OpenMenu = menu.OpenMenu;
			ProjectInfoUI.ProjectImportPalette = project.UI_ImportPalette;
			ProjectInfoUI.ProjectSaveProject = project.SaveProject;
			ProjectInfoUI.ProjectSetDirty = project.SetDirty;
			ProjectInfoUI.ProjectNewBeatmap = project.NewBeatmap;
			ProjectInfoUI.ProjectImportBeatmap = project.UI_ImportBeatmap;
			ProjectInfoUI.ProjectAddPaletteColor = project.UI_AddPaletteColor;
			ProjectInfoUI.ProjectExportPalette = project.UI_ExportPalette;
			ProjectInfoUI.ProjectImportClickSound = project.ImportClickSound;
			ProjectInfoUI.ProjectAddTween = project.UI_AddTween;
			ProjectInfoUI.ProjectImportTween = project.UI_ImportTween;
			ProjectInfoUI.ProjectExportTween = project.UI_ExportTween;
			ProjectInfoUI.GetBeatmapMap = () => project.BeatmapMap;
			ProjectInfoUI.ProjectSetPaletteColor = project.SetPaletteColor;
			ProjectInfoUI.GetProjectPalette = () => project.Palette;
			ProjectInfoUI.GetProjectTweens = () => project.Tweens;
			ProjectInfoUI.SetProjectTweenCurve = project.SetTweenCurve;
			ProjectInfoUI.GetProjectClickSounds = () => project.ClickSounds;
			ProjectInfoUI.GetProjectInfo = () => (
				project.ProjectName, project.ProjectDescription,
				project.BeatmapAuthor, project.MusicAuthor, project.BackgroundAuthor,
				project.Background.sprite, project.FrontCover.sprite, project.Music.data
			);
			ProjectInfoUI.GetBeatmapKey = () => project.BeatmapKey;
			ProjectInfoUI.ProjectImportBackground = project.ImportBackground;
			ProjectInfoUI.ProjectImportCover = project.ImportCover;
			ProjectInfoUI.ProjectImportMusic = project.ImportMusic;
			ProjectInfoUI.ProjectRemoveBackground = project.RemoveBackground;
			ProjectInfoUI.ProjectRemoveCover = project.RemoveCover;
			ProjectInfoUI.ProjectRemoveMusic = project.RemoveMusic;
			ProjectInfoUI.SetProjectInfo_Name = (name) => project.ProjectName = name;
			ProjectInfoUI.SetProjectInfo_Description = (des) => project.ProjectDescription = des;
			ProjectInfoUI.SetProjectInfo_BgAuthor = (author) => project.BackgroundAuthor = author;
			ProjectInfoUI.SetProjectInfo_MusicAuthor = (author) => project.MusicAuthor = author;
			ProjectInfoUI.SetProjectInfo_MapAuthor = (author) => project.BeatmapAuthor = author;
			ProjectInfoUI.SpawnColorPicker = SpawnColorPicker;
			ProjectInfoUI.SpawnTweenEditor = SpawnTweenEditor;
			ProjectInfoUI.OnBeatmapInfoChanged = (map) => {
				if (project.Beatmap != null) {
					game.BPM = project.Beatmap.BPM;
					game.Shift = project.Beatmap.Shift;
					game.MapDropSpeed = project.Beatmap.DropSpeed;
					game.Ratio = project.Beatmap.Ratio;
					m_BeatmapSwiperLabel.text = project.Beatmap.Tag;
				}
				RefreshGridRenderer(game);
			};
			ProjectInfoUI.OnProjectInfoChanged = () => {
				RefreshAuthorLabel();
			};

		}


		private void Awake_Setting (StageSkin skin, StageLanguage language, StageShortcut shortcut, StageMenu menu) {
			SettingUI.ResetAllSettings = () => {
				ResetAllSettings();
				LoadAllSettings();
			};
			SettingUI.SkinRefreshAllSkinNames = skin.RefreshAllSkinNames;
			SettingUI.LanguageGetDisplayName = language.GetDisplayName;
			SettingUI.LanguageGetDisplayName_Language = language.GetDisplayName;
			SettingUI.GetAllLanguages = () => language.AllLanguages;
			SettingUI.GetAllSkinNames = () => skin.AllSkinNames;
			SettingUI.GetSkinName = () => StageSkin.Data.Name;
			SettingUI.SkinLoadSkin = skin.LoadSkin;
			SettingUI.SkinDeleteSkin = skin.UI_DeleteSkin;
			SettingUI.SkinNewSkin = skin.UI_NewSkin;
			SettingUI.OpenMenu = menu.OpenMenu;
			SettingUI.ShortcutCount = () => shortcut.Datas.Length;
			SettingUI.GetShortcutAt = (i) => {
				var data = shortcut.Datas[i];
				return (data.Name, data.Key, data.Ctrl, data.Shift, data.Alt);
			};
			SettingUI.SaveShortcut = () => {
				shortcut.SaveToFile();
				shortcut.ReloadMap();
			};
			SettingUI.CheckShortcut = shortcut.CheckShortcut;
			SettingUI.SetShortcut = (index, key, ctrl, shift, alt) => {
				var data = shortcut.Datas[index];
				data.Key = key;
				data.Ctrl = ctrl;
				data.Shift = shift;
				data.Alt = alt;
				return index;
			};
			SettingUI.SpawnSkinEditor = SpawnSkinEditor;


		}


		private void Awake_UI (StageProject project, StageGame game, StageEditor editor, StageSkin skin, StageState state, StageMenu menu, StageMusic music) {

			CursorUI.GetCursorTexture = (index) => (
				index >= 0 ? m_Cursors[index].Cursor : null,
				index >= 0 ? m_Cursors[index].Offset : Vector2.zero
			);

			ProgressUI.GetSnapTime = (time, step) => game.SnapTime(time, step);

			GridRenderer.GetAreaBetween = (timeA, timeB, muti, useDy) => useDy ? game.AreaBetween(timeA, timeB, muti) : Mathf.Abs(timeA - timeB) * muti;
			GridRenderer.GetSnapedTime = game.SnapTime;

			TrackSectionRenderer.GetAreaBetween = game.AreaBetween;
			TrackSectionRenderer.GetSnapedTime = game.SnapTime;

			BeatmapSwiperUI.GetBeatmapMap = () => project.BeatmapMap;
			BeatmapSwiperUI.TriggerSwitcher = (key) => {
				project.SaveProject();
				project.OpenBeatmap(key);
			};

			HomeUI.GotoEditor = state.GotoEditor;
			HomeUI.GetWorkspace = () => project.Workspace;
			HomeUI.OpenMenu = menu.OpenMenu;
			HomeUI.SpawnProjectCreator = SpawnProjectCreator;

			ProgressUI.GetDuration = () => music.Duration;
			ProgressUI.GetReadyPlay = () => (music.IsReady, music.IsPlaying);
			ProgressUI.PlayMusic = music.Play;
			ProgressUI.PauseMusic = music.Pause;
			ProgressUI.SeekMusic = music.Seek;

			AbreastSwitcherUI.SetAbreastIndex = game.SetAbreastIndex;
			AbreastSwitcherUI.GetAbreastIndex = () => game.AbreastIndex;
			AbreastSwitcherUI.GetAbreastValue = () => game.AbreastValue;

			PreviewUI.GetMusicTime01 = (time) => time / music.Duration;
			PreviewUI.GetBeatmap = () => project.Beatmap;

			ProjectCreatorUI.ImportMusic = () => {
				project.ImportMusic((data, _) => {
					if (data is null) { return; }
					ProjectCreatorUI.MusicData = data;
					ProjectCreatorUI.SetMusicSizeDirty();
				});
			};
			ProjectCreatorUI.GotoEditor = state.GotoEditor;

			SkinEditorUI.MusicSeek_Add = (add) => music.Seek(music.Time + add);
			SkinEditorUI.SkinReloadSkin = skin.ReloadSkin;
			SkinEditorUI.OpenMenu = menu.OpenMenu;
			SkinEditorUI.SkinGetPath = skin.GetPath;
			SkinEditorUI.SkinSaveSkin = skin.SaveSkin;
			SkinEditorUI.SpawnSetting = UI_SpawnSetting;
			SkinEditorUI.RemoveUI = UI_RemoveUI;

			SkinSwiperUI.GetAllSkinNames = () => skin.AllSkinNames;
			SkinSwiperUI.SkinLoadSkin = skin.LoadSkin;

			TimingPreviewUI.GetBeatmap = () => project.Beatmap;
			TimingPreviewUI.GetMusicTime = () => music.Time;
			TimingPreviewUI.GetSpeedMuti = () => game.GameDropSpeed * game.MapDropSpeed;
			TimingPreviewUI.ShowPreview = () => game.ShowGrid && editor.GetContainerActive(3);

			m_GridRenderer.SetSortingLayer(SortingLayer.NameToID("Gizmos"), 0);
			m_VersionLabel.text = $"v{Application.version}";
		}


		#endregion




		#region --- API ---


		public void Quit () => Application.Quit();


		public void About () => DialogUtil.Open($"Stager Studio v{Application.version} by 楠瓜Moenen\nEmail moenen6@gmail.com\nTwitter @_Moenen\nQQ 754100943\nwww.stager.studio", DialogUtil.MarkType.Info, () => { });


		public void GotoWeb () {
			Application.OpenURL("http://www.stager.studio");
			DialogUtil.Open(
				string.Format(GetLanguage(UI_OpenWebMSG), "www.stager.studio"),
				DialogUtil.MarkType.Info,
				() => { }, null, null, null, null
			);
		}


		public void Undo () => StageUndo.Undo();


		public void Redo () => StageUndo.Redo();


		#endregion




		#region --- LGC ---


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


		private void RefreshAuthorLabel () {
			var info = ProjectInfoUI.GetProjectInfo();
			var map = GetBeatmap();
			m_AuthorLabel.text = $"{info.name} | {info.mapAuthor} & {info.musicAuthor}, {(map != null ? map.Tag : "")} Lv{(map != null ? map.Level : 0)}";
		}


		private void RefreshInfoLabel () {
			var map = GetBeatmap();
			int stageCount = 0;
			int trackCount = 0;
			int noteCount = 0;
			int speedCount = 0;
			if (map != null) {
				stageCount = map.Stages.Count;
				trackCount = map.Tracks.Count;
				noteCount = map.Notes.Count;
				speedCount = map.TimingNotes.Count;
			}
			try {
				m_InfoLabel.text = string.Format(GetLanguage(UI_InfoLabel), stageCount, trackCount, noteCount, speedCount);
			} catch {
				m_InfoLabel.text = "";
			}
		}


		private void RefreshGridRenderer (StageGame game) {
			m_GridRenderer.SetCountX(0, game.GridCountX0);
			m_GridRenderer.SetCountX(1, game.GridCountX1);
			m_GridRenderer.SetCountX(2, game.GridCountX2);
			m_GridRenderer.TimeGap = 60f / game.BPM / game.GridCountY;
			m_GridRenderer.TimeGap_Main = 60f / game.BPM;
			m_GridRenderer.TimeOffset = game.Shift;
			m_GridRenderer.SpeedMuti = game.GameDropSpeed * game.MapDropSpeed;
		}


		#endregion




	}
}