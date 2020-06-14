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
		[SerializeField] private Image m_Skin = null;
		[SerializeField] private Sprite[] m_ShirtIcons = null;
		[SerializeField] private Text m_TitleLabel = null;


		// API
		public void CheckEasterEggs () {

			int shirtIndex = 0;
			bool dioMode = false;

			// All Chapters
			try {
				var dirs = Util.GetDirectsIn(Workspace, true);
				foreach (var dir in dirs) {
					if (dir.Name.StartsWith("水手服") || dir.Name.StartsWith("セーラー服")) {
						shirtIndex = 1;
					}
					if (dir.Name.StartsWith("百褶裙") || dir.Name.StartsWith("プリーツスカート")) {
						shirtIndex = 2;
					}
					if (dir.Name.StartsWith("木大") || dir.Name.StartsWith("無駄") || dir.Name.StartsWith("ムダ")) {
						dioMode = true;
					}
				}
			} catch { }

			// Suit
			SetShirtIcon(shirtIndex);

			// Dio
			m_TitleLabel.text = dioMode ? "STAGER.STU<color=#9d915e>DIO</color>" : "STAGER.STUDIO";


		}



		// LGC
		private void SetShirtIcon (int index) {
			m_Skin.sprite = m_ShirtIcons[Mathf.Clamp(index, 0, m_ShirtIcons.Length - 1)];
		}





	}
}