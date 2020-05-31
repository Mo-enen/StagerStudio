namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	using Saving;
	using UIGadget;


	public class HomeUI : MonoBehaviour {




		#region --- SUB ---



		public delegate string LanguageHandler (string key);
		public delegate void VoidStringHandler (string str);
		public delegate string StringHandler ();
		public delegate void StringRtHandler (string str, RectTransform obj);


		public static class LanguageData {
			public const string Error_ChapterAlreadyExists = "PManager.Error.ChapterAlreadyExists";
			public const string Error_ProjectAlreadyExists = "PManager.Error.ProjectAlreadyExists";
			public const string Error_NewProjectSourceNotExists = "PManager.Error.NewProjectSourceNotExists";
			public const string NewChapterName = "PManager.NewChapterName";
			public const string DefaultChapterName = "PManager.DefaultChapterName";
			public const string UI_ImportProjectTitle = "PManager.UI.ImportProjectTitle";
			public const string UI_OverlapProjectConfirm = "PManager.UI.OverlapProjectConfirm";
			public const string UI_DeleteChapterConfirm = "PManager.UI.DeleteChapterConfirm";
			public const string UI_NewProjectConfirm = "PManager.UI.NewProjectConfirm";
			public const string UI_FolderAlreadyExists = "PManager.UI.FolderAlreadyExists";
			public const string UI_TrashProjectItemConfirm = "PManager.UI.TrashProjectItemConfirm";
			public const string UI_DeleteProjectItemConfirm = "PManager.UI.DeleteProjectItemConfirm";
			public const string UI_TrashbinEmpty = "PManager.UI.TrashbinEmpty";
		}



		public class ProjectItemUIComparer : IComparer<ProjectItemUI> {
			public ProjectSortMode Mode = ProjectSortMode.Time;
			public ProjectItemUIComparer (ProjectSortMode mode) {
				Mode = mode;
			}
			public int Compare (ProjectItemUI x, ProjectItemUI y) {
				if (x.gameObject.activeSelf != y.gameObject.activeSelf) {
					return x.gameObject.activeSelf.CompareTo(y.gameObject.activeSelf);
				}
				int result;
				switch (Mode) {
					default:
					case ProjectSortMode.Time:
						result = -x.LastEditTime.CompareTo(y.LastEditTime);
						return result != 0 ? result : x.ProjectName.CompareTo(y.ProjectName);
					case ProjectSortMode.Name:
						result = x.ProjectName.CompareTo(y.ProjectName);
						return result != 0 ? result : -x.LastEditTime.CompareTo(y.LastEditTime);
				}
			}
		}



		public enum ProjectSortMode {
			Name = 0,
			Time = 1,
		}



		#endregion




		#region --- VAR ---


		// Const
		private const string CHAPTER_ITEM_MENU_KEY = "Menu.ChapterItem";
		private const string PROJECT_ITEM_MENU_KEY = "Menu.ProjectItem";


		// Handler
		public static LanguageHandler GetLanguage { get; set; } = null;
		public static VoidStringHandler GotoEditor { get; set; } = null;
		public static StringHandler GetWorkspace { get; set; } = null;
		public static StringRtHandler OpenMenu { get; set; } = null;
		public static VoidStringHandler SpawnProjectCreator { get; set; } = null;

		// Short
		private string Trashbin => Util.CombinePaths(Application.persistentDataPath, "Stager_Trashbin");
		private ProjectSortMode ProjectSort {
			get => (ProjectSortMode)ProjectSortIndex.Value;
			set => ProjectSortIndex.Value = (int)value;
		}

		// Ser
		[SerializeField] private RectTransform m_ChapterContent = null;
		[SerializeField] private RectTransform m_ProjectContent = null;
		[SerializeField] private RectTransform m_MoveProjectContent = null;
		[SerializeField] private RectTransform m_MoveProjectRoot = null;
		[SerializeField] private RectTransform m_NoProjectHint = null;
		[SerializeField] private Grabber m_ChapterItemPrefab = null;
		[SerializeField] private Grabber m_ChapterItemAltPrefab = null;
		[SerializeField] private Grabber m_ProjectItemPrefab = null;
		[SerializeField] private Color m_ChapterNameNormal = Color.white;
		[SerializeField] private Color m_ChapterNameHighlight = Color.white;

		// Saving
		private SavingString OpeningChapter = new SavingString("HomeUI.OpeningChapter", "");
		private SavingInt ProjectSortIndex = new SavingInt("HomeUI.ProjectSortIndex", 1);


		#endregion




		#region --- MSG ---


		private void Awake () {
			Util.CreateFolder(Trashbin);
		}


		#endregion




		#region --- API ---


		public void Open () => OpenLogic();


		public void Close () => CloseLogic();


		public void CloseMoveProjectWindow () => CloseMoveProjectWindowLogic();


		public void OpenChapter (string name) => OpenChapterLogic(name);


		public void OpenChapterAt (int index) {
			if (m_ChapterContent.childCount == 0) { return; }
			index = Mathf.Clamp(index, 0, m_ChapterContent.childCount - 1);
			OpenChapterLogic(m_ChapterContent.GetChild(index).name);
		}


		public void StartRenameChapter (object itemRT) {
			if (itemRT == null || !(itemRT is RectTransform)) { return; }
			var rt = itemRT as RectTransform;
			var graber = rt.GetComponent<Grabber>();
			var name = graber.Grab<Text>("Name");
			var input = graber.Grab<InputField>("InputField");
			input.gameObject.SetActive(true);
			name.gameObject.SetActive(false);
			EventSystem.current.SetSelectedGameObject(input.gameObject, null);
		}


		public void StartMoveProject (object itemRT) {
			if (itemRT == null || !(itemRT is RectTransform)) { return; }
			var rt = itemRT as RectTransform;
			OpenMoveProjectWindowLogic(rt.name);
		}


		public void ShowChapterInExplorer (object itemRT) {
			if (itemRT == null || !(itemRT is RectTransform)) { return; }
			var rt = itemRT as RectTransform;
			ShowChapterInExplorerLogic(rt.name);
		}


		public void ShowProjectInExplorer (object itemRT) {
			if (itemRT == null || !(itemRT is RectTransform)) { return; }
			var rt = itemRT as RectTransform;
			ShowProjectInExplorerLogic(rt.name);
		}


		public void OpenProjectFromMenu (object itemRT) {
			if (itemRT == null || !(itemRT is RectTransform)) { return; }
			var rt = itemRT as RectTransform;
			GotoEditor(rt.name);
		}


		public void DeleteChapterItem (object itemRT) {
			if (itemRT == null || !(itemRT is RectTransform)) { return; }
			var rt = itemRT as RectTransform;
			DeleteChapterItemLogic(rt.name);
		}


		public void DeleteProjectItem (object itemRT) {
			if (itemRT == null || !(itemRT is RectTransform)) { return; }
			var rt = itemRT as RectTransform;
			string path = rt.name;
			bool inTrashbin = Util.IsChildPath(Trashbin, path);
			if (inTrashbin) {
				DialogUtil.Open(
					string.Format(GetLanguage(LanguageData.UI_DeleteProjectItemConfirm), rt.GetComponent<ProjectItemUI>().ProjectName),
					DialogUtil.MarkType.Warning,
					null, null, () => {
						Util.DeleteFile(path);
						OpenLogic();
						OpenTrashbinLogic(true);
					}, null, () => { }
				);
			} else {
				DialogUtil.Open(
					string.Format(GetLanguage(LanguageData.UI_TrashProjectItemConfirm), rt.GetComponent<ProjectItemUI>().ProjectName),
					DialogUtil.MarkType.Warning,
					 () => {
						 string newPath = Util.CombinePaths(Trashbin, Util.GetNameWithExtension(path));
						 if (Util.FileExists(path)) {
							 Util.MoveFile(path, newPath);
							 OpenLogic();
						 }
					 }, null, null, null, () => { }
				);
			}

		}


		public void ShowWorkspaceInExplorer () {
			if (!Util.DirectoryExists(GetWorkspace())) { return; }
			Util.ShowInExplorer(GetWorkspace());
		}


		public void ShowTrashbinInExplorer () {
			if (!Util.DirectoryExists(Trashbin)) { return; }
			Util.ShowInExplorer(Trashbin);
		}


		public void NewChapter () => NewChapterLogic();


		public void NewProject () {
			NewProjectLogic();
			OpenLogic();
		}


		public void ImportProject () {
			var path = DialogUtil.PickFileDialog(LanguageData.UI_ImportProjectTitle, "", "stager");
			ImportProjectLogic(path);
			OpenLogic();
		}


		public void SetProjectSortMode (int index) {
			ProjectSort = (ProjectSortMode)index;
			SortProjectLogic();
		}


		public void OpenTrashbin () => OpenTrashbinLogic();


		public void Invoke_ExistDialog () => DialogUtil.Dialog_OK(LanguageData.UI_FolderAlreadyExists, DialogUtil.MarkType.Warning);


		#endregion




		#region --- LGC ---


		private void OpenLogic () {
			ClearChapterContent();
			ClearProjectContent();
			CloseMoveProjectWindowLogic();
			m_NoProjectHint.gameObject.SetActive(false);
			// Default Chapter
			var files = Util.GetFilesIn(GetWorkspace(), true, "*.stager");
			if (files != null && files.Length > 0) {
				string defaultChapterPath = Util.CombinePaths(GetWorkspace(), GetLanguage(LanguageData.DefaultChapterName));
				if (!Util.DirectoryExists(defaultChapterPath)) {
					Util.CreateFolder(defaultChapterPath);
				}
				foreach (var file in files) {
					try {
						var newPath = Util.CombinePaths(defaultChapterPath, Util.GetNameWithExtension(file.FullName));
						if (!Util.FileExists(newPath)) {
							Util.MoveFile(file.FullName, newPath);
						}
					} catch { }
				}
			}
			// Load Chapters
			bool openFlag = false;
			var dirs = Util.GetDirectsIn(GetWorkspace(), true);
			foreach (var dir in dirs) {
				string name = dir.Name;
				if (string.IsNullOrEmpty(OpeningChapter)) {
					OpeningChapter.Value = name;
				}
				int fileCount = Util.GetFileCount(dir.FullName, "*.stager", System.IO.SearchOption.TopDirectoryOnly);
				// Spawn Chapter Item
				var graber = Instantiate(m_ChapterItemPrefab, m_ChapterContent);
				var rt = graber.transform as RectTransform;
				rt.name = name;
				rt.localPosition = (Vector2)rt.localPosition;
				rt.localRotation = Quaternion.identity;
				rt.localScale = Vector3.one;
				rt.SetAsLastSibling();
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (m_ChapterItemPrefab.transform as RectTransform).rect.height);
				// Coms
				var btn = graber.Grab<Button>();
				var text = graber.Grab<Text>("Name");
				var input = graber.Grab<InputField>("InputField");
				var trigger = graber.Grab<TriggerUI>();
				var label = graber.Grab<Text>("Label");
				btn.onClick.AddListener(OnClick);
				text.text = name;
				input.text = name;
				label.text = fileCount < 100 ? fileCount.ToString("00") : "99+";
				if (fileCount == 0) {
					var count = graber.Grab<Image>("Count");
					var c = count.color;
					c.a = 0.2f;
					count.color = c;
				}
				input.onEndEdit.AddListener(OnEditEnd);
				trigger.CallbackRight.AddListener(OnTrigger);
				// Open Chapter
				if (name == OpeningChapter) {
					OpenChapter(name);
					openFlag = true;
				}
				// Func
				void OnClick () => OpenChapterLogic(name);
				void OnEditEnd (string editText) {
					bool success = RenameChapterLogic(name, editText);
					if (success && name == OpeningChapter) {
						OpeningChapter.Value = editText;
					}
					InvokeOpen();
				}
				void OnTrigger () => OpenMenu(CHAPTER_ITEM_MENU_KEY, rt);
			}
			if (Util.HasFileIn(Trashbin, "*.stager") && OpeningChapter.Value == Util.GetNameWithoutExtension(Trashbin)) {
				OpenTrashbinLogic(true);
			} else if (!openFlag) {
				OpenChapterAt(0);
			}
		}


		private void CloseLogic () {
			ClearChapterContent();
			ClearProjectContent();
			CloseMoveProjectWindowLogic();
			m_NoProjectHint.gameObject.SetActive(false);
		}


		private void OpenChapterLogic (string name) => OpenChapterLogic(Util.CombinePaths(GetWorkspace(), name), name);


		private void OpenTrashbinLogic (bool ignoreEmptyDialog = false) {
			if (Util.HasFileIn(Trashbin, "*.stager")) {
				OpenChapterLogic(Trashbin, Util.GetNameWithoutExtension(Trashbin));
			} else if (!ignoreEmptyDialog) {
				DialogUtil.Dialog_OK(LanguageData.UI_TrashbinEmpty, DialogUtil.MarkType.Info);
			}
		}


		private void OpenChapterLogic (string path, string name) {

			if (!Util.DirectoryExists(path)) { return; }
			ClearProjectContent();

			// Highlight Name
			int len = m_ChapterContent.childCount;
			for (int i = 0; i < len; i++) {
				var rt = m_ChapterContent.GetChild(i);
				var graber = rt.GetComponent<Grabber>();
				var text = graber.Grab<Text>("Name");
				text.color = rt.name == name ? m_ChapterNameHighlight : m_ChapterNameNormal;
			}

			OpeningChapter.Value = name;

			// Get Projects
			var files = Util.GetFilesIn(path, true, "*.stager");

			// No Project Hint
			m_NoProjectHint.gameObject.SetActive(files.Length == 0);

			// Spawn Project Items
			foreach (var file in files) {
				var projectPath = file.FullName;
				var projectName = Util.GetNameWithoutExtension(file.Name);
				var graber = Instantiate(m_ProjectItemPrefab, m_ProjectContent);
				var rt = graber.transform as RectTransform;
				rt.name = projectPath;
				rt.localPosition = (Vector2)rt.localPosition;
				rt.localRotation = Quaternion.identity;
				rt.localScale = Vector3.one;
				rt.SetAsLastSibling();
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (m_ProjectItemPrefab.transform as RectTransform).rect.height);
				var trigger = graber.Grab<TriggerUI>("Root");
				var item = graber.Grab<ProjectItemUI>();
				trigger.CallbackDoubleClick.AddListener(OpenProject);
				trigger.CallbackRight.AddListener(OnMenuTriggerClick);
				graber.Grab<Button>("Start Button").onClick.AddListener(OpenProject);
				graber.Grab<Text>("Name").text = projectName;
				graber.Grab<Text>("Time").text = "-";
				item.Load(projectPath);
				void OpenProject () {
					CancelInvoke();
					GotoEditor(projectPath);
				}
				void OnMenuTriggerClick () => OpenMenu(PROJECT_ITEM_MENU_KEY, rt);
			}
			SortProjectLogic();
		}


		private void OpenMoveProjectWindowLogic (string projectPath) {
			ClearMoveProjectContent();
			m_MoveProjectRoot.gameObject.SetActive(true);
			var dirs = Util.GetDirectsIn(GetWorkspace(), true);
			foreach (var dir in dirs) {
				string name = dir.Name;
				// Spawn Chapter Item
				var graber = Instantiate(m_ChapterItemAltPrefab, m_MoveProjectContent);
				var rt = graber.transform as RectTransform;
				rt.name = name;
				rt.localPosition = (Vector2)rt.localPosition;
				rt.localRotation = Quaternion.identity;
				rt.localScale = Vector3.one;
				rt.SetAsLastSibling();
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (m_ChapterItemAltPrefab.transform as RectTransform).rect.height);
				// Coms
				var button = graber.Grab<Button>();
				var _name = graber.Grab<Text>("Name");
				_name.text = name;
				button.onClick.AddListener(OnClick);
				// Func
				void OnClick () {
					MoveProjectItemLogic(projectPath, name);
					InvokeOpen();
				}
			}
		}


		private void CloseMoveProjectWindowLogic () {
			ClearMoveProjectContent();
			m_MoveProjectRoot.gameObject.SetActive(false);
		}


		private bool RenameChapterLogic (string chapterName, string newName) {
			if (chapterName == newName) { return false; }
			try {
				var from = Util.CombinePaths(GetWorkspace(), chapterName);
				var to = Util.CombinePaths(GetWorkspace(), newName);
				if (!Util.DirectoryExists(from)) { return false; }
				if (Util.DirectoryExists(to)) {
					DialogUtil.Dialog_OK(LanguageData.Error_ChapterAlreadyExists, DialogUtil.MarkType.Warning);
					return false;
				}
				Util.MoveDirectory(from, to);
			} catch {
				return false;
			}
			return true;
		}


		private bool MoveProjectItemLogic (string projectPath, string chapterName) {
			string aimPath = Util.CombinePaths(GetWorkspace(), chapterName);
			string aimFile = Util.CombinePaths(aimPath, Util.GetNameWithExtension(projectPath));
			if (projectPath == aimFile || !Util.FileExists(projectPath) || !Util.DirectoryExists(aimPath)) { return false; }
			if (Util.FileExists(aimFile)) {
				DialogUtil.Dialog_OK(LanguageData.Error_ProjectAlreadyExists, DialogUtil.MarkType.Warning);
				return false;
			}
			try {
				Util.MoveFile(projectPath, aimFile);
			} catch {
				return false;
			}
			return true;
		}


		private void ShowChapterInExplorerLogic (string chapterName) {
			string path = Util.CombinePaths(GetWorkspace(), chapterName);
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(Util.GetFullPath(path));
		}


		private void ShowProjectInExplorerLogic (string projectPath) {
			string path = Util.GetParentPath(projectPath);
			if (!Util.DirectoryExists(path)) { return; }
			Util.ShowInExplorer(path);
		}


		private void DeleteChapterItemLogic (string chapterName) {
			string path = Util.CombinePaths(GetWorkspace(), chapterName);
			int fileCount = Util.GetFileCount(path, "*.stager", System.IO.SearchOption.TopDirectoryOnly);
			if (fileCount == 0) {
				// Delete
				Util.DeleteDirectory(path);
				OpenLogic();
			} else {
				// Dialog
				DialogUtil.Dialog_Delete_Cancel(LanguageData.UI_DeleteChapterConfirm, () => {
					// Move All Projects
					var files = Util.GetFilesIn(path, true, "*.stager");
					foreach (var file in files) {
						try {
							string trashPath = Util.CombinePaths(Trashbin, Util.GetNameWithExtension(file.FullName));
							if (Util.FileExists(trashPath)) {
								Util.DeleteFile(trashPath);
							} else {
								Util.MoveFile(file.FullName, trashPath);
							}
						} catch { }
					}
					// Delete Folder
					Util.DeleteDirectory(path);
					OpenLogic();
				});
			}
		}


		private void NewChapterLogic () {
			// Get Path
			string basicName = GetLanguage?.Invoke(LanguageData.NewChapterName);
			string chapterName = basicName;
			string path = Util.CombinePaths(GetWorkspace(), chapterName);
			int index = 0;
			while (Util.DirectoryExists(path)) {
				chapterName = basicName + "_" + index;
				index++;
				path = Util.CombinePaths(GetWorkspace(), chapterName);
			}
			// Create Folder
			Util.CreateFolder(path);
			// Opening Chapter
			OpeningChapter.Value = chapterName;
			// Reopen
			OpenLogic();
			// Start Rename
			int len = m_ChapterContent.childCount;
			for (int i = 0; i < len; i++) {
				var rt = m_ChapterContent.GetChild(i);
				if (rt.name == chapterName) {
					StartRenameChapter(rt as RectTransform);
					break;
				}
			}
		}


		private void NewProjectLogic () => SpawnProjectCreator(Util.CombinePaths(GetWorkspace(), OpeningChapter));


		private void ImportProjectLogic (string path) {
			if (!Util.FileExists(path)) { return; }
			string fileName = Util.GetNameWithExtension(path);
			string aimPath = Util.CombinePaths(GetWorkspace(), OpeningChapter, fileName);
			if (Util.FileExists(aimPath)) {
				DialogUtil.Open(string.Format(GetLanguage.Invoke(LanguageData.UI_OverlapProjectConfirm), fileName), DialogUtil.MarkType.Warning,
					() => {
						Util.CopyFile(path, aimPath);
					}, null, null, null, () => { }
				);
			} else {
				Util.CopyFile(path, aimPath);
			}
		}


		private void SortProjectLogic () {
			var items = new List<ProjectItemUI>(m_ProjectContent.GetComponentsInChildren<ProjectItemUI>(true));
			items.Sort(new ProjectItemUIComparer(ProjectSort));
			foreach (var item in items) {
				item.transform.SetAsLastSibling();
			}
		}


		// Clear
		private void ClearChapterContent () {
			int len = m_ChapterContent.childCount;
			for (int i = 0; i < len; i++) {
				DestroyImmediate(m_ChapterContent.GetChild(0).gameObject, false);
			}
		}


		private void ClearProjectContent () {
			int len = m_ProjectContent.childCount;
			for (int i = 0; i < len; i++) {
				DestroyImmediate(m_ProjectContent.GetChild(0).gameObject, false);
			}
		}


		private void ClearMoveProjectContent () {
			int len = m_MoveProjectContent.childCount;
			for (int i = 0; i < len; i++) {
				DestroyImmediate(m_MoveProjectContent.GetChild(0).gameObject, false);
			}
		}


		private void InvokeOpen (float dely = 0.01f) => Invoke("OpenLogic", dely);


		#endregion




	}
}