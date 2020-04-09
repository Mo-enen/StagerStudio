namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	public class TooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


		// SUB
		public delegate string StringStringHandler (string value);
		public delegate void VoidStringHandler (string str);


		// VAR
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static StringStringHandler GetHotKey { get; set; } = null;
		public static VoidStringHandler SetTip { get; set; } = null;

		[SerializeField] private string m_TipKey = "";
		[SerializeField] private string m_HotKey = "";


		// MSG
		private void OnDisable () => LogTip("");
		public void OnPointerEnter (PointerEventData e) => LogTip(m_TipKey, m_HotKey);
		public void OnPointerExit (PointerEventData e) => LogTip("");


		// API
		public static void LogTip (string key, string hotkey = "") => SetTip($"{GetLanguage(key)}   {GetHotKey(hotkey)}");


	}
}