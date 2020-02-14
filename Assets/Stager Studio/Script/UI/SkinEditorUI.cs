namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using Object;
	using Stage;
	using Data;


	public class SkinEditorUI : MonoBehaviour {




		#region --- VAR ---


		// Handler
		public delegate string LanguageHandler (string key);

		// Const
		private const string HINT_Saved = "SkinEditor.Saved";
		private const string PAINTER_MENU_KEY = "Menu.SkinEditor.Painter";
		private const string DIALOG_CloseConfirm = "SkinEditor.Dialog.CloseConfirm";
		private const string DIALOG_ImportImageTitle = "SkinEditor.Dialog.ImportImageTitle";
		private const int TEXTURE_MAX_SIZE = 1024;

		// API
		public static LanguageHandler GetLanguage { get; set; } = null;
		public SkinEditorPainterUI Painter => m_Painter;
		public SkinData Data { get; private set; } = new SkinData();
		public SkinType EditingType { get; private set; } = SkinType.Stage;
		public int EditingFrame { get; private set; } = 0;
		public string SkinName { get => m_SkinNameIF.text; set => m_SkinNameIF.text = value; }
		public string SkinAuthor { get => m_SkinAuthorIF.text; set => m_SkinAuthorIF.text = value; }
		public bool ApplyToAllSprite { get; private set; } = false;

		// Ser
		[SerializeField] private SkinEditorPainterUI m_Painter = null;
		[SerializeField] private InputField m_SkinNameIF = null;
		[SerializeField] private InputField m_SkinAuthorIF = null;
		[SerializeField] private InputField m_ScaleMutiIF = null;
		[SerializeField] private InputField m_LuminWidthAppendIF = null;
		[SerializeField] private InputField m_LuminHeightAppendIF = null;
		[SerializeField] private InputField m_NoteThicknessIF = null;
		[SerializeField] private InputField m_PoleThicknessIF = null;
		[SerializeField] private InputField m_DurationIF = null;
		[SerializeField] private Image m_Background = null;
		[SerializeField] private RectTransform m_TypeTgContainer = null;
		[SerializeField] private Button[] m_LoopTypeBtns = null;
		[SerializeField] private Text[] m_LanguageTexts = null;

		// Data
		private StageSkin Skin = null;
		private StageMenu Menu = null;
		private StageLanguage Language = null;
		private HintBarUI Hint = null;


		#endregion




		#region --- API ---


		public void Init (SkinData skinData, string skinName) {

			if (skinData == null || string.IsNullOrEmpty(skinName)) {
				Skin.ReloadSkin();
				StagerStudio.Main.UI_SpawnSetting();
				return;
			}

			// Stuffs...
			Hint = FindObjectOfType<HintBarUI>();
			Language = FindObjectOfType<StageLanguage>();
			Menu = FindObjectOfType<StageMenu>();
			Skin = FindObjectOfType<StageSkin>();

			// Language
			foreach (var text in m_LanguageTexts) {
				text.text = Language.Get(text.name);
			}

			// Skin
			Data = skinData;
			SkinAuthor = skinData.Author;
			SkinName = skinName;

			// Duration
			m_DurationIF.onEndEdit.AddListener((str) => {
				var ani = GetEditingAniData();
				if (ani is null || EditingFrame < 0 || EditingFrame >= ani.Rects.Count) { return; }
				if (int.TryParse(str, out int durationMS)) {
					ani.SetDuration(durationMS);
					m_DurationIF.text = ani.FrameDuration.ToString();
				}
			});

			// Scale Muti
			m_ScaleMutiIF.onEndEdit.AddListener((str) => {
				if (float.TryParse(str, out float scaleMT)) {
					Data.ScaleMuti_UI = scaleMT;
					m_ScaleMutiIF.text = Data.ScaleMuti_UI.ToString();
				}
			});

			// Luminous Append
			m_LuminWidthAppendIF.onEndEdit.AddListener((str) => {
				if (float.TryParse(str, out float lwa)) {
					Data.LuminousAppendX = Mathf.Clamp(lwa, -1f, 1f);
					m_LuminWidthAppendIF.text = Data.LuminousAppendX.ToString();
				}
			});
			m_LuminHeightAppendIF.onEndEdit.AddListener((str) => {
				if (float.TryParse(str, out float lwa)) {
					Data.LuminousAppendY = Mathf.Clamp(lwa, -1f, 1f);
					m_LuminHeightAppendIF.text = Data.LuminousAppendY.ToString();
				}
			});

			// Note Thickness
			m_NoteThicknessIF.onEndEdit.AddListener((str) => {
				if (float.TryParse(str, out float thick)) {
					Data.NoteThickness_UI = thick;
					m_NoteThicknessIF.text = Data.NoteThickness_UI.ToString();
				}
			});

			// Pole Thickness
			m_PoleThicknessIF.onEndEdit.AddListener((str) => {
				if (float.TryParse(str, out float thick)) {
					Data.PoleThickness_UI = thick;
					m_PoleThicknessIF.text = Data.PoleThickness_UI.ToString();
				}
			});

			// Type TGs
			int len = m_TypeTgContainer.childCount;
			for (int i = 0; i < len; i++) {
				var tg = m_TypeTgContainer.GetChild(i).GetComponent<Toggle>();
				int index = i;
				tg.onValueChanged.AddListener((isOn) => {
					EditingType = (SkinType)index;
					Painter.SetItemsDirty();
					Painter.ResetRootPositionSize();
					Painter.SetSelection(-1);
					RefreshInfoUI();
					TrimAllRects();
				});
			}

			// UI
			RefreshInfoUI();
			Painter.SetItemsDirty();
			Painter.SetTexture(Data.Texture);
			TrimAllRects();
			Painter.SetSelection(-1);
		}


		public void Save () {
			if (Data is null) { return; }
			Data.Author = SkinAuthor;
			Skin.SaveSkin(Data, SkinName);
			Skin.ReloadSkin();
			Hint?.SetHint(Language.Get(HINT_Saved));
			Resources.UnloadUnusedAssets();
		}


		public void Close () {
			DialogUtil.Dialog_OK_Cancel(DIALOG_CloseConfirm, DialogUtil.MarkType.Warning, () => {
				Skin.SaveSkin(Data, SkinName);
				Skin.ReloadSkin();
				Resources.UnloadUnusedAssets();
				StagerStudio.Main.UI_SpawnSetting();
			});
		}


		public void RefreshInfoUI () {
			var data = Data;
			var ani = GetEditingAniData();
			if (data is null || ani is null) { return; }
			try {
				// Duration
				m_DurationIF.text = ani.FrameDuration.ToString();
				// Scale Muti
				m_ScaleMutiIF.text = data.ScaleMuti_UI.ToString();
				// Luminous Append
				m_LuminWidthAppendIF.text = data.LuminousAppendX.ToString();
				m_LuminHeightAppendIF.text = data.LuminousAppendY.ToString();
				// Note Thickness
				m_NoteThicknessIF.text = data.NoteThickness_UI.ToString();
				// Pole Thickness
				m_PoleThicknessIF.text = data.PoleThickness_UI.ToString();
				// Loop
				for (int i = 0; i < m_LoopTypeBtns.Length; i++) {
					m_LoopTypeBtns[i].gameObject.SetActive(i == (int)ani.Loop);
				}
			} catch { }
		}


		public AnimatedItemData GetEditingAniData () {
			if (Data is null || Data.Items is null) { return null; }
			return (int)EditingType < Data.Items.Count ? Data.Items[(int)EditingType] : null;
		}


		public void ShowPainterMenu () {
			Menu.OpenMenu(PAINTER_MENU_KEY);
		}


		// UI
		public void UI_ImportImage () {
			if (Data is null) { return; }
			var path = DialogUtil.PickFileDialog(DIALOG_ImportImageTitle, "image", "png", "jpg");
			if (string.IsNullOrEmpty(path)) { return; }
			var (pixels32, width, height) = Util.ImageToPixels(path);
			if (pixels32 is null || width == 0 || height == 0) { return; }
			var texture = new Texture2D(width, height, TextureFormat.ARGB32, false) {
				filterMode = FilterMode.Point,
				alphaIsTransparency = true,
			};
			texture.SetPixels32(pixels32);
			texture.Apply();
			// Trim
			if (width > TEXTURE_MAX_SIZE || height > TEXTURE_MAX_SIZE) {
				texture = Util.ResizeTexture(texture, TEXTURE_MAX_SIZE);
			}
			// Final
			Data.Texture = texture;
			Save();
			Painter.SetTexture(Data.Texture);
			TrimAllRects();
		}


		public void UI_SetLoopType (int typeIndex) {
			var data = GetEditingAniData();
			if (data is null) { return; }
			data.Loop = (SkinLoopType)typeIndex;
			RefreshInfoUI();
		}


		public void UI_DeleteSelection () {
			Painter.DeleteSelection();
		}


		public void UI_SetDarkBackground (bool dark) => m_Background.color = dark ? new Color(0.055f, 0.055f, 0.055f, 1f) : Color.white;


		public void UI_SetApplyAllSprite (bool apply) {
			ApplyToAllSprite = apply;
		}


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