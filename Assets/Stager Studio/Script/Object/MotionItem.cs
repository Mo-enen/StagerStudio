namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Object;
	using Data;
	using Rendering;
	using UIGadget;



	public class MotionItem : MonoBehaviour {


		// SUB
		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate float FloatHandler ();
		public delegate int IntHandler ();
		public delegate void VoidHandler ();


		// Handler
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static VoidHandler OnMotionChanged { get; set; } = null;
		public static IntHandler GetPaletteCount { get; set; } = null;

		// Api
		public static (int motionIndex, int handleIndex) SelectingMotion { get; set; } = (-1, -1);
		public int ItemType { get; set; } = -1;
		public int ItemIndex { get; set; } = -1;
		public int MotionType { get; set; } = -1;

		// Ser
		[SerializeField] private float m_LineLength = 0.12f;
		[SerializeField] private TriggerUI_Collider[] m_Sliders = null;
		[SerializeField] private Transform[] m_Handles = null;
		[SerializeField] private Transform[] m_Selections = null;
		[SerializeField] private TextRenderer m_Label = null;



		// MSG
		private void OnEnable () => Update();


		private void Update () {
			var map = GetBeatmap();
			int motionIndex = transform.GetSiblingIndex();
			if (map != null) {
				// Pos
				var (zoneMin, zoneMax, _, _) = GetZoneMinMax();




				transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, 0f, 0f, 0f);
				// Slider
				var (hasA, hasB) = map.GetMotionValue(
					ItemType, ItemIndex, MotionType, motionIndex, out float valueA, out float valueB
				);
				if (hasA) {
					if (MotionType == 0) {
						// Pos
						SetSliderValue(0, valueA);
					} else if (MotionType == 1) {
						// Angle
						SetSliderValue(0, valueA / 360f);
					} else if ((ItemType == 0 && MotionType == 4) || (ItemType == 1 && MotionType == 3)) {
						// Index 0-...
						SetSliderValue(0, Util.Remap(0f, GetPaletteCount() - 1, -1f, 1f, valueA));
					} else {
						// Value 0-1
						SetSliderValue(0, Util.Remap(0f, 1f, -1f, 1f, valueA));
					}
				}
				if (hasB) {
					if (MotionType == 0 && ItemType == 0) {
						// Stage Position Only
						SetSliderValue(1, valueB);
					}
				}
				TrySetActive(m_Sliders[0].transform, hasA);
				TrySetActive(m_Sliders[1].transform, hasB);
			} else {
				// No Map
				TrySetActive(m_Sliders[0].transform, false);
				TrySetActive(m_Sliders[1].transform, false);
			}

			// Selection
			TrySetActive(m_Selections[0], motionIndex == SelectingMotion.motionIndex && SelectingMotion.handleIndex == 0);
			TrySetActive(m_Selections[1], motionIndex == SelectingMotion.motionIndex && SelectingMotion.handleIndex == 1);

			// Label
			if (!Input.GetMouseButton(0)) {
				m_Label.RendererEnable = false;
			}
		}


		public void OnAxisDownOrDrag (int handleIndex) {
			int motionIndex = transform.GetSiblingIndex();
			SelectingMotion = (motionIndex, handleIndex);




			// Label
			m_Label.Text = ""; /////////////////////////////////////////////
			m_Label.RendererEnable = true;
			OnMotionChanged();
		}


		// LGC
		private void SetSliderValue (int handleIndex, float value11) => m_Handles[handleIndex].localPosition = new Vector3(
			Util.Remap(-1f, 1f, -m_LineLength, m_LineLength, Mathf.Clamp(value11, -1f, 1f))
		, 0f, 0f);


		private void TrySetActive (Transform target, bool active) {
			if (target.gameObject.activeSelf != active) {
				target.gameObject.SetActive(active);
			}
		}


	}
}