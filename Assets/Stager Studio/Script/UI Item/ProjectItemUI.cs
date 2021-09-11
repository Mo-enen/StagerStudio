namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class ProjectItemUI : MonoBehaviour {


		// API
		public string ProjectName { get; private set; } = "";
		public long LastEditTime { get; private set; } = 0;

		// Ser
		[SerializeField] private Image m_Background = null;
		[SerializeField] private Text m_ProjectName = null;
		[SerializeField] private Text m_Author = null;
		[SerializeField] private Text m_Time = null;
		[SerializeField] private AspectRatioFitter m_Fitter = null;
		[SerializeField] private string m_SpeedKey = "Speed";



		// MSG
		private void Start () {
			transform.GetChild(0).gameObject.SetActive(true);
			var ani = GetComponent<Animator>();
			ani.SetFloat(m_SpeedKey, Random.Range(0.8f, 1.2f));
		}




		// API
		public void Load (string projectPath) {
			if (!Util.FileExists(projectPath)) { return; }
			try {
				Project project = null;
				Color[] colors = null;
				int width = 0, height = 0;
				project = new Project();
				// Read Project
				project.SetTarget(Project.ChunkType.Info, Project.ChunkType.Cover);
				using (var stream = File.OpenRead(projectPath)) {
					using (var reader = new BinaryReader(stream)) {
						project.Read(reader);
					}
				}
				width = project.FrontCover.Width;
				height = project.FrontCover.Height;
				colors = Project.ImageData_to_Colors(project.FrontCover);
				if (!(project is null)) {
					// Apply Info
					LastEditTime = project.LastEditTime;
					ProjectName = project.ProjectName;
					string timeStr = Util.GetDisplayTimeFromTicks(project.LastEditTime);
					m_ProjectName.text = !string.IsNullOrEmpty(project.ProjectName) ? project.ProjectName : "(Untitled)";
					m_Author.text = !string.IsNullOrEmpty(project.BeatmapAuthor) ? project.BeatmapAuthor : "(Author)";
					m_Time.text = !string.IsNullOrEmpty(timeStr) ? timeStr : "-";
					if (colors != null && project.FrontCover != null && width > 0 && height > 0) {
						// Apply Cover
						m_Background.sprite = Project.Colors_to_Sprite(colors, width, height);
						m_Fitter.aspectRatio = (float)width / height;
					}
				}
				gameObject.SetActive(true);
			} catch { }
		}

	}
}