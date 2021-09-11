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



		[MenuItem("Tools/Open VS")]
		public static void OpenVS () {
			var files = GetFilesIn(GetParentPath(Application.dataPath), true, "*.sln");
			if (files != null && files.Length > 0) {
				Application.OpenURL(new System.Uri(files[0].FullName).AbsoluteUri);
			}
		}



		[MenuItem("Tools/Async Res to Built")]
		public static void AsyncResToBuilt () {
			string fromRoot = GetParentPath(Application.dataPath);
			string toRoot = CombinePaths(fromRoot, "_Built", "Stager Studio v" + Application.version);
			CreateFolder(toRoot);
			string[] NAMES = { "Shortcut", "Language", "Projects", "Skins" };
			string msg = "Done: ";
			foreach (var name in NAMES) {
				string from = CombinePaths(fromRoot, name);
				string to = CombinePaths(toRoot, name);
				if (!Directory.Exists(from)) { continue; }
				if (Directory.Exists(to)) { Directory.Delete(to, true); }
				CopyDirectory(from, to, true, false);
				msg += name + ", ";
			}
			Debug.Log(msg);
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


		private static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return new FileInfo(path).FullName;
		}


		private static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !Directory.Exists(path)) {
				string pPath = GetParentPath(path);
				if (!Directory.Exists(pPath)) {
					CreateFolder(pPath);
				}
				Directory.CreateDirectory(path);
			}
		}


		private static bool CopyDirectory (string from, string to, bool copySubDirs, bool ignoreHidden) {

			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(from);

			if (!dir.Exists) {
				return false;
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(to)) {
				Directory.CreateDirectory(to);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				try {
					string temppath = Path.Combine(to, file.Name);
					if (!ignoreHidden || (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
						file.CopyTo(temppath, false);
					}
				} catch { }
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					try {
						string temppath = Path.Combine(to, subdir.Name);
						if (!ignoreHidden || (subdir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
							CopyDirectory(subdir.FullName, temppath, copySubDirs, ignoreHidden);
						}
					} catch { }
				}
			}
			return true;
		}


	}


}