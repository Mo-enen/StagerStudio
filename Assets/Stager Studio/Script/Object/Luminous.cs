namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Luminous : StageObject {




		#region --- VAR ---


		// Api
		public static int SortingLayerID_Lum { get; set; } = -1;

		// Data
		private static Vector2 LuminousAppend = Vector2.zero;
		private static float LuminousDuration_Tap = 0f;
		private static float LuminousDuration_Hold = 0f;
		private static float LumHeight_Tap = 0f;
		private static float LumHeight_Hold = 0f;


		#endregion




		#region --- MSG ---



		private void OnEnable () {
			Update();
		}


		private void Update () {

			MainRenderer.RendererEnable = false;

			int index = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && index < Beatmap.Notes.Count ? Beatmap.Notes[index] : null;
			if (noteData is null || noteData.SpeedOnDrop < 0f) {
				gameObject.SetActive(false);
				return;
			}
			SkinType type = noteData.Duration > DURATION_GAP && MusicTime < noteData.Time + noteData.Duration ? SkinType.HoldLuminous : SkinType.NoteLuminous;
			Duration = type == SkinType.NoteLuminous ? LuminousDuration_Tap : LuminousDuration_Hold;
			MainRenderer.Type = type;

			if (MusicPlaying && GetLumActive(noteData)) {
				// Active
				Update_Movement(noteData, noteData.Duration <= DURATION_GAP, type);
			} else {
				// Inactive
				gameObject.SetActive(false);
			}

		}


		private void Update_Movement (Beatmap.Note noteData, bool tap, SkinType lumType) {

			// Life Time
			float noteEndTime = Time + noteData.Duration;
			MainRenderer.Duration = Duration;
			MainRenderer.LifeTime = MusicTime < noteEndTime ? MusicTime - noteData.Time : MusicTime - noteEndTime;
			MainRenderer.RendererEnable = true;

			// Get/Check Linked Track/Stage
			MainRenderer.RendererEnable = false;
			var linkedTrack = Beatmap.Tracks[noteData.TrackIndex];
			if (linkedTrack is null || !Track.GetTrackActive(linkedTrack)) { return; }
			var linkedStage = Beatmap.Stages[linkedTrack.StageIndex];
			if (linkedStage is null || !Stage.GetStageActive(linkedStage, linkedTrack.StageIndex)) { return; }

			Time = noteData.Time;
			var noteType = SkinType.Note;
			var judgeLineSize = GetRectSize(SkinType.JudgeLine);
			var stagePos = Stage.GetStagePosition(linkedStage, linkedTrack.StageIndex);
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			float trackX = Track.GetTrackX(linkedTrack);
			float trackWidth = Track.GetTrackWidth(linkedTrack);
			float trackRotX = Track.GetTrackAngle(linkedTrack);
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			var pos = Track.LocalToZone(
				noteData.X, stageHeight > 0f ? judgeLineSize.y / 2f / stageHeight : 0f, Note.GetNoteZ(noteData),
			stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, trackRotX
			);
			var noteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			
			// Movement
			var noteSize = GetRectSize(noteType);
			var lumSize = GetRectSize(lumType, true, true);
			float scaleX, scaleY;
			if (lumSize.x < 0f) {
				// Scaled
				scaleX = (noteSize.x < 0f ? stageWidth * trackWidth * noteData.Width : noteSize.x) + LuminousAppend.x;
			} else {
				// Fixed
				scaleX = lumSize.x + LuminousAppend.x;
			}
			if (lumSize.y < 0f) {
				// Scaled
				scaleY = (tap ? LumHeight_Tap : LumHeight_Hold) + LuminousAppend.y;
			} else {
				// Fixed
				scaleY = lumSize.y + LuminousAppend.y;
			}

			transform.position = noteWorldPos;
			MainRenderer.transform.rotation = Quaternion.Euler(0f, 0f, stageRotZ);
			MainRenderer.transform.localScale = new Vector3(
				zoneSize * scaleX,
				zoneSize * scaleY,
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = true;
			MainRenderer.Scale = new Vector2(scaleX, scaleY);
			MainRenderer.SetSortingLayer(SortingLayerID_Lum, GetSortingOrder());

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


		public static bool GetLumActive (Beatmap.Note noteData) {
			SkinType type = noteData.Duration > DURATION_GAP && MusicTime < noteData.Time + noteData.Duration ? SkinType.HoldLuminous : SkinType.NoteLuminous;
			float duration = (type == SkinType.NoteLuminous ? LuminousDuration_Tap : LuminousDuration_Hold);
			return
				MusicPlaying &&
				MusicTime >= noteData.Time &&
				duration > DURATION_GAP &&
				MusicTime <= noteData.Time + noteData.Duration + duration;
		}


		#endregion




	}
}