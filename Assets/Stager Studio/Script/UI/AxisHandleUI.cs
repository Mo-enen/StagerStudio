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
		public bool Hovering => m_TriggerX.Entering || m_TriggerY.Entering || m_TriggerXY.Entering;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private ColliderTriggerUI m_TriggerX = null;
		[SerializeField] private ColliderTriggerUI m_TriggerY = null;
		[SerializeField] private ColliderTriggerUI m_TriggerXY = null;
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
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var downPos = Util.GetRayPosition(GetMouseRay(), zoneMin, zoneMax, transform, false);
			if (downPos.HasValue) {
				MouseDown = downPos.Value;
				m_OnDrag.Invoke(downPos.Value, downPos, axis);
			} else {
				MouseDown = null;
			}
		}


		public void OnAxisDrag (int axis) { // 0:x  1:y  2:xy
			if (m_OnDrag == null || !MouseDown.HasValue) { return; }
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var mousePos = Util.GetRayPosition(GetMouseRay(), zoneMin, zoneMax, transform, false);
			if (mousePos.HasValue) {
				m_OnDrag.Invoke(mousePos.Value, null, axis);
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