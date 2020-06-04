namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class LinkerUI : MonoBehaviour {


		// SUB
		public delegate Beatmap BeatmapHandler ();
		public delegate int IntHandler ();
		public delegate void VoidHandler ();
		public delegate bool BoolHandler ();

		// Handler
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static IntHandler GetSelectingIndex { get; set; } = null;
		public static VoidHandler OnLink { get; set; } = null;
		public static BoolHandler AllowLinker { get; set; } = null;


		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);
		 
		// Ser
		[SerializeField] private SpriteRenderer m_Icon = null;
		[SerializeField] private Transform m_PlaneTarget = null;
		[SerializeField] private Color m_TintA = Color.white;
		[SerializeField] private Color m_TintB = Color.black;

		// Data
		private int StartIndex = -1;
		private Camera _Camera = null;



		// MSG
		private void Update () {
			var map = GetBeatmap();
			if (!AllowLinker() || map != null && StartIndex >= 0 && StartIndex < map.Notes.Count) {
				int index = GetSelectingIndex();
				if (index != StartIndex) {
					if (index >= map.notes.Count) { index = -1; }
					// Do the Link
					map.SetNoteLinkedIndex(StartIndex, index);
					OnLink();
					// Stop
					StopLinker();
				}
				// Movement
				var pos = Util.GetRayPosition(Camera.ScreenPointToRay(Input.mousePosition), m_PlaneTarget);
				if (pos.HasValue) {
					transform.position = pos.Value;
					m_Icon.color = Color.Lerp(m_TintA, m_TintB, Mathf.PingPong(Time.time, 0.6f));
				} else {
					m_Icon.color = Color.clear;
				}
			} else {
				StopLinker();
			}
		}


		// API
		public void TryStartLinker () {
			var map = GetBeatmap();
			if (!AllowLinker() || map == null) { return; }
			if (!gameObject.activeSelf) {
				StartIndex = GetSelectingIndex();
				gameObject.TrySetActive(true);
			} else {
				StopLinker();
			}
		}


		public void StopLinker () {
			StartIndex = -1;
			gameObject.TrySetActive(false);
		}
	}
}