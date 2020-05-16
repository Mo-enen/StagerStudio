namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class KeySetterUI : MonoBehaviour {



		// Const
		private readonly KeyCode[] IGNORE_KEYS = { KeyCode.Delete, KeyCode.Backspace, KeyCode.None };

		// Handler
		public delegate void VoidHandler ();
		public delegate void VoidKeyHandler (KeyCode key);
		public VoidHandler OnSetStart { get; set; } = null;
		public VoidKeyHandler OnSetDone { get; set; } = null;

		// Api
		public string Label {
			get => m_Label.text;
			set => m_Label.text = value;
		}

		// Ser
		[SerializeField] private Text m_Label = null;
		[SerializeField] private Image m_Tint = null;
		[SerializeField] private Color m_BlinkA = default;
		[SerializeField] private Color m_BlinkB = default;

		// Data
		private bool Setting = false;
		private Color Normal = Color.white;
		private float SetStartTime = float.MinValue;


		// MSG
		private void Awake () {
			Normal = m_Tint.color;
		}


		private void OnGUI () {
			if (!Setting || Util.IsTypeing) { return; }
			if (Event.current.isScrollWheel || Event.current.type == EventType.MouseDown) {
				Setting = false;
				Event.current.Use();
				return;
			}
			if (Event.current.type != EventType.KeyDown) { return; }
			var key = Event.current.keyCode;
			foreach (var ignore in IGNORE_KEYS) {
				if (key == ignore) {
					key = KeyCode.None;
					break;
				}
			}
			if (key != KeyCode.None) {
				Label = Util.GetKeyName(key);
				OnSetDone(key);
				Setting = false;
			}
			Event.current.Use();
		}


		private void Update () {
			if (Setting) {
				const float BLINK = 0.618f;
				m_Tint.color = Color.Lerp(
					m_Tint.color,
					(Time.time - SetStartTime) % BLINK > BLINK / 2f ? m_BlinkB : m_BlinkA,
					Time.deltaTime * 8f
				);
			} else {
				m_Tint.color = Normal;
			}
		}


		// API
		public void StartSet () {
			if (!Setting) {
				Setting = true;
				SetStartTime = Time.time;
				OnSetStart();
			}
		}


		public void CancelSet () {
			if (Setting) {
				Setting = false;
			}
		}






	}
}