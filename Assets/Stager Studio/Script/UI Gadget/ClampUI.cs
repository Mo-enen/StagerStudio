namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class ClampUI : MonoBehaviour {
		void Update () => Util.ClampRectTransform(transform as RectTransform);
	}
}