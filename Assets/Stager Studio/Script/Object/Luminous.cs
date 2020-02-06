namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Luminous : StageObject {




		#region --- VAR ---


		// Data
		private readonly static float[] LuminousDuration = { 0f, 0f, 0f, };
		private static Vector2 LuminousAppend = Vector2.zero;
		private bool MovementDirty = true;


		#endregion




		#region --- MSG ---


		private void Awake () {
			MainRenderer.Pivot = new Vector3(0.5f, 0f);

		}


		private void Update () {

			MainRenderer.RendererEnable = false;

			int index = transform.GetSiblingIndex();
			var beatmap = GetBeatmap();
			var noteData = !(beatmap is null) && index < beatmap.Notes.Count ? beatmap.Notes[index] : null;
			if (noteData is null) { return; }
			float musicTime = GetMusicTime();
			float noteEndTime = noteData.Time + noteData.Duration;

			if (!GetMusicPlaying()) {
				MovementDirty = true;
				SkinType type =
					noteData.Duration > DURATION_GAP && musicTime < noteEndTime ? SkinType.HoldLuminous :
					noteData.SwipeX != 1 || noteData.SwipeY != 1 ? SkinType.ArrowLuminous :
					SkinType.NoteLuminous;
				Duration = type == SkinType.NoteLuminous ? LuminousDuration[0] :
					 type == SkinType.HoldLuminous ? LuminousDuration[1] : LuminousDuration[2];
				MainRenderer.Type = type;
				return;
			}

			// === Playing Only ===
			Update_Movement(beatmap, noteData, musicTime, noteEndTime);

		}


		private void Update_Movement (Beatmap beatmap, Beatmap.Note noteData, float musicTime, float noteEndTime) {

			// Active Check
			if (musicTime < noteData.Time || Duration <= DURATION_GAP || musicTime > noteEndTime + Duration) { return; }

			// Life Time
			MainRenderer.LifeTime = musicTime > noteEndTime ? musicTime - noteEndTime : musicTime - noteData.Time;
			MainRenderer.RendererEnable = true;

			// Movement Dirty Check
			if (!MovementDirty) { return; }

			// Get/Check Linked Track/Stage
			MainRenderer.RendererEnable = false;
			var linkedTrack = beatmap.GetTrackAt(noteData.TrackIndex);
			if (linkedTrack is null || !Track.GetTrackActive(linkedTrack, musicTime)) { return; }
			var linkedStage = beatmap.GetStageAt(linkedTrack.StageIndex);
			if (linkedStage is null || !Stage.GetStageActive(linkedStage, musicTime)) { return; }

			Time = noteData.Time;
			var stagePos = Stage.GetStagePosition(linkedStage, musicTime);
			float stageWidth = Stage.GetStageWidth(linkedStage, musicTime);
			float stageHeight = Stage.GetStageHeight(linkedStage, musicTime);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage, musicTime);
			float trackX = Track.GetTrackX(linkedTrack, musicTime);
			float trackWidth = Track.GetTrackWidth(linkedTrack, musicTime);
			float trackRotX = Stage.GetStageAngle(linkedStage, musicTime);
			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			var (pos, _, rotZ) = Track.Inside(
				noteData.X, 0f,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, trackRotX
			);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			MainRenderer.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
			MainRenderer.transform.localScale = new Vector3(
				zoneSize * (stageWidth * trackWidth * noteData.Width + LuminousAppend.x),
				zoneSize * LuminousAppend.y,
				1f
			);

			// Renderer
			MainRenderer.LifeTime = musicTime > noteEndTime ? musicTime - noteEndTime : musicTime - noteData.Time;
			MainRenderer.RendererEnable = true;
			MainRenderer.Scale = new Vector2(stageWidth * trackWidth * noteData.Width, 1f);



			// Final
			MovementDirty = false;
		}


		#endregion




		#region --- API ---


		public static void SetLuminousSkin (SkinData skin) {
			LuminousAppend.x = skin is null ? 0f : skin.LuminousAppendX;
			LuminousAppend.y = skin is null ? 0f : skin.LuminousAppendY;
			var lumAni0 = skin?.Items[(int)SkinType.NoteLuminous];
			var lumAni1 = skin?.Items[(int)SkinType.HoldLuminous];
			var lumAni2 = skin?.Items[(int)SkinType.ArrowLuminous];
			LuminousDuration[0] = lumAni0 is null ? 0f : lumAni0.TotalDuration;
			LuminousDuration[1] = lumAni1 is null ? 0f : lumAni1.TotalDuration;
			LuminousDuration[2] = lumAni2 is null ? 0f : lumAni2.TotalDuration;
		}


		#endregion




	}
}