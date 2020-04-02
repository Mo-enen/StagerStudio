namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	public class TweenEditorUI : MonoBehaviour {


		// Handler
		public delegate string StringStringHandler (string key);
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Short
		private AnimationCurve UICurve {
			get {
				var curve = m_Curve.Curve;
				if (curve is null) {
					curve = AnimationCurve.Linear(0, 0, 1, 1);
				}
				return curve;
			}
		}

		// Ser
		[SerializeField] private CurveUI m_Curve = null;
		[SerializeField] private RectTransform m_HoverPoint = null;
		[SerializeField] private RectTransform m_SelectionPoint = null;
		[SerializeField] private RectTransform m_InTangent = null;
		[SerializeField] private RectTransform m_OutTangent = null;
		[SerializeField] private EventTrigger m_EditorTrigger = null;
		[SerializeField] private EventTrigger m_HoverTrigger = null;
		[SerializeField] private EventTrigger m_PointTrigger = null;
		[SerializeField] private EventTrigger m_InTrigger = null;
		[SerializeField] private EventTrigger m_OutTrigger = null;
		[SerializeField] private Button m_RemovePointButton = null;
		[SerializeField] private Text m_RemovePointLabel = null;
		[SerializeField] private Text[] m_LanguageTexts = null;

		// Data
		private const string DIALOG_CancelConfirm = "Dialog.TweenEditor.CancelConfirm";
		private Vector3 PrevMousePosition = default;
		private System.Action<AnimationCurve> Done = null;
		private Camera _Camera = null;
		private int HoveringKeyIndex = -1;
		private int SelectingIndex = -1;
		private bool Snap = true;


		// MSG
		private void Awake () {
			foreach (var tx in m_LanguageTexts) {
				tx.text = GetLanguage?.Invoke(tx.name);
			}
		}


		private void Update () {
			// Hover
			if (Input.mousePosition != PrevMousePosition) {
				var hoverIndex = GetHoveringPoint(GetPos01(Input.mousePosition, _Camera != null ? _Camera : (_Camera = Camera.main)));
				bool show = hoverIndex >= 0 && hoverIndex != SelectingIndex;
				if (show != m_HoverPoint.gameObject.activeSelf) {
					m_HoverPoint.gameObject.SetActive(show);
				}
				if (hoverIndex != HoveringKeyIndex) {
					if (hoverIndex >= 0) {
						m_HoverPoint.anchorMin = m_HoverPoint.anchorMax = GetPoint01(hoverIndex).pos;
						m_HoverPoint.offsetMin = m_HoverPoint.offsetMax = Vector2.zero;
					}
					HoveringKeyIndex = hoverIndex;
				}
				PrevMousePosition = Input.mousePosition;
			}
			// Key
			if ((Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) && SelectingIndex >= 0) {
				UI_DeleteSelectingPoint();
			}
		}


		// API
		public void Init (AnimationCurve _curve, System.Action<AnimationCurve> done) {
			Done = done;
			// Fix Curve Point
			var curve = new AnimationCurve() {
				postWrapMode = WrapMode.Clamp,
				preWrapMode = WrapMode.Clamp,
			};
			foreach (var key in _curve.keys) {
				curve.AddKey(key);
			}
			if (curve is null || curve.length == 0) {
				curve = AnimationCurve.Linear(0, 0, 1, 1);
			} else if (curve.length == 1) {
				float value = curve[0].value;
				curve = AnimationCurve.Linear(0, value, 1, value);
			}
			// Fix Curve Start End
			int len = curve.length;
			if (curve[0].time != 0f || curve[len - 1].time != 1f) {
				float offset = -curve[0].time;
				float muti = 1f / Mathf.Max(curve[len - 1].time - curve[0].time, 0.0001f);
				for (int i = 0; i < len; i++) {
					var key = curve[i];
					key.time = key.time * muti + offset;
					curve.MoveKey(i, key);
				}
			}
			m_Curve.Curve = curve;
			// Trigger
			m_EditorTrigger.triggers[0].callback.AddListener(OnEditorDown);
			m_HoverTrigger.triggers[0].callback.AddListener(OnEditorDown);
			m_InTrigger.triggers[0].callback.AddListener(OnInHandleDrag);
			m_InTrigger.triggers[1].callback.AddListener(OnInHandleDrag);
			m_OutTrigger.triggers[0].callback.AddListener(OnOutHandleDrag);
			m_OutTrigger.triggers[1].callback.AddListener(OnOutHandleDrag);
			m_PointTrigger.triggers[0].callback.AddListener(OnPointHandleDrag);
			m_PointTrigger.triggers[1].callback.AddListener(OnPointHandleDrag);
			// --- Func ---
			void OnEditorDown (BaseEventData be) {
				var e = be as PointerEventData;
				if (e.button == PointerEventData.InputButton.Left) {
					int hIndex = GetHoveringPoint(GetPos01(e.position, e.pressEventCamera));
					if (hIndex != SelectingIndex) {
						SelectingIndex = hIndex;
						RefreshSelectionUI(hIndex);
					}
				}
			}
			void OnPointHandleDrag (BaseEventData be) {
				var e = be as PointerEventData;
				if (e.button == PointerEventData.InputButton.Left) {
					var _c = UICurve;
					if (_c is null || SelectingIndex < 0 || SelectingIndex >= _c.length) { return; }
					var pos01 = GetPos01(e.position, e.pressEventCamera);
					if (Snap) {
						pos01 = SnapPos01(pos01);
					}
					pos01.x = Mathf.Clamp(pos01.x, 0.001f, 0.999f);
					pos01.y = Mathf.Clamp01(pos01.y);
					var selectIndex = SetPoint01(SelectingIndex, pos01);
					if (selectIndex != SelectingIndex) {
						RefreshSelectionUI(selectIndex);
						SelectingIndex = selectIndex;
					}
					RefreshSelectionUI(SelectingIndex);
					m_Curve.SetVerticesDirty();
					PrevMousePosition = default;
				}
			}
			void OnInHandleDrag (BaseEventData be) {
				var e = be as PointerEventData;
				if (e.button == PointerEventData.InputButton.Left) {
					var pos01 = GetPos01(Input.mousePosition, e.pressEventCamera);
					var (point01, _, _) = GetPoint01(SelectingIndex);
					float angle = Quaternion.FromToRotation(Vector2.left, pos01 - point01).eulerAngles.z;
					if (Snap) {
						angle = SnapAngle(angle);
					}
					SetPoint01(SelectingIndex, null, Mathf.Tan(angle * Mathf.Deg2Rad));
					RefreshSelectionUI(SelectingIndex);
					m_Curve.SetVerticesDirty();
				}
			}
			void OnOutHandleDrag (BaseEventData be) {
				var e = be as PointerEventData;
				if (e.button == PointerEventData.InputButton.Left) {
					var pos01 = GetPos01(Input.mousePosition, e.pressEventCamera);
					var (point01, _, _) = GetPoint01(SelectingIndex);
					float angle = Quaternion.FromToRotation(Vector2.right, pos01 - point01).eulerAngles.z;
					if (Snap) {
						angle = SnapAngle(angle);
					}
					SetPoint01(SelectingIndex, null, null, Mathf.Tan(angle * Mathf.Deg2Rad));
					RefreshSelectionUI(SelectingIndex);
					m_Curve.SetVerticesDirty();
				}
			}
		}


		public void UI_Done (bool save) {
			if (!save) {
				DialogUtil.Dialog_OK_Cancel(DIALOG_CancelConfirm, DialogUtil.MarkType.Warning, Close);
			} else {
				Done?.Invoke(UICurve);
				Close();
			}
			// Func
			void Close () {
				transform.parent.gameObject.SetActive(false);
				transform.parent.parent.InactiveIfNoChildActive();
				DestroyImmediate(gameObject, false);
			}
		}


		public void UI_AddPoint () {
			var curve = UICurve;
			if (curve is null) { return; }
			int index = curve.AddKey(new Keyframe(Random.Range(0.4f, 0.6f), Random.Range(0.4f, 0.6f), 0f, 0f));
			m_Curve.SetVerticesDirty();
			SelectingIndex = index;
			RefreshSelectionUI(index);
		}


		public void UI_DeleteSelectingPoint () {
			var curve = UICurve;
			if (curve is null || SelectingIndex <= 0 || SelectingIndex >= curve.length - 1) { return; }
			curve.RemoveKey(SelectingIndex);
			m_Curve.SetVerticesDirty();
			SelectingIndex = -1;
			RefreshSelectionUI(-1);
		}


		public void UI_Snap (bool snap) {
			Snap = snap;
		}


		// LGC
		private Vector2 GetPos01 (Vector2 screenPos, Camera camera) => m_Curve.rectTransform.Get01Position(screenPos, camera);


		private void RefreshSelectionUI (int index) {
			m_SelectionPoint.gameObject.SetActive(index >= 0);
			bool inter = index > 0 && index < UICurve.length - 1;
			m_RemovePointButton.interactable = inter;
			var c = m_RemovePointLabel.color;
			c.a = inter ? 1f : 0.5f;
			m_RemovePointLabel.color = c;
			if (index >= 0) {
				var (pos, inTangent, outTangent) = GetPoint01(index);
				m_SelectionPoint.anchorMin = m_SelectionPoint.anchorMax = pos;
				m_SelectionPoint.offsetMin = m_SelectionPoint.offsetMax = Vector2.zero;
				// Tangent
				m_InTangent.gameObject.SetActive(index > 0);
				m_OutTangent.gameObject.SetActive(index < UICurve.length - 1);
				float ratio = m_Curve.rectTransform.rect.height / m_Curve.rectTransform.rect.width;
				m_InTangent.localRotation = Quaternion.Euler(0, 0, Mathf.Atan(inTangent * ratio) * Mathf.Rad2Deg);
				m_OutTangent.localRotation = Quaternion.Euler(0, 0, Mathf.Atan(outTangent * ratio) * Mathf.Rad2Deg);
				m_InTrigger.transform.rotation = Quaternion.identity;
				m_OutTrigger.transform.rotation = Quaternion.identity;
			}
		}


		private int GetHoveringPoint (Vector2 pos01) {
			int index = -1;
			var curve = UICurve;
			if (!(curve is null)) {
				float GAP_X = 7f / m_Curve.rectTransform.rect.width;
				float GAP_Y = 7f / m_Curve.rectTransform.rect.height;
				var keys = curve.keys;
				for (int i = 0; i < keys.Length; i++) {
					var key = keys[i];
					if (pos01.x > key.time - GAP_X &&
						pos01.x < key.time + GAP_X &&
						pos01.y > key.value - GAP_Y &&
						pos01.y < key.value + GAP_Y
					) {
						index = i;
						break;
					}
				}
			}
			return index;
		}


		private (Vector2 pos, float inTangent, float outTangent) GetPoint01 (int index) {
			var curve = UICurve;
			if (curve is null || index < 0 || index >= curve.length) { return default; }
			var point = curve[index];
			return (new Vector2(
				Mathf.Clamp01(point.time),
				Mathf.Clamp01(point.value)
			), point.inTangent, point.outTangent);
		}


		private int SetPoint01 (int index, Vector2? value01 = null, float? inTangent = null, float? outTangent = null) {
			var curve = UICurve;
			if (curve is null || index < 0 || index >= curve.length) { return index; }
			var point = curve[index];
			if (value01.HasValue) {
				point.time = index == 0 ? 0f : index == curve.length - 1 ? 1f : Mathf.Clamp01(value01.Value.x);
				point.value = Mathf.Clamp01(value01.Value.y);
			}
			if (inTangent.HasValue) {
				point.inTangent = inTangent.Value;
			}
			if (outTangent.HasValue) {
				point.outTangent = outTangent.Value;
			}
			return curve.MoveKey(index, point);
		}


		private Vector2 SnapPos01 (Vector2 pos01) {
			const float GAP_X = 64f;
			const float GAP_Y = 32f;
			pos01.x = Mathf.Round(pos01.x * GAP_X) / GAP_X;
			pos01.y = Mathf.Round(pos01.y * GAP_Y) / GAP_Y;
			return pos01;
		}


		private float SnapAngle (float angle) {
			const float GAP = 15f;
			float ratio = m_Curve.rectTransform.rect.height / m_Curve.rectTransform.rect.width;
			angle = Mathf.Atan(Mathf.Tan(angle * Mathf.Deg2Rad) * ratio) * Mathf.Rad2Deg;
			angle = Mathf.Round(angle / GAP) * GAP;
			return Mathf.Atan(Mathf.Tan(angle * Mathf.Deg2Rad) / ratio) * Mathf.Rad2Deg;
		}


	}
}