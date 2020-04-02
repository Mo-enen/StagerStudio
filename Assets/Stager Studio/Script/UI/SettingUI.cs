namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using Stage;


	public class SettingUI : MonoBehaviour {




		#region --- SUB ---



		// handler
		public delegate void VoidHandler ();
		public delegate string StringStringHandler (string value);


		// Com Data
		[System.Serializable]
		public struct ComponentData {



			[System.Serializable]
			public class InputData {
				public StagerStudio.InputType Type;
				public InputField Item;
				[System.NonSerialized] public System.Func<string> GetHandler;
				public void InitItem (UnityAction<string> setHandler) => Item.onEndEdit.AddListener(setHandler);
			}


			[System.Serializable]
			public class ToggleData {
				public StagerStudio.ToggleType Type;
				public Toggle Item;
				[System.NonSerialized] public System.Func<bool> GetHandler;
				public void InitItem (UnityAction<bool> setHandler) => Item.onValueChanged.AddListener(setHandler);
			}


			[System.Serializable]
			public class SliderData {
				public StagerStudio.SliderType Type;
				public Slider Item;
				[System.NonSerialized] public System.Func<float> GetHandler;
				public void InitItem (UnityAction<float> setHandler) => Item.onValueChanged.AddListener(setHandler);
			}


			// Items
			public InputData[] Inputs;
			public ToggleData[] Toggles;
			public SliderData[] Sliders;


			public void RefreshAllUI () {
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


			public void ForAllInputs (System.Action<InputData> action) {
				foreach (var i in Inputs) {
					action(i);
				}
			}
			public void ForAllToggles (System.Action<ToggleData> action) {
				foreach (var t in Toggles) {
					action(t);
				}
			}
			public void ForAllSliders (System.Action<SliderData> action) {
				foreach (var s in Sliders) {
					action(s);
				}
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private const string RESET_CONFIRM_KEY = "Setting.ResetConfirm";

		// Api
		public StringStringHandler GetLanguage { get; set; } = null;
		public VoidHandler ResetAllSettings { get; set; } = null;
		public bool UIReady { get; private set; } = true;
		public ComponentData Component => m_Component;

		// Short
		private StageLanguage Language => _Language != null ? _Language : (_Language = FindObjectOfType<StageLanguage>());
		private StageSkin Skin => _Skin != null ? _Skin : (_Skin = FindObjectOfType<StageSkin>());
		private StageMenu Menu => _Menu != null ? _Menu : (_Menu = FindObjectOfType<StageMenu>());
		private StageShortcut Shortcut => _Shortcut != null ? _Shortcut : (_Shortcut = FindObjectOfType<StageShortcut>());

		// Ser
		[SerializeField] private Grabber m_LanguageItemPrefab = null;
		[SerializeField] private Grabber m_SkinItemPrefab = null;
		[SerializeField] private Grabber m_ShortcutItemPrefab = null;
		[SerializeField] private RectTransform m_GeneralContent = null;
		[SerializeField] private RectTransform m_EditorContent = null;
		[SerializeField] private RectTransform m_SkinContent = null;
		[SerializeField] private RectTransform m_LanguageContent = null;
		[SerializeField] private RectTransform m_ShortcutContent = null;
		[SerializeField] private Text m_LanguageHint = null;
		[SerializeField] private ComponentData m_Component = default;
		[SerializeField] private Text[] m_LanguageLabels = null;

		// Data
		private StageLanguage _Language = null;
		private StageSkin _Skin = null;
		private StageMenu _Menu = null;
		private StageShortcut _Shortcut = null;


		#endregion




		#region --- API ---


		public void Init () {

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


		public void Refresh () {
			Skin.RefreshAllSkinNames();
			RefreshLogic(true);
		}


		public void RefreshLogic (bool refreshDynamic) {

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
					m_LanguageHint.text = Language.GetDisplayName() + " - " + Language.Get("Author");
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
					m_SkinContent.DestroyAllChildImmediately();
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
						void OnEdit () => StagerStudio.Main.SpawnSkinEditor(skinName, true);
						void OnDelete () => Skin.UI_DeleteSkin(rt);
					}
					// Shortcut
					m_ShortcutContent.DestroyAllChildImmediately();
					for (int i = 0; i < Shortcut.Datas.Length; i++) {
						var data = Shortcut.Datas[i];
						var graber = Instantiate(m_ShortcutItemPrefab, m_ShortcutContent);
						var rt = graber.transform as RectTransform;
						rt.name = data.Name;
						rt.anchoredPosition3D = rt.anchoredPosition;
						rt.localRotation = Quaternion.identity;
						rt.localScale = Vector3.one;
						rt.SetAsLastSibling();
						graber.Grab<Text>("Name").text = GetLanguage(data.Name);
						if (i % 2 == 1) {
							graber.Grab<Image>().color = Color.clear;
						}
						var ctrlTG = graber.Grab<Toggle>("Ctrl");
						var shiftTG = graber.Grab<Toggle>("Shift");
						var altTG = graber.Grab<Toggle>("Alt");
						var keySetter = graber.Grab<KeySetterUI>("Key");
						var clearBtn = graber.Grab<Button>("Clear");
						ctrlTG.isOn = data.Ctrl;
						shiftTG.isOn = data.Shift;
						altTG.isOn = data.Alt;
						ctrlTG.onValueChanged.AddListener((isOn) => {
							if (isOn != data.Ctrl) {
								data.Ctrl = isOn;
								Shortcut.SaveToFile();
								Shortcut.ReloadMap();
							}
						});
						shiftTG.onValueChanged.AddListener((isOn) => {
							if (isOn != data.Shift) {
								data.Shift = isOn;
								Shortcut.SaveToFile();
								Shortcut.ReloadMap();
							}
						});
						altTG.onValueChanged.AddListener((isOn) => {
							if (isOn != data.Alt) {
								data.Alt = isOn;
								Shortcut.SaveToFile();
								Shortcut.ReloadMap();
							}
						});
						keySetter.Label = Util.GetKeyName(data.Key);
						keySetter.OnSetStart = () => {
							var setters = m_ShortcutContent.GetComponentsInChildren<KeySetterUI>(false);
							foreach (var setter in setters) {
								if (setter != keySetter) {
									setter.CancelSet();
								}
							}
						};
						keySetter.OnSetDone = (key) => {
							// Check Key
							bool needRefreshUI = false;
							foreach (var d in Shortcut.Datas) {
								if (
									d != data &&
									d.Key == key &&
									d.Ctrl == data.Ctrl &&
									d.Shift == data.Shift &&
									d.Alt == data.Alt
								) {
									d.Key = KeyCode.None;
									needRefreshUI = true;
								}
							}
							// Set Key
							if (data.Key != key) {
								data.Key = key;
								keySetter.Label = Util.GetKeyName(key);
								Shortcut.SaveToFile();
								Shortcut.ReloadMap();
							}
							if (needRefreshUI) {
								RefreshLogic(true);
							}
						};
						clearBtn.onClick.AddListener(() => {
							if (data.Key != KeyCode.None) {
								data.Key = KeyCode.None;
								keySetter.Label = "";
								Shortcut.SaveToFile();
								Shortcut.ReloadMap();
							}
						});
					}
				}
			} catch { }
			UIReady = true;
		}


		public void ResetSettings () => DialogUtil.Dialog_OK_Cancel(RESET_CONFIRM_KEY, DialogUtil.MarkType.Warning, () => {
			ResetAllSettings();
			RefreshLogic(true);
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


		public void OpenShortcutFolder () {
			var path = Util.CombinePaths(Application.streamingAssetsPath, "Shortcut");
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(path);
		}


		public void UI_NewSkin () => FindObjectOfType<StageSkin>().UI_NewSkin();


		public void UI_OpenMenu (string key) => Menu.OpenMenu(key);


		#endregion




		#region --- LGC ---


		private void RefreshLanguageTexts () {
			foreach (var tx in m_LanguageLabels) {
				tx.text = GetLanguage(tx.name);
			}
		}


		#endregion




	}
}