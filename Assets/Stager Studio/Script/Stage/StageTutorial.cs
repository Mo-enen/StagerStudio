namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Saving;


	public class StageTutorial : MonoBehaviour {




		#region --- SUB ---


		// Handler
		public delegate void VoidStringHandler (string str);


		#endregion




		#region --- VAR ---


		// Const
		private const string DIALOG_CanNotFindProject = "Dialog.Tutorial.CanNotFindProject";
		private const string DIALOG_FirstTutorialMSG = "Dialog.Tutorial.FirstTutorialMSG";

		// Handler
		public static VoidStringHandler OpenProjectAt { get; set; } = null;

		// Short
		private string TutorialProjectPath => Util.CombinePaths(Application.streamingAssetsPath, "Assets", "Tutorial Project.stager");

		// Ser
		[SerializeField] private RectTransform m_TutorialBoard = null;

		// Saving
		private SavingBool BoardClosed = new SavingBool("StageTutorial.BoardClosed", false);


		#endregion




		#region --- MSG ---


		private void Start () {
			if (BoardClosed.Value) {
				CloseBoard();
			} else if (m_TutorialBoard != null) {
				m_TutorialBoard.gameObject.SetActive(true);
			}
			enabled = false;
		}


		#endregion




		#region --- API ---


		public void StartTutorial () => DialogUtil.Dialog_OK_Cancel(DIALOG_FirstTutorialMSG, DialogUtil.MarkType.Info, StartTutorialLogic);


		public void CloseBoard () {
			BoardClosed.Value = true;
			if (m_TutorialBoard != null) {
				Destroy(m_TutorialBoard.gameObject);
			}
		}


		#endregion




		#region --- LGC ---


		private void StartTutorialLogic () {

			CloseBoard();

			if (!Util.FileExists(TutorialProjectPath)) {
				DialogUtil.Dialog_OK(DIALOG_CanNotFindProject, DialogUtil.MarkType.Error);
				return;
			}

			OpenProjectAt(TutorialProjectPath);







		}


		#endregion




	}
}