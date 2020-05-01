namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Saving;
	using Data;


	public class ProjectInfoUI : MonoBehaviour {




		#region --- SUB ---



		[System.Serializable]
		public struct ProjectInfoComponentData {
			public InputField ProjectName;
			public InputField ProjectDescription;
			public InputField BeatmapAuthor;
			public InputField MusicAuthor;
			public InputField BackgroundAuthor;
			public Image BackgroundThumbnail;
			public AspectRatioFitter BackgroundFitter;
			public Image CoverThumbnail;
			public AspectRatioFitter CoverFitter;
			public Text BackgroundHint;
			public Text CoverHint;
			public Text MusicHint;
			public Button Browse_Background;
			public Button Browse_Cover;
			public Button Browse_Music;
			public Button Clear_Background;
			public Button Clear_Cover;
			public Button Clear_Music;
		}


		public class BeatmapInfoItemUIComparer : IComparer<BeatmapInfoItemUI> {
			private BeatmapSortMode Mode = BeatmapSortMode.Time;
			public BeatmapInfoItemUIComparer (BeatmapSortMode mode) {
				Mode = mode;
			}
			public int Compare (BeatmapInfoItemUI x, BeatmapInfoItemUI y) {
				int result;
				switch (Mode) {
					default:
					case BeatmapSortMode.Tag:
						result = x.Tag.CompareTo(y.Tag);
						result = result != 0 ? result : x.Level.CompareTo(y.Level);
						result = result != 0 ? result : x.CreatedTime.CompareTo(y.CreatedTime);
						break;
					case BeatmapSortMode.Level:
						result = x.Level.CompareTo(y.Level);
						result = result != 0 ? result : x.Tag.CompareTo(y.Tag);
						result = result != 0 ? result : x.CreatedTime.CompareTo(y.CreatedTime);
						break;
					case BeatmapSortMode.Time:
						result = x.CreatedTime.CompareTo(y.CreatedTime);
						result = result != 0 ? result : x.Level.CompareTo(y.Level);
						result = result != 0 ? result : x.Tag.CompareTo(y.Tag);
						break;
				}
				return result;
			}
		}



		public enum BeatmapSortMode {
			Tag = 0,
			Level = 1,
			Time = 2,
		}


		public delegate string LanguageHandler (string key);
		public delegate void VoidBeatmapHandler (Beatmap map);
		public delegate Dictionary<string, Beatmap> BeatmapMapHandler ();
		public delegate List<Color32> Color32sHandler ();
		public delegate List<(AnimationCurve, Color32)> TweensHandler ();
		public delegate List<(Project.FileData data, AudioClip clip)> ClickSoundsHandler ();
		public delegate void SetTweenHandler (AnimationCurve curve, Color32? color, int index);
		public delegate void VoidHandler ();
		public delegate void VoidCallbackHandler (System.Action action);
		public delegate void VoidIntFloatHandler (int i, float f);
		public delegate void VoidStringRtHandler (string str, RectTransform rt);
		public delegate void VoidColorIntHandler (Color color, int i);
		public delegate (string name, string description, string mapAuthor, string musicAuthor, string bgAuthor, Sprite bg, Sprite cover, Project.FileData music) ProjectInfoHandler ();
		public delegate string StringHandler ();
		public delegate void VoidStringHandler (string str);
		public delegate void ColorPickerHandler (Color color, System.Action<Color> done);
		public delegate void TweenEditorHandler (AnimationCurve curve, System.Action<AnimationCurve> done);


		#endregion




		#region --- VAR ---


		// Const
		private const string MENU_KEY = "Menu.BeatmapInfoItem";
		private const string MENU_PAL_KEY = "Menu.ProjectInfo.Palette";
		private const string MENU_SOUND_KEY = "Menu.ProjectInfo.ClickSound";
		private const string MENU_TWEEN_KEY = "Menu.ProjectInfo.Tween";

		// Handle
		public static LanguageHandler GetLanguage { get; set; } = null;
		public static VoidBeatmapHandler OnBeatmapInfoChanged { get; set; } = null;
		public static VoidHandler OnProjectInfoChanged { get; set; } = null;
		public static VoidHandler MusicStopClickSounds { get; set; } = null;
		public static VoidIntFloatHandler MusicPlayClickSound { get; set; } = null;
		public static VoidStringRtHandler OpenMenu { get; set; } = null;
		public static VoidCallbackHandler ProjectImportPalette { get; set; } = null;
		public static VoidHandler ProjectSaveProject { get; set; } = null;
		public static VoidHandler ProjectSetDirty { get; set; } = null;
		public static VoidHandler ProjectNewBeatmap { get; set; } = null;
		public static VoidCallbackHandler ProjectImportBeatmap { get; set; } = null;
		public static VoidHandler ProjectAddPaletteColor { get; set; } = null;
		public static VoidHandler ProjectExportPalette { get; set; } = null;
		public static VoidHandler ProjectImportClickSound { get; set; } = null;
		public static VoidHandler ProjectAddTween { get; set; } = null;
		public static VoidCallbackHandler ProjectImportTween { get; set; } = null;
		public static VoidHandler ProjectExportTween { get; set; } = null;
		public static BeatmapMapHandler GetBeatmapMap { get; set; } = null;
		public static VoidColorIntHandler ProjectSetPaletteColor { get; set; } = null;
		public static Color32sHandler GetProjectPalette { get; set; } = null;
		public static TweensHandler GetProjectTweens { get; set; } = null;
		public static SetTweenHandler SetProjectTweenCurve { get; set; } = null;
		public static ClickSoundsHandler GetProjectClickSounds { get; set; } = null;
		public static ProjectInfoHandler GetProjectInfo { get; set; } = null;
		public static StringHandler GetBeatmapKey { get; set; } = null;
		public static VoidHandler ProjectImportBackground { get; set; } = null;
		public static VoidHandler ProjectImportCover { get; set; } = null;
		public static VoidHandler ProjectImportMusic { get; set; } = null;
		public static VoidHandler ProjectRemoveBackground { get; set; } = null;
		public static VoidHandler ProjectRemoveCover { get; set; } = null;
		public static VoidHandler ProjectRemoveMusic { get; set; } = null;
		public static VoidStringHandler SetProjectInfo_Name { get; set; } = null;
		public static VoidStringHandler SetProjectInfo_Description { get; set; } = null;
		public static VoidStringHandler SetProjectInfo_BgAuthor { get; set; } = null;
		public static VoidStringHandler SetProjectInfo_MusicAuthor { get; set; } = null;
		public static VoidStringHandler SetProjectInfo_MapAuthor { get; set; } = null;
		public static ColorPickerHandler SpawnColorPicker { get; set; } = null;
		public static TweenEditorHandler SpawnTweenEditor { get; set; } = null;


		// Short
		private BeatmapSortMode BeatmapSort {
			get => (BeatmapSortMode)BeatmapSortIndex.Value;
			set => BeatmapSortIndex.Value = (int)value;
		}

		// Ser
		[SerializeField] private BeatmapInfoItemUI m_BeatmapItemPrefab = null;
		[SerializeField] private Grabber m_PaletteItemPrefab = null;
		[SerializeField] private Grabber m_TweenItemPrefab = null;
		[SerializeField] private Grabber m_SoundItemPrefab = null;
		[SerializeField] private RectTransform m_BeatmapContent = null;
		[SerializeField] private RectTransform m_PaletteContent = null;
		[SerializeField] private RectTransform m_TweenContent = null;
		[SerializeField] private RectTransform m_SoundContent = null;
		[SerializeField] private ProjectInfoComponentData m_ProjectInfoComponentData = default;
		[SerializeField] private Text[] m_LanguageLabels = null;

		// Saving
		private SavingInt BeatmapSortIndex = new SavingInt("ProjectInfoUI.BeatmapSortIndex", 2);

		// Data
		private bool ReadyForUI = true;


		#endregion




		#region --- MSG ---


		private void Awake () {
			foreach (var tx in m_LanguageLabels) {
				tx.text = GetLanguage(tx.name);
			}
			Awake_ProjectInfo();
		}


		private void OnDisable () {
			MusicStopClickSounds();
		}


		private void Awake_ProjectInfo () {
			m_ProjectInfoComponentData.ProjectName.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				SetProjectInfo_Name(text);
				ProjectSetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.ProjectDescription.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				SetProjectInfo_Description(text);
				ProjectSetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.BeatmapAuthor.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				SetProjectInfo_MapAuthor(text);
				ProjectSetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.MusicAuthor.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				SetProjectInfo_MusicAuthor(text);
				ProjectSetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.BackgroundAuthor.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				SetProjectInfo_BgAuthor(text);
				ProjectSetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.Browse_Background.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				ProjectImportBackground();
				ProjectSaveProject();
			});
			m_ProjectInfoComponentData.Browse_Cover.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				ProjectImportCover();
				ProjectSaveProject();
			});
			m_ProjectInfoComponentData.Browse_Music.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				ProjectImportMusic();
				ProjectSaveProject();
			});
			m_ProjectInfoComponentData.Clear_Background.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				ProjectRemoveBackground();
				ProjectSaveProject();
			});
			m_ProjectInfoComponentData.Clear_Cover.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				ProjectRemoveCover();
				ProjectSaveProject();
			});
			m_ProjectInfoComponentData.Clear_Music.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				ProjectRemoveMusic();
				ProjectSaveProject();
			});
		}


		#endregion




		#region --- API ---


		public void Close () {
			ProjectSaveProject();
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}
		public void Refresh () => RefreshLogic();
		public void NewBeatmap () {
			ProjectNewBeatmap();
			RefreshBeatmapUI();
		}

		public void ImportBeatmap () => ProjectImportBeatmap(RefreshBeatmapUI);

		// Palette
		public void AddPaletteColor () {
			ProjectAddPaletteColor();
			RefreshPaletteUI();
		}

		public void ImportPalette () => ProjectImportPalette(RefreshPaletteUI);
		public void ExportPalette () => ProjectExportPalette();

		// Sound
		public void StopClickSounds () => MusicStopClickSounds();
		public void AddClickSound () => ProjectImportClickSound();

		// Tween
		public void AddTween () {
			ProjectAddTween();
			RefreshTweenUI();
		}

		public void ImportTweens () => ProjectImportTween(RefreshTweenUI);
		public void ExportTweens () => ProjectExportTween();


		#endregion




		#region --- LGC ---


		private void RefreshLogic () {
			RefreshProjectUI();
			RefreshBeatmapUI();
			RefreshPaletteUI();
			RefreshTweenUI();
			RefreshSoundUI();
		}


		private void RefreshProjectUI () {
			ReadyForUI = false;
			try {
				var info = GetProjectInfo();
				var bgSprite = info.bg;
				var coverSprite = info.cover;
				m_ProjectInfoComponentData.ProjectName.text = info.name;
				m_ProjectInfoComponentData.ProjectDescription.text = info.description;
				m_ProjectInfoComponentData.BeatmapAuthor.text = info.mapAuthor;
				m_ProjectInfoComponentData.MusicAuthor.text = info.musicAuthor;
				m_ProjectInfoComponentData.BackgroundAuthor.text = info.bgAuthor;
				m_ProjectInfoComponentData.BackgroundThumbnail.sprite = bgSprite;
				m_ProjectInfoComponentData.BackgroundThumbnail.enabled = bgSprite;
				m_ProjectInfoComponentData.BackgroundThumbnail.preserveAspect = true;
				m_ProjectInfoComponentData.BackgroundFitter.aspectRatio = bgSprite is null ? 1f : bgSprite.rect.width / bgSprite.rect.height;
				m_ProjectInfoComponentData.CoverThumbnail.sprite = coverSprite;
				m_ProjectInfoComponentData.CoverThumbnail.enabled = coverSprite;
				m_ProjectInfoComponentData.CoverThumbnail.preserveAspect = true;
				m_ProjectInfoComponentData.CoverFitter.aspectRatio = coverSprite is null ? 1f : coverSprite.rect.width / coverSprite.rect.height;
				m_ProjectInfoComponentData.BackgroundHint.enabled = !bgSprite;
				m_ProjectInfoComponentData.CoverHint.enabled = !coverSprite;
				m_ProjectInfoComponentData.MusicHint.text =
					info.music != null ?
					(info.music.Data.Length / 1024f / 1024f).ToString("0.0") + " MB" : "0 MB";
			} catch { }
			ReadyForUI = true;
		}


		private void RefreshBeatmapUI () {
			ReadyForUI = false;
			try {
				m_BeatmapContent.DestroyAllChildImmediately();
				int index = 0;
				var mapMap = GetBeatmapMap();
				foreach (var pair in mapMap) {
					var key = pair.Key;
					var map = pair.Value;
					if (map == null) { continue; }
					var item = SpawnItem(m_BeatmapItemPrefab, m_BeatmapContent);
					var rt = item.transform as RectTransform;
					rt.name = key;
					item.Tag = map.Tag;
					item.Level = map.Level;
					item.CreatedTime = map.CreatedTime;
					item.Grab<Text>("Index Text").text = (index + 1).ToString();
					item.Grab<RectTransform>("Loading Sign").gameObject.SetActive(key == GetBeatmapKey());
					var tag = item.Grab<InputField>("Tag InputField");
					var level = item.Grab<InputField>("Level InputField");
					var shift = item.Grab<InputField>("Shift InputField");
					var bpm = item.Grab<InputField>("BPM InputField");
					var ratio = item.Grab<InputField>("Ratio InputField");
					foreach (var mTrigger in item.MenuTriggers) {
						mTrigger.CallbackRight.AddListener(OnMenuTrigger);
					}
					tag.text = map.Tag;
					level.text = map.Level.ToString();
					shift.text = map.Shift.ToString();
					ratio.text = map.Ratio.ToString();
					bpm.text = map.BPM.ToString();
					tag.onEndEdit.AddListener(OnTagEdit);
					level.onEndEdit.AddListener(OnLevelEdit);
					bpm.onEndEdit.AddListener(OnBpmEdit);
					shift.onEndEdit.AddListener(OnShiftEdit);
					ratio.onEndEdit.AddListener(OnRatioEdit);
					index++;
					// Language Texts
					foreach (var text in item.LanguageTexts) {
						text.text = GetLanguage(text.name);
					}
					// Funcs
					void OnTagEdit (string txt) {
						if (mapMap.ContainsKey(key)) {
							var _map = mapMap[key];
							_map.Tag = txt;
							OnBeatmapInfoChanged(_map);
							ProjectSetDirty();
						}
					}
					void OnLevelEdit (string txt) {
						if (mapMap.ContainsKey(key) && int.TryParse(txt, out int value)) {
							var _map = mapMap[key];
							_map.Level = value;
							OnBeatmapInfoChanged(_map);
							ProjectSetDirty();
						}
					}
					void OnBpmEdit (string txt) {
						if (mapMap.ContainsKey(key) && int.TryParse(txt, out int value)) {
							var _map = mapMap[key];
							_map.BPM = value;
							OnBeatmapInfoChanged(_map);
							ProjectSetDirty();
						}
					}
					void OnShiftEdit (string txt) {
						if (mapMap.ContainsKey(key) && float.TryParse(txt, out float value)) {
							var _map = mapMap[key];
							_map.Shift = value;
							OnBeatmapInfoChanged(_map);
							ProjectSetDirty();
						}
					}
					void OnRatioEdit (string txt) {
						if (mapMap.ContainsKey(key) && float.TryParse(txt, out float value)) {
							var _map = mapMap[key];
							_map.Ratio = value;
							OnBeatmapInfoChanged(_map);
							ProjectSetDirty();
						}
					}
					void OnMenuTrigger () => OpenMenu(MENU_KEY, rt);
				}
				// Sort Beatmaps
				var items = new List<BeatmapInfoItemUI>(m_BeatmapContent.GetComponentsInChildren<BeatmapInfoItemUI>(true));
				items.Sort(new BeatmapInfoItemUIComparer(BeatmapSort));
				int _index = 0;
				foreach (var item in items) {
					item.transform.SetAsLastSibling();
					item.Grab<Text>("Index Text").text = _index.ToString();
					_index++;
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex);
			}
			ReadyForUI = true;
		}


		private void RefreshPaletteUI () {
			ReadyForUI = false;
			try {
				m_PaletteContent.DestroyAllChildImmediately();
				var pals = GetProjectPalette();
				foreach (var color in pals) {
					var item = SpawnItem(m_PaletteItemPrefab, m_PaletteContent);
					var rt = item.transform as RectTransform;
					int rtIndex = rt.GetSiblingIndex();
					var palColor = color;
					// Grab
					item.Grab<Image>().color = palColor;
					item.Grab<Text>("Index").text = rtIndex.ToString();
					var trigger = item.Grab<TriggerUI>();
					trigger.CallbackLeft.AddListener(() => {
						SpawnColorPicker(palColor, (_color) => {
							ProjectSetPaletteColor(_color, rtIndex);
							RefreshPaletteUI();
						});
					});
					trigger.CallbackRight.AddListener(() => OpenMenu(MENU_PAL_KEY, rt));
				}
			} catch { }
			ReadyForUI = true;
		}


		private void RefreshTweenUI () {
			ReadyForUI = false;
			try {
				m_TweenContent.DestroyAllChildImmediately();
				var tweens = GetProjectTweens();
				foreach (var (curve, color) in tweens) {
					var item = SpawnItem(m_TweenItemPrefab, m_TweenContent);
					var rt = item.transform as RectTransform;
					int rtIndex = rt.GetSiblingIndex();
					item.Grab<Text>("Index").text = rtIndex.ToString();
					item.Grab<CurveUI>("Renderer").Curve = curve;
					var trigger = item.Grab<TriggerUI>();
					trigger.CallbackLeft.AddListener(() => SpawnTweenEditor(curve, (resultCurve) => {
						SetProjectTweenCurve(resultCurve, null, rtIndex);
						RefreshTweenUI();
					}));
					trigger.CallbackRight.AddListener(() => OpenMenu(MENU_TWEEN_KEY, rt));
					var oldColor = color;
					item.Grab<Image>("Color").color = oldColor;
					item.Grab<Button>("Color").onClick.AddListener(() => {
						SpawnColorPicker(oldColor, (resultColor) => {
							SetProjectTweenCurve(null, resultColor, rtIndex);
							RefreshTweenUI();
						});
					});
				}
			} catch { }
			ReadyForUI = true;
		}


		private void RefreshSoundUI () {
			ReadyForUI = false;
			try {
				m_SoundContent.DestroyAllChildImmediately();
				var clickSounds = GetProjectClickSounds();
				foreach (var (_, sClip) in clickSounds) {
					var item = SpawnItem(m_SoundItemPrefab, m_SoundContent);
					var rt = item.transform as RectTransform;
					int rtIndex = rt.GetSiblingIndex();
					// Grab
					item.Grab<Text>("Index").text = rtIndex.ToString();
					item.Grab<Text>("Name").text = sClip.name;
					item.Grab<Button>().onClick.AddListener(() => {
						MusicPlayClickSound(rtIndex, 1f);
					});
					item.Grab<TriggerUI>().CallbackRight.AddListener(() => {
						OpenMenu(MENU_SOUND_KEY, rt);
					});
				}
			} catch { }
			ReadyForUI = true;
		}


		private T SpawnItem<T> (T prefab, RectTransform content) where T : MonoBehaviour {
			var item = Instantiate(prefab, content);
			var rt = item.transform as RectTransform;
			rt.SetAsLastSibling();
			int rtIndex = rt.GetSiblingIndex();
			rt.gameObject.name = rtIndex.ToString();
			rt.anchoredPosition3D = rt.anchoredPosition;
			rt.localRotation = Quaternion.identity;
			rt.localScale = Vector3.one;
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (prefab.transform as RectTransform).rect.height);
			return item;
		}


		#endregion




	}
}