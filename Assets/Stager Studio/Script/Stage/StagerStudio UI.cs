namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Stage;
	using UI;
	using UnityEngine.UI;
	using Saving;
	using UndoRedo;


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
			ShowZone, // Removed
			ShowWave,
			ShowPreview,
			ShowTip,
			SnapProgressbar,
			PositiveScroll,
			ShowIndexLabel,
			ShowGrid,
			ShowKeypress,
			BrushScale,
			ShowGridOnSelect,
			SoloOnEditMotion,
			UseEditorEffect,

		}


		[System.Serializable]
		public enum SliderType {
			MusicVolume,
			SoundVolume,
			BgBrightness,
			StageBrushWidth,
			StageBrushHeight,
			TrackBrushWidth,
			NoteBrushWidth,
		}


		// Const
		private readonly static Dictionary<InputType, (System.Action<string> setHandler, System.Func<string> getHandler, SavingString saving, bool refreshUI)> InputItemMap = new Dictionary<InputType, (System.Action<string>, System.Func<string>, SavingString, bool)>();
		private readonly static Dictionary<ToggleType, (System.Action<bool> setHandler, System.Func<bool> getHandler, SavingBool saving, bool refreshUI)> ToggleItemMap = new Dictionary<ToggleType, (System.Action<bool>, System.Func<bool>, SavingBool, bool)>();
		private readonly static Dictionary<SliderType, (System.Action<float> setHandler, System.Func<float> getHandler, SavingFloat saving, bool refreshUI)> SliderItemMap = new Dictionary<SliderType, (System.Action<float>, System.Func<float>, SavingFloat, bool)>();

		// Ser
		[Header("Spawn Prefab")]
		[SerializeField] private SettingUI m_SettingPrefab = null;
		[SerializeField] private ProjectInfoUI m_ProjectInfoPrefab = null;
		[SerializeField] private SkinEditorUI m_SkinEditorPrefab = null;
		[SerializeField] private DialogUI m_DialogPrefab = null;
		[SerializeField] private ColorPickerUI m_ColorPickerPrefab = null;
		[SerializeField] private TweenEditorUI m_TweenEditorPrefab = null;
		[SerializeField] private BeatmapSwiperUI m_BeatmapSwiperPrefab = null;
		[SerializeField] private SkinSwiperUI m_SkinSwiperPrefab = null;
		[SerializeField] private LoadingUI m_LoadingPrefab = null;
		[SerializeField] private ProjectCreatorUI m_ProjectCreatorPrefab = null;
		[SerializeField] private CommandUI m_CommandPrefab = null;

		[Header("Spawn Root")]
		[SerializeField] private RectTransform m_Root = null;
		[SerializeField] private RectTransform m_SettingRoot = null;
		[SerializeField] private RectTransform m_ProjectInfoRoot = null;
		[SerializeField] private RectTransform m_SkinEditorRoot = null;
		[SerializeField] private RectTransform m_DialogRoot = null;
		[SerializeField] private RectTransform m_ColorPickerRoot = null;
		[SerializeField] private RectTransform m_TweenEditorRoot = null;
		[SerializeField] private RectTransform m_BeatmapSwiperRoot = null;
		[SerializeField] private RectTransform m_SkinSwiperRoot = null;
		[SerializeField] private RectTransform m_LoadingRoot = null;
		[SerializeField] private RectTransform m_ProjectCreatorRoot = null;
		[SerializeField] private RectTransform m_CommandRoot = null;


		// MSG
		private void Awake_Setting_UI () {


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
				m_SoundFX.SetUseFX(isOn);
				ToggleItemMap[ToggleType.UseSFX].saving.Value = isOn;
			}, null, m_SoundFX.UseFX, true));

			ToggleItemMap.Add(ToggleType.ShowWave, ((isOn) => {
				m_Wave.enabled = isOn;
				ToggleItemMap[ToggleType.ShowWave].saving.Value = isOn;
			}, null, new SavingBool("SS.ShowWave", true), true));

			ToggleItemMap.Add(ToggleType.ShowPreview, ((isOn) => {
				m_Preview.Show(isOn);
				ToggleItemMap[ToggleType.ShowPreview].saving.Value = isOn;
			}, () => m_Preview.gameObject.activeSelf, new SavingBool("SS.ShowPreview", true), true));

			ToggleItemMap.Add(ToggleType.ShowTip, ((isOn) => {
				TooltipUI.ShowTip.Value = isOn;
			}, null, TooltipUI.ShowTip, true));

			ToggleItemMap.Add(ToggleType.SnapProgressbar, ((isOn) => {
				m_Progress.Snap = isOn;
				ToggleItemMap[ToggleType.SnapProgressbar].saving.Value = isOn;
			}, () => m_Progress.Snap, new SavingBool("SS.SnapProgress", true), true));

			ToggleItemMap.Add(ToggleType.PositiveScroll, ((isOn) => {
				m_Game.PositiveScroll = isOn;
				ToggleItemMap[ToggleType.PositiveScroll].saving.Value = isOn;
			}, () => m_Game.PositiveScroll, new SavingBool("StageGame.PositiveScroll", true), true));

			ToggleItemMap.Add(ToggleType.ShowIndexLabel, ((isOn) => {
				Object.StageObject.ShowIndexLabel = isOn;
				ToggleItemMap[ToggleType.ShowIndexLabel].saving.Value = isOn;
			}, () => Object.StageObject.ShowIndexLabel, new SavingBool("StageGame.ShowIndexLabel", false), true));

			ToggleItemMap.Add(ToggleType.ShowGrid, ((isOn) => {
				m_Game.SetShowGrid(isOn);
			}, () => m_Game.ShowGrid, m_Game.ShowGrid, true));

			ToggleItemMap.Add(ToggleType.ShowKeypress, ((isOn) => {
				m_Keypress.gameObject.SetActive(isOn);
				ToggleItemMap[ToggleType.ShowKeypress].saving.Value = isOn;
			}, () => m_Keypress.gameObject.activeSelf, new SavingBool("StageGame.ShowKeypress", false), true));

			ToggleItemMap.Add(ToggleType.BrushScale, ((isOn) => {
				m_Editor.UseGlobalBrushScale.Value = isOn;
			}, () => m_Editor.UseGlobalBrushScale.Value, m_Editor.UseGlobalBrushScale, true));

			ToggleItemMap.Add(ToggleType.ShowGridOnSelect, ((isOn) => {
				m_Editor.ShowGridOnSelect.Value = isOn;
			}, () => m_Editor.ShowGridOnSelect.Value, m_Editor.ShowGridOnSelect, true));

			ToggleItemMap.Add(ToggleType.SoloOnEditMotion, ((isOn) => {
				SoloOnEditMotion.Value = isOn;
			}, () => SoloOnEditMotion.Value, SoloOnEditMotion, true));

			ToggleItemMap.Add(ToggleType.UseEditorEffect, ((isOn) => {
				m_Effect.UseEffect.Value = isOn;
			}, () => m_Effect.UseEffect.Value, m_Effect.UseEffect, true));


			// Slider
			SliderItemMap.Add(SliderType.MusicVolume, ((value) => {
				value = Mathf.Clamp01(value / 12f);
				m_Music.Volume = value;
				SliderItemMap[SliderType.MusicVolume].saving.Value = value * 12f;
			}, () => m_Music.Volume * 12f, new SavingFloat("SS.MusicVolume", 6f), true));

			SliderItemMap.Add(SliderType.SoundVolume, ((value) => {
				value = Mathf.Clamp01(value / 12f);
				m_Music.SfxVolume = value;
				SliderItemMap[SliderType.SoundVolume].saving.Value = value * 12f;
			}, () => m_Music.SfxVolume * 12f, new SavingFloat("SS.SoundVolume", 6f), true));

			SliderItemMap.Add(SliderType.BgBrightness, ((value) => {
				m_Background.SetBrightness(Mathf.Clamp01(value / 12f));
				SliderItemMap[SliderType.BgBrightness].saving.Value = value;
			}, () => m_Background.Brightness * 12f, new SavingFloat("SS.BgBrightness", 3.7f), true));

			SliderItemMap.Add(SliderType.StageBrushWidth, ((value) => {
				m_Editor.StageBrushWidth = Mathf.Clamp01(value / 12f);
				SliderItemMap[SliderType.StageBrushWidth].saving.Value = value;
			}, null, new SavingFloat("SS.StageBrushWidth", 12f), true));

			SliderItemMap.Add(SliderType.StageBrushHeight, ((value) => {
				m_Editor.StageBrushHeight = Mathf.Clamp01(value / 12f);
				SliderItemMap[SliderType.StageBrushHeight].saving.Value = value;
			}, null, new SavingFloat("SS.StageBrushHeight", 12f), true));

			SliderItemMap.Add(SliderType.TrackBrushWidth, ((value) => {
				m_Editor.TrackBrushWidth = Mathf.Clamp01(value / 12f);
				SliderItemMap[SliderType.TrackBrushWidth].saving.Value = value;
			}, null, new SavingFloat("SS.TrackBrushWidth", 2.5f), true));

			SliderItemMap.Add(SliderType.NoteBrushWidth, ((value) => {
				m_Editor.NoteBrushWidth = Mathf.Clamp01(value / 12f);
				SliderItemMap[SliderType.NoteBrushWidth].saving.Value = value;
			}, null, new SavingFloat("SS.NoteBrushWidth", 2.5f), true));


		}


		// UI
		public void UI_SpawnSetting () {

			UI_RemoveUI();

			var setting = Util.SpawnUI(m_SettingPrefab, m_SettingRoot, "Setting");

			LoadAllSettings();

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


		public void UI_SpawnBeatmapSwiper () {
			UI_RemoveUI();
			Util.SpawnUI(m_BeatmapSwiperPrefab, m_BeatmapSwiperRoot, "Beatmap Swiper").Init();
		}


		public void UI_SpawnSkinSwiper () {
			UI_RemoveUI();
			Util.SpawnUI(m_SkinSwiperPrefab, m_SkinSwiperRoot, "Skin Swiper").Init();
		}


		public void SpawnProjectCreator (string root) {
			UI_RemoveUI();
			Util.SpawnUI(m_ProjectCreatorPrefab, m_ProjectCreatorRoot, "Project Creator").Init(root);
		}


		public void UI_SpawnCommand () {
			UI_RemoveUI();
			Util.SpawnUI(m_CommandPrefab, m_CommandRoot, "Command");
		}


		public void UI_RemoveUI () {

			m_SettingRoot.DestroyAllChildImmediately();
			m_ProjectInfoRoot.DestroyAllChildImmediately();
			m_SkinEditorRoot.DestroyAllChildImmediately();
			//m_DialogRoot.DestroyAllChildImmediately();
			//m_ColorPickerRoot.DestroyAllChildImmediately();
			m_TweenEditorRoot.DestroyAllChildImmediately();
			m_BeatmapSwiperRoot.DestroyAllChildImmediately();
			m_SkinSwiperRoot.DestroyAllChildImmediately();
			m_LoadingRoot.DestroyAllChildImmediately();
			m_ProjectCreatorRoot.DestroyAllChildImmediately();
			m_CommandRoot.DestroyAllChildImmediately();

			m_SettingRoot.gameObject.SetActive(false);
			m_ProjectInfoRoot.gameObject.SetActive(false);
			m_SkinEditorRoot.gameObject.SetActive(false);
			//m_DialogRoot.gameObject.SetActive(false);
			//m_ColorPickerRoot.gameObject.SetActive(false);
			m_TweenEditorRoot.gameObject.SetActive(false);
			m_BeatmapSwiperRoot.gameObject.SetActive(false);
			m_SkinSwiperRoot.gameObject.SetActive(false);
			m_LoadingRoot.gameObject.SetActive(false);
			m_ProjectCreatorRoot.gameObject.SetActive(false);
			m_CommandRoot.gameObject.SetActive(false);

			m_Root.InactiveIfNoChildActive();
			m_Inspector.StopEditMotion(false);

			MusicPause();
		}


		private void RemoveLoading () {
			m_LoadingRoot.DestroyAllChildImmediately();
			m_LoadingRoot.gameObject.SetActive(false);
			m_LoadingRoot.parent.InactiveIfNoChildActive();
		}


		// Color Picker
		public void SpawnColorPicker (Color color, System.Action<Color> done) {
			m_ColorPickerRoot.gameObject.SetActive(true);
			m_ColorPickerRoot.parent.gameObject.SetActive(true);
			MusicPause();
			Util.SpawnUI(m_ColorPickerPrefab, m_ColorPickerRoot, "Color Picker").Init(color, done);
		}


		// Tween Editor
		public void SpawnTweenEditor (AnimationCurve curve, System.Action<AnimationCurve> done) {
			m_TweenEditorRoot.gameObject.SetActive(true);
			m_TweenEditorRoot.parent.gameObject.SetActive(true);
			MusicPause();
			Util.SpawnUI(m_TweenEditorPrefab, m_TweenEditorRoot, "Tween Editor").Init(curve, done);
		}


		// Skin Editor
		public void UI_SpawnSkinEditor () => SpawnSkinEditor(StageSkin.Data.Name, false);


		public void SpawnSkinEditor (string skinName, bool openSettingAfterClose) {
			if (string.IsNullOrEmpty(skinName)) { return; }
			UI_RemoveUI();
			Util.SpawnUI(m_SkinEditorPrefab, m_SkinEditorRoot, "Skin Editor").Init(
				GetSkinFromDisk(skinName), skinName, openSettingAfterClose
			);
		}


		// Project Info
		public void UI_ProjectInfo_DeletePaletteItem (object palRT) {
			if (palRT is null || !(palRT is RectTransform)) { return; }
			int index = (palRT as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(GetLanguage(Confirm_DeleteProjectPal), index),
				DialogUtil.MarkType.Warning, null, null, () => {
					ProjectRemovePaletteAt(index);
					TryRefreshProjectInfo();
					UndoRedo.ClearUndo();
				}, null, () => { }
			);
		}


		public void UI_ProjectInfo_DeleteClickSound (object rtObj) {
			if (rtObj is null || !(rtObj is RectTransform)) { return; }
			int index = (rtObj as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(GetLanguage(Confirm_DeleteProjectSound), index),
				DialogUtil.MarkType.Warning, null, null, () => {
					ProjectRemoveClickSound(index);
					TryRefreshProjectInfo();
					UndoRedo.ClearUndo();
				}, null, () => { }
			);
		}


		public void UI_ProjectInfo_DeleteTween (object rtObj) {
			if (rtObj is null || !(rtObj is RectTransform)) { return; }
			int index = (rtObj as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(GetLanguage(Confirm_DeleteProjectTween), index),
				DialogUtil.MarkType.Warning, null, null, () => {
					ProjectRemoveTweenAt(index);
					TryRefreshProjectInfo();
					UndoRedo.ClearUndo();
				}, null, () => { }
			);
		}


		// Command
		public void UI_TrySetCommand_Target (int index) {
			var command = m_CommandRoot.childCount > 0 ? m_CommandRoot.GetChild(0).GetComponent<CommandUI>() : null;
			if (command != null) {
				command.SetTargetIndex(index);
			}
		}


		public void UI_TrySetCommand_Command (int index) {
			var command = m_CommandRoot.childCount > 0 ? m_CommandRoot.GetChild(0).GetComponent<CommandUI>() : null;
			if (command != null) {
				command.SetCommandIndex(index);
			}
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