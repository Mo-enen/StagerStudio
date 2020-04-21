namespace StagerStudio {
	using UnityEngine;
	//using Crosstales.FB;
	using SFB;
	using UI;


	public static class DialogUtil {






		#region --- SUB ---


		public delegate string StringStringHandler (string str);
		public delegate RectTransform RectTransformHandler ();
		public delegate DialogUI DialogUIHandler ();


		public enum MarkType {
			Info = 0,
			Success = 1,
			Warning = 2,
			Error = 3,

		}


		#endregion




		#region --- VAR ---


		// Handler
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static RectTransformHandler GetRoot { get; set; } = null;
		public static DialogUIHandler GetPrefab { get; set; } = null;


		#endregion




		#region --- API ---


		// ok yes delete no cancel
		public static void Dialog_OK (string contentKey, MarkType mark, System.Action ok = null) => OpenLogic(GetLanguage(contentKey), mark, -1f, ok is null ? () => { } : ok);
		public static void Dialog_OK_Cancel (string contentKey, MarkType mark, System.Action ok) => OpenLogic(GetLanguage(contentKey), mark, -1f, ok, null, null, null, () => { });
		public static void Dialog_Yes_No_Cancel (string contentKey, MarkType mark, System.Action yes, System.Action no) => OpenLogic(GetLanguage(contentKey), mark, -1f, null, yes, null, no, () => { });
		public static void Dialog_Delete_Cancel (string contentKey, System.Action delete) => OpenLogic(GetLanguage(contentKey), MarkType.Warning, -1f, null, null, delete, null, () => { });
		public static void Open (string content, MarkType mark, params System.Action[] actions) => OpenLogic(content, mark, -1f, actions);
		public static void Open (string content, MarkType mark, float height, params System.Action[] actions) => OpenLogic(content, mark, height, actions);


		// File
		public static string PickFolderDialog (string key) {
			var lastPickedFolder = PlayerPrefs.GetString(
				"DialogUtil.LastPickedFolder",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
			);
			var paths = StandaloneFileBrowser.OpenFolderPanel(GetLanguage(key), lastPickedFolder, false);
			var path = paths is null || paths.Length == 0 ? "" : paths[0];
			//string path = FileBrowser.OpenSingleFolder(GetLanguage(key), lastPickedFolder);
			if (!string.IsNullOrEmpty(path)) {
				PlayerPrefs.SetString("DialogUtil.LastPickedFolder", GetParentPath(path));
				return path;
			}
			return "";
		}


		public static string PickFileDialog (string key, string filterName, params string[] filters) {
			var lastPickedFolder = PlayerPrefs.GetString(
				"DialogUtil.LastPickedFolder",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
			);
			var paths = StandaloneFileBrowser.OpenFilePanel(GetLanguage(key), lastPickedFolder, new ExtensionFilter[1] { new ExtensionFilter(filterName, filters) }, false);
			var path = paths is null || paths.Length == 0 ? "" : paths[0];
			//var path = FileBrowser.OpenSingleFile(GetLanguage(key), lastPickedFolder, new ExtensionFilter[1] { new ExtensionFilter(filterName, filters) });
			if (!string.IsNullOrEmpty(path)) {
				PlayerPrefs.SetString("DialogUtil.LastPickedFolder", GetParentPath(path));
				return path;
			}

			return "";
		}


		public static string CreateFileDialog (string key, string defaultName, string ext) {
			var lastPickedFolder = PlayerPrefs.GetString(
				"DialogUtil.LastPickedFolder",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
			);
			var path = StandaloneFileBrowser.SaveFilePanel(GetLanguage(key), lastPickedFolder, defaultName, ext);
			//var path = FileBrowser.SaveFile(GetLanguage(key), lastPickedFolder, defaultName, ext);
			if (!string.IsNullOrEmpty(path)) {
				PlayerPrefs.SetString("DialogUtil.LastPickedFolder", GetParentPath(path));
				return path;
			}
			return "";
		}


		private static string GetParentPath (string path) => System.IO.Directory.GetParent(path).FullName;



		#endregion




		#region --- LGC ---


		private static void OpenLogic (string content, MarkType mark, float height, params System.Action[] actions) {
			if (actions is null || actions.Length == 0) { return; }
			var root = GetRoot();
			root.parent.gameObject.SetActive(true);
			root.DestroyAllChildImmediately();
			root.gameObject.SetActive(true);
			Util.SpawnUI(GetPrefab(), root, "Dialog").Init(content, (int)mark, height, actions);
		}


		#endregion




	}
}