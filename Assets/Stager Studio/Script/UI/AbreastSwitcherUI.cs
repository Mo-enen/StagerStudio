namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;



	public class AbreastSwitcherUI : MonoBehaviour {



		// SUB
		public delegate void VoidFloatHandler (float value);
		public delegate bool BoolHandler ();
		public delegate float FloatHandler ();


		// Handler
		public static VoidFloatHandler SetAbreastIndex { get; set; } = null;
		public static FloatHandler GetAbreastValue { get; set; } = null;
		public static FloatHandler GetAbreastIndex { get; set; } = null;


		// Ser
		[SerializeField] private Transform m_StageContainer = null;
		[SerializeField] private RectTransform m_Container = null;
		[SerializeField] private Grabber m_ItemPrefab = null;
		[SerializeField] private RectTransform m_Highlight = null;
		[SerializeField] private RectTransform m_Root = null;

		// Data
		public bool Bar_Dirty = true;



		// MSG
		private void OnEnable () {
			RefreshHighlightUI();
		}


		private void Update () {
			// Item Count
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
							SetAbreastIndex(sIndex);
						});
					}
				}
				// Labels
				int count = m_Container.childCount;
				for (int i = 0; i < count; i++) {
					var grab = m_Container.GetChild(i).GetComponent<Grabber>();
					grab.Grab<Text>("Index").text = i.ToString();
				}
				// Highlight
				RefreshHighlightUI();
			}
			// Bar Position
			if (Bar_Dirty) {
				float aValue = GetAbreastValue();
				float aPosX = Mathf.Lerp(-m_Root.rect.width, 0f, aValue);
				var oldPos = m_Root.anchoredPosition;
				if (Mathf.Abs(oldPos.x - aPosX) > 0.01f) {
					m_Root.anchoredPosition = new Vector2(
						Mathf.Lerp(oldPos.x, aPosX, Time.deltaTime * 10f),
						oldPos.y
					);
					if (!m_Root.gameObject.activeSelf) {
						m_Root.gameObject.SetActive(true);
					}
				} else {
					Bar_Dirty = false;
					m_Root.anchoredPosition = new Vector2(aPosX, oldPos.y);
					bool active = aValue > 0.5f;
					if (m_Root.gameObject.activeSelf != active) {
						m_Root.gameObject.SetActive(active);
					}
				}
			}
		}


		// API
		public void SetBarUIDirty () => Bar_Dirty = true;


		public void RefreshHighlightUI () {
			int sCount = m_Container.childCount;
			if (sCount > 0) {
				if (!m_Highlight.gameObject.activeSelf) {
					m_Highlight.gameObject.SetActive(true);
				}
				m_Highlight.position = Vector3.Lerp(
					m_Container.GetChild(0).position,
					m_Container.GetChild(sCount - 1).position,
					Mathf.InverseLerp(0f, sCount - 1, GetAbreastIndex())
				);
			} else if (m_Highlight.gameObject.activeSelf) {
				m_Highlight.gameObject.SetActive(false);
			}
		}



	}
}