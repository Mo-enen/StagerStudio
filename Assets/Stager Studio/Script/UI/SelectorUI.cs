namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class SelectorUI : MonoBehaviour {




		#region --- SUB ---


		// Handler
		public delegate Beatmap BeatmapHandler ();
		public delegate void VoidIntHandler (int i);
		public delegate void VoidRtIntHandler (RectTransform rt, int i);
		public delegate int IntHandler ();

		#endregion




		#region --- VAR ---


		// Handler
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static VoidRtIntHandler OpenItemMenu { get; set; } = null;
		public static VoidIntHandler SelectStage { get; set; } = null;
		public static VoidIntHandler SelectTrack { get; set; } = null;
		public static IntHandler GetSelectionType { get; set; } = null;
		public static IntHandler GetSelectionIndex { get; set; } = null;

		// Ser
		[SerializeField] private RectTransform m_Container_Stage = null;
		[SerializeField] private RectTransform m_Container_Track = null;
		[SerializeField] private Grabber m_StagePrefab = null;
		[SerializeField] private Grabber m_TrackPrefab = null;
		[SerializeField] private RectTransform m_Highlight = null;

		// Data
		private float LastUpdateTime = 0f;


		#endregion




		#region --- MSG ---


		private void Awake () {
			var bars = GetComponentsInChildren<Scrollbar>(true);
			foreach (var bar in bars) {
				bar.interactable = true;
			}
		}


		private void Update () {

			// Check
			if (Time.time <= LastUpdateTime + 0.1f) { return; }
			LastUpdateTime = Time.time;
			var map = GetBeatmap();
			if (map == null) {
				if (m_Container_Track.childCount > 0 || m_Container_Track.childCount > 0) {
					m_Container_Stage.DestroyAllChildImmediately();
					m_Container_Track.DestroyAllChildImmediately();
				}
				return;
			}

			// Fix Stage
			int stageCount = map.Stages.Count;
			if (m_Container_Stage.childCount > stageCount) {
				m_Container_Stage.FixChildcountImmediately(stageCount);
			} else if (m_Container_Stage.childCount < stageCount) {
				for (int i = m_Container_Stage.childCount; i < stageCount; i++) {
					var grab = Instantiate(m_StagePrefab, m_Container_Stage);
					grab.transform.SetAsLastSibling();
					grab.Grab<Button>().onClick.AddListener(() => SelectStage(grab.transform.GetSiblingIndex()));
					grab.Grab<Text>("Label").text = i.ToString("00");
					grab.Grab<TriggerUI>().CallbackRight.AddListener(() => OpenItemMenu(grab.transform as RectTransform, 0));
				}
			}

			// Fix Track
			int trackCount = map.Tracks.Count;
			if (m_Container_Track.childCount > trackCount) {
				m_Container_Track.FixChildcountImmediately(trackCount);
			} else if (m_Container_Track.childCount < trackCount) {
				for (int i = m_Container_Track.childCount; i < trackCount; i++) {
					var grab = Instantiate(m_TrackPrefab, m_Container_Track);
					grab.transform.SetAsLastSibling();
					grab.Grab<Button>().onClick.AddListener(() => SelectTrack(grab.transform.GetSiblingIndex()));
					grab.Grab<Text>("Label").text = i.ToString("00");
					grab.Grab<TriggerUI>().CallbackRight.AddListener(() => OpenItemMenu(grab.transform as RectTransform, 1));
				}
			}

			// Highlight
			int selectingType = GetSelectionType();
			int selectingIndex = GetSelectionIndex();
			bool active = selectingIndex >= 0 && ((selectingType == 0 && selectingIndex < stageCount) || (selectingType == 1 && selectingIndex < trackCount));
			if (m_Highlight.gameObject.activeSelf != active) {
				m_Highlight.gameObject.SetActive(active);
			}
			if (active) {
				RectTransform target;
				if (selectingType == 0) {
					target = m_Container_Stage.GetChild(selectingIndex) as RectTransform;
				} else {
					target = m_Container_Track.GetChild(selectingIndex) as RectTransform;
				}
				m_Highlight.position = target.position;
				m_Highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target.rect.width);
				m_Highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target.rect.height);
			}
		}


		#endregion



	}
}