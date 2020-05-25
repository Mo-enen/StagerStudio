namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;


	[AddComponentMenu("UIGadget/TimeBomb")]
	public class TimeBomb : MonoBehaviour {


		// SUB
		[System.Serializable] public class BlowUpEvent : UnityEvent { }

		// Api
		public BlowUpEvent OnBlowUp => m_OnBlowUp;

		// Ser
		[SerializeField] private bool m_LitOnStart = true;
		[SerializeField] private bool m_DestroyOnBlowUp = true;
		[SerializeField] private bool m_BlowUpOnDestroy = false;
		[SerializeField] private float m_Duration = 1f;
		[SerializeField] private BlowUpEvent m_OnBlowUp = null;

		// Data
		private float? BlowUpTime = null;



		// MSG
		private void Start () {
			if (m_LitOnStart) {
				Lit();
			}
		}


		private void Update () {
			if (BlowUpTime.HasValue && Time.time >= BlowUpTime) {
				BlowUp();
				BlowUpTime = null;
			}
		}


		private void OnDestroy () {
			if (m_BlowUpOnDestroy) {
				OnBlowUp?.Invoke();
			}
		}


		// API
		public void Lit () => BlowUpTime = Time.time + m_Duration;


		public void BlowUp () {
			OnBlowUp?.Invoke();
			if (m_DestroyOnBlowUp && gameObject) {
				Destroy(gameObject);
			}
		}


	}
}