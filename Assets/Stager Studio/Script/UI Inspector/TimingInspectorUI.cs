namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class TimingInspectorUI : MonoBehaviour {

		// Api
		public InputField TimeIF => m_TimeIF;
		public InputField BeatIF => m_BeatIF;
		public InputField DurationIF => m_DurationIF;
		public InputField SpeedIF => m_SpeedIF;
		public InputField SfxIF => m_SfxIF;
		public InputField SfxParamAIF => m_SfxParamAIF;
		public InputField SfxParamBIF => m_SfxParamBIF;
		public Text[] LanguageLabels => m_LanguageLabels;

		// Ser
		[SerializeField] private InputField m_TimeIF = null;
		[SerializeField] private InputField m_BeatIF = null;
		[SerializeField] private InputField m_DurationIF = null;
		[SerializeField] private InputField m_SpeedIF = null;
		[SerializeField] private InputField m_SfxIF = null;
		[SerializeField] private InputField m_SfxParamAIF = null;
		[SerializeField] private InputField m_SfxParamBIF = null;
		[SerializeField] private Text[] m_LanguageLabels = null;


		// API
		public float GetTime () => float.TryParse(m_TimeIF.text, out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetBeat () => float.TryParse(m_BeatIF.text, out float result) ? result : 0f;
		public int GetSpeed () => int.TryParse(m_SpeedIF.text, out int result) ? Mathf.Clamp(result, -51200, 51200) : 100;
		public float GetDuration () => float.TryParse(m_DurationIF.text, out float result) ? Mathf.Max(result, 0f) : 0f;
		public byte GetSfx () => byte.TryParse(m_SfxIF.text, out byte result) ? (byte)Mathf.Max(result, 0) : (byte)0;
		public int GetSfxParamA () => int.TryParse(m_SfxParamAIF.text, out int result) ? Mathf.Max(result, 0) : 0;
		public int GetSfxParamB () => int.TryParse(m_SfxParamBIF.text, out int result) ? Mathf.Max(result, 0) : 0;


		public void SetTime (float value) => m_TimeIF.text = value.ToString();
		public void SetBeat (float value) => m_BeatIF.text = value.ToString();
		public void SetDuration (float value) => m_DurationIF.text = value.ToString();
		public void SetSpeed (int value) => m_SpeedIF.text = value.ToString();
		public void SetSfx (byte value) => m_SfxIF.text = value.ToString();
		public void SetSfxParamA (int value) => m_SfxParamAIF.text = value.ToString();
		public void SetSfxParamB (int value) => m_SfxParamBIF.text = value.ToString();

	}
}