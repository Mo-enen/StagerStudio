namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class StageEditor : MonoBehaviour {




		#region --- SUB ---


		public enum EditMode {
			Stage = 0,
			Track = 1,
			Note = 2,
			Speed = 3,
			Motion = 4,
		}

		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();
		public delegate void VoidEditModeHandler (EditMode mode);
		public delegate void VoidHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate int IntHandler ();



		#endregion



		#region --- VAR ---


		// Handle
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static VoidEditModeHandler OnEditModeChanged { get; set; } = null;
		public static VoidHandler OnSelectionChanged { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static IntHandler GetBrushIndex { get; set; } = null;

		// Api
		public EditMode TheEditMode { get; private set; } = EditMode.Stage;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private LayerMask[] m_EditLayerMasks = null;
		[SerializeField] private Toggle[] m_EditModeTGs = null;
		[SerializeField] private Toggle[] m_EyeTGs = null;
		[SerializeField] private Toggle[] m_LockTGs = null;
		[SerializeField] private Transform[] m_Containers = null;
		[SerializeField] private Transform[] m_AntiTargets = null;
		[SerializeField] private RectTransform m_FocusCancel = null;
		[SerializeField] private RectTransform m_SelectionRect = null;
		[SerializeField] private Animator m_FocusAni = null;
		[SerializeField] private string m_FocusKey = "Focus";
		[SerializeField] private string m_UnfocusKey = "Unfocus";
		[SerializeField] private float m_Duration = 0.5f;

		// Data
		private readonly bool[] ItemLock = { false, false, false, false, false, false, };
		private readonly List<int> SelectingObjectsIndex = new List<int>();
		private readonly Collider2D[] CastCols = new Collider2D[64];
		private bool FocusMode = false;
		private bool UIReady = true;
		private Coroutine FocusAniCor = null;
		private Camera _Camera = null;
		private Ray? MouseRayDown = null;
		private bool MouseDown = false;


		#endregion




		#region --- MSG ---


		private void Awake () {
			// Init Edit Mode TGs
			for (int i = 0; i < m_EditModeTGs.Length; i++) {
				int index = i;
				m_EditModeTGs[index].onValueChanged.AddListener((isOn) => {
					if (UIReady && isOn) {
						SetEditMode(index);
						ClearSelection();
					}
				});
			}
			// Eye TGs
			for (int i = 0; i < m_EyeTGs.Length; i++) {
				int index = i;
				var tg = m_EyeTGs[index];
				tg.isOn = false;
				tg.onValueChanged.AddListener((isOn) => SetContainerActive(index, !isOn));
			}
			// Lock TGs
			for (int i = 0; i < m_LockTGs.Length; i++) {
				int index = i;
				var tg = m_LockTGs[index];
				tg.isOn = ItemLock[index];
				tg.onValueChanged.AddListener((locked) => {
					ItemLock[index] = locked;
				});
			}
		}


		private void LateUpdate () {
			var map = GetBeatmap();
			if (map is null || !AntiTargetCheck()) {
				SetSelectionRect(false, default, default);
				return;
			}
			// Mouse
			if (Input.GetMouseButton(0)) {
				if (!MouseDown) {
					MouseDown = true;
					MouseRayDown = GetMouseRay();
					OnMouseLeftDown();
				} else {
					OnMouseLeftDrag();
				}
			} else {
				if (MouseDown) {
					MouseDown = false;
					SetSelectionRect(false, default, default);
					OnMouseLeftUp();
				}
				if (MouseRayDown.HasValue) {
					SetSelectionRect(false, default, default);
					MouseRayDown = null;
				}
			}
		}


		private void OnMouseLeftDown () {
			int brushIndex = GetBrushIndex();
			if (brushIndex < 0) {
				// Select
				bool clearFlag = true;
				var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
				var plane = new Plane(Vector3.back, zoneMin);
				if (plane.Raycast(MouseRayDown.Value, out float enter)) {
					Vector2 pos = MouseRayDown.Value.GetPoint(enter);
					if (pos.x > zoneMin.x && pos.x < zoneMax.x && pos.y > zoneMin.y && pos.y < zoneMax.y) {
						clearFlag = false;
					}
				}
				if (clearFlag) {
					MouseRayDown = null;
				}
			} else {
				// Paint



			}
		}


		private void OnMouseLeftDrag () {
			if (!MouseRayDown.HasValue) { return; }
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var mouseRay = GetMouseRay();
			var plane = new Plane(Vector3.back, zoneMin);
			int brushIndex = GetBrushIndex();
			if (brushIndex < 0) {
				// Select
				if (plane.Raycast(MouseRayDown.Value, out float enterA) && plane.Raycast(mouseRay, out float enterB)) {
					Vector2 a = MouseRayDown.Value.GetPoint(enterA);
					Vector2 b = mouseRay.GetPoint(enterB);
					Vector2 min = Vector2.Min(a, b);
					Vector2 max = Vector2.Max(a, b);
					SetSelectionRect(true,
						new Vector2(Util.RemapUnclamped(zoneMin.x, zoneMax.x, 0f, 1f, min.x), Util.RemapUnclamped(zoneMin.y, zoneMax.y, 0f, 1f, min.y)),
						new Vector2(Util.RemapUnclamped(zoneMin.x, zoneMax.x, 0f, 1f, max.x), Util.RemapUnclamped(zoneMin.y, zoneMax.y, 0f, 1f, max.y))
					);
				}
			} else {
				// Paint



			}
		}


		private void OnMouseLeftUp () {
			if (!MouseRayDown.HasValue) { return; }
			var (zoneMin, _, _, _) = GetZoneMinMax();
			var mouseRay = GetMouseRay();
			var plane = new Plane(Vector3.back, zoneMin);
			int brushIndex = GetBrushIndex();
			if (brushIndex < 0) {
				// Select
				SelectingObjectsIndex.Clear();
				int layerMask = m_EditLayerMasks[(int)TheEditMode];
				if (plane.Raycast(MouseRayDown.Value, out float enterA) && plane.Raycast(mouseRay, out float enterB)) {
					Vector2 a = MouseRayDown.Value.GetPoint(enterA);
					Vector2 b = mouseRay.GetPoint(enterB);
					int count = Physics2D.OverlapBoxNonAlloc((a + b) / 2f, (a - b).Abs(), 0f, CastCols, layerMask);
					if (count > 0) {
						for (int i = 0; i < count; i++) {
							SelectingObjectsIndex.Add(CastCols[i].transform.parent.GetSiblingIndex());
						}
						OnSelectionChanged();
					}
				}
			} else {
				// Paint




			}
		}


		#endregion




		#region --- API ---


		// Set
		public void UI_SetEditMode (int index) {
			if (!UIReady) { return; }
			SetEditMode(index);
			ClearSelection();
		}


		public void ClearSelection () {
			if (SelectingObjectsIndex.Count > 0) {
				SelectingObjectsIndex.Clear();
				OnSelectionChanged();
			}
		}


		// Container Active
		public void UI_SwitchContainerActive (int index) => SetContainerActive(index, !GetContainerActive(index));


		public bool GetContainerActive (int index) => m_Containers[index].gameObject.activeSelf;


		public void SetContainerActive (int index, bool active) {
			m_Containers[index].gameObject.SetActive(active);
			m_EyeTGs[index].isOn = !active;
		}


		// Item Lock
		public bool GetItemLock (int item) => item >= 0 ? ItemLock[item] : false;


		public void UI_SwitchLock (int index) {
			ItemLock[index] = !ItemLock[index];
			m_LockTGs[index].isOn = ItemLock[index];
		}


		// Focus
		public void UI_SetFocus (bool focus) {
			if (!(FocusAniCor is null)) { return; }
			if (focus != FocusMode) {
				FocusMode = focus;
				m_FocusAni.enabled = true;
				m_FocusAni.SetTrigger(focus ? m_FocusKey : m_UnfocusKey);
				if (!(FocusAniCor is null)) {
					StopCoroutine(FocusAniCor);
					FocusAniCor = null;
				}
				FocusAniCor = StartCoroutine(AniCheck());
			}
			// Func
			IEnumerator AniCheck () {
				if (focus) {
					m_FocusCancel.gameObject.SetActive(true);
				}
				yield return new WaitForSeconds(m_Duration);
				yield return new WaitUntil(() =>
					m_FocusAni.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !m_FocusAni.IsInTransition(0)
				);
				if (!focus) {
					m_FocusCancel.gameObject.SetActive(false);
				}
				m_FocusAni.enabled = false;
				FocusAniCor = null;
			}
		}


		public void UI_SwitchFocus () => UI_SetFocus(!FocusMode);


		#endregion



		#region --- LGC ---


		private bool AntiTargetCheck () {
			foreach (var t in m_AntiTargets) {
				if (t.gameObject.activeSelf) {
					return false;
				}
			}
			return true;
		}


		private Ray GetMouseRay () => Camera.ScreenPointToRay(Input.mousePosition);


		// Set
		private void SetEditMode (int index) {
			var mode = (EditMode)index;
			if (mode != TheEditMode) {
				TheEditMode = mode;
				OnEditModeChanged(mode);
			}
			UIReady = false;
			try {
				for (int j = 0; j < m_EditModeTGs.Length; j++) {
					m_EditModeTGs[j].isOn = (int)TheEditMode == j;
				}
			} catch { }
			UIReady = true;
		}


		private void SetSelectionRect (bool active, Vector2 min01, Vector2 max01) {
			if (m_SelectionRect.gameObject.activeSelf != active) {
				m_SelectionRect.gameObject.SetActive(active);
			}
			if (active) {
				m_SelectionRect.anchorMin = min01;
				m_SelectionRect.anchorMax = max01;
			}
		}


		#endregion

	}
}