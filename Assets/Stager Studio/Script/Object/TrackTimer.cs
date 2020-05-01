namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class TrackTimer : ObjectTimer {



		private void OnEnable () {
			Update();
			LateUpdate();
		}


		private void Update () {

			int index = transform.GetSiblingIndex();
			if (Beatmap == null || !Beatmap.GetActive(5, index) || MusicPlaying) {
				gameObject.SetActive(false);
				return;
			}

			// Movement
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			var trackData = Beatmap.Tracks[index];
			var linkedStage = trackData.StageIndex >= 0 && trackData.StageIndex < Beatmap.Stages.Count ? Beatmap.Stages[trackData.StageIndex] : null;
			if (linkedStage == null || !linkedStage.Active) {
				gameObject.SetActive(false);
				return;
			}
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			var stagePos = Stage.GetStagePosition(linkedStage, trackData.StageIndex);
			float trackX = Track.GetTrackX(trackData);
			var pos = Stage.LocalToZone(trackX, 0f, 0f, stagePos, stageWidth, stageHeight, stageRotZ);
			LateScaleY = Stage.GetStageHeight(linkedStage) * zoneSize;
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			transform.rotation = Quaternion.Euler(0f, 0f, stageRotZ);
			LateTime = trackData.Time;
			LateDuration = trackData.Duration;

		}










	}
}