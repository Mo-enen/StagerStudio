namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class BeatmapInspectorUI : MonoBehaviour {


		// Api
		public InputField BpmIF => m_BpmIF;
		public InputField ShiftIF => m_ShiftIF;
		public InputField RatioIF => m_RatioIF;
		public InputField LevelIF => m_LevelIF;
		public InputField TagIF => m_TagIF;
		public Text[] LanguageLabels => m_LanguageLabels;

		// Ser
		[SerializeField] private InputField m_BpmIF = null;
		[SerializeField] private InputField m_ShiftIF = null;
		[SerializeField] private InputField m_RatioIF = null;
		[SerializeField] private InputField m_LevelIF = null;
		[SerializeField] private InputField m_TagIF = null;
		[SerializeField] private Text[] m_LanguageLabels = null;


		// API
		public int GetBpm () => int.TryParse(m_BpmIF.text, out int result) ? Mathf.Max(result, 0) : 0;
		public int GetShift () => int.TryParse(m_ShiftIF.text, out int result) ? result : 0;
		public float GetRatio () => float.TryParse(m_RatioIF.text, out float result) ? Mathf.Max(result, 0.1f) : 0f;
		public int GetLevel () => int.TryParse(m_LevelIF.text, out int result) ? Mathf.Max(result, 0) : 0;
		public string GetTag () => m_TagIF.text;

		public void SetBpm (int value) => m_BpmIF.text = value.ToString();
		public void SetShift (int value) => m_ShiftIF.text = value.ToString();
		public void SetRatio (float value) => m_RatioIF.text = value.ToString("0.##");
		public void SetLevel (float value) => m_LevelIF.text = value.ToString();
		public void SetTag (string value) => m_TagIF.text = value;


	}
}