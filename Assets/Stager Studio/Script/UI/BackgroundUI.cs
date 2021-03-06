﻿namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Saving;


	public class BackgroundUI : MonoBehaviour {



		// Handler
		public delegate void ExceptionHandler (System.Exception ex);
		public static ExceptionHandler OnException { get; set; } = null;

		// API
		public float Brightness { get; private set; } = 0.309f;
		public int DefaultIndex => BackgroundIndex.Value;

		// Short
		private Image IMG => _IMG != null ? _IMG : (_IMG = GetComponent<Image>());
		private AspectRatioFitter Fitter => _Fitter != null ? _Fitter : (_Fitter = GetComponent<AspectRatioFitter>());

		// Ser
		[SerializeField] private Sprite[] m_DefaultBGs = null;
		[SerializeField] private Material m_BlurMat = null;

		// Data
		private Coroutine SwipingCor = null;
		private Image _IMG = null;
		private AspectRatioFitter _Fitter = null;
		private bool UsingDefault = true;

		// Saving
		private SavingInt BackgroundIndex = new SavingInt("BackgroundUI.BackgroundIndex", 0);


		// API
		public void SetBackground (Sprite sprite, bool animation = true) {
			try {
				SetBackgroundLogic(sprite, animation);
			} catch (System.Exception ex) { OnException(ex); }
		}


		public void SetBrightness (float bright01) {
			bright01 = Mathf.Clamp01(bright01);
			Brightness = bright01;
			IMG.color = new Color(bright01, bright01, bright01, 1f);
		}


		public void SetDefaultIndex (int index) {
			BackgroundIndex.Value = Mathf.Clamp(index, -1, m_DefaultBGs.Length - 1);
			if (UsingDefault) {
				SetBackgroundLogic(null, true);
			}
		}


		// LGC
		private void SetBackgroundLogic (Sprite sprite, bool animation) {
			const float SCALE_MUTI = 1.06f;
			// Blur Sprite
			if (!sprite || !sprite.texture) {
				int index = BackgroundIndex.Value < 0 ?
					(int)Random.Range(0f, m_DefaultBGs.Length - 0.0001f) :
					Mathf.Clamp(BackgroundIndex.Value, 0, m_DefaultBGs.Length - 1);
				sprite = m_DefaultBGs[index];
				UsingDefault = true;
			} else {
				sprite = BlurTexture(sprite);
				UsingDefault = false;
			}
			// Swip
			if (SwipingCor != null) {
				StopCoroutine(SwipingCor);
			}
			if (animation) {
				SwipingCor = StartCoroutine(Swip());
			} else {
				IMG.sprite = sprite;
				Fitter.aspectRatio = sprite ? sprite.rect.width / sprite.rect.height : 1f;
				IMG.color = new Color(Brightness, Brightness, Brightness, 1f);
				IMG.transform.localScale = Vector3.one * SCALE_MUTI;
				Resources.UnloadUnusedAssets();
			}
			IEnumerator Swip () {
				yield return new WaitForSeconds(0.5f);
				float startDarkness = IMG.color.r;
				float aimBrightness = Brightness;
				float rgb;
				const float DURATION = 3.277f;
				for (float t = 0f; t < DURATION; t += Time.deltaTime) {
					rgb = startDarkness * (1f - t / DURATION);
					IMG.color = new Color(rgb, rgb, rgb, 1f);
					yield return new WaitForEndOfFrame();
				}
				IMG.transform.localScale = Vector3.one;
				IMG.color = Color.black;
				IMG.sprite = sprite;
				Fitter.aspectRatio = sprite ? sprite.rect.width / sprite.rect.height : 1f;
				for (float t = 0f; t < DURATION; t += Time.deltaTime) {
					rgb = aimBrightness * (t / DURATION);
					IMG.color = new Color(rgb, rgb, rgb, 1f);
					IMG.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * SCALE_MUTI, t / DURATION);
					yield return new WaitForEndOfFrame();
				}
				IMG.color = new Color(Brightness, Brightness, Brightness, 1f);
				IMG.transform.localScale = Vector3.one * SCALE_MUTI;
				Resources.UnloadUnusedAssets();
				SwipingCor = null;
			}
		}


		private Sprite BlurTexture (Sprite sprite) {
			try {
				if (sprite.texture.format != TextureFormat.ARGB32 || (int)sprite.texture.graphicsFormat != 88) {
					var texture = TryFixTextureFormat(sprite.texture);
					sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
				}
				var rTexture = new RenderTexture(sprite.texture.width, sprite.texture.height, 0);
				Graphics.Blit(sprite.texture, rTexture, m_BlurMat);
				Graphics.CopyTexture(rTexture, 0, 0, sprite.texture, 0, 0);
			} catch { }
			return sprite;
		}


		private Texture2D TryFixTextureFormat (Texture2D texture) {
			var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
			newTexture.SetPixels(texture.GetPixels());
			newTexture.Apply();
			return newTexture;
		}


	}
}