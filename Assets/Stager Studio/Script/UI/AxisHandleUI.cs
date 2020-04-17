namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;



	public class AxisHandleUI : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable] public class AxisEventHandler : UnityEvent<Vector3, Vector3?, int> { }
		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();


		#endregion



		#region --- VAR ---


		// Api
		public static ZoneHandler GetZoneMinMax { get; set; } = null;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private AxisEventHandler m_OnDrag = null;

		// Data
		private Camera _Camera = null;
		private Vector3? MouseDown = null;


		#endregion




		#region --- MSG ---


		private void Update () {
			if (MouseDown.HasValue && !Input.GetMouseButton(0) && !Input.GetMouseButton(1)) {
				MouseDown = null;
			}
		}


		public void OnAxisDown (int axis) {
			if (m_OnDrag == null) { return; }
			var ray = GetMouseRay();
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var downPos = Util.GetRayPosition(ray, zoneMin, zoneMax, null, false);
			if (downPos.HasValue) {
				MouseDown = downPos.Value;
				m_OnDrag.Invoke(downPos.Value, null, axis);
			} else {
				MouseDown = null;
			}
		}


		public void OnAxisDrag (int axis) { // 0:x  1:y  2:xy
			if (m_OnDrag == null || !MouseDown.HasValue) { return; }
			var ray = GetMouseRay();
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var mousePos = Util.GetRayPosition(ray, zoneMin, zoneMax, null, false);
			if (mousePos.HasValue) {
				m_OnDrag.Invoke(mousePos.Value, MouseDown.Value, axis);
			}
		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---


		private Ray GetMouseRay () => Camera.ScreenPointToRay(Input.mousePosition);


		#endregion




	}
}