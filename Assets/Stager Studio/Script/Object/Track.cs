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

		// Short
		private StageRenderer TrayRenderer => m_TrayRenderer;

		// Ser
		[SerializeField] StageRenderer m_TrayRenderer = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			MainRenderer.Pivot = TrayRenderer.Pivot = new Vector3(0.5f, 0f);

		}


		private void Update () {

			MainRenderer.RendererEnable = false;

			// Get TrackData
			int index = transform.GetSiblingIndex();
			var beatmap = GetBeatmap();
			var trackData = !(beatmap is null) && index < beatmap.Tracks.Count ? beatmap.Tracks[index] : null;
			if (trackData is null) { return; }

			Time = trackData.Time;
			Duration = trackData.Duration;

			// Get/Check Track/Stage
			var linkedStage = beatmap.GetStageAt(trackData.StageIndex);
			if (linkedStage is null || !Stage.GetStageActive(linkedStage) || !GetTrackActive(trackData)) { return; }

			// Movement
			Update_Movement(linkedStage, trackData);

		}


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track trackData) {

			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			float trackWidth = GetTrackWidth(trackData);
			float stageWidth = Stage.GetStageWidth(linkedStage);
			float stageHeight = Stage.GetStageHeight(linkedStage);
			float stageRotZ = Stage.GetStageWorldRotationZ(linkedStage);
			var stagePos = Stage.GetStagePosition(linkedStage);
			var (pos, _, rotZ) = Stage.Inside(GetTrackX(trackData), 0f, stagePos, stageWidth, stageHeight, stageRotZ);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
			MainRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ) * Quaternion.Euler(Stage.GetStageAngle(linkedStage), 0, 0);
			MainRenderer.transform.localScale = new Vector3(
				zoneSize * trackWidth * stageWidth,
				zoneSize * stageHeight,
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = true;
			TrayRenderer.RendererEnable = trackData.HasTray;
			MainRenderer.Type = SkinType.Track;
			MainRenderer.LifeTime = TrayRenderer.LifeTime = MusicTime - Time + TRANSATION_DURATION;
			MainRenderer.Scale = new Vector2(stageWidth * trackWidth, stageWidth);
			MainRenderer.Tint = GetTrackColor(trackData);
			MainRenderer.Alpha = TrayRenderer.Alpha = Stage.GetStageAlpha(linkedStage, TRANSATION_DURATION) * GetTrackAlpha(trackData, TRANSATION_DURATION);
			MainRenderer.SetSortingLayer(LayerID_Track, GetSortingOrder());
			TrayRenderer.SetSortingLayer(LayerID_Tray, GetSortingOrder());


		}


		#endregion




		#region --- API ---


		public static float GetTrackWidth (Beatmap.Track data) {
			return Mathf.Clamp(data.Width + Evaluate(data.Widths, MusicTime - data.Time), 0f, 2f);
		}


		public static bool GetTrackActive (Beatmap.Track data) => MusicTime > data.Time - TRANSATION_DURATION && MusicTime < data.Time + data.Duration + TRANSATION_DURATION;


		public static (Vector3 pos, float rotX, float rotZ) Inside (
			float x01, float y01,
			Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ,
			float trackX, float trackWidth, float trackRotX
		) {
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


		public static float GetTrackX (Beatmap.Track data) {
			return Mathf.Repeat(data.X + Evaluate(data.Xs, MusicTime - data.Time), 1f);
		}


		public static Color GetTrackColor (Beatmap.Track data) {
			if (data.Colors is null) {
				return PaletteColor(data.Color);
			} else {
				return PaletteColor(data.Color) * EvaluateColor(data.Colors, MusicTime - data.Time);
			}
		}


		public static float GetTrackAlpha (Beatmap.Track data, float transation) => Mathf.Clamp01(
			MusicTime < data.Time ? (MusicTime - data.Time + transation) / transation :
			MusicTime > data.Time + data.Duration ? (data.Time + data.Duration - MusicTime + transation) / transation :
			1f
		);


		public override void SetSkinData (SkinData skin) {
			base.SetSkinData(skin);
			m_TrayRenderer.SkinData = skin;
		}


		#endregion




	}
}