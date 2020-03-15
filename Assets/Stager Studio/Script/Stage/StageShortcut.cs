namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;


	public class StageShortcut : MonoBehaviour {


		// SUB
		[System.Serializable]
		public class ShortcutData {
			private readonly static Dictionary<KeyCode, string> SpecialKeyNames = new Dictionary<KeyCode, string> {
				{ KeyCode.Alpha0, "0" },
				{ KeyCode.Alpha1, "1" },
				{ KeyCode.Alpha2, "2" },
				{ KeyCode.Alpha3, "3" },
				{ KeyCode.Alpha4, "4" },
				{ KeyCode.Alpha5, "5" },
				{ KeyCode.Alpha6, "6" },
				{ KeyCode.Alpha7, "7" },
				{ KeyCode.Alpha8, "8" },
				{ KeyCode.Alpha9, "9" },
				{ KeyCode.UpArrow, "↑" },
				{ KeyCode.DownArrow, "↓" },
				{ KeyCode.LeftArrow, "←" },
				{ KeyCode.RightArrow, "→" },
			};
			public string Name = "";
			public KeyCode Key = KeyCode.None;
			public bool Ctrl = false;
			public bool Shift = false;
			public bool Alt = false;
			public Transform[] AntiTFs = null;
			public UnityEvent Action = null;
			public override string ToString () {
				var builder = new System.Text.StringBuilder();
				builder.Append(' ');
				if (Ctrl) {
					builder.Append("<color=#25D2A8ff>Ctrl</color>+");
				}
				if (Shift) {
					builder.Append("<color=#25D2A8ff>Shift</color>+");
				}
				if (Alt) {
					builder.Append("<color=#25D2A8ff>Alt</color>+");
				}
				builder.Append($"<color=#25D2A8ff>{(SpecialKeyNames.ContainsKey(Key) ? SpecialKeyNames[Key] : Key.ToString())}</color>");
				builder.Append(' ');
				return builder.ToString();
			}
		}


		// Ser
		[SerializeField] private ShortcutData[] m_Datas = null;

		// Data
		private Dictionary<(KeyCode code, bool ctrl, bool shift, bool alt), (UnityEvent action, Transform[] antiTFs)> Map { get; } = new Dictionary<(KeyCode, bool, bool, bool), (UnityEvent, Transform[])>();


		// MSG
		private void Awake () {
			// Load From File
			string path = Util.CombinePaths(Application.streamingAssetsPath, "Shortcut", "Shortcut.txt");
			if (Util.FileExists(path)) {
				var strs = Util.FileToText(path).Replace("\t", "").Replace("\r", "").Replace("\n", "").Split('#');
				foreach (var str in strs) {
					if (string.IsNullOrEmpty(str)) { continue; }
					var s = str.Split(',');
					if (s.Length < 5) { continue; }
					string name = s[0];
					foreach (var data in m_Datas) {
						if (data.Name == name) {
							if (System.Enum.TryParse(s[1], out KeyCode key)) {
								data.Key = key;
							}
							if (bool.TryParse(s[2], out bool ctrl)) {
								data.Ctrl = ctrl;
							}
							if (bool.TryParse(s[3], out bool shift)) {
								data.Shift = shift;
							}
							if (bool.TryParse(s[4], out bool alt)) {
								data.Alt = alt;
							}
							break;
						}
					}
				}
			}
			// Get Map
			Map.Clear();
			foreach (var data in m_Datas) {
				var key = (data.Key, data.Ctrl, data.Shift, data.Alt);
				if (Map.ContainsKey(key)) { continue; }
				Map.Add(key, (data.Action, data.AntiTFs));
			}
		}


		private void OnGUI () {
			if (Util.IsTypeing || Event.current.type != EventType.KeyDown) { return; }
			var key = (Event.current.keyCode, Event.current.control, Event.current.shift, Event.current.alt);
			if (Map.ContainsKey(key)) {
				var (action, antiTFs) = Map[key];
				if (CheckAnti(antiTFs)) {
					action.Invoke();
				}
			}
			// Func
			bool CheckAnti (Transform[] antiTFs) {
				if (antiTFs is null) { return true; }
				foreach (var tf in antiTFs) {
					if (tf && tf.gameObject.activeSelf) { return false; }
				}
				return true;
			}
		}


		// API
		public string GetHotkeyLabel (string name) {
			foreach (var data in m_Datas) {
				if (data.Name == name) {
					return $"[{data.ToString()}]";
				}
			}
			return "";
		}


	}
}


#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Stage;


	[CustomEditor(typeof(StageShortcut))]
	public class StageShortcutInspector : Editor {


		private void OnDisable () {
			// Shortcut
			var p_Datas = serializedObject.FindProperty("m_Datas");
			var keyCodes = System.Enum.GetValues(typeof(KeyCode));
			string str = "";
			for (int i = 0; i < p_Datas.arraySize; i++) {
				var p_Data = p_Datas.GetArrayElementAtIndex(i);
				string name = p_Data.FindPropertyRelative("Name").stringValue;
				string key = ((KeyCode)p_Data.FindPropertyRelative("Key").intValue).ToString();
				bool ctrl = p_Data.FindPropertyRelative("Ctrl").boolValue;
				bool shift = p_Data.FindPropertyRelative("Shift").boolValue;
				bool alt = p_Data.FindPropertyRelative("Alt").boolValue;
				str += $"{name},\t{key},\t{ctrl},\t{shift},\t{alt}#\r\n";
			}
			Util.TextToFile(str, Util.CombinePaths(Application.streamingAssetsPath, "Shortcut", "Shortcut.txt"));
			// Helper
			str = $"Format of Shortcut.txt in StagerStudio v{Application.version}:\r\n\r\n";
			str += "\tAction,\tKey,\tCtrl,\tShift,\tAlt#\r\n\r\n";
			str += "Available Actions:\r\n\r\n";
			for (int i = 0; i < p_Datas.arraySize; i++) {
				var p_Data = p_Datas.GetArrayElementAtIndex(i);
				string name = p_Data.FindPropertyRelative("Name").stringValue;
				str += $"\t{name}\r\n";
			}
			str += "\r\nAvailable Keys:\r\n\r\n";
			for (int i = 0; i < keyCodes.Length; i++) {
				var key = (KeyCode)keyCodes.GetValue(i);
				str += $"\t{key.ToString()}\r\n";
			}
			Util.TextToFile(str, Util.CombinePaths(Application.streamingAssetsPath, "Shortcut", "Keycode Helper.txt"));
		}


	}
}
#endif