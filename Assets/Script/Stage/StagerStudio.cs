namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Stage;
	using UI;


	public class StagerStudio : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable]
		public struct CursorData {
			public Texture2D Cursor;
			public Vector2 Offset;
		}


		#endregion




		#region --- VAR ---


		// Ser
		[Header("Stage")]
		[SerializeField] StageGame m_Game = null;
		[SerializeField] StageMusic m_Music = null;
		[SerializeField] StageLanguage m_Language = null;
		[SerializeField] StageProject m_Project = null;
		[Header("Data")]
		[SerializeField] Text[] m_LanguageTexts = null;
		[SerializeField] CursorData[] m_Cursors = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			Awake_Language();
			Awake_Music();
			Awake_Misc();
		}


		private void Awake_Language () {

			StageProject.GetLanguage = m_Language.Get;


		}


		private void Awake_Music () {
			StageMusic.OnMusicClipLoaded = () => {

			};
			StageMusic.OnMusicPlayPause = (isPlay) => {

			};
			StageMusic.OnMusicTimeChanged = (time, duration) => {

			};
			StageMusic.OnPitchChanged = () => {

			};
		}


		private void Awake_Misc () {
			CursorUI.GetCursorTexture = (index) => (
				index >= 0 ? m_Cursors[index].Cursor : null,
				index >= 0 ? m_Cursors[index].Offset : Vector2.zero
			);
		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---




		#endregion




		#region --- UTL ---




		#endregion




	}
}


#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEditor;
	[CustomEditor(typeof(StagerStudio))]
	public class StagerStudio_Inspector : Editor {
		private readonly string[] EXCLUDE = new string[] { "m_Script" };
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, EXCLUDE);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif