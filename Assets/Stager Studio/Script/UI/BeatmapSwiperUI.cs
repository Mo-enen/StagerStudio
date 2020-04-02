namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class BeatmapSwiperUI : MonoBehaviour {


		// SUB
		private class TransformComparer : IComparer<Transform> {
			public int Compare (Transform x, Transform y) {
				if (int.TryParse(x.name, out int _x) && int.TryParse(y.name, out int _y)) {
					return _x.CompareTo(_y);
				}
				return x.name.CompareTo(y.name);
			}
		}
		public delegate Dictionary<string, Beatmap> BeatmapMapHandler ();
		public delegate void VoidStringHandler (string str);


		// Api
		public static BeatmapMapHandler GetBeatmapMap { get; set; } = null;
		public static VoidStringHandler TriggerSwitcher { get; set; } = null;

		// Ser
		[SerializeField] private RectTransform m_Content = null;
		[SerializeField] private Grabber m_BeatmapItemPrefab = null;


		// API
		public void Init () {
			var mapMap = GetBeatmapMap();
			foreach (var pair in mapMap) {
				if (pair.Value == null) { continue; }
				var graber = Instantiate(m_BeatmapItemPrefab, m_Content);
				var rt = graber.transform as RectTransform;
				rt.name = pair.Value.Level.ToString();
				rt.anchoredPosition3D = rt.anchoredPosition;
				rt.localRotation = Quaternion.identity;
				rt.localScale = Vector3.one;
				rt.SetAsLastSibling();
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (m_BeatmapItemPrefab.transform as RectTransform).rect.height);
				graber.Grab<Text>("Text").text = pair.Value.Tag;
				graber.Grab<Button>().onClick.AddListener(OnClick);
				void OnClick () {
					Close();
					TriggerSwitcher(pair.Key);
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