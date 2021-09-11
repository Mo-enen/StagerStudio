namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;
	using Saving;
	using UnityEngine.UI;


	public class StageLanguage : MonoBehaviour {




		#region --- SUB ---


		public delegate void VoidHandler ();


		#endregion




		#region --- VAR ---


		// Handle
		public static VoidHandler OnLanguageLoaded { get; set; } = null;

		// API
		public List<SystemLanguage> AllLanguages => _AllLanguages ?? (_AllLanguages = RefreshAllLanguageNames());

		public Dictionary<string, string> Map { get; } = new Dictionary<string, string>();

		// Short
		private string RootPath => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Language");

		// Ser
		[SerializeField] private string[] m_DefaultData = null;

		// Data
		private static readonly Dictionary<SystemLanguage, string> DISPLAY_NAME_MAP = new Dictionary<SystemLanguage, string>() {
			{SystemLanguage.Afrikaans, "Arabisch"},
			{SystemLanguage.Arabic, "عربى" },
			{SystemLanguage.Basque, "Euskal" },
			{SystemLanguage.Belarusian, "беларускі" },
			{SystemLanguage.Bulgarian, "български" },
			{SystemLanguage.Catalan, "Català" },
			{SystemLanguage.Chinese, "中文" },
			{SystemLanguage.ChineseSimplified, "简体中文" },
			{SystemLanguage.ChineseTraditional, "正体中文" },
			{SystemLanguage.Czech, "čeština" },
			{SystemLanguage.Danish, "dansk" },
			{SystemLanguage.Dutch, "Nederlands" },
			{SystemLanguage.English, "English" },
			{SystemLanguage.Estonian, "Eesti keel" },
			{SystemLanguage.Finnish, "Suomalainen" },
			{SystemLanguage.French, "Français" },
			{SystemLanguage.German, "Deutsch" },
			{SystemLanguage.Greek, "Ελληνικά" },
			{SystemLanguage.Hebrew, "עברית" },
			{SystemLanguage.Hungarian, "Magyar" },
			{SystemLanguage.Icelandic, "Íslensku" },
			{SystemLanguage.Indonesian, "bahasa Indonesia" },
			{SystemLanguage.Italian, "Italiano" },
			{SystemLanguage.Japanese, "日本語" },
			{SystemLanguage.Korean, "한국어" },
			{SystemLanguage.Latvian, "Latviešu valoda" },
			{SystemLanguage.Lithuanian, "Lietuvių" },
			{SystemLanguage.Norwegian, "norsk" },
			{SystemLanguage.Polish, "Polskie" },
			{SystemLanguage.Portuguese, "Português" },
			{SystemLanguage.Romanian, "Română" },
			{SystemLanguage.Russian, "русский" },
			{SystemLanguage.SerboCroatian, "Српско-хрватски" },
			{SystemLanguage.Slovak, "slovenský" },
			{SystemLanguage.Slovenian, "Slovenščina" },
			{SystemLanguage.Spanish, "Español" },
			{SystemLanguage.Swedish, "svenska" },
			{SystemLanguage.Thai, "ไทย" },
			{SystemLanguage.Turkish, "Türk" },
			{SystemLanguage.Ukrainian, "Українська" },
			{SystemLanguage.Vietnamese, "Tiếng Việt" },
		};
		private Dictionary<string, string> DefaultMap { get; } = new Dictionary<string, string>();
		private List<SystemLanguage> _AllLanguages { get; set; } = null;

		// Saving
		private SavingInt LanguageIndex = new SavingInt("StageLanguage.LanguageIndex", -1);


		#endregion




		#region --- MSG ---


		private void Awake () {

			// Init Default Map
			DefaultMap.Clear();
			for (int i = 0; i < m_DefaultData.Length - 1; i += 2) {
				var key = m_DefaultData[i];
				if (DefaultMap.ContainsKey(key)) {
					DefaultMap.Add(key, m_DefaultData[i + 1]);
				}
			}
			m_DefaultData = null;

			// Init Index
			if (LanguageIndex < 0) {
				LanguageIndex.Value = (int)Application.systemLanguage;
			}

			// Load Language
			var language = (SystemLanguage)LanguageIndex.Value;
			if (language == SystemLanguage.Chinese) {
				language = SystemLanguage.ChineseSimplified;
			}
			bool success = LoadLanguage(language);
			if (!success) {
				switch (language) {
					case SystemLanguage.Chinese:
						success = LoadLanguage(SystemLanguage.ChineseSimplified);
						if (!success) {
							success = LoadLanguage(SystemLanguage.ChineseTraditional);
						}
						break;
					case SystemLanguage.ChineseSimplified:
						success = LoadLanguage(SystemLanguage.Chinese);
						if (!success) {
							success = LoadLanguage(SystemLanguage.ChineseTraditional);
						}
						break;
					case SystemLanguage.ChineseTraditional:
						success = LoadLanguage(SystemLanguage.Chinese);
						if (!success) {
							success = LoadLanguage(SystemLanguage.ChineseSimplified);
						}
						break;
				}
			}
			if (!success) {
				LoadLanguage(SystemLanguage.English);
			}

		}


		#endregion




		#region --- API ---


		public List<SystemLanguage> RefreshAllLanguageNames () {
			_AllLanguages = new List<SystemLanguage>();
			try {
				var path = RootPath;
				if (!Util.DirectoryExists(path)) { return _AllLanguages; }
				var files = Util.GetFilesIn(path, true, "*.txt");
				foreach (var file in files) {
					var name = Util.GetNameWithoutExtension(file.Name);
					if (System.Enum.TryParse(name, out SystemLanguage language)) {
						_AllLanguages.Add(language);
					}
				}
			} catch { }
			return _AllLanguages;
		}


		public bool LoadLanguage (SystemLanguage language) {
			// Load to Map
			bool success = GetLanguageMap(language, Map);
			if (success) {
				// Saving
				LanguageIndex.Value = (int)language;
				// MSG
				OnLanguageLoaded.Invoke();
			}
			return success;
		}


		public bool GetLanguageMap (SystemLanguage language, Dictionary<string, string> map) {
			try {
				var path = Util.CombinePaths(RootPath, language.ToString() + ".txt");
				if (!Util.FileExists(path)) { return false; }
				map.Clear();
				using (var reader = new StreamReader(path, true)) {
					string key = null, value = null;
					string line = null;
					while ((line = reader.ReadLine()) != null) {
						int i = line.IndexOf(':');
						if (i >= 0) {
							key = line.Substring(0, i);
							value = line.Substring(i + 1, line.Length - i - 1);
							if (!string.IsNullOrEmpty(key) && !map.ContainsKey(key)) {
								map.Add(key, value);
							}
						}
					}
				}
			} catch { }
			return true;
		}


		public string Get (string key) {
			return Map.ContainsKey(key) ? Map[key] : DefaultMap.ContainsKey(key) ? DefaultMap[key] : "";
		}


		public string GetDisplayName () => GetDisplayName((SystemLanguage)LanguageIndex.Value);


		public string GetDisplayName (SystemLanguage language) => DISPLAY_NAME_MAP.ContainsKey(language) ? DISPLAY_NAME_MAP[language] : "";


		#endregion




	}
}




