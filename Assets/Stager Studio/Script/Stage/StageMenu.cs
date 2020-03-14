namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using global::StagerStudio.UI;


	public class StageMenu : MonoBehaviour {



		#region --- SUB ---


		public delegate string LanguageHandler (string key);


		[System.Serializable]
		public class MenuEvent : UnityEvent<object> { }


		[System.Serializable]
		public class MenuData {
			public string Key = "";
			public string[] CallbackKeys = null;
		}


		[System.Serializable]
		public class CallbackData {
			public string Key = "";
			public Sprite Icon = null;
			public MenuEvent Callback = null;
		}


		#endregion




		#region --- VAR ---

		// Handler
		public static LanguageHandler GetLanguage { get; set; } = null;

		// Ser
		[SerializeField] private RectTransform m_MenuRoot = null;
		[SerializeField] private RectTransform m_WindowPrefab = null;
		[SerializeField] private Grabber m_ItemPrefab = null;
		[SerializeField] private RectTransform m_LinePrefab = null;
		[SerializeField] private Camera m_EventCamera = null;
		[SerializeField] private Sprite m_Checker = null;
		[SerializeField] private MenuData[] m_Datas = null;
		[SerializeField] public CallbackData[] m_Callbacks = null;

		// Data
		private Dictionary<string, (MenuEvent callback, Sprite icon)> CallbackMap { get; } = new Dictionary<string, (MenuEvent, Sprite)>();
		private Dictionary<string, string[]> MenuDataMap { get; } = new Dictionary<string, string[]>();
		private Dictionary<string, System.Func<bool>> CheckerMap = new Dictionary<string, System.Func<bool>>();


		#endregion




		#region --- MSG ---


		private void Awake () {
			CallbackMap.Clear();
			foreach (var callback in m_Callbacks) {
				if (!CallbackMap.ContainsKey(callback.Key)) {
					CallbackMap.Add(callback.Key, (callback.Callback, callback.Icon));
				}
			}
			MenuDataMap.Clear();
			foreach (var data in m_Datas) {
				if (!MenuDataMap.ContainsKey(data.Key)) {
					MenuDataMap.Add(data.Key, data.CallbackKeys);
				}
			}
		}


		#endregion




		#region --- API ---


		public void OpenMenu (string key) => OpenMenuLogic(key);


		public void OpenMenu (string key, RectTransform param = null) => OpenMenuLogic(key, param);


		public void CloseMenu () => CloseMenuLogic();


		public void AddCheckerFunc (string key, System.Func<bool> func) {
			if (!CheckerMap.ContainsKey(key)) {
				CheckerMap.Add(key, func);
			}
		}


		#endregion




		#region --- LGC ---


		private void OpenMenuLogic (string key, RectTransform param = null) {
			CloseMenuLogic();
			m_MenuRoot.gameObject.SetActive(true);
			m_MenuRoot.parent.gameObject.SetActive(true);
			// Data
			if (!MenuDataMap.ContainsKey(key) || MenuDataMap[key].Length == 0) { return; }

			// Instance
			var winRT = Instantiate(m_WindowPrefab.gameObject, m_MenuRoot).transform as RectTransform;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_MenuRoot, Input.mousePosition, m_EventCamera, out Vector2 localPoint);
			localPoint.y -= 8f;
			winRT.anchoredPosition = localPoint;
			winRT.localRotation = Quaternion.identity;
			winRT.localScale = Vector3.one;

			// Callback
			var callbackKeys = MenuDataMap[key];
			foreach (var cKey in callbackKeys) {
				if (!string.IsNullOrEmpty(cKey)) {
					if (!CallbackMap.ContainsKey(cKey)) { continue; }
					// Item
					var graber = Instantiate(m_ItemPrefab, winRT);
					var rt = graber.transform as RectTransform;
					rt.localRotation = Quaternion.identity;
					rt.localScale = Vector3.one;
					rt.SetAsLastSibling();
					var callback = CallbackMap[cKey];
					// Button
					var btn = graber.Grab<Button>();
					btn.onClick.AddListener(InvokeCallback);
					// Name
					var text = graber.Grab<Text>("Name");
					text.text = GetLanguage.Invoke(cKey);
					// Icon
					var icon = graber.Grab<Image>("Icon");

					// Checker
					if (CheckerMap.ContainsKey(cKey) && CheckerMap[cKey]()) {
						icon.enabled = true;
						icon.sprite = m_Checker;
					} else {
						icon.enabled = callback.icon;
						icon.sprite = callback.icon;
					}
					// Func
					void InvokeCallback () {
						callback.callback.Invoke(param);
						CloseMenuLogic();
					}
				} else {
					// Line
					var line = Instantiate(m_LinePrefab.gameObject, winRT).transform as RectTransform;
					line.localRotation = Quaternion.identity;
					line.localScale = Vector3.one;
					line.SetAsLastSibling();
				}
			}

			// Clamp Position
			Invoke("Invoke_ClampWindow", 0.1f);
		}


		private void CloseMenuLogic () {
			m_MenuRoot.DestroyAllChildImmediately();
			m_MenuRoot.gameObject.SetActive(false);
			m_MenuRoot.parent.InactiveIfNoChildActive();
		}



		public void Invoke_ClampWindow () {
			if (m_MenuRoot.childCount > 0) {
				Util.ClampRectTransform(m_MenuRoot.GetChild(0) as RectTransform);
			}
		}


		#endregion




	}
}