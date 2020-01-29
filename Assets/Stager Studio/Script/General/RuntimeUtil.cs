namespace StagerStudio {
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
			using (var ms = new MemoryStream()) {
				new BinaryFormatter().Serialize(ms, obj);
				return ms.ToArray();
			}
		}


		public static object BytesToObject (byte[] bytes) {
			if (bytes == null || bytes.Length == 0) { return null; }
			using (var memStream = new MemoryStream()) {
				memStream.Write(bytes, 0, bytes.Length);
				memStream.Seek(0, SeekOrigin.Begin);
				var obj = new BinaryFormatter().Deserialize(memStream);
				return obj;
			}
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


		private static System.Drawing.Bitmap ResizeImage (System.Drawing.Image image, int width, int height) {
			if (image is null) { return null; }
			var destRect = new System.Drawing.Rectangle(0, 0, width, height);
			var destImage = new System.Drawing.Bitmap(width, height);
			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			using (var graphics = System.Drawing.Graphics.FromImage(destImage)) {
				graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				using (var wrapMode = new System.Drawing.Imaging.ImageAttributes()) {
					wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
				}
			}
			return destImage;
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




		#region --- Misc ---


		public static bool IsTypeing {
			get {
				var g = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
				if (g) {
					var input = g.GetComponent<UnityEngine.UI.InputField>();
					return input && input.isFocused;
				} else {
					return false;
				}
			}
		}


		public static Vector3 Vector3Lerp3 (Vector3 a, Vector3 b, float x, float y, float z = 0f) => new Vector3(
			Mathf.LerpUnclamped(a.x, b.x, x),
			Mathf.LerpUnclamped(a.y, b.y, y),
			Mathf.LerpUnclamped(a.z, b.z, z)
		);


		public static float Remap (float l, float r, float newL, float newR, float t) {
			return l == r ? 0 : Mathf.LerpUnclamped(
				newL, newR,
				(t - l) / (r - l)
			);
		}


		public static Texture2D TrimTexture (Texture2D texture, float alpha = 0.01f, int gap = 0) {
			int width = texture.width;
			int height = texture.height;
			var colors = texture.GetPixels();
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					var c = colors[y * width + x];
					if (c.a > alpha) {
						minX = Mathf.Min(minX, x);
						minY = Mathf.Min(minY, y);
						maxX = Mathf.Max(maxX, x);
						maxY = Mathf.Max(maxY, y);
					}
				}
			}

			// Gap
			minX = Mathf.Clamp(minX - gap, 0, width - 1);
			minY = Mathf.Clamp(minY - gap, 0, height - 1);
			maxX = Mathf.Clamp(maxX + gap, 0, width - 1);
			maxY = Mathf.Clamp(maxY + gap, 0, height - 1);

			int newWidth = maxX - minX + 1;
			int newHeight = maxY - minY + 1;
			if (newWidth != width || newHeight != height) {
				texture.Resize(newWidth, newHeight);
				var newColors = new Color[newWidth * newHeight];
				for (int y = 0; y < newHeight; y++) {
					for (int x = 0; x < newWidth; x++) {
						newColors[y * newWidth + x] = colors[(y + minY) * width + (x + minX)];
					}
				}
				texture.SetPixels(newColors);
				texture.Apply();
			}
			return texture;
		}


		public static bool GetBit (int value, int index) {
			if (index < 0 || index > 31) { return false; }
			var val = 1 << index;
			return (value & val) == val;
		}


		public static int SetBitValue (int value, int index, bool bitValue) {
			if (index < 0 || index > 31) { return value; }
			var val = 1 << index;
			return bitValue ? (value | val) : (value & ~val);
		}


		public static void ShowInExplorer (string path) => System.Diagnostics.Process.Start("Explorer.exe", GetFullPath(path));


		public static void ClampRectTransform (RectTransform target) {
			target.anchoredPosition = VectorClamp2(
				target.anchoredPosition,
				target.pivot * target.rect.size - target.anchorMin * ((RectTransform)target.parent).rect.size,
				(Vector2.one - target.anchorMin) * ((RectTransform)target.parent).rect.size - (Vector2.one - target.pivot) * target.rect.size
			);
			Vector2 VectorClamp2 (Vector2 v, Vector2 min, Vector2 max) => new Vector2(
				Mathf.Clamp(v.x, min.x, max.x),
				Mathf.Clamp(v.y, min.y, max.y)
			);
		}


		public static Vector2 GetDisc01 (Vector2 pos01, float disc, float width, float height, float offsetY) {
			// Disc
			const float THE_ANGLE = 180f / Mathf.PI;
			pos01.x = Mathf.Repeat(Mathf.Lerp(0.5f, Mathf.LerpUnclamped(0.5f, pos01.x, width), disc / 360f), 1f);
			pos01.y = Mathf.Clamp(
				offsetY + pos01.y * height,
				0f,
				THE_ANGLE / Mathf.Clamp(disc, Mathf.Epsilon, THE_ANGLE)
			);
			var result = new Vector2(
				Mathf.LerpUnclamped(
					0.5f * Mathf.Sin((pos01.x + 0.5f) * 360f * Mathf.Deg2Rad) + 0.5f,
					0.5f,
					pos01.y * (disc < THE_ANGLE ? disc / THE_ANGLE : 1f)
				),
				Mathf.LerpUnclamped(
					0.5f * Mathf.Cos(pos01.x * 360f * Mathf.Deg2Rad),
					0f,
					pos01.y * (disc < THE_ANGLE ? disc / THE_ANGLE : 1f)
				)
			);
			// Scale
			if (disc < 180f) {
				result.x = (result.x - 0.5f) / Mathf.Sin(disc * 0.5f * Mathf.Deg2Rad) + 0.5f;
				result.y = (result.y + 0.5f) / Mathf.Sin(disc * 0.5f * Mathf.Deg2Rad) - 0.5f;
			}
			// Shift
			result.y += 0.5f;
			return result;
		}


		public static T SpawnUI<T> (T prefab, RectTransform root, string name = "") where T : MonoBehaviour {
			root.gameObject.SetActive(true);
			root.parent.gameObject.SetActive(true);
			var obj = UnityEngine.Object.Instantiate(prefab, root);
			var rt = obj.transform as RectTransform;
			rt.name = name;
			rt.SetAsLastSibling();
			rt.localRotation = Quaternion.identity;
			rt.localScale = Vector3.one;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = rt.offsetMax = Vector2.zero;
			return obj;
		}


		public static bool PointInTriangle (float px, float py, float p0x, float p0y, float p1x, float p1y, float p2x, float p2y) {
			var s = p0y * p2x - p0x * p2y + (p2y - p0y) * px + (p0x - p2x) * py;
			var t = p0x * p1y - p0y * p1x + (p0y - p1y) * px + (p1x - p0x) * py;
			if ((s < 0) != (t < 0)) { return false; }
			var A = -p1y * p2x + p0y * (p2x - p1x) + p0x * (p1y - p2y) + p1x * p2y;
			return A < 0 ? (s <= 0 && s + t >= A) : (s >= 0 && s + t <= A);
		}


		public static bool PointInTriangle (Vector2 p, Vector2 a, Vector2 b, Vector2 c) => PointInTriangle(p.x, p.y, a.x, a.y, b.x, b.y, c.x, c.y);


		public static Canvas GetCanvas (RectTransform rt) {
			var list = new List<Canvas>();
			rt.gameObject.GetComponentsInParent(false, list);
			if (list.Count > 0) {
				for (int i = 0; i < list.Count; ++i) {
					if (list[i].isActiveAndEnabled) {
						return list[i];
					}
				}
			}
			return null;
		}


		#endregion




	}
}