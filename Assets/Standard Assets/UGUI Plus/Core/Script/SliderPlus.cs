namespace UGUIPlus {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	[DisallowMultipleComponent]
	[AddComponentMenu("uGUI Plus/Slider Plus")]
	public class SliderPlus : Slider, IPointerClickHandler {




		#region --- SUB ---



		[System.Serializable]
		public class MinValueChangeEventHandler : UnityEvent<float> { }



		#endregion




		#region --- VAR ---



		// Short Cut
		public PointerEventHandler PointerEventHandler {
			get {
				return m_PointerEventHandler;
			}
		}

		public RectTransform MinHandleRect {
			get {
				return m_MinHandleRect;
			}

			set {
				m_MinHandleRect = value;
			}
		}

		public bool UseMinMaxSlider {
			get {
				return m_UseMinMaxSlider;
			}

			set {
				m_UseMinMaxSlider = value;
				UpdateFill();
			}
		}

		public MinValueChangeEventHandler MinValueChangeEvent {
			get {
				return m_MinValueChangeEvent;
			}

			set {
				m_MinValueChangeEvent = value;
			}
		}

		public float LeftValue {
			get {
				return m_LeftValue;
			}

			set {
				m_LeftValue = value;
				UpdateMinHandle();
			}
		}

		public float Length {
			get {
				return value - m_LeftValue;
			}
		}

		private bool Horizontal {
			get {
				return direction == Direction.LeftToRight || direction == Direction.RightToLeft;
			}
		}

		private bool MinToMax {
			get {
				return direction == Direction.LeftToRight || direction == Direction.BottomToTop;
			}
		}


		// Ser
		[SerializeField] private bool m_UseMinMaxSlider = false;
		[SerializeField] private float m_LeftValue = 0f;
		[SerializeField] private RectTransform m_MinHandleRect;
		[SerializeField] private MinValueChangeEventHandler m_MinValueChangeEvent;
		[SerializeField] private PointerEventHandler m_PointerEventHandler = new PointerEventHandler();


		// Data
		private Vector2 m_DragOffset = Vector2.zero;


		#endregion




		#region --- MSG ---



		protected override void Awake () {
			base.Awake();
			// Init Event Delegator
			InitHandle(handleRect, false);
			InitHandle(m_MinHandleRect, true);
			onValueChanged.AddListener(OnValueChanged);
		}


		protected override void Start () {
			UpdateFill();
			UpdateMinHandle();
		}


		protected override void OnDestroy () {
			base.OnDestroy();
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) {
				return;
			}
#endif
			StopAllCoroutines();
			onValueChanged.RemoveAllListeners();
		}


		protected override void OnDisable () {
			base.OnDisable();
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) {
				return;
			}
#endif
			StopAllCoroutines();
			onValueChanged.RemoveAllListeners();
		}


		protected override void OnRectTransformDimensionsChange () {
			base.OnRectTransformDimensionsChange();
			UpdateFill();
			UpdateMinHandle();
		}


		public override void GraphicUpdateComplete () {
			base.GraphicUpdateComplete();
			UpdateFill();
			UpdateMinHandle();
		}


		public override void LayoutComplete () {
			base.LayoutComplete();
			UpdateFill();
			UpdateMinHandle();
		}


#if UNITY_EDITOR

		protected override void Reset () {
			base.Reset();
			transition = Transition.None;
		}

