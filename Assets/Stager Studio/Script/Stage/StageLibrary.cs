namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;
	using global::StagerStudio.Stage;

	public class StageLibrary : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable]
		public struct PrefabDataArray {
			public PrefabData[] Prefabs;
		}


		[System.Serializable]
		public struct PrefabData {

			[System.Serializable]
			public enum DataType {
				Stage = 0,
				Track = 1,
				Note = 2,
			}

			// Ser-API
			public DataType Type;
			public Beatmap.Stage Stage;
			public Beatmap.Track Track;
			public List<Beatmap.Note> Notes;

			// API
			public PrefabData (Beatmap.Stage stage) : this(DataType.Stage, stage, null, null, 0f) { }
			public PrefabData (Beatmap.Track track) : this(DataType.Track, null, track, null, 0f) { }
			public PrefabData (List<Beatmap.Note> notes, float bpm) : this(DataType.Note, null, null, notes, bpm) { }
			public PrefabData (DataType type, Beatmap.Stage stage, Beatmap.Track track, List<Beatmap.Note> notes, float bpm) {
				Type = type;
				Stage = null;
				Track = null;
				Notes = null;
				if (!(stage is null)) {
					try {
						Stage = Util.BytesToObject(Util.ObjectToBytes(stage)) as Beatmap.Stage;
						Stage.Time = 0f;
						Stage.X = 0f;
						Stage.Y = 0f;
					} catch { }
				} else if (!(track is null)) {
					try {
						Track = Util.BytesToObject(Util.ObjectToBytes(track)) as Beatmap.Track;
						Track.Time = 0f;
						Track.X = 0f;
					} catch { }
				} else if (!(notes is null) && notes.Count > 0) {
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
			public void SetThumbnail (RectUI thumb, Color stageColor, Color trackColor, Color noteColor) {
				if (thumb is null) { return; }
				thumb.Clear();
				switch (Type) {
					default:
					case DataType.Stage: {
							float ratio =
								Mathf.Max(Stage.Width, float.Epsilon) /
								Mathf.Max(Stage.Height, float.Epsilon);
							thumb.Add(new Rect(
								ratio > 1f ? 0f : (1f - ratio) / 2f,
								0,
								ratio > 1f ? 1f : ratio,
								ratio > 1f ? 1f / ratio : 1f
							));
							thumb.color = stageColor;
							break;
						}
					case DataType.Track: {
							float ratio = Mathf.Max(Track.Width, float.Epsilon);
							thumb.Add(new Rect(
								ratio > 1f ? 0f : (1f - ratio) / 2f,
								0,
								ratio > 1f ? 1f : ratio,
								ratio > 1f ? 1f / ratio : 1f
							));
							thumb.color = trackColor;
							break;
						}
					case DataType.Note: {
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
							thumb.color = noteColor;
							break;
						}
				}
			}

		}


		public delegate void VoidIntHandler (int index);


		#endregion




		#region --- VAR ---


		// Handler
		public static VoidIntHandler OnSelectionChanged { get; set; } = null;

		// API
		public int SelectingItemIndex { get; private set; } = -1;

		// Short
		private string PrefabJsonPath => Util.CombinePaths(Application.streamingAssetsPath, "Library", "Prefabs.json");
		private static StageMenu Menu => _Menu != null ? _Menu : (_Menu = FindObjectOfType<StageMenu>());

		// Ser
		[SerializeField] private Toggle[] m_DefaultBrushTGs = null;
		[SerializeField] private Grabber m_BrushPrefab = null;
		[SerializeField] private RectTransform m_LibraryView = null;
		[SerializeField] private RectTransform m_BrushContainer = null;
		[SerializeField] private Color m_StagePrefabColor = Color.white;
		[SerializeField] private Color m_TrackPrefabColor = Color.white;
		[SerializeField] private Color m_NotePrefabColor = Color.white;

		// Data
		private const string DIALOG_DeletePrefabConfirm = "Dialog.Library.DeletePrefabConfirm";
		private const string DIALOG_ImportPrefabConfirm = "Dialog.Library.ImportPrefabConfirm";
		private const string DIALOG_PrefabExported = "Dialog.Library.PrefabExported";
		private const string DIALOG_ImportPrefabTitle = "Dialog.Library.ImportPrefabTitle";
		private const string DIALOG_ExportPrefabTitle = "Dialog.Library.ExportPrefabTitle";
		private const string PREFAB_MENU_KEY = "Menu.Library.PrefabItem";
		private readonly List<PrefabData> PrefabDatas = new List<PrefabData>();
		private static StageMenu _Menu = null;
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


		private void OnApplicationQuit () {
			SaveToFile(PrefabJsonPath);
		}


		#endregion




		#region --- API ---


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
				ReloadPrefabsUI();
			});
		}


		public void UI_MovePrefabLeft (object itemRT) {
			if (!(itemRT is RectTransform rt)) { return; }
			MovePrefabLogic(rt.GetSiblingIndex(), true);
		}


		public void UI_MovePrefabRight (object itemRT) {
			if (!(itemRT is RectTransform rt)) { return; }
			MovePrefabLogic(rt.GetSiblingIndex(), false);
		}


		#endregion




		#region --- LGC ---


		// Brush Selection
		private void SetSelectionLogic (int index) {
			if (!enabled) { index = -1; }
			UIReady = false;
			try {
				SelectingItemIndex = index;
				int defaultCount = m_DefaultBrushTGs.Length;
				for (int i = 0; i < defaultCount; i++) {
					m_DefaultBrushTGs[i].isOn = i == index;
				}
				int prefabCount = m_BrushContainer.childCount;
				for (int i = 0; i < prefabCount; i++) {
					var tg = m_BrushContainer.GetChild(i).GetComponent<Toggle>();
					if (tg is null) { continue; }
					bool isOn = i + defaultCount == index;
					if (tg.isOn != isOn) {
						tg.isOn = isOn;
					}
				}
				OnSelectionChanged(index);
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
			ReloadPrefabsUI();
		}


		// File
		private void ReloadPrefabsUI () {
			m_BrushContainer.DestroyAllChildImmediately();
			int defaultCount = m_DefaultBrushTGs.Length;
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
					SetSelectionLogic(isOn ? index + defaultCount : -1);
				});
				// Menu
				grab.Grab<TriggerUI>().CallbackRight.AddListener(() => Menu.OpenMenu(PREFAB_MENU_KEY, rt));
				// Thumbnail
				prefabData.SetThumbnail(grab.Grab<RectUI>("Thumbnail"), m_StagePrefabColor, m_TrackPrefabColor, m_NotePrefabColor);
			}
			SetSelectionLogic(SelectingItemIndex);
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


		#endregion




	}
}