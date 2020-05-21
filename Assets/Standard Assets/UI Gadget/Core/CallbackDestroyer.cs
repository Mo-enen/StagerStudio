namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	public class CallbackDestroyer : MonoBehaviour {



		// Ser
		[SerializeField] private InputField m_Input = null;
		[SerializeField] private Button m_Button = null;
		[SerializeField] private Toggle m_Toggle = null;
		[SerializeField] private EventTrigger m_Trigger = null;
		[SerializeField] private ScrollRect m_Scroll = null;
		[SerializeField] private Slider m_Slider = null;



		// MSG
		private void Reset () {
			m_Input = GetComponent<InputField>();
			m_Button = GetComponent<Button>();
			m_Toggle = GetComponent<Toggle>();
			m_Trigger = GetComponent<EventTrigger>();
			m_Scroll = GetComponent<ScrollRect>();
			m_Slider = GetComponent<Slider>();
		}


		private void OnDestroy () => RemoveAllListeners();


		// API
		public void RemoveAllListeners () {
			if (m_Input) {
				m_Input.onEndEdit.RemoveAllListeners();
				m_Input.onValueChanged.RemoveAllListeners();
			}
			if (m_Button) {
				m_Button.onClick.RemoveAllListeners();
			}
			if (m_Toggle) {
				m_Toggle.onValueChanged.RemoveAllListeners();
			}
			if (m_Trigger && m_Trigger.triggers != null) {
				foreach (var trigger in m_Trigger.triggers) {
					trigger.callback.RemoveAllListeners();
				}
			}
			if (m_Scroll) {
				m_Scroll.onValueChanged.RemoveAllListeners();
			}
			if (m_Slider) {
				m_Slider.onValueChanged.RemoveAllListeners();
			}
		}



	}
}