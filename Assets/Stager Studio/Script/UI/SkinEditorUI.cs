namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class SkinEditorUI : MonoBehaviour {




		#region --- VAR ---


		// Handler
		public delegate string StringStringHandler (string key);
		public delegate void VoidFloatHandler (float value);
		public delegate void VoidHandler ();
		public delegate void VoidStringHandler (string str);
		public delegate string PathSkinStringHandler (SkinData skin, string name);

		// Const
		private const string HINT_Saved = "SkinEditor.Saved";
		private const string PAINTER_MENU_KEY = "Menu.SkinEditor.Painter";
		private const string DIALOG_CloseConfirm = "SkinEditor.Dialog.CloseConfirm";
		private const string DIALOG_ImportImageTitle = "SkinEditor.Dialog.ImportImageTitle";
		private const string DIALOG_ExportImageTitle = "SkinEditor.Dialog.ExportImageTitle";
		private const string DIALOG_SkinImageExported = "SkinEditor.Dialog.SkinImageExported";
		private const int TEXTURE_MAX_SIZE = 1024;

		// API
		public SkinData Data { get; private set; } = new SkinData();
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static VoidFloatHandler MusicSeek_Add { get; set; } = null;
		public static VoidHandler SkinReloadSkin { get; set; } = null;
		public static VoidStringHandler OpenMenu { get; set; } = null;
		public static StringStringHandler SkinGetPath { get; set; } = null;
		public static PathSkinStringHandler SkinSaveSkin { get; set; } = null;
		public static VoidHandler SpawnSetting { get; set; } = null;
		public static VoidHandler RemoveUI { get; set; } = null;

		// Short
		private SkinEditorPainterUI Painter => m_Painter;
		private SkinType EditingType { get; set; } = SkinType.Stage;
		private string SkinName { get => m_SkinNameIF.text; set => m_SkinNameIF.text = value; }
		private string SkinAuthor { get => m_SkinAuthorIF.text; set => m_SkinAuthorIF.text = value; }

		// Ser
		[SerializeField] private SkinEditorPainterUI m_Painter = null;
		[SerializeField] private InputField m_SkinNameIF = null;
		[SerializeField] private InputField m_SkinAuthorIF = null;
		[SerializeField] private InputField m_ScaleMutiIF = null;
		[SerializeField] private InputField m_LuminWidthAppendIF = null;
		[SerializeField] private InputField m_LuminHeightAppendIF = null;
		[SerializeField] private InputField m_VanishDurationIF = null;
		[SerializeField] private InputField m_DurationIF = null;
		[SerializeField] private InputField m_MinSizeIF = null;
		[SerializeField] private Toggle m_TintNoteTG = null;
		[SerializeField] private Toggle m_FrontPoleTG = null;
		[SerializeField] private Button m_HighlightTint = null;
		[SerializeField] private Toggle m_FixedRatioTG = null;
		[SerializeField] private Image m_Background = null;
		[SerializeField] private RectTransform m_Window = null;
		[SerializeField] private RectTransform m_TypeTgContainer = null;
		[SerializeField] private RectTransform m_SecondLine = null;
		[SerializeField] private Text[] m_LanguageTexts = null;

		// Data
		private StagerStudio SS = null;
		private HintBarUI Hint = null;
		private bool UIReady = true;
		private bool OpenSettingAfterClose = false;
		private string SavedName = "";
		private int PrevPainterSelectIndex = -1;


		#endregion




		#region --- MSG ---


		private void Awake () => m_Window.anchoredPosition3D = new Vector2(m_Window.anchoredPosition3D.x, -46f);


		private void Update () {
			if (transform.localScale.x < 0.5f) {
				if (Input.GetMouseButton(0)) {
					// Viewing
					MusicSeek_Add(Input.GetAxis("Mouse X") * 0.1f);
				} else {
					// View Cancel
					transform.localScale = Vector3.one;
				}
			}
			m_Window.LerpUI(Vector2.zero, 8f);
			if (PrevPainterSelectIndex != m_Painter.SelectingRectIndex) {
				PrevPainterSelectIndex = m_Painter.SelectingRectIndex;
				RefreshInfoUI();
			}
		}


		#endregion




		#region --- API ---


		public void Init (SkinData skinData, string skinName, bool openSettingAfterClose) {

			if (skinData == null || string.IsNullOrEmpty(skinName)) {
				SkinReloadSkin();
				SpawnSetting();
				return;
			}

			// Stuffs...
			Hint = FindObjectOfType<HintBarUI>();
			SS = FindObjectOfType<StagerStudio>();

			// Language
			foreach (var text in m_LanguageTexts) {
				text.text = GetLanguage(text.name);
			}

			// Skin
			Data = skinData;
			SkinAuthor = skinData.Author;
			SkinName = skinName;
			SavedName = skinName;

			// Duration
			m_DurationIF.onEndEdit.AddListener((str) => {
				if (!UIReady) { return; }
				var ani = GetEditingAniData();
				if (ani is null) { return; }
				if (int.TryParse(str, out int durationMS)) {
					ani.SetDuration(durationMS);
					m_DurationIF.text = ani.FrameDuration.ToString();
				}
			});

			// MinSize
			m_MinSizeIF.onEndEdit.AddListener((str) => {
				if (!UIReady) { return; }
				var ani = GetEditingAniData();
				if (ani is null) { return; }
				if (int.TryParse(str, out int value)) {
					var i = m_Painter.SelectingRectIndex;
					if (ani.Rects != null && i >= 0 && i < ani.Rects.Count) {
						var rect = ani.Rects[i];
						rect.MinHeight = Mathf.Max(value, 0);
						ani.Rects[i] = rect;
						m_MinSizeIF.text = rect.MinHeight.ToString();
					}
				}
			});

			// Highlight
			m_HighlightTint.onClick.AddListener(() => {
				if (!UIReady) { return; }
				var ani = GetEditingAniData();
				if (ani is null) { return; }
				SS.SpawnColorPicker(ani.HighlightTint, (newHighlight) => {
					ani.HighlightTint = newHighlight;
					m_HighlightTint.GetComponent<Image>().color = ani.HighlightTint;
				});
			});


			// Scale Muti
			m_ScaleMutiIF.onEndEdit.AddListener((str) => {
				if (!UIReady) { return; }
				if (float.TryParse(str, out float scaleMT)) {
					Data.ScaleMuti_UI = scaleMT;
					m_ScaleMutiIF.text = Data.ScaleMuti_UI.ToString();
				}
			});

			// Luminous Append
			m_LuminWidthAppendIF.onEndEdit.AddListener((str) => {
				if (!UIReady) { return; }
				if (float.TryParse(str, out float lwa)) {
					Data.LuminousAppendX_UI = Mathf.Clamp(lwa, 0f, 100f);
					m_LuminWidthAppendIF.text = Data.LuminousAppendX_UI.ToString();
				}
			});
			m_LuminHeightAppendIF.onEndEdit.AddListener((str) => {
				if (!UIReady) { return; }
				if (float.TryParse(str, out float lwa)) {
					Data.LuminousAppendY_UI = Mathf.Clamp(lwa, 0f, 100f);
					m_LuminHeightAppendIF.text = Data.LuminousAppendY_UI.ToString();
				}
			});

			// Vanish Duration 
			m_VanishDurationIF.onEndEdit.AddListener((str) => {
				if (!UIReady) { return; }
				if (int.TryParse(str, out int value)) {
					Data.VanishDuration_UI = value;
					m_VanishDurationIF.text = Data.VanishDuration_UI.ToString();
				}
			});

			// Fixed Ratio
			m_FixedRatioTG.onValueChanged.AddListener((isOn) => {
				if (!UIReady) { return; }
				var ani = GetEditingAniData();
				if (ani is null) { return; }
				ani.FixedRatio = isOn;
			});

			// Tint Note
			m_TintNoteTG.onValueChanged.AddListener((isOn) => {
				if (!UIReady) { return; }
				Data.TintNote = isOn;
			});

			// Front Pole
			m_FrontPoleTG.onValueChanged.AddListener((isOn) => {
				if (!UIReady) { return; }
				Data.FrontPole = isOn;
			});

			// Type TGs
			int len = m_TypeTgContainer.childCount;
			for (int i = 0; i < len; i++) {
				var tg = m_TypeTgContainer.GetChild(i).GetComponent<Toggle>();
				int index = i;
				tg.onValueChanged.AddListener((isOn) => {
					if (!UIReady || !isOn) { return; }
					EditingType = (SkinType)index;
					Painter.SetItemsDirty();
					Painter.ResetRootPositionSize();
					Painter.SetSelection(-1);
					RefreshInfoUI();
					TrimAllRects();
				});
			}

			// UI
			OpenSettingAfterClose = openSettingAfterClose;
			RefreshInfoUI();
			Painter.SetItemsDirty();
			Painter.SetTexture(Data.Texture);
			TrimAllRects();
			Painter.SetSelection(-1);
		}


		public void Save () {
			if (Data is null) { return; }
			Data.Author = SkinAuthor;
			var newPath = SkinSaveSkin(Data, SkinName);
			SkinReloadSkin();
			Hint?.SetHint(GetLanguage(HINT_Saved));
			Resources.UnloadUnusedAssets();
			// Delete Old when Rename
			if (SkinName != SavedName) {
				if (Util.FileExists(newPath)) {
					Util.DeleteFile(SkinGetPath(SavedName));
				}
				SavedName = SkinName;
			}
		}


		public void Close () {
			DialogUtil.Dialog_OK_Cancel(DIALOG_CloseConfirm, DialogUtil.MarkType.Warning, () => {
				Save();
				if (OpenSettingAfterClose) {
					SpawnSetting();
				} else {
					RemoveUI();
				}
			});
		}


		public void RefreshInfoUI () {
			var data = Data;
			var ani = GetEditingAniData();
			if (data is null || ani is null) { return; }
			UIReady = false;
			try {
				// Active
				bool fixedRatioActive = EditingType == SkinType.Note ||
					EditingType == SkinType.NoteLuminous ||
					EditingType == SkinType.HoldLuminous;
				bool highlightActive = EditingType == SkinType.Pole ||
					EditingType == SkinType.Note;
				bool durationActive = EditingType == SkinType.NoteLuminous ||
					EditingType == SkinType.HoldLuminous;
				bool minSizeActie = EditingType == SkinType.Note && m_Painter.SelectingRectIndex >= 0;
				m_FixedRatioTG.transform.parent.gameObject.SetActive(fixedRatioActive);
				m_HighlightTint.transform.parent.gameObject.SetActive(highlightActive);
				m_DurationIF.transform.parent.gameObject.SetActive(durationActive);
				m_MinSizeIF.transform.parent.gameObject.SetActive(minSizeActie);
				m_SecondLine.gameObject.SetActive(fixedRatioActive || highlightActive || durationActive);
				// Data
				m_DurationIF.text = ani.FrameDuration.ToString();
				var i = m_Painter.SelectingRectIndex;
				if (ani.Rects != null && i >= 0 && i < ani.Rects.Count) {
					var rect = ani.Rects[i];
					m_MinSizeIF.text = rect.MinHeight.ToString();
				}
				m_HighlightTint.GetComponent<Image>().color = ani.HighlightTint;
				m_ScaleMutiIF.text = data.ScaleMuti_UI.ToString();
				m_LuminWidthAppendIF.text = data.LuminousAppendX_UI.ToString();
				m_LuminHeightAppendIF.text = data.LuminousAppendY_UI.ToString();
				m_VanishDurationIF.text = data.VanishDuration_UI.ToString();
				m_FixedRatioTG.isOn = ani.FixedRatio;
				m_TintNoteTG.isOn = data.TintNote;
				m_FrontPoleTG.isOn = data.FrontPole;
			} catch { }
			UIReady = true;
		}


		public AnimatedItemData GetEditingAniData () {
			if (Data is null || Data.Items is null) { return null; }
			return (int)EditingType < Data.Items.Count ? Data.Items[(int)EditingType] : null;
		}


		public void ShowPainterMenu () => OpenMenu(PAINTER_MENU_KEY);


		// UI
		public void UI_ImportImage () {
			if (Data == null) { return; }
			var path = DialogUtil.PickFileDialog(DIALOG_ImportImageTitle, "image", "png", "jpg");
			if (string.IsNullOrEmpty(path)) { return; }
			var (pixels32, width, height) = Util.ImageToPixels(path);
			if (pixels32 is null || width == 0 || height == 0) { return; }
			var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp,
			};
			texture.SetPixels32(pixels32);
			texture.Apply();
			// Trim
			if (width > TEXTURE_MAX_SIZE || height > TEXTURE_MAX_SIZE) {
				texture = Util.ResizeTexture(texture, TEXTURE_MAX_SIZE);
			}
			// Final
			Data.SetPng(texture);
			Save();
			Painter.SetTexture(Data.Texture);
			TrimAllRects();
		}


		public void UI_ExportImage () {
			if (Data == null || Data.Texture == null) { return; }
			var path = DialogUtil.CreateFileDialog(DIALOG_ExportImageTitle, $"{SkinName}_Export", "png");
			if (string.IsNullOrEmpty(path)) { return; }
			try {
				Util.ByteToFile(Data.Texture.EncodeToPNG(), path);
				DialogUtil.Dialog_OK(DIALOG_SkinImageExported, DialogUtil.MarkType.Success);
			} catch { }
		}


		public void UI_DeleteSelection () {
			Painter.DeleteSelection();
		}


		public void UI_SetDarkBackground (bool dark) => m_Background.color = dark ? new Color(0.055f, 0.055f, 0.055f, 1f) : Color.white;


		public void UI_HideSkinEditorUI () => transform.localScale = Vector3.zero;


		#endregion




		#region --- LGC ---


		private void TrimAllRects () {
			var ani = GetEditingAniData();
			if (Data is null || Data.Texture is null || ani is null || ani.Rects is null || ani.Rects.Count == 0) { return; }
			int width = Data.Texture.width;
			int height = Data.Texture.height;
			for (int i = 0; i < ani.Rects.Count; i++) {
				var rect = ani.Rects[i];
				rect.Width = Mathf.Clamp(rect.Width, 0, width);
				rect.Height = Mathf.Clamp(rect.Height, 0, height);
				rect.BorderU = Mathf.Clamp(rect.BorderU, 0, rect.Height);
				rect.BorderD = Mathf.Clamp(rect.BorderD, 0, rect.Height);
				rect.BorderL = Mathf.Clamp(rect.BorderL, 0, rect.Width);
				rect.BorderR = Mathf.Clamp(rect.BorderR, 0, rect.Width);
				rect.X = Mathf.Clamp(rect.X, 0, width - rect.Width);
				rect.Y = Mathf.Clamp(rect.Y, 0, height - rect.Height);
				ani.Rects[i] = rect;
			}
			Painter.SetSelection(Painter.SelectingRectIndex);
		}


		#endregion


	}
}