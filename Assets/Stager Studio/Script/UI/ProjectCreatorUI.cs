namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class ProjectCreatorUI : MonoBehaviour {


		// Handler
		public delegate string StringStringHandler (string key);
		public delegate void VoidStringHandler (string str);
		public delegate void VoidHandler ();

		// Api
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static VoidStringHandler GotoEditor { get; set; } = null;
		public static VoidHandler ImportMusic { get; set; } = null;
		public static Project.FileData MusicData { get; set; } = null;

		// Ser
		[SerializeField] private RectTransform[] m_Containers = null;
		[SerializeField] private RectTransform[] m_Dots = null;
		[SerializeField] private ProjectTemplateAsset[] m_Templates = null;
		[SerializeField] private RectTransform m_ContentRoot = null;
		[SerializeField] private RectTransform m_ContentPivot = null;
		[SerializeField] private RectTransform m_Lerp = null;
		[SerializeField] private Text m_GeneHintLabel = null;
		[SerializeField] private Button m_NextButton = null;
		[SerializeField] private Button m_BackButton = null;
		[SerializeField] private Button m_CreateButton = null;
		[SerializeField] private InputField m_ProjectName = null;
		[SerializeField] private InputField m_ProjectDescription = null;
		[SerializeField] private InputField m_BeatmapAuthor = null;
		[SerializeField] private InputField m_MusicAuthor = null;
		[SerializeField] private Text m_MusicSizeLabel = null;
		[SerializeField] private ProjectTemplateType m_FuckedType = ProjectTemplateType.Stager;
		[SerializeField] private Text[] m_LanguageTexts = null;

		// Data
		private const string PROJECT_TEMPLATE_HINT_KEY = "ProjectCreator.Hint.Template";
		private const string PROJECT_TYPE_HINT_KEY_FUCK = ".Fuck";
		private const string DIALOG_ErrorOnCreateProject = "Dialog.Error.FailCreateProject";
		private const string DIALOG_NoTitle = "ProjectCreator.Error.NoTitle";
		private const string DIALOG_NoMusic = "ProjectCreator.Error.NoMusic";
		private ProjectTemplateType CurrentProjectType = ProjectTemplateType.Stager;
		private int CurrentStep = 0;
		private bool UIMoving = true;

		// Project
		private static bool MusicSizeDirty = true;
		private string RootPath = "";


		// MSG
		private void Update () {
			if (UIMoving) {
				int uiMotion = 2;
				// Lerp Dot
				var oldPos = m_Lerp.position;
				var aimPos = m_Dots[CurrentStep].position;
				float dis = Vector2.Distance(aimPos, oldPos);
				if (dis > 0.0001f) {
					m_Lerp.position = Vector3.Lerp(oldPos, aimPos, Time.deltaTime * 6f);
					m_Lerp.localScale = new Vector3(dis * 20f + 1f, 1, 1);
				} else {
					uiMotion--;
				}
				// Lerp Content
				var content = m_Containers[CurrentStep];
				oldPos = m_ContentRoot.position;
				aimPos = m_ContentPivot.position - (content.position - oldPos);
				dis = Vector2.Distance(aimPos, oldPos);
				if (dis > 0.0001f) {
					m_ContentRoot.position = Vector3.Lerp(oldPos, aimPos, Time.deltaTime * 6f);
				} else {
					uiMotion--;
				}
				if (uiMotion <= 0) {
					UIMoving = false;
				}
			}
			// Music Size
			if (MusicSizeDirty) {
				m_MusicSizeLabel.text = MusicData != null ? (MusicData.Data.Length / 1024f / 1024f).ToString("0.0") + " MB" : "0 MB";
				MusicSizeDirty = false;
			}
		}


		// API
		public void Init (string rootPath) {
			// Project Info
			m_ProjectName.text = "New Project";
			m_ProjectDescription.text = "";
			m_MusicAuthor.text = "";
			m_BeatmapAuthor.text = "";
			// Language
			foreach (var tx in m_LanguageTexts) {
				tx.text = GetLanguage?.Invoke(tx.name);
			}
			// Final
			MusicSizeDirty = true;
			RootPath = rootPath;
			GotoStep(0);
			UI_SetProjectType((int)ProjectTemplateType.Stager);
		}


		public void UI_Next () => GotoStep(CurrentStep + 1);


		public void UI_Back () => GotoStep(CurrentStep - 1);


		public void UI_Create () {
			// Check
			if (string.IsNullOrEmpty(m_ProjectName.text)) {
				DialogUtil.Dialog_OK(DIALOG_NoTitle, DialogUtil.MarkType.Warning);
				return;
			}
			if (MusicData is null) {
				DialogUtil.Dialog_OK(DIALOG_NoMusic, DialogUtil.MarkType.Warning);
				return;
			}
			// Create Project
			var result = new Project() {
				LastEditTime = Util.GetLongTime(),
				ProjectName = m_ProjectName.text,
				Description = m_ProjectDescription.text,
				BackgroundAuthor = "",
				BeatmapAuthor = m_BeatmapAuthor.text,
				MusicAuthor = m_MusicAuthor.text,
				MusicData = MusicData,
				BackgroundData = null,
				FrontCover = null,
			};
			// Template
			ProjectTemplateAsset template = null;
			foreach (var _template in m_Templates) {
				if (_template.Type == CurrentProjectType) {
					template = _template;
					break;
				}
			}
			if (template != null) {
				// Data
				try {
					result.Palette.AddRange(template.Palette.GetPixels32());
					result.Tweens.AddRange(JsonUtility.FromJson<Project.TweenArray>(template.Tween.text).GetAnimationTweens());
				} catch {
					Debug.LogError("Failed to add data");
				}
				// Beatmap
				try {
					var key = System.Guid.NewGuid().ToString();
					result.OpeningBeatmap = key;
					Beatmap map = template.Beatmap != null ? JsonUtility.FromJson<Beatmap>(template.Beatmap.text) : Beatmap.NewBeatmap();
					map.CreatedTime = Util.GetLongTime();
					result.BeatmapMap.Add(key, map);
				} catch {
					Debug.LogError("Failed to create beatmap");
				}
				// Gene
				try {
					result.Gene = template.Gene;
				} catch {
					Debug.LogError("Failed to do gene");
				}
			}
			// Create File
			string path = Util.CombinePaths(RootPath, $"{Util.GetTimeString()}_{m_ProjectName.text}.stager");
			try {
				using (var stream = System.IO.File.Create(path)) {
					using (var writer = new System.IO.BinaryWriter(stream)) {
						result.Write(writer);
					}
				}
				UI_Close();
				GotoEditor(path);
			} catch (System.Exception ex) {
				DialogUtil.Open(GetLanguage(DIALOG_ErrorOnCreateProject) + "\n" + ex.Message, DialogUtil.MarkType.Warning, () => { });
			}
		}


		public void UI_Close () {
			MusicData = null;
			transform.parent.gameObject.SetActive(false);
			transform.parent.parent.InactiveIfNoChildActive();
			DestroyImmediate(gameObject, false);
		}


		public void UI_SetProjectType (int type) {
			CurrentProjectType = (ProjectTemplateType)type;
			ProjectTemplateAsset template = null;
			foreach (var _template in m_Templates) {
				if (_template.Type == CurrentProjectType) {
					template = _template;
					break;
				}
			}
			if (template != null) {
				m_GeneHintLabel.text = $"{GetLanguage(PROJECT_TEMPLATE_HINT_KEY)}{GetLanguage(template.Gene.Key + (CurrentProjectType == m_FuckedType ? PROJECT_TYPE_HINT_KEY_FUCK : ""))}";
			}
		}


		public void UI_ImportMusic () => ImportMusic();


		public static void SetMusicSizeDirty () => MusicSizeDirty = true;


		// LGC
		private void GotoStep (int index) {
			int len = m_Containers.Length;
			index = Mathf.Clamp(index, 0, len - 1);
			CurrentStep = index;
			m_NextButton.gameObject.SetActive(index < len - 1);
			m_BackButton.gameObject.SetActive(index > 0);
			m_CreateButton.gameObject.SetActive(index >= len - 1);
			UIMoving = true;
		}




	}
}