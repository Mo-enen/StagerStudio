namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Note : StageObject {


		// SUB
		public delegate float GameDropOffsetHandler (float muti);
		public delegate float DropOffsetHandler (float time, float muti);
		public delegate float FilledTimeHandler (float time, float fill, float muti);
		public delegate float SpeedMutiHandler ();

		// Handler
		public static GameDropOffsetHandler GetGameDropOffset { get; set; } = null;
		public static DropOffsetHandler GetDropOffset { get; set; } = null;
		public static SpeedMutiHandler GetGameSpeedMuti { get; set; } = null;
		public static FilledTimeHandler GetFilledTime { get; set; } = null;

		// API
		public StageRenderer SubRenderer => m_SubRenderer;
		public static float NoteThickness { get; set; } = 0.015f;

		// Ser
		[SerializeField] private StageRenderer m_SubRenderer = null;

		// Data
		private static byte CacheDirtyID = 1;
		private byte LocalCacheDirtyID = 0;
		private float AppearTime = 0f;
		private float LocalSpeedMuti = float.MinValue;
		private float NoteDropStart = float.MinValue;
		private float NoteDropEnd = float.MinValue;


		// MSG
		private void Update () {

			MainRenderer.RendererEnable = false;
			SubRenderer.RendererEnable = false;

			// Get NoteData
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

			// Cache
			float speedMuti = GetGameSpeedMuti() * linkedStage.Speed;
			Update_Cache(noteData, speedMuti);

			// Active
			if (!GetNoteActive(noteData, musicTime, AppearTime)) { return; }

			// Movement
			Update_Movement(linkedStage, linkedTrack, noteData, musicTime, speedMuti);

		}


		private void Update_Cache (Beatmap.Note noteData, float speedMuti) {
			if (GetMusicPlaying()) { return; }
			if (LocalCacheDirtyID != CacheDirtyID) {
				LocalCacheDirtyID = CacheDirtyID;
				Time = -1f;
			}
			if (speedMuti != LocalSpeedMuti) {
				LocalSpeedMuti = speedMuti;
				Time = -1f;
			}
			if (Time != noteData.Time) {
				Time = noteData.Time;
				AppearTime = GetFilledTime(
					noteData.Time,
					-1,
					speedMuti
				);
				NoteDropStart = -1f;
			}
			if (Duration != noteData.Duration) {
				Duration = noteData.Duration;
				NoteDropStart = -1f;
			}

			// Note Drop Offset
			if (NoteDropStart < 0f) {
				NoteDropStart = GetDropOffset(noteData.Time, speedMuti);
				NoteDropEnd = GetDropOffset(noteData.Time + noteData.Duration, speedMuti);
			}
		}


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track linkedTrack, Beatmap.Note noteData, float musicTime, float speedMuti) {

			var stagePos = Stage.GetStagePosition(linkedStage, musicTime);
			float stageWidth = Stage.GetStageWidth(linkedStage, musicTime);
			float stageHeight = Stage.GetStageHeight(linkedStage, musicTime);
			float disc = Stage.GetStageDisc(linkedStage, musicTime);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage, musicTime);
			float trackX = Track.GetTrackX(linkedTrack, musicTime);
			float trackWidth = Track.GetTrackWidth(linkedTrack, musicTime);
			float trackRotX = Track.GetTrackRotation(linkedTrack, musicTime);
			float gameOffset = GetGameDropOffset(speedMuti);
			float noteY01 = musicTime < Time ? GetNoteY(gameOffset, NoteDropStart) : 0f;
			float noteSizeY = GetNoteY(gameOffset, NoteDropEnd) - noteY01;
			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			bool noDisc = disc < DISC_GAP;

			// Movement
			var (pos, rotX, rotZ) = Track.Inside(
				noteData.X, noDisc ? noteY01 : 0f,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, trackRotX, disc
			);
			var notePos = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			notePos.z += pos.z * zoneSize;
			transform.position = notePos;
			transform.rotation = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0f, 0f);
			transform.localScale = new Vector3(
				zoneSize * stageWidth * (noDisc ? trackWidth * noteData.Width : 1f),
				zoneSize * Mathf.Max(noDisc ? noteSizeY * stageHeight : stageWidth, NoteThickness),
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = true;
			MainRenderer.LifeTime = SubRenderer.LifeTime = musicTime - Time;
			MainRenderer.Alpha = SubRenderer.Alpha =
				Stage.GetStageAlpha(linkedStage, musicTime, TRANSATION_DURATION) *
				Track.GetTrackAlpha(linkedTrack, musicTime, TRANSATION_DURATION) *
				Mathf.Clamp01(16f - noteY01 * 16f);
			MainRenderer.Scale = new Vector2(
				stageWidth * trackWidth * noteData.Width,
				Mathf.Max(noDisc ? noteSizeY * stageHeight : noteSizeY, NoteThickness)
			);
			SubRenderer.Scale = Vector2.one;
			MainRenderer.Disc = disc;
			MainRenderer.Pivot = SubRenderer.Pivot = new Vector3(0.5f, 0f);
			MainRenderer.Type = !noteData.Tap ? SkinType.SlideNote :
				noteData.Duration > DURATION_GAP ? SkinType.HoldNote : SkinType.TapNote;
			SubRenderer.Type = noteData.SwipeX.HasValue || noteData.SwipeY.HasValue ? SkinType.SwipeArrow : SkinType.LinkPole;
			SubRenderer.RendererEnable = noteData.SwipeX.HasValue || noteData.SwipeY.HasValue || noteData.LinkedNoteIndex >= 0;
			if (!noDisc) {
				float scope = stageHeight / stageWidth;
				MainRenderer.DiscOffsetY = SubRenderer.DiscOffsetY = noteY01 * scope;
				MainRenderer.DiscWidth = SubRenderer.DiscWidth = trackWidth * noteData.Width;
				MainRenderer.DiscHeight = Mathf.Max(noteSizeY * scope, NoteThickness);
				SubRenderer.DiscHeight = zoneSize * NoteThickness;
			}

		}


		// API
		public static void SetCacheDirty () => CacheDirtyID++;


		public override void SetSkinData (SkinData skin, int layerID, int orderID) {
			base.SetSkinData(skin, layerID, orderID);
			SubRenderer.SkinData = skin;
			SubRenderer.SetSortingLayer(layerID, orderID + 1);
		}


		// LGC
		private static bool GetNoteActive (Beatmap.Note data, float musicTime, float appearTime) => musicTime > appearTime && musicTime < data.Time + data.Duration;


		private static float GetNoteY (float gameDropOffset, float noteDropOffset) => noteDropOffset - gameDropOffset;


	}
}