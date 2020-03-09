namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Luminous : StageObject {




		#region --- VAR ---


		// Api
		public static int LayerID_Lum { get; set; } = -1;

		// Data
		private static Vector2 LuminousAppend = Vector2.zero;
		private static float LuminousDuration_Tap = 0f;
		private static float LuminousDuration_Hold = 0f;
		private static float LumHeight_Tap = 0f;
		private static float LumHeight_Hold = 0f;
		private bool MovementDirty = true;


		#endregion




		#region --- MSG ---


		private void Update () {

			MainRenderer.RendererEnable = false;

			int index = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && index < Beatmap.Notes.Count ? Beatmap.Notes[index] : null;
			if (noteData is null) { return; }
			float noteEndTime = noteData.Time + noteData.Duration;

			if (!MusicPlaying) {
				MovementDirty = true;
				SkinType type = noteData.Duration > DURATION_GAP ? SkinType.HoldLuminous : SkinType.NoteLuminous;
				Duration = type == SkinType.NoteLuminous ? LuminousDuration_Tap : LuminousDuration_Hold;
				MainRenderer.Type = type;
				return;
			}

			// === Playing Only ===
			Update_Movement(noteData, noteEndTime, noteData.Duration <= DURATION_GAP);

		}


		private void Update_Movement (Beatmap.Note noteData, float noteEndTime, bool tap) {

			// Active Check
			if (MusicTime < noteData.Time || Duration <= DURATION_GAP) { return; }
			if (tap) {
				if (MusicTime > noteEndTime + Duration) { return; }
			} else {
				if (MusicTime > noteEndTime) { return; }
			}

			// Life Time
			MainRenderer.LifeTime = tap ? MusicTime - noteEndTime : MusicTime - noteData.Time;
			MainRenderer.RendererEnable = true;

			// Movement Dirty Check
			if (!MovementDirty) { return; }

			// Get/Check Linked Track/Stage
			MainRenderer.RendererEnable = false;
			var linkedTrack = Beatmap.GetTrackAt(noteData.TrackIndex);
			if (linkedTrack is null || !Track.GetTrackActive(linkedTrack)) { return; }
			var linkedStage = Beatmap.GetStageAt(linkedTrack.StageIndex);
			if (linkedStage is null || !Stage.GetStageActive(linkedStage, linkedTrack.StageIndex)) { return; }

			Time = noteData.Time;
			var stagePos = Stage.GetStagePosition(linkedStage, linkedTrack.StageIndex);
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			float trackX = Track.GetTrackX(linkedTrack);
			float trackWidth = Track.GetTrackWidth(linkedTrack);
			float trackRotX = Stage.GetStageAngle(linkedStage);
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			var (pos, _, rotZ) = Track.Inside(
				noteData.X, stageHeight > 0f ? Stage.JudgeLineHeight / 2f / stageHeight : 0f,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, trackRotX
			);

			// Movement
			float scaleX = (Note.NoteSize.x < 0f ? stageWidth * trackWidth * noteData.Width : Note.NoteSize.x) + LuminousAppend.x;
			float scaleY = (tap ? LumHeight_Tap : LumHeight_Hold) + LuminousAppend.y;
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			MainRenderer.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
			MainRenderer.transform.localScale = new Vector3(
				zoneSize * scaleX,
				zoneSize * scaleY,
				1f
			);

			// Renderer
			MainRenderer.LifeTime = MusicTime > noteEndTime ? MusicTime - noteEndTime : MusicTime - noteData.Time;
			MainRenderer.RendererEnable = true;
			MainRenderer.Scale = new Vector2(scaleX, scaleY);
			MainRenderer.SetSortingLayer(LayerID_Lum, GetSortingOrder());

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
			LuminousDuration_Tap = lumAni0 is null ? 0f : lumAni0.TotalDuration;
			LuminousDuration_Hold = lumAni1 is null ? 0f : lumAni1.TotalDuration;
			LumHeight_Tap = skin.TryGetItemSize((int)SkinType.NoteLuminous).y / skin.ScaleMuti;
			LumHeight_Hold = skin.TryGetItemSize((int)SkinType.HoldLuminous).y / skin.ScaleMuti;
		}


		#endregion




	}
}