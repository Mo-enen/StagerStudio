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
			var stagePos = Stage.GetStagePosition(stageData, index);
			var stageWidth = Stage.GetStageWidth(stageData);
			var stageHeight = Stage.GetStageHeight(stageData);
			var stagePivotY = Stage.GetStagePivotY(stageData);
			var stageRotZ = Stage.GetStageWorldRotationZ(stageData);
			var pos = Stage.LocalToZone(0.5f, 0f, 0f, stagePos, stageWidth, stageHeight, stagePivotY, stageRotZ);
			LateScaleY = stageHeight * zoneSize;
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			transform.rotation = Quaternion.Euler(0f, 0f, stageRotZ);
			LateTime = stageData.Time;
			LateDuration = stageData.Duration;

		}





		#endregion


		public static float ZoneToLocalY (
			float zoneX, float zoneY, float zoneZ, Vector2 stagePos, float stageHeight, float stagePivotY, float stageRotZ
		) => Matrix4x4.TRS(
			stagePos,
			Quaternion.Euler(0f, 0f, stageRotZ),
			new Vector3(1f, stageHeight, stageHeight)
		).inverse.MultiplyPoint3x4(new Vector3(zoneX, zoneY, zoneZ)).y + stagePivotY;



	}
}