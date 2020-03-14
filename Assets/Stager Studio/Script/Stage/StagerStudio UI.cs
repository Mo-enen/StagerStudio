namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Stage;
	using UI;
	using UnityEngine.UI;
	using Saving;


	public partial class StagerStudio { /// --- Spawn UI ---


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

		// Saving
		private SavingInt FrameRate = new SavingInt("SS.FrameRate", 120);
		private SavingInt UIScale = new SavingInt("SS.UIScale", 1);
		private SavingFloat MusicVolume = new SavingFloat("SS.MusicVolume", 0.5f);
		private SavingFloat SfxVolume = new SavingFloat("SS.SfxVolume", 0.5f);
		private SavingFloat BgBrightness = new SavingFloat("SS.BgBrightness", 0.309f);
		private SavingBool ShowZone = new SavingBool("SS.ShowZone", true);
		private SavingBool ShowWave = new SavingBool("SS.ShowWave", true);
		private SavingBool ShowPreview = new SavingBool("SS.ShowPreview", true);
		private SavingBool ShowTip = new SavingBool("SS.ShowTip", true);
		private SavingBool ShowWelcome = new SavingBool("SS.ShowWelcome", true);
		private SavingBool SnapProgress = new SavingBool("SS.SnapProgress", true);
		private SavingBool PositiveScroll = new SavingBool("StageGame.PositiveScroll", true);
		private SavingBool ShowIndexLabel = new SavingBool("StageGame.ShowIndexLabel", false);


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
			setting.GetFramerate = () => Application.targetFrameRate;
			setting.SetFramerate = SetFramerate;
			setting.GetUIScale = () => UIScale;
			setting.SetUIScale = SetUIScale;
			setting.GetMusicVolume = () => Music.Volume;
			setting.SetMusicVolume = SetMusicVolume;
			setting.GetSfxVolume = () => Music.SfxVolume;
			setting.SetSfxVolume = SetSfxVolume;
			setting.GetBgBright = () => m_Background.Brightness;
			setting.SetBgBright = SetBgBrightness;
			setting.GetShowZone = () => m_Zone.IsShowing;
			setting.SetShowZone = SetShowZone;
			setting.GetShowWave = () => ShowWave;
			setting.SetShowWave = SetShowWave;
			setting.GetShowPreview = () => m_Preview.gameObject.activeSelf;
			setting.SetShowPreview = SetShowPreview;
			setting.GetShowTip = () => m_TipLabel.gameObject.activeSelf;
			setting.SetShowTip = SetShowTip;
			setting.GetShowWelcome = () => ShowWelcome;
			setting.SetShowWelcome = SetShowWelcome;
			setting.GetSnapProgress = () => SnapProgress;
			setting.SetSnapProgress = SetSnapProgress;
			setting.GetPositiveScroll = () => Game.PositiveScroll;
			setting.SetPositiveScroll = SetPositiveScroll;
			setting.GetShowIndexLabel = () => Object.StageObject.ShowIndexLabel;
			setting.SetShowIndexLabel = SetShowIndexLabel;

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
			SetFramerate(FrameRate);
			SetUIScale(UIScale);
			SetMusicVolume(MusicVolume);
			SetSfxVolume(SfxVolume);
			SetBgBrightness(BgBrightness);
			SetShowZone(ShowZone);
			SetShowWave(ShowWave);
			SetShowPreview(ShowPreview);
			SetShowTip(ShowTip);
			SetShowWelcome(ShowWelcome);
			SetSnapProgress(SnapProgress);
			SetPositiveScroll(PositiveScroll);
			SetShowIndexLabel(ShowIndexLabel);
		}
		private void ResetAllSettings () {
			FrameRate.Reset();
			UIScale.Reset();
			MusicVolume.Reset();
			SfxVolume.Reset();
			BgBrightness.Reset();
			ShowZone.Reset();
			ShowWave.Reset();
			ShowPreview.Reset();
			ShowTip.Reset();
			SnapProgress.Reset();
			PositiveScroll.Reset();
			ShowIndexLabel.Reset();
		}
		private void SetFramerate (int fRate) {
			fRate = Mathf.Clamp(fRate, 12, 1200);
			FrameRate.Value = fRate;
			Application.targetFrameRate = fRate;
		}
		private void SetUIScale (int uiScale) {
			uiScale = Mathf.Clamp(uiScale, 0, 2);
			UIScale.Value = uiScale;
			var scalers = m_CanvasRoot.GetComponentsInChildren<CanvasScaler>(true);
			float height = uiScale == 0 ? 1000 : uiScale == 1 ? 800 : 600;
			foreach (var scaler in scalers) {
				scaler.referenceResolution = new Vector2(scaler.referenceResolution.x, height);
			}
		}
		private void SetMusicVolume (float volume) {
			volume = Mathf.Clamp01(volume);
			MusicVolume.Value = volume;
			Music.Volume = volume;
		}
		private void SetSfxVolume (float volume) {
			volume = Mathf.Clamp01(volume);
			SfxVolume.Value = volume;
			Music.SfxVolume = volume;
		}
		private void SetBgBrightness (float value) {
			value = Mathf.Clamp01(value);
			BgBrightness.Value = value;
			m_Background.SetBrightness(value);
		}
		private void SetShowZone (bool show) {
			ShowZone.Value = show;
			m_Zone.ShowZone(show);
		}
		private void SetShowWave (bool show) {
			ShowWave.Value = show;
			m_Wave.enabled = show;
		}
		private void SetShowPreview (bool show) {
			ShowPreview.Value = show;
			m_Preview.Show(show);
		}
		private void SetShowTip (bool show) {
			ShowTip.Value = show;
			m_TipLabel.gameObject.SetActive(show);
		}
		private void SetShowWelcome (bool show) {
			ShowWelcome.Value = show;
		}
		private void SetSnapProgress (bool snap) {
			SnapProgress.Value = snap;
			m_Progress.Snap = snap;
		}
		private void SetPositiveScroll (bool positive) {
			PositiveScroll.Value = positive;
			Game.PositiveScroll = positive;
		}
		private void SetShowIndexLabel (bool show) {
			ShowIndexLabel.Value = show;
			Object.StageObject.ShowIndexLabel = show;
		}

	}
}