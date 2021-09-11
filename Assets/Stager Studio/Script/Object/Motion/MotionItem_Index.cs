namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class MotionItem_Index : MotionItem {



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
						SetSliderValue(Util.Remap(0f, IndexCount - 1, 0f, 1f, valueA));
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
				float value = Util.Remap(-m_Line.size.x / 2f, m_Line.size.x / 2f, 0, IndexCount - 1, localPos.x);
				map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), value);
				SetLabelText(Mathf.RoundToInt(value).ToString());
				OnMotionChanged();
			}
		}


		// LGC
		private void SetSliderValue (float value01) => Handle.localPosition = new Vector3(
			Util.Remap(0f, 1f, -m_Line.size.x / 2f, m_Line.size.x / 2f, Mathf.Clamp(value01, 0f, 1f)),
			0f, 0f
		);



	}
}