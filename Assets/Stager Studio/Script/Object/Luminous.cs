namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Luminous : StageObject {


		// Data
		private readonly float[] LuminousDuration = { 0f, 0f, 0f, };


		// MSG
		private void Update () {

			MainRenderer.RendererEnable = false;

			// Active Check
			int index = transform.GetSiblingIndex();
			var beatmap = GetBeatmap();
			var noteData = !(beatmap is null) && index < beatmap.Notes.Count ? beatmap.Notes[index] : null;
			if (noteData is null) { return; }

			// Get/Check Linked Track/Stage
			float musicTime = GetMusicTime();
			var linkedTrack = beatmap.GetTrackAt(noteData.TrackIndex);
			if (linkedTrack is null || !Track.GetTrackActive(linkedTrack, musicTime)) { return; }
			var linkedStage = beatmap.GetStageAt(linkedTrack.StageIndex);
			if (linkedStage is null || !Stage.GetStageActive(linkedStage, musicTime)) { return; }

			Time = noteData.Time;
			float noteEndTime = noteData.Time + noteData.Duration;
			if (musicTime < noteData.Time) { return; }
			SkinType type =
				noteData.Duration > DURATION_GAP && musicTime < noteEndTime ? SkinType.HoldLuminous :
				noteData.SwipeX.HasValue || noteData.SwipeY.HasValue ? SkinType.ArrowLuminous :
				SkinType.NoteLuminous;
			Duration = type == SkinType.NoteLuminous ? LuminousDuration[0] :
				 type == SkinType.HoldLuminous ? LuminousDuration[1] : LuminousDuration[2];
			if (Duration <= DURATION_GAP || musicTime > noteEndTime + Duration) { return; }

			// Movement
			const float LUMINOUS_SIZE = 0.04f;
			float zoneSize = GetZoneMinMax().size;
			float disc = Stage.GetStageDisc(linkedStage, musicTime);
			transform.position = Note.GetNoteWorldPosition(linkedStage, linkedTrack, noteData, noteData.Time);
			transform.rotation = Quaternion.Euler(0f, 0f, GetLuminousRotationZ(linkedStage, linkedTrack, noteData, noteData.Time));
			transform.localScale = Note.GetNoteWorldScale(linkedStage, linkedTrack, noteData, noteData.Time);

			// Renderer
			MainRenderer.RendererEnable = true;
			MainRenderer.Type = type;
			MainRenderer.LifeTime = musicTime > noteEndTime ? musicTime - noteEndTime : musicTime - noteData.Time;
			MainRenderer.Scale = Note.GetNoteRendererScale(linkedStage, linkedTrack, noteData, noteData.Time);
			MainRenderer.Disc = disc;
			MainRenderer.Pivot = new Vector3(0.5f, 0f);
			if (disc >= DISC_GAP) {
				MainRenderer.DiscOffsetY = 0f;
				MainRenderer.DiscWidth = Track.GetTrackWidth(linkedTrack, musicTime) * noteData.Width;
				MainRenderer.DiscHeight = zoneSize * LUMINOUS_SIZE;
			}

		}


		// API
		public override void SetSkinData (SkinData skin, int layerID, int orderID) {
			base.SetSkinData(skin, layerID, orderID);
			LuminousDuration[0] = skin is null ? 0f : skin.Items[(int)SkinType.NoteLuminous].TotalDuration;
			LuminousDuration[1] = skin is null ? 0f : skin.Items[(int)SkinType.HoldLuminous].TotalDuration;
			LuminousDuration[2] = skin is null ? 0f : skin.Items[(int)SkinType.ArrowLuminous].TotalDuration;
		}


		// LGC
		private static float GetLuminousRotationZ (Beatmap.Stage linkedStage, Beatmap.Track linkedTrack, Beatmap.Note noteData, float musicTime) {
			var (_, _, rotZ) = Track.Inside(
				linkedStage, linkedTrack, musicTime,
				noteData.X,
				0f
			);
			return rotZ;
		}


	}
}