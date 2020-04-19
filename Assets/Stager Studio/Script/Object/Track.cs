﻿namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;


	public class Track : StageObject {



		#region --- VAR ---


		// API
		public static int SortingLayerID_Track { get; set; } = -1;
		public static int SortingLayerID_TrackTint { get; set; } = -1;
		public static int SortingLayerID_Tray { get; set; } = -1;
		public static int BeatPerSection { get; set; } = 1;

		// Ser
		[SerializeField] ObjectRenderer m_TrackTintRenderer = null;
		[SerializeField] ObjectRenderer m_TrayRenderer = null;
		[SerializeField] TrackSectionRenderer m_SectionRenderer = null;

		// Data
		private Beatmap.Track LateTrackData = null;
		private float AimTrayX = 0.5f;
		private float TrayX = 0.5f;


		#endregion




		#region --- MSG ---


		protected override void Awake () {
			base.Awake();
			ColRot = null;
			MainRenderer.Type = SkinType.Track;
			MainRenderer.Tint = Color.white;
			m_TrackTintRenderer.Type = SkinType.TrackTint;
			m_TrayRenderer.Type = SkinType.Tray;
			m_TrayRenderer.SkinData = Skin;
			m_TrackTintRenderer.SkinData = Skin;
		}


		private void OnEnable () {
			Update();
			LateUpdate();
		}


		private void Update () {

			MainRenderer.RendererEnable = false;
			m_TrackTintRenderer.RendererEnable = false;
			m_TrayRenderer.RendererEnable = false;
			ColSize = null;
			LateTrackData = null;

			// Get TrackData
			int trackIndex = transform.GetSiblingIndex();
			var trackData = !(Beatmap is null) && trackIndex < Beatmap.Tracks.Count ? Beatmap.Tracks[trackIndex] : null;
			if (trackData is null) {
				Update_Gizmos(false, false, trackIndex, 1f);
				gameObject.SetActive(false);
				return;
			}

			bool oldSelecting = trackData.Selecting;
			trackData.Active = false;
			if (GetDeselectWhenInactive()) {
				trackData.Selecting = false;
			}
			Time = trackData.Time;
			Duration = trackData.Duration;

			// Get/Check Track/Stage
			var linkedStage = Beatmap.Stages[trackData.StageIndex];
			bool active = Stage.GetStageActive(linkedStage, trackData.StageIndex) && GetTrackActive(trackData);
			trackData.Active = active;
			trackData.SpeedMuti = linkedStage.SpeedMuti;

			Update_Gizmos(trackData.Active, oldSelecting, trackIndex, GameSpeedMuti * Stage.GetStageSpeed(linkedStage));

			if (!active) {
				gameObject.SetActive(false);
				return;
			}
			trackData.Selecting = oldSelecting;

			LateTrackData = trackData;

			// Movement
			Update_Movement(linkedStage, trackData);

		}


		protected override void LateUpdate () {
			base.LateUpdate();
			if (LateTrackData != null) {
				LateUpdate_Tray(LateTrackData);
			}
		}


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track trackData) {

			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			float trackWidth = GetTrackWidth(trackData);
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			var stagePos = Stage.GetStagePosition(linkedStage, trackData.StageIndex);
			float rotX = GetTrackAngle(trackData);
			float trackX = GetTrackX(trackData);
			var pos = Stage.LocalToZone(trackX, 0f, 0f, stagePos, stageWidth, stageHeight, stageRotZ);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			transform.localRotation = Quaternion.Euler(0f, 0f, stageRotZ) * Quaternion.Euler(rotX, 0, 0);
			ColSize = MainRenderer.transform.localScale = m_TrackTintRenderer.transform.localScale = new Vector3(
				zoneSize * trackWidth * stageWidth,
				zoneSize * stageHeight,
				1f
			);

			// Tray
			if (trackData.HasTray) {
				var traySize = GetRectSize(SkinType.Tray, false, false);
				var judgeLineSize = GetRectSize(SkinType.JudgeLine);
				var trayPos = LocalToZone(
					TrayX, judgeLineSize.y / 2f / stageHeight, 0f,
					stagePos, stageWidth, stageHeight, stageRotZ,
					trackX, trackWidth, rotX
				);
				m_TrayRenderer.transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, trayPos.x, trayPos.y);
				m_TrayRenderer.transform.localScale = new Vector3(traySize.x, traySize.y, 1f);
				m_TrayRenderer.Scale = traySize;
			}

			// Renderer
			MainRenderer.RendererEnable = true;
			m_TrackTintRenderer.RendererEnable = true;
			m_TrayRenderer.RendererEnable = trackData.HasTray;
			MainRenderer.Duration = m_TrayRenderer.Duration = m_TrackTintRenderer.Duration = Duration;
			MainRenderer.LifeTime = m_TrayRenderer.LifeTime = m_TrackTintRenderer.LifeTime = MusicTime - Time;
			MainRenderer.Scale = m_TrackTintRenderer.Scale = new Vector2(stageWidth * trackWidth, stageHeight);
			m_TrackTintRenderer.Tint = trackData.Tint = GetTrackColor(trackData);
			MainRenderer.Alpha = m_TrayRenderer.Alpha = Stage.GetStageAlpha(linkedStage) * GetTrackAlpha(trackData);
			m_TrackTintRenderer.Alpha *= MainRenderer.Alpha;
			MainRenderer.SetSortingLayer(SortingLayerID_Track, GetSortingOrder());
			m_TrackTintRenderer.SetSortingLayer(SortingLayerID_TrackTint, GetSortingOrder());
			m_TrayRenderer.SetSortingLayer(SortingLayerID_Tray, GetSortingOrder());
		}


		private void Update_Gizmos (bool trackActive, bool selecting, int trackIndex, float speed) {

			// Label
			bool active = ShowIndexLabel && !MusicPlaying && trackActive;
			Label.gameObject.SetActive(active);
			if (active) {
				Label.Text = trackIndex.ToString();
			}

			// Highlight
			bool highlighting = !MusicPlaying && trackActive && selecting;
			if ((Highlight != null) != highlighting) {
				if (highlighting) {
					InstantiateHighlight();
				} else {
					Destroy(Highlight.gameObject);
				}
			}

			// Section
			active = ShowGrid && trackActive && !MusicPlaying;
			m_SectionRenderer.RendererEnable = active;
			if (active) {
				m_SectionRenderer.MusicTime = MusicTime;
				m_SectionRenderer.SpeedMuti = speed;
				m_SectionRenderer.TimeGap = 60f / Beatmap.BPM;
				m_SectionRenderer.TimeOffset = Beatmap.Shift;
				m_SectionRenderer.BeatPerSection = BeatPerSection;
				m_SectionRenderer.Scale = MainRenderer.Scale;
				m_SectionRenderer.SetSortingLayer(SortingLayerID_Gizmos, GetSortingOrder());
			}

		}


		private void LateUpdate_Tray (Beatmap.Track trackData) {
			if (trackData.HasTray) {
				if (AimTrayX < trackData.TrayX.min) {
					AimTrayX = Mathf.Lerp(trackData.TrayX.min, trackData.TrayX.max, trackData.TrayTime - MusicTime > 0.5f ? 0.5f : 0.3f);
				} else if (AimTrayX > trackData.TrayX.max) {
					AimTrayX = Mathf.Lerp(trackData.TrayX.min, trackData.TrayX.max, trackData.TrayTime - MusicTime > 0.5f ? 0.5f : 0.7f);
				}
				TrayX = Mathf.Lerp(TrayX, AimTrayX, UnityEngine.Time.deltaTime * 20f);
			}
			trackData.TrayTime = float.MaxValue;
		}


		#endregion




		#region --- API ---


		public static bool GetTrackActive (Beatmap.Track data) => MusicTime >= data.Time && MusicTime <= data.Time + data.Duration;


		public static float GetTrackWidth (Beatmap.Track data) => Mathf.Clamp(data.Width * Evaluate(data.Widths, MusicTime - data.Time, 1f), 0f, 128f);


		public static float GetTrackX (Beatmap.Track data) => data.X + Evaluate(data.Xs, MusicTime - data.Time);


		public static Color GetTrackColor (Beatmap.Track data) => EvaluateColor(data.Colors, MusicTime - data.Time, PaletteColor(data.Color));


		public static float GetTrackAlpha (Beatmap.Track data) => Mathf.Clamp01(
			VanishDuration < DURATION_GAP ? 1f : (data.Time + data.Duration - MusicTime) / VanishDuration
		);


		public static float GetTrackAngle (Beatmap.Track data) => Mathf.LerpAngle(
			data.Angle + Evaluate(data.Angles, MusicTime - data.Time),
			0f,
			Abreast.value
		);


		// Matrix
		public static Vector3 LocalToZone (
			float x01, float y01, float z01,
			Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ,
			float trackX, float trackWidth, float trackRotX
		) {
			var sPos = Matrix4x4.TRS(
				new Vector3(trackX, 0f, 0f),
				Quaternion.Euler(trackRotX, 0f, 0f),
				new Vector3(trackWidth, 1f, 1f)
			).MultiplyPoint(new Vector3(x01 - 0.5f, y01, -z01));
			return Stage.LocalToZone(sPos.x, sPos.y, sPos.z, stagePos, stageWidth, stageHeight, stageRotZ);
		}


		public static Vector3 ZoneToLocal (
			float zoneX, float zoneY, float zoneZ,
			Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ,
			float trackX, float trackWidth, float trackRotX
		) {
			var sPos01 = Stage.ZoneToLocal(
				zoneX, zoneY, zoneZ,
				stagePos, stageWidth, stageHeight, stageRotZ
			);
			var tPos01 = Matrix4x4.TRS(
				new Vector3(trackX, 0f, 0f),
				Quaternion.Euler(trackRotX, 0f, 0f),
				new Vector3(trackWidth, 1f, 1f)
			).inverse.MultiplyPoint3x4(sPos01);
			tPos01.x += 0.5f;
			return tPos01;
		}


		protected override void RefreshRendererZone () {
			base.RefreshRendererZone();
			RefreshRendererZoneFor(m_TrackTintRenderer);
			RefreshRendererZoneFor(m_TrayRenderer);
			RefreshRendererZoneFor(m_SectionRenderer);
		}



		#endregion




	}
}