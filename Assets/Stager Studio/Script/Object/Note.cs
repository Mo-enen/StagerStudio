namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;


	public class Note : StageObject {




		#region --- SUB ---


		public delegate float GameDropOffsetHandler (float muti);
		public delegate float DropSpeedHandler (float time);
		public delegate float DropOffsetHandler (float time, float muti);
		public delegate float FilledTimeHandler (float time, float fill, float muti);
		public delegate void VoidIntFloatHandler (int i, float f);
		public delegate void SfxHandler (byte type, int duration, int a, int b);


		#endregion




		#region --- VAR ---


		// Handler
		public static GameDropOffsetHandler GetGameDropOffset { get; set; } = null;
		public static DropSpeedHandler GetDropSpeedAt { get; set; } = null;
		public static DropOffsetHandler GetDropOffset { get; set; } = null;
		public static FilledTimeHandler GetFilledTime { get; set; } = null;
		public static VoidIntFloatHandler PlayClickSound { get; set; } = null;
		public static SfxHandler PlaySfx { get; set; } = null;

		// API
		public static int LayerID_Note { get; set; } = -1;
		public static int LayerID_Note_Hold { get; set; } = -1;
		public static int SortingLayerID_Note { get; set; } = -1;
		public static int SortingLayerID_Note_Hold { get; set; } = -1;
		public static int SortingLayerID_Pole_Front { get; set; } = -1;
		public static int SortingLayerID_Pole_Back { get; set; } = -1;
		public static Vector3 CameraWorldPos { get; set; } = default;

		// Ser
		[SerializeField] private ObjectRenderer m_PoleRenderer = null;
		[SerializeField] private SpriteRenderer m_ZLineRenderer = null;

		// Data
		private bool PrevClicked = false;
		private bool PrevClickedAlt = false;
		private Beatmap.Note Late_Note = null;
		private Beatmap.Note Late_LinkedNote = null;
		private Vector3 Late_NoteWorldPos = default;


		#endregion




		#region --- MSG ---


		protected override void Awake () {
			base.Awake();
			m_PoleRenderer.SkinData = Skin;
			MainRenderer.Type = SkinType.Note;
			m_PoleRenderer.Type = SkinType.Pole;
		}


		private void OnEnable () {
			Update();
			LateUpdate();
		}


		private void Update () {

			Late_Note = null;
			Late_LinkedNote = null;
			MainRenderer.RendererEnable = false;
			m_PoleRenderer.RendererEnable = false;
			ColSize = null;

			// Get NoteData
			int noteIndex = transform.GetSiblingIndex();
			var noteData = Beatmap?.Notes[noteIndex];
			if (noteData is null) {
				Update_Gizmos(null, null, noteIndex);
				gameObject.SetActive(false);
				return;
			}
			noteData.Active = false;

			// Get/Check Linked Track/Stage
			var linkedTrack = Beatmap.Tracks[noteData.TrackIndex];
			var linkedStage = Beatmap.Stages[linkedTrack.StageIndex];
			if (!linkedStage.Active || !linkedTrack.Active) {
				Update_Gizmos(null, null, noteIndex);
				gameObject.SetActive(false);
				return;
			}

			// Data
			Time = noteData.Time;
			Duration = noteData.Duration;

			// Linked
			var linkedNote = noteData.LinkedNoteIndex >= 0 && noteData.LinkedNoteIndex < Beatmap.Notes.Count ? Beatmap.Notes[noteData.LinkedNoteIndex] : null;

			// Sound
			Update_Sound(noteData, linkedNote);

			// Active
			bool active = GetNoteActive(noteData, linkedNote, noteData.AppearTime);
			noteData.Active = active;
			Update_Gizmos(linkedStage, noteData, noteIndex);

			if (!active) {
				gameObject.SetActive(false);
				return;
			}

			// Tray
			Update_Tray(linkedTrack, noteData);

			// Final
			Late_Note = noteData;
			Late_LinkedNote = linkedNote;

			Update_Movement(linkedStage, linkedTrack, noteData, linkedNote);


		}


		protected override void LateUpdate () {

			if (Late_Note == null) {
				base.LateUpdate();
				return;
			}

			// Link Update
			if (Late_LinkedNote != null) {
				LateUpdate_Movement_Linked(Late_Note, Late_LinkedNote, Late_NoteWorldPos, ColRot ?? Quaternion.identity, MainRenderer.Alpha);
			}

			base.LateUpdate();
		}


		public static void Update_Cache (Beatmap.Note noteData, float speedMuti) {
			if (noteData.LocalCacheDirtyID != Beatmap.Note.CacheDirtyID) {
				noteData.LocalCacheDirtyID = Beatmap.Note.CacheDirtyID;
				noteData.CacheTime = -1f;
			}
			if (speedMuti != noteData.SpeedMuti) {
				noteData.SpeedMuti = speedMuti;
				noteData.CacheTime = -1f;
			}
			if (noteData.CacheTime != noteData.Time) {
				noteData.CacheTime = noteData.Time;
				noteData.AppearTime = GetFilledTime(noteData.Time, -1f, speedMuti);
				noteData.NoteDropStart = -1f;
				noteData.SpeedOnDrop = GetDropSpeedAt(noteData.Time);
			}
			float duration = Mathf.Max(noteData.Duration, 0f);
			if (noteData.CacheDuration != duration) {
				noteData.CacheDuration = duration;
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
			float stagePivotY = Stage.GetStagePivotY(linkedStage);
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
			bool highlighing = MusicTime > Time && MusicTime < Time + Duration;
			float noteZ = GetNoteZ(noteData);
			var tint = highlighing ? HighlightTints[(int)SkinType.Note] : WHITE_32;
			if (TintNote) { tint *= linkedTrack.Tint; }

			// Movement
			var noteZonePos = Track.LocalToZone(
				noteData.X, noteY01, noteZ,
				stagePos, stageWidth, stageHeight, stagePivotY, stageRotZ,
				trackX, trackWidth, trackAngle
			);
			var noteRot = Quaternion.Euler(0f, 0f, stageRotZ) * Quaternion.Euler(trackAngle, 0f, 0f);
			var noteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, noteZonePos.x, noteZonePos.y, noteZonePos.z);

			// Size
			var noteSize = GetRectSize(SkinType.Note, noteData.ItemType);
			float noteScaleX = noteSize.x < 0f ? stageWidth * trackWidth * noteData.Width : noteSize.x;
			float noteScaleY = Mathf.Max(noteSizeY * stageHeight, Duration > DURATION_GAP ? 0.0001f : noteSize.y);
			var zoneNoteScale = new Vector3(
				zoneSize * noteScaleX,
				zoneSize * noteScaleY,
				1f
			);

			// Transform
			transform.position = Late_NoteWorldPos = noteWorldPos;
			ColRot = MainRenderer.transform.rotation = noteRot;
			ColSize = MainRenderer.transform.localScale = zoneNoteScale;

			// Renderer
			MainRenderer.RendererEnable = !isLink || activeSelf;
			MainRenderer.ItemType = noteData.ItemType;
			MainRenderer.Tint = tint;
			MainRenderer.Alpha = alpha;
			MainRenderer.Duration = Duration;
			MainRenderer.LifeTime = MusicTime - Time;
			MainRenderer.Scale = new Vector2(noteScaleX, noteScaleY);
			MainRenderer.SetSortingLayer(
				Duration <= DURATION_GAP ? SortingLayerID_Note : SortingLayerID_Note_Hold,
				GetSortingOrder()
			);

		}


		private void Update_Gizmos (Beatmap.Stage linkedStage, Beatmap.Note noteData, int noteIndex) {

			bool active = !(noteData is null) && noteData.Active && MusicTime < noteData.Time + noteData.Duration;

			// Label
			if (Label != null) {
				if (ShowIndexLabel && !MusicPlaying && active) {
					Label.gameObject.SetActive(true);
					Label.Text = noteData.SoundFxIndex <= 0 ? noteIndex.ToString() : (noteIndex.ToString() + " fx");
					Label.transform.localRotation = MainRenderer.transform.localRotation;
				} else {
					Label.gameObject.SetActive(false);
				}
			}

			// Col
			TrySetColliderLayer(Duration <= DURATION_GAP ? LayerID_Note : LayerID_Note_Hold);

			// ZLine
			bool zlineActive = ShowGrid && active && Abreast.value < 0.5f && noteData.Z > 0.0005f;
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
				float zlineScaleY = GetNoteZ(noteData) * Stage.GetStageHeight(linkedStage) * ZoneMinMax.size;
				var zScale = m_ZLineRenderer.transform.localScale;
				if (Mathf.Abs(zScale.y - zlineScaleY) > 0.0001f) {
					zScale.y = zlineScaleY;
					m_ZLineRenderer.transform.localScale = zScale;
				}
				// ZLine Rot
				m_ZLineRenderer.transform.localRotation = MainRenderer.transform.localRotation * Quaternion.Euler(-90f, 0f, 0f);
			}

		}


		private void Update_Sound (Beatmap.Note noteData, Beatmap.Note linkedNote) {

			if (noteData.SpeedOnDrop < 0f) { return; }

			// Start Trigger
			bool clicked = MusicTime > noteData.Time;
			if (MusicPlaying && clicked && !PrevClicked) {
				// Click 
				if (noteData.ClickSoundIndex >= 0) {
					PlayClickSound(noteData.ClickSoundIndex, 1f);
				}
				// Fx
				if (noteData.SoundFxIndex > 0) {
					PlaySfx(noteData.SoundFxIndex, linkedNote != null ? Mathf.Max(noteData.m_Duration, linkedNote.m_Time - noteData.m_Time) : noteData.m_Duration, noteData.SoundFxParamA, noteData.SoundFxParamB);
				}
			}
			PrevClicked = clicked;

			// End Trigger
			if (noteData.Duration > DURATION_GAP) {
				// End Click Sound
				bool altClicked = MusicTime > noteData.Time + noteData.Duration;
				if (MusicPlaying && altClicked && !PrevClickedAlt) {
					PlayClickSound(noteData.ClickSoundIndex, 0.618f);
				}
				PrevClickedAlt = altClicked;
			}

		}


		private void LateUpdate_Movement_Linked (Beatmap.Note noteData, Beatmap.Note linkedNote, Vector3 noteWorldPos, Quaternion noteRot, float alpha) {

			// Get Basic Linked Data
			if (linkedNote == null || noteData.Time + noteData.Duration >= linkedNote.Time + 0.001f) { return; }
			var linkedTrack = Beatmap.Tracks[linkedNote.TrackIndex];
			if (linkedTrack == null || !Track.GetTrackActive(linkedTrack)) { return; }
			var linkedStage = Beatmap.Stages[linkedTrack.StageIndex];
			if (linkedStage == null || !Stage.GetStageActive(linkedStage, linkedTrack.StageIndex)) { return; }

			// Get Linked Stage Data
			Vector2 linkedStagePos = Stage.GetStagePosition(linkedStage, linkedTrack.StageIndex);
			float linkedStageWidth = Stage.GetStageWidth(linkedStage);
			float linkedStageHeight = Stage.GetStageHeight(linkedStage);
			float linkedStagePivotY = Stage.GetStagePivotY(linkedStage);
			float linkedStageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			float linkedTrackAngle = Track.GetTrackAngle(linkedTrack);

			// Movement
			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			float gameOffset = GetGameDropOffset(noteData.SpeedMuti);
			float linkedNoteY01 = MusicTime < linkedNote.Time ? (linkedNote.NoteDropStart - gameOffset) : 0f;
			var linkedZonePos = Track.LocalToZone(
				linkedNote.X,
				linkedNoteY01,
				GetNoteZ(linkedNote),
				linkedStagePos,
				linkedStageWidth,
				linkedStageHeight,
				linkedStagePivotY,
				linkedStageRotZ,
				Track.GetTrackX(linkedTrack),
				Track.GetTrackWidth(linkedTrack),
				linkedTrackAngle
			);

			// Linked Note World Pos
			var linkedNoteWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, linkedZonePos.x, linkedZonePos.y, linkedZonePos.z);

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
				float offset = linkedNoteY01 - noteData.NoteDropEnd + gameOffset;
				scaleY = offset == 0f ? 0f : scaleY - scaleY * (linkedNoteY01 - 1f) / offset;
			}

			// Final
			var poleSize = GetRectSize(SkinType.Pole, noteData.ItemType, false, false);
			m_PoleRenderer.ItemType = noteData.ItemType;
			m_PoleRenderer.transform.rotation = linkedNoteWorldPos != subWorldPos ? Quaternion.LookRotation(
				linkedNoteWorldPos - subWorldPos, -MainRenderer.transform.forward
			) * Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
			m_PoleRenderer.transform.localScale = new Vector3(zoneSize * poleSize.x, scaleY, 1f);
			m_PoleRenderer.RendererEnable = true;
			m_PoleRenderer.Duration = Duration;
			m_PoleRenderer.LifeTime = MusicTime - Time;
			m_PoleRenderer.Pivot = new Vector3(0.5f, 0f);
			m_PoleRenderer.Scale = new Vector2(poleSize.x, scaleY / zoneSize);
			m_PoleRenderer.Tint = MusicTime > Time + Duration ? HighlightTints[(int)SkinType.Pole] : WHITE_32;
			m_PoleRenderer.Alpha = alpha * Mathf.Clamp01((linkedNote.NoteDropStart - gameOffset) * 16f);
			m_PoleRenderer.SetSortingLayer(
				FrontPole ? SortingLayerID_Pole_Front : SortingLayerID_Pole_Back,
				GetSortingOrder()
			);

		}


		#endregion




		#region --- API ---


		public static void SetCacheDirty () => Beatmap.Note.CacheDirtyID++;


		public static float GetNoteZ (Beatmap.Note data) => Mathf.LerpUnclamped(data.Z, 0f, Abreast.value);


		protected override void RefreshRendererZone () {
			base.RefreshRendererZone();
			RefreshRendererZoneFor(m_PoleRenderer);
		}


		#endregion




		#region --- LGC ---


		public static bool GetNoteActive (Beatmap.Note data, Beatmap.Note linkedNote, float appearTime) => data.SpeedOnDrop >= 0f && MusicTime >= appearTime && (MusicTime <= data.Time + data.Duration || (linkedNote != null && MusicTime <= linkedNote.Time));


		#endregion




	}
}