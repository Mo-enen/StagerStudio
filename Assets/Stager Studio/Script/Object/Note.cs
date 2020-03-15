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
		public delegate float SpeedMutiHandler ();
		public delegate void VoidIntFloatHandler (int i, float f);


		#endregion




		#region --- VAR ---


		// Handler
		public static GameDropOffsetHandler GetGameDropOffset { get; set; } = null;
		public static DropOffsetHandler GetDropOffset { get; set; } = null;
		public static SpeedMutiHandler GetGameSpeedMuti { get; set; } = null;
		public static FilledTimeHandler GetFilledTime { get; set; } = null;
		public static VoidIntFloatHandler PlayClickSound { get; set; } = null;

		// API
		public static Vector2 NoteSize { get; private set; } = new Vector2(0.015f, 0.015f);
		public static Vector2 ArrowSize { get; private set; } = new Vector2(0.015f, 0.015f);
		public static float PoleThickness { get; private set; } = 0.007f;
		public static int LayerID_Note { get; set; } = -1;
		public static int LayerID_Shadow { get; set; } = -1;
		public static int LayerID_Pole { get; set; } = -1;
		public static int LayerID_Arrow { get; set; } = -1;

		// Ser
		[SerializeField] private ObjectRenderer m_SubRenderer = null;
		[SerializeField] private ObjectRenderer m_ShadowRenderer = null;

		// Data
		private static byte CacheDirtyID = 1;
		private static float ShadowDistance = 0f;
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
			m_ShadowRenderer.RendererEnable = false;
			ColSize = null;

			// Get NoteData
			int noteIndex = transform.GetSiblingIndex();
			var noteData = !(Beatmap is null) && noteIndex < Beatmap.Notes.Count ? Beatmap.Notes[noteIndex] : null;
			if (noteData is null) { return; }
			bool oldSelecting = noteData.Selecting;
			noteData.Active = false;
			noteData.Selecting = false;

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

			// Get/Check Linked Track/Stage
			var linkedTrack = Beatmap.GetTrackAt(noteData.TrackIndex);
			var linkedStage = Beatmap.GetStageAt(linkedTrack.StageIndex);
			if (!Stage.GetStageActive(linkedStage, linkedTrack.StageIndex) || !Track.GetTrackActive(linkedTrack)) {
				Update_Gizmos(null, false, noteIndex);
				return;
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
			if (LateNote is null) { return; }
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
			float stageAngle = Stage.GetStageAngle(linkedStage);
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
				trackX, trackWidth, stageAngle
			);
			var notePos = Util.Vector3Lerp3(zoneMin, zoneMax, noteZonePos.x, noteZonePos.y);
			notePos.z += noteZonePos.z * zoneSize;
			var noteRot = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0f, 0f);
			float noteScaleX = NoteSize.x < 0f ? stageWidth * trackWidth * noteData.Width : NoteSize.x;
			float noteScaleY = Mathf.Max(noteSizeY * stageHeight, NoteSize.y);
			var zoneNoteScale = new Vector3(
				zoneSize * noteScaleX,
				zoneSize * noteScaleY,
				1f
			);
			transform.position = notePos;
			ColRot = MainRenderer.transform.rotation = noteRot;
			ColSize = MainRenderer.transform.localScale = zoneNoteScale;

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
			MainRenderer.Scale = new Vector2(noteScaleX, noteScaleY);
			MainRenderer.Type = !noteData.Tap ? SkinType.SlideNote : noteData.Duration > DURATION_GAP ? SkinType.HoldNote : SkinType.TapNote;
			m_SubRenderer.Type = isLink ? SkinType.LinkPole : SkinType.SwipeArrow;
			MainRenderer.SetSortingLayer(LayerID_Note, GetSortingOrder());

			// Shadow
			if (ShadowDistance > 0.0001f) {
				// Movement
				m_ShadowRenderer.transform.localPosition = Vector3.down * ShadowDistance;
				m_ShadowRenderer.transform.rotation = noteRot;
				m_ShadowRenderer.transform.localScale = zoneNoteScale;
				// Render
				m_ShadowRenderer.RendererEnable = MainRenderer.RendererEnable;
				m_ShadowRenderer.Type = MainRenderer.Type;
				m_ShadowRenderer.LifeTime = MainRenderer.LifeTime;
				m_ShadowRenderer.Tint = Color.black;
				m_ShadowRenderer.Scale = MainRenderer.Scale;
				m_ShadowRenderer.Alpha = alpha * 0.309f;
				m_ShadowRenderer.SetSortingLayer(LayerID_Shadow, GetSortingOrder());
			}
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
			m_SubRenderer.transform.localPosition = noteRot * new Vector3(
				MusicTime < noteData.Time + noteData.Duration ? 0f : zoneSize * (
					linkedNoteY01 * (noteZoneX - linkedZonePos.x) / (linkedNoteY01 - noteData.NoteDropEnd + gameOffset) - noteZoneX + linkedZonePos.x
				),
				zoneSize * Mathf.Max(
					(MusicTime < noteData.Time ?
						noteData.NoteDropEnd - noteData.NoteDropStart :
						noteData.NoteDropEnd - gameOffset
					) * stageHeight, 0f
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
			m_SubRenderer.transform.localPosition = rot * new Vector3(0f, zoneSize * NoteSize.y * 0.5f, 0f);
			m_SubRenderer.transform.rotation = rot * Quaternion.Euler(0f, 0f, arrowZ);
			m_SubRenderer.transform.localScale = new Vector3(zoneSize * ArrowSize.x, zoneSize * ArrowSize.y, 1f);
			m_SubRenderer.RendererEnable = true;
			m_SubRenderer.Pivot = new Vector3(0.5f, 0.5f);
			m_SubRenderer.Scale = ArrowSize;
			m_SubRenderer.Alpha = alpha;
			m_SubRenderer.SetSortingLayer(LayerID_Arrow, GetSortingOrder());
		}


		private void Update_Gizmos (Beatmap.Note noteData, bool selecting, int noteIndex) {

			bool active = !MusicPlaying && !(noteData is null) && noteData.Active && MusicTime < noteData.Time + noteData.Duration;

			// ID
			if (Label != null) {
				if (ShowIndexLabel && active) {
					Label.gameObject.SetActive(true);
					Label.text = noteIndex.ToString();
					Label.transform.localRotation = MainRenderer.transform.localRotation;
				} else {
					Label.gameObject.SetActive(false);
				}
			}

			// Highlight
			if (Highlight != null) {
				Highlight.enabled = active && selecting;
			}



		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => CacheDirtyID++;


		public override void SetSkinData (SkinData skin) {
			base.SetSkinData(skin);
			m_SubRenderer.SkinData = skin;
			m_ShadowRenderer.SkinData = skin;
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
			// Shadow
			ShadowDistance = skin.NoteShadowDistance;
		}


		protected override void RefreshRendererZone () {
			base.RefreshRendererZone();
			RefreshRendererZoneFor(m_SubRenderer);
			RefreshRendererZoneFor(m_ShadowRenderer);
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