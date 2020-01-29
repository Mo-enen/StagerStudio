namespace StagerStudio.UI {
	using UnityEngine;
	using UnityEngine.UI;
	public class TransactionUI : MonoBehaviour {
		[SerializeField] private Image m_Background = null;
		[SerializeField] private float m_Duration = 1f;
		private float AnimationTime = 0f;
		private Color BasicColor = Color.white;
		private void Awake () {
			m_Background.enabled = true;
			BasicColor = m_Background.color;
		}
		void Update () {
			m_Background.color = Color.Lerp(BasicColor, Color.clear, AnimationTime / m_Duration);
			AnimationTime += Mathf.Min(Time.deltaTime, 0.05f);
			if (AnimationTime > m_Duration) {
				Destroy(gameObject);
			}
		}
	}
}