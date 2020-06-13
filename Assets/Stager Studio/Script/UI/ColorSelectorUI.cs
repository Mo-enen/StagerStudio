namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UIGadget;


	public class ColorSelectorUI : MonoBehaviour {



		// Handler
		public delegate List<Color32> PaletteColorHandler ();
		public delegate void VoidIntHandler (int i);
		public static PaletteColorHandler GetPalette { get; set; } = null;
		public static VoidIntHandler TrySetCurrentColor { get; set; } = null;

		// Ser
		[SerializeField] private Grabber m_Prefab = null;
		[SerializeField] private RectTransform m_Content = null;



		// API
		public void Open () {
			var pal = GetPalette();
			if (pal == null || pal.Count == 0) { return; }
			for (int i = 0; i < pal.Count; i++) {
				int index = i;
				var grab = Instantiate(m_Prefab, m_Content);
				var rt = grab.transform as RectTransform;
				rt.anchoredPosition3D = rt.anchoredPosition;
				rt.localScale = Vector3.one;
				rt.localRotation = Quaternion.identity;
				rt.SetAsLastSibling();
				grab.Grab<Button>().onClick.AddListener(() => {
					TrySetCurrentColor(index);
				});
				grab.Grab<Text>("Label").text = index.ToString("00");
				grab.Grab<Image>("Color").color = pal[index];
			}
		}


		public void Close () {
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}


	}
}