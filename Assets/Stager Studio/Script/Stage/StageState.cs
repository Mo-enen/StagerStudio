namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UI;


	public class StageState : MonoBehaviour {



		private class LanguageData {
			public const string UI_GoHomeConfirmMsg = "UI.GoHomeConfirmMsg";
			public const string UI_GoHomeDirtyConfirmMsg = "UI.GoHomeDirtyConfirmMsg";
		}


		public delegate string StringStringHandler (string str);



		#region --- VAR ---


		// Handler
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Short
		private StageProject Project => _Project ?? (_Project = FindObjectOfType<StageProject>());

		// Ser
		[SerializeField] private HomeUI m_Home = null;
		[SerializeField] private RectTransform m_Zone = null;
		[SerializeField] private Animator m_UIAni = null;
		[SerializeField] private string m_ShowHomeKey = "ShowHome";
		[SerializeField] private string m_HideHomeKey = "HideHome";
		[SerializeField] private string m_ShowEditorKey = "ShowEditor";
		[SerializeField] private string m_HideEditorKey = "HideEditor";
		[SerializeField] private float m_Duration = 0.5f;

		// Data
		private Coroutine GoingCor = null;
		private StageProject _Project = null;


		#endregion




		#region --- MSG ---


		private void Start () {
			GoHomeLogic();
		}


		#endregion




		#region --- API ---


		public void GotoHome () {
			if (Project.IsDirty) {
				DialogUtil.Dialog_Yes_No_Cancel(
					LanguageData.UI_GoHomeDirtyConfirmMsg,
					DialogUtil.MarkType.Warning,
					() => {
						Project.SaveProject();
						GoHomeLogic();
					}, GoHomeLogic
				);
			} else {
				DialogUtil.Dialog_OK_Cancel(
					LanguageData.UI_GoHomeConfirmMsg,
					DialogUtil.MarkType.Info,
					GoHomeLogic
				);
			}
		}


		public void GotoEditor (string projectPath) => GotoEditorLogic(projectPath);


		#endregion




		#region --- LGC ---


		private void GoHomeLogic () {
			if (GoingCor != null) {
				StopCoroutine(GoingCor);
			}
			GoingCor = StartCoroutine(GoingToHome());
			IEnumerator GoingToHome () {

				// Hide
				m_UIAni.enabled = true;
				Project.CloseProject();
				m_Home.gameObject.SetActive(false);
				m_UIAni.SetTrigger(m_HideEditorKey);
				yield return new WaitForSeconds(m_Duration);

				m_Zone.gameObject.SetActive(false);

				// Show
				m_Home.gameObject.SetActive(true);
				m_Home.RefreshBarUI();

				m_UIAni.SetTrigger(m_ShowHomeKey);
				yield return new WaitForSeconds(m_Duration);

				m_Home.Open();

				// Final
				yield return new WaitUntil(() =>
					m_UIAni.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !m_UIAni.IsInTransition(0)
				);
				m_UIAni.enabled = false;
				yield break;
			}
		}


		private void GotoEditorLogic (string projectPath) {
			if (GoingCor != null) {
				StopCoroutine(GoingCor);
			}
			GoingCor = StartCoroutine(GoingToEditor());
			IEnumerator GoingToEditor () {

				// Hide
				m_UIAni.enabled = true;
				m_Zone.gameObject.SetActive(false);
				m_UIAni.SetTrigger(m_HideHomeKey);
				yield return new WaitForSeconds(m_Duration);

				m_Home.Close();
				m_Home.gameObject.SetActive(false);

				// Show
				m_Zone.gameObject.SetActive(true);
				m_UIAni.SetTrigger(m_ShowEditorKey);
				yield return new WaitForSeconds(m_Duration);

				Project.OpenProject(projectPath);

				// Final
				yield return new WaitUntil(() =>
					m_UIAni.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !m_UIAni.IsInTransition(0)
				);
				m_UIAni.enabled = false;
				yield break;
			}
		}




		#endregion




	}
}