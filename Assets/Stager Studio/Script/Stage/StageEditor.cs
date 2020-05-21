﻿namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;
	using Rendering;
	using Saving;
	using Object;
	using UI;



	public class StageEditor : MonoBehaviour {




		#region --- SUB ---


		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();
		public delegate void VoidHandler ();
		public delegate float FloatHandler ();
		public delegate void VoidFloatHandler (float f);
		public delegate Beatmap BeatmapHandler ();
		public delegate bool BoolHandler ();
		public delegate void VoidStringBoolHandler (string str, bool b);
		public delegate string StringStringHandler (string str);
		public delegate float FillHandler (float time, float fill, float muti);
		public delegate void LogAxisHintHandler (int axis, string hint);
		public delegate float SnapTimeHandler (float time, float gap, float offset);


		#endregion




		#region --- VAR ---


		// Const
		private readonly static string[] ITEM_LAYER_NAMES = { "Stage", "Track", "Note", "Timing", "Stage Timer", "Track Timer", };
		private const string HINT_GlobalBrushScale = "Editor.Hint.GlobalBrushScale";
		private const string HINT_Copy = "Editor.Hint.Copy";
		private const string HINT_CopyFail = "Editor.Hint.CopyFail";
		private const string HINT_Paste = "Editor.Hint.Paste";
		private const string DIALOG_CannotSelectStageBrush = "Dialog.Editor.CannotSelectStageBrush";
		private const string DIALOG_SelectingLayerLocked = "Dialog.Editor.SelectingLayerLocked";
		private const string DIALOG_SelectingLayerInactive = "Dialog.Editor.SelectingLayerInactive";
		private const string HOLD_NOTE_LAYER_NAME = "HoldNote";

		// Handle
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static ZoneHandler GetRealZoneMinMax { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetMusicDuration { get; set; } = null;
		public static BoolHandler GetEditorActive { get; set; } = null;
		public static VoidHandler OnSelectionChanged { get; set; } = null;
		public static VoidHandler OnObjectEdited { get; set; } = null;
		public static VoidHandler OnLockEyeChanged { get; set; } = null;
		public static BoolHandler GetUseDynamicSpeed { get; set; } = null;
		public static BoolHandler GetUseAbreast { get; set; } = null;
		public static VoidStringBoolHandler LogHint { get; set; } = null;
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static BoolHandler GetMoveAxisHovering { get; set; } = null;
		public static FillHandler GetFilledTime { get; set; } = null;
		public static VoidFloatHandler SetAbreastIndex { get; set; } = null;
		public static LogAxisHintHandler LogAxisMessage { get; set; } = null;
		public static SnapTimeHandler GetSnapedTime { get; set; } = null;

		// Api
		public int SelectingItemType { get; private set; } = -1;
		public int SelectingItemIndex { get; private set; } = -1;
		public int SelectingItemSubIndex { get; private set; } = -1;
		public int SelectingBrushIndex { get; private set; } = -1;
		public float StageBrushWidth { get; set; } = 1f;
		public float StageBrushHeight { get; set; } = 1f;
		public float TrackBrushWidth { get; set; } = 0.2f;
		public float NoteBrushWidth { get; set; } = 0.2f;
		public bool AxisDragging { get; private set; } = false;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);
		private Transform AxisMoveX => m_MoveAxis.transform.GetChild(0);
		private Transform AxisMoveY => m_MoveAxis.transform.GetChild(1);
		private Transform AxisMoveXY => m_MoveAxis.transform.GetChild(2);
		private Transform AxisWidth => m_MoveAxis.transform.GetChild(3);
		private Transform AxisDuration => m_MoveAxis.transform.GetChild(4);

		// Ser
		[SerializeField] private Toggle[] m_EyeTGs = null;
		[SerializeField] private Toggle[] m_LockTGs = null;
		[SerializeField] private Transform[] m_Containers = null;
		[SerializeField] private Transform[] m_AntiTargets = null;
		[SerializeField] private Toggle[] m_DefaultBrushTGs = null;
		[SerializeField] private GridRenderer m_Grid = null;
		[SerializeField] private SpriteRenderer m_Hover = null;
		[SerializeField] private LoopUvRenderer m_Ghost = null;
		[SerializeField] private LoopUvRenderer m_Highlight = null;
		[SerializeField] private LoopUvRenderer m_Erase = null;
		[SerializeField] private AxisHandleUI m_MoveAxis = null;
		[SerializeField] private Sprite m_HoverSP_Item = null;
		[SerializeField] private Sprite m_HoverSP_Timer = null;
		[SerializeField] private RectTransform m_EraseUI = null;

		// Data
		private readonly static Dictionary<int, float> LayerToTypeMap = new Dictionary<int, float>();
		private readonly static bool[] ItemLock = { false, false, false, false, };
		private readonly static byte[] CopyBuffer = new byte[1024 * 512]; // 500KB
		private readonly static LayerMask[] ItemMasks = { -1, -1, -1, -1, -1, -1, };
		private int CopyType = -1;
		private int CopyLength = 0;
		private int SortingLayerID_UI = -1;
		private bool UIReady = true;
		private Camera _Camera = null;
		private LayerMask UnlockedMask = default;

		// Mouse
		private readonly static RaycastHit[] CastHits = new RaycastHit[64];
		private int HoldNoteLayer = -1;
		private Vector2 HoverScaleMuti = Vector2.one;
		private Vector3 DragOffsetWorld = default;

		// Saving
		public SavingBool UseGlobalBrushScale { get; set; } = new SavingBool("StageEditor.UseGlobalBrushScale", true);
		public SavingBool ShowGridOnSelect { get; set; } = new SavingBool("StageEditor.ShowGridOnSelect", false);


		#endregion




		#region --- MSG ---


		private void Awake () {

			// Init Layer
			SortingLayerID_UI = SortingLayer.NameToID("UI");
			for (int i = 0; i < ITEM_LAYER_NAMES.Length; i++) {
				LayerToTypeMap.Add(LayerMask.NameToLayer(ITEM_LAYER_NAMES[i]), i + 0.5f);
			}
			HoldNoteLayer = LayerMask.NameToLayer(HOLD_NOTE_LAYER_NAME);
			LayerToTypeMap.Add(HoldNoteLayer, 2.4f);

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
					SetContainerActive(index, !isOn);
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

			// Init Brush
			for (int i = 0; i < m_DefaultBrushTGs.Length; i++) {
				int index = i;
				m_DefaultBrushTGs[index].onValueChanged.AddListener((isOn) => {
					if (!UIReady) { return; }
					SetBrushLogic(isOn ? index : -1);
				});
			}

			// UI
			HoverScaleMuti = m_Hover.transform.localScale;

			m_Ghost.SetSortingLayer(SortingLayerID_UI, 0);
			m_Highlight.SetSortingLayer(SortingLayerID_UI, 0);
			m_Erase.SetSortingLayer(SortingLayerID_UI, 0);

		}


		private void LateUpdate () {
			if (GetEditorActive() && AntiTargetAllow()) {
				// Editor Active
				var map = GetBeatmap();
				LateUpdate_Key(map);
				LateUpdate_Selection();
				LateUpdate_Hover(map);
				LateUpdate_Down(map);
				LateUpdate_Axis(map);
				LateUpdate_Highlight();
			} else {
				// Editor InActive
				TrySetGridInactive();
				m_Hover.gameObject.TrySetActive(false);
				m_Ghost.gameObject.TrySetActive(false);
				m_MoveAxis.gameObject.TrySetActive(false);
				m_Highlight.gameObject.TrySetActive(false);
				m_Erase.gameObject.TrySetActive(false);
				AxisDragging = false;
				if (SelectingBrushIndex != -1) {
					SetBrushLogic(-1);
				}
				return;
			}
		}


		private void LateUpdate_Key (Beatmap map) {

			bool hasSelection = SelectingItemType >= 0 && SelectingItemIndex >= 0;

			if (hasSelection && (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))) {
				// Del
				map.DeleteItem(SelectingItemType, SelectingItemIndex);
				OnObjectEdited();
			}

			if (hasSelection && (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.X)) && Input.GetKey(KeyCode.LeftControl)) {
				// Copy
				var item = map.GetItem(SelectingItemType, SelectingItemIndex);
				CopyLength = Util.ObjectToBytes(item, CopyBuffer, 0);
				CopyType = SelectingItemType;
				if (CopyLength > 0) {
					// Copy Success
					if (Input.GetKeyDown(KeyCode.X)) {
						// Delete If Cut
						map.DeleteItem(SelectingItemType, SelectingItemIndex);
						OnObjectEdited();
					}
					// Hint
					LogHint(GetLanguage(HINT_Copy), true);
				} else {
					// Clear Buffer on Fail
					for (int i = 0; i < CopyBuffer.Length; i++) {
						CopyBuffer[i] = 0;
					}
					CopyType = -1;
					// Hint
					LogHint(GetLanguage(HINT_CopyFail), true);
				}
			}

			if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl)) {
				// Paste
				if (CopyType >= 0 && CopyLength > 0) {
					var obj = Util.BytesToObject(CopyBuffer, 0, CopyLength);
					if (obj != null && obj is Beatmap.MapItem mObj) {
						mObj.Time = GetSnapedTime(GetMusicTime(), m_Grid.TimeGap, m_Grid.TimeOffset);
						switch (CopyType) {
							case 0: // Stage
								if (obj is Beatmap.Stage sObj) {
									map.AddStage(sObj);
									SetSelection(0, map.Stages.Count - 1);
									OnObjectEdited();
									// Hint
									LogHint(GetLanguage(HINT_Paste), true);
								}
								break;
							case 1: // Track
								if (obj is Beatmap.Track tObj) {
									if (SelectingItemType == 0) {
										tObj.StageIndex = SelectingItemIndex;
									}
									map.AddTrack(tObj);
									SetSelection(1, map.Tracks.Count - 1);
									OnObjectEdited();
									// Hint
									LogHint(GetLanguage(HINT_Paste), true);
								}
								break;
							case 2: // Note
								if (obj is Beatmap.Note nObj) {
									nObj.LinkedNoteIndex = -1;
									if (SelectingItemType == 1) {
										nObj.TrackIndex = SelectingItemIndex;
									}
									map.AddNote(nObj);
									SetSelection(2, map.Notes.Count - 1);
									OnObjectEdited();
									// Hint
									LogHint(GetLanguage(HINT_Paste), true);
								}
								break;
							case 3: // Timing
								if (obj is Beatmap.Timing tiObj) {
									map.AddTiming(tiObj);
									SetSelection(3, map.Timings.Count - 1);
									OnObjectEdited();
									// Hint
									LogHint(GetLanguage(HINT_Paste), true);
								}
								break;
						}
					}
				}
			}

		}


		private void LateUpdate_Selection () {
			if (SelectingItemIndex >= 0 && SelectingBrushIndex != -1) {
				SelectingItemIndex = -1;
				SelectingItemSubIndex = -1;
				SelectingItemType = -1;
				OnSelectionChanged();
			}
		}


		private void LateUpdate_Hover (Beatmap map) {
			if (SelectingBrushIndex >= 0) {
				// --- Painting ---
				if (GetItemLock(SelectingBrushIndex)) {
					m_Ghost.gameObject.TrySetActive(false);
					TrySetGridInactive();
				} else {
					OnMouseHover_Grid(map, false);
					OnMouseHover_Ghost();
				}
				m_Hover.gameObject.TrySetActive(false);
				m_Erase.gameObject.TrySetActive(false);
			} else if (SelectingBrushIndex == -2) {
				// --- Erase ---
				OnMouseHover_Erase();
				TrySetGridInactive();
				m_Hover.gameObject.TrySetActive(false);
				m_Ghost.gameObject.TrySetActive(false);
			} else {
				// --- Normal Select ---
				OnMouseHover_Normal();
				OnMouseHover_Grid(map, true);
				m_Ghost.gameObject.TrySetActive(false);
				m_Erase.gameObject.TrySetActive(false);
			}
		}


		private void LateUpdate_Down (Beatmap map) {
			if (GetMoveAxisHovering() || !Input.GetMouseButtonDown(0)) { return; }
			if (SelectingBrushIndex == -1) {
				// Select or Move
				var ray = GetMouseRay();
				var (overlapType, overlapIndex, overlapSubIndex, _) = GetCastTypeIndex(ray, UnlockedMask, true);
				if (SelectingItemIndex < 0 || overlapType != SelectingItemType || overlapIndex != SelectingItemIndex) {
					// Select
					if (RayInsideZone(ray, true, false)) {
						ClearSelection();
					}
					if (overlapType >= 0 && overlapIndex >= 0) {
						SetSelection(overlapType, overlapIndex, overlapSubIndex);
					}
				}
			} else if (SelectingBrushIndex == -2) {
				// Erase
				var ray = GetMouseRay();
				var (type, index, _, _) = GetCastTypeIndex(ray, UnlockedMask, true);
				if (type < 4) {
					// Stage Track Note Timing
					map.DeleteItem(type, index);
					OnObjectEdited();
				}
			} else {
				var ray = GetMouseRay();
				if (!RayInsideZone(ray) || !m_Ghost.RendererEnable || GetItemLock(SelectingBrushIndex)) { return; }
				// Paint
				switch (SelectingBrushIndex) {
					case 0: { // Stage
						var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
						var ghostWorldPos = m_Ghost.transform.position;
						var zonePos = Util.Vector3InverseLerp3(
							zoneMin, zoneMax,
							ghostWorldPos.x, ghostWorldPos.y
						);
						float musicTime = GetMusicTime();
						if (m_Grid.RendererEnable) {
							musicTime = GetSnapedTime(musicTime, m_Grid.TimeGap, m_Grid.TimeOffset);
						}
						float musicDuration = GetMusicDuration();
						map.AddStage(
							musicTime,
							musicDuration - musicTime,
							zonePos.x,
							zonePos.y,
							StageBrushWidth,
							StageBrushHeight
						);
						OnObjectEdited();
					}
					break;
					case 1: { // Track
						var (type, index, _, _) = GetCastTypeIndex(ray, ItemMasks[0], true);
						if (type != 0 || index < 0 || index >= map.Stages.Count) { break; }
						float musicTime = GetMusicTime();
						if (m_Grid.RendererEnable) {
							musicTime = GetSnapedTime(musicTime, m_Grid.TimeGap, m_Grid.TimeOffset);
						}
						var stage = map.Stages[index];
						if (stage.duration <= 0) { break; }
						var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
						var ghostWorldPos = m_Ghost.transform.position;
						var zonePos = Util.Vector3InverseLerp3(
							zoneMin, zoneMax,
							ghostWorldPos.x, ghostWorldPos.y
						);
						float stageWidth = Mathf.Max(Stage.GetStageWidth(stage), 0.001f);
						float trackX = Stage.ZoneToLocal(
							zonePos.x, zonePos.y, zonePos.z,
							Stage.GetStagePosition(stage, index),
							stageWidth,
							Stage.GetStageHeight(stage),
							Stage.GetStagePivotY(stage),
							Stage.GetStageWorldRotationZ(stage)
						).x;
						map.AddTrack(
							index,
							musicTime,
							Mathf.Max(stage.Duration - (musicTime - stage.Time), 0.001f),
							trackX,
							UseGlobalBrushScale.Value ? TrackBrushWidth / stageWidth : TrackBrushWidth
						);
						OnObjectEdited();
					}
					break;
					case 2: { // Note 
						var (type, index, _, _) = GetCastTypeIndex(ray, ItemMasks[1], true);
						if (type != 1 || index < 0 || index >= map.Tracks.Count) { break; }
						var track = map.Tracks[index];
						if (track.StageIndex < 0 || track.StageIndex >= map.Stages.Count) { break; }
						var stage = map.Stages[track.StageIndex];
						float stageWidth = Mathf.Max(Stage.GetStageWidth(stage), 0.001f);
						float trackWidth = Mathf.Max(Track.GetTrackWidth(track), 0.001f);
						var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
						var ghostWorldPos = m_Ghost.transform.position;
						var zonePos = Util.Vector3InverseLerp3(
							zoneMin, zoneMax,
							ghostWorldPos.x, ghostWorldPos.y, ghostWorldPos.z
						);
						var localPos = Track.ZoneToLocal(
							zonePos.x, zonePos.y, zonePos.z,
							Stage.GetStagePosition(stage, track.StageIndex),
							stageWidth,
							Stage.GetStageHeight(stage),
							Stage.GetStagePivotY(stage),
							Stage.GetStageWorldRotationZ(stage),
							Track.GetTrackX(track),
							trackWidth,
							Track.GetTrackAngle(track)
						);
						map.AddNote(
							index,
							GetFilledTime(GetMusicTime(), localPos.y, m_Grid.SpeedMuti * m_Grid.ObjectSpeedMuti),
							0f,
							localPos.x,
							UseGlobalBrushScale.Value ? NoteBrushWidth / trackWidth / stageWidth : NoteBrushWidth
						);
						OnObjectEdited();
					}
					break;
					case 3: { // Timing
						var (zoneMin, zoneMax_real, _, _) = GetRealZoneMinMax();
						var ghostWorldPos = m_Ghost.transform.position;
						var zonePos = Util.Vector3InverseLerp3(
							zoneMin, zoneMax_real,
							ghostWorldPos.x, ghostWorldPos.y, ghostWorldPos.z
						);
						float timingTime = GetFilledTime(GetMusicTime(), zonePos.y, m_Grid.SpeedMuti);
						if (m_Grid.RendererEnable) {
							timingTime = GetSnapedTime(timingTime, m_Grid.TimeGap, m_Grid.TimeOffset);
						}
						map.AddTiming(timingTime, 1f);
						OnObjectEdited();
					}
					break;
				}
			}
		}


		private void LateUpdate_Axis (Beatmap map) {
			var con = SelectingItemType >= 0 && SelectingItemType < m_Containers.Length ? m_Containers[SelectingItemType] : null;
			bool targetActive = map.GetActive(SelectingItemType, SelectingItemIndex);
			if (
				con != null && SelectingItemIndex >= 0 && SelectingItemIndex < con.childCount &&
				(targetActive || Input.GetMouseButton(0))
			) {
				var target = con.GetChild(SelectingItemIndex);
				if ((SelectingItemType == 4 || SelectingItemType == 5) && SelectingItemSubIndex >= 0 && SelectingItemSubIndex < target.childCount) {
					target = target.GetChild(SelectingItemSubIndex);
				}
				// Pos
				var pos = target.position;
				if (m_MoveAxis.transform.position != pos) {
					m_MoveAxis.transform.position = pos;
				}
				// Rot
				Quaternion rot;
				if (SelectingItemType == 0) {
					// Stage
					rot = Quaternion.identity;
				} else if (SelectingItemType <= 3) {
					// Track Note Timing
					var pIndex = map.GetParentIndex(SelectingItemType, SelectingItemIndex);
					var pTarget = pIndex >= 0 ? m_Containers[SelectingItemType - 1].GetChild(pIndex) : null;
					if (pTarget != null) {
						rot = pTarget.GetChild(0).rotation;
					} else {
						rot = target.GetChild(0).rotation;
					}
				} else {
					// Timer
					rot = target.rotation;
				}
				if (m_MoveAxis.transform.rotation != rot) {
					m_MoveAxis.transform.rotation = rot;
				}
				// Handle Active
				bool editorActive = GetEditorActive();
				bool xActive = editorActive && SelectingItemType != 3 && SelectingItemType != 4 && SelectingItemType != 5;
				bool yActive = editorActive && SelectingItemType != 1;
				bool xyActive = xActive && yActive;
				bool wActive = editorActive && SelectingItemType != 3 && SelectingItemType != 4 && SelectingItemType != 5;
				bool dActive = editorActive && SelectingItemType == 2;
				if (AxisMoveX.gameObject.activeSelf != xActive) {
					AxisMoveX.gameObject.SetActive(xActive);
				}
				if (AxisMoveY.gameObject.activeSelf != yActive) {
					AxisMoveY.gameObject.SetActive(yActive);
				}
				if (AxisMoveXY.gameObject.activeSelf != xyActive) {
					AxisMoveXY.gameObject.SetActive(xyActive);
				}
				if (AxisWidth.gameObject.activeSelf != wActive) {
					AxisWidth.gameObject.SetActive(wActive);
				}
				if (AxisDuration.gameObject.activeSelf != dActive) {
					AxisDuration.gameObject.SetActive(dActive);
				}
				// Width Position
				if (wActive) {
					var target0 = target.GetChild(0);
					AxisWidth.transform.position = target0.position - target0.right * target0.localScale.x * 0.5f;
				}
				// Duration Position
				if (dActive) {
					var target0 = target.GetChild(0);
					AxisDuration.transform.position = target0.position + target0.up * target0.localScale.y;
				}
				// Axis Renderers
				m_MoveAxis.SetAxisRendererActive(targetActive);

				// Active
				m_MoveAxis.gameObject.TrySetActive(true);
				AxisDragging = AxisDragging && Input.GetMouseButton(0);
			} else {
				m_MoveAxis.gameObject.TrySetActive(false);
				AxisDragging = false;
			}
		}


		private void LateUpdate_Highlight () {
			var con = SelectingItemType >= 0 && SelectingItemType < 3 ? m_Containers[SelectingItemType] : null;
			bool active = con != null && SelectingItemIndex >= 0 && SelectingItemIndex < con.childCount && con.GetChild(SelectingItemIndex).gameObject.activeSelf;
			m_Highlight.gameObject.TrySetActive(active);
			if (active) {
				var target0 = con.GetChild(SelectingItemIndex).GetChild(0);
				var scale = target0.localScale;
				m_Highlight.transform.position = target0.position;
				m_Highlight.transform.rotation = target0.rotation;
				m_Highlight.transform.localScale = scale;
				m_Highlight.Size = scale * 24f;
			}
		}


		// Hover
		private void OnMouseHover_Grid (Beatmap map, bool selectingMode) {

			// Check
			if (selectingMode && SelectingItemIndex < 0) {
				TrySetGridInactive();
				return;
			}

			// Paint Grid
			bool gridEnable = false;
			Vector3 pos = default;
			Quaternion rot = default;
			Vector3 scl = default;
			var ray = GetMouseRay();
			var (zoneMin, zoneMax, zoneSize, zoneRatio) = GetRealZoneMinMax();
			int gridingItemType = selectingMode ? SelectingItemType : SelectingBrushIndex;
			switch (gridingItemType) {
				case 0: // Stage
					gridEnable = true;
					pos = Util.Vector3Lerp3(zoneMin, zoneMax, 0.5f, 0f);
					rot = Quaternion.identity;
					scl = new Vector3(zoneSize, zoneSize / zoneRatio, 1f);
					m_Grid.Mode = 0;
					m_Grid.IgnoreDynamicSpeed = false;
					m_Grid.ObjectSpeedMuti = 1f;
					break;
				case 1: // Track
				case 2:  // Note
					int hoverItemType, hoverItemIndex;
					Transform hoverTarget;
					if (selectingMode) {
						hoverItemType = gridingItemType - 1;
						hoverItemIndex = map.GetParentIndex(gridingItemType, SelectingItemIndex);
						hoverTarget = hoverItemIndex >= 0 ? m_Containers[hoverItemType].GetChild(hoverItemIndex) : null;
					} else {
						(hoverItemType, hoverItemIndex, _, hoverTarget) = GetCastTypeIndex(ray, ItemMasks[gridingItemType - 1], true);
					}
					if (hoverTarget != null) {
						gridEnable = true;
						pos = hoverTarget.GetChild(0).position;
						rot = hoverTarget.rotation;
						scl = hoverTarget.GetChild(0).localScale;
						m_Grid.ObjectSpeedMuti = GetUseDynamicSpeed() ? map.GetSpeedMuti(hoverItemType, hoverItemIndex) : 1f;
					} else {
						m_Grid.ObjectSpeedMuti = 1f;
					}
					m_Grid.Mode = gridingItemType;
					m_Grid.IgnoreDynamicSpeed = false;
					break;
				case 3: // Timing
					gridEnable = true;
					scl = new Vector3(zoneSize, zoneSize / zoneRatio, 1f);
					pos = new Vector3((zoneMin.x + zoneMax.x) / 2f, zoneMin.y, zoneMin.z);
					rot = Quaternion.identity;
					m_Grid.ObjectSpeedMuti = 1f;
					m_Grid.Mode = 3;
					m_Grid.IgnoreDynamicSpeed = true;
					break;
				case 4: // Stage Timer
				case 5: // Track Timer
					if (!selectingMode) { break; }
					var target = m_Containers[gridingItemType].GetChild(SelectingItemIndex);
					gridEnable = true;
					pos = target.position;
					rot = target.rotation;
					scl = target.GetChild(0).localScale;
					m_Grid.Mode = 3;
					m_Grid.IgnoreDynamicSpeed = true;
					m_Grid.ObjectSpeedMuti = 1f / m_Grid.SpeedMuti;
					break;
			}
			m_Grid.SetGridTransform(gridEnable, ShowGridOnSelect.Value || !selectingMode, pos, rot, scl);
		}


		private void OnMouseHover_Ghost () {

			// Ghost
			var ray = GetMouseRay();
			var (zoneMin, zoneMax, zoneSize, zoneRatio) = GetRealZoneMinMax();
			bool ghostEnable = false;
			float ghostPivotX = 0.5f;
			Vector2 ghostSize = default;
			Vector3 ghostPos = default;
			Quaternion ghostRot = Quaternion.identity;
			const float GHOST_NOTE_Y = 0.032f;
			Transform hoverTarget = null;

			// Movement
			if (RayInsideZone(ray)) {
				switch (SelectingBrushIndex) {
					case 0: // Stage
						if (!GetUseAbreast()) {
							var mousePos = Util.GetRayPosition(ray, zoneMin, zoneMax, null, true);
							ghostEnable = mousePos.HasValue;
							if (mousePos.HasValue) {
								ghostSize.x = zoneSize * StageBrushWidth;
								ghostSize.y = zoneSize * StageBrushHeight / zoneRatio;
								ghostPos = mousePos.Value;
								ghostPos = m_Grid.SnapWorld(ghostPos, false);
								ghostPivotX = 0.5f;
							}
						}
						break;
					case 1: // Track
						hoverTarget = GetCastTypeIndex(ray, ItemMasks[0], true).target;
						if (hoverTarget != null) {
							var mousePos = Util.GetRayPosition(ray, zoneMin, zoneMax, null, true);
							ghostEnable = mousePos.HasValue;
							if (mousePos.HasValue) {
								ghostSize.x = UseGlobalBrushScale ? TrackBrushWidth * zoneSize : TrackBrushWidth * hoverTarget.GetChild(0).localScale.x;
								ghostSize.y = hoverTarget.GetChild(0).localScale.y;
								ghostPos = mousePos.Value;
								ghostPos = m_Grid.SnapWorld(ghostPos, true, true);
								ghostRot = hoverTarget.transform.rotation;
								ghostPivotX = 0.5f;
							}
						}
						break;
					case 2: // Note
						hoverTarget = GetCastTypeIndex(ray, ItemMasks[1], true).target;
						if (hoverTarget != null) {
							var mousePos = Util.GetRayPosition(ray, zoneMin, zoneMax, hoverTarget, false);
							ghostEnable = mousePos.HasValue;
							if (mousePos.HasValue) {
								ghostSize.x = UseGlobalBrushScale ? NoteBrushWidth * zoneSize : NoteBrushWidth * hoverTarget.GetChild(0).localScale.x;
								ghostSize.y = GHOST_NOTE_Y / zoneSize;
								ghostPos = mousePos.Value;
								ghostPos = m_Grid.SnapWorld(ghostPos, true);
								ghostRot = hoverTarget.transform.rotation;
								ghostPivotX = 0.5f;
							}
						}
						break;
					case 3: { // Speed
						var mousePos = Util.GetRayPosition(ray, zoneMin, zoneMax, null, false);
						ghostEnable = mousePos.HasValue;
						if (mousePos.HasValue) {
							ghostSize.x = 0.12f / zoneSize;
							ghostSize.y = GHOST_NOTE_Y / zoneSize;
							ghostPos.x = zoneMin.x;
							ghostPos.y = m_Grid.SnapWorld(mousePos.Value, true).y;
							ghostPos.z = zoneMin.z;
							ghostRot = Quaternion.identity;
							ghostPivotX = 0f;
						}
					}
					break;
				}
			}

			// Final
			if (ghostEnable) {
				m_Ghost.Size = ghostSize;
				m_Ghost.transform.localScale = ghostSize;
				m_Ghost.transform.position = ghostPos;
				m_Ghost.transform.rotation = ghostRot;
				m_Ghost.Pivot = new Vector3(ghostPivotX, 0f, 0f);
				m_Ghost.SetSortingLayer(SortingLayerID_UI, short.MaxValue - 1);
				m_Ghost.gameObject.TrySetActive(true);
			} else {
				m_Ghost.gameObject.TrySetActive(false);
			}
		}


		private void OnMouseHover_Normal () {
			if (GetMoveAxisHovering() || Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
				m_Hover.gameObject.TrySetActive(false);
				return;
			}
			// Hover
			var ray = GetMouseRay();
			var (type, _, subIndex, target) = GetCastTypeIndex(ray, UnlockedMask, true);
			if (target != null) {
				if (type < 4) {
					// Stage Track Note Timing
					m_Hover.transform.position = target.GetChild(0).position;
					m_Hover.transform.rotation = target.GetChild(0).rotation;
					m_Hover.size = target.GetChild(0).localScale / HoverScaleMuti;
					if (m_Hover.sprite != m_HoverSP_Item) {
						m_Hover.sprite = m_HoverSP_Item;
					}
				} else if (type < 6) {
					// Timer
					m_Hover.transform.position = target.GetChild(subIndex).position;
					m_Hover.transform.rotation = target.rotation;
					m_Hover.size = target.GetChild(subIndex).localScale / HoverScaleMuti;
					if (m_Hover.sprite != m_HoverSP_Timer) {
						m_Hover.sprite = m_HoverSP_Timer;
					}
				}
			}
			m_Hover.gameObject.TrySetActive(target != null);
		}


		private void OnMouseHover_Erase () {
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
				m_Erase.gameObject.TrySetActive(false);
				return;
			}
			var ray = GetMouseRay();
			var (type, _, _, target) = GetCastTypeIndex(ray, UnlockedMask, true);
			if (target != null) {
				if (type < 4) {
					// Stage Track Note Timing
					m_Erase.transform.position = target.GetChild(0).position;
					m_Erase.transform.rotation = target.GetChild(0).rotation;
					m_Erase.transform.localScale = target.GetChild(0).localScale;
					m_Erase.Scale = target.GetChild(0).localScale * 32f;
				}
			}
			m_Erase.gameObject.TrySetActive(target != null);
		}


		// Axis
		public void OnMoveAxisDrag (Vector3 pos, Vector3 downPos, int axis) {
			AxisDragging = true;
			var map = GetBeatmap();
			if (map == null || SelectingItemIndex < 0) { return; }
			var (zoneMin, zoneMax_real, zoneSize, _) = GetRealZoneMinMax();
			var zoneMax = zoneMax_real;
			zoneMax.y = zoneMin.y + zoneSize;
			// Fix Axis
			var axisWorldPos = m_MoveAxis.transform.position;
			bool isDown = axis < 0;
			axis = Mathf.Abs(axis) - 1;
			if (isDown) {
				DragOffsetWorld =
					axis == 3 ? downPos - AxisWidth.position :
					axis == 4 ? downPos - AxisDuration.position :
					downPos - axisWorldPos;
			}
			// Dragging
			int index = SelectingItemIndex;
			switch (SelectingItemType) {
				case 0: { // Stage
					if (index >= map.Stages.Count) { break; }
					var stage = map.Stages[index];
					Vector3 worldMotion = Stage.GetStagePosition_Motion(stage) * zoneSize;
					var worldPos = pos - DragOffsetWorld;
					var snappedPos = m_Grid.SnapWorld(worldPos - worldMotion, false);
					var snappedZonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, snappedPos.x, snappedPos.y, snappedPos.z);
					if (axis == 0 || axis == 2) {
						map.SetX(0, index, snappedZonePos.x);
						LogAxisMSG(axis, snappedZonePos.x, snappedZonePos.y);
					}
					if (axis == 1 || axis == 2) {
						map.SetStageY(index, snappedZonePos.y);
						LogAxisMSG(axis, snappedZonePos.x, snappedZonePos.y);
					}
					if (axis == 3) {
						var zonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, worldPos.x, worldPos.y, worldPos.z);
						float oldWidth = Stage.GetStageWidth(stage);
						float motionWidth = Mathf.Max(Stage.GetStageWidth_Motion(stage), 0.001f);
						float localPosX = Stage.ZoneToLocal(
							zonePos.x, zonePos.y, zonePos.z,
							Stage.GetStagePosition(stage, index),
							oldWidth,
							Stage.GetStageHeight(stage),
							Stage.GetStagePivotY(stage),
							Stage.GetStageWorldRotationZ(stage)
						).x;
						float newWidth = 2f * Mathf.Abs(localPosX - 0.5f) * oldWidth / motionWidth;
						if (m_Grid.RendererEnable) {
							newWidth = Util.Snap(newWidth, 20f);
						}
						newWidth = Mathf.Max(newWidth, 0.001f);
						map.SetStageWidth(index, newWidth);
						LogAxisMSG(axis, newWidth);
					}
				}
				break;
				case 1: {// Track
					var track = index < map.Tracks.Count ? map.Tracks[index] : null;
					if (track == null || track.StageIndex < 0 || track.StageIndex >= map.Stages.Count) { break; }
					var stage = map.Stages[track.StageIndex];
					var sPos = Stage.GetStagePosition(stage, track.StageIndex);
					var sWidth = Stage.GetStageWidth(stage);
					var sHeight = Stage.GetStageHeight(stage);
					var sPivotY = Stage.GetStagePivotY(stage);
					var sRotZ = Stage.GetStageWorldRotationZ(stage);
					var basicZonePos = Stage.LocalToZone(
						track.X, 0f, 0f,
						sPos, sWidth, sHeight, sPivotY, sRotZ
					);
					var worldPos = pos - DragOffsetWorld;
					if (axis == 0 || axis == 2) {
						var snappedPos = m_Grid.SnapWorld(worldPos - axisWorldPos + Util.Vector3Lerp3(zoneMin, zoneMax, basicZonePos.x, basicZonePos.y, basicZonePos.z), true, true);
						var snappedZonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, snappedPos.x, snappedPos.y, snappedPos.z);
						float newX = Stage.ZoneToLocal(
							snappedZonePos.x, snappedZonePos.y, snappedZonePos.z,
							sPos, sWidth, sHeight, sPivotY, sRotZ
						).x;
						map.SetX(1, index, newX);
						LogAxisMSG(axis, newX);
					}
					if (axis == 3) {
						var zonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, worldPos.x, worldPos.y, worldPos.z);
						float oldWidth = Track.GetTrackWidth(track);
						float motionWidth = Mathf.Max(Track.GetTrackWidth_Motion(track), 0.0001f);
						float localPosX = Track.ZoneToLocal(
							zonePos.x, zonePos.y, zonePos.z,
							sPos, sWidth, sHeight, sPivotY, sRotZ,
							Track.GetTrackX(track),
							oldWidth,
							Track.GetTrackAngle(track)
						).x;
						float newWidth = 2f * Mathf.Abs(localPosX - 0.5f) * oldWidth / motionWidth;
						if (m_Grid.RendererEnable) {
							newWidth = Util.Snap(newWidth, 20f);
						}
						newWidth = Mathf.Max(newWidth, 0.001f);
						map.SetTrackWidth(index, newWidth);
						LogAxisMSG(axis, newWidth);
					}
				}
				break;
				case 2: { // Note
					var note = index < map.Notes.Count ? map.Notes[index] : null;
					if (note == null || note.TrackIndex < 0 || note.TrackIndex >= map.Tracks.Count) { break; }
					var track = map.Tracks[note.TrackIndex];
					if (track == null || track.StageIndex < 0 || track.StageIndex >= map.Stages.Count) { break; }
					var stage = map.Stages[track.StageIndex];
					var sPos = Stage.GetStagePosition(stage, track.StageIndex);
					var sWidth = Stage.GetStageWidth(stage);
					var sHeight = Stage.GetStageHeight(stage);
					var sPivotY = Stage.GetStagePivotY(stage);
					var sRotZ = Stage.GetStageWorldRotationZ(stage);
					var tX = Track.GetTrackX(track);
					var tWidth = Track.GetTrackWidth(track);
					var tAngle = Track.GetTrackAngle(track);
					var worldPos = pos - DragOffsetWorld;
					var snappedPos = m_Grid.SnapWorld(worldPos, true);
					var snappedZonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, snappedPos.x, snappedPos.y, snappedPos.z);
					var localPos = Track.ZoneToLocal(
						snappedZonePos.x, snappedZonePos.y, snappedZonePos.z,
						sPos, sWidth, sHeight, sPivotY, sRotZ,
						tX, tWidth, tAngle
					);
					if (axis == 0 || axis == 2) {
						map.SetX(2, index, localPos.x);
						LogAxisMSG(axis, localPos.x);
					}
					if (axis == 1 || axis == 2) {
						float newTime = GetFilledTime(
							m_Grid.MusicTime,
							localPos.y,
							m_Grid.SpeedMuti * m_Grid.ObjectSpeedMuti
						);
						map.SetTime(2, index, newTime);
						LogAxisMSG(axis, localPos.x, newTime);
					}
					if (axis == 4) {
						float newDuration = GetFilledTime(
							m_Grid.MusicTime,
							localPos.y,
							m_Grid.SpeedMuti * m_Grid.ObjectSpeedMuti
						) - note.Time;
						map.SetDuration(2, index, Mathf.Max(newDuration, 0f));
						LogAxisMSG(axis, newDuration);
					}
					if (axis == 3) {
						var zonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, worldPos.x, worldPos.y, worldPos.z);
						float localPosX = Track.ZoneToLocal(
							zonePos.x, zonePos.y, zonePos.z,
							sPos, sWidth, sHeight, sPivotY, sRotZ,
							tX, tWidth, tAngle
						).x;
						float newWidth = Mathf.Abs(note.X - localPosX) * 2f;
						if (m_Grid.RendererEnable) {
							newWidth = Util.Snap(newWidth, 20f);
						}
						newWidth = Mathf.Max(newWidth, 0.001f);
						map.SetNoteWidth(index, newWidth);
						LogAxisMSG(axis, newWidth);
					}
				}
				break;
				case 3: { // Timing
					if (axis == 1 || axis == 2) {
						var snappedPos = m_Grid.SnapWorld(pos - DragOffsetWorld, true);
						var zonePos_real = Util.Vector3InverseLerp3(zoneMin, zoneMax_real, snappedPos.x, snappedPos.y, snappedPos.z);
						float newTime = Mathf.Max(m_Grid.MusicTime + zonePos_real.y / m_Grid.SpeedMuti, 0f);
						map.SetTime(3, index, newTime);
						LogAxisMSG(axis, newTime);
					}
				}
				break;
				case 4: // Stage Timer
				case 5: // Track Timer
					if (axis == 1 || axis == 2) {
						if (SelectingItemType == 5 && (index < 0 || index >= map.Tracks.Count)) { break; }
						int stageIndex = SelectingItemType == 4 ? index : map.GetParentIndex(1, index);
						var stage = stageIndex >= 0 && stageIndex < map.Stages.Count ? map.Stages[stageIndex] : null;
						if (stage == null) { break; }
						var snappedPos = m_Grid.SnapWorld(pos - DragOffsetWorld, true);
						var snappedZonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, snappedPos.x, snappedPos.y, snappedPos.z);
						var localPosY = SelectingItemType == 4 ? StageTimer.ZoneToLocalY(
							snappedZonePos.x, snappedZonePos.y, snappedZonePos.z,
							Stage.GetStagePosition(stage, index),
							Stage.GetStageHeight(stage),
							Stage.GetStagePivotY(stage),
							Stage.GetStageWorldRotationZ(stage)
						) : TrackTimer.ZoneToLocalY(
							snappedZonePos.x, snappedZonePos.y, snappedZonePos.z,
							Stage.GetStagePosition(stage, index),
							Stage.GetStageHeight(stage),
							Stage.GetStagePivotY(stage),
							Stage.GetStageWorldRotationZ(stage),
							Track.GetTrackAngle(map.Tracks[index])
						);
						if (SelectingItemSubIndex == 2) {
							// Head
							float newTime = Mathf.Max(Util.Remap(0f, 1f, m_Grid.MusicTime, m_Grid.MusicTime + 1f, localPosY), 0f);
							map.SetTime(SelectingItemType - 4, index, newTime);
							LogAxisMSG(axis, newTime);
						} else if (SelectingItemSubIndex == 3) {
							// Tail
							float startTime = map.GetTime(SelectingItemType - 4, index);
							float newTime = Mathf.Max(Util.Remap(0f, 1f, m_Grid.MusicTime - startTime, m_Grid.MusicTime - startTime + 1f, localPosY), 0f);
							map.SetDuration(SelectingItemType - 4, index, newTime);
							LogAxisMSG(axis, newTime);
						}
					}
					break;
			}
			OnObjectEdited();
		}


		#endregion




		#region --- API ---


		public void SwitchUseGlobalBrushScale () {
			UseGlobalBrushScale.Value = !UseGlobalBrushScale;
			try {
				LogHint(
					string.Format(GetLanguage(HINT_GlobalBrushScale), UseGlobalBrushScale.Value ? "ON" : "OFF"),
					true
				);
			} catch { }
		}


		public void MoveStageItemUp (object target) {
			if (target is RectTransform) {
				int index = (target as RectTransform).GetSiblingIndex();
				MoveItem(0, index, index - 1);
			}
		}
		public void MoveStageItemDown (object target) {
			if (target is RectTransform) {
				int index = (target as RectTransform).GetSiblingIndex();
				MoveItem(0, index, index + 1);
			}
		}
		public void MoveTrackItemUp (object target) {
			if (target is RectTransform) {
				int index = (target as RectTransform).GetSiblingIndex();
				MoveItem(1, index, index - 1);
			}
		}
		public void MoveTrackItemDown (object target) {
			if (target is RectTransform) {
				int index = (target as RectTransform).GetSiblingIndex();
				MoveItem(1, index, index + 1);
			}
		}


		// Selection
		public void SetSelection (int type, int index, int subIndex = 0) {
			// No Stage Selection in Abreast View
			if (GetUseAbreast() && (type == 0 || type == 4)) {
				// Switch Abreast Index
				// SetAbreastIndex(index);
				type = -1;
				index = -1;
				subIndex = -1;
			}
			// No Selecting When Locked or unEyed
			if (GetItemLock(type) || !GetContainerActive(type)) {
				type = -1;
				index = -1;
				subIndex = -1;
			}
			// Select
			bool changed = SelectingItemType != type || SelectingItemIndex != index || SelectingItemSubIndex != subIndex;
			SelectingItemType = type;
			SelectingItemIndex = index;
			SelectingItemSubIndex = subIndex;
			if (changed) {
				OnSelectionChanged();
			}
		}


		public void ClearSelection () {
			if (SelectingItemIndex >= 0 || SelectingItemType >= 0) {
				// Clear
				SelectingItemIndex = -1;
				SelectingItemSubIndex = -1;
				SelectingItemType = -1;
				// Final
				OnSelectionChanged();
			}
		}


		// Container
		public void UI_SwitchContainerActive (int index) => SetContainerActive(index, !GetContainerActive(index));


		public bool GetContainerActive (int index) => index >= 0 && index < m_Containers.Length && m_Containers[index].gameObject.activeSelf;


		public void SetContainerActive (int index, bool active) {
			if (index < 0 || index >= m_Containers.Length) { return; }
			m_Containers[index].gameObject.SetActive(active);
			// UI
			UIReady = false;
			try {
				m_EyeTGs[index].isOn = !active;
			} catch { }
			UIReady = true;
			// MSG
			OnLockEyeChanged();
		}


		// Item Lock
		public bool GetItemLock (int item) => item >= 0 && item < ItemLock.Length && ItemLock[item];


		public void UI_SwitchLock (int index) => SetLock(index, !GetItemLock(index));


		// Brush
		public void SetBrush (int index) {
			if (UIReady) {
				SetBrushLogic(index);
			}
		}


		public void SetErase () => SetEraseLogic();


		#endregion



		#region --- LGC ---


		private bool AntiTargetAllow () {
			foreach (var t in m_AntiTargets) {
				if (t.gameObject.activeSelf) {
					return false;
				}
			}
			return true;
		}


		private Ray GetMouseRay () => Camera.ScreenPointToRay(Input.mousePosition);


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


		private (int type, int index, int subIndex, Transform target) GetCastTypeIndex (Ray ray, LayerMask mask, bool insideZone) {
			int count = Physics.RaycastNonAlloc(ray, CastHits, float.MaxValue, mask);
			float overlapType = -1f;
			int overlapIndex = -1;
			int overlapSubIndex = -1;
			Transform tf = null;
			if (!insideZone || RayInsideZone(ray)) {
				for (int i = 0; i < count; i++) {
					var hit = CastHits[i];
					int layer = hit.transform.gameObject.layer;
					float typeF = LayerToTypeMap.ContainsKey(layer) ? LayerToTypeMap[layer] : -1;
					int itemIndex = hit.transform.parent.GetSiblingIndex();
					int itemSubIndex = hit.transform.GetSiblingIndex();
					if (typeF >= overlapType) {
						if (typeF != overlapType) {
							overlapIndex = -1;
							overlapSubIndex = -1;
						}
						if (itemIndex >= overlapIndex) {
							tf = CastHits[i].transform.parent;
						}
						overlapIndex = Mathf.Max(itemIndex, overlapIndex);
						overlapSubIndex = (int)typeF == 4 || (int)typeF == 5 ? Mathf.Max(itemSubIndex, overlapSubIndex) : 0;
						overlapType = typeF;
					}
				}
			}
			return ((int)overlapType, overlapIndex, overlapSubIndex, tf);
		}


		private bool RayInsideZone (Ray ray, bool checkX = true, bool checkY = true) {
			var (zoneMin, zoneMax, _, _) = GetRealZoneMinMax();
			if (new Plane(Vector3.back, zoneMin).Raycast(ray, out float enter)) {
				var point = ray.GetPoint(enter);
				return
					(!checkX || (point.x > zoneMin.x && point.x < zoneMax.x)) &&
					(!checkY || (point.y > zoneMin.y && point.y < zoneMax.y));
			}
			return false;
		}


		private void RefreshUnlockedMask () {
			var list = new List<string>();
			for (int i = 0; i < ItemLock.Length; i++) {
				if (!ItemLock[i]) {
					list.Add(ITEM_LAYER_NAMES[i]);
					if (i == 0) {
						list.Add(ITEM_LAYER_NAMES[4]);
					} else if (i == 1) {
						list.Add(ITEM_LAYER_NAMES[5]);
					}
				}
			}
			// Hold Note Layer
			if (!ItemLock[2]) {
				list.Add(HOLD_NOTE_LAYER_NAME);
			}
			UnlockedMask = LayerMask.GetMask(list.ToArray());
		}


		private void TrySetGridInactive () {
			if (m_Grid.GridEnabled) {
				m_Grid.SetGridTransform(false, false);
			}
		}


		private void SetBrushLogic (int brushIndex) {
			if (!enabled || !GetEditorActive()) { brushIndex = -1; }
			UIReady = false;
			try {
				// No Stage in Abreast
				if (brushIndex == 0 && GetUseAbreast()) {
					brushIndex = -1;
					LogHint(GetLanguage(DIALOG_CannotSelectStageBrush), true);
				}
				// Layer Lock
				if (brushIndex >= 0 && GetItemLock(brushIndex)) {
					brushIndex = -1;
					LogHint(GetLanguage(DIALOG_SelectingLayerLocked), true);
				}
				// Layer Invisible
				if (brushIndex >= 0 && !GetContainerActive(brushIndex)) {
					brushIndex = -1;
					LogHint(GetLanguage(DIALOG_SelectingLayerInactive), true);
				}
				m_EraseUI.gameObject.TrySetActive(false);
				// Brushs TG
				for (int i = 0; i < m_DefaultBrushTGs.Length; i++) {
					m_DefaultBrushTGs[i].isOn = i == brushIndex;
				}
				// Logic
				SelectingBrushIndex = brushIndex;
			} catch { }
			UIReady = true;
		}


		private void SetEraseLogic () {
			UIReady = false;
			try {
				if (!enabled || !GetEditorActive()) {
					SelectingBrushIndex = -1;
				} else {
					SelectingBrushIndex = -2;
					m_EraseUI.gameObject.TrySetActive(true);
				}
				ClearSelection();
				// Brushs TG
				for (int i = 0; i < m_DefaultBrushTGs.Length; i++) {
					m_DefaultBrushTGs[i].isOn = false;
				}
			} catch { }
			UIReady = true;
		}


		private void MoveItem (int type, int index, int newIndex) {
			var map = GetBeatmap();
			if (map != null) {
				ClearSelection();
				map.SetItemIndex(type, index, newIndex);
			}
		}


		private void LogAxisMSG (int axis, float value) => LogAxisMessage(axis, value.ToString("0.##"));
		private void LogAxisMSG (int axis, float value0, float value1) {
			switch (axis) {
				case 0:
					LogAxisMessage(axis, value0.ToString("0.##"));
					break;
				case 1:
					LogAxisMessage(axis, value1.ToString("0.##"));
					break;
				case 2:
					LogAxisMessage(axis, value0.ToString("0.##") + ", " + value1.ToString("0.##"));
					break;
				case 3:
					LogAxisMessage(axis, value0.ToString("0.##"));
					break;
			}
		}


		#endregion



	}
}