namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class DialogUI : MonoBehaviour {

		// Handler
		public delegate string LanguageHandler (string key);
		public static LanguageHandler GetLanguage { get; set; } = null;

		// Ser
		[SerializeField] private RectTransform m_Window = null;
		[SerializeField] private RectTransform m_ButtonContainer = null;
		[SerializeField] private Text m_Content = null;
		[SerializeField] private Image m_Mark = null;
		[SerializeField] private Sprite[] m_MarkSP = null;
		[SerializeField] private Color[] m_MarkColors = null;
		[SerializeField] private Text[] m_LanguageTexts = null;

		// Data
		private System.Action[] ButtonActions = null;


		// MSG
		private void Update () {
			if (Input.GetKeyDown(KeyCode.Return)) {
				foreach (var action in ButtonActions) {
					if (!(action is null)) {
						action();
						Close();
						break;
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Escape)) {
				Close();
			}
		}


		// API
		public void Init (string content, int mark, float height, params System.Action[] actions) {
			// Language
			foreach (var txt in m_LanguageTexts) {
				txt.text = GetLanguage(txt.name);
			}
			int buttonCount = m_ButtonContainer.childCount;
			// Init Actions
			ButtonActions = new System.Action[buttonCount];
			for (int i = 0; i < ButtonActions.Length; i++) {
				ButtonActions[i] = null;
			}
			// Init Callback
			for (int i = 0; i < buttonCount; i++) {
				int index = i;
				var btn = m_ButtonContainer.GetChild(index).GetComponent<Button>();
				btn.onClick.AddListener(() => ButtonClick(index));
			}
			// Init Content
			m_Content.text = content;
			m_Mark.sprite = m_MarkSP[mark];
			m_Mark.color = m_MarkColors[mark];
			for (int i = 0; i < ButtonActions.Length; i++) {
				var action = i < actions.Length ? actions[i] : null;
				ButtonActions[i] = action;
				m_ButtonContainer.GetChild(i).gameObject.SetActive(!(action is null));
			}
			// Large
			if (height > 0f) {
				m_Window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
			}

			// Func
			void ButtonClick (int buttonID) {
				try {
					if (!(ButtonActions[buttonID] is null)) {
						ButtonActions[buttonID].Invoke();
					}
				} catch { }
				Close();
			}
		}


		public void Close () {
			if (gameObject == null) { return; }
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}


	}
}