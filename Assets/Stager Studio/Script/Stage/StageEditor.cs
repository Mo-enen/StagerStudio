namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class StageEditor : MonoBehaviour {




		#region --- SUB ---


		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();
		public delegate void VoidHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate int IntHandler ();



		#endregion



		#region --- VAR ---


		// Handle
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static VoidHandler OnSelectionChanged { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static IntHandler GetBrushIndex { get; set; } = null;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private LayerMask m_AllObjectMask = default;
		[SerializeField] private string[] m_ItemLayerNames = null;
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
		private int[] ItemLayers = null;
		private bool FocusMode = false;
		private Coroutine FocusAniCor = null;
		private Camera _Camera = null;

		// Mouse
		private readonly List<(int, int)> SelectingObjectsIndex = new List<(int, int)>();
		private readonly RaycastHit[] CastHits = new RaycastHit[64];
		private bool ClickStartInsideSelection = false;
		private Ray? MouseRayDown = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			// Init Layer
			ItemLayers = new int[m_ItemLayerNames.Length];
			for (int i = 0; i < m_ItemLayerNames.Length; i++) {
				ItemLayers[i] = LayerMask.NameToLayer(m_ItemLayerNames[i]);
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
			} else if (MouseRayDown.HasValue) {
				MouseRayDown = null;
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
				int count = Physics.RaycastNonAlloc(MouseRayDown.Value, CastHits, float.MaxValue, m_AllObjectMask);
				int overlapLayer = -1;
				int overlapIndex = -1;
				int maxSelectingLayer = -1;
				foreach (var (layerIndex, _) in SelectingObjectsIndex) {
					maxSelectingLayer = Mathf.Max(maxSelectingLayer, layerIndex);
				}
				for (int i = 0; i < count; i++) {
					var (layerIndex, itemIndex) = GetObjectIndexFromCollider(CastHits[i]);
					if (layerIndex >= overlapLayer) {
						if (layerIndex != overlapLayer) {
							overlapIndex = -1;
						}
						overlapIndex = Mathf.Max(itemIndex, overlapIndex);
						overlapLayer = layerIndex;
					}
				}
				ClickStartInsideSelection = SelectingObjectsIndex.Count > 0 && !alt && overlapLayer < maxSelectingLayer && CheckSelecting(overlapLayer, overlapIndex, map);
				// Select
				if (!ClickStartInsideSelection) {
					if (!ctrl && !alt) { ClearSelection(); }
					if (overlapLayer >= 0 && overlapIndex >= 0) {
						AddSelection(overlapLayer, overlapIndex, !alt, map);
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


		#endregion




		#region --- API ---


		// Selection
		public bool CheckSelecting (int layer, int index, Beatmap map) {
			switch (layer) {
				case int l when l == ItemLayers[0]:
					if (index >= 0 && index < map.Stages.Count) {
						return map.Stages[index].Selecting;
					}
					break;
				case int l when l == ItemLayers[1]:
					if (index >= 0 && index < map.Tracks.Count) {
						return map.Tracks[index].Selecting;
					}
					break;
				case int l when l == ItemLayers[2]:
					if (index >= 0 && index < map.Notes.Count) {
						return map.Notes[index].Selecting;
					}
					break;
				case int l when l == ItemLayers[3]:
					break;
				case int l when l == ItemLayers[4]:
					break;
			}
			return false;
		}


		public void AddSelection (int layer, int index, bool select, Beatmap map) {
			// Selecting Index List
			if (select) {
				// Add
				SelectingObjectsIndex.Add((layer, index));
			} else {
				// Remove
				for (int i = 0; i < SelectingObjectsIndex.Count; i++) {
					if (SelectingObjectsIndex[i] == (layer, index)) {
						SelectingObjectsIndex.RemoveAt(i);
						break;
					}
				}
			}
			// Set Cache
			switch (layer) {
				case int l when l == ItemLayers[0]:
					if (index >= 0 && index < map.Stages.Count) {
						map.Stages[index].Selecting = select;
					}
					break;
				case int l when l == ItemLayers[1]:
					if (index >= 0 && index < map.Tracks.Count) {
						map.Tracks[index].Selecting = select;
					}
					break;
				case int l when l == ItemLayers[2]:
					if (index >= 0 && index < map.Notes.Count) {
						map.Notes[index].Selecting = select;
					}
					break;
				case int l when l == ItemLayers[3]:
					break;
				case int l when l == ItemLayers[4]:
					break;
			}
		}


		public void ClearSelection () {
			var map = GetBeatmap();
			if (!(map is null) && SelectingObjectsIndex.Count > 0) {
				// Clear Cache
				foreach (var (layer, index) in SelectingObjectsIndex) {
					switch (layer) {
						case int l when l == ItemLayers[0]:
							if (!(map.Stages is null) && index >= 0 && index < map.Stages.Count) {
								map.Stages[index].Selecting = false;
							}
							break;
						case int l when l == ItemLayers[1]:
							if (!(map.Tracks is null) && index >= 0 && index < map.Tracks.Count) {
								map.Tracks[index].Selecting = false;
							}
							break;
						case int l when l == ItemLayers[2]:
							if (!(map.Notes is null) && index >= 0 && index < map.Notes.Count) {
								map.Notes[index].Selecting = false;
							}
							break;
						case int l when l == ItemLayers[3]:
							break;
						case int l when l == ItemLayers[4]:
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


		private Ray GetMouseRay () => Camera.ScreenPointToRay(Input.mousePosition);


		private (int, int) GetObjectIndexFromCollider (RaycastHit hit) =>
			(hit.transform.gameObject.layer, hit.transform.parent.GetSiblingIndex());


		#endregion

	}
}