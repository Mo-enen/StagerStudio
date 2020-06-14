namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UI;


	public class BrushInspectorUI : MonoBehaviour {



		// Api
		public Text[] LanguageLabels => m_LanguageLabels;
		public int BrushType {
			get => m_TypeSelector.TypeIndex;
			set => m_TypeSelector.TypeIndex = value;
		}

		// Ser
		[SerializeField] private InputField m_WidthIF = null;
		[SerializeField] private InputField m_HeightIF = null;
		[SerializeField] private InputField m_TypeIF = null;
		[SerializeField] private TypeSelectorUI m_TypeSelector = null;
		[SerializeField] private Text[] m_LanguageLabels = null;



		// API
		public int GetWidth1000 () => m_WidthIF.text.TryParseIntForInspector(out int result) ? Mathf.Clamp(result, 0, 1000) : 1000;
		public int GetHeight1000 () => m_HeightIF.text.TryParseIntForInspector(out int result) ? Mathf.Clamp(result, 0, 1000) : 1000;
		public int GetItemType () => m_TypeIF.text.TryParseIntForInspector(out int result) ? Mathf.Max(result, 0) : 0;

		public void SetWidth1000 (int value) => m_WidthIF.text = value.ToString();
		public void SetHeight1000 (int value) => m_HeightIF.text = value.ToString();
		public void SetItemType (int value) => m_TypeIF.text = value.ToString();

		public void ShowHeight (bool show) {
			m_HeightIF.transform.parent.TrySetActive(show);
		}


	}
}