namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;



	[System.Serializable]
	public enum SkinType {

		Stage = 0,
		JudgeLine = 1,

		Track = 2,
		TrackTint = 3,
		Tray = 4,

		TapNote = 5,
		HoldNote = 6,
		SwipeArrow = 7,
		SlideNote = 8,
		LinkPole = 9,

		NoteLuminous = 10,
		HoldLuminous = 11,

	}


	[System.Serializable]
	public enum SkinLoopType {
		Forward = 0,
		Loop = 1,
		PingPong = 2,

	}


	[System.Serializable]
	public class SkinData {


		// API
		public Texture2D Texture { get; set; } = null;
		public float NoteShadowDistance_UI {
			get => NoteShadowDistance * 100f;
			set => NoteShadowDistance = Mathf.Clamp01(value / 100f);
		}
		public float LuminousAppendX_UI {
			get => LuminousAppendX * 100f;
			set => LuminousAppendX = Mathf.Clamp01(value / 100f);
		}
		public float LuminousAppendY_UI {
			get => LuminousAppendY * 100f;
			set => LuminousAppendY = Mathf.Clamp01(value / 100f);
		}
		public float ScaleMuti_UI {
			get => ScaleMuti;
			set => ScaleMuti = Mathf.Max(value, 1f);
		}
		public int VanishDuration_UI {
			get => (int)(VanishDuration * 1000f);
			set => VanishDuration = Mathf.Max(value / 1000f, 0f);
		}

		// Ser
		public string Author = "";
		public float ScaleMuti = 2000f;
		public float NoteShadowDistance = 0f;
		public float LuminousAppendX = 0f;
		public float LuminousAppendY = 0f;
		public float VanishDuration = 0.1f;
		public bool FixedNoteWidth = false;
		public List<AnimatedItemData> Items = new List<AnimatedItemData>();

		// API
		public static SkinData ByteToSkin (byte[] bytes) {
			if (bytes is null || bytes.Length <= 4) { return null; }
			try {
				SkinData skin = null;
				// Json Len
				int index = 0;
				int jsonLength = System.BitConverter.ToInt32(bytes, index);
				index += 4;
				// Json
				var json = System.Text.Encoding.UTF8.GetString(bytes, index, jsonLength);
				index += jsonLength;
				skin = JsonUtility.FromJson<SkinData>(json);
				if (skin is null) {
					skin = new SkinData();
				}
				skin.Fillup();
				// PNG Len
				int pngLength = System.BitConverter.ToInt32(bytes, index);
				index += 4;
				// PNG
				skin.Texture = null;
				if (pngLength > 0) {
					var (pixels32, width, height) = Util.ImageToPixels(bytes.Skip(index).Take(pngLength).ToArray());
					if (!(pixels32 is null) && pixels32.Length > 0 && width * height == pixels32.Length) {
						skin.Texture = new Texture2D(width, height, TextureFormat.ARGB32, false) {
							filterMode = FilterMode.Point,
							alphaIsTransparency = true,
						};
						skin.Texture.SetPixels32(pixels32);
						skin.Texture.Apply();
					}
				}
				return skin;
			} catch {
				return null;
			}
		}


		public static byte[] SkinToByte (SkinData skin) {
			var list = new List<byte>();
			if (skin is null) { return null; }
			skin.Fillup();
			try {
				// Json
				var json = JsonUtility.ToJson(skin, true);
				if (string.IsNullOrEmpty(json)) { return null; }
				var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
				list.AddRange(System.BitConverter.GetBytes(jsonBytes.Length));
				list.AddRange(jsonBytes);
				// PNG
				if (!(skin.Texture is null) && skin.Texture.width > 0 && skin.Texture.height > 0) {
					var pngBytes = skin.Texture.EncodeToPNG();
					if (pngBytes is null || pngBytes.Length == 0) {
						list.AddRange(System.BitConverter.GetBytes(0));
					} else {
						list.AddRange(System.BitConverter.GetBytes(pngBytes.Length));
						list.AddRange(pngBytes);
					}
				} else {
					list.AddRange(System.BitConverter.GetBytes(0));
				}
			} catch {
				return null;
			}
			return list.ToArray();
		}


		public void Fillup () {
			if (Items is null) { Items = new List<AnimatedItemData>(); }
			int typeCount = System.Enum.GetNames(typeof(SkinType)).Length;
			// Fill
			while (Items.Count < typeCount) {
				Items.Add(new AnimatedItemData() {
					FrameDuration = 120,
					Loop = SkinLoopType.Loop,
					Rects = new List<AnimatedItemData.RectData>(),
				});
			}
			// Fix
			for (int i = 0; i < Items.Count; i++) {
				var item = Items[i];
				if (item is null) {
					item = new AnimatedItemData();
				}
				if (item.Rects is null) {
					item.Rects = new List<AnimatedItemData.RectData>();
				}
				Items[i] = item;
			}
		}


		public Vector2 TryGetItemSize (int index) {
			if (Items is null || index < 0 || index >= Items.Count) { return default; }
			var item = Items[index];
			if (item.Rects is null || item.Rects.Count == 0) { return default; }
			var rect = item.Rects[0];
			return new Vector2(rect.Width, rect.Height);
		}


		public Vector4 TryGetItemBorder (int index) {
			if (Items is null || index < 0 || index >= Items.Count) { return default; }
			var item = Items[index];
			if (item.Rects is null || item.Rects.Count == 0) { return default; }
			var rect = item.Rects[0];
			return new Vector4(rect.BorderL, rect.BorderR, rect.BorderD, rect.BorderU);
		}


	}


	[System.Serializable]
	public class AnimatedItemData {


		// SUB
		[System.Serializable]
		public struct RectData {

			public int R => X + Width;
			public int U => Y + Height;
			public int L => X;
			public int D => Y;

			public int X;
			public int Y;
			public int Width;
			public int Height;

			public int BorderU;
			public int BorderD;
			public int BorderL;
			public int BorderR;

			public static RectData MinMax (int xMin, int xMax, int yMin, int yMax) => new RectData(xMin, yMin, xMax - xMin, yMax - yMin);

			public RectData (int x, int y, int width, int height, int borderU = 0, int borderD = 0, int borderL = 0, int borderR = 0) {
				X = x;
				Y = y;
				Width = width;
				Height = height;
				BorderU = borderU;
				BorderD = borderD;
				BorderL = borderL;
				BorderR = borderR;
			}

		}

		// API
		public float TotalDuration => FrameDuration / 1000f * Rects.Count;
		public int Thickness3D_UI {
			get => Mathf.RoundToInt(Thickness3D * 1000f);
			set => Thickness3D = value / 1000f;
		}

		// Ser
		public SkinLoopType Loop = SkinLoopType.Forward;
		public List<RectData> Rects = new List<RectData>();
		public int FrameDuration = 200;
		public float Thickness3D = 0f;


		// API
		public void SetDuration (int durationMS) => FrameDuration = Mathf.Max(durationMS, 1);


		public int GetFrame (float lifeTime) {
			int count = Rects.Count;
			float spf = FrameDuration / 1000f;
			if (count <= 1 || FrameDuration == 0) { return 0; }
			switch (Loop) {
				default:
				case SkinLoopType.Forward:
					return Mathf.Clamp(Mathf.FloorToInt(
						Mathf.Clamp(lifeTime, 0f, TotalDuration) / spf
					), 0, count - 1);
				case SkinLoopType.Loop:
					return Mathf.Clamp(Mathf.FloorToInt(
						Mathf.Repeat(lifeTime, TotalDuration) / spf
					), 0, count - 1);
				case SkinLoopType.PingPong:
					return Mathf.Clamp(Mathf.FloorToInt(
						Mathf.PingPong(lifeTime, TotalDuration) / spf
					), 0, count - 1);
			}
		}


	}



}