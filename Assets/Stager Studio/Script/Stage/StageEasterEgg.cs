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

		// Saving
		private SavingInt ShirtIconIndex = new SavingInt("StageEasterEgg.ShirtIconIndex", 0);


		// MSG
		private void Start () {
			SetShirtIcon(ShirtIconIndex.Value);


		}



		// API
		public void CheckEasterEggs () {
			// Suit
			try {
				var dirs = Util.GetDirectsIn(Workspace, true);
				foreach (var dir in dirs) {
					switch (dir.Name) {
						case "短袖圆领T恤":
							SetShirtIcon(0);
							break;
						case "水手服":
							SetShirtIcon(1);
							break;
						case "百褶裙":
							SetShirtIcon(2);
							break;
					}
				}
			} catch { }





		}



		// LGC
		private void SetShirtIcon (int index) {
			ShirtIconIndex.Value = index;
			m_Skin.sprite = m_ShirtIcons[Mathf.Clamp(index, 0, m_ShirtIcons.Length - 1)];
		}





	}
}