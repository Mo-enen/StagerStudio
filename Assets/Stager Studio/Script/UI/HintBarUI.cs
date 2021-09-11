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

		// Data
		private Coroutine VanishCor = null;
		private Coroutine FlashCor = null;


		// API
		public void SetHint (string hint = "", bool flash = true) {
			SetLabelText(hint);
			// Flash
			if (flash && !string.IsNullOrEmpty(hint)) {
				if (FlashCor != null) {
					StopCoroutine(FlashCor);
					FlashCor = null;
				}
				FlashCor = StartCoroutine(Flash());
			}
			// Vanish
			if (VanishCor != null) {
				StopCoroutine(VanishCor);
				SetLabelText("");
				VanishCor = null;
			}
			if (!string.IsNullOrEmpty(hint)) {
				VanishCor = StartCoroutine(Vanish());
			}
			IEnumerator Vanish () {
				for (float time01 = 0f; time01 < 1f; time01 += Time.deltaTime * 0.1f) {
					SetLabelText(hint);
					SetLabelAlpha(1f - time01);
					yield return new WaitForEndOfFrame();
				}
				SetLabelText("");
				SetLabelAlpha(1f);
				VanishCor = null;
			}
			IEnumerator Flash () {
				m_Ani.enabled = true;
				m_BG.gameObject.SetActive(true);
				m_Ani.SetTrigger(m_Keyword);
				yield return new WaitForSeconds(m_FlashDuration);
				m_Ani.enabled = false;
				m_BG.gameObject.SetActive(false);
				FlashCor = null;
			}
		}


		public void SetProgress (float progress01) {
			m_Progress.gameObject.SetActive(progress01 > 0f && progress01 < 1f);
			m_Progress.fillAmount = Mathf.Clamp01(progress01);
		}


		// LGC
		private void SetLabelText (string text) => m_Text.text = text;


		private void SetLabelAlpha (float a) {
			var color = m_Text.color;
			color.a = a;
			m_Text.color = color;
		}


	}
}