namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;



	public class AbreastSwitcherUI : MonoBehaviour {



		// SUB
		public delegate void VoidIntHandler (int value);
		public delegate void VoidBoolHandler (bool value);
		public delegate bool BoolHandler ();
		public delegate int IntHandler ();


		// Handler
		public static VoidIntHandler SetAbreastIndex { get; set; } = null;
		public static VoidBoolHandler SetAllStageAbreast { get; set; } = null;
		public static BoolHandler GetUseAbreast { get; set; } = null;
		public static BoolHandler GetAllStageAbreast { get; set; } = null;
		public static IntHandler GetAbreastIndex { get; set; } = null;


		// Ser
		[SerializeField] private Transform m_StageContainer = null;
		[SerializeField] private RectTransform m_Container = null;
		[SerializeField] private Grabber m_ItemPrefab = null;
		[SerializeField] private RectTransform m_AbreastAllHighlight = null;


		// MSG
		private void Update () {
			int itemCount = m_StageContainer.childCount;
			int conCount = m_Container.childCount;
			if (conCount != itemCount) {
				if (conCount > itemCount) {
					m_Container.FixChildcountImmediately(itemCount);
				} else {
					bool allA = GetAllStageAbreast();
					for (int i = conCount; i < itemCount; i++) {
						var grab = Instantiate(m_ItemPrefab, m_Container);
						var rt = grab.transform as RectTransform;
						rt.SetAsLastSibling();
						rt.localRotation = Quaternion.identity;
						rt.localScale = Vector3.one;
						grab.Grab<Button>().onClick.AddListener(() => {
							int sIndex = grab.transform.GetSiblingIndex();
							SetAbreastIndex(sIndex);
							SetAllStageAbreast(false);
							if (allA) {
								SetAllStageAbreast(false);
							}
						});
					}
				}
				RefreshUI();
			}
		}


		// API
		public void RefreshUI () {
			bool useA = GetUseAbreast();
			gameObject.SetActive(useA);
			// Highlight
			if (useA) {
				int aIndex = GetAbreastIndex();
				int count = m_Container.childCount;
				bool allA = GetAllStageAbreast();
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