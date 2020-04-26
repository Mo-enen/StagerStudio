namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class StageTimer : ObjectTimer {




		#region --- VAR ---


		// Api
		public static bool UseAbreast { get; set; } = false;


		#endregion




		#region --- MSG ---


		private void OnEnable () => Update();


		private void Update () {

			int index = transform.GetSiblingIndex();
			if (Beatmap == null || !Beatmap.GetActive(4, index) || MusicPlaying) {
				gameObject.SetActive(false);
				return;
			}

			// Movement
			var stageData = Beatmap.Stages[index];
			var pos = Stage.GetStagePosition(stageData, index);
			var (zoneMin, zoneMax, _, _) = ZoneMinMax;
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			transform.rotation = Quaternion.Euler(0f, 0f, Stage.GetStageWorldRotationZ(stageData));

			// Head
			bool headActive = GetTimerActive(stageData.Time);
			if (HeadRenderer.gameObject.activeSelf != headActive) {
				HeadRenderer.gameObject.SetActive(headActive);
			}
			if (headActive) {
				HeadRenderer.transform.localPosition = new Vector3(
					0f, Util.Remap(stageData.Time, stageData.Time - 1f, 0f, 1f, MusicTime) * Sublength, 0f
				);
			}

			// Tail
			float endTime = stageData.Time + stageData.Duration;
			bool tailActive = GetTimerActive(endTime);
			if (TailRenderer.gameObject.activeSelf != tailActive) {
				TailRenderer.gameObject.SetActive(tailActive);
			}
			if (tailActive) {
				TailRenderer.transform.localPosition = new Vector3(
					0f, Util.Remap(endTime, endTime - 1f, 0f, 1f, MusicTime) * Sublength, 0f
				);
			}

			// Line
			bool lineActive = headActive || tailActive;
			if (LineRenderer.gameObject.activeSelf != lineActive) {
				LineRenderer.gameObject.SetActive(lineActive);
			}
		}


		#endregion




		#region --- API ---


		#endregion




		#region --- LGC ---


		private static bool GetTimerActive (float time) => MusicTime >= time - 1f && MusicTime <= time;


		#endregion




	}
}