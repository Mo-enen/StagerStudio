namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class BeatmapInspectorUI : MonoBehaviour {


		// Api
		public Text[] LanguageLabels => m_LanguageLabels;

		// Ser
		[SerializeField] private InputField m_BpmIF = null;
		[SerializeField] private InputField m_ShiftIF = null;
		[SerializeField] private InputField m_RatioIF = null;
		[SerializeField] private InputField m_LevelIF = null;
		[SerializeField] private InputField m_TagIF = null;
		[SerializeField] private Text[] m_LanguageLabels = null;



		// API
		public int GetBpm () => m_BpmIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;
		public int GetShift () => m_ShiftIF.text.TryParseIntForInspector(out int result) ? result : 0;
		public float GetRatio () => m_RatioIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0.1f) : 0f;
		public int GetLevel () => m_LevelIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;
		public string GetTag () => m_TagIF.text;

		public void SetBpm (int value) => m_BpmIF.text = value.ToString();
		public void SetShift (int value) => m_ShiftIF.text = value.ToString();
		public void SetRatio (float value) => m_RatioIF.text = value.ToString();
		public void SetLevel (float value) => m_LevelIF.text = value.ToString();
		public void SetTag (string value) => m_TagIF.text = value;



	}
}