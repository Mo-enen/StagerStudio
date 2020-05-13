namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class TimingInspectorUI : MonoBehaviour {

		// Api
		public InputField TimeIF => m_TimeIF;
		public BeatInputUI BeatIF => m_BeatIF;
		public InputField DurationIF => m_DurationIF;
		public InputField SpeedIF => m_SpeedIF;
		public InputField SfxIF => m_SfxIF;
		public InputField SfxParamAIF => m_SfxParamAIF;
		public InputField SfxParamBIF => m_SfxParamBIF;
		public Text[] LanguageLabels => m_LanguageLabels;

		// Ser
		[SerializeField] private InputField m_TimeIF = null;
		[SerializeField] private BeatInputUI m_BeatIF = null;
		[SerializeField] private InputField m_DurationIF = null;
		[SerializeField] private InputField m_SpeedIF = null;
		[SerializeField] private InputField m_SfxIF = null;
		[SerializeField] private InputField m_SfxParamAIF = null;
		[SerializeField] private InputField m_SfxParamBIF = null;
		[SerializeField] private Text[] m_LanguageLabels = null;


		// API
		public float GetTime () => m_TimeIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetBeat () => m_BeatIF.GetBeat();
		public int GetSpeed () => m_SpeedIF.text.TryParseIntForInspector(out int result) ? Mathf.Clamp(result, -51200, 51200) : 100;
		public float GetDuration () => m_DurationIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public byte GetSfx () => m_SfxIF.text.TryParseIntForInspector(out int result) ? (byte)Mathf.Max(result, 0) : (byte)0;
		public int GetSfxParamA () => m_SfxParamAIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;
		public int GetSfxParamB () => m_SfxParamBIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;


		public void SetTime (float value) => m_TimeIF.text = value.ToString();
		public void SetBeat (float value) => m_BeatIF.SetBeatToUI(value);
		public void SetDuration (float value) => m_DurationIF.text = value.ToString();
		public void SetSpeed (int value) => m_SpeedIF.text = value.ToString();
		public void SetSfx (byte value) => m_SfxIF.text = value.ToString();
		public void SetSfxParamA (int value) => m_SfxParamAIF.text = value.ToString();
		public void SetSfxParamB (int value) => m_SfxParamBIF.text = value.ToString();

	}
}