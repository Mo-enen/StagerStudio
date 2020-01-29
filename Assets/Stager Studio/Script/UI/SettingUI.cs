namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Stage;
	using Saving;
	


	public class SettingUI : MonoBehaviour {




		#region --- SUB ---



		public delegate void VoidHandler ();
		public delegate void VoidStringHandler (string value);
		public delegate string StringStringHandler (string value);
		public delegate string StringVoidHandler ();
		public delegate void VoidIntHandler (int value);
		public delegate int IntVoidHandler ();
		public delegate void VoidBoolHandler (bool value);
		public delegate bool BoolVoidHandler ();
		public delegate void VoidBoolIntHandler (bool value, int index);
		public delegate bool BoolIntHandler (int index);
		public delegate void VoidFloatHandler (float value);
		public delegate float FloatVoidHandler ();


		[System.Serializable]
		public struct ComppnentData {
			// General
			public InputField Framerate;
			public Toggle UIScale_0;
			public Toggle UIScale_1;
			public Toggle UIScale_2;
			public Slider MusicVolume;
			public Slider SfxVolume;
			public Slider BgBright;
			public Toggle ShowZone;
			public Toggle ShowWave;
			public Toggle ShowPreview;
			public Toggle ShowTip;
			public Toggle ShowWelcome;
			// Editor
			public Toggle SnapProgress;
			public Toggle PositiveScroll;

			// Language
			public Text LanguageHint;

		}


		#endregion




		#region --- VAR ---


		// Const
		private const string RESET_CONFIRM_KEY = "Setting.ResetConfirm";

		// Handler
		public StringStringHandler GetLanguage { get; set; } = null;
		public VoidHandler ResetAllSettings { get; set; } = null;
		public VoidIntHandler SetFramerate { get; set; } = null;
		public IntVoidHandler GetFramerate { get; set; } = null;
		public VoidIntHandler SetUIScale { get; set; } = null;
		public IntVoidHandler GetUIScale { get; set; } = null;
		public VoidFloatHandler SetMusicVolume { get; set; } = null;
		public FloatVoidHandler GetMusicVolume { get; set; } = null;
		public VoidFloatHandler SetSfxVolume { get; set; } = null;
		public FloatVoidHandler GetSfxVolume { get; set; } = null;
		public VoidFloatHandler SetBgBright { get; set; } = null;
		public FloatVoidHandler GetBgBright { get; set; } = null;
		public VoidBoolHandler SetShowZone { get; set; } = null;
		public BoolVoidHandler GetShowZone { get; set; } = null;
		public VoidBoolHandler SetShowWave { get; set; } = null;
		public BoolVoidHandler GetShowWave { get; set; } = null;
		public VoidBoolHandler SetShowPreview { get; set; } = null;
		public BoolVoidHandler GetShowPreview { get; set; } = null;
		public VoidBoolHandler SetShowTip { get; set; } = null;
		public BoolVoidHandler GetShowTip { get; set; } = null;
		public VoidBoolHandler SetShowWelcome { get; set; } = null;
		public BoolVoidHandler GetShowWelcome { get; set; } = null;
		public VoidBoolHandler SetSnapProgress { get; set; } = null;
		public BoolVoidHandler GetSnapProgress { get; set; } = null;
		public VoidBoolHandler SetPositiveScroll { get; set; } = null;
		public BoolVoidHandler GetPositiveScroll { get; set; } = null;

		// Short
		private StageLanguage Language => _Language ?? (_Language = FindObjectOfType<StageLanguage>());
		private StageMenu Menu => _Menu != null ? _Menu : (_Menu = FindObjectOfType<StageMenu>());
		private StageSkin Skin => _Skin ?? (_Skin = FindObjectOfType<StageSkin>());

		// Ser
		[SerializeField] private Grabber m_LanguageItemPrefab = null;
		[SerializeField] private Grabber m_SkinItemPrefab = null;
		[SerializeField] private RectTransform m_GeneralContent = null;
		[SerializeField] private RectTransform m_LanguageContent = null;
		[SerializeField] private RectTransform m_SkinContent = null;
		[SerializeField] private ComppnentData m_Component = default;
		[SerializeField] private Text[] m_LanguageLabels = null;

		// Data
		private StageLanguage _Language = null;
		private StageMenu _Menu = null;
		private StageSkin _Skin = null;
		private bool UIReady = true;


		#endregion




		#region --- API ---


		public void Init () {

			// Language
			RefreshLanguageTexts();

			// Content
			m_GeneralContent.gameObject.SetActive(true);
			m_LanguageContent.gameObject.SetActive(false);

			// Callback
			m_Component.Framerate.onEndEdit.AddListener((str) => {
				if (UIReady && int.TryParse(str, out int result)) {
					SetFramerate(result);
					RefreshLogic(false);
				}
			});
			m_Component.UIScale_0.onValueChanged.AddListener((isOn) => {
				if (UIReady && isOn) {
					SetUIScale(0);
				}
			});
			m_Component.UIScale_1.onValueChanged.AddListener((isOn) => {
				if (UIReady && isOn) {
					SetUIScale(1);
				}
			});
			m_Component.UIScale_2.onValueChanged.AddListener((isOn) => {
				if (UIReady && isOn) {
					SetUIScale(2);
				}
			});
			m_Component.MusicVolume.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetMusicVolume((value - m_Component.MusicVolume.minValue) / (m_Component.MusicVolume.maxValue - m_Component.MusicVolume.minValue));
					RefreshLogic(false);
				}
			});
			m_Component.SfxVolume.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetSfxVolume((value - m_Component.SfxVolume.minValue) / (m_Component.SfxVolume.maxValue - m_Component.SfxVolume.minValue));
					RefreshLogic(false);
				}
			});
			m_Component.BgBright.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetBgBright((value - m_Component.SfxVolume.minValue) / (m_Component.SfxVolume.maxValue - m_Component.SfxVolume.minValue));
					RefreshLogic(false);
				}
			});
			m_Component.ShowZone.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetShowZone(value);
					RefreshLogic(false);
				}
			});
			m_Component.ShowWave.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetShowWave(value);
					RefreshLogic(false);
				}
			});
			m_Component.ShowPreview.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetShowPreview(value);
					RefreshLogic(false);
				}
			});
			m_Component.ShowTip.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetShowTip(value);
					RefreshLogic(false);
				}
			});
			m_Component.ShowWelcome.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetShowWelcome(value);
					RefreshLogic(false);
				}
			});
			// Editor
			m_Component.SnapProgress.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetSnapProgress(value);
					RefreshLogic(false);
				}
			});
			m_Component.PositiveScroll.onValueChanged.AddListener((value) => {
				if (UIReady) {
					SetPositiveScroll(value);
					RefreshLogic(false);
				}
			});
			// Refresh
			RefreshLogic();
		}


		public void Refresh () => RefreshLogic();


		public void ResetSettings () => DialogUtil.Dialog_OK_Cancel(RESET_CONFIRM_KEY, DialogUtil.MarkType.Warning, () => {
			ResetAllSettings();
			RefreshLogic();
		});


		// UI
		public void Close () {
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}


		public void OpenLanguageFolder () {
			var path = Util.CombinePaths(Application.streamingAssetsPath, "Language");
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(path);
		}


		public void OpenSkinFolder () {
			var path = Util.CombinePaths(Application.streamingAssetsPath, "Skins");
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(path);
		}


		public void UI_NewSkin () => FindObjectOfType<StageSkin>().UI_NewSkin();


		#endregion




		#region --- LGC ---


		private void RefreshLogic (bool refreshDynamic = true) {

			// General
			UIReady = false;
			m_Component.Framerate.text = GetFramerate().ToString();
			int uiScale = GetUIScale();
			m_Component.UIScale_0.isOn = uiScale == 0;
			m_Component.UIScale_1.isOn = uiScale == 1;
			m_Component.UIScale_2.isOn = uiScale == 2;
			m_Component.MusicVolume.value = Mathf.Lerp(m_Component.MusicVolume.minValue, m_Component.MusicVolume.maxValue, Mathf.Clamp01(GetMusicVolume()));
			m_Component.SfxVolume.value = Mathf.Lerp(m_Component.SfxVolume.minValue, m_Component.SfxVolume.maxValue, Mathf.Clamp01(GetSfxVolume()));
			m_Component.BgBright.value = Mathf.Lerp(m_Component.BgBright.minValue, m_Component.BgBright.maxValue, Mathf.Clamp01(GetBgBright()));
			m_Component.ShowZone.isOn = GetShowZone();
			m_Component.ShowWave.isOn = GetShowWave();
			m_Component.ShowPreview.isOn = GetShowPreview();
			m_Component.ShowTip.isOn = GetShowTip();
			m_Component.ShowWelcome.isOn = GetShowWelcome();
			m_Component.SnapProgress.isOn = GetSnapProgress();
			m_Component.PositiveScroll.isOn = GetPositiveScroll();

			UIReady = true;

			if (refreshDynamic) {
				// Language
				ClearLanguageLogic();
				m_Component.LanguageHint.text = Language.GetDisplayName() + " - " + Language.Get("Author");
				foreach (var lan in Language.AllLanguages) {
					var graber = Instantiate(m_LanguageItemPrefab, m_LanguageContent);
					var rt = graber.transform as RectTransform;
					rt.name = lan.ToString();
					rt.anchoredPosition3D = rt.anchoredPosition;
					rt.localRotation = Quaternion.identity;
					rt.localScale = Vector3.one;
					rt.SetAsLastSibling();
					graber.Grab<Button>().onClick.AddListener(OnClick);
					graber.Grab<Text>("Text").text = Language.GetDisplayName(lan);
					// Func
					void OnClick () {
						Language.LoadLanguage(lan);
						RefreshLanguageTexts();
					}
				}
				// Skin
				ClearSkinLogic();
				int uiLayerID = SortingLayer.NameToID("UI");
				foreach (var _name in Skin.AllSkinNames) {
					string skinName = _name;
					var graber = Instantiate(m_SkinItemPrefab, m_SkinContent);
					var rt = graber.transform as RectTransform;
					rt.name = skinName.ToString();
					rt.anchoredPosition3D = rt.anchoredPosition;
					rt.localRotation = Quaternion.identity;
					rt.localScale = Vector3.one;
					rt.SetAsLastSibling();
					graber.Grab<Button>().onClick.AddListener(OnClick);
					graber.Grab<Button>("Edit").onClick.AddListener(OnEdit);
					graber.Grab<Button>("Delete").onClick.AddListener(OnDelete);
					graber.Grab<Text>("Name").text = skinName;
					graber.Grab<RectTransform>("Mark").gameObject.SetActive(skinName == StageSkin.Data.Name);
					// Func
					void OnClick () => Skin.LoadSkin(skinName);
					void OnEdit () => StagerStudio.Main.SpawnSkinEditor(skinName);
					void OnDelete () => Skin.UI_DeleteSkin(rt);
				}
			}
		}


		private void RefreshLanguageTexts () {
			foreach (var tx in m_LanguageLabels) {
				tx.text = GetLanguage(tx.name);
			}
		}


		private void ClearLanguageLogic () => m_LanguageContent.DestroyAllChildImmediately();


		private void ClearSkinLogic () => m_SkinContent.DestroyAllChildImmediately();


		#endregion




	}
}