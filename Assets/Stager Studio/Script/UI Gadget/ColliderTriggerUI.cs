namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;


	public class ColliderTriggerUI : MonoBehaviour {



		// VAR
		public UnityEvent CallbackLeft => m_CallbackLeft;
		public UnityEvent CallbackRight => m_CallbackRight;
		public UnityEvent CallbackDrag => m_CallbackDrag;

		// Api
		public bool Entering = false;

		// Ser
		[SerializeField] private UnityEvent m_CallbackLeft = null;
		[SerializeField] private UnityEvent m_CallbackRight = null;
		[SerializeField] private UnityEvent m_CallbackDrag = null;
		[SerializeField] private Transform m_Highlight = null;



		// MSG
		private void OnDisable () {
			if (m_Highlight != null) {
				m_Highlight.gameObject.SetActive(false);
				Entering = false;
			}
		}


		private void Update () {
			if (m_Highlight != null) {
				bool highlight = Entering && !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
				if (m_Highlight.gameObject.activeSelf != highlight) {
					m_Highlight.gameObject.SetActive(highlight);
				}
			}
		}


		private void OnMouseDown () {
			if (Input.GetMouseButton(0)) {
				CallbackLeft?.Invoke();
			} else if (Input.GetMouseButton(1)) {
				CallbackRight?.Invoke();
			}
		}
		private void OnMouseDrag () => CallbackDrag?.Invoke();
		private void OnMouseEnter () => Entering = true;
		private void OnMouseExit () => Entering = false;


	}
}