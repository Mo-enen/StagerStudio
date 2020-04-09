namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	using Data;



	public class StageLibrary : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable]
		public struct PrefabDataArray {
			public PrefabData[] Prefabs;
		}


		[System.Serializable]
		public struct PrefabData {


			public List<Beatmap.Note> Notes;

			public void SetThumbnail (RectUI thumb) {
				if (thumb is null) { return; }
				thumb.Clear();

				float startTime = float.MaxValue;
				float endTime = float.MinValue;
				float startX = float.MaxValue;
				float endX = float.MinValue;
				foreach (var noteData in Notes) {
					startTime = Mathf.Min(startTime, noteData.Time);
					endTime = Mathf.Max(endTime, noteData.Time + noteData.Duration);
					startX = Mathf.Min(startX, noteData.X - noteData.Width / 2f);
					endX = Mathf.Max(endX, noteData.X + noteData.Width / 2f);
				}
				endX = Mathf.Max(endX, startX + 0.001f);
				endTime = Mathf.Max(endTime, startTime + 0.001f);
				foreach (var noteData in Notes) {
					thumb.Add(Rect.MinMaxRect(
						Util.RemapUnclamped(startX, endX, 0f, 1f,
							noteData.X - noteData.Width / 2f
						),
						Util.RemapUnclamped(startTime, endTime, 0f, 1f,
							noteData.Time
						),
						Util.RemapUnclamped(startX, endX, 0f, 1f,
							noteData.X + noteData.Width / 2f
						),
						Util.RemapUnclamped(startTime, endTime, 0f, 1f,
							noteData.Time + noteData.Duration
						)
					));
				}
			}

			public PrefabData (List<Beatmap.Note> notes, float bpm) {
				Notes = null;
				if (!(notes is null) && notes.Count > 0) {
					Notes = new List<Beatmap.Note>();
					float firstTime = 0f;
					bpm = Mathf.Max(bpm, float.Epsilon);
					foreach (var n in notes) {
						if (n is null) { continue; }
						firstTime = Mathf.Min(firstTime, n.Time);
					}
					foreach (var n in notes) {
						try {
							var newNote = Util.BytesToObject(Util.ObjectToBytes(n)) as Beatmap.Note;
							if (!(newNote is null)) {
								newNote.Time -= firstTime;
								newNote.Time /= 60f / bpm;
								Notes.Add(newNote);
							}
						} catch { }
					}
				}
			}

		}


		public delegate int IntHandler ();
		public delegate void VoidHandler ();
		public delegate float FloatHandler ();
		public delegate void VoidStringRTHandler (string key, RectTransform rt);
		public delegate string StringStringHandler (string key);
		public delegate void VoidStringBoolHandler (string str, bool b);
		public delegate (int type, List<object> list) SelectionHandler ();


		#endregion




		#region --- VAR ---


		// Const
		private const int DEFAULT_BRUSH_COUNT = 5;

		// Handler
		public static VoidHandler OnSelectionChanged { get; set; } = null;
		public static SelectionHandler GetSelectingObjects { get; set; } = null;
		public static IntHandler GetSelectionCount { get; set; } = null;
		public static FloatHandler GetBPM { get; set; } = null;
		public static VoidStringRTHandler OpenMenu { get; set; } = null;
		public static FloatHandler GetAbreastValue { get; set; } = null;
		public static VoidStringBoolHandler LogHint { get; set; } = null;
		public static StringStringHandler GetLanguage { get; set; } = null;

		// API
		public (int type, int index) SelectingItemTypeIndex { get; private set; } = (-1, -1);
		public Beatmap.Stage GetDefaultStage => m_DefaultStage;
		public Beatmap.Track GetDefaultTrack => m_DefaultTrack;
		public List<Beatmap.Note> GetDefaultNotes => m_DefaultNotes;

		// Short
		private string PrefabJsonPath => Util.CombinePaths(Application.streamingAssetsPath, "Library", "Prefabs.json");

		// Ser
		[SerializeField] private Toggle[] m_DefaultBrushTGs = null;
		[SerializeField] private Beatmap.Stage m_DefaultStage = null;
		[SerializeField] private Beatmap.Track m_DefaultTrack = null;
		[SerializeField] private List<Beatmap.Note> m_DefaultNotes = null;
		[SerializeField] private Grabber m_BrushPrefab = null;
		[SerializeField] private EventTrigger m_AddPrefab = null;
		[SerializeField] private RectTransform m_LibraryView = null;
		[SerializeField] private RectTransform m_BrushContainer = null;

		// Data
		private const string DIALOG_DeletePrefabConfirm = "Dialog.Library.DeletePrefabConfirm";
		private const string DIALOG_ImportPrefabConfirm = "Dialog.Library.ImportPrefabConfirm";
		private const string DIALOG_PrefabExported = "Dialog.Library.PrefabExported";
		private const string DIALOG_CannotSelectStage = "Dialog.Library.CannotSelectStage";
		private const string DIALOG_ImportPrefabTitle = "Dialog.Library.ImportPrefabTitle";
		private const string DIALOG_ExportPrefabTitle = "Dialog.Library.ExportPrefabTitle";
		private const string PREFAB_MENU_KEY = "Menu.Library.PrefabItem";
		private readonly List<PrefabData> PrefabDatas = new List<PrefabData>();
		private EventTrigger AddFromSelectionButton = null;
		private bool UIReady = true;


		#endregion




		#region --- MSG ---


		private void Awake () {
			// Default Brush
			for (int i = 0; i < m_DefaultBrushTGs.Length; i++) {
				int index = i;
				m_DefaultBrushTGs[index].onValueChanged.AddListener((isOn) => {
					if (!UIReady) { return; }
					SetSelectionLogic(isOn ? index : -1);
				});
			}
			// Final
			LoadFromFile(PrefabJsonPath);
		}


		private void Start () {
			ReloadPrefabsUI();
		}


		private void OnEnable () {
			SetSelectionLogic(-1);
			m_LibraryView.gameObject.SetActive(true);
		}


		private void OnDisable () {
			SetSelectionLogic(-1);
			if (m_LibraryView != null) {
				m_LibraryView.gameObject.SetActive(false);
			}
		}


		#endregion




		#region --- API ---


		public List<Beatmap.Note> GetNotesAt (int index) {
			// Default
			if (index < DEFAULT_BRUSH_COUNT) {
				return index == 2 ? m_DefaultNotes : null;
			}
			// Custom
			index -= DEFAULT_BRUSH_COUNT;
			if (index < 0 || index >= PrefabDatas.Count) { return null; }
			return PrefabDatas[index].Notes;
		}


		public void UI_SetSelection (int index) {
			if (UIReady) {
				SetSelectionLogic(index);
			}
		}


		public void UI_ImportPrefabs () {
			DialogUtil.Dialog_OK_Cancel(DIALOG_ImportPrefabConfirm, DialogUtil.MarkType.Warning, () => {
				var path = DialogUtil.PickFileDialog(DIALOG_ImportPrefabTitle, "json", "json");
				if (!string.IsNullOrEmpty(path)) {
					LoadFromFile(path);
					SaveToFile(PrefabJsonPath);
					ReloadPrefabsUI();
				}
			});
		}


		public void UI_ExportPrefabs () {
			var path = DialogUtil.CreateFileDialog(DIALOG_ExportPrefabTitle, "Prefabs", "json");
			if (!string.IsNullOrEmpty(path)) {
				SaveToFile(path);
				DialogUtil.Dialog_OK(DIALOG_PrefabExported, DialogUtil.MarkType.Success);
			}
		}


		public void UI_DeletePrefabItem (object itemRT) {
			if (!(itemRT is RectTransform rt)) { return; }
			int index = rt.GetSiblingIndex();
			if (index < 0 || index >= PrefabDatas.Count) { return; }
			DialogUtil.Dialog_Delete_Cancel(DIALOG_DeletePrefabConfirm, () => {
				PrefabDatas.RemoveAt(index);
				SaveToFile(PrefabJsonPath);
				ReloadPrefabsUI();
			});
		}


		public void UI_MovePrefabLeft (object itemRT) {
			if (!(itemRT is RectTransform rt)) { return; }
			MovePrefabLogic(rt.GetSiblingIndex(), true);
			SaveToFile(PrefabJsonPath);
			ReloadPrefabsUI();
		}


		public void UI_MovePrefabRight (object itemRT) {
			if (!(itemRT is RectTransform rt)) { return; }
			MovePrefabLogic(rt.GetSiblingIndex(), false);
			SaveToFile(PrefabJsonPath);
			ReloadPrefabsUI();
		}


		public void RefreshAddButton (bool active) {
			if (AddFromSelectionButton != null) {
				AddFromSelectionButton.gameObject.SetActive(active);
			}
		}


		#endregion




		#region --- LGC ---


		// Brush Selection
		private void SetSelectionLogic (int index) {
			if (!enabled) { index = -1; }
			UIReady = false;
			try {
				// Logic
				if (index >= 0) {
					int type = -1;
					int pIndex = index - DEFAULT_BRUSH_COUNT;
					if (pIndex >= 0 && pIndex < PrefabDatas.Count) {
						// Custom
						type = 2;
					} else if (index >= 0 && index < DEFAULT_BRUSH_COUNT) {
						// Default
						type = index;
					}
					if (type == 0 && GetAbreastValue() > 0.5f) {
						index = -1;
						type = -1;
						LogHint(GetLanguage(DIALOG_CannotSelectStage), true);
					}
					SelectingItemTypeIndex = (type, index);
				} else {
					SelectingItemTypeIndex = (-1, index);
				}
			} catch { }
			try {
				// Default Brush TG
				for (int i = 0; i < DEFAULT_BRUSH_COUNT; i++) {
					m_DefaultBrushTGs[i].isOn = i == index;
				}
			} catch { }
			try {
				// Brush TG
				int prefabCount = m_BrushContainer.childCount;
				for (int i = 0; i < prefabCount; i++) {
					var tg = m_BrushContainer.GetChild(i).GetComponent<Toggle>();
					if (tg is null) { continue; }
					bool isOn = i + DEFAULT_BRUSH_COUNT == index;
					if (tg.isOn != isOn) {
						tg.isOn = isOn;
					}
				}
				OnSelectionChanged();
			} catch { }
			UIReady = true;
		}


		// Prefab
		private void MovePrefabLogic (int index, bool left) {
			int newIndex = index + (left ? -1 : 1);
			if (index < 0 || index >= PrefabDatas.Count ||
				newIndex < 0 || newIndex >= PrefabDatas.Count
			) { return; }
			var temp = PrefabDatas[index];
			PrefabDatas[index] = PrefabDatas[newIndex];
			PrefabDatas[newIndex] = temp;
		}


		// File
		private void ReloadPrefabsUI () {
			m_BrushContainer.DestroyAllChildImmediately();
			AddFromSelectionButton = null;
			// Brushs
			for (int i = 0; i < PrefabDatas.Count; i++) {
				var prefabData = PrefabDatas[i];
				var grab = Instantiate(m_BrushPrefab, m_BrushContainer);
				// RT
				var rt = grab.transform as RectTransform;
				rt.SetAsLastSibling();
				rt.anchoredPosition3D = rt.anchoredPosition;
				// TG
				int index = i;
				var tg = grab.Grab<Toggle>();
				tg.isOn = false;
				tg.onValueChanged.AddListener((isOn) => {
					if (!UIReady) { return; }
					SetSelectionLogic(isOn ? index + DEFAULT_BRUSH_COUNT : -1);
				});
				// Menu
				grab.Grab<TriggerUI>().CallbackRight.AddListener(() => OpenMenu(PREFAB_MENU_KEY, rt));
				// Thumbnail
				prefabData.SetThumbnail(grab.Grab<RectUI>("Thumbnail"));
			}
			SetSelectionLogic(SelectingItemTypeIndex.index);
			// Add Button
			{
				var addTri = AddFromSelectionButton = Instantiate(m_AddPrefab, m_BrushContainer);
				// RT
				var rt = addTri.transform as RectTransform;
				rt.SetAsLastSibling();
				rt.anchoredPosition3D = rt.anchoredPosition;
				rt.gameObject.SetActive(GetSelectionCount() > 0);
				// Btn
				addTri.triggers[0].callback.AddListener((e) => {
					if ((e as PointerEventData).button != PointerEventData.InputButton.Left) { return; }
					// Clear Lib Selection
					if (SelectingItemTypeIndex.type < 0) {
						SetSelectionLogic(-1);
					}
					// Add Prefab
					var (type, selectionList) = GetSelectingObjects();
					if (selectionList == null || selectionList.Count == 0) { return; }
					if (type == 2) {
						AddToPrefab(selectionList);
					}
					// Final
					SaveToFile(PrefabJsonPath);
					Invoke("ReloadPrefabsUI", 0.01f);
				});

			}
		}


		private void LoadFromFile (string path) {
			try {
				PrefabDatas.Clear();
				if (Util.FileExists(path)) {
					var data = JsonUtility.FromJson<PrefabDataArray>(Util.FileToText(path));
					if (!(data.Prefabs is null)) {
						PrefabDatas.AddRange(data.Prefabs);
					}
				}
			} catch { }
		}


		private void SaveToFile (string path) {
			try {
				Util.TextToFile(JsonUtility.ToJson(new PrefabDataArray() {
					Prefabs = PrefabDatas.ToArray(),
				}, false), path);
			} catch { }
		}


		private void AddToPrefab (List<object> notes) {
			if (notes == null) { return; }
			var noteList = new List<Beatmap.Note>();
			foreach (Beatmap.Note note in notes) {
				if (note == null) { continue; }
				noteList.Add(note);
			}
			PrefabDatas.Add(new PrefabData(noteList, GetBPM()));
		}


		#endregion




	}
}