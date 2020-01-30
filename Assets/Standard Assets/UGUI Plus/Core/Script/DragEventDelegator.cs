namespace UGUIPlus {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;


	[DisallowMultipleComponent]
	[AddComponentMenu("uGUI Plus/Drag Event Delegator")]
	public class DragEventDelegator : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler {


		// SUB
		public delegate void DragEventHandler (PointerEventData eventData);



		// Short Cut
		public DragEventHandler Drag {
			get {
				return m_OnDrag;
			}

			set {
				m_OnDrag = value;
			}
		}

		public DragEventHandler BeginDrag {
			get {
				return m_OnBeginDrag;
			}

			set {
				m_OnBeginDrag = value;
			}
		}

		public DragEventHandler EndDrag {
			get {
				return m_OnEndDrag;
			}

			set {
				m_OnEndDrag = value;
			}
		}

		public DragEventHandler InitializePotentialDrag {
			get {
				return m_OnInitializePotentialDrag;
			}

			set {
				m_OnInitializePotentialDrag = value;
			}
		}


		// Data
		private DragEventHandler m_OnDrag;
		private DragEventHandler m_OnBeginDrag;
		private DragEventHandler m_OnEndDrag;
		private DragEventHandler m_OnInitializePotentialDrag;



		// API
		public void OnDrag (PointerEventData eventData) {
			if (m_OnDrag != null) {
				m_OnDrag.Invoke(eventData);
			}
		}


		public void OnBeginDrag (PointerEventData eventData) {
			if (m_OnBeginDrag != null) {
				m_OnBeginDrag.Invoke(eventData);
			}
		}


		public void OnEndDrag (PointerEventData eventData) {
			if (m_OnEndDrag != null) {
				m_OnEndDrag.Invoke(eventData);
			}
		}


		public void OnInitializePotentialDrag (PointerEventData eventData) {
			if (m_OnInitializePotentialDrag != null) {
				m_OnInitializePotentialDrag.Invoke(eventData);
			}
		}



	}
}
