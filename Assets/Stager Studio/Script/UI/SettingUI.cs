namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using UIGadget;


	public class SettingUI : MonoBehaviour {




		#region --- SUB ---


		// Com Data
		[System.Serializable]
		public struct ComponentData {



			[System.Serializable]
			public class InputData {
				public StagerStudio.InputType Type;
				public InputField Item;
				[System.NonSerialized] public System.Func<string> GetHandler;
				public void InitItem(UnityAction<string> setHandler) => Item.onEndEdit.AddListener(setHandler);
			}


			[System.Serializable]
			public class ToggleData {
				public StagerStudio.ToggleType Type;
				public Toggle Item;
				[System.NonSerialized] public System.Func<bool> GetHandler;
				public void InitItem(UnityAction<bool> setHandler) => Item.onValueChanged.AddListener(setHandler);
			}


			[System.Serializable]
			public class SliderData {
				public StagerStudio.SliderType Type;
				public Slider Item;
				[System.NonSerialized] public System.Func<float> GetHandler;
				public void InitItem(UnityAction<float> setHandler) => Item.onValueChanged.AddListener(setHandler);
			}


			// Items
			public InputData[] Inputs;
			public ToggleData[] Toggles;
			public SliderData[] Sliders;


			public void RefreshAllUI() {
				foreach (var i in Inputs) {
					i.Item.text = i.GetHandler();
				}
				foreach (var t in Toggles) {
					t.Item.isOn = t.GetHandler();
				}
				foreach (var s in Sliders) {
					s.Item.value = s.GetHandler();
				}
			}


			public void ForAllInputs(System.Action<InputData> action) {
				foreach (var i in Inputs) {
					action(i);
				}
			}
			public void ForAllToggles(System.Action<ToggleData> action) {
				foreach (var t in Toggles) {
					action(t);
				}
			}
			public void ForAllSliders(System.Action<SliderData> action) {
				foreach (var s in Sliders) {
					action(s);
				}
			}

		}


		// Handler
		public delegate void VoidHandler();
		public delegate string StringStringHandler(string value);
		public delegate string StringHandler();
		public delegate void VoidStringHandler(string str);
		public delegate string StringLanguageHandler(SystemLanguage language);
		public delegate bool BoolLanguageHandler(SystemLanguage language);
		public delegate List<SystemLanguage> LanguagesHandler();
		public delegate string[] StringsHandler();
		public delegate void VoidRtHandler(RectTransform rt);
		public delegate int IntHandler();
		public delegate (string name, KeyCode key, bool ctrl, bool shift, bool alt) ShortcutItemIntHandler(int index);
		public delegate int ShortcutHandler(int index, KeyCode key, bool ctrl, bool shift, bool alt);
		public delegate void StringBoolHandler(string str, bool b);


		#endregion




		#region --- VAR ---


		// Const
		private const string RESET_CONFIRM_KEY = "Setting.ResetConfirm";

		// Handler
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static VoidHandler ResetAllSettings { get; set; } = null;
		public static VoidHandler SkinRefreshAllSkinNames { get; set; } = null;
		public static StringHandler LanguageGetDisplayName { get; set; } = null;
		public static StringLanguageHandler LanguageGetDisplayName_Language { get; set; } = null;
		public static BoolLanguageHandler LoadLanguage { get; set; } = null;
		public static LanguagesHandler GetAllLanguages { get; set; } = null;
		public static StringsHandler GetAllSkinNames { get; set; } = null;
		public static StringHandler GetSkinName { get; set; } = null;
		public static VoidStringHandler SkinLoadSkin { get; set; } = null;
		public static VoidRtHandler SkinDeleteSkin { get; set; } = null;
		public static VoidHandler SkinNewSkin { get; set; } = null;
		public static VoidStringHandler OpenMenu { get; set; } = null;
		public static IntHandler ShortcutCount { get; set; } = null;
		public static ShortcutItemIntHandler GetShortcutAt { get; set; } = null;
		public static VoidHandler SaveShortcut { get; set; } = null;
		public static ShortcutHandler SetShortcut { get; set; } = null;
		public static StringBoolHandler SpawnSkinEditor { get; set; } = null;

		// Api
		public bool UIReady { get; private set; } = true;
		public ComponentData Component => m_Component;

		// Ser
		[SerializeField] private Grabber m_LanguageItemPrefab = null;
		[SerializeField] private Grabber m_SkinItemPrefab = null;
		[SerializeField] private Grabber m_ShortcutItemPrefab = null;
		[SerializeField] private RectTransform m_Window = null;
		[SerializeField] private RectTransform m_GeneralContent = null;
		[SerializeField] private RectTransform m_EditorContent = null;
		[SerializeField] private RectTransform m_SkinContent = null;
		[SerializeField] private RectTransform m_LanguageContent = null;
		[SerializeField] private RectTransform m_ShortcutContent = null;
		[SerializeField] private Text m_LanguageHint = null;
		[SerializeField] private ComponentData m_Component = default;
		[SerializeField] private Text[] m_LanguageLabels = null;


		#endregion




		#region --- MSG ---


		private void Awake() => m_Window.anchoredPosition3D = new Vector2(m_Window.anchoredPosition3D.x, -46f);


		private void Update() => m_Window.LerpUI(Vector2.zero, 8f);


		#endregion




		#region --- API ---


		public void Init() {

			// Language
			RefreshLanguageTexts();

			// Content
			m_GeneralContent.parent.parent.gameObject.SetActive(true);
			m_EditorContent.parent.parent.gameObject.SetActive(false);
			m_SkinContent.parent.parent.gameObject.SetActive(false);
			m_LanguageContent.parent.parent.gameObject.SetActive(false);
			m_ShortcutContent.parent.parent.gameObject.SetActive(false);

			// Refresh
			RefreshLogic(true);
		}


		public void Refresh() {
			SkinRefreshAllSkinNames();
			RefreshLogic(true);
		}


		public void RefreshLogic(bool refreshDynamic) {

			// General
			UIReady = false;
			try {
				m_Component.RefreshAllUI();
			} catch { }
			UIReady = true;

			// Dynamic
			UIReady = false;
			try {
				if (refreshDynamic) {
					// Language
					m_LanguageContent.DestroyAllChildImmediately();
					m_LanguageHint.text = LanguageGetDisplayName() + " - " + GetLanguage("Author");
					var allLanguages = GetAllLanguages();
					foreach (var lan in allLanguages) {
						var graber = Instantiate(m_LanguageItemPrefab, m_LanguageContent);
						var rt = graber.transform as RectTransform;
						rt.name = lan.ToString();
						rt.anchoredPosition3D = rt.anchoredPosition;
						rt.localRotation = Quaternion.identity;
						rt.localScale = Vector3.one;
						rt.SetAsLastSibling();
						graber.Grab<Button>().onClick.AddListener(OnClick);
						graber.Grab<Text>("Text").text = LanguageGetDisplayName_Language(lan);
						// Func
						void OnClick() {
							LoadLanguage(lan);
							RefreshLanguageTexts();
						}
					}
					// Skin
					m_SkinContent.DestroyAllChildImmediately();
					int uiLayerID = SortingLayer.NameToID("UI");
					var allSkinNames = GetAllSkinNames();
					foreach (var _name in allSkinNames) {
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
						graber.Grab<RectTransform>("Mark").gameObject.SetActive(skinName == GetSkinName());
						// Func
						void OnClick() => SkinLoadSkin(skinName);
						void OnEdit() => SpawnSkinEditor(skinName, true);
						void OnDelete() => SkinDeleteSkin(rt);
					}
					// Shortcut
					m_ShortcutContent.DestroyAllChildImmediately();
					int shortcutCount = ShortcutCount();
					for (int i = 0; i < shortcutCount; i++) {
						int index = i;
						var data = GetShortcutAt(index);
						var graber = Instantiate(m_ShortcutItemPrefab, m_ShortcutContent);
						var rt = graber.transform as RectTransform;
						rt.name = data.name;
						rt.anchoredPosition3D = rt.anchoredPosition;
						rt.localRotation = Quaternion.identity;
						rt.localScale = Vector3.one;
						rt.SetAsLastSibling();
						graber.Grab<Text>("Name").text = GetLanguage(data.name);
						if (index % 2 == 1) {
							graber.Grab<Image>().color = Color.clear;
						}
						var ctrlTG = graber.Grab<Toggle>("Ctrl");
						var shiftTG = graber.Grab<Toggle>("Shift");
						var altTG = graber.Grab<Toggle>("Alt");
						var keySetter = graber.Grab<KeySetterUI>("Key");
						var clearBtn = graber.Grab<Button>("Clear");
						ctrlTG.isOn = data.ctrl;
						shiftTG.isOn = data.shift;
						altTG.isOn = data.alt;
						ctrlTG.onValueChanged.AddListener((isOn) => {
							if (isOn != data.ctrl) {
								data.ctrl = isOn;
								SetShortcut(index, data.key, data.ctrl, data.shift, data.alt);
								SaveShortcut();
							}
						});
						shiftTG.onValueChanged.AddListener((isOn) => {
							if (isOn != data.shift) {
								data.shift = isOn;
								SetShortcut(index, data.key, data.ctrl, data.shift, data.alt);
								SaveShortcut();
							}
						});
						altTG.onValueChanged.AddListener((isOn) => {
							if (isOn != data.alt) {
								data.alt = isOn;
								SetShortcut(index, data.key, data.ctrl, data.shift, data.alt);
								SaveShortcut();
							}
						});
						keySetter.Label = Util.GetKeyName(data.key);
						keySetter.OnSetStart = () => {
							var setters = m_ShortcutContent.GetComponentsInChildren<KeySetterUI>(false);
							foreach (var setter in setters) {
								if (setter != keySetter) {
									setter.CancelSet();
								}
							}
						};
						keySetter.OnSetDone = (key) => {
							// Set Key
							if (data.key != key) {
								data.key = key;
								SetShortcut(index, data.key, data.ctrl, data.shift, data.alt);
								SaveShortcut();
								keySetter.Label = Util.GetKeyName(key);
							}
							RefreshLogic(true);
						};
						clearBtn.onClick.AddListener(() => {
							if (data.key != KeyCode.None) {
								data.key = KeyCode.None;
								SetShortcut(index, data.key, data.ctrl, data.shift, data.alt);
								SaveShortcut();
								keySetter.Label = "";
							}
						});
					}
				}
			} catch { }
			UIReady = true;
		}


		public void ResetSettings() => DialogUtil.Dialog_OK_Cancel(RESET_CONFIRM_KEY, DialogUtil.MarkType.Warning, () => {
			ResetAllSettings();
			RefreshLogic(true);
		});


		// UI
		public void Close() {
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}


		public void OpenLanguageFolder() {
			var path = Util.CombinePaths(Util.GetParentPath(Application.dataPath), "Language");
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(path);
		}


		public void OpenSkinFolder() {
			var path = Util.CombinePaths(Util.GetParentPath(Application.dataPath), "Skins");
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(path);
		}


		public void OpenShortcutFolder() {
			var path = Util.CombinePaths(Util.GetParentPath(Application.dataPath), "Shortcut");
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(path);
		}


		public void UI_NewSkin() => SkinNewSkin();


		public void UI_OpenMenu(string key) => OpenMenu(key);


		#endregion




		#region --- LGC ---


		private void RefreshLanguageTexts() {
			foreach (var tx in m_LanguageLabels) {
				tx.text = GetLanguage(tx.name);
			}
		}


		#endregion




	}
}