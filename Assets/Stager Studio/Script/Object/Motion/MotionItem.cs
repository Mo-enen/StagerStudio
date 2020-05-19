namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;



	public abstract class MotionItem : MonoBehaviour {



		// SUB
		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate float FloatHandler ();
		public delegate void VoidHandler ();
		public delegate bool BoolHandler ();


		// Handler
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetSpeedMuti { get; set; } = null;
		public static VoidHandler OnMotionChanged { get; set; } = null;
		public static BoolHandler GetGridEnabled { get; set; } = null;

		// Api
		public static int SelectingMotionIndex { get; set; } = -1;
		public int ItemIndex { get; set; } = -1;
		public int MotionType { get; set; } = -1;
		public int IndexCount { get; set; } = 0;

		// Short
		protected Transform Root => transform.GetChild(0);
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private TextRenderer m_Label = null;

		// Data
		private Camera _Camera = null;




		// MSG
		private void LateUpdate () {
			// Label
			if (!Input.GetMouseButton(0)) {
				m_Label.RendererEnable = false;
			}
		}


		// API
		public void OnAxisDownOrDrag () {
			int motionIndex = transform.GetSiblingIndex();
			SelectingMotionIndex = motionIndex;
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var pos = Util.GetRayPosition(Camera.ScreenPointToRay(Input.mousePosition), transform);
			if (pos.HasValue) {
				InvokeAxis(transform.worldToLocalMatrix.MultiplyPoint(pos.Value));
			}
		}



		// ABS
		protected abstract void InvokeAxis (Vector2 localPos);



		// LGC
		protected void SetLabelText (string text) {
			m_Label.Text = $" {text} ";
			m_Label.RendererEnable = true;
		}


	}
}