namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UIGadget;



	public class TweenSelectorUI : MonoBehaviour {

		

		// Handler
		public delegate List<AnimationCurve> TweensHandler ();
		public delegate void IntHandler (int i);
		public static TweensHandler GetProjectTweens { get; set; } = null;
		public static IntHandler TrySetCurrentTween { get; set; } = null;

		// Ser
		[SerializeField] private Grabber m_Prefab = null;
		[SerializeField] private RectTransform m_Content = null;


		// API
		public void Open () {
			var tweens = GetProjectTweens();
			if (tweens == null || tweens.Count == 0) { return; }
			for (int i = 0; i < tweens.Count; i++) {
				int index = i;
				var grab = Instantiate(m_Prefab, m_Content);
				var rt = grab.transform as RectTransform;
				rt.anchoredPosition3D = rt.anchoredPosition;
				rt.localScale = Vector3.one;
				rt.localRotation = Quaternion.identity;
				rt.SetAsLastSibling();
				grab.Grab<Button>().onClick.AddListener(() => {
					TrySetCurrentTween(index);
					Invoke(nameof(Close), 0.01f);
				});
				grab.Grab<Text>("Label").text = index.ToString("00");
				grab.Grab<Curve>("Curve").CurveData = tweens[index];
			}
		}


		public void Close () {
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}




	}
}