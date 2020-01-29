namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class HintBarUI : MonoBehaviour {


		// VAR
		[SerializeField] private Text m_Text = null;
		[SerializeField] private Animator m_Ani = null;
		[SerializeField] private Image m_BG = null;
		[SerializeField] private Image m_Progress = null;
		[SerializeField] private float m_FlashDuration = 0.5f;
		[SerializeField] private string m_Keyword = "Flash";

		private Coroutine VanishCor = null;


		// API
		public void SetHint (string hint = "") {
			m_Text.text = hint;
			// Flash
			if (!string.IsNullOrEmpty(hint)) {
				CancelInvoke();
				Invoke("FlashLogic", 0f);
				Invoke("FlashEndLogic", m_FlashDuration);
			}
			// Vanish
			if (VanishCor != null) {
				StopCoroutine(VanishCor);
				VanishCor = null;
			}
			if (!string.IsNullOrEmpty(hint)) {
				VanishCor = StartCoroutine(Vanish());
			}
			IEnumerator Vanish () {
				m_Text.gameObject.SetActive(true);
				var color = m_Text.color;
				for (float time01 = 0f; time01 < 1f; time01 += Time.deltaTime * 0.1f) {
					m_Text.color = new Color(color.r, color.g, color.b, 1f - time01);
					yield return new WaitForEndOfFrame();
				}
				m_Text.gameObject.SetActive(false);
				VanishCor = null;
			}
		}


		public void SetProgress (float progress01) {
			m_Progress.gameObject.SetActive(progress01 > 0f && progress01 < 1f);
			m_Progress.fillAmount = Mathf.Clamp01(progress01);
		}


		// LGC
		private void FlashLogic () {
			m_Ani.enabled = true;
			m_BG.gameObject.SetActive(true);
			m_Ani.SetTrigger(m_Keyword);
		}


		private void FlashEndLogic () {
			m_Ani.enabled = false;
			m_BG.gameObject.SetActive(false);
		}



	}
}