namespace Moenen.Tools {
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Reflection;
	using System.IO;
	using UnityEditor.SceneManagement;


	public class MoenenTools {




		private static long PrevDoTheThingTime = 0;


		// Clear Console
		// & alt   % ctrl   # Shift
		[MenuItem("Tools/Do the Thing _F5")]
		public static void ClearAndReStage () {
			// Clear Console
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			var type = assembly.GetType("UnityEditorInternal.LogEntries");
			if (type == null) {
				type = assembly.GetType("UnityEditor.LogEntries");
			}
			var method = type.GetMethod("Clear");
			method.Invoke(new object(), null);

			long time = System.DateTime.Now.Ticks;
			if (time - PrevDoTheThingTime < 5000000) {
				// Deselect
				Selection.activeObject = null;
				// Save
				if (!EditorApplication.isPlaying) {
					EditorSceneManager.SaveOpenScenes();
				}
			}
			PrevDoTheThingTime = time;
		}



		[MenuItem("Tools/Open VS" +
			"")]
		public static void OpenVS () {
			var files = GetFilesIn(GetParentPath(Application.dataPath), true, "*.sln");
			if (files != null && files.Length > 0) {
				Application.OpenURL(new System.Uri(files[0].FullName).AbsoluteUri);
			}
		}




		// UTL
		private static string GetParentPath (string path) => Directory.GetParent(path).FullName;


		private static FileInfo[] GetFilesIn (string path, bool topOnly, params string[] searchPattern) {
			var allFiles = new List<FileInfo>();
			if (PathIsDirectory(path)) {
				var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
				if (searchPattern.Length == 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*", option));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], option));
					}
				}
			}
			return allFiles.ToArray();
		}


		private static bool PathIsDirectory (string path) {
			if (!Directory.Exists(path)) { return false; }
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}


	}


}