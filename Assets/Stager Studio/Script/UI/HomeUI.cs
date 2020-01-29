namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	using Stage;
	using Saving;


	public class HomeUI : MonoBehaviour {




		#region --- SUB ---



		public delegate void LogHandler (string key);
		public delegate string LanguageHandler (string key);



		public static class LanguageData {
			public const string Error_ChapterAlreadyExists = "P-Manager.Error.ChapterAlreadyExists";
			public const string Error_ProjectAlreadyExists = "P-Manager.Error.ProjectAlreadyExists";
			public const string Error_NewProjectSourceNotExists = "P-Manager.Error.NewProjectSourceNotExists";
			public const string NewChapterName = "P-Manager.NewChapterName";

			public const string UI_ImportProjectTitle = "P-Manager.UI.ImportProjectTitle";
			public const string UI_OverlapProjectConfirm = "P-Manager.UI.OverlapProjectConfirm";
			public const string UI_NewProjectConfirm = "P-Manager.UI.NewProjectConfirm";
			public const string UI_PickWrokspaceTitle = "P-Manager.UI.PickWrokspaceTitle";
			public const string UI_FolderAlreadyExists = "P-Manager.UI.FolderAlreadyExists";
			public const string UI_CopyWorkspaceConfirm = "P-Manager.UI.CopyWorkspaceConfirm";
			public const string UI_TrashProjectItemConfirm = "P-Manager.UI.TrashProjectItemConfirm";
			public const string UI_DeleteProjectItemConfirm = "P-Manager.UI.DeleteProjectItemConfirm";
			public const string UI_TrashbinEmpty = "P-Manager.UI.TrashbinEmpty";

			public const string Hint_DoubleClick = "P-Manager.Hint.NeedDoubleClick";
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
		public static LogHandler LogHint { get; set; } = null;
		public static LanguageHandler GetLanguage { get; set; } = null;

		// Short
		private StageProject Project => _Project ?? (_Project = FindObjectOfType<StageProject>());

		private string Workspace { get => Project.TheWorkspace; set => Project.TheWorkspace = value; }
		private string Trashbin => Util.CombinePaths(Application.persistentDataPath, "Trashbin");
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
		[SerializeField] private RectTransform m_Bar = null;
		[SerializeField] private Grabber m_ChapterItemPrefab = null;
		[SerializeField] private Grabber m_ChapterItemAltPrefab = null;
		[SerializeField] private Grabber m_ProjectItemPrefab = null;
		[SerializeField] private Color m_ChapterNameNormal = Color.white;
		[SerializeField] private Color m_ChapterNameHighlight = Color.white;

		// Data
		private StageMenu Menu { get; set; } = null;
		private StageState State { get; set; } = null;
		private StageProject _Project { get; set; } = null;

		// Saving
		private SavingString OpeningChapter = new SavingString("HomeUI.OpeningChapter", "");
		private SavingInt ProjectSortIndex = new SavingInt("HomeUI.ProjectSortIndex", 1);


		#endregion




		#region --- MSG ---


		private void Awake () {
			Menu = FindObjectOfType<StageMenu>();
			State = FindObjectOfType<StageState>();
			OpeningChapter.Load();
			ProjectSortIndex.Load();
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
			State.GotoEditor(rt.name);
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
			if (!Util.DirectoryExists(Workspace)) { return; }
			//Application.OpenURL(Util.GetUrl(Workspace));
			Util.ShowInExplorer(Workspace);
		}


		public void PickWorkspace () {
			try {
				var path = DialogUtil.PickFolderDialog(LanguageData.UI_PickWrokspaceTitle);
				if (string.IsNullOrEmpty(path)) { return; }
				if (!Util.DirectoryExists(path)) {
					Util.CreateFolder(path);
				}
				if (Util.DirectoryExists(Workspace)) {
					string oldWorkSpace = Workspace;
					DialogUtil.Dialog_OK_Cancel(LanguageData.UI_CopyWorkspaceConfirm, DialogUtil.MarkType.Info, () => {
						bool existsWarning = false;
						var dirs = Util.GetDirectsIn(oldWorkSpace, true);
						foreach (var dir in dirs) {
							string to = Util.CombinePaths(path, dir.Name);
							if (Util.DirectoryExists(to)) {
								existsWarning = true;
								continue;
							}
							Util.MoveDirectory(dir.FullName, to);
						}
						if (existsWarning) {
							DialogUtil.Dialog_OK(LanguageData.UI_FolderAlreadyExists, DialogUtil.MarkType.Warning);
						}
					});
				}
				Workspace = path;
				OpenLogic();
			} catch { }
		}


		public void RefreshBarUI () => Invoke("RefreshBarUILogic", 0.01f);


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


		#endregion




		#region --- LGC ---


		private void OpenLogic () {
			ClearChapterContent();
			ClearProjectContent();
			CloseMoveProjectWindowLogic();
			m_NoProjectHint.gameObject.SetActive(false);
			Invoke("RefreshBarUILogic", 0.01f);
			// Load Chapters
			bool openFlag = false;
			var dirs = Util.GetDirectsIn(Workspace, true);
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
					Invoke("OpenLogic", 0.01f);
				}
				void OnTrigger () => Menu.OpenMenu(CHAPTER_ITEM_MENU_KEY, rt);
			}
			if (!openFlag) {
				OpenChapterAt(0);
			}
		}


		private void CloseLogic () {
			ClearChapterContent();
			ClearProjectContent();
			CloseMoveProjectWindowLogic();
			m_NoProjectHint.gameObject.SetActive(false);
		}


		private void OpenChapterLogic (string name) => OpenChapterLogic(Util.CombinePaths(Workspace, name), name);


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
				trigger.CallbackLeft.AddListener(ShowHint);
				trigger.CallbackRight.AddListener(OnMenuTriggerClick);
				graber.Grab<Button>("Start Button").onClick.AddListener(OpenProject);
				graber.Grab<Text>("Name").text = projectName;
				graber.Grab<Text>("Time").text = "-";
				item.Load(projectPath);
				void OpenProject () {
					CancelInvoke();
					State.GotoEditor(projectPath);
				}
				void ShowHint () => Invoke("Invoke_ShowDoubleClickHint", 0.618f);
				void OnMenuTriggerClick () => Menu.OpenMenu(PROJECT_ITEM_MENU_KEY, rt);
			}
			SortProjectLogic();
		}


		private void OpenMoveProjectWindowLogic (string projectPath) {
			ClearMoveProjectContent();
			m_MoveProjectRoot.gameObject.SetActive(true);
			var dirs = Util.GetDirectsIn(Workspace, true);
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
					bool success = MoveProjectItemLogic(projectPath, name);
					Invoke("OpenLogic", 0.01f);
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
				var from = Util.CombinePaths(Workspace, chapterName);
				var to = Util.CombinePaths(Workspace, newName);
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
			string aimPath = Util.CombinePaths(Workspace, chapterName);
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
			string path = Util.CombinePaths(Workspace, chapterName);
			if (!Util.DirectoryExists(path)) { return; }
			//Application.OpenURL(Util.GetUrl(Util.GetFullPath(path)));
			Util.ShowInExplorer(Util.GetFullPath(path));
		}


		private void ShowProjectInExplorerLogic (string projectPath) {
			string path = Util.GetParentPath(projectPath);
			if (!Util.DirectoryExists(path)) { return; }
			//Application.OpenURL(Util.GetUrl(path));
			Util.ShowInExplorer(path);
		}


		private void RefreshBarUILogic () {
			int len = m_Bar.childCount;
			for (int i = 0; i < len; i++) {
				var rt = m_Bar.GetChild(i) as RectTransform;
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (rt.GetChild(0) as RectTransform).rect.width);
			}
		}


		private void NewChapterLogic () {
			// Get Path
			string basicName = GetLanguage?.Invoke(LanguageData.NewChapterName);
			string chapterName = basicName;
			string path = Util.CombinePaths(Workspace, chapterName);
			int index = 0;
			while (Util.DirectoryExists(path)) {
				chapterName = basicName + "_" + index;
				index++;
				path = Util.CombinePaths(Workspace, chapterName);
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


		private void NewProjectLogic () {
			StagerStudio.Main.SpawnProjectCreator(Util.CombinePaths(Workspace, OpeningChapter));
		}


		private void ImportProjectLogic (string path) {
			if (!Util.FileExists(path)) { return; }
			string fileName = Util.GetNameWithExtension(path);
			string aimPath = Util.CombinePaths(Workspace, OpeningChapter, fileName);
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


		private void Invoke_ShowDoubleClickHint () => LogHint(GetLanguage(LanguageData.Hint_DoubleClick));


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


		#endregion




	}
}