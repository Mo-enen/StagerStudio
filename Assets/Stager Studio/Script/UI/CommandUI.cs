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
		private static readonly string[] TARGET_MENU = {
			"Menu.Command.Target.None",
			"Menu.Command.Target.Stage",
			"Menu.Command.Target.Track",
			"Menu.Command.Target.TrackInside",
			"Menu.Command.Target.Note",
			"Menu.Command.Target.NoteInside",
			"Menu.Command.Target.Timing",
		};
		private static readonly string[] COMMAND_MENU = {
			"Menu.Command.Command.None",
			"Menu.Command.Command.Time",
			"Menu.Command.Command.X",
			"Menu.Command.Command.Width",
			"Menu.Command.Command.Delete",
		};

		// Handler
		public static CommandHandler DoCommand { get; set; } = null;
		public static MenuHandler OpenMenu { get; set; } = null;
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Ser
		[SerializeField] private RectTransform m_Window = null;
		[SerializeField] private InputField m_IndexIF = null;
		[SerializeField] private InputField m_ValueIF = null;
		[SerializeField] private Text m_TargetLabel = null;
		[SerializeField] private Text m_CommandLabel = null;
		[SerializeField] private MenuData[] Menus = null;

		// Data
		private bool UIReady = true;
		private int TargetIndex = 0;
		private int CommandIndex = 0;


		// MSG
		private void Awake () => m_Window.anchoredPosition3D = new Vector2(m_Window.anchoredPosition3D.x, -m_Window.rect.height - 12f);


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
			CommandIndex = 0;
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

				if (TargetIndex >= 0 && TargetIndex < TARGET_MENU.Length) {
					m_TargetLabel.text = GetLanguage(TARGET_MENU[TargetIndex]);
				}

				if (TargetIndex >= 0 && TargetIndex < Menus.Length) {
					var menu = Menus[TargetIndex];
					if (CommandIndex >= 0 && CommandIndex < menu.CommandIndexs.Length) {
						m_CommandLabel.text = GetLanguage(COMMAND_MENU[menu.CommandIndexs[CommandIndex]]);
					}
				}

				m_IndexIF.gameObject.SetActive(TargetIndex == 3 || TargetIndex == 5);
				m_ValueIF.gameObject.SetActive(CommandIndex == 1 || CommandIndex == 2 || CommandIndex == 3);

			} catch { }
			UIReady = true;
		}


	}
}