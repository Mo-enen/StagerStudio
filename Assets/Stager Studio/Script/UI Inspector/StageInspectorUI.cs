namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class StageInspectorUI : MonoBehaviour {


		// Api
		public InputField TimeIF => m_TimeIF;
		public InputField BeatIF => m_BeatIF;
		public InputField TypeIF => m_TypeIF;
		public InputField DurationIF => m_DurationIF;
		public InputField SpeedIF => m_SpeedIF;
		public InputField PivotIF => m_PivotIF;
		public InputField PosXIF => m_PosXIF;
		public InputField PosYIF => m_PosYIF;
		public InputField RotIF => m_RotIF;
		public InputField WidthIF => m_WidthIF;
		public InputField HeightIF => m_HeightIF;
		public InputField ColorIF => m_ColorIF;
		public Text[] LanguageLabels => m_LanguageLabels;

		// Ser
		[SerializeField] private InputField m_TimeIF = null;
		[SerializeField] private InputField m_BeatIF = null;
		[SerializeField] private InputField m_TypeIF = null;
		[SerializeField] private InputField m_DurationIF = null;
		[SerializeField] private InputField m_SpeedIF = null;
		[SerializeField] private InputField m_PivotIF = null;
		[SerializeField] private InputField m_PosXIF = null;
		[SerializeField] private InputField m_PosYIF = null;
		[SerializeField] private InputField m_RotIF = null;
		[SerializeField] private InputField m_WidthIF = null;
		[SerializeField] private InputField m_HeightIF = null;
		[SerializeField] private InputField m_ColorIF = null;
		[SerializeField] private Text[] m_LanguageLabels = null;


		// API
		public float GetTime () => float.TryParse(m_TimeIF.text, out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetBeat () => float.TryParse(m_BeatIF.text, out float result) ? result : 0f;
		public int GetItemType () => int.TryParse(m_TypeIF.text, out int result) ? Mathf.Max(result, 0) : 0;
		public float GetDuration () => float.TryParse(m_DurationIF.text, out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetSpeed () => float.TryParse(m_SpeedIF.text, out float result) ? Mathf.Max(result, 0f) : 1f;
		public float GetPivot () => float.TryParse(m_PivotIF.text, out float result) ? result : 0f;
		public float GetPosX () => float.TryParse(m_PosXIF.text, out float result) ? result : 0f;
		public float GetPosY () => float.TryParse(m_PosYIF.text, out float result) ? result : 0f;
		public float GetRot () => float.TryParse(m_RotIF.text, out float result) ? result : 0f;
		public float GetWidth () => float.TryParse(m_WidthIF.text, out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetHeight () => float.TryParse(m_HeightIF.text, out float result) ? Mathf.Max(result, 0f) : 0f;
		public int GetColor () => int.TryParse(m_ColorIF.text, out int result) ? Mathf.Max(result, 0) : 0;

		public void SetTime (float value) => m_TimeIF.text = value.ToString("0.###");
		public void SetBeat (float value) => m_BeatIF.text = value.ToString("0.###");
		public void SetItemType (int value) => m_TypeIF.text = value.ToString();
		public void SetDuration (float value) => m_DurationIF.text = value.ToString("0.###");
		public void SetSpeed (float value) => m_SpeedIF.text = value.ToString("0.###");
		public void SetPivot (float value) => m_PivotIF.text = value.ToString("0.###");
		public void SetPosX (float value) => m_PosXIF.text = value.ToString("0.###");
		public void SetPosY (float value) => m_PosYIF.text = value.ToString("0.###");
		public void SetRot (float value) => m_RotIF.text = value.ToString("0.###");
		public void SetWidth (float value) => m_WidthIF.text = value.ToString("0.###");
		public void SetHeight (float value) => m_HeightIF.text = value.ToString("0.###");
		public void SetColor (int value) => m_ColorIF.text = value.ToString();


	}
}