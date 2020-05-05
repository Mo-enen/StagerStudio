namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;


	public class ColliderTriggerUI : MonoBehaviour {



		// VAR
		public UnityEvent CallbackLeft => m_CallbackLeft;
		public UnityEvent CallbackDrag => m_CallbackDrag;

		// Api
		public bool Entering { get; private set; } = false;

		// Short
		private static Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private UnityEvent m_CallbackLeft = null;
		[SerializeField] private UnityEvent m_CallbackDrag = null;
		[SerializeField] private Transform m_Highlight = null;
		[SerializeField] private LayerMask m_AxisMask = 0;

		// Data
		private static Camera _Camera = null;
		private bool Dragging = false;


		// MSG
		private void OnDisable () {
			if (m_Highlight != null) {
				m_Highlight.gameObject.SetActive(false);
				Entering = false;
				Dragging = false;
			}
		}


		private void Update () {
			// Hover Check
			Entering = Physics.Raycast(
				Camera.ScreenPointToRay(Input.mousePosition),
				out RaycastHit hit, float.MaxValue, m_AxisMask.value
			) && hit.transform == transform;
			if (Entering) {
				if (Input.GetMouseButtonDown(0)) {
					CallbackLeft?.Invoke();
					Dragging = true;
				}
			}
			if (Dragging) {
				if (Input.GetMouseButton(0)) {
					CallbackDrag?.Invoke();
				} else {
					Dragging = false;
				}
			}
			// Highlight
			if (m_Highlight != null) {
				bool highlight = Entering && !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
				if (m_Highlight.gameObject.activeSelf != highlight) {
					m_Highlight.gameObject.SetActive(highlight);
				}
			}
		}



	}
}