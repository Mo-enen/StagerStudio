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
		public static int LayerID_Note { get; set; } = -1;
		public static int LayerID_Note_Hold { get; set; } = -1;
		public static int SortingLayerID_Note { get; set; } = -1;
		public static int SortingLayerID_Note_Hold { get; set; } = -1;
		public static int SortingLayerID_Pole { get; set; } = -1;
		public static int SortingLayerID_Arrow { get; set; } = -1;

		// Ser
		[SerializeField] private ObjectRenderer m_ArrowRenderer = null;
		[SerializeField] private ObjectRenderer m_PoleRenderer = null;
		[SerializeField] private SpriteRenderer m_ZLineRenderer = null;

		// Data
		private static byte CacheDirtyID = 1;
		private byte LocalCacheDirtyID = 0;
		private bool PrevClicked = false;
		private bool PrevClickedAlt = false;
		private Beatmap.Note Late_Note = null;
		private Beatmap.Note Late_LinkedNote = null;
		private Vector3 Late_NoteWorldPos = default;


		#endregion




		#region --- MSG ---


		private void Update () {

			Late_Note = null;
			Late_LinkedNote = null;
			MainRenderer.RendererEnable = false;
			m_ArrowRenderer.RendererEnable = false;
			m_PoleRenderer.RendererEnable = false;
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
			if (string.IsNullOrEmpty(noteData.Comment)) {
				// Main Click Sound
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
			Late_Note = noteData;
			Late_LinkedNote = linkedNote;

			Update_Movement(linkedStage, linkedTrack, noteData, linkedNote);

		}


		protected override void LateUpdate () {
			if (Late_Note is null || Beatmap is null) {
				base.LateUpdate();
				return;
			}

			// Sub Update
			var (_, _, zoneSize, _) = ZoneMinMax;
			if (Late_LinkedNote != null) {
				LateUpdate_Movement_Linked(Late_Note, Late_LinkedNote, Late_NoteWorldPos, ColRot ?? Quaternion.identity, MainRenderer.Alpha);
			}
			if ((Late_Note.SwipeX != 1 || Late_Note.SwipeY != 1) && GetNoteActive(Late_Note, null, Late_Note.AppearTime)) {
				LateUpdate_Movement_Swipe(Late_Note, zoneSize, ColRot ?? Quaternion.identity, MainRenderer.Alpha, MainRenderer.Type);
			}

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
			bool activeSelf = GetNoteActive(noteData, null, noteData.AppearTime);
			float alpha = Stage.GetStageAlpha(linkedStage) * Track.GetTrackAlpha(linkedTrack) * Mathf.Clamp01(16f - noteY01 * 16f);
			var type = !string.IsNullOrEmpty(noteData.Comment) ? SkinType.Comment : !noteData.Tap ? SkinType.SlideNote : noteData.Duration > DURATION_GAP ? SkinType.HoldNote : SkinType.TapNote;
			bool highlighing = (type == SkinType.HoldNote || type == SkinType.Comment || type == SkinType.SlideNote) && MusicTime > Time && MusicTime < Time + Duration;

			// Movement
			var (noteZonePos, rotX, rotZ) = Track.Inside(
				noteData.X, noteY01,
				stagePos, stageWidth, stageHeight, stageRotZ,
				trackX, trackWidth, trackAngle
			);
			var noteRot = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0f, 0f);
			var noteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, noteZonePos.x, noteZonePos.y);
			noteWorldPos.z += noteZonePos.z * zoneSize;
			if (Mathf.Abs(noteData.Z) > 0.0005f) {
				noteWorldPos += noteData.Z * zoneSize * (noteRot * Vector3.back);
			}
			var noteSize = GetRectSize(type == SkinType.HoldNote ? SkinType.TapNote : type);
			float noteScaleX = noteSize.x < 0f ? stageWidth * trackWidth * noteData.Width : noteSize.x;
			float noteScaleY = Mathf.Max(noteSizeY * stageHeight, noteSize.y);
			var zoneNoteScale = new Vector3(
				zoneSize * noteScaleX,
				zoneSize * noteScaleY,
				1f
			);
			transform.position = Late_NoteWorldPos = noteWorldPos;
			ColRot = MainRenderer.transform.rotation = noteRot;
			ColSize = MainRenderer.transform.localScale = zoneNoteScale;

			// Renderer
			MainRenderer.RendererEnable = !isLink || activeSelf;
			MainRenderer.Tint = highlighing ? HighlightTints[(int)type] : WHITE_32;
			MainRenderer.Alpha = alpha;
			MainRenderer.Duration = Duration;
			MainRenderer.LifeTime = MusicTime - Time;
			MainRenderer.Scale = new Vector2(noteScaleX, noteScaleY);
			MainRenderer.Type = type;
			MainRenderer.SetSortingLayer(Duration <= DURATION_GAP ? SortingLayerID_Note : SortingLayerID_Note_Hold, GetSortingOrder());

		}


		private void Update_Gizmos (Beatmap.Note noteData, bool selecting, int noteIndex) {

			bool active = !(noteData is null) && noteData.Active && MusicTime < noteData.Time + noteData.Duration;

			// ID
			if (Label != null) {
				if (ShowIndexLabel && !MusicPlaying && active) {
					bool hasComment = !string.IsNullOrEmpty(noteData.Comment);
					Label.gameObject.SetActive(true);
					Label.Text = hasComment ? $"{noteIndex} /{noteData.Comment.ToLower()}" : noteIndex.ToString();
					Label.transform.localRotation = MainRenderer.transform.localRotation;
				} else {
					Label.gameObject.SetActive(false);
				}
			}

			// Highlight
			if (Highlight != null) {
				Highlight.enabled = !MusicPlaying && active && selecting;
			}

			// Col
			TrySetColliderLayer(Duration <= DURATION_GAP ? LayerID_Note : LayerID_Note_Hold);

			// ZLine
			bool zlineActive = ShowGrid && active && noteData.Z > 0.0005f;
			if (m_ZLineRenderer.gameObject.activeSelf != zlineActive) {
				m_ZLineRenderer.gameObject.SetActive(zlineActive);
			}
			// ZLine Color
			if (zlineActive) {
				m_ZLineRenderer.color = Color.Lerp(
					m_ZLineRenderer.color,
					Color.black * 0.2f,
					UnityEngine.Time.deltaTime * 1.4f
				);
			} else {
				if (m_ZLineRenderer.color != Color.clear) {
					m_ZLineRenderer.color = Color.clear;
				}
			}
			// ZLine Scale
			if (zlineActive) {
				float zlineScaleY = (noteData.Z * ZoneMinMax.size * (MainRenderer.transform.rotation * Vector3.back)).magnitude;
				var zScale = m_ZLineRenderer.transform.localScale;
				if (Mathf.Abs(zScale.y - zlineScaleY) > 0.0001f) {
					zScale.y = zlineScaleY;
					m_ZLineRenderer.transform.localScale = zScale;
				}
				// ZLine Rot
				m_ZLineRenderer.transform.localRotation = MainRenderer.transform.localRotation * Quaternion.Euler(-90f, 0f, 0f);
			}

		}


		private void LateUpdate_Movement_Linked (Beatmap.Note noteData, Beatmap.Note linkedNote, Vector3 noteWorldPos, Quaternion noteRot, float alpha) {

			// Get Basic Linked Data
			if (linkedNote == null || noteData.Time + noteData.Duration >= linkedNote.Time + 0.001f) { return; }
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
			if (Mathf.Abs(linkedNote.Z) > 0.0005f) {
				var linkedRot = Quaternion.Euler(0f, 0f, linkedStageRotZ) * Quaternion.Euler(linkedTrackAngle, 0f, 0f);
				linkedNoteWorldPos += linkedNote.Z * zoneSize * (linkedRot * Vector3.back);
			}

			// Sub Pole World Pos
			float noteEndTime = noteData.Time + noteData.Duration;
			var subWorldPos = m_PoleRenderer.transform.position = Mathf.Abs(linkedNote.Time - noteEndTime) > 0.01f ? Util.Remap(
				noteEndTime,
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
			) : noteWorldPos;

			// Get Scale Y from World Distance
			float scaleY = Vector3.Distance(subWorldPos, linkedNoteWorldPos);

			// Reduce Scale Y when Linked Note Not Appear Yet
			if (MusicTime < linkedNote.AppearTime) {
				scaleY -= scaleY * (linkedNoteY01 - 1f) / (linkedNoteY01 - noteData.NoteDropEnd + gameOffset);
			}

			// Final
			var poleType = string.IsNullOrEmpty(noteData.Comment) ? SkinType.LinkPole : SkinType.Pixel;
			var poleSize = GetRectSize(poleType, false, false);
			m_PoleRenderer.transform.rotation = linkedNoteWorldPos != subWorldPos ? Quaternion.LookRotation(
				linkedNoteWorldPos - subWorldPos, Vector3.back
			) * Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
			m_PoleRenderer.transform.localScale = new Vector3(zoneSize * poleSize.x, scaleY, 1f);
			m_PoleRenderer.RendererEnable = true;
			m_PoleRenderer.Type = poleType;
			m_PoleRenderer.Duration = Duration;
			m_PoleRenderer.LifeTime = MusicTime - Time;
			m_PoleRenderer.Pivot = new Vector3(0.5f, 0f);
			m_PoleRenderer.Scale = new Vector2(poleSize.x, scaleY / zoneSize);
			m_PoleRenderer.Tint = MusicTime > Time + Duration ? HighlightTints[(int)poleType] : WHITE_32;
			m_PoleRenderer.Alpha = alpha * Mathf.Clamp01((linkedNote.NoteDropStart - gameOffset) * 16f);
			m_PoleRenderer.SetSortingLayer(SortingLayerID_Pole, GetSortingOrder());
		}


		private void LateUpdate_Movement_Swipe (Beatmap.Note noteData, float zoneSize, Quaternion rot, float alpha, SkinType type) {
			var noteSize = GetRectSize(type);
			var arrowSize = GetRectSize(SkinType.SwipeArrow, false, false);
			float arrowZ = noteData.SwipeX == 1 ? (180f - 90f * noteData.SwipeY) : Mathf.Sign(-noteData.SwipeX) * (135f - 45f * noteData.SwipeY);
			m_ArrowRenderer.transform.localPosition = rot * new Vector3(0f, zoneSize * noteSize.y * 0.5f, -noteSize.z);
			m_ArrowRenderer.transform.rotation = rot * Quaternion.Euler(0f, 0f, arrowZ);
			m_ArrowRenderer.transform.localScale = new Vector3(zoneSize * arrowSize.x, zoneSize * arrowSize.y, 1f);
			m_ArrowRenderer.RendererEnable = true;
			m_ArrowRenderer.Type = SkinType.SwipeArrow;
			m_ArrowRenderer.Pivot = new Vector3(0.5f, 0.5f);
			m_ArrowRenderer.Scale = arrowSize;
			m_ArrowRenderer.Alpha = alpha;
			m_ArrowRenderer.Duration = Duration;
			m_ArrowRenderer.LifeTime = MusicTime - Time;
			m_ArrowRenderer.SetSortingLayer(SortingLayerID_Arrow, GetSortingOrder());
		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => CacheDirtyID++;


		public override void SetSkinData (SkinData skin) {
			base.SetSkinData(skin);
			m_ArrowRenderer.SkinData = skin;
			m_PoleRenderer.SkinData = skin;
		}


		protected override void RefreshRendererZone () {
			base.RefreshRendererZone();
			RefreshRendererZoneFor(m_ArrowRenderer);
			RefreshRendererZoneFor(m_PoleRenderer);
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