namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using Rendering;
	using UIGadget;


	public class AxisHandleUI : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable] public class AxisEventHandler : UnityEvent<Vector3, Vector3, int> { }
		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();


		#endregion




		#region --- VAR ---


		// Api
		public static ZoneHandler GetZoneMinMax { get; set; } = null;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);
		private TriggerCollider[] Triggers => _Triggers != null && _Triggers.Length > 0 ? _Triggers : (_Triggers = GetComponentsInChildren<TriggerCollider>(true));

		// Ser
		[SerializeField] private TextRenderer m_Hint = null;
		[SerializeField] private Renderer[] AxisRenderers = null;
		[SerializeField] private Color[] m_HintTints = null;
		[SerializeField] private float m_TintBgAlpha = 0.12f;
		[Space, SerializeField] private AxisEventHandler m_OnDrag = null;

		// Data
		private TriggerCollider[] _Triggers = null;
		private Camera _Camera = null;
		private Vector3? MouseDown = null;


		#endregion




		#region --- MSG ---


		private void Update () {
			if (!Input.GetMouseButton(0)) {
				if (MouseDown.HasValue) {
					MouseDown = null;
				}
				m_Hint.RendererEnable = false;
			}
		}


		public void OnAxisDown (int axis) { // 0:x  1:y  2:xy  3: width
			if (m_OnDrag == null) { return; }
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var downPos = Util.GetRayPosition(GetMouseRay(), zoneMin, zoneMax, transform, false);
			if (downPos.HasValue) {
				MouseDown = downPos.Value;
				m_OnDrag.Invoke(downPos.Value, downPos.Value, -axis - 1);
			} else {
				MouseDown = null;
			}
		}


		public void OnAxisDrag (int axis) { // 0:x  1:y  2:xy  3: width
			if (m_OnDrag == null || !MouseDown.HasValue) { return; }
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var mousePos = Util.GetRayPosition(GetMouseRay(), zoneMin, zoneMax, transform, false);
			if (mousePos.HasValue) {
				m_OnDrag.Invoke(mousePos.Value, MouseDown.Value, axis + 1);
			}
		}


		#endregion




		#region --- API ---


		public bool GetEntering () {
			foreach (var t in Triggers) {
				if (t.Entering) { return true; }
			}
			return false;
		}


		public void LogAxisMessage (int axis, string hint) {
			m_Hint.RendererEnable = true;
			m_Hint.Text = $" {hint} ";
			if (axis >= 0 && axis < m_HintTints.Length) {
				var tint = m_HintTints[axis];
				m_Hint.Tint = tint;
				tint.a = m_TintBgAlpha;
				m_Hint.Background = tint;
			}
		}


		public void SetAxisRendererActive (bool active) {
			foreach (var renderer in AxisRenderers) {
				if (renderer.enabled != active) {
					renderer.enabled = active;
				}
			}
		}


		#endregion




		#region --- LGC ---


		private Ray GetMouseRay () => Camera.ScreenPointToRay(Input.mousePosition);


		#endregion




	}
}