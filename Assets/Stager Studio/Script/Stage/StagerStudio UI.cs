namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Stage;
	using UI;
	using UnityEngine.UI;
	using Saving;


	public partial class StagerStudio { /// --- Spawn UI ---



		[System.Serializable]
		public enum InputType {
			Frame,

		}


		[System.Serializable]
		public enum ToggleType {
			UIScale_0,
			UIScale_1,
			UIScale_2,
			UseSFX,
			ShowZone,
			ShowWave,
			ShowPreview,
			ShowTip,
			ShowWelcome,
			SnapProgressbar,
			PositiveScroll,
			ShowIndexLabel,
			ShowGrid,
			ShowKeypress,

		}


		[System.Serializable]
		public enum SliderType {
			MusicVolume,
			SoundVolume,
			BgBrightness,

		}


		// Const
		private readonly static Dictionary<InputType, (System.Action<string> setHandler, System.Func<string> getHandler, SavingString saving, bool refreshUI)> InputItemMap = new Dictionary<InputType, (System.Action<string>, System.Func<string>, SavingString, bool)>();
		private readonly static Dictionary<ToggleType, (System.Action<bool> setHandler, System.Func<bool> getHandler, SavingBool saving, bool refreshUI)> ToggleItemMap = new Dictionary<ToggleType, (System.Action<bool>, System.Func<bool>, SavingBool, bool)>();
		private readonly static Dictionary<SliderType, (System.Action<float> setHandler, System.Func<float> getHandler, SavingFloat saving, bool refreshUI)> SliderItemMap = new Dictionary<SliderType, (System.Action<float>, System.Func<float>, SavingFloat, bool)>();

		// Short
		private bool ShowWelcome => ToggleItemMap[ToggleType.ShowWelcome].saving.Value;

		// Ser
		[Header("Spawn Prefab")]
		[SerializeField] private SettingUI m_SettingPrefab = null;
		[SerializeField] private ProjectInfoUI m_ProjectInfoPrefab = null;
		[SerializeField] private SkinEditorUI m_SkinEditorPrefab = null;
		[SerializeField] private DialogUI m_DialogPrefab = null;
		[SerializeField] private ColorPickerUI m_ColorPickerPrefab = null;
		[SerializeField] private TweenEditorUI m_TweenEditorPrefab = null;
		[SerializeField] private WelcomeUI m_WelcomePrefab = null;
		[SerializeField] private BeatmapSwiperUI m_BeatmapSwiperPrefab = null;
		[SerializeField] private SkinSwiperUI m_SkinSwiperPrefab = null;
		[SerializeField] private LoadingUI m_LoadingPrefab = null;
		[SerializeField] private ProjectCreatorUI m_ProjectCreatorPrefab = null;

		[Header("Spawn Root")]
		[SerializeField] private RectTransform m_Root = null;
		[SerializeField] private RectTransform m_SettingRoot = null;
		[SerializeField] private RectTransform m_ProjectInfoRoot = null;
		[SerializeField] private RectTransform m_SkinEditorRoot = null;
		[SerializeField] private RectTransform m_DialogRoot = null;
		[SerializeField] private RectTransform m_ColorPickerRoot = null;
		[SerializeField] private RectTransform m_TweenEditorRoot = null;
		[SerializeField] private RectTransform m_WelcomeRoot = null;
		[SerializeField] private RectTransform m_BeatmapSwiperRoot = null;
		[SerializeField] private RectTransform m_SkinSwiperRoot = null;
		[SerializeField] private RectTransform m_LoadingRoot = null;
		[SerializeField] private RectTransform m_ProjectCreatorRoot = null;


		// MSG
		private void Awake_Setting () {



			// Input
			InputItemMap.Add(InputType.Frame, ((str) => {
				if (int.TryParse(str, out int result)) {
					Application.targetFrameRate = Mathf.Clamp(result, 12, 1200);
					InputItemMap[InputType.Frame].saving.Value = result.ToString();
				}
			}, () => Application.targetFrameRate.ToString(), new SavingString("SS.FrameRate", "120"), true));



			// Toggle
			ToggleItemMap.Add(ToggleType.UIScale_0, ((isOn) => {
				if (isOn) {
					SetUIScale(0);
					ToggleItemMap[ToggleType.UIScale_0].saving.Value = true;
					ToggleItemMap[ToggleType.UIScale_1].saving.Value = false;
					ToggleItemMap[ToggleType.UIScale_2].saving.Value = false;
				}
			}, null, new SavingBool("SS.UIScale0", false), false));

			ToggleItemMap.Add(ToggleType.UIScale_1, ((isOn) => {
				if (isOn) {
					SetUIScale(1);
					ToggleItemMap[ToggleType.UIScale_0].saving.Value = false;
					ToggleItemMap[ToggleType.UIScale_1].saving.Value = true;
					ToggleItemMap[ToggleType.UIScale_2].saving.Value = false;
				}
			}, null, new SavingBool("SS.UIScale1", true), false));

			ToggleItemMap.Add(ToggleType.UIScale_2, ((isOn) => {
				if (isOn) {
					SetUIScale(2);
					ToggleItemMap[ToggleType.UIScale_0].saving.Value = false;
					ToggleItemMap[ToggleType.UIScale_1].saving.Value = false;
					ToggleItemMap[ToggleType.UIScale_2].saving.Value = true;
				}
			}, null, new SavingBool("SS.UIScale2", false), false));

			ToggleItemMap.Add(ToggleType.UseSFX, ((isOn) => {
				ToggleItemMap[ToggleType.UseSFX].saving.Value = isOn;
			}, null, SFX.UseFX, true));

			ToggleItemMap.Add(ToggleType.ShowZone, ((isOn) => {
				m_Zone.ShowZone(isOn);
				ToggleItemMap[ToggleType.ShowZone].saving.Value = isOn;
			}, () => m_Zone.IsShowing, new SavingBool("SS.ShowZone", true), true));

			ToggleItemMap.Add(ToggleType.ShowWave, ((isOn) => {
				m_Wave.enabled = isOn;
				ToggleItemMap[ToggleType.ShowWave].saving.Value = isOn;
			}, null, new SavingBool("SS.ShowWave", true), true));

			ToggleItemMap.Add(ToggleType.ShowPreview, ((isOn) => {
				m_Preview.Show(isOn);
				ToggleItemMap[ToggleType.ShowPreview].saving.Value = isOn;
			}, () => m_Preview.gameObject.activeSelf, new SavingBool("SS.ShowPreview", true), true));

			ToggleItemMap.Add(ToggleType.ShowTip, ((isOn) => {
				m_TipLabel.gameObject.SetActive(isOn);
				ToggleItemMap[ToggleType.ShowTip].saving.Value = isOn;
			}, () => m_TipLabel.gameObject.activeSelf, new SavingBool("SS.ShowTip", true), true));

			ToggleItemMap.Add(ToggleType.ShowWelcome, ((isOn) => {
				ToggleItemMap[ToggleType.ShowWelcome].saving.Value = isOn;
			}, null, new SavingBool("SS.ShowWelcome", true), true));

			ToggleItemMap.Add(ToggleType.SnapProgressbar, ((isOn) => {
				m_Progress.Snap = isOn;
				ToggleItemMap[ToggleType.SnapProgressbar].saving.Value = isOn;
			}, () => m_Progress.Snap, new SavingBool("SS.SnapProgress", true), true));

			ToggleItemMap.Add(ToggleType.PositiveScroll, ((isOn) => {
				Game.PositiveScroll = isOn;
				ToggleItemMap[ToggleType.PositiveScroll].saving.Value = isOn;
			}, () => Game.PositiveScroll, new SavingBool("StageGame.PositiveScroll", true), true));

			ToggleItemMap.Add(ToggleType.ShowIndexLabel, ((isOn) => {
				Object.StageObject.ShowIndexLabel = isOn;
				ToggleItemMap[ToggleType.ShowIndexLabel].saving.Value = isOn;
			}, () => Object.StageObject.ShowIndexLabel, new SavingBool("StageGame.ShowIndexLabel", false), true));

			ToggleItemMap.Add(ToggleType.ShowGrid, ((isOn) => {
				Game.SetShowGrid(isOn);
			}, () => Game.ShowGrid, Game.ShowGrid, true));

			ToggleItemMap.Add(ToggleType.ShowKeypress, ((isOn) => {
				m_Keypress.gameObject.SetActive(isOn);
				ToggleItemMap[ToggleType.ShowKeypress].saving.Value = isOn;
			}, () => m_Keypress.gameObject.activeSelf, new SavingBool("StageGame.ShowKeypress", false), true));



			// Slider
			SliderItemMap.Add(SliderType.MusicVolume, ((value) => {
				value = Mathf.Clamp01(value / 12f);
				Music.Volume = value;
				SliderItemMap[SliderType.MusicVolume].saving.Value = value * 12f;
			}, () => Music.Volume * 12f, new SavingFloat("SS.MusicVolume", 6f), true));

			SliderItemMap.Add(SliderType.SoundVolume, ((value) => {
				value = Mathf.Clamp01(value / 12f);
				Music.SfxVolume = value;
				SliderItemMap[SliderType.SoundVolume].saving.Value = value * 12f;
			}, () => Music.SfxVolume * 12f, new SavingFloat("SS.SoundVolume", 6f), true));

			SliderItemMap.Add(SliderType.BgBrightness, ((value) => {
				value = Mathf.Clamp01(value / 12f);
				m_Background.SetBrightness(value);
				SliderItemMap[SliderType.BgBrightness].saving.Value = value * 12f;
			}, () => m_Background.Brightness * 12f, new SavingFloat("SS.BgBrightness", 3.7f), true));

		}


		// UI
		public void UI_SpawnSetting () {
			UI_RemoveUI();
			var setting = Util.SpawnUI(m_SettingPrefab, m_SettingRoot, "Setting");
			var language = FindObjectOfType<StageLanguage>();

			LoadAllSettings();

			// Handler
			setting.GetLanguage = language.Get;
			setting.ResetAllSettings = () => {
				ResetAllSettings();
				LoadAllSettings();
			};

			// Init Components
			setting.Component.ForAllInputs((item) => {
				var (set, get, saving, refreshUI) = InputItemMap[item.Type];
				item.InitItem((str) => {
					if (!setting.UIReady) { return; }
					set(str);
					if (refreshUI) {
						setting.RefreshLogic(false);
					}
				});
				item.GetHandler = get ?? (() => saving);
			});
			setting.Component.ForAllToggles((item) => {
				var (set, get, saving, refreshUI) = ToggleItemMap[item.Type];
				item.InitItem((isOn) => {
					if (!setting.UIReady) { return; }
					set(isOn);
					if (refreshUI) {
						setting.RefreshLogic(false);
					}
				});
				item.GetHandler = get ?? (() => saving);
			});
			setting.Component.ForAllSliders((item) => {
				var (set, get, saving, refreshUI) = SliderItemMap[item.Type];
				item.InitItem((value) => {
					if (!setting.UIReady) { return; }
					set(value);
					if (refreshUI) {
						setting.RefreshLogic(false);
					}
				});
				item.GetHandler = get ?? (() => saving);
			});
			setting.Init();
		}


		public void UI_SpawnProjectInfo () {
			UI_RemoveUI();
			Util.SpawnUI(m_ProjectInfoPrefab, m_ProjectInfoRoot, "Project Info").Refresh();
		}


		public void SpawnWelcome () {
			UI_RemoveUI();
			Util.SpawnUI(m_WelcomePrefab, m_WelcomeRoot, "Welcome").Init(Project.ProjectName, Project.BackgroundAuthor, Project.MusicAuthor, Project.BeatmapAuthor, Project.ProjectDescription, Project.FrontCover.sprite);
		}


		public void UI_SpawnBeatmapSwiper () {
			UI_RemoveUI();
			Util.SpawnUI(m_BeatmapSwiperPrefab, m_BeatmapSwiperRoot, "Beatmap Swiper").Init(Project);
		}


		public void UI_SpawnSkinSwiper () {
			UI_RemoveUI();
			Util.SpawnUI(m_SkinSwiperPrefab, m_SkinSwiperRoot, "Skin Swiper").Init(Skin);
		}


		public void SpawnProjectCreator (string root) {
			UI_RemoveUI();
			Util.SpawnUI(m_ProjectCreatorPrefab, m_ProjectCreatorRoot, "Project Creator").Init(root);
		}


		public void UI_RemoveUI () {

			m_SettingRoot.DestroyAllChildImmediately();
			m_ProjectInfoRoot.DestroyAllChildImmediately();
			m_SkinEditorRoot.DestroyAllChildImmediately();
			m_ColorPickerRoot.DestroyAllChildImmediately();
			m_TweenEditorRoot.DestroyAllChildImmediately();
			m_WelcomeRoot.DestroyAllChildImmediately();
			m_BeatmapSwiperRoot.DestroyAllChildImmediately();
			m_LoadingRoot.DestroyAllChildImmediately();

			m_SettingRoot.gameObject.SetActive(false);
			m_ProjectInfoRoot.gameObject.SetActive(false);
			m_SkinEditorRoot.gameObject.SetActive(false);
			m_ColorPickerRoot.gameObject.SetActive(false);
			m_TweenEditorRoot.gameObject.SetActive(false);
			m_WelcomeRoot.gameObject.SetActive(false);
			m_BeatmapSwiperRoot.gameObject.SetActive(false);
			m_LoadingRoot.gameObject.SetActive(false);

			m_Root.InactiveIfNoChildActive();

			Music.Pause();
		}


		private void RemoveLoading () {
			Main.m_LoadingRoot.DestroyAllChildImmediately();
			Main.m_LoadingRoot.gameObject.SetActive(false);
			Main.m_LoadingRoot.parent.InactiveIfNoChildActive();
		}


		// Color Picker
		public void SpawnColorPicker (Color color, System.Action<Color> done) {
			m_ColorPickerRoot.gameObject.SetActive(true);
			m_ColorPickerRoot.parent.gameObject.SetActive(true);
			Music.Pause();
			Util.SpawnUI(Main.m_ColorPickerPrefab, Main.m_ColorPickerRoot, "Color Picker").Init(color, done);
		}


		// Tween Editor
		public void SpawnTweenEditor (AnimationCurve curve, System.Action<AnimationCurve> done) {
			m_TweenEditorRoot.gameObject.SetActive(true);
			m_TweenEditorRoot.parent.gameObject.SetActive(true);
			Music.Pause();
			Util.SpawnUI(Main.m_TweenEditorPrefab, Main.m_TweenEditorRoot, "Tween Editor").Init(curve, done);
		}


		// Skin Editor
		public void UI_SpawnSkinEditor () => SpawnSkinEditor(StageSkin.Data.Name, false);


		public void SpawnSkinEditor (string skinName, bool openSettingAfterClose) {
			if (string.IsNullOrEmpty(skinName)) { return; }
			UI_RemoveUI();
			Util.SpawnUI(Main.m_SkinEditorPrefab, Main.m_SkinEditorRoot, "Skin Editor").Init(
				Main.Skin.GetSkinFromDisk(skinName), skinName, openSettingAfterClose
			);
		}


		// Project Info
		public void UI_ProjectInfo_DeletePaletteItem (object palRT) {
			if (palRT is null || !(palRT is RectTransform)) { return; }
			int index = (palRT as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(Language.Get(LanguageData.Confirm_DeleteProjectPal), index),
				DialogUtil.MarkType.Warning, null, null, () => {
					Project.RemovePaletteAt(index);
					TryRefreshProjectInfo();
					StageUndo.ClearUndo();
				}, null, () => { }
			);
		}


		public void UI_ProjectInfo_DeleteClickSound (object rtObj) {
			if (rtObj is null || !(rtObj is RectTransform)) { return; }
			int index = (rtObj as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(Language.Get(LanguageData.Confirm_DeleteProjectSound), index),
				DialogUtil.MarkType.Warning, null, null, () => {
					Project.RemoveClickSound(index);
					TryRefreshProjectInfo();
					StageUndo.ClearUndo();
				}, null, () => { }
			);
		}


		public void UI_ProjectInfo_DeleteTween (object rtObj) {
			if (rtObj is null || !(rtObj is RectTransform)) { return; }
			int index = (rtObj as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(Language.Get(LanguageData.Confirm_DeleteProjectTween), index),
				DialogUtil.MarkType.Warning, null, null, () => {
					Project.RemoveTweenAt(index);
					TryRefreshProjectInfo();
					StageUndo.ClearUndo();
				}, null, () => { }
			);
		}


		// LGC
		private void LoadAllSettings () {
			foreach (var pair in InputItemMap) {
				pair.Value.setHandler(pair.Value.saving);
			}
			foreach (var pair in ToggleItemMap) {
				pair.Value.setHandler(pair.Value.saving);
			}
			foreach (var pair in SliderItemMap) {
				pair.Value.setHandler(pair.Value.saving);
			}
		}


		private void ResetAllSettings () {
			foreach (var pair in InputItemMap) {
				pair.Value.saving.Reset();
			}
			foreach (var pair in ToggleItemMap) {
				pair.Value.saving.Reset();
			}
			foreach (var pair in SliderItemMap) {
				pair.Value.saving.Reset();
			}
		}


		// Setting Logic
		private void SetUIScale (int uiScale) {
			uiScale = Mathf.Clamp(uiScale, 0, 2);
			var scalers = m_CanvasRoot.GetComponentsInChildren<CanvasScaler>(true);
			float height = uiScale == 0 ? 1000 : uiScale == 1 ? 800 : 600;
			foreach (var scaler in scalers) {
				scaler.referenceResolution = new Vector2(scaler.referenceResolution.x, height);
			}
		}


	}
}