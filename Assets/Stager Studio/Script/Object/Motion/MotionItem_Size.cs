namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class MotionItem_Size : MotionItem {



		// Ser
		[SerializeField] private Transform m_Selection = null;
		[SerializeField] private SpriteRenderer m_Line = null;



		// MSG
		private void OnEnable () => Update();


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
				float value02 = Mathf.Clamp(Util.Remap(-m_Line.size.x, m_Line.size.x, 0, 2f, localPos.x), 0f, 2f);
				float value016 = value02 < 1f ? value02 : Util.Remap(1f, 2f, 1f, 16f, value02);
				if (GetGridEnabled()) {
					value016 = Util.Snap(value016, value02 < 1f ? 20f : 1f);
				}
				map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), value016);
				SetLabelText(value016.ToString());
				OnMotionChanged();
			}
		}


		// LGC
		private void SetSliderValue (float value016) {
			float value02 = Mathf.Clamp(value016 < 1f ? value016 : Util.Remap(1f, 16f, 1f, 2f, value016), 0f, 2f);
			Handle.localPosition = new Vector3(
				Util.Remap(0f, 2f, -m_Line.size.x, m_Line.size.x, value02),
				0f, 0f
			);
		}


	}
}