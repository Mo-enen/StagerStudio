namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;



	[AddComponentMenu("UIGadget/ColorPicker")]
	public class ColorPicker : Image, IPointerDownHandler, IDragHandler {



		// SUB
		[System.Serializable]
		public class SetColorEvent : UnityEvent<Color> { }


		[System.Serializable]
		public enum PickerMode {
			HSV = 0,
			RGB = 1,
		}


		// Api
		public Color Color {
			get {
				var c = Color.HSVToRGB(m_H, m_S, m_V);
				c.a = m_A;
				return c;
			}
		}
		public float R {
			get {
				return Color.HSVToRGB(m_H, m_S, m_V).r;
			}
			set {
				var c = Color.HSVToRGB(m_H, m_S, m_V);
				c.r = value;
				Color.RGBToHSV(c, out m_H, out m_S, out m_V);
			}
		}
		public float G {
			get {
				return Color.HSVToRGB(m_H, m_S, m_V).g;
			}
			set {
				var c = Color.HSVToRGB(m_H, m_S, m_V);
				c.g = value;
				Color.RGBToHSV(c, out m_H, out m_S, out m_V);
			}
		}
		public float B {
			get {
				return Color.HSVToRGB(m_H, m_S, m_V).b;
			}
			set {
				var c = Color.HSVToRGB(m_H, m_S, m_V);
				c.b = value;
				Color.RGBToHSV(c, out m_H, out m_S, out m_V);
			}
		}
		public float H { get => m_H; set => m_H = value; }
		public float S { get => m_S; set => m_S = value; }
		public float V { get => m_V; set => m_V = value; }
		public float A { get => m_UseAlpha ? m_A : 1f; set => m_A = value; }

		// VAR
		[Header("Picker")]
		[Range(0f, 1f), SerializeField] private float m_H = 0f;
		[Range(0f, 1f), SerializeField] private float m_S = 1f;
		[Range(0f, 1f), SerializeField] private float m_V = 1f;
		[Range(0f, 1f), SerializeField] private float m_A = 1f;
		[SerializeField] private PickerMode m_Mode = PickerMode.HSV;
		[Header("UI")]
		[SerializeField] private bool m_UseAlpha = true;
		[SerializeField] private float m_ThumbnailWidthAmonut = 1f;
		[SerializeField] private float m_InnerGap = 0f;
		[SerializeField] private float m_HandleWidth = 12f;
		[SerializeField] private float m_HandleOutlineSize = 6f;
		[SerializeField] private Color m_Background = new Color(0.86f, 0.86f, 0.86f, 1f);
		[SerializeField] private Color m_Handle = Color.white;
		[SerializeField] private Color m_HandleOutline = Color.black;
		[SerializeField] private RectOffset m_Padding = default;

		[Space]
		[SerializeField] private SetColorEvent m_OnColorEdit = null;


		// MSG
		protected override void OnPopulateMesh (VertexHelper toFill) {

			toFill.Clear();

			var pixelRect = GetPixelAdjustedRect();
			var rect = pixelRect;
			rect.x += m_Padding.left;
			rect.y += m_Padding.bottom;
			rect.width -= m_Padding.horizontal;
			rect.height -= m_Padding.vertical;
			float thumbnailWidth = rect.height * m_ThumbnailWidthAmonut;

			// Background
			GadgetUtil.SetCacheColor(m_Background);
			GadgetUtil.FillQuad(toFill, pixelRect.xMin, pixelRect.xMax, pixelRect.yMin, pixelRect.yMax);

			// Thumbnail
			if (thumbnailWidth > GadgetUtil.FLOAT_GAP) {
				GadgetUtil.SetCacheColor(Color);
				GadgetUtil.FillQuad(
					toFill,
					rect.xMax - thumbnailWidth,
					rect.xMax, rect.yMin, rect.yMax
				);
			}

			// Slider
			FillSliderMesh(toFill, rect, thumbnailWidth);

		}


		private void FillSliderMesh (VertexHelper toFill, Rect rect, float thumbnailWidth) {

			float innerGap = Mathf.Max(0, m_InnerGap);
			int sliderCount = m_UseAlpha ? 4 : 3;
			float sliderLeft = rect.xMin;
			float sliderRight = rect.xMax - thumbnailWidth - innerGap;
			float sliderDown, sliderUp;
			bool isHSV = m_Mode == PickerMode.HSV;
			bool useHandleOutline = m_HandleOutlineSize > GadgetUtil.FLOAT_GAP && m_HandleOutline.a > GadgetUtil.FLOAT_GAP;
			bool useHandle = m_HandleWidth > GadgetUtil.FLOAT_GAP;
			float halfHandleWidth = m_HandleWidth / 2f;
			float r = R, g = G, b = B;
			float handleX;
			Color colorL, colorR;

			// H/R Slider
			(sliderDown, sliderUp) = GetSliderDownUp(0, rect, sliderCount, innerGap);
			if (isHSV) {
				// BG
				for (int i = 0; i < 7; i++) {
					// Color
					colorL = Color.HSVToRGB(i / 7f, 1f, 1f);
					colorR = Color.HSVToRGB((i + 1) / 7f, 1f, 1f);
					GadgetUtil.SetCacheColor(colorL, colorL, colorR, colorR);
					// Pos
					GadgetUtil.FillQuad(
						toFill,
						GadgetUtil.Remap(0, 7, sliderLeft, sliderRight, i),
						GadgetUtil.Remap(0, 7, sliderLeft, sliderRight, i + 1),
						sliderDown, sliderUp
					);
				}
			} else {
				colorL = new Color(0f, g, b);
				colorR = new Color(1f, g, b);
				GadgetUtil.SetCacheColor(colorL, colorL, colorR, colorR);
				GadgetUtil.FillQuad(
					toFill,
					sliderLeft, sliderRight,
					sliderDown, sliderUp
				);
			}

			// Handle
			if (useHandle) {
				handleX = GadgetUtil.Remap(0f, 1f, sliderLeft, sliderRight, isHSV ? m_H : R);
				// Outline
				if (useHandleOutline) {
					GadgetUtil.SetCacheColor(m_HandleOutline);
					GadgetUtil.FillQuad(
					toFill,
					handleX - halfHandleWidth - m_HandleOutlineSize,
					handleX + halfHandleWidth + m_HandleOutlineSize,
					sliderDown - m_HandleOutlineSize, sliderUp + m_HandleOutlineSize
				);
				}
				// Handle
				GadgetUtil.SetCacheColor(m_Handle);
				GadgetUtil.FillQuad(
					toFill,
					handleX - halfHandleWidth,
					handleX + halfHandleWidth,
					sliderDown, sliderUp
				);
			}

			// S/G Slider
			(sliderDown, sliderUp) = GetSliderDownUp(1, rect, sliderCount, innerGap);
			colorL = isHSV ? Color.HSVToRGB(m_H, 0f, Mathf.Lerp(0.3f, 1f, m_V)) : new Color(r, 0f, b);
			colorR = isHSV ? Color.HSVToRGB(m_H, 1f, Mathf.Lerp(0.3f, 1f, m_V)) : new Color(r, 1f, b);
			GadgetUtil.SetCacheColor(colorL, colorL, colorR, colorR);
			GadgetUtil.FillQuad(
				toFill,
				sliderLeft, sliderRight,
				sliderDown, sliderUp
			);

			// Handle
			if (useHandle) {
				handleX = GadgetUtil.Remap(0f, 1f, sliderLeft, sliderRight, isHSV ? m_S : G);
				// Outline
				if (useHandleOutline) {
					GadgetUtil.SetCacheColor(m_HandleOutline);
					GadgetUtil.FillQuad(
					toFill,
					handleX - halfHandleWidth - m_HandleOutlineSize,
					handleX + halfHandleWidth + m_HandleOutlineSize,
					sliderDown - m_HandleOutlineSize, sliderUp + m_HandleOutlineSize
				);
				}
				// Handle
				GadgetUtil.SetCacheColor(m_Handle);
				GadgetUtil.FillQuad(
					toFill,
					handleX - halfHandleWidth,
					handleX + halfHandleWidth,
					sliderDown, sliderUp
				);
			}

			// V/B Slider
			(sliderDown, sliderUp) = GetSliderDownUp(2, rect, sliderCount, innerGap);
			colorL = isHSV ? Color.HSVToRGB(m_H, m_S, 0f) : new Color(r, g, 0f);
			colorR = isHSV ? Color.HSVToRGB(m_H, m_S, 1f) : new Color(r, g, 1f);
			GadgetUtil.SetCacheColor(colorL, colorL, colorR, colorR);
			GadgetUtil.FillQuad(
				toFill,
				sliderLeft, sliderRight,
				sliderDown, sliderUp
			);

			// Handle
			if (useHandle) {
				handleX = GadgetUtil.Remap(0f, 1f, sliderLeft, sliderRight, isHSV ? m_V : B);
				// Outline
				if (useHandleOutline) {
					GadgetUtil.SetCacheColor(m_HandleOutline);
					GadgetUtil.FillQuad(
					toFill,
					handleX - halfHandleWidth - m_HandleOutlineSize,
					handleX + halfHandleWidth + m_HandleOutlineSize,
					sliderDown - m_HandleOutlineSize, sliderUp + m_HandleOutlineSize
				);
				}
				// Handle
				GadgetUtil.SetCacheColor(m_Handle);
				GadgetUtil.FillQuad(
					toFill,
					handleX - halfHandleWidth,
					handleX + halfHandleWidth,
					sliderDown, sliderUp
				);
			}

			// A Slider
			if (m_UseAlpha) {
				(sliderDown, sliderUp) = GetSliderDownUp(3, rect, sliderCount, innerGap);
				colorL = Color.HSVToRGB(m_H, m_S, m_V);
				colorR = Color.HSVToRGB(m_H, m_S, m_V);
				colorL.a = 0f;
				colorR.a = 1f;
				GadgetUtil.SetCacheColor(colorL, colorL, colorR, colorR);
				GadgetUtil.FillQuad(
					toFill,
					sliderLeft, sliderRight,
					sliderDown, sliderUp
				);
				// Handle
				if (useHandle) {
					handleX = GadgetUtil.Remap(0f, 1f, sliderLeft, sliderRight, m_A);
					// Outline
					if (useHandleOutline) {
						GadgetUtil.SetCacheColor(m_HandleOutline);
						GadgetUtil.FillQuad(
						toFill,
						handleX - halfHandleWidth - m_HandleOutlineSize,
						handleX + halfHandleWidth + m_HandleOutlineSize,
						sliderDown - m_HandleOutlineSize, sliderUp + m_HandleOutlineSize
					);
					}
					// Handle
					GadgetUtil.SetCacheColor(m_Handle);
					GadgetUtil.FillQuad(
						toFill,
						handleX - halfHandleWidth,
						handleX + halfHandleWidth,
						sliderDown, sliderUp
					);
				}
			}
		}


		public void OnPointerDown (PointerEventData eventData) => OnDrag(eventData);


		public void OnDrag (PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) { return; }
			if (
				RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint) &&
				RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localDownPoint)
			) {
				var rect = GetPixelAdjustedRect();
				rect.x += m_Padding.left;
				rect.y += m_Padding.bottom;
				rect.width -= m_Padding.horizontal;
				rect.height -= m_Padding.vertical;
				float innerGap = Mathf.Max(0, m_InnerGap);
				float thumbnailWidth = rect.height * m_ThumbnailWidthAmonut;
				float sliderLeft = rect.xMin;
				float sliderRight = rect.xMax - thumbnailWidth - innerGap;
				int sliderCount = m_UseAlpha ? 4 : 3;
				int draggingID = -1;

				// Down Check
				if (localDownPoint.x > sliderLeft && localDownPoint.x < sliderRight) {
					for (int id = 0; id < sliderCount; id++) {
						var (down, up) = GetSliderDownUp(id, rect, sliderCount, innerGap);
						if (localDownPoint.y > down && localDownPoint.y < up) {
							draggingID = id;
							break;
						}
					}
				}

				if (draggingID < 0) { return; }

				float x01 = Mathf.InverseLerp(sliderLeft, sliderRight, localPoint.x);
				switch (draggingID) {
					case 0:
						if (m_Mode == PickerMode.HSV) {
							H = x01;
						} else {
							R = x01;
						}
						break;
					case 1:
						if (m_Mode == PickerMode.HSV) {
							S = x01;
						} else {
							G = x01;
						}
						break;
					case 2:
						if (m_Mode == PickerMode.HSV) {
							V = x01;
						} else {
							B = x01;
						}
						break;
					case 3:
						A = x01;
						break;
				}

				// Final
				m_OnColorEdit?.Invoke(Color);
				SetVerticesDirty();
			}
		}


		// LGC
		private (float down, float up) GetSliderDownUp (int id, Rect rect, int sliderCount, float innerGap) => (
			Mathf.Lerp(rect.yMin, rect.yMax, (sliderCount - id - 1f) / sliderCount) + innerGap / 2f,
			Mathf.Lerp(rect.yMin, rect.yMax, (sliderCount - id - 0f) / sliderCount) - innerGap / 2f
		);


	}
}