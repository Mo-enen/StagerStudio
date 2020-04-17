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

		// Ser
		[SerializeField] private UnityEvent m_CallbackLeft = null;
		[SerializeField] private UnityEvent m_CallbackRight = null;
		[SerializeField] private UnityEvent m_CallbackDrag = null;
		[SerializeField] private Transform m_Highlight = null;


		// MSG
		private void OnDisable () {
			if (m_Highlight != null) {
				m_Highlight.gameObject.SetActive(false);
			}
		}



		private void OnMouseDown () {
			if (Input.GetMouseButton(0)) {
				CallbackLeft?.Invoke();
			} else if (Input.GetMouseButton(1)) {
				CallbackRight?.Invoke();
			}
		}


		private void OnMouseDrag () {
			CallbackDrag?.Invoke();
		}


		private void OnMouseEnter () {
			if (m_Highlight != null) {
				m_Highlight.gameObject.SetActive(true);
			}
		}



		private void OnMouseExit () {
			if (m_Highlight != null) {
				m_Highlight.gameObject.SetActive(false);
			}
		}


	}
}