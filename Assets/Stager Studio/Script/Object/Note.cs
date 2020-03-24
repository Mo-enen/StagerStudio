namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;


	public class Note : StageObject {




		#region --- SUB ---


		public delegate float GameDropOffsetHandler (float muti);
		public delegate float DropOffsetHandler (float time, float muti);
		public delegate float FilledTimeHandler (float time, float fill, float muti);
		public delegate void VoidIntFloatHandler (int i, float f);


		#endregion




		#region --- VAR ---


		// Handler
		public static GameDropOffsetHandler GetGameDropOffset { get; set; } = null;
		public static DropOffsetHandler GetDropOffset { get; set; } = null;
		public static FilledTimeHandler GetFilledTime { get; set; } = null;
		public static VoidIntFloatHandler PlayClickSound { get; set; } = null;

		// API
		public static Vector2 NoteSize { get; private set; } = new Vector2(0.015f, 0.015f);
		public static Vector2 ArrowSize { get; private set; } = new Vector2(0.015f, 0.015f);
		public static float PoleThickness { get; private set; } = 0.007f;
		public static int LayerID_Note { get; set; } = -1;
		public static int LayerID_Note_Hold { get; set; } = -1;
		public static int SortingLayerID_Note { get; set; } = -1;
		public static int SortingLayerID_Note_Hold { get; set; } = -1;
		public static int SortingLayerID_Pole { get; set; } = -1;
		public static int SortingLayerID_Arrow { get; set; } = -1;

		// Ser
		[SerializeField] private ObjectRenderer m_SubRenderer = null;

		// Data
		private static byte CacheDirtyID = 1;
		private byte LocalCacheDirtyID = 0;
		private bool PrevClicked = false;
		private bool PrevClickedAlt = false;
		private Beatmap.Stage LateStage = null;
		private Beatmap.Track LateTrack = null;
		private Beatmap.Note LateNote = null;
		private Beatmap.Note LateLinkedNote = null;


		#endregion




		#region --- MSG ---



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
				Update_Gizmos(null, false, noteIndex);
				return;
			}

			// Click Sound
			bool clicked = MusicTime > noteData.Time;
			if (MusicPlaying && clicked && !PrevClicked && noteData.ClickSoundIndex >= 0) {
				PlayClickSound(noteData.ClickSoundIndex, 1f);
			}
			PrevClicked = clicked;

			// Alt Click Sound
			if (noteData.Duration > DURATION_GAP) {
				bool altClicked = MusicTime > noteData.Time + noteData.Duration;
				if (MusicPlaying && altClicked && !PrevClickedAlt) {
					PlayClickSound(noteData.ClickSoundIndex, 0.618f);
				}
				PrevClickedAlt = altClicked;
			}

			// Cache
			Update_Cache(noteData, GetGameSpeedMuti() * linkedStage.Speed);

			// Linked
			var linkedNote = noteData.LinkedNoteIndex >= 0 && noteData.LinkedNoteIndex < Beatmap.Notes.Count ? Beatmap.Notes[noteData.LinkedNoteIndex] : null;

			// Active
			bool active = GetNoteActive(noteData, linkedNote, noteData.AppearTime);
			noteData.Active = active;
			Update_Gizmos(noteData, oldSelecting, noteIndex);
			if (!active) { return; }
			noteData.Selecting = oldSelecting;

			// Tray
			Update_Tray(linkedTrack, noteData);

			// Final
			LateStage = linkedStage;
			LateTrack = linkedTrack;
			LateNote = noteData;
			LateLinkedNote = linkedNote;

		}


		protected override void LateUpdate () {
			if (LateNote is null || Beatmap is null) {
				base.LateUpdate();
				return;
			}
			Update_Movement(LateStage, LateTrack, LateNote, LateLinkedNote);
			base.LateUpdate();
		}


		private void Update_Cache (Beatmap.Note noteData, float speedMuti) {
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

			float duration = Mathf.Max(noteData.Duration, 0f);
			if (Duration != duration) {
				Duration = duration;
				noteData.NoteDropStart = -1f;
			}
			// Note Drop
			if (noteData.NoteDropStart < 0f) {
				noteData.NoteDropStart = GetDropOffset(noteData.Time, speedMuti);
				noteData.NoteDropEnd = GetDropOffset(noteData.Time + noteData.Duration, speedMuti);
			}
		}


		private void Update_Tray (Beatmap.Track linkedTrack, Beatmap.Note noteData) {
			if (linkedTrack.HasTray && noteData.Time < linkedTrack.TrayTime && (noteData.Time + noteData.Duration) > MusicTime) {
				linkedTrack.TrayTime = noteData.Time;
				linkedTrack.TrayX = (noteData.X - noteData.Width * 0.5f, noteData.X + noteData.Width * 0.5f);
			}
		}


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track linkedTrack, Beatmap.Note noteData, Beatmap.Note linkedNote) {

			var stagePos = Stage.GetStagePosition(linkedStage, linkedTrack.StageIndex);
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			float trackX = Track.GetTrackX(linkedTrack);
			float trackWidth = Track.GetTrackWidth(linkedTrack);
			float trackAngle = Track.GetTrackAngle(linkedTrack);
			float gameOffset = GetGameDropOffset(noteData.SpeedMuti);
			float noteY01 = MusicTime < Time ? (noteData.NoteDropStart - gameOffset) : 0f;
			float noteSizeY = noteData.NoteDropEnd - gameOffset - noteY01;
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			bool isLink = !(linkedNote is null);
			bool isSwipe = noteData.SwipeX != 1 || noteData.SwipeY != 1;
			float alpha = Stage.GetStageAlpha(linkedStage) * Track.GetTrackAlpha(linkedTrack) * Mathf.Clamp01(16f - noteY01 * 16f);

			// Movement
			var (noteZonePos, rotX, rotZ) = Track.Inside(
				noteData.X, noteY01,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, trackAngle
			);
			var noteRot = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0f, 0f);
			var noteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, noteZonePos.x, noteZonePos.y);
			noteWorldPos.z += noteZonePos.z * zoneSize;
			if (noteData.Z != 0f) {
				noteWorldPos += noteData.Z * zoneSize * (noteRot * Vector3.back);
			}
			float noteScaleX = NoteSize.x < 0f ? stageWidth * trackWidth * noteData.Width : NoteSize.x;
			float noteScaleY = Mathf.Max(noteSizeY * stageHeight, NoteSize.y);
			var zoneNoteScale = new Vector3(
				zoneSize * noteScaleX,
				zoneSize * noteScaleY,
				1f
			);
			transform.position = noteWorldPos;
			ColRot = MainRenderer.transform.rotation = noteRot;
			ColSize = MainRenderer.transform.localScale = zoneNoteScale;

			// Sub Update
			if (isLink) {
				Update_Movement_Linked(noteData, linkedNote, noteWorldPos, noteRot, alpha);
			} else if (isSwipe) {
				Update_Movement_Swipe(noteData, zoneSize, noteRot, alpha);
			}

			// Renderer
			MainRenderer.RendererEnable = !isLink || GetNoteActive(noteData, null, noteData.AppearTime);
			MainRenderer.Duration = m_SubRenderer.Duration = Duration;
			MainRenderer.LifeTime = m_SubRenderer.LifeTime = MusicTime - Time;
			MainRenderer.Alpha = alpha;
			MainRenderer.Scale = new Vector2(noteScaleX, noteScaleY);
			MainRenderer.Type = !noteData.Tap ? SkinType.SlideNote : noteData.Duration > DURATION_GAP ? SkinType.HoldNote : SkinType.TapNote;
			m_SubRenderer.Type = isLink ? SkinType.LinkPole : SkinType.SwipeArrow;
			MainRenderer.SetSortingLayer(Duration <= DURATION_GAP ? SortingLayerID_Note : SortingLayerID_Note_Hold, GetSortingOrder());

		}


		private void Update_Movement_Linked (Beatmap.Note noteData, Beatmap.Note linkedNote, Vector3 noteWorldPos, Quaternion noteRot, float alpha) {

			// Get Basic Linked Data
			if (linkedNote == null || noteData.Time + noteData.Duration > linkedNote.Time) { return; }
			var linkedTrack = Beatmap.GetTrackAt(linkedNote.TrackIndex);
			if (linkedTrack == null || !Track.GetTrackActive(linkedTrack)) { return; }
			var linkedStage = Beatmap.GetStageAt(linkedTrack.StageIndex);
			if (linkedStage == null || !Stage.GetStageActive(linkedStage, linkedTrack.StageIndex)) { return; }

			// Get Linked Stage Data
			Vector2 linkedStagePos = Stage.GetStagePosition(linkedStage, linkedTrack.StageIndex);
			float linkedStageWidth = Stage.GetStageWidth(linkedStage);
			float linkedStageHeight = Stage.GetStageHeight(linkedStage);
			float linkedStageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			float linkedTrackAngle = Track.GetTrackAngle(linkedTrack);

			// Movement
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			float gameOffset = GetGameDropOffset(noteData.SpeedMuti);
			float linkedNoteY01 = MusicTime < linkedNote.Time ? (linkedNote.NoteDropStart - gameOffset) : 0f;
			var (linkedZonePos, _, _) = Track.Inside(
				linkedNote.X,
				linkedNoteY01,
				linkedStagePos,
				linkedStageWidth,
				linkedStageHeight,
				linkedStageRotZ,
				Track.GetTrackX(linkedTrack),
				Track.GetTrackWidth(linkedTrack),
				linkedTrackAngle
			);

			// Linked Note World Pos
			var linkedNoteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, linkedZonePos.x, linkedZonePos.y);
			linkedNoteWorldPos.z += linkedZonePos.z * zoneSize;

			// Sub Pole World Pos
			var subWorldPos = m_SubRenderer.transform.position = Util.Remap(
				noteData.Time + noteData.Duration,
				linkedNote.Time,
				noteWorldPos + noteRot * new Vector3(
					0f,
					zoneSize * (MusicTime < noteData.Time ?
						noteData.NoteDropEnd - noteData.NoteDropStart :
						noteData.NoteDropEnd - gameOffset
					) * linkedStageHeight
				),
				linkedNoteWorldPos,
				MusicTime
			);

			// Get Scale Y from World Distance
			float scaleY = Vector3.Distance(subWorldPos, linkedNoteWorldPos);

			// Reduce Scale Y when Linked Note Not Appear Yet
			if (MusicTime < linkedNote.AppearTime) {
				scaleY -= scaleY * (linkedNoteY01 - 1f) / (linkedNoteY01 - noteData.NoteDropEnd + gameOffset);
			}

			// Final
			m_SubRenderer.transform.rotation = linkedNoteWorldPos != subWorldPos ? Quaternion.LookRotation(
				linkedNoteWorldPos - subWorldPos, -MainRenderer.transform.forward
			) * Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
			m_SubRenderer.transform.localScale = new Vector3(zoneSize * PoleThickness, scaleY, 1f);
			m_SubRenderer.RendererEnable = true;
			m_SubRenderer.Pivot = new Vector3(0.5f, 0f);
			m_SubRenderer.Scale = new Vector2(PoleThickness, scaleY / zoneSize);
			m_SubRenderer.Alpha = alpha * Mathf.Clamp01((linkedNote.NoteDropStart - gameOffset) * 16f);
			m_SubRenderer.SetSortingLayer(SortingLayerID_Pole, GetSortingOrder());
		}


		private void Update_Movement_Swipe (Beatmap.Note noteData, float zoneSize, Quaternion rot, float alpha) {
			float arrowZ = noteData.SwipeX == 1 ? (180f - 90f * noteData.SwipeY) : Mathf.Sign(-noteData.SwipeX) * (135f - 45f * noteData.SwipeY);
			m_SubRenderer.transform.localPosition = rot * new Vector3(0f, zoneSize * NoteSize.y * 0.5f, 0f);
			m_SubRenderer.transform.rotation = rot * Quaternion.Euler(0f, 0f, arrowZ);
			m_SubRenderer.transform.localScale = new Vector3(zoneSize * ArrowSize.x, zoneSize * ArrowSize.y, 1f);
			m_SubRenderer.RendererEnable = true;
			m_SubRenderer.Pivot = new Vector3(0.5f, 0.5f);
			m_SubRenderer.Scale = ArrowSize;
			m_SubRenderer.Alpha = alpha;
			m_SubRenderer.SetSortingLayer(SortingLayerID_Arrow, GetSortingOrder());
		}


		private void Update_Gizmos (Beatmap.Note noteData, bool selecting, int noteIndex) {

			bool active = !MusicPlaying && !(noteData is null) && noteData.Active && MusicTime < noteData.Time + noteData.Duration;

			// ID
			if (Label != null) {
				if (ShowIndexLabel && active) {
					Label.gameObject.SetActive(true);
					Label.Text = noteIndex.ToString();
					Label.transform.localRotation = MainRenderer.transform.localRotation;
				} else {
					Label.gameObject.SetActive(false);
				}
			}

			// Highlight
			if (Highlight != null) {
				Highlight.enabled = active && selecting;
			}

			// Col
			TrySetColliderLayer(Duration <= DURATION_GAP ? LayerID_Note : LayerID_Note_Hold);

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
			var noteSize = skin.TryGetItemSize((int)SkinType.TapNote) / skin.ScaleMuti;
			noteSize.x = Mathf.Max(noteSize.x, 0f);
			noteSize.y = Mathf.Max(noteSize.y, 0.001f);
			if (!skin.FixedNoteWidth) {
				noteSize.x = -1f;
			}
			NoteSize = noteSize;
			var arrowSize = skin.TryGetItemSize((int)SkinType.SwipeArrow) / skin.ScaleMuti;
			arrowSize.x = Mathf.Max(arrowSize.x, 0f);
			arrowSize.y = Mathf.Max(arrowSize.y, 0.001f);
			ArrowSize = new Vector2(arrowSize.x, arrowSize.y);
			PoleThickness = skin.TryGetItemSize((int)SkinType.LinkPole).x / skin.ScaleMuti;
		}


		protected override void RefreshRendererZone () {
			base.RefreshRendererZone();
			RefreshRendererZoneFor(m_SubRenderer);
		}


		#endregion




		#region --- LGC ---


		private static bool GetNoteActive (Beatmap.Note data, Beatmap.Note linkedNote, float appearTime) {
			if (linkedNote is null) {
				return MusicTime >= appearTime && MusicTime <= data.Time + data.Duration;
			} else {
				return MusicTime >= appearTime && (MusicTime <= data.Time + data.Duration || MusicTime <= linkedNote.Time);
			}
		}


		#endregion




	}
}