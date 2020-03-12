namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Stage;


	public class SkinSwiperUI : MonoBehaviour {


		// SUB
		private class TransformComparer : IComparer<Transform> {
			public int Compare (Transform x, Transform y) {
				if (int.TryParse(x.name, out int _x) && int.TryParse(y.name, out int _y)) {
					return _x.CompareTo(_y);
				}
				return x.name.CompareTo(y.name);
			}
		}


		// Ser
		[SerializeField] private RectTransform m_Content = null;
		[SerializeField] private Grabber m_SkinItemPrefab = null;


		// API
		public void Init (StageSkin stageSkin) {
			foreach (var skinName in stageSkin.AllSkinNames) {
				var sName = skinName;
				if (string.IsNullOrEmpty(sName)) { continue; }
				var graber = Instantiate(m_SkinItemPrefab, m_Content);
				var rt = graber.transform as RectTransform;
				rt.name = sName;
				rt.anchoredPosition3D = rt.anchoredPosition;
				rt.localRotation = Quaternion.identity;
				rt.localScale = Vector3.one;
				rt.SetAsLastSibling();
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (m_SkinItemPrefab.transform as RectTransform).rect.height);
				graber.Grab<Text>("Text").text = sName;
				graber.Grab<Button>().onClick.AddListener(OnClick);
				void OnClick () {
					Close();
					stageSkin.LoadSkin(sName);
				}
			}
			SortContent();
		}


		public void Close () {
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			transform.parent.DestroyAllChildImmediately();
		}


		// LGC
		private void SortContent () {
			var list = new List<Transform>();
			int len = m_Content.childCount;
			for (int i = 0; i < len; i++) {
				list.Add(m_Content.GetChild(i));
			}
			list.Sort(new TransformComparer());
			foreach (var tf in list) {
				tf.SetAsLastSibling();
			}
		}


	}
}