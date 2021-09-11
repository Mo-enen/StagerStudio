namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Rendering;



	public class KeypressUI : MonoBehaviour {



		// Ser
		[SerializeField] private Transform m_Container = null;
		[SerializeField] private TextRenderer m_Prefab = null;

		// Data
		private int SortingLayerUI_ID = -1;
		private int PrevSortingOrder = 0;
		private float PrevAddTime = float.MinValue;


		// MSG
		private void Awake () {
			SortingLayerUI_ID = SortingLayer.NameToID("UI");
		}


		private void OnGUI () {
			if (!Util.IsTypeing && Event.current.type == EventType.KeyDown) {
				AddKey(Event.current.keyCode, Event.current.control, Event.current.shift, Event.current.alt);
			}
		}


		// LGC
		private void AddKey (KeyCode key, bool ctrl, bool shift, bool alt) {
			if (
				key == KeyCode.None ||
				key == KeyCode.LeftControl || key == KeyCode.RightControl ||
				key == KeyCode.LeftShift || key == KeyCode.RightShift ||
				key == KeyCode.LeftAlt || key == KeyCode.RightAlt
			) { return; }
			var label = Instantiate(m_Prefab, m_Container);
			label.transform.position = transform.position;
			label.transform.rotation = Quaternion.identity;
			label.transform.localScale = m_Prefab.transform.localScale;
			label.Text = $"{(ctrl ? "ctrl " : "")}{(shift ? "shift " : "")}{(alt ? "alt " : "")}{Util.GetKeyName(key)}";
			if (Time.time > PrevAddTime + 1f) {
				PrevSortingOrder = -1;
			}
			label.SetSortingLayer(SortingLayerUI_ID, PrevSortingOrder + 1);
			PrevSortingOrder++;
			PrevAddTime = Time.time;
		}




	}
}