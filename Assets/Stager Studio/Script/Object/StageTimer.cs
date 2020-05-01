namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class StageTimer : ObjectTimer {




		#region --- MSG ---


		private void OnEnable () {
			Update();
			LateUpdate();
		}


		private void Update () {

			int index = transform.GetSiblingIndex();
			if (Beatmap == null || !Beatmap.GetActive(4, index) || MusicPlaying) {
				gameObject.SetActive(false);
				return;
			}

			// Movement
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			var stageData = Beatmap.Stages[index];
			var pos = Stage.GetStagePosition(stageData, index);
			LateScaleY = Stage.GetStageHeight(stageData) * zoneSize;
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			transform.rotation = Quaternion.Euler(0f, 0f, Stage.GetStageWorldRotationZ(stageData));
			LateTime = stageData.Time;
			LateDuration = stageData.Duration;

		}





		#endregion



	}
}