#endif


		#endregion




		#region --- API ---



		public override void OnDrag (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				base.OnDrag(eventData);
			}
		}


		public override void OnInitializePotentialDrag (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				base.OnInitializePotentialDrag(eventData);
			}
		}


		public override void OnPointerDown (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				base.OnPointerDown(eventData);
				m_PointerEventHandler.OnPointerDown(eventData);
			}
		}


		public override void OnPointerUp (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				base.OnPointerUp(eventData);
				m_PointerEventHandler.OnPointerUp(eventData);
			}
		}


		public override void OnPointerEnter (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				base.OnPointerEnter(eventData);
				m_PointerEventHandler.OnPointerEnter(eventData);

			}
		}


		public override void OnPointerExit (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				base.OnPointerExit(eventData);
				m_PointerEventHandler.OnPointerExit(eventData);

			}
		}


		public override void OnSelect (BaseEventData eventData) {
			base.OnSelect(eventData);
			m_PointerEventHandler.OnSelect(eventData);
		}


		public override void OnDeselect (BaseEventData eventData) {
			base.OnDeselect(eventData);
			m_PointerEventHandler.OnDeselect(eventData);
		}


		public void OnPointerClick (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				m_PointerEventHandler.OnPointerClick(eventData);
			}
		}




		#endregion




		#region --- LGC ---



		private void InitHandle (RectTransform handle, bool isMin) {
			if (!handle) {
				return;
			}
			// Drag
			var handleD = handle.GetComponent<DragEventDelegator>();
			if (!handleD) {
				handleD = handle.gameObject.AddComponent<DragEventDelegator>();
			}
			if (isMin) {
				handleD.Drag += OnMinHandleDrag;
				handleD.InitializePotentialDrag += OnMinHandlerBeginDrag;
			} else {
				handleD.Drag += OnMaxHandleDrag;
				handleD.InitializePotentialDrag += OnMaxHandlerBeginDrag;
			}
			// Pointer
			var handleP = handle.GetComponent<PointerEventDelegator>();
			if (!handleP) {
				handleP = handle.gameObject.AddComponent<PointerEventDelegator>();
			}
			if (isMin) {
				handleP.PointerDown += OnMinHandlerDown;
			} else {
				handleP.PointerDown += OnMaxHandlerDown;
			}
			handleP.PointerUp += OnMinMaxHandlerUp;
			handleP.PointerEnter += OnMinMaxHandlerEnter;
			handleP.PointerExit += OnMinMaxHandlerExit;
			handleP.PointerClick += OnMinMaxHandlerClick;
		}


		public void UpdateFill () {
			if (!fillRect) {
				return;
			}
			if (m_UseMinMaxSlider) {
				float xy01 = (m_LeftValue - minValue) / (maxValue - minValue);
				if (MinToMax) {
					fillRect.anchorMin = new Vector2(
						Horizontal ? xy01 : fillRect.anchorMin.x,
						Horizontal ? fillRect.anchorMin.y : xy01
					);
				} else {
					fillRect.anchorMax = new Vector2(
						Horizontal ? 1f - xy01 : fillRect.anchorMax.x,
						Horizontal ? fillRect.anchorMax.y : 1f - xy01
					);
				}
			} else {
				if (MinToMax) {
					fillRect.anchorMin = new Vector2(
						Horizontal ? 0f : fillRect.anchorMin.x,
						Horizontal ? fillRect.anchorMin.y : 0f
					);
				} else {
					fillRect.anchorMax = new Vector2(
						Horizontal ? 1f : fillRect.anchorMax.x,
						Horizontal ? fillRect.anchorMax.y : 1f
					);
				}
			}
		}


		private void ClampByLeftValue () {
			value = Mathf.Clamp(value, LeftValue, maxValue);
		}


		public void UpdateMinHandle () {
			if (!m_MinHandleRect) {
				return;
			}
			float xy01 = (m_LeftValue - minValue) / (maxValue - minValue);
			if (!MinToMax) {
				xy01 = 1f - xy01;
			}
			if (Horizontal) {
				// H
				m_MinHandleRect.anchorMin = new Vector2(xy01, 0f);
				m_MinHandleRect.anchorMax = new Vector2(xy01, 1f);
			} else {
				// V
				m_MinHandleRect.anchorMin = new Vector2(0f, xy01);
				m_MinHandleRect.anchorMax = new Vector2(1f, xy01);
			}
		}


		private void OnValueChanged (float value) {
			if (m_UseMinMaxSlider) {
				UpdateFill();
				ClampByLeftValue();
			}
		}


		// Drag
		private void OnMinHandleDrag (PointerEventData eventData) {
			if (m_UseMinMaxSlider) {
				float newLeftValue;

				RectTransform prt = (RectTransform)m_MinHandleRect.parent;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(prt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint01);

				localPoint01 = new Vector2(
					(localPoint01.x + m_DragOffset.x + prt.rect.width * prt.pivot.x) / prt.rect.width,
					(localPoint01.y + m_DragOffset.y + prt.rect.height * prt.pivot.y) / prt.rect.height
				);


				float newValue01;
				if (Horizontal) {
					newValue01 = Mathf.Clamp01(localPoint01.x);
				} else {
					newValue01 = Mathf.Clamp01(localPoint01.y);
				}
				if (!MinToMax) {
					newValue01 = 1f - newValue01;
				}
				newLeftValue = Mathf.Lerp(minValue, maxValue, newValue01);
				if (wholeNumbers) {
					newLeftValue = Mathf.Round(newLeftValue);
				}

				m_LeftValue = Mathf.Clamp(newLeftValue, minValue, value);
				m_MinValueChangeEvent.Invoke(newLeftValue);
				UpdateFill();
				UpdateMinHandle();

			}
		}


		private void OnMaxHandleDrag (PointerEventData eventData) {
			//eventData.position += m_DragOffset;
			base.OnDrag(eventData);
			if (m_UseMinMaxSlider) {
				ClampByLeftValue();
				UpdateFill();
			}
		}


		private void OnMinHandlerBeginDrag (PointerEventData eventData) {
			if (m_UseMinMaxSlider) {
				m_DragOffset = GetMouseOffset(m_MinHandleRect, eventData.position, eventData.pressEventCamera);
				MinHandleRect.SetAsLastSibling();
			}
		}


		private void OnMaxHandlerBeginDrag (PointerEventData eventData) {
			base.OnInitializePotentialDrag(eventData);
			if (m_UseMinMaxSlider) {
				m_DragOffset = GetMouseOffset(handleRect, eventData.position, eventData.pressEventCamera);
				handleRect.SetAsLastSibling();
			}
		}



		// Pointer
		private void OnMinHandlerDown (PointerEventData eventData) {
			if (m_UseMinMaxSlider) {
				m_PointerEventHandler.OnPointerDown(eventData);
			}
		}


		private void OnMaxHandlerDown (PointerEventData eventData) {
			if (!m_UseMinMaxSlider) {
				base.OnPointerDown(eventData);
			}
			m_PointerEventHandler.OnPointerDown(eventData);
		}


		private void OnMinMaxHandlerUp (PointerEventData eventData) {
			base.OnPointerUp(eventData);
			m_PointerEventHandler.OnPointerUp(eventData);
		}


		private void OnMinMaxHandlerEnter (PointerEventData eventData) {
			base.OnPointerEnter(eventData);
			m_PointerEventHandler.OnPointerEnter(eventData);

		}


		private void OnMinMaxHandlerExit (PointerEventData eventData) {
			base.OnPointerExit(eventData);
			m_PointerEventHandler.OnPointerExit(eventData);

		}


		private void OnMinMaxHandlerClick (PointerEventData eventData) {
			m_PointerEventHandler.OnPointerClick(eventData);
		}


		private Vector2 GetMouseOffset (RectTransform rt, Vector2 pos, Camera cam) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pos, cam, out Vector2 v);
			return -v;
		}



		#endregion




	}
}
