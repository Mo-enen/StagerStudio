namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	public class TooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


		// SUB
		public delegate string StringStringHandler (string value);


		// VAR
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static StringStringHandler GetHotKey { get; set; } = null;
		public static Text TipLabel { get; set; } = null;

		[SerializeField] private string m_TipKey = "";
		[SerializeField] private string m_HotKey = "";


		// MSG
		private void OnDisable () => LogTip("");
		public void OnPointerEnter (PointerEventData e) => LogTip(m_TipKey, m_HotKey);
		public void OnPointerExit (PointerEventData e) => LogTip("");


		// API
		public static void LogTip (string key, string hotkey = "") =>
			TipLabel.text = $"{GetLanguage(key)}   {GetHotKey(hotkey)}";


	}
}