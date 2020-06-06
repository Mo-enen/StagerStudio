namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Saving;



	public class StageEasterEgg : MonoBehaviour {


		// Api
		public string Workspace { get; set; } = "";

		// Ser
		[Header("Skin Icon"), SerializeField] private Image m_Skin = null;
		[SerializeField] private Sprite[] m_ShirtIcons = null;



		// API
		public void CheckEasterEggs () {
			// Suit
			int index = 0;
			try {
				var dirs = Util.GetDirectsIn(Workspace, true);
				foreach (var dir in dirs) {
					if (dir.Name.StartsWith("水手服")) {
						index = 1;
						break;
					}
					if (dir.Name.StartsWith("百褶裙")) {
						index = 2;
						break;
					}
				}
			} catch { }
			SetShirtIcon(index);
		}



		// LGC
		private void SetShirtIcon (int index) {
			m_Skin.sprite = m_ShirtIcons[Mathf.Clamp(index, 0, m_ShirtIcons.Length - 1)];
		}





	}
}