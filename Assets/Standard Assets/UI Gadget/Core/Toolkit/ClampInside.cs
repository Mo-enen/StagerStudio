namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	[AddComponentMenu("UIGadget/ClampInside")]
	public class ClampInside : MonoBehaviour {


		public enum Mode {
			Update = 0,
			Manually = 1,
		}


		[SerializeField] private Mode m_ClampMode = Mode.Update;


		void Update () {
			if (m_ClampMode == Mode.Update) {
				Clamp();
			}
		}


		public void Clamp () {
			var target = transform as RectTransform;
			target.anchoredPosition = VectorClamp2(
				target.anchoredPosition,
				target.pivot * target.rect.size - target.anchorMin * ((RectTransform)target.parent).rect.size,
				(Vector2.one - target.anchorMin) * ((RectTransform)target.parent).rect.size - (Vector2.one - target.pivot) * target.rect.size
			);
			Vector2 VectorClamp2 (Vector2 v, Vector2 min, Vector2 max) => new Vector2(
				Mathf.Clamp(v.x, min.x, max.x),
				Mathf.Clamp(v.y, min.y, max.y)
			);
		}


	}
}