namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UI;


	public class StageInspectorUI : MonoBehaviour {


		// Api
		public InputField TimeIF => m_TimeIF;
		public BeatInputUI BeatIF => m_BeatIF;
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
		public Button PivotButton_Top => m_PivotButton_Top;
		public Button PivotButton_Mid => m_PivotButton_Mid;
		public Button PivotButton_Bottom => m_PivotButton_Bottom;
		public Text[] LanguageLabels => m_LanguageLabels;

		// Ser
		[SerializeField] private InputField m_TimeIF = null;
		[SerializeField] private BeatInputUI m_BeatIF = null;
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
		[SerializeField] private Button m_PivotButton_Top = null;
		[SerializeField] private Button m_PivotButton_Mid = null;
		[SerializeField] private Button m_PivotButton_Bottom = null;
		[SerializeField] private Text[] m_LanguageLabels = null;


		// API
		public float GetTime () => m_TimeIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetBeat () => m_BeatIF.GetBeat();
		public int GetItemType () => m_TypeIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;
		public float GetDuration () => m_DurationIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetSpeed () => m_SpeedIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 1f;
		public float GetPivot () => m_PivotIF.text.TryParseFloatForInspector(out float result) ? result : 0f;
		public float GetPosX () => m_PosXIF.text.TryParseFloatForInspector(out float result) ? result : 0f;
		public float GetPosY () => m_PosYIF.text.TryParseFloatForInspector(out float result) ? result : 0f;
		public float GetRot () => m_RotIF.text.TryParseFloatForInspector(out float result) ? result : 0f;
		public float GetWidth () => m_WidthIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public float GetHeight () => m_HeightIF.text.TryParseFloatForInspector(out float result) ? Mathf.Max(result, 0f) : 0f;
		public int GetColor () => m_ColorIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;

		public void SetTime (float value) => m_TimeIF.text = value.ToString();
		public void SetBeat (float value) => m_BeatIF.SetBeatToUI(value);
		public void SetItemType (int value) => m_TypeIF.text = value.ToString();
		public void SetDuration (float value) => m_DurationIF.text = value.ToString();
		public void SetSpeed (float value) => m_SpeedIF.text = value.ToString();
		public void SetPivot (float value) => m_PivotIF.text = value.ToString();
		public void SetPosX (float value) => m_PosXIF.text = value.ToString();
		public void SetPosY (float value) => m_PosYIF.text = value.ToString();
		public void SetRot (float value) => m_RotIF.text = value.ToString();
		public void SetWidth (float value) => m_WidthIF.text = value.ToString();
		public void SetHeight (float value) => m_HeightIF.text = value.ToString();
		public void SetColor (int value) => m_ColorIF.text = value.ToString();


	}
}