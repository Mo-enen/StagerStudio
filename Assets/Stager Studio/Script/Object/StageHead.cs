namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class StageHead : MonoBehaviour {




		#region --- SUB ---





		#endregion




		#region --- VAR ---


		// Api
		public static Beatmap Beatmap { get; set; } = null;
		public static float MusicTime { get; set; } = 0f;
		public static float SpeedMuti { get; set; } = 1f;
		public static bool UseAbreast { get; set; } = false;


		#endregion




		#region --- MSG ---


		private void Update () {

			int stageIndex = transform.GetSiblingIndex();
			var stageData = Beatmap != null && stageIndex < Beatmap.Stages.Count ? Beatmap.Stages[stageIndex] : null;
			if (stageData == null || !GetHeadActive(stageData)) {
				gameObject.SetActive(false);
				return;
			}



		}


		#endregion




		#region --- API ---


		public static bool GetHeadActive (Beatmap.Stage stageData) => !UseAbreast && MusicTime >= stageData.Time - 1f / SpeedMuti && MusicTime <= stageData.Time;


		#endregion




		#region --- LGC ---




		#endregion




	}
}