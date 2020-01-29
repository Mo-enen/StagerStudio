namespace StagerStudio.UGUIPlus {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;


	[DisallowMultipleComponent]
	[AddComponentMenu("uGUI Plus/Pointer Event Delegator")]
	public class PointerEventDelegator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {


		// SUB
		public delegate void PointerEventHandler (PointerEventData eventData);



		// Short Cut
		public PointerEventHandler PointerEnter {
			get {
				return m_PointerEnter;
			}

			set {
				m_PointerEnter = value;
			}
		}

		public PointerEventHandler PointerExit {
			get {
				return m_PointerExit;
			}

			set {
				m_PointerExit = value;
			}
		}

		public PointerEventHandler PointerDown {
			get {
				return m_PointerDown;
			}

			set {
				m_PointerDown = value;
			}
		}

		public PointerEventHandler PointerUp {
			get {
				return m_PointerUp;
			}

			set {
				m_PointerUp = value;
			}
		}

		public PointerEventHandler PointerClick {
			get {
				return m_PointerClick;
			}

			set {
				m_PointerClick = value;
			}
		}


		// Data
		private PointerEventHandler m_PointerEnter;
		private PointerEventHandler m_PointerExit;
		private PointerEventHandler m_PointerDown;
		private PointerEventHandler m_PointerUp;
		private PointerEventHandler m_PointerClick;



		// API
		public void OnPointerEnter (PointerEventData eventData) {
			if (m_PointerEnter != null) {
				m_PointerEnter.Invoke(eventData);
			}
		}


		public void OnPointerExit (PointerEventData eventData) {
			if (m_PointerExit != null) {
				m_PointerExit.Invoke(eventData);
			}
		}


		public void OnPointerDown (PointerEventData eventData) {
			if (m_PointerDown != null) {
				m_PointerDown.Invoke(eventData);
			}
		}


		public void OnPointerUp (PointerEventData eventData) {
			if (m_PointerUp != null) {
				m_PointerUp.Invoke(eventData);
			}
		}


		public void OnPointerClick (PointerEventData eventData) {
			if (m_PointerClick != null) {
				m_PointerClick.Invoke(eventData);
			}
		}


	}
}
