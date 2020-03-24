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
		private float MovementDirtyTime = -1f;


		#endregion




		#region --- MSG ---


		private void Update () {

			MainRenderer.RendererEnable = false;

			int index = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && index < Beatmap.Notes.Count ? Beatmap.Notes[index] : null;
			if (noteData is null || !string.IsNullOrEmpty(noteData.Comment)) { return; }
			float noteEndTime = noteData.Time + noteData.Duration;

			if (!MusicPlaying) {
				MovementDirtyTime = float.MaxValue / 2f;
				SkinType type = noteData.Duration > DURATION_GAP && MusicTime < noteEndTime ? SkinType.HoldLuminous : SkinType.NoteLuminous;
				Duration = type == SkinType.NoteLuminous ? LuminousDuration_Tap : LuminousDuration_Hold;
				MainRenderer.Type = type;
				return;
			}

			// === Playing Only ===
			Update_Movement(noteData, noteEndTime, noteData.Duration <= DURATION_GAP);

		}


		private void Update_Movement (Beatmap.Note noteData, float noteEndTime, bool tap) {

			// Active Check
			if (MusicTime < noteData.Time || Duration <= DURATION_GAP || MusicTime > noteEndTime + Duration) { return; }

			// Life Time
			MainRenderer.Duration = Duration;
			MainRenderer.LifeTime = MusicTime < noteEndTime ? MusicTime - noteData.Time : MusicTime - noteEndTime;
			MainRenderer.RendererEnable = true;

			// Movement Dirty Check
			if (MovementDirtyTime < 0f || Mathf.Abs(MovementDirtyTime - MusicTime) < 0.01f) { return; }

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
			float trackRotX = Track.GetTrackAngle(linkedTrack);
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			var (pos, rotX, rotZ) = Track.Inside(
				noteData.X, stageHeight > 0f ? Stage.JudgeLineHeight / 2f / stageHeight : 0f,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, trackRotX
			);
			var noteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			if (noteData.Z != 0f) {
				var noteRot = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0f, 0f);
				noteWorldPos += noteData.Z * zoneSize * (noteRot * Vector3.back);
			}
			// Movement
			float scaleX = (Note.NoteSize.x < 0f ? stageWidth * trackWidth * noteData.Width : Note.NoteSize.x) + LuminousAppend.x;
			float scaleY = (tap ? LumHeight_Tap : LumHeight_Hold) + LuminousAppend.y;
			transform.position = noteWorldPos;
			MainRenderer.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
			MainRenderer.transform.localScale = new Vector3(
				zoneSize * scaleX,
				zoneSize * scaleY,
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = true;
			MainRenderer.Scale = new Vector2(scaleX, scaleY);
			MainRenderer.SetSortingLayer(LayerID_Lum, GetSortingOrder());

			// Final
			MovementDirtyTime = MusicTime;
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


		protected override void RefreshRendererZone () {
			MainRenderer.Renderer.material.SetVector(MaterialZoneID, new Vector4(
				float.MinValue, float.MinValue,
				float.MaxValue, float.MaxValue
			));
		}


		#endregion




	}
}