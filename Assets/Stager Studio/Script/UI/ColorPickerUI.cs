namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class ColorPickerUI : MonoBehaviour {




		#region --- VAR ---


		// Handler
		public delegate string StringStringHandler (string key);
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Short
		private float Color_H {
			get {
				return m_SliderH.value;
			}
			set {
				m_SliderH.value = value;
			}
		}
		private float Color_S {
			get {
				return m_SliderS.value;
			}
			set {
				m_SliderS.value = value;
			}
		}
		private float Color_V {
			get {
				return m_SliderV.value;
			}
			set {
				m_SliderV.value = value;
			}
		}
		private float Color_A {
			get {
				return m_SliderA.value;
			}
			set {
				m_SliderA.value = value;
			}
		}

		// Ser
		[SerializeField] private Image m_Thumbnail_Old = null;
		[SerializeField] private Image m_Thumbnail_New = null;
		[SerializeField] private Image m_SliderTint_S = null;
		[SerializeField] private Image m_SliderTint_S_Back = null;
		[SerializeField] private Image m_SliderTint_V = null;
		[SerializeField] private Image m_SliderTint_A = null;
		[SerializeField] private Slider m_SliderH = null;
		[SerializeField] private Slider m_SliderS = null;
		[SerializeField] private Slider m_SliderV = null;
		[SerializeField] private Slider m_SliderA = null;
		[SerializeField] private InputField m_FieldH = null;
		[SerializeField] private InputField m_FieldS = null;
		[SerializeField] private InputField m_FieldV = null;
		[SerializeField] private InputField m_FieldA = null;
		[SerializeField] private Text[] m_LanguageTexts = null;

		// Data
		private System.Action<Color> Done = null;
		private bool UIReady = true;


		#endregion




		#region --- MSG ---


		private void Awake () {
			foreach (var tx in m_LanguageTexts) {
				tx.text = GetLanguage(tx.name);
			}
			RefreshUI();
		}


		#endregion




		#region --- API ---


		public void Init (Color color, System.Action<Color> done) {
			Done = done;
			m_Thumbnail_Old.color = color;
			Color.RGBToHSV(color, out float h, out float s, out float v);
			Color_H = h;
			Color_S = s;
			Color_V = v;
			Color_A = color.a;
		}


		// UI
		public void UI_Done (bool save) {
			// Callback
			if (save && !(Done is null)) {
				Done(GetColorFromUI());
			}
			Done = null;
			// Close
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			DestroyImmediate(gameObject, false);
		}


		public void RefreshUI () {
			if (!UIReady) { return; }
			UIReady = false;
			try {
				var tint = GetColorFromUI();
				m_Thumbnail_New.color = tint;
				m_SliderTint_S.color = Color.HSVToRGB(Color_H, 1f, Mathf.Lerp(0.4f, 1f, Color_V));
				m_SliderTint_S_Back.color = Color.HSVToRGB(Color_H, 0f, Mathf.Lerp(0.4f, 1f, Color_V));
				m_SliderTint_V.color = Color.HSVToRGB(Color_H, Color_S, 1f);
				m_SliderTint_A.color = tint;
				m_SliderH.value = Mathf.Clamp01(m_SliderH.value);
				m_FieldH.text = Mathf.RoundToInt(Mathf.Repeat(Color_H * 360f, 360f)).ToString();
				m_FieldS.text = Mathf.RoundToInt(Mathf.Clamp(Color_S * 100f, 0, 100)).ToString();
				m_FieldV.text = Mathf.RoundToInt(Mathf.Clamp(Color_V * 100f, 0, 100)).ToString();
				m_FieldA.text = Mathf.RoundToInt(Mathf.Clamp(Color_A * 100f, 0, 100)).ToString();
			} catch { }
			UIReady = true;
		}


		public void FieldToSlider () {
			UIReady = false;
			try {
				if (int.TryParse(m_FieldH.text, out int valueH)) {
					m_SliderH.value = valueH / 360f;
				}
				if (int.TryParse(m_FieldS.text, out int valueS)) {
					m_SliderS.value = valueS / 100f;
				}
				if (int.TryParse(m_FieldV.text, out int valueV)) {
					m_SliderV.value = valueV / 100f;
				}
				if (int.TryParse(m_FieldA.text, out int valueA)) {
					m_SliderA.value = valueA / 100f;
				}
			} catch { }
			UIReady = true;
		}


		#endregion




		#region --- LGC ---


		private Color GetColorFromUI () {
			var c = Color.HSVToRGB(Color_H, Color_S, Color_V);
			c.a = Color_A;
			return c;
		}


		#endregion




	}
}