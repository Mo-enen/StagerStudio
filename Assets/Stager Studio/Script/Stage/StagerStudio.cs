namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UI;
	using UnityEngine.UI;
	using Stage;
	using Object;
	using Rendering;


	public partial class StagerStudio : MonoBehaviour {




		#region --- SUB ---


		private static class LanguageData {
			public const string UI_QuitConfirm = "Menu.UI.QuitConfirm";
			public const string UI_OpenWebMSG = "Dialog.OpenWebMSG";
			public const string Confirm_DeleteProjectPal = "ProjectInfo.Dialog.DeletePal";
			public const string Confirm_DeleteProjectSound = "ProjectInfo.Dialog.DeleteSound";
			public const string Confirm_DeleteProjectTween = "ProjectInfo.Dialog.DeleteTween";
		}


		[System.Serializable]
		public struct CursorData {
			public Texture2D Cursor;
			public Vector2 Offset;
		}


		[System.Serializable]
		public class UndoData {
			public byte[] Beatmap;
			public float MusicTime;
			public bool[] ContainerActive;
		}


		#endregion




		#region --- VAR ---


		// Short
		public static StagerStudio Main => _Main != null ? _Main : (_Main = FindObjectOfType<StagerStudio>());
		private StageMusic Music => _Music != null ? _Music : (_Music = FindObjectOfType<StageMusic>());
		private StageProject Project => _Project != null ? _Project : (_Project = FindObjectOfType<StageProject>());
		private StageGame Game => _Game != null ? _Game : (_Game = FindObjectOfType<StageGame>());
		private StageSoundFX SFX => _SFX != null ? _SFX : (_SFX = FindObjectOfType<StageSoundFX>());
		private StageEditor Editor => _Editor != null ? _Editor : (_Editor = FindObjectOfType<StageEditor>());
		private StageLibrary Library => _Library != null ? _Library : (_Library = FindObjectOfType<StageLibrary>());
		private StageLanguage Language => _Language != null ? _Language : (_Language = FindObjectOfType<StageLanguage>());
		private StageSkin Skin => _Skin != null ? _Skin : (_Skin = FindObjectOfType<StageSkin>());
		private StageShortcut Short => _Short != null ? _Short : (_Short = FindObjectOfType<StageShortcut>());
		private StageMenu Menu => _Menu != null ? _Menu : (_Menu = FindObjectOfType<StageMenu>());

		// Ser
		[SerializeField] private Transform m_CanvasRoot = null;
		[SerializeField] private RectTransform m_DirtyMark = null;
		[SerializeField] private Text m_TipLabel = null;
		[SerializeField] private Text m_BeatmapSwiperLabel = null;
		[SerializeField] private Text m_SkinSwiperLabel = null;
		[SerializeField] private Toggle m_UseDynamicSpeed = null;
		[SerializeField] private Toggle m_UseAbreastView = null;
		[SerializeField] private Text m_AuthorLabel = null;
		[SerializeField] private Text m_VersionLabel = null;
		[SerializeField] private GridRenderer m_GridRenderer = null;
		[SerializeField] private RectTransform m_PitchWarningBlock = null;
		[SerializeField] private RectTransform m_Keypress = null;
		[Header("UI")]
		[SerializeField] private Text[] m_LanguageTexts = null;
		[SerializeField] private Selectable[] m_NavigationItems = null;
		[SerializeField] private BackgroundUI m_Background = null;
		[SerializeField] private ProgressUI m_Progress = null;
		[SerializeField] private HintBarUI m_Hint = null;
		[SerializeField] private AbreastSwitcherUI m_AbreastSwitcherUI = null;
		[SerializeField] private ZoneUI m_Zone = null;
		[SerializeField] private PreviewUI m_Preview = null;
		[SerializeField] private WaveUI m_Wave = null;
		[Header("Data")]
		[SerializeField] private CursorData[] m_Cursors = null;

		// Data
		private static StagerStudio _Main = null;
		private StageMusic _Music = null;
		private StageProject _Project = null;
		private StageGame _Game = null;
		private StageSoundFX _SFX = null;
		private StageEditor _Editor = null;
		private StageLibrary _Library = null;
		private StageLanguage _Language = null;
		private StageSkin _Skin = null;
		private StageShortcut _Short = null;
		private StageMenu _Menu = null;
		private bool WillQuit = false;


		#endregion




		#region --- MSG ---


		private void Awake () {
			Awake_Message();
			Awake_Setting();
			Awake_Menu();
			Awake_Object();
			Awake_Project();
			Awake_Game();
			Awake_Music();
			Awake_Editor();
			Awake_Library();
			Awake_Skin();
			Awake_Undo();
			Awake_ProjectInfo();
			Awake_Preview();
			Awake_Misc();
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
			CursorUI.GlobalUpdate();
			StageUndo.GlobalUpdate();
			StageObject.ZoneMinMax = m_Zone.GetZoneMinMax();
			StageObject.Abreast = (Game.AbreastIndex, Game.UseAbreast, Game.AllStageAbreast);
			StageObject.ScreenZoneMinMax = m_Zone.GetScreenZoneMinMax();
			Object.Stage.StageCount = Game.GetItemCount(0);
			Object.Stage.AbreastWidth = Game.AbreastWidth;
		}


		private void Awake_Message () {
			// Language
			StageProject.GetLanguage = Language.Get;
			StageGame.GetLanguage = Language.Get;
			StageMenu.GetLanguage = Language.Get;
			StageState.GetLanguage = Language.Get;
			StageSkin.GetLanguage = Language.Get;
			DialogUtil.GetLanguage = Language.Get;
			HomeUI.GetLanguage = Language.Get;
			ProjectInfoUI.GetLanguage = Language.Get;
			TooltipUI.GetLanguage = Language.Get;
			ColorPickerUI.GetLanguage = Language.Get;
			DialogUI.GetLanguage = Language.Get;
			TweenEditorUI.GetLanguage = Language.Get;
			ProjectCreatorUI.GetLanguage = Language.Get;
			SkinEditorUI.GetLanguage = Language.Get;
			// Misc
			TooltipUI.TipLabel = m_TipLabel;
			HomeUI.LogHint = m_Hint.SetHint;
			StageProject.LogHint = m_Hint.SetHint;
			StageGame.LogHint = m_Hint.SetHint;
			StageLanguage.OnLanguageLoaded = () => {
				TryRefreshSetting();
				ReloadSSLanguageTexts();
			};
			DialogUtil.GetRoot = () => m_DialogRoot;
			DialogUtil.GetPrefab = () => m_DialogPrefab;
			TooltipUI.GetHotKey = Short.GetHotkeyLabel;
			// Quit
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) { return true; }
#endif
				if (WillQuit) {
					return true;
				} else {
					DialogUtil.Dialog_OK_Cancel(LanguageData.UI_QuitConfirm, DialogUtil.MarkType.Info, () => {
						WillQuit = true;
						Application.Quit();
					});
					return false;
				}
			};
			// Reload
			ReloadSSLanguageTexts();
			// Func
			void ReloadSSLanguageTexts () {
				foreach (var text in m_LanguageTexts) {
					text.text = Language.Get(text.name);
				}
			}
		}


		private void Awake_Menu () {
			// Grid
			Menu.AddCheckerFunc("Menu.Grid.x0", () => Game.GridCountX == 1);
			Menu.AddCheckerFunc("Menu.Grid.x1", () => Game.GridCountX == 3);
			Menu.AddCheckerFunc("Menu.Grid.x2", () => Game.GridCountX == 7);
			Menu.AddCheckerFunc("Menu.Grid.x3", () => Game.GridCountX == 15);
			Menu.AddCheckerFunc("Menu.Grid.y0", () => Game.GridCountY == 1);
			Menu.AddCheckerFunc("Menu.Grid.y1", () => Game.GridCountY == 2);
			Menu.AddCheckerFunc("Menu.Grid.y2", () => Game.GridCountY == 4);
			Menu.AddCheckerFunc("Menu.Grid.y3", () => Game.GridCountY == 8);
			// Auto Save
			Menu.AddCheckerFunc("Menu.AutoSave.0", () => Mathf.Abs(Project.UI_AutoSaveTime - 30f) < 1f);
			Menu.AddCheckerFunc("Menu.AutoSave.1", () => Mathf.Abs(Project.UI_AutoSaveTime - 120f) < 1f);
			Menu.AddCheckerFunc("Menu.AutoSave.2", () => Mathf.Abs(Project.UI_AutoSaveTime - 300f) < 1f);
			Menu.AddCheckerFunc("Menu.AutoSave.3", () => Mathf.Abs(Project.UI_AutoSaveTime - 600f) < 1f);
			Menu.AddCheckerFunc("Menu.AutoSave.Off", () => Project.UI_AutoSaveTime < 0f);
			// Beat per Section
			Menu.AddCheckerFunc("Menu.Grid.bps0", () => Game.BeatPerSection == 3);
			Menu.AddCheckerFunc("Menu.Grid.bps1", () => Game.BeatPerSection == 4);
		}


		private void Awake_Object () {
			StageObject.TweenEvaluate = (x, index) => Project.Tweens[Mathf.Clamp(index, 0, Project.Tweens.Count - 1)].curve.Evaluate(x);
			StageObject.PaletteColor = (index) => index < 0 ? new Color32(0, 0, 0, 0) : Project.Palette[Mathf.Min(index, Project.Palette.Count - 1)];
			StageObject.MaterialZoneID = Shader.PropertyToID("_ZoneMinMax");
			StageObject.GetGameSpeedMuti = () => Game.GameDropSpeed * Game.MapDropSpeed;
			Note.GetFilledTime = Game.FillDropTime;
			Note.GetGameDropOffset = (muti) => Game.AreaBetweenDrop(Music.Time, muti);
			Note.GetDropOffset = Game.AreaBetweenDrop;
			Note.PlayClickSound = Music.PlayClickSound;
		}


		private void Awake_Project () {

			// Project
			StageProject.OnProjectLoadingStart = () => {
				Music.SetClip(null);
				Game.SetSpeedCurveDirty();
				m_Preview.SetDirty();
				UI_RemoveUI();
				StageUndo.ClearUndo();
				StageObject.Beatmap = null;
				Game.SetAbreastIndex(0);
				Game.SetUseAbreastView(false);
				Game.SetUseDynamicSpeed(true);
				Game.SetGameDropSpeed(1f);
				RefreshAuthorLabel();
			};
			StageProject.OnProjectLoaded = () => {
				Game.SetSpeedCurveDirty();
				Music.Pitch = 1f;
				Music.Seek(0f);
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
				Game.SetSpeedCurveDirty();
				Game.SetUseAbreastView(false);
				Game.SetGameDropSpeed(1f);
				Music.Pitch = 1f;
				Music.SetClip(null);
				StageUndo.ClearUndo();
				m_Preview.SetDirty();
				Editor.ClearSelection();
				StageObject.Beatmap = null;
			};

			// Beatmap
			StageProject.OnBeatmapOpened = (map, key) => {
				if (!(map is null)) {
					Game.BPM = map.BPM;
					Game.Shift = map.Shift;
					Game.MapDropSpeed = map.DropSpeed;
					Game.Ratio = map.Ratio;
					m_BeatmapSwiperLabel.text = map.Tag;
					StageObject.Beatmap = map;
				}
				TryRefreshProjectInfo();
				RefreshLoading(-1f);
				Editor.ClearSelection();
				Game.SetSpeedCurveDirty();
				Game.ClearAllContainers();
				Music.Pitch = 1f;
				Music.Seek(0f);
				StageUndo.ClearUndo();
				StageUndo.RegisterUndo();
				m_Preview.SetDirty();
				Resources.UnloadUnusedAssets();
				RefreshGridRenderer();
			};
			StageProject.OnBeatmapRemoved = () => {
				TryRefreshProjectInfo();
			};
			StageProject.OnBeatmapCreated = () => {
				TryRefreshProjectInfo();
			};

			// Assets
			StageProject.OnMusicLoaded = (clip) => {
				Music.SetClip(clip);
				TryRefreshProjectInfo();
				m_Wave.LoadWave(clip);
				Music.Pitch = 1f;
				Music.Seek(0f);
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
				Music.SetClickSounds(clips);
				TryRefreshProjectInfo();

			};

			// Misc
			StageProject.OnDirtyChanged = m_DirtyMark.gameObject.SetActive;
			StageProject.OnLoadProgress = RefreshLoading;

			// Func
			IEnumerator SaveProgressing () {
				float pg = 0f;
				m_Hint.SetProgress(0f);
				while (Project.SavingProject) {
					yield return new WaitUntil(() => Project.SavingProgress != pg);
					pg = Project.SavingProgress;
					m_Hint.SetProgress(pg);
				}
				m_Hint.SetProgress(-1f);
			}

		}


		private void Awake_Game () {
			StageGame.OnStageObjectChanged = () => {
				m_Preview.SetDirty();
			};
			StageGame.OnAbreastChanged = () => {
				m_UseAbreastView.isOn = Game.UseAbreast;
				m_Wave.gameObject.SetActive(Game.UseAbreast && !Game.UseDynamicSpeed);
				m_AbreastSwitcherUI.RefreshUI();
			};
			StageGame.OnSpeedChanged = () => {
				m_UseDynamicSpeed.isOn = Game.UseDynamicSpeed;
				m_Wave.gameObject.SetActive(Game.UseAbreast && !Game.UseDynamicSpeed);
				Note.SetCacheDirty();
				RefreshGridRenderer();
			};
			StageGame.OnGridChanged = () => {
				m_GridRenderer.SetShow(Game.ShowGrid);
				Track.BeatPerSection = Game.BeatPerSection;
				StageObject.ShowGrid = Game.ShowGrid;
				RefreshGridRenderer();
			};
			StageGame.OnRatioChanged = (ratio) => {
				m_Zone.SetFitterRatio(ratio);
				Data.Beatmap.DEFAULT_STAGE.Height = 1f / ratio;
			};
			StageGame.GetBeatmap = () => Project.Beatmap;
		}


		private void Awake_Music () {
			StageMusic.OnMusicPlayPause = (playing) => {
				m_Progress.RefreshControlUI();
				SetNavigationInteractable(!playing);
				StageObject.MusicPlaying = playing;

			};
			StageMusic.OnMusicTimeChanged = (time, duration) => {
				m_Progress.SetProgress(time, Game.BPM);
				m_Wave.Time01 = time / duration;
				m_GridRenderer.MusicTime = time;
				StageObject.MusicTime = time;
				StageObject.MusicDuration = duration;
			};
			StageMusic.OnMusicClipLoaded = () => {
				m_Progress.RefreshControlUI();
			};
			StageMusic.OnPitchChanged = () => {
				m_PitchWarningBlock.gameObject.SetActive(Music.Pitch < 0.05f);
			};
		}


		private void Awake_Editor () {
			StageEditor.GetZoneMinMax = () => m_Zone.GetZoneMinMax(true);
			StageEditor.OnSelectionChanged = () => {
				Library.RefreshAddButton(Editor.SelectingCount > 0);
				m_Preview.SetDirty();
			};
			StageEditor.OnLockEyeChanged = () => {
				Editor.ClearSelection();
			};
			StageEditor.GetBrushTypeIndex = () => Library.SelectingItemTypeIndex;
			StageEditor.GetBeatmap = () => Project.Beatmap;
			StageEditor.GetEditorActive = () => Project.Beatmap && !Music.IsPlaying;
		}


		private void Awake_Library () {
			StageLibrary.OnSelectionChanged = (index) => {
				if (index >= 0) {
					Editor.ClearSelection();
				}
			};
			StageLibrary.GetSelectingObjects = () => {
				int finalType = -1;
				var finalList = new List<object>();
				Editor.ForAddSelectingObjects((type, index) => {
					var map = Project.Beatmap;
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
			StageLibrary.GetSelectionCount = () => Editor.SelectingCount;
			StageLibrary.OpenMenu = Menu.OpenMenu;
			StageLibrary.GetBPM = () => Game.BPM;
		}


		private void Awake_Skin () {
			StageSkin.OnSkinLoaded = (data) => {
				TryRefreshSetting();
				Game.ClearAllContainers();
				StageObject.LoadSkin(data);
				Luminous.SetLuminousSkin(data);
				Resources.UnloadUnusedAssets();
				m_SkinSwiperLabel.text = StageSkin.Data.Name;
			};
			StageSkin.OnSkinDeleted = () => {
				TryRefreshSetting();
				Game.ClearAllContainers();
			};
		}


		private void Awake_Undo () {
			StageUndo.GetObjectData = () => new UndoData() {
				Beatmap = Util.ObjectToBytes(Project.Beatmap),
				MusicTime = Music.Time,
				ContainerActive = new bool[5] {
					Editor.GetContainerActive(0),
					Editor.GetContainerActive(1),
					Editor.GetContainerActive(2),
					Editor.GetContainerActive(3),
					Editor.GetContainerActive(4),
				},
			};
			StageUndo.OnUndo = (bytes) => {
				var step = Util.BytesToObject(bytes) as UndoData;
				if (step is null || step.Beatmap is null) { return; }
				// Map
				Project.Beatmap.LoadFromBytes(step.Beatmap);
				// Music Time
				Music.Seek(step.MusicTime);
				// Container Active
				for (int i = 0; i < step.ContainerActive.Length; i++) {
					Editor.SetContainerActive(i, step.ContainerActive[i]);
				}
				// Final
				Editor.ClearSelection();
			};
		}


		private void Awake_ProjectInfo () {
			ProjectInfoUI.OnBeatmapInfoChanged = (map) => {
				if (Project.Beatmap) {
					Game.BPM = Project.Beatmap.BPM;
					Game.Shift = Project.Beatmap.Shift;
					Game.MapDropSpeed = Project.Beatmap.DropSpeed;
					Game.Ratio = Project.Beatmap.Ratio;
					m_BeatmapSwiperLabel.text = Project.Beatmap.Tag;
				}
				RefreshGridRenderer();
			};
			ProjectInfoUI.OnProjectInfoChanged = () => {
				RefreshAuthorLabel();


			};
		}


		private void Awake_Preview () {
			PreviewUI.GetMusicTime01 = (time) => time / Music.Duration;
			PreviewUI.GetBeatmap = () => Project.Beatmap;
		}


		private void Awake_Misc () {
			CursorUI.GetCursorTexture = (index) => (
				index >= 0 ? m_Cursors[index].Cursor : null,
				index >= 0 ? m_Cursors[index].Offset : Vector2.zero
			);
			ProgressUI.GetSnapTime = (time, step) => Game.SnapTime(time, step);
			GridRenderer.GetAreaBetween = Game.AreaBetween;
			GridRenderer.GetSnapedTime = Game.SnapTime;
			TrackSectionRenderer.GetAreaBetween = Game.AreaBetween;
			TrackSectionRenderer.GetSnapedTime = Game.SnapTime;
			m_GridRenderer.SetSortingLayer(SortingLayer.NameToID("UI"), 0);
			m_VersionLabel.text = $"v{Application.version}";
			SetNavigationInteractable(true);
		}


		#endregion




		#region --- API ---


		public void Quit () => Application.Quit();


		public void About () => DialogUtil.Open($"Stager Studio v{Application.version} by 楠瓜Moenen\nEmail moenenn@163.com\nTwitter @_Moenen\nQQ 754100943", DialogUtil.MarkType.Info, () => { });


		public void GotoWeb () {
			Application.OpenURL("http://www.stager.studio");
			DialogUtil.Open(
				string.Format(Language.Get(LanguageData.UI_OpenWebMSG), "www.stager.studio"),
				DialogUtil.MarkType.Info,
				() => { }, null, null, null, null
			);
		}


		public void Undo () => StageUndo.Undo();


		public void Redo () => StageUndo.Redo();


		#endregion




		#region --- LGC ---


		private void SetNavigationInteractable (bool interactable) {
			foreach (var item in m_NavigationItems) {
				item.interactable = interactable;
			}
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


		private void RefreshAuthorLabel () => m_AuthorLabel.text = $"{Project.ProjectName} | {Project.BeatmapAuthor} & {Project.MusicAuthor}, {(Project.Beatmap ? Project.Beatmap.Tag : "")} Lv{(Project.Beatmap ? Project.Beatmap.Level : 0)}";


		private void RefreshGridRenderer () {
			m_GridRenderer.CountX = Game.GridCountX;
			m_GridRenderer.TimeGap = 60f / Game.BPM / Game.GridCountY;
			m_GridRenderer.TimeGap_Main = 60f / Game.BPM;
			m_GridRenderer.TimeOffset = Game.Shift;
			m_GridRenderer.SpeedMuti = Game.GameDropSpeed * Game.MapDropSpeed;
		}


		#endregion




	}
}