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
		[SerializeField] private Animator m_FocusAni = null;
		[SerializeField] private string m_FocusKey = "Focus";
		[SerializeField] private string m_UnfocusKey = "Unfocus";
		[SerializeField] private float m_Duration = 0.5f;

		// Data
		private readonly bool[] ItemLock = { false, false, false, false, false, false, };
		private bool FocusMode = false;
		private bool UIReady = true;
		private Coroutine FocusAniCor = null;
		private Camera _Camera = null;

		// Mouse
		private readonly List<int> SelectingObjectsIndex = new List<int>();
		private readonly RaycastHit[] CastHits = new RaycastHit[64];
		private bool ClickStartInsideSelection = false;
		private Ray? MouseRayDown = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			// Init Edit Mode TGs
			for (int i = 0; i < m_EditModeTGs.Length; i++) {
				int index = i;
				m_EditModeTGs[index].onValueChanged.AddListener((isOn) => {
					if (UIReady && isOn) {
						ClearSelection();
						SetEditMode(index);
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
				MouseRayDown = null;
				ClickStartInsideSelection = false;
				return;
			}
			// Mouse
			if (Input.GetMouseButton(0)) {
				if (!MouseRayDown.HasValue) {
					MouseRayDown = GetMouseRay();
					OnMouseLeftDown();
				} else {
					OnMouseLeftDrag();
				}
			} else {
				if (MouseRayDown.HasValue) {
					OnMouseLeftUp();
					MouseRayDown = null;
				}
				ClickStartInsideSelection = false;
			}
		}


		// Mouse Left
		private void OnMouseLeftDown () {
			int brushIndex = GetBrushIndex();
			if (brushIndex < 0) {
				// Select or Move
				var map = GetBeatmap();
				bool ctrl = Input.GetKey(KeyCode.LeftControl);
				bool alt = Input.GetKey(KeyCode.LeftAlt);
				int count = Physics.RaycastNonAlloc(MouseRayDown.Value, CastHits, float.MaxValue, m_EditLayerMasks[(int)TheEditMode]);
				int overlapIndex = -1;
				ClickStartInsideSelection = false;
				for (int i = 0; i < count; i++) {
					int itemIndex = GetObjectIndexFromCollider(CastHits[i]);
					overlapIndex = Mathf.Max(itemIndex, overlapIndex);
					if (SelectingObjectsIndex.Count > 0 && !alt && CheckSelecting(itemIndex, map)) {
						ClickStartInsideSelection = true;
						break;
					}
				}
				// Select
				if (!ClickStartInsideSelection) {
					if (!ctrl && !alt) { ClearSelection(); }
					if (overlapIndex >= 0) {
						AddSelection(overlapIndex, !alt, map);
						OnSelectionChanged();
					}
				}
			} else {
				// Paint






			}
		}


		private void OnMouseLeftDrag () {
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var mouseRay = GetMouseRay();
			var plane = new Plane(Vector3.back, zoneMin);
			int brushIndex = GetBrushIndex();
			if (brushIndex < 0 && ClickStartInsideSelection) {
				// Moving Selection





			}
		}


		private void OnMouseLeftUp () {





		}


		#endregion




		#region --- API ---


		// Edit Mode
		public void UI_SetEditMode (int index) {
			if (!UIReady) { return; }
			ClearSelection();
			SetEditMode(index);
		}


		// Selection
		public bool CheckSelecting (int index, Beatmap map) {
			switch (TheEditMode) {
				case EditMode.Stage:
					if (!(map.Stages is null) && index >= 0 && index < map.Stages.Count) {
						return map.Stages[index].Selecting;
					}
					break;
				case EditMode.Track:
					if (!(map.Tracks is null) && index >= 0 && index < map.Tracks.Count) {
						return map.Tracks[index].Selecting;
					}
					break;
				case EditMode.Note:
					if (!(map.Notes is null) && index >= 0 && index < map.Notes.Count) {
						return map.Notes[index].Selecting;
					}
					break;
				case EditMode.Speed:
					break;
				case EditMode.Motion:
					break;
			}
			return false;
		}


		public void AddSelection (int index, bool select, Beatmap map) {
			// Selecting Index List
			if (select) {
				// Add
				SelectingObjectsIndex.Add(index);
			} else {
				// Remove
				for (int i = 0; i < SelectingObjectsIndex.Count; i++) {
					if (SelectingObjectsIndex[i] == index) {
						SelectingObjectsIndex.RemoveAt(i);
						break;
					}
				}
			}
			// Set Cache
			switch (TheEditMode) {
				case EditMode.Stage:
					if (!(map.Stages is null) && index >= 0 && index < map.Stages.Count) {
						map.Stages[index].Selecting = select;
					}
					break;
				case EditMode.Track:
					if (!(map.Tracks is null) && index >= 0 && index < map.Tracks.Count) {
						map.Tracks[index].Selecting = select;
					}
					break;
				case EditMode.Note:
					if (!(map.Notes is null) && index >= 0 && index < map.Notes.Count) {
						map.Notes[index].Selecting = select;
					}
					break;
				case EditMode.Speed:
					break;
				case EditMode.Motion:
					break;
			}
		}


		public void ClearSelection () {
			var map = GetBeatmap();
			if (!(map is null) && SelectingObjectsIndex.Count > 0) {
				// Clear Cache
				foreach (var index in SelectingObjectsIndex) {
					switch (TheEditMode) {
						case EditMode.Stage:
							if (!(map.Stages is null) && index >= 0 && index < map.Stages.Count) {
								map.Stages[index].Selecting = false;
							}
							break;
						case EditMode.Track:
							if (!(map.Tracks is null) && index >= 0 && index < map.Tracks.Count) {
								map.Tracks[index].Selecting = false;
							}
							break;
						case EditMode.Note:
							if (!(map.Notes is null) && index >= 0 && index < map.Notes.Count) {
								map.Notes[index].Selecting = false;
							}
							break;
						case EditMode.Speed:
							break;
						case EditMode.Motion:
							break;
					}
				}
				// Clear List
				SelectingObjectsIndex.Clear();
				// Final
				OnSelectionChanged();
			}
		}


		// Container
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


		private Ray GetMouseRay () => Camera.ScreenPointToRay(Input.mousePosition);


		private int GetObjectIndexFromCollider (RaycastHit hit) => hit.transform.parent.GetSiblingIndex();


		#endregion

	}
}