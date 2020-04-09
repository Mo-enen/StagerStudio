namespace StagerStudio.UI {
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;


	public class TriggerUI : MonoBehaviour, IPointerClickHandler {


		// VAR
		public UnityEvent CallbackLeft => m_CallbackLeft;
		public UnityEvent CallbackRight => m_CallbackRight;
		public UnityEvent CallbackDoubleClick => m_CallbackDoubleClick;

		// Ser
		[SerializeField] private UnityEvent m_CallbackLeft = null;
		[SerializeField] private UnityEvent m_CallbackRight = null;
		[SerializeField] private UnityEvent m_CallbackDoubleClick = null;

		// Data
		private float LastClickTime { get; set; } = float.MinValue;



		// MSG
		public void OnPointerClick (PointerEventData eventData) {
			if (eventData.button == PointerEventData.InputButton.Left) {
				CallbackLeft.Invoke();
				if (Time.time < LastClickTime + 0.4f) {
					CallbackDoubleClick.Invoke();
				}
				LastClickTime = Time.time;
			} else if (eventData.button == PointerEventData.InputButton.Right) {
				CallbackRight.Invoke();
			}
		}



	}
}