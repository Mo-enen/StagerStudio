namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	using Data;



	public class SkinEditorPainterUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler {




		#region --- VAR ---


		// API
		public int SelectingRectIndex { get; private set; } = -1;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);
		private Color32[] Pixels32 {
			get {
				if (_Pixels32 is null) {
					var data = m_Editor.Data;
					if (!(data is null) && data.Texture) {
						_Pixels32 = data.Texture.GetPixels32();
					}
				}
				return _Pixels32;
			}
		}

		// Ser
		[SerializeField] private SkinEditorUI m_Editor = null;
		[SerializeField] private Image m_Background = null;
		[SerializeField] private RectTransform m_Root = null;
		[SerializeField] private RectTransform m_RectContainer = null;
		[SerializeField] private RectTransform m_Selection = null;
		[SerializeField] private RectTransform m_DragRect = null;
		[SerializeField] private RectTransform m_3DRectD = null;
		[SerializeField] private RectTransform m_3DRectL = null;
		[SerializeField] private Toggle m_3dTG = null;
		[SerializeField] private Grabber m_RectPrefab = null;
		[SerializeField] private RectTransform[] m_SelectionBorders = null; // UDLR
		[Header("Sub"), SerializeField] private RectTransform m_SubPanelRoot = null;
		[SerializeField] private InputField m_SubX = null;
		[SerializeField] private InputField m_SubY = null;
		[SerializeField] private InputField m_SubW = null;
		[SerializeField] private InputField m_SubH = null;
		[SerializeField] private InputField m_SubL = null;
		[SerializeField] private InputField m_SubR = null;
		[SerializeField] private InputField m_SubB = null;
		[SerializeField] private InputField m_SubT = null;
		[SerializeField] private InputField m_Sub3D = null;

		// Data
		private const string DIALOG_DeleteConfirm = "SkinEditor.Dialog.DeleteConfirm";
		private bool ItemDirty = true;
		private bool UIReady = true;
		private float Ratio = 1f;
		private Camera _Camera = null;
		private Vector4 ContainerZeroPosSize = default;
		private Vector2? DragOffset_View = null;
		private Vector2 DragOffset_SelectionMove = default;
		private Rect? DraggedRect = null;
		private Color32[] _Pixels32 = null;


		#endregion




		#region --- MSG ---


		// Update
		private void Start () {
			var pos = m_Root.anchoredPosition;
			var size = m_Root.rect.size;
			ContainerZeroPosSize = new Vector4(pos.x, pos.y, size.x, size.y);
			SetSelection(-1);
			m_3dTG.onValueChanged.AddListener((isOn) => {
				if (UIReady) {
					var data = m_Editor.Data;
					var ani = m_Editor.GetEditingAniData();
					if (ani is null || data is null || data.Texture is null) { return; }
					ani.Is3D = isOn;
					Refresh3DUI();
				}
			});
		}


		private void Update () {
			ItemUpdate();
			MouseUpdate();
		}


		private void ItemUpdate () {
			// Item Dirty
			if (ItemDirty) {
				m_RectContainer.DestroyAllChildImmediately();
				var data = m_Editor.Data;
				var texture = data.Texture;
				var ani = m_Editor.GetEditingAniData();
				if (!(ani is null) && !(texture is null) && texture.width > 0 && texture.height > 0) {
					int textureWidth = texture.width;
					int textureHeight = texture.height;
					foreach (var rect in ani.Rects) {
						var grab = Instantiate(m_RectPrefab, m_RectContainer);
						// RT
						var rt = grab.transform as RectTransform;
						rt.SetAsLastSibling();
						SetPositionRect(rt, rect, textureWidth, textureHeight);
						// Index
						int index = rt.GetSiblingIndex();
						grab.Grab<Text>("Index").text = (index + 1).ToString();
						// Trigger
						var trigger = grab.Grab<EventTrigger>();
						trigger.triggers[0].callback.AddListener((bEvent) => {
							var e = bEvent as PointerEventData;
							if (e.button == PointerEventData.InputButton.Left) {
								SetSelection(index);
							}
						});
					}
				}
				ItemDirty = false;
			}
		}


		private void MouseUpdate () {
			// Zoom
			float delta = Input.mouseScrollDelta.y;
			if (Mathf.Abs(delta) > 0.001f && MouseInPainter()) {
				m_Root.SetPivotWithoutChangePosition(m_Root.Get01Position(Input.mousePosition, Camera));
				m_Root.SetSizeWithCurrentAnchors(
					RectTransform.Axis.Vertical,
					Mathf.Clamp(m_Root.rect.height * (delta > 0f ? 1.1f : 0.9f), ContainerZeroPosSize.w, ContainerZeroPosSize.w * 20f)
				);
				m_Root.SetSizeWithCurrentAnchors(
					RectTransform.Axis.Horizontal,
					m_Root.rect.height * Ratio
				);
				Refresh3DUI();
			}
			// Move View
			if (Input.GetMouseButton(2) || Input.GetMouseButton(1)) {
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					m_Root.parent as RectTransform,
					Input.mousePosition,
					Camera,
					out Vector2 point
				);
				if (DragOffset_View.HasValue) {
					m_Root.anchoredPosition = point - DragOffset_View.Value;
				} else {
					DragOffset_View = point - m_Root.anchoredPosition;
				}
			} else if (DragOffset_View.HasValue) {
				DragOffset_View = null;
			}
			// Delete
			if (!Util.IsTypeing && (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))) {
				DeleteSelection();
			}
			// No Mouse Drag
			if (!Input.GetMouseButton(0) && m_DragRect.gameObject.activeSelf) {
				m_DragRect.gameObject.SetActive(false);
			}
		}


		// UI MSG
		public void OnPointerDown (PointerEventData eventData) {
			switch (eventData.button) {
				case PointerEventData.InputButton.Left:
					SetSelection(-1);
					DraggedRect = null;
					break;
			}
		}


		public void OnDrag (PointerEventData eventData) {
			switch (eventData.button) {
				case PointerEventData.InputButton.Left:
					if (!m_DragRect.gameObject.activeSelf) {
						m_DragRect.gameObject.SetActive(true);
					}
					var press01 = m_Root.Get01Position(eventData.pressPosition, Camera);
					var pos01 = m_Root.Get01Position(eventData.position, Camera);
					// Clamp
					press01.x = Mathf.Clamp01(press01.x);
					pos01.x = Mathf.Clamp01(pos01.x);
					press01.y = Mathf.Clamp01(press01.y);
					pos01.y = Mathf.Clamp01(pos01.y);
					// Snap
					var data = m_Editor.Data;
					if (!(data is null) && !(data.Texture is null)) {
						int textureWidth = data.Texture.width;
						int textureHeight = data.Texture.height;
						press01.x = Snap01(press01.x, textureWidth);
						pos01.x = Snap01(pos01.x, textureWidth);
						press01.y = Snap01(press01.y, textureHeight);
						pos01.y = Snap01(pos01.y, textureHeight);
					}
					// UI
					var min = Vector2.Min(press01, pos01);
					var max = Vector2.Max(press01, pos01);
					DraggedRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
					m_DragRect.anchorMin = min;
					m_DragRect.anchorMax = max;
					m_DragRect.offsetMin = m_DragRect.offsetMax = Vector2.zero;
					break;
			}
		}


		public void OnEndDrag (PointerEventData eventData) {
			switch (eventData.button) {
				case PointerEventData.InputButton.Left:
					if (DraggedRect.HasValue) {
						var data = m_Editor.Data;
						var ani = m_Editor.GetEditingAniData();
						if (data is null || ani is null || ani.Rects is null || data.Texture is null) { break; }
						int textureWidth = data.Texture.width;
						int textureHeight = data.Texture.height;
						var rect = DraggedRect.Value;
						var rData = new AnimatedItemData.RectData(
							Mathf.RoundToInt(rect.x * textureWidth),
							Mathf.RoundToInt(rect.y * textureHeight),
							Mathf.RoundToInt(rect.width * textureWidth),
							Mathf.RoundToInt(rect.height * textureHeight)
						);
						if (rData.Width <= 0 || rData.Height <= 0) { break; }
						ani.Rects.Add(rData);
						SetSelection(ani.Rects.Count - 1);
						SetItemsDirty();
						DraggedRect = null;
					}
					break;
			}
		}


		public void Selection_Drag_M (BaseEventData bData) => Selection_Drag(bData as PointerEventData, null, null);
		public void Selection_Down_M (BaseEventData bData) => DragOffset_SelectionMove = m_Root.Get01Position((bData as PointerEventData).pressPosition, Camera) - m_Selection.anchorMin;
		public void Selection_Drag_U (BaseEventData bData) => Selection_Drag(bData as PointerEventData, null, true);
		public void Selection_Drag_D (BaseEventData bData) => Selection_Drag(bData as PointerEventData, null, false);
		public void Selection_Drag_L (BaseEventData bData) => Selection_Drag(bData as PointerEventData, false, null);
		public void Selection_Drag_R (BaseEventData bData) => Selection_Drag(bData as PointerEventData, true, null);
		public void Selection_Drag_UL (BaseEventData bData) => Selection_Drag(bData as PointerEventData, false, true);
		public void Selection_Drag_UR (BaseEventData bData) => Selection_Drag(bData as PointerEventData, true, true);
		public void Selection_Drag_DL (BaseEventData bData) => Selection_Drag(bData as PointerEventData, false, false);
		public void Selection_Drag_DR (BaseEventData bData) => Selection_Drag(bData as PointerEventData, true, false);
		public void Selection_Border_Drag_U (BaseEventData bData) => Selection_Drag(bData as PointerEventData, null, true, true);
		public void Selection_Border_Drag_D (BaseEventData bData) => Selection_Drag(bData as PointerEventData, null, false, true);
		public void Selection_Border_Drag_L (BaseEventData bData) => Selection_Drag(bData as PointerEventData, false, null, true);
		public void Selection_Border_Drag_R (BaseEventData bData) => Selection_Drag(bData as PointerEventData, true, null, true);
		public void Selection_3D_Drag_D (BaseEventData bData) => Selection_3D_Drag(bData as PointerEventData, true);
		public void Selection_3D_Drag_L (BaseEventData bData) => Selection_3D_Drag(bData as PointerEventData, false);

		public void UI_SubPositionX (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(value, -1, -1, -1, -1, -1, -1, -1);
				SetSelection();
			}
		}
		public void UI_SubPositionY (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(-1, value, -1, -1, -1, -1, -1, -1);
				SetSelection();
			}
		}
		public void UI_SubPositionW (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(-1, -1, value, -1, -1, -1, -1, -1);
				SetSelection();
			}
		}
		public void UI_SubPositionH (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(-1, -1, -1, value, -1, -1, -1, -1);
				SetSelection();
			}
		}
		public void UI_SubBorderL (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(-1, -1, -1, -1, value, -1, -1, -1);
				SetSelection();
			}
		}
		public void UI_SubBorderR (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(-1, -1, -1, -1, -1, value, -1, -1);
				SetSelection();
			}
		}
		public void UI_SubBorderB (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(-1, -1, -1, -1, -1, -1, value, -1);
				SetSelection();
			}
		}
		public void UI_SubBorderT (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				SetSelectingRect(-1, -1, -1, -1, -1, -1, -1, value);
				SetSelection();
			}
		}
		public void UI_Sub3D (string str) {
			if (!UIReady || SelectingRectIndex < 0) { return; }
			if (int.TryParse(str, out int value)) {
				var ani = m_Editor.GetEditingAniData();
				if (ani != null) {
					ani.Thickness3D_UI = value;
				}
				Refresh3DUI();
			}
		}

		// MSG Logic
		private void Selection_Drag (PointerEventData e, bool? right, bool? up, bool border = false) {
			if (e.button == PointerEventData.InputButton.Left) {
				if (border) {
					Selection_Drag_Border(e, right, up);
				} else {
					Selection_Drag_Rect(e, right, up);
				}
			}
		}
		private void Selection_Drag_Rect (PointerEventData e, bool? right, bool? up) {
			var (rData, ani, data) = GetSelectingRect();
			if (ani is null) { return; }
			SelectingRectIndex = Mathf.Clamp(SelectingRectIndex, 0, ani.Rects.Count - 1);
			int tWidth = data.Texture.width;
			int tHeight = data.Texture.height;
			// Get Drag Pos
			Vector2 aimPos01 = m_Root.Get01Position(e.position, Camera);
			// Move
			if (right is null && up is null) {
				// M
				aimPos01 -= DragOffset_SelectionMove;
				aimPos01.x = Snap01(aimPos01.x, tWidth);
				aimPos01.y = Snap01(aimPos01.y, tHeight);
				rData.X = Mathf.Clamp(Mathf.RoundToInt(aimPos01.x * tWidth), 0, tWidth - rData.Width);
				rData.Y = Mathf.Clamp(Mathf.RoundToInt(aimPos01.y * tHeight), 0, tHeight - rData.Height);
				ani.Rects[SelectingRectIndex] = rData;
			} else {
				aimPos01.x = Snap01(aimPos01.x, tWidth);
				aimPos01.y = Snap01(aimPos01.y, tHeight);
				// Other
				int minX = rData.X;
				int minY = rData.Y;
				int maxX = rData.X + rData.Width;
				int maxY = rData.Y + rData.Height;
				// Set Data X
				if (right.HasValue) {
					if (right.Value) {
						maxX = Mathf.Clamp(Mathf.RoundToInt(aimPos01.x * tWidth), minX + 1, tWidth);
					} else {
						minX = Mathf.Clamp(Mathf.RoundToInt(aimPos01.x * tWidth), 0, maxX - 1);
					}
				}
				// Set Data Y
				if (up.HasValue) {
					if (up.Value) {
						maxY = Mathf.Clamp(Mathf.RoundToInt(aimPos01.y * tHeight), minY + 1, tHeight);
					} else {
						minY = Mathf.Clamp(Mathf.RoundToInt(aimPos01.y * tHeight), 0, maxY - 1);
					}
				}
				// Set to rData
				rData.X = minX;
				rData.Y = minY;
				rData.Width = maxX - minX;
				rData.Height = maxY - minY;
				rData.BorderU = Mathf.Clamp(rData.BorderU, 0, rData.Height);
				rData.BorderD = Mathf.Clamp(rData.BorderD, 0, rData.Height);
				rData.BorderL = Mathf.Clamp(rData.BorderL, 0, rData.Width);
				rData.BorderR = Mathf.Clamp(rData.BorderR, 0, rData.Width);
				ani.Rects[SelectingRectIndex] = rData;
			}
			// Final
			RefreshRectPosition(SelectingRectIndex, rData, tWidth, tHeight);
			SetSelection();
		}
		private void Selection_Drag_Border (PointerEventData e, bool? right, bool? up) {
			if (right is null && up is null) { return; }
			var (rData, ani, data) = GetSelectingRect();
			if (ani is null) { return; }
			SelectingRectIndex = Mathf.Clamp(SelectingRectIndex, 0, ani.Rects.Count - 1);
			int tWidth = data.Texture.width;
			int tHeight = data.Texture.height;
			// Get Drag Pos
			Vector2 aimPos01 = m_Root.Get01Position(e.position, Camera);
			aimPos01.x = Snap01(aimPos01.x, tWidth);
			aimPos01.y = Snap01(aimPos01.y, tHeight);
			// Set Rect Data
			if (right.HasValue) {
				if (right.Value) {
					rData.BorderR = Mathf.Clamp(
						rData.Width - (Mathf.RoundToInt(aimPos01.x * tWidth) - rData.X),
						0, rData.Width - rData.BorderL
					);
				} else {
					rData.BorderL = Mathf.Clamp(
						Mathf.RoundToInt(aimPos01.x * tWidth) - rData.X,
						0, rData.Width - rData.BorderR
					);
				}
			} else if (up.HasValue) {
				if (up.Value) {
					rData.BorderU = Mathf.Clamp(
						rData.Height - (Mathf.RoundToInt(aimPos01.y * tHeight) - rData.Y),
						0, rData.Height - rData.BorderD
					);
				} else {
					rData.BorderD = Mathf.Clamp(
						Mathf.RoundToInt(aimPos01.y * tHeight) - rData.Y,
						0, rData.Height - rData.BorderU
					);
				}
			}
			// Apply
			ApplyData(ani, rData);
			SetSelection();
		}
		private void Selection_3D_Drag (PointerEventData e, bool down) {
			var (rData, ani, data) = GetSelectingRect();
			if (ani is null || !ani.Is3D) { return; }
			SelectingRectIndex = Mathf.Clamp(SelectingRectIndex, 0, ani.Rects.Count - 1);
			int tWidth = data.Texture.width;
			int tHeight = data.Texture.height;
			// Get Drag Pos
			Vector2 aimPos01 = m_Root.Get01Position(e.position, Camera);
			if (down) {
				aimPos01.y = Snap01(aimPos01.y, tHeight);
				int y = Mathf.Clamp(Mathf.RoundToInt(aimPos01.y * tHeight), 0, tHeight - 1);
				ani.Thickness3D_UI = Mathf.Clamp(rData.Y - y, Mathf.Min(rData.X, rData.Y, 4), Mathf.Min(rData.X, rData.Y));
			} else {
				aimPos01.x = Snap01(aimPos01.x, tWidth);
				int x = Mathf.Clamp(Mathf.RoundToInt(aimPos01.x * tWidth), 0, tWidth - 1);
				ani.Thickness3D_UI = Mathf.Clamp(rData.X - x, Mathf.Min(rData.X, rData.Y, 4), Mathf.Min(rData.X, rData.Y));
			}
			SetSelection();
		}


		#endregion




		#region --- API ---


		public void SetItemsDirty () {
			ItemDirty = true;
		}


		public void SetTexture (Texture2D texture) {
			if (texture is null) {
				m_Background.sprite = null;
				m_Background.color = Color.clear;
			} else {
				m_Background.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
				m_Background.color = Color.white;
				Ratio = (float)texture.width / Mathf.Max(texture.height, 1);
				m_Root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_Root.rect.height * Ratio);
			}
		}


		public void ResetRootPositionSize () {
			m_Root.pivot = Vector2.one * 0.5f;
			m_Root.anchoredPosition = new Vector2(ContainerZeroPosSize.x, ContainerZeroPosSize.y);
			m_Root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ContainerZeroPosSize.w);
			m_Root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ContainerZeroPosSize.w * Ratio);
			Refresh3DUI();
		}


		public void SetSelection () => SetSelection(SelectingRectIndex);


		public void SetSelection (int index) {
			var data = m_Editor.Data;
			var ani = m_Editor.GetEditingAniData();
			if (ani is null || ani.Rects is null || data is null || data.Texture is null) {
				SelectingRectIndex = -1;
				return;
			}
			SelectingRectIndex = Mathf.Clamp(index, -1, ani.Rects.Count - 1);
			m_Selection.gameObject.SetActive(SelectingRectIndex >= 0);
			m_SubPanelRoot.gameObject.SetActive(SelectingRectIndex >= 0);
			if (SelectingRectIndex >= 0) {
				var rData = ani.Rects[SelectingRectIndex];
				// Borders
				float u01 = rData.Height <= 0 ? 0 : Mathf.Clamp01((float)(rData.Height - rData.BorderU) / rData.Height);
				float d01 = rData.Height <= 0 ? 0 : Mathf.Clamp01((float)rData.BorderD / rData.Height);
				float l01 = rData.Width <= 0 ? 0 : Mathf.Clamp01((float)rData.BorderL / rData.Width);
				float r01 = rData.Width <= 0 ? 0 : Mathf.Clamp01((float)(rData.Width - rData.BorderR) / rData.Width);
				m_SelectionBorders[0].anchorMin = new Vector2(0, u01);
				m_SelectionBorders[0].anchorMax = new Vector2(1, u01);
				m_SelectionBorders[1].anchorMin = new Vector2(0, d01);
				m_SelectionBorders[1].anchorMax = new Vector2(1, d01);
				m_SelectionBorders[2].anchorMin = new Vector2(l01, 0);
				m_SelectionBorders[2].anchorMax = new Vector2(l01, 1);
				m_SelectionBorders[3].anchorMin = new Vector2(r01, 0);
				m_SelectionBorders[3].anchorMax = new Vector2(r01, 1);
				// Sub
				UIReady = false;
				try {
					m_SubX.text = rData.X.ToString();
					m_SubY.text = rData.Y.ToString();
					m_SubW.text = rData.Width.ToString();
					m_SubH.text = rData.Height.ToString();
					m_SubL.text = rData.BorderL.ToString();
					m_SubR.text = rData.BorderR.ToString();
					m_SubB.text = rData.BorderD.ToString();
					m_SubT.text = rData.BorderU.ToString();
					m_Sub3D.text = ani.Thickness3D_UI.ToString();
				} catch { }
				UIReady = true;
				// Pos
				SetPositionRect(m_Selection, rData, data.Texture.width, data.Texture.height);
				RefreshRectPosition(SelectingRectIndex, rData, data.Texture.width, data.Texture.height);
				Refresh3DUI();
			}

		}


		public void Refresh3DUI () {
			var data = m_Editor.Data;
			var ani = m_Editor.GetEditingAniData();
			if (ani == null || ani.Rects == null || data == null || data.Texture == null) { return; }
			if (SelectingRectIndex >= 0 && SelectingRectIndex < ani.Rects.Count) {
				var rData = ani.Rects[SelectingRectIndex];
				// 3D Pos
				ani.Is3D = ani.Is3D && rData.Width > 0 && rData.Height > 0;
				m_3DRectD.gameObject.SetActive(ani.Is3D);
				m_3DRectL.gameObject.SetActive(ani.Is3D);
				if (ani.Is3D) {
					float uiSize3D = (float)ani.Thickness3D_UI / data.Texture.height * m_Root.rect.height;
					m_3DRectL.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, uiSize3D);
					m_3DRectD.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, uiSize3D);
				}
			} else {
				m_3DRectD.gameObject.SetActive(false);
				m_3DRectL.gameObject.SetActive(false);
			}
			// 3D Toggle
			UIReady = false;
			try {
				m_3dTG.isOn = ani.Is3D;
				m_Sub3D.text = ani.Thickness3D_UI.ToString();
			} catch { }
			UIReady = true;
		}


		public void DeleteSelection () {
			var ani = m_Editor.GetEditingAniData();
			if (ani is null || ani.Rects is null || SelectingRectIndex < 0 || SelectingRectIndex >= ani.Rects.Count) { }
			int index = SelectingRectIndex;
			if (index < 0) { return; }
			DialogUtil.Dialog_Delete_Cancel(DIALOG_DeleteConfirm, () => {
				ani.Rects.RemoveAt(index);
				SetItemsDirty();
				SetSelection(-1);
			});
		}


		public void TrimSelection () {
			if (Pixels32 is null) { return; }
			var data = m_Editor.Data;
			var ani = m_Editor.GetEditingAniData();
			if (ani is null || ani.Rects is null || SelectingRectIndex < 0 || SelectingRectIndex >= ani.Rects.Count) { }
			int index = SelectingRectIndex;
			if (index < 0) { return; }
			var rectData = ani.Rects[index];
			int textureWidth = data.Texture.width;
			int textureHeight = data.Texture.height;
			int xMin = rectData.X;
			int yMin = rectData.Y;
			int xMax = rectData.X + rectData.Width;
			int yMax = rectData.Y + rectData.Height;
			// U
			for (; yMax > yMin; yMax--) {
				if (!AllClear(xMin, xMax, yMax - 1, yMax)) {
					break;
				}
			}
			// D
			for (; yMin < yMax; yMin++) {
				if (!AllClear(xMin, xMax, yMin, yMin + 1)) {
					break;
				}
			}
			// R
			for (; xMax > xMin; xMax--) {
				if (!AllClear(xMax - 1, xMax, yMin, yMax)) {
					break;
				}
			}
			// L
			for (; xMin < xMax; xMin++) {
				if (!AllClear(xMin, xMin + 1, yMin, yMax)) {
					break;
				}
			}
			// Clamp
			if (xMax <= xMin) {
				xMax = xMin + 1;
			}
			if (yMax <= yMin) {
				yMax = yMin + 1;
			}
			// Final
			rectData.X = xMin;
			rectData.Y = yMin;
			rectData.Width = xMax - xMin;
			rectData.Height = yMax - yMin;
			ani.Rects[index] = rectData;
			SetItemsDirty();
			SetSelection(index);
			// Func
			bool AllClear (int _xMin, int _xMax, int _yMin, int _yMax) {
				for (int x = _xMin; x < _xMax; x++) {
					for (int y = _yMin; y < _yMax; y++) {
						if (!IsClear(x, y)) { return false; }
					}
				}
				return true;
			}
			bool IsClear (int x, int y) => x < 0 || y < 0 || x >= textureWidth || y >= textureHeight || Pixels32[y * textureWidth + x].a == 0;
		}


		#endregion




		#region --- LGC ---


		private bool MouseInPainter () {
			var pos01 = (transform as RectTransform).Get01Position(Input.mousePosition, Camera);
			return pos01.x > 0f && pos01.x < 1f && pos01.y > 0f && pos01.y < 1f;
		}


		private void SetPositionRect (RectTransform rt, AnimatedItemData.RectData rect, int textureWidth, int textureHeight) {
			rt.anchoredPosition3D = rt.anchoredPosition;
			rt.localRotation = Quaternion.identity;
			rt.localScale = Vector3.one;
			rt.anchorMin = new Vector2(
				(float)rect.X / textureWidth,
				(float)rect.Y / textureHeight
			);
			rt.anchorMax = new Vector2(
				(float)(rect.X + Mathf.Max(rect.Width, 1)) / textureWidth,
				(float)(rect.Y + Mathf.Max(rect.Height, 1)) / textureHeight
			);
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;
		}


		private float Snap01 (float value01, float size) => Mathf.Round(value01 * size) / size;


		private void RefreshRectPosition (int index, AnimatedItemData.RectData rData, int textureWidth, int textureHeight) {
			int childCount = m_RectContainer.childCount;
			if (index < 0 || index >= childCount) { return; }
			SetPositionRect(m_RectContainer.GetChild(index) as RectTransform, rData, textureWidth, textureHeight);
		}


		private void SetSelectingRect (int x, int y, int w, int h, int l, int r, int d, int u) {
			var (rData, ani, data) = GetSelectingRect();
			if (ani is null) { return; }
			int tWidth = data.Texture.width;
			int tHeight = data.Texture.height;
			rData.X = x < 0 ? rData.X : Mathf.Clamp(x, 0, tWidth);
			rData.Y = y < 0 ? rData.Y : Mathf.Clamp(y, 0, tHeight);
			rData.Width = w < 0 ? rData.Width : Mathf.Clamp(w, 0, tWidth);
			rData.Height = h < 0 ? rData.Height : Mathf.Clamp(h, 0, tHeight);
			rData.BorderL = l < 0 ? rData.BorderL : Mathf.Clamp(l, 0, rData.Width);
			rData.BorderR = r < 0 ? rData.BorderR : Mathf.Clamp(r, 0, rData.Width);
			rData.BorderD = d < 0 ? rData.BorderD : Mathf.Clamp(d, 0, rData.Height);
			rData.BorderU = u < 0 ? rData.BorderU : Mathf.Clamp(u, 0, rData.Height);
			// Apply
			if (x < 0 && y < 0) {
				ApplyData(ani, rData);
			} else {
				ani.Rects[SelectingRectIndex] = rData;
			}
		}


		private (AnimatedItemData.RectData rData, AnimatedItemData ani, SkinData data) GetSelectingRect () {
			var data = m_Editor.Data;
			var ani = m_Editor.GetEditingAniData();
			if (ani is null || ani.Rects is null || data is null || data.Texture is null || SelectingRectIndex < 0) { return (default, null, null); }
			return (ani.Rects[Mathf.Clamp(SelectingRectIndex, 0, ani.Rects.Count - 1)], ani, data);
		}


		private void ApplyData (AnimatedItemData ani, AnimatedItemData.RectData rData) {
			ani.Rects[SelectingRectIndex] = rData;
		}


		#endregion



	}
}