#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Stage;


	[CustomEditor(typeof(StageLanguage)), DisallowMultipleComponent]
	public class StageLanguage_Inspector : Editor {


		// SUB
		private class StringStringComparer : IComparer<(string, string)> {
			public int Compare ((string, string) x, (string, string) y) => x.Item1.CompareTo(y.Item1);
		}


		// VAR
		private List<SystemLanguage> Languages { get; } = new List<SystemLanguage>();
		private (string key, string value)[][] Datas { get; set; } = new (string, string)[0][];
		private string RootPath => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Language");



		// MSG
		private void OnEnable () {

			Util.CreateFolder(RootPath);

			var targetLanguage = target as StageLanguage;
			// Get Languages
			var languages = targetLanguage.RefreshAllLanguageNames();
			Languages.Clear();
			foreach (var language in languages) {
				Languages.Add(language);
			}
			// Get Keys
			var tempKeysMap = new Dictionary<string, byte>();
			var keys = new List<string>();
			for (int i = 0; i < Languages.Count; i++) {
				var language = Languages[i];
				var map = new Dictionary<string, string>();
				if (targetLanguage.GetLanguageMap(language, map)) {
					foreach (var pair in map) {
						if (!tempKeysMap.ContainsKey(pair.Key)) {
							tempKeysMap.Add(pair.Key, 0);
							keys.Add(pair.Key);
						}
					}
				}
			}
			// Get Map
			Datas = new (string, string)[Languages.Count][];
			for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++) {
				var language = Languages[languageIndex];
				var values = new (string, string)[keys.Count];
				var map = new Dictionary<string, string>();
				if (targetLanguage.GetLanguageMap(language, map)) {
					for (int i = 0; i < keys.Count; i++) {
						var key = keys[i];
						if (map.ContainsKey(key)) {
							values[i] = (key, map[key]);
						} else {
							values[i] = (key, "");
						}
					}
				}
				System.Array.Sort(values, new StringStringComparer());
				Datas[languageIndex] = values;
			}
		}


		private void OnDisable () => Save();


		public override void OnInspectorGUI () {

			Space(4);

			var leftLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleLeft, };

			// Buttons
			LayoutH(() => {
				GUIRect(0, 1);
				if (GUI.Button(GUIRect(62, 18), "Save", EditorStyles.miniButtonLeft)) {
					GUI.FocusControl(null);
					Save();
				}
				if (GUI.Button(GUIRect(62, 18), "+", EditorStyles.miniButtonRight)) {
					for (int i = 0; i < Datas.Length; i++) {
						int newSize = Datas[i].Length + 1;
						var list = new List<(string, string)>(Datas[i]);
						list.Insert(0, ("", ""));
						Datas[i] = list.ToArray();
					}
				}
				GUIRect(0, 1);
			});
			Space(4);

			// Bar
			LayoutH(() => {
				GUI.Label(GUIRect(0, 18), "Key × " + Datas[0].Length.ToString(), leftLabelStyle);
				foreach (var language in Languages) {
					EditorGUI.DelayedTextField(GUIRect(0, 18), (target as StageLanguage).GetDisplayName(language), leftLabelStyle);
				}
			});

			// Content
			string prevKey = "";
			for (int i = 0; i < Datas[0].Length; i++) {
				// Tag
				var key = Datas[0][i].key ?? "";
				int index = key.IndexOf('.');
				if (index >= 0) {
					key = key.Substring(0, index);
				}
				if (key != prevKey && !string.IsNullOrEmpty(key) && i != 0) {
					GUI.Label(GUIRect(0, 12), key, leftLabelStyle);
					Space(2);
				}
				prevKey = key;
				// Content
				LayoutH(() => {
					var newKey = EditorGUI.DelayedTextField(GUIRect(0, 18), Datas[0][i].key);
					// Key
					if (newKey != Datas[0][i].key) {
						for (int j = 0; j < Datas.Length; j++) {
							Datas[j][i] = (newKey, Datas[j][i].value);
						}
					}
					// Languages
					for (int j = 0; j < Datas.Length; j++) {
						string value = Datas[j][i].value;
						string newValue = EditorGUI.DelayedTextField(GUIRect(0, 18), value);
						if (newValue != value) {
							Datas[j][i] = (Datas[j][i].key, newValue);
						}
					}
				});
			}
			Space(4);

			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl(null);
			}
			//base.OnInspectorGUI();

		}


		// LGC
		private void Save () {
			// Save Maps
			int defaultIndex = 0;
			int index = 0;
			foreach (var language in Languages) {
				try {
					var path = Util.CombinePaths(RootPath, language.ToString() + ".txt");
					var builder = new System.Text.StringBuilder();
					var values = Datas[index];
					foreach (var value in values) {
						string key = System.Text.RegularExpressions.Regex.Replace(value.key, @"[^a-zA-Z0-9.]", "");
						bool hasLetter = System.Text.RegularExpressions.Regex.IsMatch(key, @"[a-zA-Z]");
						if (!hasLetter) { continue; }
						builder.Append(key);
						builder.Append(':');
						builder.Append(value.value);
						builder.AppendLine();
					}
					Util.TextToFile(builder.ToString(), path);
					if (language == SystemLanguage.English) {
						defaultIndex = index;
					}
					index++;
				} catch (System.Exception ex) {
					Debug.LogError(ex.Message);
				}
			}
			// Save Default Data 
			serializedObject.Update();
			var prop_DefaultData = serializedObject.FindProperty("m_DefaultData");
			prop_DefaultData.arraySize = Datas[defaultIndex].Length * 2;
			var dataValues = Datas[defaultIndex];
			for (int i = 0; i < dataValues.Length; i++) {
				var prop = prop_DefaultData.GetArrayElementAtIndex(i * 2 + 0);
				prop.stringValue = dataValues[i].key;
				prop = prop_DefaultData.GetArrayElementAtIndex(i * 2 + 1);
				prop.stringValue = dataValues[i].value;
			}
			serializedObject.ApplyModifiedProperties();
			// Save Helper
			var hBuilder = new System.Text.StringBuilder();
			foreach (var name in System.Enum.GetNames(typeof(SystemLanguage))) {
				if (name == "Hugarian" || name == "Chinese" || name == "Unknown") { continue; }
				hBuilder.AppendLine(name);
			}
			var helperPath = Util.CombinePaths(RootPath, "_File Name Helper.txt");
			Util.TextToFile(hBuilder.ToString(), helperPath);
		}


		// UTL
		private Rect GUIRect (float width, float height) => GUILayoutUtility.GetRect(
			width, height,
			GUILayout.ExpandWidth(width == 0),
			GUILayout.ExpandHeight(height == 0)
		);


		private void LayoutH (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = GUI.skin.box;
			}
			if (style != null) {
				GUILayout.BeginHorizontal(style);
			} else {
				GUILayout.BeginHorizontal();
			}
			action();
			GUILayout.EndHorizontal();
		}


		private void Space (float space = 4f) => GUILayout.Space(space);




	}
}
#endif