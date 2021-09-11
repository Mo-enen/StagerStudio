namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using UnityEngine;


	public class Project {




		#region --- SUB ---



		private class Chunk {

			// VAR
			private static readonly byte[] EMPTY_BYTES = new byte[0];

			public byte Type { get; private set; } = 0;

			private Dictionary<string, byte[]> Map { get; } = new Dictionary<string, byte[]>();

			// API
			public Chunk (byte type, Dictionary<string, object> objMap) {
				Type = type;
				Map.Clear();
				foreach (var pair in objMap) {
					if (Map.ContainsKey(pair.Key)) { continue; }
					var obj = pair.Value;
					switch (obj) {
						case byte[] o:
							Map.Add(pair.Key, o);
							break;
						case byte o:
							Map.Add(pair.Key, new byte[1] { o });
							break;
						case string o:
							Map.Add(pair.Key, Encoding.Unicode.GetBytes(o));
							break;
						case int o:
							Map.Add(pair.Key, System.BitConverter.GetBytes(o));
							break;
						case bool o:
							Map.Add(pair.Key, new byte[1] { (byte)(o ? 1 : 0) });
							break;
						case short o:
							Map.Add(pair.Key, System.BitConverter.GetBytes(o));
							break;
						case long o:
							Map.Add(pair.Key, System.BitConverter.GetBytes(o));
							break;
						case float o:
							Map.Add(pair.Key, System.BitConverter.GetBytes(o));
							break;
						default:
							break;
					}
				}
			}

			public Chunk (BinaryReader reader) => Read(reader);

			public void Read (BinaryReader reader) {
				Map.Clear();
				Type = reader.ReadByte();
				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++) {
					byte keyLength = reader.ReadByte();
					string key = Encoding.ASCII.GetString(reader.ReadBytes(keyLength));
					int valueLength = reader.ReadInt32();
					var value = reader.ReadBytes(valueLength);
					Map.Add(key, value);
				}
			}

			public void Write (BinaryWriter writer) {
				writer.Write(Type);
				writer.Write(Map.Count);
				foreach (var pair in Map) {
					if (string.IsNullOrEmpty(pair.Key)) { continue; }
					writer.Write((byte)pair.Key.Length);
					writer.Write(Encoding.ASCII.GetBytes(pair.Key));
					writer.Write(pair.Value.Length);
					writer.Write(pair.Value);
				}
			}

			// Get From Map
			public string GetString (string key) => Map.ContainsKey(key) ? Encoding.Unicode.GetString(Map[key]) : "";

			public byte[] GetBytes (string key) => Map.ContainsKey(key) ? Map[key] : EMPTY_BYTES;

			public byte GetByte (string key) {
				if (!Map.ContainsKey(key)) { return 0; }
				var value = Map[key];
				return value == null || value.Length == 0 ? byte.MinValue : value[0];
			}

			public int GetInt (string key) => Map.ContainsKey(key) ? System.BitConverter.ToInt32(Map[key], 0) : 0;

			public long GetLong (string key) => Map.ContainsKey(key) ? System.BitConverter.ToInt64(Map[key], 0) : 0;

			public short GetShort (string key) => Map.ContainsKey(key) ? System.BitConverter.ToInt16(Map[key], 0) : (short)0;

			public bool GetBool (string key) => GetByte(key) == 1;

			public float GetFloat (string key) => Map.ContainsKey(key) ? System.BitConverter.ToSingle(Map[key], 0) : 0f;


		}



		[System.Serializable]
		public class FileData {
			public string Name = "";
			public string Format = "";//eg. ".mp3"
			public byte[] Data = null;
		}



		public class PreviewAudioData {
			public float StartTime = 0f;
			public float Duration = 0f;
			public bool FadeIn = true;
			public bool FadeOut = true;
		}



		public class ImageData {
			public int Width = 0;
			public int Height = 0;
			public bool Alpha = true;
			public byte[] Data = null;
		}



		[System.Serializable]
		public struct TweenArray {

			[System.Serializable]
			public struct TweenData {

				[System.Serializable]
				public struct Frame {
					public float time;
					public float value;
					public float inTangent;
					public float outTangent;
				}

				public Frame[] Frames;
			}
			public List<TweenData> Tweens;


			public TweenArray (List<AnimationCurve> aCurves) {
				Tweens = new List<TweenData>();
				for (int index = 0; index < aCurves.Count; index++) {
					var curve = aCurves[index];
					var tData = new TweenData() {
						Frames = new TweenData.Frame[curve.keys.Length]
					};
					for (int i = 0; i < curve.keys.Length; i++) {
						var frame = curve.keys[i];
						tData.Frames[i] = new TweenData.Frame() {
							time = frame.time,
							value = frame.value,
							inTangent = frame.inTangent,
							outTangent = frame.outTangent,
						};
					}
					Tweens.Add(tData);
				}
			}


			public List<AnimationCurve> GetAnimationTweens () {
				var result = new List<AnimationCurve>();
				foreach (var tween in Tweens) {
					var curve = new AnimationCurve() {
						postWrapMode = WrapMode.Clamp,
						preWrapMode = WrapMode.Clamp,
					};
					for (int i = 0; i < tween.Frames.Length; i++) {
						var frame = tween.Frames[i];
						curve.AddKey(new Keyframe() {
							time = frame.time,
							value = frame.value,
							inTangent = frame.inTangent,
							outTangent = frame.outTangent,
							inWeight = 1f,
							outWeight = 1f,
						});
					}
					result.Add(curve);
				}
				return result;
			}
		}


		[System.Serializable]
		public struct FileDataArray {
			public FileData[] Array;
		}


		public enum ChunkType {
			Info = 0,
			Cover = 1,
			// Preview: 2 Removed
			Music = 3,
			Background = 4,
			Beatmap = 5,
			Palette = 6,
			Tween = 7,
			ClickSound = 8,
			Gene = 9,

		}


		#endregion




		#region --- VAR ---


		// Const
		private const string MAGIC_ID = "STAGER";
		private const short CODE_VERSION = 0;

		// Info
		public string ProjectName { get; set; } = "";
		public string Description { get; set; } = "";
		public string BeatmapAuthor { get; set; } = "";
		public string MusicAuthor { get; set; } = "";
		public string BackgroundAuthor { get; set; } = "";
		public string OpeningBeatmap { get; set; } = "";
		public long LastEditTime { get; set; } = 0;

		// Asset
		public Dictionary<string, Beatmap> BeatmapMap { get; } = new Dictionary<string, Beatmap>();
		public List<Color32> Palette { get; } = new List<Color32>();
		public List<AnimationCurve> Tweens { get; } = new List<AnimationCurve>();
		public List<FileData> ClickSounds { get; } = new List<FileData>();
		public FileData MusicData { get; set; } = null;
		public FileData BackgroundData { get; set; } = null;
		public ImageData FrontCover { get; set; } = null;
		public GeneData Gene { get; set; } = null;

		// Config
		private bool[] TargetData { get; } = new bool[10] { 
			true, true, true, true, true,
			true, true, true, true, true, 
		};


		#endregion




		#region --- API ---


		public bool Read (BinaryReader reader, System.Action<string> messageCallback = null) {
			// Magic ID
			var magic = Encoding.ASCII.GetString(reader.ReadBytes(MAGIC_ID.Length));
			if (magic != MAGIC_ID) {
				messageCallback?.Invoke(string.Format("Wrong magic id. Expecting \"{0}\", get \"{1}\".", MAGIC_ID, magic));
				return false;
			}
			// Clear Old Data
			ProjectName = "";
			Description = "";
			MusicAuthor = "";
			BeatmapAuthor = "";
			BackgroundAuthor = "";
			OpeningBeatmap = "";
			LastEditTime = 0;
			MusicData = null;
			BackgroundData = null;
			BeatmapMap.Clear();
			Gene = null;
			// Get New Data
			short dataVersion = -1;
			int count = reader.ReadInt32();
			for (int i = 0; i < count && i < 1024; i++) {
				try {
					var chunk = new Chunk(reader);
					if (!TargetData[chunk.Type]) { continue; }
					switch ((ChunkType)chunk.Type) {
						default:
							break;
						case ChunkType.Info: {
								ProjectName = chunk.GetString("ProjectName");
								Description = chunk.GetString("Description");
								MusicAuthor = chunk.GetString("MusicAuthor");
								BeatmapAuthor = chunk.GetString("BeatmapAuthor");
								BackgroundAuthor = chunk.GetString("BackgroundAuthor");
								OpeningBeatmap = chunk.GetString("OpeningBeatmap");
								LastEditTime = chunk.GetLong("LastSaveTime");
								dataVersion = chunk.GetShort("DataVersion");
								break;
							}
						case ChunkType.Cover: {
								FrontCover = new ImageData() {
									Width = chunk.GetInt("CoverWidth"),
									Height = chunk.GetInt("CoverHeight"),
									Data = chunk.GetBytes("FrontCover"),
									Alpha = false,
								};
								break;
							}
						case ChunkType.Music: {
								var clipData = new FileData() {
									Format = chunk.GetString("Format"),
									Data = chunk.GetBytes("Data"),
								};
								if (clipData.Data == null || clipData.Data.Length == 0) { break; }
								MusicData = clipData;
								break;
							}
						case ChunkType.Background: {
								var texture = new FileData() {
									Format = chunk.GetString("Format"),
									Data = chunk.GetBytes("Data"),
								};
								if (texture.Data == null || texture.Data.Length == 0) { break; }
								BackgroundData = texture;
								break;
							}
						case ChunkType.Beatmap: {
								var key = chunk.GetString("Key");
								if (BeatmapMap.ContainsKey(key)) { break; }
								var map = JsonUtility.FromJson<Beatmap>(chunk.GetString("Data"));
								if (map != null) {
									map.FixEmpty();
									BeatmapMap.Add(key, map);
								}
								break;
							}
						case ChunkType.Palette: {
								Palette.Clear();
								var data = chunk.GetBytes("Data");
								for (int j = 0; j < data.Length; j += 4) {
									Palette.Add(new Color32(data[j + 0], data[j + 1], data[j + 2], data[j + 3]));
								}
								break;
							}
						case ChunkType.Tween: {
								Tweens.Clear();
								Tweens.AddRange(JsonUtility.FromJson<TweenArray>(chunk.GetString("Data")).GetAnimationTweens());
								break;
							}
						case ChunkType.ClickSound: {
								ClickSounds.Clear();
								ClickSounds.AddRange(((FileDataArray)Util.BytesToObject(chunk.GetBytes("Data"))).Array);
								break;
							}
						case ChunkType.Gene: {
								Gene = JsonUtility.FromJson<GeneData>(chunk.GetString("Gene"));
								break;
							}
					}
				} catch (System.Exception ex) {
					messageCallback?.Invoke("Error on read chunk.\n" + ex.Message);
				}
			}
			FixEmptyValues();
			return true;
		}


		public void Write (BinaryWriter writer, System.Action<string> messageCallback = null, System.Action<float> progressCallback = null) {
			FixEmptyValues();
			// Get Chunks
			var chunks = new List<Chunk>();
			// Info
			progressCallback?.Invoke(0.01f);
			try {
				chunks.Add(new Chunk((byte)ChunkType.Info, new Dictionary<string, object>() {
					{ "ProjectName", ProjectName },
					{ "Description", Description },
					{ "DataVersion", CODE_VERSION },
					{ "BeatmapAuthor", BeatmapAuthor },
					{ "MusicAuthor", MusicAuthor },
					{ "BackgroundAuthor", BackgroundAuthor },
					{ "OpeningBeatmap", OpeningBeatmap },
					{ "LastSaveTime", LastEditTime },
				}));
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get info.\n" + ex.Message);
			}
			// Cover
			try {
				chunks.Add(new Chunk((byte)ChunkType.Cover, new Dictionary<string, object>() {
					{ "CoverWidth", FrontCover.Width },
					{ "CoverHeight", FrontCover.Height },
					{ "FrontCover", FrontCover.Data },
				}));
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get cover.\n" + ex.Message);
			}
			// Music
			progressCallback?.Invoke(0.1f);
			try {
				if (MusicData != null && MusicData.Data != null && MusicData.Data.Length > 0) {
					chunks.Add(new Chunk((byte)ChunkType.Music, new Dictionary<string, object>() {
						{ "Format", MusicData.Format},
						{ "Data", MusicData.Data },
					}));
				}
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get music.\n" + ex.Message);
			}

			// Background
			progressCallback?.Invoke(0.2f);
			try {
				if (BackgroundData != null && BackgroundData.Data != null && BackgroundData.Data.Length > 0) {
					chunks.Add(new Chunk((byte)ChunkType.Background, new Dictionary<string, object>() {
						{ "Format", BackgroundData.Format},
						{ "Data", BackgroundData.Data },
					}));
				}
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get image.\n" + ex.Message);
			}

			// Beatmap
			progressCallback?.Invoke(0.3f);
			foreach (var pair in BeatmapMap) {
				try {
					if (pair.Value == null) { continue; }
					chunks.Add(new Chunk((byte)ChunkType.Beatmap, new Dictionary<string, object>() {
						{ "Key", pair.Key },
						{ "Data", JsonUtility.ToJson(pair.Value)},
					}));
				} catch (System.Exception ex) {
					messageCallback?.Invoke("Error on get beatmap.\n" + ex.Message);
				}
			}

			// Palette
			progressCallback?.Invoke(0.4f);
			try {
				var palBytes = new List<byte>();
				foreach (var color in Palette) {
					palBytes.Add(color.r);
					palBytes.Add(color.g);
					palBytes.Add(color.b);
					palBytes.Add(color.a);
				}
				chunks.Add(new Chunk((byte)ChunkType.Palette, new Dictionary<string, object>() {
					{ "Data", palBytes.ToArray() },
				}));
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get Palette.\n" + ex.Message);
			}

			// Tween
			try {
				chunks.Add(new Chunk((byte)ChunkType.Tween, new Dictionary<string, object>() {
					{ "Data", JsonUtility.ToJson(new TweenArray(Tweens), false) },
				}));
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get Tween.\n" + ex.Message);
			}

			// Click Sound
			try {
				chunks.Add(new Chunk((byte)ChunkType.ClickSound, new Dictionary<string, object>() {
					{ "Data", Util.ObjectToBytes(new FileDataArray(){  Array = ClickSounds.ToArray() }) },
				}));
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get click sound.\n" + ex.Message);
			}

			// Gene
			try {
				chunks.Add(new Chunk((byte)ChunkType.Gene, new Dictionary<string, object>() {
					{"Gene", JsonUtility.ToJson(Gene, false) }
				}));
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on get gene.\n" + ex.Message);
			}

			// Write Chunks
			progressCallback?.Invoke(0.5f);
			try {
				writer.Write(Encoding.ASCII.GetBytes(MAGIC_ID));
				writer.Write(chunks.Count);
			} catch (System.Exception ex) {
				messageCallback?.Invoke("Error on write magicID or chunk count.\n" + ex.Message);
			}
			float chunkIndex = 0f;
			foreach (var chunk in chunks) {
				try {
					chunk.Write(writer);
					progressCallback?.Invoke(0.5f + 0.5f * chunkIndex / chunks.Count);
				} catch (System.Exception ex) {
					messageCallback?.Invoke("Error on write chunk.\nType = " + (chunk != null ? ((ChunkType)chunk.Type).ToString() : "(null)") + "\n" + ex.Message);
				}
				chunkIndex++;
			}

		}


		public void SetTarget (params ChunkType[] targetTypes) {
			for (int i = 0; i < TargetData.Length; i++) {
				TargetData[i] = false;
			}
			foreach (var type in targetTypes) {
				TargetData[(int)type] = true;
			}
		}


		public static implicit operator bool (Project p) => p != null;


		// Audio
		public static FileData AudioBytes_to_AudioData (string name, string format, byte[] bytes) => bytes != null && bytes.Length > 0 ? new Project.FileData() {
			Name = name,
			Format = format,
			Data = bytes,
		} : null;


		// Image
		public static Sprite ImageData_to_Sprite (ImageData data) {
			if (data == null) {
				Debug.Log("Data is null");
				return null;
			}
			try {
				var colors = ImageData_to_Colors(data);
				return Colors_to_Sprite(colors, data.Width, data.Height);
			} catch { }
			return null;
		}


		public static Color[] ImageData_to_Colors (ImageData data) {
			var bytes = data.Data;
			int width = data.Width;
			int height = data.Height;
			bool alpha = data.Alpha;
			int alphaLen = (alpha ? 4 : 3);
			if (bytes == null || bytes.Length == 0 || width <= 0 || height <= 0) {
				return new Color[0];
			}
			if (bytes == null || bytes.Length != width * height * alphaLen) {
				return new Color[0];
			}
			var colors = new Color[width * height];
			for (int i = 0; i < colors.Length; i++) {
				colors[i] = new Color(
					(float)bytes[i * alphaLen + 0] / byte.MaxValue,
					(float)bytes[i * alphaLen + 1] / byte.MaxValue,
					(float)bytes[i * alphaLen + 2] / byte.MaxValue,
					alpha ? (float)bytes[i * alphaLen + 3] / byte.MaxValue : 1f
				);
			}
			return colors;
		}


		public static Sprite Colors_to_Sprite (Color[] colors, int width, int height) {
			if (colors.Length == 0 || colors.Length != width * height || width <= 0 || height <= 0) {
				return null;
			}
			var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			texture.SetPixels(colors);
			texture.Apply();
			var sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
			return sp;
		}


		public static ImageData Texture_to_ImageData (Texture2D texture, int width, int height, bool alpha) => texture != null ? new Project.ImageData() {
			Width = width,
			Height = height,
			Alpha = alpha,
			Data = Texture_to_Bytes(texture, width, height, alpha),
		} : null;


		public static FileData Texture_to_FileData (Texture2D texture, string format) => texture != null ? new Project.FileData() {
			Format = format,
			Data = texture.EncodeToPNG(),
		} : null;


		#endregion



		#region --- LGC ---


		private void FixEmptyValues () {
			if (string.IsNullOrEmpty(ProjectName)) {
				ProjectName = "";
			}
			if (string.IsNullOrEmpty(Description)) {
				Description = "";
			}
			if (FrontCover is null) {
				FrontCover = new ImageData();
			}
			if (Palette.Count == 0) {
				Palette.Add(Color.white);
			}
			if (Tweens.Count == 0) {
				Tweens.Add(AnimationCurve.Linear(0f, 0f, 1f, 1f));
			}
		}


		private static byte[] Texture_to_Bytes (Texture2D texture, int width, int height, bool alpha) {
			if (!texture || texture.width == 0 || texture.height == 0 || width <= 0 || height <= 0) { return null; }
			int alphaLen = alpha ? 4 : 3;
			var bytes = new byte[width * height * alphaLen];
			if (width == texture.width && height == texture.height) {
				var colors = texture.GetPixels();
				for (int i = 0; i < colors.Length; i++) {
					var c = colors[i];
					bytes[i * alphaLen + 0] = (byte)(Mathf.Clamp01(c.r) * byte.MaxValue);
					bytes[i * alphaLen + 1] = (byte)(Mathf.Clamp01(c.g) * byte.MaxValue);
					bytes[i * alphaLen + 2] = (byte)(Mathf.Clamp01(c.b) * byte.MaxValue);
					if (alpha) {
						bytes[i * alphaLen + 3] = (byte)(Mathf.Clamp01(c.a) * byte.MaxValue);
					}
				}
			} else {
				int i = 0;
				for (float y = 0; y < height; y++) {
					for (float x = 0; x < width; x++) {
						var c = texture.GetPixelBilinear(x / width, y / height);
						bytes[i * alphaLen + 0] = (byte)(Mathf.Clamp01(c.r) * byte.MaxValue);
						bytes[i * alphaLen + 1] = (byte)(Mathf.Clamp01(c.g) * byte.MaxValue);
						bytes[i * alphaLen + 2] = (byte)(Mathf.Clamp01(c.b) * byte.MaxValue);
						if (alpha) {
							bytes[i * alphaLen + 3] = (byte)(Mathf.Clamp01(c.a) * byte.MaxValue);
						}
						i++;
					}
				}
			}
			return bytes;
		}


		#endregion



	}
}