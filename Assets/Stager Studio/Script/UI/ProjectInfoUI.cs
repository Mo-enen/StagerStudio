namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Saving;
	using Stage;
	using Data;


	public class ProjectInfoUI : MonoBehaviour {




		#region --- SUB ---



		public delegate string LanguageHandler (string key);
		public delegate void VoidBeatmapHandler (Beatmap map);
		public delegate void VoidHandler ();



		public static class LanguageData {
			public const string UI_NoBackgroundHint = "ProjectInfo.UI.NoBackgroundHint";
			public const string UI_NoCoverHint = "ProjectInfo.UI.NoCoverHint";
			public const string UI_NoMusicHint = "ProjectInfo.UI.NoMusicHint";
			public const string UI_ImportPalette = "ProjectInfo.Dialog.ImportPalette";
			public const string UI_ImportPaletteTitle = "ProjectInfo.Dialog.ImportPaletteTitle";
			public const string UI_ExportPaletteTitle = "ProjectInfo.Dialog.ExportPaletteTitle";
			public const string UI_PaletteFull = "ProjectInfo.Dialog.PaletteFull";
			public const string UI_PaletteExported = "ProjectInfo.Dialog.PaletteExported";

			public const string UI_ImportTween = "ProjectInfo.Dialog.ImportTween";
			public const string UI_ImportTweenTitle = "ProjectInfo.Dialog.ImportTweenTitle";
			public const string UI_ExportTweenTitle = "ProjectInfo.Dialog.ExportTweenTitle";
			public const string UI_TweenFull = "ProjectInfo.Dialog.TweenFull";
			public const string UI_TweenExported = "ProjectInfo.Dialog.TweenExported";

			public const string UI_ImportGene = "ProjectInfo.Dialog.ImportGene";
			public const string UI_ImportGeneTitle = "ProjectInfo.Dialog.ImportGeneTitle";
			public const string UI_ExportGeneTitle = "ProjectInfo.Dialog.ExportGeneTitle";
			public const string UI_GeneExported = "ProjectInfo.Dialog.GeneExported";
		}



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


		[System.Serializable]
		public struct ProjectInfoGeneData {
			public Grabber[] Permissions;
			public Grabber[] Ranges;
			public Grabber[] Limitations;
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

		// Short
		private BeatmapSortMode BeatmapSort {
			get => (BeatmapSortMode)BeatmapSortIndex.Value;
			set => BeatmapSortIndex.Value = (int)value;
		}
		private static StageProject Project => _Project != null ? _Project : (_Project = FindObjectOfType<StageProject>());
		private StageMusic Music => _Music != null ? _Music : (_Music = FindObjectOfType<StageMusic>());
		private StageMenu Menu => _Menu != null ? _Menu : (_Menu = FindObjectOfType<StageMenu>());

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
		[SerializeField] private ProjectInfoGeneData m_ProjectInfoGeneData = default;
		[SerializeField] private Text[] m_LanguageLabels = null;

		// Saving
		private SavingInt BeatmapSortIndex = new SavingInt("ProjectInfoUI.BeatmapSortIndex", 2);

		// Data
		private static StageProject _Project = null;
		private static StageMusic _Music = null;
		private static StageMenu _Menu = null;
		private bool ReadyForUI = true;


		#endregion




		#region --- MSG ---


		private void Awake () {
			foreach (var tx in m_LanguageLabels) {
				tx.text = GetLanguage(tx.name);
			}
			Awake_ProjectInfo();
			Awake_Gene();
		}


		private void OnDisable () {
			Music?.StopClickSounds();
		}


		private void Awake_ProjectInfo () {
			m_ProjectInfoComponentData.ProjectName.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				Project.ProjectName = text;
				Project.SetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.ProjectDescription.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				Project.ProjectDescription = text;
				Project.SetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.BeatmapAuthor.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				Project.BeatmapAuthor = text;
				Project.SetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.MusicAuthor.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				Project.MusicAuthor = text;
				Project.SetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.BackgroundAuthor.onEndEdit.AddListener((text) => {
				if (!ReadyForUI) { return; }
				Project.BackgroundAuthor = text;
				Project.SetDirty();
				OnProjectInfoChanged();
			});
			m_ProjectInfoComponentData.Browse_Background.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				Project.ImportBackground();
				Project.SaveProject();
			});
			m_ProjectInfoComponentData.Browse_Cover.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				Project.ImportCover();
				Project.SaveProject();
			});
			m_ProjectInfoComponentData.Browse_Music.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				Project.ImportMusic();
				Project.SaveProject();
			});
			m_ProjectInfoComponentData.Clear_Background.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				Project.RemoveBackground();
				Project.SaveProject();
			});
			m_ProjectInfoComponentData.Clear_Cover.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				Project.RemoveCover();
				Project.SaveProject();
			});
			m_ProjectInfoComponentData.Clear_Music.onClick.AddListener(() => {
				if (!ReadyForUI) { return; }
				Project.RemoveMusic();
				Project.SaveProject();
			});
		}


		private void Awake_Gene () {
			// Permission
			for (int i = 0; i < m_ProjectInfoGeneData.Permissions.Length; i++) {
				int index = i;
				var grab = m_ProjectInfoGeneData.Permissions[index];
				grab.Grab<Toggle>("Toggle").onValueChanged.AddListener((isOn) => {
					if (!ReadyForUI) { return; }
					Project.ProjectGene.SetPermisstion(grab.name, isOn);
					Project.SetDirty();
				});
			}
			// Range
			for (int i = 0; i < m_ProjectInfoGeneData.Ranges.Length; i++) {
				int index = i;
				var grab = m_ProjectInfoGeneData.Ranges[index];
				// CallBack
				grab.Grab<InputField>("Min").onEndEdit.AddListener((txt) => {
					if (!ReadyForUI) { return; }
					if (float.TryParse(txt, out float value)) {
						var range = Project.ProjectGene.GetRange(grab.name);
						range.Min = Mathf.Min(value, range.Max);
						Project.ProjectGene.SetRange(grab.name, range);
						Project.SetDirty();
					}
					RefreshGeneUI();
				});
				grab.Grab<InputField>("Max").onEndEdit.AddListener((txt) => {
					if (!ReadyForUI) { return; }
					if (float.TryParse(txt, out float value)) {
						var range = Project.ProjectGene.GetRange(grab.name);
						range.Max = Mathf.Max(value, range.Min);
						Project.ProjectGene.SetRange(grab.name, range);
						Project.SetDirty();
					}
					RefreshGeneUI();
				});
				grab.Grab<Toggle>("MinUse").onValueChanged.AddListener((isOn) => {
					if (!ReadyForUI) { return; }
					var range = Project.ProjectGene.GetRange(grab.name);
					range.HasMin = isOn;
					Project.ProjectGene.SetRange(grab.name, range);
					Project.SetDirty();
					RefreshGeneUI();
				});
				grab.Grab<Toggle>("MaxUse").onValueChanged.AddListener((isOn) => {
					if (!ReadyForUI) { return; }
					var range = Project.ProjectGene.GetRange(grab.name);
					range.HasMax = isOn;
					Project.ProjectGene.SetRange(grab.name, range);
					Project.SetDirty();
					RefreshGeneUI();
				});
			}
			// Limit
			for (int i = 0; i < m_ProjectInfoGeneData.Limitations.Length; i++) {
				int index = i;
				var grab = m_ProjectInfoGeneData.Limitations[index];
				var input = grab.Grab<InputField>("InputField");
				input.onEndEdit.AddListener((txt) => {
					if (!ReadyForUI) { return; }
					int value = -1;
					int.TryParse(txt, out value);
					Project.ProjectGene.SetLimitation(grab.name, value >= 0 ? value : -1);
					Project.SetDirty();
					RefreshGeneUI();
				});
			}
		}


		#endregion




		#region --- API ---


		public void Close () {
			Project.SaveProject();
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}


		public void Refresh () => RefreshLogic();


		public void NewBeatmap () {
			Project.NewBeatmap();
			RefreshBeatmapUI();
		}


		public void ImportBeatmap () {
			Project.ImportBeatmap();
			RefreshBeatmapUI();
		}


		// Palette
		public void AddPaletteColor () {
			if (Project.Palette.Count >= 256) {
				DialogUtil.Dialog_OK(LanguageData.UI_PaletteFull, DialogUtil.MarkType.Warning, () => { });
				return;
			}
			Project.AddPaletteColor(Color.white);
			RefreshPaletteUI();
		}


		public void ImportPalette () {
			DialogUtil.Dialog_OK_Cancel(LanguageData.UI_ImportPalette, DialogUtil.MarkType.Warning, () => {
				var path = DialogUtil.PickFileDialog((LanguageData.UI_ImportPaletteTitle), "images", "png", "jpg");
				if (string.IsNullOrEmpty(path)) { return; }
				try {
					var colors = Util.ImageToPixels(path, false).pixels;
					if (colors is null) { throw new System.Exception("Error on import image."); }
					Project.Palette.Clear();
					Project.Palette.AddRange(colors);
					Project.SetDirty();
					if (Project.Palette.Count > 256) {
						Project.Palette.RemoveRange(256, Project.Palette.Count - 256);
					}
				} catch (System.Exception ex) {
					DialogUtil.Open(ex.Message, DialogUtil.MarkType.Error, () => { });
				}
				RefreshPaletteUI();
			});
		}


		public void ExportPalette () {
			var path = DialogUtil.CreateFileDialog((LanguageData.UI_ExportPaletteTitle), "Pal", "png");
			if (string.IsNullOrEmpty(path)) { return; }
			try {
				var texture = new Texture2D(Project.Palette.Count, 1);
				var colors = new List<Color>();
				foreach (var c in Project.Palette) {
					colors.Add(c);
				}
				texture.SetPixels(colors.ToArray());
				texture.Apply();
				Util.ByteToFile(texture.EncodeToPNG(), path);
				DialogUtil.Dialog_OK(LanguageData.UI_PaletteExported, DialogUtil.MarkType.Success, () => { });
			} catch (System.Exception ex) {
				DialogUtil.Open(ex.Message, DialogUtil.MarkType.Error, () => { });
			}
		}


		// Sound
		public void StopClickSounds () => Music.StopClickSounds();


		public void AddClickSound () {
			Project.ImportClickSound();
		}


		// Tween
		public void AddTween () {
			if (Project.Tweens.Count >= 256) {
				DialogUtil.Dialog_OK(LanguageData.UI_TweenFull, DialogUtil.MarkType.Warning, () => { });
				return;
			}
			Project.AddTweenCurve(AnimationCurve.Linear(0, 0, 1, 1), Color.white);
			RefreshTweenUI();
		}


		public void ImportTweens () {
			DialogUtil.Dialog_OK_Cancel(LanguageData.UI_ImportTween, DialogUtil.MarkType.Warning, () => {
				var path = DialogUtil.PickFileDialog((LanguageData.UI_ImportTweenTitle), "json", "json");
				if (string.IsNullOrEmpty(path) || !Util.FileExists(path)) { return; }
				try {
					var tArray = JsonUtility.FromJson<Data.Project.TweenArray>(Util.FileToText(path));
					Project.Tweens.Clear();
					Project.Tweens.AddRange(tArray.GetAnimationTweens());
					Project.SetDirty();
					if (Project.Tweens.Count > 256) {
						Project.Tweens.RemoveRange(256, Project.Tweens.Count - 256);
					}
				} catch (System.Exception ex) {
					DialogUtil.Open(ex.Message, DialogUtil.MarkType.Error, () => { });
				}
				RefreshTweenUI();
			});
		}


		public void ExportTweens () {
			var path = DialogUtil.CreateFileDialog((LanguageData.UI_ExportTweenTitle), "json", "json");
			if (string.IsNullOrEmpty(path)) { return; }
			try {
				var json = JsonUtility.ToJson(new Data.Project.TweenArray(Project.Tweens), true);
				Util.TextToFile(json, path);
				DialogUtil.Dialog_OK(LanguageData.UI_TweenExported, DialogUtil.MarkType.Success, () => { });
			} catch (System.Exception ex) {
				DialogUtil.Open(ex.Message, DialogUtil.MarkType.Error, () => { });
			}
		}


		// Gene
		public void ImportGene () {
			DialogUtil.Dialog_OK_Cancel(LanguageData.UI_ImportGene, DialogUtil.MarkType.Warning, () => {
				try {
					var path = DialogUtil.PickFileDialog(LanguageData.UI_ImportGeneTitle, "json", "json");
					if (!Util.FileExists(path)) { return; }
					var gene = Data.Gene.JsonToGene(Util.FileToText(path));
					if (gene is null) { return; }
					Project.ProjectGene = gene;
					Project.SetDirty();
					RefreshGeneUI();
				} catch { }
			});
		}


		public void ExportGene () {
			try {
				var path = DialogUtil.CreateFileDialog(LanguageData.UI_ExportGeneTitle, "Gene", "json");
				if (string.IsNullOrEmpty(path)) { return; }
				Util.TextToFile(Data.Gene.GeneToJson(Project.ProjectGene), path);
				DialogUtil.Dialog_OK(LanguageData.UI_GeneExported, DialogUtil.MarkType.Success);
			} catch { }
		}


		#endregion




		#region --- LGC ---


		private void RefreshLogic () {
			RefreshProjectUI();
			RefreshGeneUI();
			RefreshBeatmapUI();
			RefreshPaletteUI();
			RefreshTweenUI();
			RefreshSoundUI();
		}


		private void RefreshProjectUI () {
			ReadyForUI = false;
			try {
				var bgSprite = Project.Background.sprite;
				var coverSprite = Project.FrontCover.sprite;
				m_ProjectInfoComponentData.ProjectName.text = Project.ProjectName;
				m_ProjectInfoComponentData.ProjectDescription.text = Project.ProjectDescription;
				m_ProjectInfoComponentData.BeatmapAuthor.text = Project.BeatmapAuthor;
				m_ProjectInfoComponentData.MusicAuthor.text = Project.MusicAuthor;
				m_ProjectInfoComponentData.BackgroundAuthor.text = Project.BackgroundAuthor;
				m_ProjectInfoComponentData.BackgroundThumbnail.sprite = bgSprite;
				m_ProjectInfoComponentData.BackgroundThumbnail.enabled = bgSprite;
				m_ProjectInfoComponentData.BackgroundThumbnail.preserveAspect = true;
				m_ProjectInfoComponentData.BackgroundFitter.aspectRatio = bgSprite is null ? 1f : bgSprite.rect.width / bgSprite.rect.height;
				m_ProjectInfoComponentData.CoverThumbnail.sprite = coverSprite;
				m_ProjectInfoComponentData.CoverThumbnail.enabled = coverSprite;
				m_ProjectInfoComponentData.CoverThumbnail.preserveAspect = true;
				m_ProjectInfoComponentData.CoverFitter.aspectRatio = coverSprite is null ? 1f : coverSprite.rect.width / coverSprite.rect.height;
				m_ProjectInfoComponentData.BackgroundHint.enabled = !Project.Background.sprite;
				m_ProjectInfoComponentData.CoverHint.enabled = !Project.FrontCover.sprite;
				m_ProjectInfoComponentData.MusicHint.text =
					Project.Music.data != null ?
					(Project.Music.data.Data.Length / 1024f / 1024f).ToString("0.0") + " MB" : "0 MB";
			} catch { }
			ReadyForUI = true;
		}


		private void RefreshBeatmapUI () {
			ReadyForUI = false;
			try {
				m_BeatmapContent.DestroyAllChildImmediately();
				int index = 0;
				foreach (var pair in Project.BeatmapMap) {
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
					item.Grab<RectTransform>("Loading Sign").gameObject.SetActive(key == Project.BeatmapKey);
					var tag = item.Grab<InputField>("Tag InputField");
					var level = item.Grab<InputField>("Level InputField");
					var speed = item.Grab<InputField>("Speed InputField");
					var bpm = item.Grab<InputField>("BPM InputField");
					var shift = item.Grab<InputField>("Shift InputField");
					var ratio = item.Grab<InputField>("Ratio InputField");
					foreach (var mTrigger in item.MenuTriggers) {
						mTrigger.CallbackRight.AddListener(OnMenuTrigger);
					}
					tag.text = map.Tag;
					level.text = map.Level.ToString();
					speed.text = map.DropSpeed.ToString();
					bpm.text = map.BPM.ToString();
					shift.text = map.Shift.ToString();
					ratio.text = map.Ratio.ToString();
					tag.onEndEdit.AddListener(OnTagEdit);
					level.onEndEdit.AddListener(OnLevelEdit);
					speed.onEndEdit.AddListener(OnSpeedEdit);
					bpm.onEndEdit.AddListener(OnBPMEdit);
					shift.onEndEdit.AddListener(OnShiftEdit);
					ratio.onEndEdit.AddListener(OnRatioEdit);
					index++;
					// Language Texts
					foreach (var text in item.LanguageTexts) {
						text.text = GetLanguage(text.name);
					}
					// Funcs
					void OnTagEdit (string txt) {
						if (Project.BeatmapMap.ContainsKey(key)) {
							var _map = Project.BeatmapMap[key];
							_map.Tag = txt;
							OnBeatmapInfoChanged(_map);
							Project.SetDirty();
						}
					}
					void OnLevelEdit (string txt) {
						if (Project.BeatmapMap.ContainsKey(key) && int.TryParse(txt, out int value)) {
							var _map = Project.BeatmapMap[key];
							_map.Level = value;
							OnBeatmapInfoChanged(_map);
							Project.SetDirty();
						}
					}
					void OnSpeedEdit (string txt) {
						if (Project.BeatmapMap.ContainsKey(key) && float.TryParse(txt, out float value)) {
							var _map = Project.BeatmapMap[key];
							_map.DropSpeed = value;
							OnBeatmapInfoChanged(_map);
							Project.SetDirty();
						}
					}
					void OnBPMEdit (string txt) {
						if (Project.BeatmapMap.ContainsKey(key) && float.TryParse(txt, out float value)) {
							var _map = Project.BeatmapMap[key];
							_map.BPM = value;
							OnBeatmapInfoChanged(_map);
							Project.SetDirty();
						}
					}
					void OnShiftEdit (string txt) {
						if (Project.BeatmapMap.ContainsKey(key) && float.TryParse(txt, out float value)) {
							var _map = Project.BeatmapMap[key];
							_map.Shift = value;
							OnBeatmapInfoChanged(_map);
							Project.SetDirty();
						}
					}
					void OnRatioEdit (string txt) {
						if (Project.BeatmapMap.ContainsKey(key) && float.TryParse(txt, out float value)) {
							var _map = Project.BeatmapMap[key];
							_map.Ratio = value;
							OnBeatmapInfoChanged(_map);
							Project.SetDirty();
						}
					}
					void OnMenuTrigger () => Menu.OpenMenu(MENU_KEY, rt);
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
			} catch { }
			ReadyForUI = true;
		}


		private void RefreshGeneUI () {
			ReadyForUI = false;
			try {
				// P
				for (int i = 0; i < m_ProjectInfoGeneData.Permissions.Length; i++) {
					var grab = m_ProjectInfoGeneData.Permissions[i];
					grab.Grab<Toggle>("Toggle").isOn = Project.ProjectGene.GetPermission(grab.name);
				}
				// R
				for (int i = 0; i < m_ProjectInfoGeneData.Ranges.Length; i++) {
					var grab = m_ProjectInfoGeneData.Ranges[i];
					var pr = Project.ProjectGene.GetRange(grab.name);
					var min = grab.Grab<InputField>("Min");
					var max = grab.Grab<InputField>("Max");
					min.interactable = pr.HasMin;
					max.interactable = pr.HasMax;
					min.text = pr.HasMin ? pr.Min.ToString() : "";
					max.text = pr.HasMax ? pr.Max.ToString() : "";
					grab.Grab<Toggle>("MinUse").isOn = pr.HasMin;
					grab.Grab<Toggle>("MaxUse").isOn = pr.HasMax;
					grab.Grab<RectTransform>("MinHint").gameObject.SetActive(!pr.HasMin);
					grab.Grab<RectTransform>("MaxHint").gameObject.SetActive(!pr.HasMax);
				}
				// L
				for (int i = 0; i < m_ProjectInfoGeneData.Limitations.Length; i++) {
					var grab = m_ProjectInfoGeneData.Limitations[i];
					var input = grab.Grab<InputField>("InputField");
					var limit = Project.ProjectGene.GetLimitation(grab.name);
					input.contentType = limit >= 0 ? InputField.ContentType.IntegerNumber : InputField.ContentType.Standard;
					input.text = limit >= 0 ? limit.ToString() : "∞";
				}
			} catch { }
			ReadyForUI = true;
		}


		private void RefreshPaletteUI () {
			ReadyForUI = false;
			try {
				m_PaletteContent.DestroyAllChildImmediately();
				foreach (var color in Project.Palette) {
					var item = SpawnItem(m_PaletteItemPrefab, m_PaletteContent);
					var rt = item.transform as RectTransform;
					int rtIndex = rt.GetSiblingIndex();
					var palColor = color;
					// Grab
					item.Grab<Image>().color = palColor;
					item.Grab<Text>("Index").text = rtIndex.ToString();
					var trigger = item.Grab<TriggerUI>();
					trigger.CallbackLeft.AddListener(() => {
						StagerStudio.Main.SpawnColorPicker(palColor, (_color) => {
							Project.SetPaletteColor(_color, rtIndex);
							RefreshPaletteUI();
						});
					});
					trigger.CallbackRight.AddListener(() => Menu.OpenMenu(MENU_PAL_KEY, rt));
				}
			} catch { }
			ReadyForUI = true;
		}


		private void RefreshTweenUI () {
			ReadyForUI = false;
			try {
				m_TweenContent.DestroyAllChildImmediately();
				foreach (var tween in Project.Tweens) {
					var curve = tween.curve;
					var item = SpawnItem(m_TweenItemPrefab, m_TweenContent);
					var rt = item.transform as RectTransform;
					int rtIndex = rt.GetSiblingIndex();
					item.Grab<Text>("Index").text = rtIndex.ToString();
					item.Grab<CurveUI>("Renderer").Curve = curve;
					var trigger = item.Grab<TriggerUI>();
					trigger.CallbackLeft.AddListener(() => StagerStudio.Main.SpawnTweenEditor(curve, (resultCurve) => {
						Project.SetTweenCurve(resultCurve, null, rtIndex);
						RefreshTweenUI();
					}));
					trigger.CallbackRight.AddListener(() => Menu.OpenMenu(MENU_TWEEN_KEY, rt));
					var oldColor = tween.color;
					item.Grab<Image>("Color").color = oldColor;
					item.Grab<Button>("Color").onClick.AddListener(() => {
						StagerStudio.Main.SpawnColorPicker(oldColor, (resultColor) => {
							Project.SetTweenCurve(null, resultColor, rtIndex);
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
				foreach (var (_, sClip) in Project.ClickSounds) {
					var item = SpawnItem(m_SoundItemPrefab, m_SoundContent);
					var rt = item.transform as RectTransform;
					int rtIndex = rt.GetSiblingIndex();
					// Grab
					item.Grab<Text>("Index").text = rtIndex.ToString();
					item.Grab<Text>("Name").text = sClip.name;
					item.Grab<Button>().onClick.AddListener(() => {
						Music.PlayClickSound(rtIndex, 1f);
					});
					item.Grab<TriggerUI>().CallbackRight.AddListener(() => {
						Menu.OpenMenu(MENU_SOUND_KEY, rt);
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