﻿namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;


	public class Track : StageObject {



		#region --- VAR ---


		// API
		public static int LayerID_Track { get; set; } = -1;
		public static int LayerID_TrackTint { get; set; } = -1;
		public static int LayerID_Tray { get; set; } = -1;

		// Ser
		[SerializeField] ObjectRenderer m_TrackTintRenderer = null;
		[SerializeField] ObjectRenderer m_TrayRenderer = null;

		// Data
		private static Vector2 TraySize = default;
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
			if (trackData is null) { return; }

			bool oldSelecting = trackData.Selecting;
			trackData.Active = false;
			trackData.Selecting = false;
			Time = trackData.Time;
			Duration = trackData.Duration;

			// Get/Check Track/Stage
			var linkedStage = Beatmap.GetStageAt(trackData.StageIndex);
			bool active = Stage.GetStageActive(linkedStage, trackData.StageIndex) && GetTrackActive(trackData);
			trackData.Active = active;

			Update_Gizmos(trackData.Active, oldSelecting, trackIndex);

			if (!active) { return; }
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
			float rotX = Stage.GetStageAngle(linkedStage);
			float trackX = GetTrackX(trackData);
			var (pos, _, rotZ) = Stage.Inside(trackX, 0f, stagePos, stageWidth, stageHeight, stageRotZ);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			transform.localRotation = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(rotX, 0, 0);
			ColSize = MainRenderer.transform.localScale = m_TrackTintRenderer.transform.localScale = new Vector3(
				zoneSize * trackWidth * stageWidth,
				zoneSize * stageHeight,
				1f
			);

			// Tray
			if (trackData.HasTray) {
				var (trayPos, _, _) = Inside(TrayX, Stage.JudgeLineHeight / 2f / stageHeight, stagePos, stageWidth, stageHeight, stageRotZ, trackX, trackWidth, rotX);
				m_TrayRenderer.transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, trayPos.x, trayPos.y);
				m_TrayRenderer.transform.localScale = TraySize;
				m_TrayRenderer.Scale = TraySize;
			}

			// Renderer
			MainRenderer.RendererEnable = true;
			m_TrackTintRenderer.RendererEnable = true;
			m_TrayRenderer.RendererEnable = trackData.HasTray;
			MainRenderer.LifeTime = m_TrayRenderer.LifeTime = m_TrackTintRenderer.LifeTime = MusicTime - Time;
			MainRenderer.Scale = m_TrackTintRenderer.Scale = new Vector2(stageWidth * trackWidth, stageHeight);
			m_TrackTintRenderer.Tint = GetTrackColor(trackData);
			MainRenderer.Alpha = m_TrayRenderer.Alpha = Stage.GetStageAlpha(linkedStage) * GetTrackAlpha(trackData);
			m_TrackTintRenderer.Alpha *= MainRenderer.Alpha;
			MainRenderer.SetSortingLayer(LayerID_Track, GetSortingOrder());
			m_TrackTintRenderer.SetSortingLayer(LayerID_TrackTint, GetSortingOrder());
			m_TrayRenderer.SetSortingLayer(LayerID_Tray, GetSortingOrder());
		}


		private void Update_Gizmos (bool trackActive, bool selecting, int trackIndex) {

			// Label
			if (Label != null) {
				bool active = ShowIndexLabel && !MusicPlaying && trackActive;
				Label.gameObject.SetActive(active);
				if (active) {
					Label.text = trackIndex.ToString();
				}
			}

			// Highlight
			if (Highlight != null) {
				Highlight.enabled = !MusicPlaying && trackActive && selecting;
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


		public static float GetTrackWidth (Beatmap.Track data) => Mathf.Clamp(data.Width * Evaluate(data.Widths, MusicTime - data.Time, 1f), 0f, 2f);


		public static float GetTrackX (Beatmap.Track data) => Mathf.Clamp01(data.X + Evaluate(data.Xs, MusicTime - data.Time));


		public static Color GetTrackColor (Beatmap.Track data) => PaletteColor(data.Color) * EvaluateColor(data.Colors, MusicTime - data.Time);


		public static float GetTrackAlpha (Beatmap.Track data) => Mathf.Clamp01(
			VanishDuration < DURATION_GAP ? 1f : (data.Time + data.Duration - MusicTime) / VanishDuration
		);


		public static (Vector3 pos, float rotX, float rotZ) Inside (float x01, float y01, Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ, float trackX, float trackWidth, float trackRotX) {
			float halfTrackWidth = trackWidth * 0.5f;
			var (pos, pivot, rotZ) = Stage.Inside(
				Mathf.LerpUnclamped(trackX - halfTrackWidth, trackX + halfTrackWidth, x01), y01,
				stagePos, stageWidth, stageHeight, stageRotZ
			);
			return (
				(Quaternion.AngleAxis(trackRotX, Quaternion.Euler(0, 0, rotZ) * Vector3.right) * (pos - pivot)) + (Vector3)pivot,
				trackRotX, rotZ
			);
		}


		public override void SetSkinData (SkinData skin) {
			base.SetSkinData(skin);
			m_TrayRenderer.SkinData = skin;
			m_TrackTintRenderer.SkinData = skin;
		}


		public static void SetTrackSkinData (SkinData skin) {
			TraySize = skin.TryGetItemSize((int)SkinType.Tray) / skin.ScaleMuti;
		}


		protected override void RefreshRendererZone () {
			base.RefreshRendererZone();
			RefreshRendererZoneFor(m_TrackTintRenderer);
			RefreshRendererZoneFor(m_TrayRenderer);
		}



		#endregion




	}
}