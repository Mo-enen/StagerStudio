namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class ProjectUtil {


		// Const
		public const float FADE_DURATION = 2.4f;


		// Audio
		public static Project.FileData AudioBytes_to_AudioData (string name, string format, byte[] bytes) => bytes != null && bytes.Length > 0 ? new Project.FileData() {
			Name = name,
			Format = format,
			Data = bytes,
		} : null;


		// Image
		public static Sprite ImageData_to_Sprite (Project.ImageData data) {
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


		public static Color[] ImageData_to_Colors (Project.ImageData data) {
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


		public static Project.ImageData Texture_to_ImageData (Texture2D texture, int width, int height, bool alpha) => texture != null ? new Project.ImageData() {
			Width = width,
			Height = height,
			Alpha = alpha,
			Data = Texture_to_Bytes(texture, width, height, alpha),
		} : null;


		public static Project.FileData Texture_to_FileData (Texture2D texture, string format) => texture != null ? new Project.FileData() {
			Format = format,
			Data = texture.EncodeToPNG(),
		} : null;


		// Logic
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




	}
}