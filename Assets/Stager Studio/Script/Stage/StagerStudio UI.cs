namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Stage;
	using UI;
	using UnityEngine.UI;
	using Saving;
	using UndoRedo;
	using DebugLog;


	public partial class StagerStudio { /// --- Spawn UI ---




		#region --- SUB ---


		[System.Serializable]
		public enum InputType {
			Frame,
			GridCount_X0_S,
			GridCount_X0_M,
			GridCount_X0_L,
			GridCount_X1_S,
			GridCount_X1_M,
			GridCount_X1_L,
			GridCount_X2_S,
			GridCount_X2_M,
			GridCount_X2_L,
			GridCount_Y_S,
			GridCount_Y_M,
			GridCount_Y_L,
			StageBrushWidth,
			StageBrushHeight,
			TrackBrushWidth,
			NoteBrushWidth,

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
			ShowTimerOnPlay,
			Log,

		}


		[System.Serializable]
		public enum SliderType {
			MusicVolume,
			SoundVolume,
			BgBrightness,

		}


		#endregion




		#region --- VAR ---


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
		[SerializeField] private TweenSelectorUI m_TweenSelectorPrefab = null;

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
		[SerializeField] private RectTransform m_TweenSelectorRoot = null;


		#endregion




		#region --- MSG ---


		private void Awake_Setting_UI_Input () {

			// Input
			InputItemMap.Add(
				InputType.Frame,
				((str) => {
					if (int.TryParse(str, out int result)) {
						Application.targetFrameRate = Mathf.Clamp(result, 12, 3600);
						InputItemMap[InputType.Frame].saving.Value = result.ToString();
					}
				},
				() => Application.targetFrameRate.ToString(),
				new SavingString("SS.FrameRate", "120"),
				true
			));

			// X0
			InputItemMap.Add(
				InputType.GridCount_X0_S,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(0, 0, result);
					}
					InputItemMap[InputType.GridCount_X0_S].saving.Value = m_Game.GetGridCount(0, 0).ToString();
				},
				null,
				new SavingString("SS.GridCount_X0_S", "1"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_X0_M,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(0, 1, result);
					}
					InputItemMap[InputType.GridCount_X0_M].saving.Value = m_Game.GetGridCount(0, 1).ToString();
				},
				null,
				new SavingString("SS.GridCount_X0_M", "7"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_X0_L,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(0, 2, result);
					}
					InputItemMap[InputType.GridCount_X0_L].saving.Value = m_Game.GetGridCount(0, 2).ToString();
				},
				null,
				new SavingString("SS.GridCount_X0_L", "15"),
				true
			));

			// X1
			InputItemMap.Add(
				InputType.GridCount_X1_S,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(1, 0, result);
					}
					InputItemMap[InputType.GridCount_X1_S].saving.Value = m_Game.GetGridCount(1, 0).ToString();
				},
				null,
				new SavingString("SS.GridCount_X1_S", "1"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_X1_M,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(1, 1, result);
					}
					InputItemMap[InputType.GridCount_X1_M].saving.Value = m_Game.GetGridCount(1, 1).ToString();
				},
				null,
				new SavingString("SS.GridCount_X1_M", "7"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_X1_L,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(1, 2, result);
					}
					InputItemMap[InputType.GridCount_X1_L].saving.Value = m_Game.GetGridCount(1, 2).ToString();
				},
				null,
				new SavingString("SS.GridCount_X1_L", "15"),
				true
			));

			// X2
			InputItemMap.Add(
				InputType.GridCount_X2_S,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(2, 0, result);
					}
					InputItemMap[InputType.GridCount_X2_S].saving.Value = m_Game.GetGridCount(2, 0).ToString();
				},
				null,
				new SavingString("SS.GridCount_X2_S", "1"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_X2_M,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(2, 1, result);
					}
					InputItemMap[InputType.GridCount_X2_M].saving.Value = m_Game.GetGridCount(2, 1).ToString();
				},
				null,
				new SavingString("SS.GridCount_X2_M", "7"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_X2_L,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(2, 2, result);
					}
					InputItemMap[InputType.GridCount_X2_L].saving.Value = m_Game.GetGridCount(2, 2).ToString();
				},
				null,
				new SavingString("SS.GridCount_X2_L", "15"),
				true
			));

			// Y
			InputItemMap.Add(
				InputType.GridCount_Y_S,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(3, 0, result);
					}
					InputItemMap[InputType.GridCount_Y_S].saving.Value = m_Game.GetGridCount(3, 0).ToString();
				},
				null,
				new SavingString("SS.GridCount_Y_S", "2"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_Y_M,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(3, 1, result);
					}
					InputItemMap[InputType.GridCount_Y_M].saving.Value = m_Game.GetGridCount(3, 1).ToString();
				},
				null,
				new SavingString("SS.GridCount_Y_M", "4"),
				true
			));

			InputItemMap.Add(
				InputType.GridCount_Y_L,
				((str) => {
					if (int.TryParse(str, out int result)) {
						m_Game.SetGridCount(3, 2, result);
					}
					InputItemMap[InputType.GridCount_Y_L].saving.Value = m_Game.GetGridCount(3, 2).ToString();
				},
				null,
				new SavingString("SS.GridCount_Y_L", "8"),
				true
			));

			// Brush
			InputItemMap.Add(InputType.StageBrushWidth, ((str) => {
				if (str.TryParseFloatForInspector(out float result)) {
					SetBrushSize(0, 0, result);
				}
			}, null, new SavingString("SS.StageBrushWidth", "1"), true));

			InputItemMap.Add(InputType.StageBrushHeight, ((str) => {
				if (str.TryParseFloatForInspector(out float result)) {
					SetBrushSize(0, 1, result);
				}
			}, null, new SavingString("SS.StageBrushHeight", "1"), true));

			InputItemMap.Add(InputType.TrackBrushWidth, ((str) => {
				if (str.TryParseFloatForInspector(out float result)) {
					SetBrushSize(1, 0, result);
				}
			}, null, new SavingString("SS.TrackBrushWidth", "0.2"), true));

			InputItemMap.Add(InputType.NoteBrushWidth, ((str) => {
				if (str.TryParseFloatForInspector(out float result)) {
					SetBrushSize(2, 0, result);
				}
			}, null, new SavingString("SS.NoteBrushWidth", "0.2"), true));

		}


		private void Awake_Setting_UI_Toggle () {

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
				m_GlobalBrushMark.gameObject.TrySetActive(isOn);
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

			ToggleItemMap.Add(ToggleType.ShowTimerOnPlay, ((isOn) => {
				m_Game.ShowGridTimerOnPlay.Value = isOn;
			}, () => m_Game.ShowGridTimerOnPlay.Value, m_Game.ShowGridTimerOnPlay, true));

			ToggleItemMap.Add(ToggleType.Log, ((isOn) => {
				DebugLog.UseLog = isOn;
				ToggleItemMap[ToggleType.Log].saving.Value = isOn;
			}, () => DebugLog.UseLog, new SavingBool("SS.UseLog", true), true));

		}


		private void Awake_Setting_UI_Slider () {

			// Slider
			SliderItemMap.Add(SliderType.MusicVolume, ((value) => {
				SetMusicVolume(value / 12f);
				//value = Mathf.Clamp01(value / 12f);
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

		}


		#endregion



		#region --- API ---


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


		public void UI_SpawnProjectCreator (string root) {
			UI_RemoveUI();
			Util.SpawnUI(m_ProjectCreatorPrefab, m_ProjectCreatorRoot, "Project Creator").Init(root);
		}


		public void UI_SpawnCommand () {
			UI_RemoveUI();
			Util.SpawnUI(m_CommandPrefab, m_CommandRoot, "Command");
		}


		public void UI_SpawnTweenSelector () {
			//UI_RemoveUI();
			Util.SpawnUI(m_TweenSelectorPrefab, m_TweenSelectorRoot, "TweenSelector").Open();
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
			m_TweenSelectorRoot.DestroyAllChildImmediately();

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
			m_TweenSelectorRoot.gameObject.SetActive(false);

			m_Root.InactiveIfNoChildActive();
			m_Inspector.StopEditMotion(false);

			m_Music.Pause();
		}


		public void UI_RemoveTweenSelector () {
			m_TweenSelectorRoot.DestroyAllChildImmediately();
			m_TweenSelectorRoot.gameObject.SetActive(false);
			m_Root.InactiveIfNoChildActive();
		}


		// Color Picker
		public void SpawnColorPicker (Color color, System.Action<Color> done) {
			m_ColorPickerRoot.gameObject.SetActive(true);
			m_ColorPickerRoot.parent.gameObject.SetActive(true);
			m_Music.Pause();
			Util.SpawnUI(m_ColorPickerPrefab, m_ColorPickerRoot, "Color Picker").Init(color, done);
		}


		// Tween Editor
		public void SpawnTweenEditor (AnimationCurve curve, System.Action<AnimationCurve> done) {
			m_TweenEditorRoot.gameObject.SetActive(true);
			m_TweenEditorRoot.parent.gameObject.SetActive(true);
			m_Music.Pause();
			Util.SpawnUI(m_TweenEditorPrefab, m_TweenEditorRoot, "Tween Editor").Init(curve, done);
		}


		// Skin Editor
		public void UI_SpawnSkinEditor () => SpawnSkinEditor(StageSkin.Data.Name, false);


		public void SpawnSkinEditor (string skinName, bool openSettingAfterClose) {
			if (string.IsNullOrEmpty(skinName)) { return; }
			UI_RemoveUI();
			Util.SpawnUI(m_SkinEditorPrefab, m_SkinEditorRoot, "Skin Editor").Init(
				m_Skin.GetSkinFromDisk(skinName), skinName, openSettingAfterClose
			);
		}


		// Project Info - Delete
		public void UI_ProjectInfo_DeletePaletteItem (object palRT) {
			if (palRT is null || !(palRT is RectTransform)) { return; }
			int index = (palRT as RectTransform).GetSiblingIndex();
			m_Project.RemovePaletteAt(index);
			TryRefreshProjectInfo();
			UndoRedo.SetDirty();
		}


		public void UI_ProjectInfo_DeleteClickSound (object rtObj) {
			if (rtObj is null || !(rtObj is RectTransform)) { return; }
			int index = (rtObj as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(m_Language.Get(Confirm_DeleteProjectSound), index),
				DialogUtil.MarkType.Warning, null, null,
				() => {
					m_Project.RemoveClickSound(index);
					TryRefreshProjectInfo();
					UndoRedo.ClearUndo();
				}, null, () => { }
			);
		}


		public void UI_ProjectInfo_DeleteTween (object rtObj) {
			if (rtObj is null || !(rtObj is RectTransform)) { return; }
			int index = (rtObj as RectTransform).GetSiblingIndex();
			m_Project.RemoveTweenAt(index);
			TryRefreshProjectInfo();
			UndoRedo.SetDirty();
		}


		// Project Info - Swipe
		public void UI_ProjectInfo_SwipePaletteItem_Left (object itemRT) {
			if (itemRT is null || !(itemRT is RectTransform)) { return; }
			int index = (itemRT as RectTransform).GetSiblingIndex();
			m_Project.SwipePalette(index, index - 1);
			TryRefreshProjectInfo();
			UndoRedo.SetDirty();
		}


		public void UI_ProjectInfo_SwipePaletteItem_Right (object itemRT) {
			if (itemRT is null || !(itemRT is RectTransform)) { return; }
			int index = (itemRT as RectTransform).GetSiblingIndex();
			m_Project.SwipePalette(index, index + 1);
			TryRefreshProjectInfo();
			UndoRedo.SetDirty();
		}


		public void UI_ProjectInfo_SwipeClickSoundItem_Left (object itemRT) {
			if (itemRT is null || !(itemRT is RectTransform)) { return; }
			int index = (itemRT as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(m_Language.Get(Confirm_SwipeProjectSound), index),
				DialogUtil.MarkType.Warning, null, null,
				() => {
					m_Project.SwipeClickSound(index, index - 1);
					TryRefreshProjectInfo();
					UndoRedo.ClearUndo();
				}, null, () => { }
			);
		}


		public void UI_ProjectInfo_SwipeClickSoundItem_Right (object itemRT) {
			if (itemRT is null || !(itemRT is RectTransform)) { return; }
			int index = (itemRT as RectTransform).GetSiblingIndex();
			DialogUtil.Open(
				string.Format(m_Language.Get(Confirm_SwipeProjectSound), index),
				DialogUtil.MarkType.Warning, null, null,
				() => {
					m_Project.SwipeClickSound(index, index + 1);
					TryRefreshProjectInfo();
					UndoRedo.ClearUndo();
				}, null, () => { }
			);
		}


		public void UI_ProjectInfo_SwipeTweenItem_Left (object itemRT) {
			if (itemRT is null || !(itemRT is RectTransform)) { return; }
			int index = (itemRT as RectTransform).GetSiblingIndex();
			m_Project.SwipeTween(index, index - 1);
			TryRefreshProjectInfo();
			UndoRedo.SetDirty();
		}


		public void UI_ProjectInfo_SwipeTweenItem_Right (object itemRT) {
			if (itemRT is null || !(itemRT is RectTransform)) { return; }
			int index = (itemRT as RectTransform).GetSiblingIndex();
			m_Project.SwipeTween(index, index + 1);
			TryRefreshProjectInfo();
			UndoRedo.SetDirty();
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


		// Brush
		public void BrushSizeUp (bool alt) => SetBrushSize(
			m_Editor.SelectingBrushIndex, alt ? 1 : 0,
			Mathf.Clamp(GetBrushSize(m_Editor.SelectingBrushIndex, alt ? 1 : 0) + 0.05f, 0.05f, 1f)
		);


		public void BrushSizeDown (bool alt) => SetBrushSize(
			m_Editor.SelectingBrushIndex, alt ? 1 : 0,
			Mathf.Clamp(GetBrushSize(m_Editor.SelectingBrushIndex, alt ? 1 : 0) - 0.05f, 0.05f, 1f)
		);


		#endregion




		#region --- LGC ---



		// Setting
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


		private void SetUIScale (int uiScale) {
			uiScale = Mathf.Clamp(uiScale, 0, 2);
			var scalers = m_CanvasRoot.GetComponentsInChildren<CanvasScaler>(true);
			float height = uiScale == 0 ? 1000 : uiScale == 1 ? 800 : 600;
			foreach (var scaler in scalers) {
				scaler.referenceResolution = new Vector2(scaler.referenceResolution.x, height);
			}
		}


		private void SetMusicVolume (float value) {
			m_Music.Volume = value;
			SliderItemMap[SliderType.MusicVolume].saving.Value = value * 12f;
		}


		// Brush
		private float GetBrushSize (int index, int brushType) {
			switch (index) {
				default:
					return 0f;
				case 0: // Stage Width
					return brushType == 0 ? m_Editor.StageBrushWidth : m_Editor.StageBrushHeight;
				case 1: // Track Width
					return m_Editor.TrackBrushWidth;
				case 2: // Note Width
					return m_Editor.NoteBrushWidth;
			}
		}


		private void SetBrushSize (int itemType, int brushType, float size01) {
			size01 = Mathf.Clamp01(size01);
			switch (itemType) {
				case 0: // Stage
					if (brushType == 0) {
						// Width
						m_Editor.StageBrushWidth = size01;
						InputItemMap[InputType.StageBrushWidth].saving.Value = size01.ToString();
					} else {
						// Height
						m_Editor.StageBrushHeight = size01;
						InputItemMap[InputType.StageBrushHeight].saving.Value = size01.ToString();
					}
					break;
				case 1: // Track Width
					m_Editor.TrackBrushWidth = size01;
					InputItemMap[InputType.TrackBrushWidth].saving.Value = size01.ToString();
					break;
				case 2: // Note Width
					m_Editor.NoteBrushWidth = size01;
					InputItemMap[InputType.NoteBrushWidth].saving.Value = size01.ToString();
					break;
			}
		}


		#endregion


	}
}