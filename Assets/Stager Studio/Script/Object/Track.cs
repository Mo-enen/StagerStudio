namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Track : StageObject {



		#region --- VAR ---


		// API
		public static int LayerID_Track { get; set; } = -1;
		public static int LayerID_Tray { get; set; } = -1;

		// Ser
		[SerializeField] StageRenderer m_TrayRenderer = null;


		#endregion




		#region --- MSG ---


		protected override void Awake () {
			base.Awake();
			MainRenderer.Pivot = new Vector3(0.5f, 0f);
			m_TrayRenderer.Pivot = new Vector3(0.5f, 0f);
			ColRot = null;
		}


		private void Update () {

			MainRenderer.RendererEnable = false;
			ColSize = null;

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
			bool active = Stage.GetStageActive(linkedStage, trackData.StageIndex) || !GetTrackActive(trackData);
			trackData.Active = active;

			Update_Gizmos(trackData.Active, trackIndex);

			if (!active) { return; }
			trackData.Selecting = oldSelecting;

			// Movement
			Update_Movement(linkedStage, trackData);

		}


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track trackData) {

			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			float trackWidth = GetTrackWidth(trackData);
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			var stagePos = Stage.GetStagePosition(linkedStage, trackData.StageIndex);
			var (pos, _, rotZ) = Stage.Inside(GetTrackX(trackData), 0f, stagePos, stageWidth, stageHeight, stageRotZ);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			transform.localRotation = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(Stage.GetStageAngle(linkedStage), 0, 0);
			ColSize = MainRenderer.transform.localScale = new Vector3(
				zoneSize * trackWidth * stageWidth,
				zoneSize * stageHeight,
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = true;
			m_TrayRenderer.RendererEnable = trackData.HasTray;
			MainRenderer.Type = SkinType.Track;
			MainRenderer.LifeTime = m_TrayRenderer.LifeTime = MusicTime - Time + TRANSATION_DURATION;
			MainRenderer.Scale = new Vector2(stageWidth * trackWidth, stageHeight);
			MainRenderer.Tint = GetTrackColor(trackData);
			MainRenderer.Alpha = m_TrayRenderer.Alpha = Stage.GetStageAlpha(linkedStage) * GetTrackAlpha(trackData);
			MainRenderer.SetSortingLayer(LayerID_Track, GetSortingOrder());
			m_TrayRenderer.SetSortingLayer(LayerID_Tray, GetSortingOrder());

		}


		private void Update_Gizmos (bool trackActive, int trackIndex) {
			// ID
			bool active = ShowIndexLabel && !MusicPlaying && trackActive;
			Label.gameObject.SetActive(active);
			if (active) {
				Label.text = trackIndex.ToString();
			}

			// Selection Highlight




		}


		#endregion




		#region --- API ---


		public static bool GetTrackActive (Beatmap.Track data) => MusicTime > data.Time - TRANSATION_DURATION && MusicTime < data.Time + data.Duration + TRANSATION_DURATION;


		public static float GetTrackWidth (Beatmap.Track data) => Mathf.Clamp(data.Width + Evaluate(data.Widths, MusicTime - data.Time), 0f, 2f);


		public static float GetTrackX (Beatmap.Track data) => Mathf.Repeat(data.X + Evaluate(data.Xs, MusicTime - data.Time), 1f);


		public static Color GetTrackColor (Beatmap.Track data) => PaletteColor(data.Color) * EvaluateColor(data.Colors, MusicTime - data.Time);


		public static float GetTrackAlpha (Beatmap.Track data) => Mathf.Clamp01(MusicTime < data.Time ? (MusicTime - data.Time + TRANSATION_DURATION) / TRANSATION_DURATION : MusicTime > data.Time + data.Duration ? (data.Time + data.Duration - MusicTime + TRANSATION_DURATION) / TRANSATION_DURATION : 1f);


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
		}


		#endregion




	}
}