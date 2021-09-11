namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class NoteInspectorUI : MonoBehaviour {


		public delegate string LanguageHandler (string key);
		public static LanguageHandler GetLanguage { get; set; } = null;

		// Api
		public Text[] LanguageLabels => m_LanguageLabels;

		// Ser
		[SerializeField] private InputField m_TimeIF = null;
		[SerializeField] private BeatInputUI m_BeatIF = null;
		[SerializeField] private InputField m_TypeIF = null;
		[SerializeField] private InputField m_DurationIF = null;
		[SerializeField] private InputField m_SpeedIF = null;
		[SerializeField] private InputField m_PosXIF = null;
		[SerializeField] private InputField m_WidthIF = null;
		[SerializeField] private InputField m_IndexIF = null;
		[SerializeField] private InputField m_LinkIF = null;
		[SerializeField] private InputField m_PosZIF = null;
		[SerializeField] private InputField m_ClickIF = null;
		[SerializeField] private InputField m_TimingIDIF = null;
		[SerializeField] private InputField m_SfxParamAIF = null;
		[SerializeField] private InputField m_SfxParamBIF = null;
		[SerializeField] private Text m_SfxLabel = null;
		[SerializeField] private string[] m_SfxLabels = null;
		[SerializeField] private Text[] m_LanguageLabels = null;

		// Data
		private int SfxIndex = 0;

		// API
		public float GetTime () => m_TimeIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetBeat () => m_BeatIF.GetBeat();
		public int GetItemType () => m_TypeIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;
		public float GetDuration () => m_DurationIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetSpeed () => m_SpeedIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 1f;
		public float GetPosX () => m_PosXIF.text.TryParseFloatForInspector(out float result) ? result : 0f;
		public float GetWidth () => m_WidthIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public int GetIndex () => m_IndexIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;
		public int GetLink () => m_LinkIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, -1) : -1;
		public float GetPosZ () => m_PosZIF.text.TryParseFloatForInspector(out float result) ? result : 0;
		public short GetClick () => short.TryParse(m_ClickIF.text, out short result) ? (short)Mathf.Max(result, -1) : (short)0;
		public byte GetTimingID () => byte.TryParse(m_TimingIDIF.text, out byte result) ? (byte)Mathf.Max(result, 0) : (byte)0;
		public byte GetSfx () => (byte)SfxIndex;
		public int GetSfxParamA () => m_SfxParamAIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;
		public int GetSfxParamB () => m_SfxParamBIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;


		public void SetTime (float value) => m_TimeIF.text = value.ToString();
		public void SetBeat (float value) => m_BeatIF.SetBeatToUI(value);
		public void SetItemType (int value) => m_TypeIF.text = value.ToString();
		public void SetDuration (float value) => m_DurationIF.text = value.ToString();
		public void SetSpeed (float value) => m_SpeedIF.text = value.ToString();
		public void SetPosX (float value) => m_PosXIF.text = value.ToString();
		public void SetWidth (float value) => m_WidthIF.text = value.ToString();
		public void SetIndex (int value) => m_IndexIF.text = value.ToString();
		public void SetLink (int value) => m_LinkIF.text = value.ToString();
		public void SetPosZ (float value) => m_PosZIF.text = value.ToString();
		public void SetClick (short value) => m_ClickIF.text = value.ToString();
		public void SetTimingID (byte value) => m_TimingIDIF.text = value.ToString();
		public void SetSfx (int value) {
			if (!gameObject.activeSelf) { return; }
			SfxIndex = value;
			m_SfxLabel.text = GetLanguage(m_SfxLabels[Mathf.Clamp(SfxIndex, 0, m_SfxLabels.Length - 1)]);
		}
		public void SetSfxParamA (int value) => m_SfxParamAIF.text = value.ToString();
		public void SetSfxParamB (int value) => m_SfxParamBIF.text = value.ToString();


	}
}