namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class MotionItem_X : MotionItem {




		// Ser
		[SerializeField] private Transform m_Selection = null;
		[SerializeField] private SpriteRenderer m_Line = null;
		[SerializeField] private SpriteRenderer m_Highlight = null;

		// Data
		private float LineScaleX = 1f;

		// MSG
		private void OnEnable () {
			LineScaleX = m_Line.transform.localScale.x;
			Update();
		}

		private void Update () {
			var map = GetBeatmap();
			int motionIndex = transform.GetSiblingIndex();
			if (map != null) {
				var pos = GetPosition(map, motionIndex);
				if (pos.HasValue) {
					// Pos
					transform.position = pos.Value;
					// Slider Value
					if (map.GetMotionValueTween(ItemIndex, MotionType, motionIndex, out float valueA, out _, out _).hasA) {
						SetSliderValue(valueA);
					}
				}
				// Root
				Root.gameObject.TrySetActive(pos.HasValue);
				// Selection
				m_Selection.gameObject.TrySetActive(motionIndex == SelectingMotionIndex);
			}
		}


		protected override void InvokeAxis (Vector2 localPos, Vector3 worldPos) {
			var map = GetBeatmap();
			if (map != null) {
				float value = Util.RemapUnclamped(-m_Line.size.x * LineScaleX / 2f, m_Line.size.x * LineScaleX / 2f, -1f, 1f, localPos.x);
				if (GetGridEnabled()) {
					value = Util.Snap(value, 16f);
				}
				map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), value);
				SetLabelText(value.ToString("0.00"));
				OnMotionChanged();
			}
		}


		private void SetSliderValue (float value11) {
			Handle.localPosition = new Vector3(
				Util.RemapUnclamped(-1f, 1f, -m_Line.size.x * LineScaleX / 2f, m_Line.size.x * LineScaleX / 2f, Mathf.Clamp(value11, -2f, 2f)),
				0f, 0f
			);
		}


		private void SetSliderWidth (float width) {
			m_Line.size = new Vector2(width / LineScaleX, m_Line.size.y);
			m_Highlight.size = new Vector2(width / m_Highlight.transform.localScale.x, m_Highlight.size.y);
		}


	}
}