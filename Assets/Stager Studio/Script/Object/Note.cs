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
		public static float NoteThickness { get; private set; } = 0.015f;
		public static float PoleThickness { get; private set; } = 0.007f;
		public static int LayerID_Note { get; set; } = -1;
		public static int LayerID_Pole { get; set; } = -1;
		public static int LayerID_Arrow { get; set; } = -1;

		// Ser
		[SerializeField] private StageRenderer m_SubRenderer = null;

		// Data
		private static byte CacheDirtyID = 1;
		private static float ArrowSize = 0.1f;
		private byte LocalCacheDirtyID = 0;
		private Beatmap.Stage LateStage = null;
		private Beatmap.Track LateTrack = null;
		private Beatmap.Note LateNote = null;
		private Beatmap.Note LateLinkedNote = null;


		#endregion




		#region --- MSG ---


		protected override void Awake () {
			base.Awake();
			MainRenderer.Pivot = new Vector3(0.5f, 0f);
		}


		private void Update () {

			LateStage = null;
			LateTrack = null;
			LateNote = null;
			LateLinkedNote = null;
			MainRenderer.RendererEnable = false;
			m_SubRenderer.RendererEnable = false;
			ColSize = null;

			// Get NoteData
			int noteIndex = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && noteIndex < Beatmap.Notes.Count ? Beatmap.Notes[noteIndex] : null;
			if (noteData is null) { return; }
			bool oldSelecting = noteData.Selecting;
			noteData.Active = false;
			noteData.Selecting = false;

			// Get/Check Linked Track/Stage
			var linkedTrack = Beatmap.GetTrackAt(noteData.TrackIndex);
			var linkedStage = Beatmap.GetStageAt(linkedTrack.StageIndex);
			if (!Stage.GetStageActive(linkedStage, linkedTrack.StageIndex) || !Track.GetTrackActive(linkedTrack)) {
				Update_Gizmos(null, noteIndex);
				return;
			}

			// Cache
			Update_Cache(noteData, GetGameSpeedMuti() * linkedStage.Speed);

			// Linked
			var linkedNote = noteData.LinkedNoteIndex >= 0 && noteData.LinkedNoteIndex < Beatmap.Notes.Count ? Beatmap.Notes[noteData.LinkedNoteIndex] : null;

			// Active
			bool active = GetNoteActive(noteData, linkedNote, noteData.AppearTime);
			noteData.Active = active;
			Update_Gizmos(noteData, noteIndex);
			if (!active) { return; }
			noteData.Selecting = oldSelecting;

			// Final
			LateStage = linkedStage;
			LateTrack = linkedTrack;
			LateNote = noteData;
			LateLinkedNote = linkedNote;

		}


		protected override void LateUpdate () {
			if (LateNote is null) { return; }
			Update_Movement(LateStage, LateTrack, LateNote, LateLinkedNote);
			base.LateUpdate();
		}


		private void Update_Cache (Beatmap.Note noteData, float speedMuti) {
			if (MusicPlaying) { return; }
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


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track linkedTrack, Beatmap.Note noteData, Beatmap.Note linkedNote) {

			var stagePos = Stage.GetStagePosition(linkedStage, linkedTrack.StageIndex);
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			float trackX = Track.GetTrackX(linkedTrack);
			float trackWidth = Track.GetTrackWidth(linkedTrack);
			float stageAngle = Stage.GetStageAngle(linkedStage);
			float gameOffset = GetGameDropOffset(noteData.SpeedMuti);
			float noteY01 = MusicTime < Time ? (noteData.NoteDropStart - gameOffset) : 0f;
			float noteSizeY = noteData.NoteDropEnd - gameOffset - noteY01;
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			bool isLink = !(linkedNote is null);
			bool isSwipe = noteData.SwipeX != 1 || noteData.SwipeY != 1;
			float alpha = Stage.GetStageAlpha(linkedStage) *
				Track.GetTrackAlpha(linkedTrack) *
				Mathf.Clamp01(16f - noteY01 * 16f);

			// Movement
			var (noteZonePos, rotX, rotZ) = Track.Inside(
				noteData.X, noteY01,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, stageAngle
			);
			var notePos = Util.Vector3Lerp3(zoneMin, zoneMax, noteZonePos.x, noteZonePos.y);
			notePos.z += noteZonePos.z * zoneSize;
			var noteRot = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0f, 0f);
			transform.position = notePos;
			ColRot = MainRenderer.transform.rotation = noteRot;
			ColSize = MainRenderer.transform.localScale = new Vector3(
				zoneSize * Mathf.Max(stageWidth * trackWidth * noteData.Width, NoteThickness),
				zoneSize * Mathf.Max(noteSizeY * stageHeight, NoteThickness),
				1f
			);

			// Sub Update
			if (isLink) {
				Update_Movement_Linked(linkedTrack.StageIndex, noteData, linkedNote, noteRot, stagePos, stageWidth, stageHeight, stageRotZ, stageAngle, noteZonePos.x, alpha);
			} else if (isSwipe) {
				Update_Movement_Swipe(noteData, zoneSize, noteRot, alpha);
			}

			// Renderer
			MainRenderer.RendererEnable = !isLink || GetNoteActive(noteData, null, noteData.AppearTime);
			MainRenderer.LifeTime = m_SubRenderer.LifeTime = MusicTime - Time;
			MainRenderer.Alpha = alpha;
			MainRenderer.Scale = new Vector2(stageWidth * trackWidth * noteData.Width, Mathf.Max(noteSizeY * stageHeight, NoteThickness));
			MainRenderer.Type = !noteData.Tap ? SkinType.SlideNote : noteData.Duration > DURATION_GAP ? SkinType.HoldNote : SkinType.TapNote;
			m_SubRenderer.Type = isLink ? SkinType.LinkPole : SkinType.SwipeArrow;
			MainRenderer.SetSortingLayer(LayerID_Note, GetSortingOrder());
		}


		private void Update_Movement_Linked (int stageIndex, Beatmap.Note noteData, Beatmap.Note linkedNote, Quaternion noteRot, Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ, float stageAngle, float noteZoneX, float alpha) {

			// Get Linked Data
			if (linkedNote is null || noteData.Time + noteData.Duration > linkedNote.Time) { return; }
			var linkedTrack = Beatmap.GetTrackAt(linkedNote.TrackIndex);
			if (linkedTrack is null || linkedTrack.StageIndex != stageIndex || !Track.GetTrackActive(linkedTrack)) { return; }

			// Movement
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			float gameOffset = GetGameDropOffset(noteData.SpeedMuti);
			float linkedNoteY01 = MusicTime < linkedNote.Time ? (linkedNote.NoteDropStart - gameOffset) : 0f;
			var (linkedZonePos, _, _) = Track.Inside(
				linkedNote.X,
				linkedNoteY01,
				stagePos,
				stageWidth,
				stageHeight,
				stageRotZ,
				Track.GetTrackX(linkedTrack),
				Track.GetTrackWidth(linkedTrack),
				stageAngle
			);
			linkedZonePos += noteRot * Vector3.up * (zoneSize * NoteThickness * 0.5f);
			m_SubRenderer.transform.localPosition = noteRot * new Vector3(
				MusicTime < noteData.Time + noteData.Duration ? 0f : zoneSize * (
					linkedNoteY01 * (noteZoneX - linkedZonePos.x) / (linkedNoteY01 - noteData.NoteDropEnd + gameOffset) - noteZoneX + linkedZonePos.x
				),
				zoneSize * Mathf.Max(
					(MusicTime < noteData.Time ? noteData.NoteDropEnd - noteData.NoteDropStart : noteData.NoteDropEnd - gameOffset) * stageHeight - NoteThickness * 0.5f,
					NoteThickness * 0.5f
				)
			);
			var poleWorldPos = m_SubRenderer.transform.position;
			var linkedNoteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, linkedZonePos.x, linkedZonePos.y);
			linkedNoteWorldPos.z += linkedZonePos.z * zoneSize;
			float scaleY = Vector3.Distance(poleWorldPos, linkedNoteWorldPos);
			if (MusicTime < linkedNote.AppearTime) {
				scaleY -= scaleY * (linkedNoteY01 - 1f) / (linkedNoteY01 - noteData.NoteDropEnd + gameOffset);
			}
			m_SubRenderer.transform.rotation = Quaternion.FromToRotation(Vector3.up, linkedNoteWorldPos - poleWorldPos);
			m_SubRenderer.transform.localScale = new Vector3(zoneSize * PoleThickness, scaleY, 1f);
			m_SubRenderer.RendererEnable = true;
			m_SubRenderer.Pivot = new Vector3(0.5f, 0f);
			m_SubRenderer.Scale = new Vector2(PoleThickness, scaleY / zoneSize);
			m_SubRenderer.Alpha = alpha * Mathf.Clamp01((linkedNote.NoteDropStart - gameOffset) * 16f);
			m_SubRenderer.SetSortingLayer(LayerID_Pole, GetSortingOrder());
		}


		private void Update_Movement_Swipe (Beatmap.Note noteData, float zoneSize, Quaternion rot, float alpha) {
			float arrowZ = noteData.SwipeX == 1 ? (180f - 90f * noteData.SwipeY) : Mathf.Sign(-noteData.SwipeX) * (135f - 45f * noteData.SwipeY);
			m_SubRenderer.transform.localPosition = rot * new Vector3(0f, zoneSize * NoteThickness * 0.5f, 0f);
			m_SubRenderer.transform.rotation = rot * Quaternion.Euler(0f, 0f, arrowZ);
			m_SubRenderer.transform.localScale = new Vector3(zoneSize * NoteThickness, zoneSize * ArrowSize, 1f);
			m_SubRenderer.RendererEnable = true;
			m_SubRenderer.Pivot = new Vector3(0.5f, 0.5f);
			m_SubRenderer.Scale = new Vector2(NoteThickness, ArrowSize);
			m_SubRenderer.Alpha = alpha;
			m_SubRenderer.SetSortingLayer(LayerID_Arrow, GetSortingOrder());
		}


		private void Update_Gizmos (Beatmap.Note noteData, int noteIndex) {

			bool active = ShowIndexLabel && !MusicPlaying && !(noteData is null) && noteData.Active && MusicTime < noteData.Time + noteData.Duration;

			// ID
			Label.gameObject.SetActive(active);
			if (active) {
				Label.text = noteIndex.ToString();
				Label.transform.localRotation = MainRenderer.transform.localRotation;
			}

			// Selection Highlight




		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => CacheDirtyID++;


		public override void SetSkinData (SkinData skin) {
			base.SetSkinData(skin);
			m_SubRenderer.SkinData = skin;
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


		private static bool GetNoteActive (Beatmap.Note data, Beatmap.Note linkedNote, float appearTime) {
			if (linkedNote is null) {
				return MusicTime > appearTime && MusicTime < data.Time + data.Duration;
			} else {
				return MusicTime > appearTime && (MusicTime < data.Time + data.Duration || MusicTime < linkedNote.Time);
			}
		}


		#endregion




	}
}