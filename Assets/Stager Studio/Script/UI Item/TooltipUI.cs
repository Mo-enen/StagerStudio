﻿namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using Saving;


	public class TooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


		// SUB
		public delegate string StringStringHandler (string value);
		public delegate void VoidStringHandler (string str);


		// VAR
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static StringStringHandler GetHotKey { get; set; } = null;
		public static VoidStringHandler SetTip { get; set; } = null;
		public static SavingBool ShowTip { get; } = new SavingBool("TooltipUI.ShowTip", true);

		[SerializeField] private string m_TipKey = "";
		[SerializeField] private string m_HotKey = "";
		[SerializeField] private string m_HotKey_Alt = "";


		// MSG
		private void OnDisable () => LogTip("");
		public void OnPointerEnter (PointerEventData e) => LogTip(m_TipKey, m_HotKey, m_HotKey_Alt);
		public void OnPointerExit (PointerEventData e) => LogTip("");


		// API
		public static void LogTip (string key) =>
			SetTip(ShowTip ? GetLanguage(key) : "");


		public static void LogTip (string key, string hotkey) =>
			SetTip(ShowTip ? $"{GetLanguage(key)}   {GetHotKey(hotkey)}" : "");


		public static void LogTip (string key, string hotkey, string hotKeyAlt) =>
			SetTip(ShowTip ? $"{GetLanguage(key)}   {GetHotKey(hotkey)}   {GetHotKey(hotKeyAlt)}" : "");


	}
}