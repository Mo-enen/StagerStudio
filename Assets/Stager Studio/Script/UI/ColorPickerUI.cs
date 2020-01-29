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
		[SerializeField] private Text[] m_LanguageTexts = null;

		// Data
		private System.Action<Color> Done = null;


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
			var tint = GetColorFromUI();
			m_Thumbnail_New.color = tint;
			m_SliderTint_S.color = Color.HSVToRGB(Color_H, 1f, Mathf.Lerp(0.4f, 1f, Color_V));
			m_SliderTint_S_Back.color = Color.HSVToRGB(Color_H, 0f, Mathf.Lerp(0.4f, 1f, Color_V));
			m_SliderTint_V.color = Color.HSVToRGB(Color_H, Color_S, 1f);
			m_SliderTint_A.color = tint;
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