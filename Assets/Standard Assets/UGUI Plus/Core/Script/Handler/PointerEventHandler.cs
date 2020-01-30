namespace UGUIPlus {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;


	[System.Serializable]
	public class PointerEventHandler {

		// SUB
		[System.Serializable]
		public class PointerEvent : UnityEvent<PointerEventData> { }

		[System.Serializable]
		public class BasicEvent : UnityEvent<BaseEventData> { }
		

		// Short Cut
		public PointerEvent OnPointerDownEvent {
			get {
				return m_OnPointerDown;
			}

			set {
				m_OnPointerDown = value;
			}
		}

		public PointerEvent OnPointerUpEvent {
			get {
				return m_OnPointerUp;
			}

			set {
				m_OnPointerUp = value;
			}
		}

		public PointerEvent OnPointerEnterEvent {
			get {
				return m_OnPointerEnter;
			}

			set {
				m_OnPointerEnter = value;
			}
		}

		public PointerEvent OnPointerExitEvent {
			get {
				return m_OnPointerExit;
			}

			set {
				m_OnPointerExit = value;
			}
		}

		public BasicEvent OnSelectEvent {
			get {
				return m_OnSelect;
			}

			set {
				m_OnSelect = value;
			}
		}

		public BasicEvent OnDeselectEvent {
			get {
				return m_OnDeselect;
			}

			set {
				m_OnDeselect = value;
			}
		}

		public PointerEvent OnPointerClickEvent {
			get {
				return m_OnPointerClick;
			}

			set {
				m_OnPointerClick = value;
			}
		}


		// Ser
		[SerializeField] private PointerEvent m_OnPointerDown;
		[SerializeField] private PointerEvent m_OnPointerUp;
		[SerializeField] private PointerEvent m_OnPointerEnter;
		[SerializeField] private PointerEvent m_OnPointerExit;
		[SerializeField] private PointerEvent m_OnPointerClick;
		[SerializeField] private BasicEvent m_OnSelect;
		[SerializeField] private BasicEvent m_OnDeselect;


		public void OnPointerDown (PointerEventData eventData) {
			m_OnPointerDown.Invoke(eventData);
		}


		public void OnPointerUp (PointerEventData eventData) {
			m_OnPointerUp.Invoke(eventData);
		}


		public void OnPointerEnter (PointerEventData eventData) {
			m_OnPointerEnter.Invoke(eventData);
		}


		public void OnPointerExit (PointerEventData eventData) {
			m_OnPointerExit.Invoke(eventData);
		}


		public void OnPointerClick (PointerEventData eventData) {
			m_OnPointerClick.Invoke(eventData);
		}


		public void OnSelect (BaseEventData eventData) {
			m_OnSelect.Invoke(eventData);
		}


		public void OnDeselect (BaseEventData eventData) {
			m_OnDeselect.Invoke(eventData);
		}


		


	}
}