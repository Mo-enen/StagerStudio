namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class LoadingUI : MonoBehaviour {


		// Ser
		[SerializeField] private Text m_Hint = null;
		[SerializeField] private Image m_Progress = null;
		[SerializeField] private Image m_Window = null;

		// Data
		private float Progress01 = 0f;


		// MSG
		private void Update () {
			// BG Alpha
			var c = m_Window.color;
			c.a = Mathf.Lerp(m_Window.color.a, 1f, Time.deltaTime * 12f);
			m_Window.color = c;
			// Progress
			m_Progress.fillAmount = Mathf.Lerp(m_Progress.fillAmount, Progress01, Time.deltaTime * 12f);
		}


		// API
		public void SetProgress (float progress01, string hint = "") {
			bool active = progress01 > 0f && progress01 < 1f;
			transform.parent.gameObject.SetActive(active);
			m_Hint.text = hint;
			Progress01 = Mathf.Clamp01(progress01);
		}


	}
}