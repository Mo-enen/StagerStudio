namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Stage;
	using UnityEngine.UI;

	public class AbreastSwitcherUI : MonoBehaviour {




		// Short
		private StageGame Game => _Game != null ? _Game : (_Game = FindObjectOfType<StageGame>());

		// Ser
		[SerializeField] private Transform m_StageContainer = null;
		[SerializeField] private RectTransform m_Container = null;
		[SerializeField] private Grabber m_ItemPrefab = null;
		[SerializeField] private RectTransform m_AbreastAllHighlight = null;

		// Data
		private StageGame _Game = null;


		// MSG
		private void Update () {
			int itemCount = m_StageContainer.childCount;
			int conCount = m_Container.childCount;
			if (conCount != itemCount) {
				if (conCount > itemCount) {
					m_Container.FixChildcountImmediately(itemCount);
				} else {
					for (int i = conCount; i < itemCount; i++) {
						var grab = Instantiate(m_ItemPrefab, m_Container);
						var rt = grab.transform as RectTransform;
						rt.SetAsLastSibling();
						rt.localRotation = Quaternion.identity;
						rt.localScale = Vector3.one;
						grab.Grab<Button>().onClick.AddListener(() => {
							int sIndex = grab.transform.GetSiblingIndex();
							Game.SetAbreastIndex(sIndex);
							if (Game.AllStageAbreast) {
								Game.SetAllStageAbreast(false);
							}
						});
					}
				}
				RefreshUI();
			}
		}


		// API
		public void RefreshUI () {
			bool useA = Game.UseAbreast;
			gameObject.SetActive(useA);
			// Highlight
			if (useA) {
				int aIndex = Game.AbreastIndex;
				int count = m_Container.childCount;
				bool allA = Game.AllStageAbreast;
				m_AbreastAllHighlight.gameObject.SetActive(allA);
				for (int i = 0; i < count; i++) {
					var grab = m_Container.GetChild(i).GetComponent<Grabber>();
					grab.Grab<RectTransform>("Highlight").gameObject.SetActive(!allA && i == aIndex);
					grab.Grab<Text>("Index").text = i.ToString();
				}
			}
		}



	}
}