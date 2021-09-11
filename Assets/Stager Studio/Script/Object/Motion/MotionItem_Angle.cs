namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class MotionItem_Angle : MotionItem {



		// Ser
		[SerializeField] private Transform m_Selection = null;
		[SerializeField] private SpriteRenderer m_Ring = null;


		// MSG
		private void OnEnable () => Update();


		private void Update () {
			var map = GetBeatmap();
			int motionIndex = transform.GetSiblingIndex();
			if (map != null) {
				var pos = GetPosition(map, motionIndex, false);
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
				float value180 = Mathf.Repeat(360f - Quaternion.LookRotation(
					Vector3.forward,
					localPos
				).eulerAngles.z + 180f, 360f) - 180f;
				if (GetGridEnabled()) {
					float minDelta = float.MaxValue;
					float resultAngle = 0f;
					for (int i = 0; i <= 12; i++) {
						float angle = i * 30f - 180f;
						float delta = Mathf.Abs(Mathf.DeltaAngle(value180, angle));
						if (delta < minDelta) {
							minDelta = delta;
							resultAngle = angle;
						}
					}
					value180 = resultAngle;
				}
				map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), value180);
				SetLabelText(value180.ToString("0"));
				OnMotionChanged();
			}
		}


		// LGC
		private void SetSliderValue (float value360) {
			Handle.localPosition = Quaternion.Euler(0f, 0f, -value360) * Vector3.up * (m_Ring.size.x * 0.5f - 0.0025f);
		}


	}
}