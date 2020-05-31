namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class CommandUI : MonoBehaviour {



		// SUB
		[System.Serializable]
		public class MenuData {
			public string TargetMenuKey = "";
			public int[] CommandIndexs = null;
		}
		public delegate void CommandHandler (int target, int command, int index, float value);
		public delegate void MenuHandler (string key);
		public delegate string StringStringHandler (string str);


		// Const
		private const string TARGET_MENU_KEY = "Menu.Command.Target";
		private static readonly (string key, bool hasIndex)[] TARGET_MENU = {
			("Menu.Command.Target.None", false),
			("Menu.Command.Target.Stage", false),
			("Menu.Command.Target.Track", false),
			("Menu.Command.Target.TrackInside", true),
			("Menu.Command.Target.Note", false),
			("Menu.Command.Target.NoteInside", true),
			("Menu.Command.Target.Timing", false),
		};
		private static readonly (string key, bool hasValue)[] COMMAND_MENU = {
			("Menu.Command.Command.None", false),
			("Menu.Command.Command.Time", true),
			("Menu.Command.Command.TimeAdd", true),
			("Menu.Command.Command.X", true),
			("Menu.Command.Command.XAdd", true),
			("Menu.Command.Command.Width", true),
			("Menu.Command.Command.WidthAdd", true),
		};

		// Handler
		public static CommandHandler DoCommand { get; set; } = null;
		public static MenuHandler OpenMenu { get; set; } = null;
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Api
		public static int TargetIndex { get; private set; } = 0;
		public static int CommandIndex { get; private set; } = 0;

		// Ser
		[SerializeField] private RectTransform m_Window = null;
		[SerializeField] private InputField m_IndexIF = null;
		[SerializeField] private InputField m_ValueIF = null;
		[SerializeField] private Text m_TargetLabel = null;
		[SerializeField] private Text m_CommandLabel = null;
		[SerializeField] private Text[] m_LanguageLabels = null;
		[SerializeField] private MenuData[] Menus = null;

		// Data
		private bool UIReady = true;


		// MSG
		private void Awake () {
			m_Window.anchoredPosition3D = new Vector2(m_Window.anchoredPosition3D.x, -m_Window.rect.height - 12f);
			foreach (var label in m_LanguageLabels) {
				label.text = GetLanguage(label.name);
			}
		}


		private void Start () => RefreshUI();


		private void Update () => m_Window.LerpUI(new Vector2(0f, 64f), 12f);


		// API
		public void UI_ApplyCommand () {
			if (!m_IndexIF.text.TryParseIntForInspector(out int index)) { return; }
			if (!m_ValueIF.text.TryParseFloatForInspector(out float value)) { return; }
			DoCommand(TargetIndex, CommandIndex, index, value);
		}


		public void SetTargetIndex (int index) {
			TargetIndex = index;
			if (TargetIndex >= 0 && TargetIndex < Menus.Length) {
				var menu = Menus[TargetIndex];
				bool needResetCommandIndex = true;
				foreach (var cIndex in menu.CommandIndexs) {
					if (cIndex == CommandIndex) {
						needResetCommandIndex = false;
						break;
					}
				}
				if (needResetCommandIndex) {
					CommandIndex = 0;
				}
			}
			RefreshUI();
		}


		public void SetCommandIndex (int index) {
			CommandIndex = index;
			RefreshUI();
		}


		public void UI_Close () {
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			DestroyImmediate(gameObject, false);
		}


		public void UI_OpenTargetMenu () => OpenMenu(TARGET_MENU_KEY);


		public void UI_OpenCommandMenu () {
			if (TargetIndex >= 0 && TargetIndex < Menus.Length) {
				OpenMenu(Menus[TargetIndex].TargetMenuKey);
			}
		}


		// LGC
		private void RefreshUI () {
			if (!UIReady) { return; }
			UIReady = false;
			try {

				if (m_IndexIF.text.TryParseIntForInspector(out int index)) {
					m_IndexIF.text = index.ToString();
				}

				if (m_ValueIF.text.TryParseFloatForInspector(out float value)) {
					m_ValueIF.text = value.ToString();
				}

				bool hasIndex = false;
				if (TargetIndex >= 0 && TargetIndex < TARGET_MENU.Length) {
					var tPair = TARGET_MENU[TargetIndex];
					m_TargetLabel.text = GetLanguage(tPair.key);
					hasIndex = tPair.hasIndex;
				}

				bool hasValue = false;
				if (CommandIndex >= 0 && CommandIndex < COMMAND_MENU.Length) {
					var cPair = COMMAND_MENU[CommandIndex];
					m_CommandLabel.text = GetLanguage(cPair.key);
					hasValue = cPair.hasValue;
				}

				m_IndexIF.gameObject.SetActive(hasIndex);
				m_ValueIF.gameObject.SetActive(hasValue);

			} catch { }
			UIReady = true;
		}


	}
}