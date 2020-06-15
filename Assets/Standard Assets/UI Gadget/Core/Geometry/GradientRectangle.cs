namespace UIGadget {
	using Boo.Lang.Runtime.DynamicDispatching;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;



	[AddComponentMenu("UIGadget/Gradient Rectangle")]
	public class GradientRectangle : Image {



		// Ser
		[SerializeField] private bool m_Horizontal = true;
		[SerializeField] private Gradient m_Gradien = null;


		protected override void OnPopulateMesh (VertexHelper toFill) {

			if (m_Gradien == null) {
				base.OnPopulateMesh(toFill);
				return;
			}

			var cKeys = m_Gradien.colorKeys;
			var aKeys = m_Gradien.alphaKeys;

			if ((cKeys == null || cKeys.Length == 0) && (aKeys == null || aKeys.Length == 0)) {
				base.OnPopulateMesh(toFill);
				return;
			}

			bool blend = m_Gradien.mode == GradientMode.Blend;
			var rect = GetPixelAdjustedRect();

			toFill.Clear();

			int cLen = cKeys != null ? cKeys.Length : 0;
			int aLen = aKeys != null ? aKeys.Length : 0;
			float cTime = cLen > 0 ? cKeys[0].time : 0f;
			float aTime = aLen > 0 ? aKeys[0].time : 0f;
			float prevTime = 0f;
			float prevX = m_Horizontal ? rect.xMin : rect.yMin;
			Color prevColor = m_Gradien.Evaluate(0f) * color;
			for (int cIndex = 0, aIndex = 0; cIndex < cLen || aIndex < aLen;) {
				if (cTime <= aTime) {
					float x = Mathf.Lerp(
						m_Horizontal ? rect.xMin : rect.yMin,
						m_Horizontal ? rect.xMax : rect.yMax,
						cTime
					);
					var _color = m_Gradien.Evaluate(cTime) * color;
					if (!blend) {
						prevColor = _color;
					}
					if (Mathf.Abs(cTime - prevTime) > GadgetUtil.FLOAT_GAP) {
						if (m_Horizontal) {
							GadgetUtil.SetCacheColor(prevColor, prevColor, _color, _color);
							GadgetUtil.FillQuad(toFill, prevX, x, rect.yMin, rect.yMax);
						} else {
							GadgetUtil.SetCacheColor(prevColor, _color, _color, prevColor);
							GadgetUtil.FillQuad(toFill, rect.xMin, rect.xMax, prevX, x);
						}
					}
					prevColor = _color;
					prevTime = cTime;
					prevX = x;
					cIndex++;
					if (cTime == aTime) {
						aIndex++;
					}
					cTime = cIndex < cLen ? cKeys[cIndex].time : 1f;
				} else {
					float x = Mathf.Lerp(
						m_Horizontal ? rect.xMin : rect.yMin,
						m_Horizontal ? rect.xMax : rect.yMax,
						aTime
					);
					var _color = m_Gradien.Evaluate(aTime) * color;
					if (!blend) {
						prevColor = _color;
					}
					if (Mathf.Abs(aTime - prevTime) > GadgetUtil.FLOAT_GAP) {
						if (m_Horizontal) {
							GadgetUtil.SetCacheColor(prevColor, prevColor, _color, _color);
							GadgetUtil.FillQuad(toFill, prevX, x, rect.yMin, rect.yMax);
						} else {
							GadgetUtil.SetCacheColor(prevColor, _color, _color, prevColor);
							GadgetUtil.FillQuad(toFill, rect.xMin, rect.xMax, prevX, x);
						}
					}
					prevColor = _color;
					prevTime = aTime;
					prevX = x;
					aIndex++;
					aTime = aIndex < aLen ? aKeys[aIndex].time : 1f;
				}
			}
			// End
			if (prevTime < 1f - GadgetUtil.FLOAT_GAP) {
				var _color = m_Gradien.Evaluate(1f) * color;
				if (!blend) {
					prevColor = _color;
				}
				if (m_Horizontal) {
					GadgetUtil.SetCacheColor(prevColor, prevColor, _color, _color);
					GadgetUtil.FillQuad(toFill, prevX, rect.xMax, rect.yMin, rect.yMax);
				} else {
					GadgetUtil.SetCacheColor(prevColor, _color, _color, prevColor);
					GadgetUtil.FillQuad(toFill, rect.xMin, rect.xMax, prevX, rect.yMax);
				}
			}
		}


	}
}