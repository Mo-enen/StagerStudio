namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Note : StageObject {




		#region --- SUB ---


		public delegate float GameDropOffsetHandler (float muti);
		public delegate float DropOffsetHandler (float time, float muti);
		public delegate float FilledTimeHandler (float time, float fill, float muti);
		public delegate float SpeedMutiHandler ();


		#endregion




		#region --- VAR ---


		// Handler
		public static GameDropOffsetHandler GetGameDropOffset { get; set; } = null;
		public static DropOffsetHandler GetDropOffset { get; set; } = null;
		public static SpeedMutiHandler GetGameSpeedMuti { get; set; } = null;
		public static FilledTimeHandler GetFilledTime { get; set; } = null;

		// API
		public StageRenderer SubRenderer => m_SubRenderer;
		public static float NoteThickness { get; private set; } = 0.015f;
		public static float PoleThickness { get; private set; } = 0.007f;

		// Ser
		[SerializeField] private StageRenderer m_SubRenderer = null;

		// Data
		private static byte CacheDirtyID = 1;
		private static float ArrowSize = 0.1f;
		private byte LocalCacheDirtyID = 0;
		private Beatmap.Stage LateStage = null;
		private Beatmap.Track LateTrack = null;
		private Beatmap.Note LateNote = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			MainRenderer.Pivot = new Vector3(0.5f, 0f);

		}


		private void Update () {

			LateStage = null;
			LateTrack = null;
			LateNote = null;
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
			Update_Cache(noteData, GetGameSpeedMuti() * linkedStage.Speed);

			// Active
			if (!GetNoteActive(noteData, musicTime, noteData.AppearTime)) { return; }

			// Final
			LateStage = linkedStage;
			LateTrack = linkedTrack;
			LateNote = noteData;

		}


		private void LateUpdate () {
			if (LateNote is null) { return; }
			Update_Movement(LateStage, LateTrack, LateNote, GetMusicTime());
		}


		private void Update_Cache (Beatmap.Note noteData, float speedMuti) {
			if (GetMusicPlaying()) { return; }
			if (LocalCacheDirtyID != CacheDirtyID) {
				LocalCacheDirtyID = CacheDirtyID;
				Time = -1f;
			}
			if (speedMuti != noteData.SpeedMuti) {
				noteData.SpeedMuti = speedMuti;
				Time = -1f;
			}
			if (Time != noteData.Time) {
				Time = noteData.Time;
				noteData.AppearTime = GetFilledTime(
					noteData.Time,
					-1,
					speedMuti
				);
				noteData.NoteDropStart = -1f;
			}
			if (Duration != noteData.Duration) {
				Duration = noteData.Duration;
				noteData.NoteDropStart = -1f;
			}
			// Note Drop
			if (noteData.NoteDropStart < 0f) {
				noteData.NoteDropStart = GetDropOffset(noteData.Time, speedMuti);
				noteData.NoteDropEnd = GetDropOffset(noteData.Time + noteData.Duration, speedMuti);
			}
		}


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track linkedTrack, Beatmap.Note noteData, float musicTime) {

			var stagePos = Stage.GetStagePosition(linkedStage, musicTime);
			float stageWidth = Stage.GetStageWidth(linkedStage, musicTime);
			float stageHeight = Stage.GetStageHeight(linkedStage, musicTime);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage, musicTime);
			float trackX = Track.GetTrackX(linkedTrack, musicTime);
			float trackWidth = Track.GetTrackWidth(linkedTrack, musicTime);
			float stageAngle = Stage.GetStageAngle(linkedStage, musicTime);
			float gameOffset = GetGameDropOffset(noteData.SpeedMuti);
			float noteY01 = musicTime < Time ? (noteData.NoteDropStart - gameOffset) : 0f;
			float noteSizeY = noteData.NoteDropEnd - gameOffset - noteY01;
			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			bool isSwipe = noteData.SwipeX != 1 || noteData.SwipeY != 1;
			bool isLink = noteData.LinkedNoteIndex >= 0;
			float alpha = Stage.GetStageAlpha(linkedStage, musicTime, TRANSATION_DURATION) *
				Track.GetTrackAlpha(linkedTrack, musicTime, TRANSATION_DURATION) *
				Mathf.Clamp01(16f - noteY01 * 16f);

			// Movement
			var (pos, rotX, rotZ) = Track.Inside(
				noteData.X, noteY01,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, stageAngle
			);
			var notePos = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			notePos.z += pos.z * zoneSize;
			var noteRot = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0f, 0f);
			transform.position = notePos;
			MainRenderer.transform.rotation = noteRot;
			MainRenderer.transform.localScale = new Vector3(
				zoneSize * Mathf.Max(stageWidth * trackWidth * noteData.Width, NoteThickness),
				zoneSize * Mathf.Max(noteSizeY * stageHeight, NoteThickness),
				1f
			);

			if (isLink) {
				Update_Linked(linkedTrack.StageIndex, noteData, musicTime, noteRot, stagePos, stageWidth, stageHeight, stageRotZ, stageAngle, alpha);
			} else if (isSwipe) {
				Update_Swipe(noteData, zoneSize, rotZ, alpha);
			}

			// Renderer
			MainRenderer.RendererEnable = true;
			MainRenderer.LifeTime = SubRenderer.LifeTime = musicTime - Time;
			MainRenderer.Alpha = alpha;
			MainRenderer.Scale = new Vector2(stageWidth * trackWidth * noteData.Width, Mathf.Max(noteSizeY * stageHeight, NoteThickness));
			MainRenderer.Type = !noteData.Tap ? SkinType.SlideNote : noteData.Duration > DURATION_GAP ? SkinType.HoldNote : SkinType.TapNote;
			SubRenderer.Type = isLink ? SkinType.LinkPole : SkinType.SwipeArrow;
		}


		private void Update_Linked (int stageIndex, Beatmap.Note noteData, float musicTime, Quaternion noteRot, Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ, float stageAngle, float alpha) {
			// Get Linked Data
			var beatmap = GetBeatmap();
			var linkedNote = !(beatmap is null) && noteData.LinkedNoteIndex < beatmap.Notes.Count ? beatmap.Notes[noteData.LinkedNoteIndex] : null;
			if (linkedNote is null || noteData.Time + noteData.Duration > linkedNote.Time) { return; }
			var linkedTrack = beatmap.GetTrackAt(linkedNote.TrackIndex);
			if (linkedTrack is null || linkedTrack.StageIndex != stageIndex || !Track.GetTrackActive(linkedTrack, musicTime)) { return; }

			// Movement
			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			float gameOffset = GetGameDropOffset(noteData.SpeedMuti);
			float linkedNoteY01 = musicTime < linkedNote.Time ? (linkedNote.NoteDropStart - gameOffset) : 0f;
			float noteSizeY = musicTime < Time ? noteData.NoteDropEnd - noteData.NoteDropStart : noteData.NoteDropEnd - gameOffset;
			SubRenderer.transform.localPosition = noteRot * Vector3.up * (
				zoneSize * Mathf.Max(noteSizeY * stageHeight - NoteThickness * 0.5f, NoteThickness * 0.5f)
			);
			var subPivotPos = SubRenderer.transform.position;
			var (linkedPos, _, _) = Track.Inside(
				linkedNote.X,
				linkedNoteY01,
				stagePos,
				stageWidth,
				stageHeight,
				stageRotZ,
				Track.GetTrackX(linkedTrack, musicTime),
				Track.GetTrackWidth(linkedTrack, musicTime),
				stageAngle
			);
			linkedPos += noteRot * Vector3.up * (zoneSize * NoteThickness * 0.5f);
			var linkedNotePos = Util.Vector3Lerp3(zoneMin, zoneMax, linkedPos.x, linkedPos.y);
			linkedNotePos.z += linkedPos.z * zoneSize;
			float scaleY = Vector3.Distance(subPivotPos, linkedNotePos);
			if (musicTime < linkedNote.AppearTime) {
				float noteYEnd01 = musicTime < Time ? (noteData.NoteDropEnd - gameOffset) : 0f;
				scaleY -= scaleY * (linkedNoteY01 - 1f) / (linkedNoteY01 - noteYEnd01);
			}
			SubRenderer.transform.rotation = Quaternion.FromToRotation(Vector3.up, linkedNotePos - subPivotPos);
			SubRenderer.transform.localScale = new Vector3(zoneSize * PoleThickness, scaleY, 1f);
			SubRenderer.RendererEnable = true;
			SubRenderer.Pivot = new Vector3(0.5f, 0f);
			SubRenderer.Scale = new Vector2(PoleThickness, scaleY / zoneSize);
			SubRenderer.Alpha = alpha * Mathf.Clamp01((noteData.NoteDropEnd - gameOffset) * 16f);
		}


		private void Update_Swipe (Beatmap.Note noteData, float zoneSize, float rotZ, float alpha) {
			SubRenderer.transform.localPosition = new Vector3(0f, zoneSize * NoteThickness * 0.5f, 0f);
			SubRenderer.transform.rotation = Quaternion.Euler(0f, 0f, rotZ + (noteData.SwipeX == 1 ? (180f - 90f * noteData.SwipeY) : Mathf.Sign(-noteData.SwipeX) * (135f - 45f * noteData.SwipeY)));
			SubRenderer.transform.localScale = new Vector3(zoneSize * NoteThickness, zoneSize * ArrowSize, 1f);
			SubRenderer.RendererEnable = true;
			SubRenderer.Pivot = new Vector3(0.5f, 0.5f);
			SubRenderer.Scale = new Vector2(NoteThickness, ArrowSize);
			SubRenderer.Alpha = alpha;
		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => CacheDirtyID++;


		public override void SetSkinData (SkinData skin, int layerID, int orderID) {
			base.SetSkinData(skin, layerID, orderID);
			SubRenderer.SkinData = skin;
			SubRenderer.SetSortingLayer(layerID, orderID + 1);
		}


		public static void SetNoteSkin (SkinData skin) {
			// Thickness
			NoteThickness = skin.NoteThickness;
			PoleThickness = skin.PoleThickness;
			// Arrow
			ArrowSize = 0.1f;
			var aRects = skin.Items[(int)SkinType.SwipeArrow].Rects;
			if (!(aRects is null) && aRects.Count != 0) {
				var aRect = aRects[0];
				ArrowSize = skin.NoteThickness / aRect.Width * aRect.Height;
			}
		}


		#endregion




		#region --- LGC ---


		private static bool GetNoteActive (Beatmap.Note data, float musicTime, float appearTime) => musicTime > appearTime && musicTime < data.Time + data.Duration;


		#endregion




	}
}