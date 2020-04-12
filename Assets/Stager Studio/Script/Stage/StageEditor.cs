namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;
	using Rendering;



	public class StageEditor : MonoBehaviour {




		#region --- SUB ---


		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();
		public delegate void VoidHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate (int type, int index) IntIntHandler ();
		public delegate bool BoolHandler ();
		public delegate Beatmap.Stage StageHandler ();
		public delegate Beatmap.Track TrackHandler ();
		public delegate List<Beatmap.Note> NotesIntHandler (int i);


		#endregion




		#region --- VAR ---


		// Const
		private readonly static string[] ITEM_LAYER_NAMES = { "Stage", "Track", "Note", "Speed", "Motion", };

		// Handle
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static IntIntHandler GetBrushTypeIndex { get; set; } = null;
		public static BoolHandler GetEditorActive { get; set; } = null;
		public static VoidHandler OnSelectionChanged { get; set; } = null;
		public static VoidHandler OnLockEyeChanged { get; set; } = null;
		public static BoolHandler GetUseDynamicSpeed { get; set; } = null;
		public static BoolHandler GetUseAbreast { get; set; } = null;
		public static StageHandler GetDefaultStageBrush { get; set; } = null;
		public static TrackHandler GetDefaultTrackBrush { get; set; } = null;
		public static NotesIntHandler GetNotesBrushAt { get; set; } = null;

		// Api
		public int SelectingCount => SelectingObjectsIndex.Count;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Toggle[] m_EyeTGs = null;
		[SerializeField] private Toggle[] m_LockTGs = null;
		[SerializeField] private Transform[] m_Containers = null;
		[SerializeField] private Transform[] m_AntiTargets = null;
		[SerializeField] private RectTransform m_FocusCancel = null;
		[SerializeField] private Animator m_FocusAni = null;
		[SerializeField] private GridRenderer m_Grid = null;
		[SerializeField] private SpriteRenderer m_Hover = null;
		[SerializeField] private LoopUvRenderer m_Ghost = null;
		[SerializeField] private SpriteRenderer m_GhostPivot = null;
		[SerializeField] private string m_NoteHoldLayerName = "HoldNote";
		[SerializeField] private string m_FocusKey = "Focus";
		[SerializeField] private string m_UnfocusKey = "Unfocus";
		[SerializeField] private float m_Duration = 0.5f;

		// Data
		private readonly static bool[] ItemLock = { false, false, false, false, false, };
		private readonly static int[] ItemLayers = { -1, -1, -1, -1, -1 };
		private readonly static LayerMask[] ItemMasks = { -1, -1, -1, -1, -1, };
		private bool FocusMode = false;
		private bool UIReady = true;
		private int SortingLayerID_UI = -1;
		private Coroutine FocusAniCor = null;
		private Camera _Camera = null;
		private LayerMask UnlockedMask = default;

		// Mouse
		private readonly static List<(int, int)> SelectingObjectsIndex = new List<(int, int)>();
		private readonly static RaycastHit[] CastHits = new RaycastHit[64];
		private bool ClickStartInsideSelection = false;
		private int HoldNoteLayer = -1;
		private Ray? MouseRayDown = null;
		private Vector2 HoverScaleMuti = Vector2.one;
		private Vector2 GhostPivotScaleMuti = Vector2.one;


		#endregion




		#region --- MSG ---


		private void Awake () {

			// Init Layer
			for (int i = 0; i < ITEM_LAYER_NAMES.Length; i++) {
				ItemLayers[i] = LayerMask.NameToLayer(ITEM_LAYER_NAMES[i]);
			}
			HoldNoteLayer = LayerMask.NameToLayer(m_NoteHoldLayerName);
			SortingLayerID_UI = SortingLayer.NameToID("UI");

			// Unlock Mask
			RefreshUnlockedMask();

			// ItemMasks
			for (int i = 0; i < ITEM_LAYER_NAMES.Length; i++) {
				ItemMasks[i] = LayerMask.GetMask(ITEM_LAYER_NAMES[i]);
			}

			// Eye TGs
			for (int i = 0; i < m_EyeTGs.Length; i++) {
				int index = i;
				var tg = m_EyeTGs[index];
				tg.isOn = false;
				tg.onValueChanged.AddListener((isOn) => {
					if (!UIReady) { return; }
					SetEye(index, !isOn);
				});
			}

			// Lock TGs
			for (int i = 0; i < m_LockTGs.Length; i++) {
				int index = i;
				var tg = m_LockTGs[index];
				tg.isOn = ItemLock[index];
				tg.onValueChanged.AddListener((locked) => {
					if (!UIReady) { return; }
					SetLock(index, locked);
				});
			}

			// UI
			HoverScaleMuti = m_Hover.transform.localScale;
			GhostPivotScaleMuti = m_GhostPivot.transform.localScale;

		}


		private void LateUpdate () {
			// Check
			if (!AntiTargetAllow()) {
				MouseRayDown = null;
				if (m_Grid.GridEnabled) {
					m_Grid.SetGridTransform(false);
				}
				SetTargetActive(m_Hover.gameObject, false);
				return;
			}
			// Mouse
			if (Input.GetMouseButton(0)) {
				if (!MouseRayDown.HasValue) {
					// Down
					MouseRayDown = GetMouseRay();
					OnMouseLeftDown();
				} else {
					// Drag
					OnMouseLeftDrag();
				}
			} else {
				// Normal
				if (MouseRayDown.HasValue) {
					MouseRayDown = null;
				}
			}
			OnMouseHover(GetBeatmap());
		}


		// Mouse Left
		private void OnMouseLeftDown () {
			var (_, brushIndex) = GetBrushTypeIndex();
			if (brushIndex < 0) {
				// Select or Move
				var map = GetBeatmap();
				bool ctrl = Input.GetKey(KeyCode.LeftControl);
				bool alt = Input.GetKey(KeyCode.LeftAlt);
				int maxSelectingLayer = GetMaxSelectingLayer();
				var (overlapLayer, overlapIndex, _) = GetCastLayerIndex(GetMouseRay(), UnlockedMask, true);
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
			//var plane = new Plane(Vector3.back, zoneMin);
			var (_, brushIndex) = GetBrushTypeIndex();
			if (brushIndex < 0 && ClickStartInsideSelection) {
				// Moving Selection





			}
		}


		private void OnMouseHover (Beatmap map) {

			var (brushType, brushIndex) = GetBrushTypeIndex();

			// --- Painting ---
			bool brushUnlocked = !GetItemLock(brushType);
			if (brushIndex >= 0 && !brushUnlocked) {
				SetTargetActive(m_Hover.gameObject, false);
				SetTargetActive(m_Ghost.gameObject, false);
			}
			if (brushIndex >= 0 && brushUnlocked) {

				// Hover
				SetTargetActive(m_Hover.gameObject, false);

				// Paint Grid
				bool gridEnable = false;
				Vector3 pos = default;
				Quaternion rot = default;
				Vector3 scl = default;
				float speed = 1f;
				var ray = GetMouseRay();
				var (zoneMin, zoneMax, zoneSize, zoneRatio) = GetZoneMinMax();
				int hoverItemLayer = -1;
				int hoverItemIndex = -1;
				Transform hoverTarget = null;
				if (brushType > 0) {
					(hoverItemLayer, hoverItemIndex, hoverTarget) = GetCastLayerIndex(ray, ItemMasks[brushType - 1], true);
					if (hoverTarget != null) {
						gridEnable = true;
						pos = hoverTarget.position;
						rot = hoverTarget.rotation;
						scl = hoverTarget.GetChild(0).localScale;
						speed = hoverItemLayer == ItemLayers[1] && GetUseDynamicSpeed() ? GetTrackSpeedMuti(map, hoverItemIndex) : 1f;
					}
					m_Grid.Mode = hoverItemLayer == ItemLayers[0] ? 1 : 2;
				} else {
					gridEnable = true;
					pos = Util.Vector3Lerp3(zoneMin, zoneMax, 0.5f, 0f);
					rot = Quaternion.identity;
					scl = new Vector3(zoneSize, zoneSize / zoneRatio, 1f);
					m_Grid.Mode = 0;
				}
				m_Grid.ObjectSpeedMuti = speed;
				m_Grid.SetGridTransform(gridEnable, pos, rot, scl);
				// Ghost
				bool ghostEnable = false;
				Vector2 ghostSize01 = default;
				Vector3 ghostPos = default;
				Quaternion ghostRot = Quaternion.identity;
				switch (brushType) {
					case 0: // Stage
						var stage = GetDefaultStageBrush();
						if (stage != null && !GetUseAbreast()) {
							var mousePos = GetRayPosition(ray, null, true);
							ghostEnable = mousePos.HasValue;
							if (mousePos.HasValue) {
								ghostSize01.x = zoneSize * stage.Width;
								ghostSize01.y = zoneSize * stage.Height / zoneRatio;
								ghostPos = mousePos.Value;
								ghostPos = m_Grid.SnapWorld(ghostPos);
							}
						}
						break;
					case 1: // Track
						var track = GetDefaultTrackBrush();
						if (track != null && hoverTarget != null) {
							var mousePos = GetRayPosition(ray, null, true);
							ghostEnable = mousePos.HasValue;
							if (mousePos.HasValue) {
								ghostSize01.x = track.Width * zoneSize;
								ghostSize01.y = hoverTarget.GetChild(0).localScale.y;
								ghostPos = mousePos.Value;
								ghostPos = m_Grid.SnapWorld(ghostPos, true);
								ghostRot = hoverTarget.transform.rotation;
							}
						}
						break;
					case 2: // Note
						var notes = GetNotesBrushAt(brushIndex);
						if (notes != null && notes.Count > 0 && hoverTarget != null) {
							var note = notes[0];
							var mousePos = GetRayPosition(ray, hoverTarget, false);
							ghostEnable = mousePos.HasValue;
							if (mousePos.HasValue) {
								ghostSize01.x = note.Width * zoneSize;
								ghostSize01.y = Mathf.Max(note.Duration, 0.032f) * speed * hoverTarget.GetChild(0).localScale.y;
								ghostPos = mousePos.Value;
								ghostPos = m_Grid.SnapWorld(ghostPos, false, true);
								ghostRot = hoverTarget.transform.rotation;
							}
						}
						break;
				}
				SetTargetActive(m_Ghost.gameObject, ghostEnable);
				if (ghostEnable) {
					m_Ghost.Size = ghostSize01;
					m_Ghost.transform.localScale = ghostSize01;
					m_GhostPivot.transform.localScale = new Vector3(GhostPivotScaleMuti.x / ghostSize01.x, GhostPivotScaleMuti.y / ghostSize01.y, 1f);
					m_Ghost.transform.position = ghostPos;
					m_Ghost.transform.rotation = ghostRot;
					m_Ghost.SetSortingLayer(SortingLayerID_UI, short.MaxValue - 1);
				}
			}


			// --- Normal Select ---
			if (brushIndex < 0) {


				SetTargetActive(m_Ghost.gameObject, false);

				// Grid
				if (m_Grid.GridEnabled) {
					m_Grid.SetGridTransform(false);
				}
				// Hover
				var ray = GetMouseRay();
				var (_, _, target) = GetCastLayerIndex(ray, UnlockedMask, true);
				if (target != null) {
					SetTargetActive(m_Hover.gameObject, true);
					m_Hover.transform.position = target.GetChild(0).position;
					m_Hover.transform.rotation = target.GetChild(0).rotation;
					m_Hover.size = target.GetChild(0).localScale / HoverScaleMuti;
				} else {
					SetTargetActive(m_Hover.gameObject, false);
				}
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
				case int hl when hl == HoldNoteLayer:
					if (index >= 0 && index < map.Notes.Count) {
						return map.Notes[index].Selecting;
					}
					break;
				case int l when l == ItemLayers[3]:
					if (index >= 0 && index < map.TimingNotes.Count) {
						return map.TimingNotes[index].Selecting;
					}
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
				case int hl when hl == HoldNoteLayer:
					if (index >= 0 && index < map.Notes.Count) {
						map.Notes[index].Selecting = select;
					}
					break;
				case int l when l == ItemLayers[3]:
					if (index >= 0 && index < map.TimingNotes.Count) {
						map.TimingNotes[index].Selecting = select;
					}
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
							if (index >= 0 && index < map.Stages.Count) {
								map.Stages[index].Selecting = false;
							}
							break;
						case int l when l == ItemLayers[1]:
							if (index >= 0 && index < map.Tracks.Count) {
								map.Tracks[index].Selecting = false;
							}
							break;
						case int l when l == ItemLayers[2]:
						case int hl when hl == HoldNoteLayer:
							if (index >= 0 && index < map.Notes.Count) {
								map.Notes[index].Selecting = false;
							}
							break;
						case int l when l == ItemLayers[3]:
							if (index >= 0 && index < map.TimingNotes.Count) {
								map.TimingNotes[index].Selecting = false;
							}
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


		public void ForAddSelectingObjects (System.Action<int, int> action) {   // type, index 
			foreach (var (layer, index) in SelectingObjectsIndex) {
				int type = -1;
				switch (layer) {
					case int l when l == ItemLayers[0]:
						type = 0;
						break;
					case int l when l == ItemLayers[1]:
						type = 1;
						break;
					case int l when l == ItemLayers[2]:
					case int hl when hl == HoldNoteLayer:
						type = 2;
						break;
					case int l when l == ItemLayers[3]:
						type = 3;
						break;
					case int l when l == ItemLayers[4]:
						type = 4;
						break;
				}
				if (type >= 0) {
					action(type, index);
				}
			}
		}


		// Container
		public void UI_SwitchContainerActive (int index) => SetEye(index, !GetContainerActive(index));


		public bool GetContainerActive (int index) => m_Containers[index].gameObject.activeSelf;


		public void SetContainerActive (int index, bool active) => SetEye(index, active);


		// Item Lock
		public bool GetItemLock (int item) => item >= 0 ? ItemLock[item] : false;


		public void UI_SwitchLock (int index) => SetLock(index, !GetItemLock(index));


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


		private bool AntiTargetAllow () {
			if (!GetEditorActive()) { return false; }
			foreach (var t in m_AntiTargets) {
				if (t.gameObject.activeSelf) {
					return false;
				}
			}
			return true;
		}


		private Ray GetMouseRay () => Camera.ScreenPointToRay(Input.mousePosition);


		private (int layer, int ItemIndex) GetObjectIndexFromCollider (RaycastHit hit) => (hit.transform.gameObject.layer, hit.transform.parent.GetSiblingIndex());


		private void SetLock (int index, bool locked) {
			// Set Logic
			ItemLock[index] = locked;
			// Refresh Unlock Mask
			RefreshUnlockedMask();
			// Refresh UI
			UIReady = false;
			try {
				m_LockTGs[index].isOn = locked;
			} catch { }
			UIReady = true;
			OnLockEyeChanged();
		}


		private void SetEye (int index, bool see) {
			m_Containers[index].gameObject.SetActive(see);
			// UI
			UIReady = false;
			try {
				m_EyeTGs[index].isOn = !see;
			} catch { }
			UIReady = true;
			// MSG
			OnLockEyeChanged();
		}


		private (int layer, int index, Transform target) GetCastLayerIndex (Ray ray, LayerMask mask, bool insideZone) {
			int count = Physics.RaycastNonAlloc(ray, CastHits, float.MaxValue, mask);
			int overlapLayer = -1;
			int overlapIndex = -1;
			Transform tf = null;
			if (!insideZone || RayInsideZone(ray)) {
				for (int i = 0; i < count; i++) {
					var (layerIndex, itemIndex) = GetObjectIndexFromCollider(CastHits[i]);
					if (layerIndex >= overlapLayer) {
						if (layerIndex != overlapLayer) {
							overlapIndex = -1;
						}
						if (itemIndex >= overlapIndex) {
							tf = CastHits[i].transform.parent;
						}
						overlapIndex = Mathf.Max(itemIndex, overlapIndex);
						overlapLayer = layerIndex;
					}
				}
			}
			return (overlapLayer, overlapIndex, tf);
		}


		private int GetMaxSelectingLayer () {
			int maxSelectingLayer = -1;
			foreach (var (layerIndex, _) in SelectingObjectsIndex) {
				maxSelectingLayer = Mathf.Max(maxSelectingLayer, layerIndex);
			}
			return maxSelectingLayer;
		}


		private float GetTrackSpeedMuti (Beatmap map, int index) {
			if (index >= 0 && index < map.Tracks.Count) {
				int sIndex = map.Tracks[index].StageIndex;
				if (sIndex >= 0 && sIndex < map.Stages.Count) {
					return map.Stages[sIndex].SpeedMuti;
				}
			}
			return 1f;
		}


		private bool RayInsideZone (Ray ray) {
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			if (new Plane(Vector3.back, zoneMin).Raycast(ray, out float enter)) {
				var point = ray.GetPoint(enter);
				return point.x > zoneMin.x && point.x < zoneMax.x && point.y > zoneMin.y && point.y < zoneMax.y;
			}
			return false;
		}


		private Vector3? GetRayPosition (Ray ray, Transform planeTarget = null, bool clampInZone = false) {
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			if (new Plane(
				planeTarget != null ? -planeTarget.forward : Vector3.back,
				planeTarget != null ? planeTarget.position : zoneMin
			).Raycast(ray, out float enter)) {
				var point = ray.GetPoint(enter);
				if (clampInZone) {
					point.x = Mathf.Clamp(point.x, zoneMin.x, zoneMax.x);
					point.y = Mathf.Clamp(point.y, zoneMin.y, zoneMax.y);
					point.z = Mathf.Clamp(point.z, zoneMin.z, zoneMax.z);
				}
				return point;
			}
			return null;
		}


		private void RefreshUnlockedMask () {
			var list = new List<string>();
			for (int i = 0; i < ItemLock.Length; i++) {
				if (!ItemLock[i]) {
					list.Add(ITEM_LAYER_NAMES[i]);
				}
			}
			// Hold Note Layer
			if (!ItemLock[2]) {
				list.Add(m_NoteHoldLayerName);
			}
			UnlockedMask = LayerMask.GetMask(list.ToArray());
		}


		private void SetTargetActive (GameObject target, bool active) {
			if (target.activeSelf != active) {
				target.SetActive(active);
			}
		}


		#endregion



	}
}