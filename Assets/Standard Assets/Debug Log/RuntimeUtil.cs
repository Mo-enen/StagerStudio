namespace DebugLog {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Xml.Serialization;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;


	public struct Util {






		#region --- File ---


		public static string FileToText (string path) {
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadToEnd();
			sr.Close();
			return data;
		}


		public static void TextToFile (string data, string path) {
			FileStream fs = new FileStream(path, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}


		public static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !DirectoryExists(path)) {
				string pPath = GetParentPath(path);
				if (!DirectoryExists(pPath)) {
					CreateFolder(pPath);
				}
				Directory.CreateDirectory(path);
			}
		}


		public static byte[] FileToByte (string path) {
			byte[] bytes = null;
			if (FileExists(path)) {
				bytes = File.ReadAllBytes(path);
			}
			return bytes;
		}


		public static void ByteToFile (byte[] bytes, string path) {
			string parentPath = GetParentPath(path);
			CreateFolder(parentPath);
			FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}


		public static byte[] ObjectToBytes (object obj) {
			if (obj == null) { return new byte[0]; }
			try {
				using (var ms = new MemoryStream()) {
					new BinaryFormatter().Serialize(ms, obj);
					return ms.ToArray();
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex);
			}
			return new byte[0];
		}


		public static int ObjectToBytes (object obj, byte[] buffer, int offset) {
			if (obj == null) { return offset; }
			int len = 0;
			int realLen = 0;
			try {
				using (var ms = new MemoryStream()) {
					new BinaryFormatter().Serialize(ms, obj);
					ms.Seek(0, SeekOrigin.Begin);
					len = (int)ms.Length;
					if (offset + len < buffer.Length) {
						realLen = ms.Read(buffer, offset, len);
						if (realLen == 0) {
							realLen = len;
						}
					}
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex);
			}
			return len == realLen ? offset + len : offset;
		}


		public static object BytesToObject (byte[] bytes) {
			if (bytes == null || bytes.Length == 0) { return null; }
			try {
				using (var memStream = new MemoryStream()) {
					memStream.Write(bytes, 0, bytes.Length);
					memStream.Seek(0, SeekOrigin.Begin);
					var obj = new BinaryFormatter().Deserialize(memStream);
					return obj;
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex);
			}
			return null;
		}


		public static object BytesToObject (byte[] bytes, int offset, int len) {
			if (bytes == null || bytes.Length == 0) { return null; }
			try {
				using (var memStream = new MemoryStream()) {
					memStream.Write(bytes, offset, len);
					memStream.Seek(0, SeekOrigin.Begin);
					var obj = new BinaryFormatter().Deserialize(memStream);
					return obj;
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex);
			}
			return null;
		}


		public static bool HasFileIn (string path, params string[] searchPattern) {
			if (PathIsDirectory(path)) {
				for (int i = 0; i < searchPattern.Length; i++) {
					if (new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories).Length > 0) {
						return true;
					}
				}
			}
			return false;
		}


		public static FileInfo[] GetFilesIn (string path, bool topOnly, params string[] searchPattern) {
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


		public static DirectoryInfo[] GetDirectsIn (string path, bool topOnly) {
			var allDirs = new List<DirectoryInfo>();
			if (PathIsDirectory(path)) {
				allDirs.AddRange(new DirectoryInfo(path).GetDirectories("*", topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories));
			}
			return allDirs.ToArray();
		}


		public static void DeleteFile (string path) {
			if (FileExists(path)) {
				File.Delete(path);
			}
		}


		public static void CopyFile (string from, string to) {
			if (FileExists(from)) {
				File.Copy(from, to, true);
			}
		}


		public static bool CopyDirectory (string from, string to, bool copySubDirs, bool ignoreHidden) {

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


		public static void DeleteDirectory (string path) {
			if (DirectoryExists(path)) {
				Directory.Delete(path, true);
			}
		}


		public static void DeleteAllFilesIn (string path) {
			if (DirectoryExists(path)) {
				var files = GetFilesIn(path, false, "*");
				foreach (var file in files) {
					DeleteFile(file.FullName);
				}
			}
		}


		public static float GetFileSizeInMB (string path) {
			float size = -1f;
			if (FileExists(path)) {
				size = (new FileInfo(path).Length / 1024f) / 1024f;
			}
			return size;
		}


		public static T ReadXML<T> (string path) where T : class {
			var serializer = new XmlSerializer(typeof(T));
			var stream = new FileStream(path, FileMode.Open);
			var container = serializer.Deserialize(stream) as T;
			stream.Close();
			return container;
		}


		public static void WriteXML<T> (T data, string path) where T : class {
			var serializer = new XmlSerializer(typeof(T));
			var stream = new FileStream(path, FileMode.Create);
			serializer.Serialize(stream, data);
			stream.Close();
		}


		public static int GetFileCount (string path, string search = "", SearchOption option = SearchOption.TopDirectoryOnly) {
			if (DirectoryExists(path)) {
				return Directory.EnumerateFiles(path, search, option).Count();
			}
			return 0;
		}


		public static void MoveFile (string from, string to) {
			if (from != to && FileExists(from)) {
				File.Move(from, to);
			}
		}


		public static bool MoveDirectory (string from, string to) {
			if (from != to && DirectoryExists(from)) {
				try {
					Directory.Move(from, to);
					return true;
				} catch { }
			}
			return false;
		}


		public static (Color32[] pixels, int width, int height) ImageToPixels (string path, bool ignoreClear = false) {
			try {
				return ImageToPixels(System.Drawing.Image.FromFile(path), ignoreClear);
			} catch {
				return (null, 0, 0);
			}
		}


		public static (Color32[] pixels, int width, int height) ImageToPixels (byte[] bytes, bool ignoreClear = false) {
			try {
				using (var image = System.Drawing.Image.FromStream(new MemoryStream(bytes))) {
					return ImageToPixels(image, ignoreClear);
				}
			} catch {
				return (null, 0, 0);
			}
		}


		private static (Color32[] pixels, int width, int height) ImageToPixels (System.Drawing.Image image, bool ignoreClear) {
			try {
				if (image is null) { return (null, 0, 0); }
				var colors = new List<Color32>();
				using (var map = new System.Drawing.Bitmap(image)) {
					int width = map.Width;
					int height = map.Height;
					for (int y = height - 1; y >= 0; y--) {
						for (int x = 0; x < width; x++) {
							var c = map.GetPixel(x, y);
							if (ignoreClear && c.A == 0) { continue; }
							colors.Add(new Color32(c.R, c.G, c.B, c.A));
						}
					}
					return (colors.ToArray(), width, height);
				}
			} catch {
				return (null, 0, 0);
			}
		}


		public static Texture2D ResizeTexture (Texture2D texture, int width, int height) {
			if (!texture) { return null; }
			var newTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			var pixels = new Color[width * height];
			int i = 0;
			for (float y = 0; y < height; y++) {
				for (float x = 0; x < width; x++) {
					pixels[i] = texture.GetPixelBilinear(x / width, y / height);
					i++;
				}
			}
			newTexture.SetPixels(pixels);
			newTexture.Apply();
			return newTexture;
		}


		public static Texture2D ResizeTexture (Texture2D texture, int maxPixelSize) {
			float width = texture.width;
			float height = texture.height;
			bool trimed = false;
			if (height > maxPixelSize) {
				width *= maxPixelSize / height;
				height = maxPixelSize;
				trimed = true;
			}
			if (width > maxPixelSize) {
				height *= maxPixelSize / width;
				width = maxPixelSize;
				trimed = true;
			}
			if (trimed) {
				return ResizeTexture(texture, (int)width, (int)height);
			} else {
				return texture;
			}
		}


		#endregion




		#region --- Path ---


		public static string GetParentPath (string path) => Directory.GetParent(path).FullName;


		public static string GetFullPath (string path) => new FileInfo(path).FullName;


		public static string GetDirectoryFullPath (string path) => new DirectoryInfo(path).FullName;


		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return GetFullPath(path);
		}


		public static string GetExtension (string path) => Path.GetExtension(path);//.txt


		public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);


		public static string GetNameWithExtension (string path) => Path.GetFileName(path);


		public static string ChangeExtension (string path, string newEx) => Path.ChangeExtension(path, newEx);


		public static bool DirectoryExists (string path) => Directory.Exists(path);


		public static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);


		public static bool PathIsDirectory (string path) {
			if (!DirectoryExists(path)) { return false; }
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}


		public static bool IsChildPath (string pathA, string pathB) {
			pathA = GetFullPath(pathA);
			pathB = GetFullPath(pathB);
			if (pathA.Length == pathB.Length) {
				return pathA == pathB;
			} else if (pathA.Length > pathB.Length) {
				return IsChildPathCompair(pathA, pathB);
			} else {
				return IsChildPathCompair(pathB, pathA);
			}
		}


		private static bool IsChildPathCompair (string longPath, string path) {
			if (longPath.Length <= path.Length || !PathIsDirectory(path) || !longPath.StartsWith(path)) {
				return false;
			}
			char c = longPath[path.Length];
			if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) {
				return false;
			}
			return true;
		}


		public static string GetUrl (string path) => string.IsNullOrEmpty(path) ? "" : new System.Uri(path).AbsoluteUri;


		public static AudioType GetAudioType (string path) {
			var ex = GetExtension(path);
			switch (ex) {
				default:
					return AudioType.UNKNOWN;
				case ".mp3":
					return AudioType.MPEG;
				case ".ogg":
					return AudioType.OGGVORBIS;
				case ".wav":
					return AudioType.WAV;
			}
		}


		public static string GetTimeString () => System.DateTime.Now.ToString("yyyyMMddHHmmssffff");


		public static long GetLongTime () => System.DateTime.Now.Ticks;


		public static string GetDisplayTimeFromTicks (long ticks) => new System.DateTime(ticks).ToString("yyyy-MM-dd HH:mm");


		#endregion




	}
}