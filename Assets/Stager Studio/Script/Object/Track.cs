namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Track : StageObject {


		// Var
		private const float TRANSATION_DURATION = 0.264f;

		// Ser
		[SerializeField] StageRenderer m_TrayRenderer = null;



		// MSG
		private void Update () {

			MainRenderer.RendererEnable = false;

			// Get TrackData
			int index = transform.GetSiblingIndex();
			var beatmap = GetBeatmap();
			var trackData = !(beatmap is null) && index < beatmap.Tracks.Count ? beatmap.Tracks[index] : null;
			if (trackData is null) { return; }

			Time = trackData.Time;
			Duration = trackData.Duration;
			float musicTime = GetMusicTime();

			// Get/Check Track/Stage
			var linkedStage = beatmap.GetStageAt(trackData.StageIndex);
			if (linkedStage is null || !Stage.GetStageActive(linkedStage, musicTime) || !GetTrackActive(trackData, musicTime)) { return; }

			// Movement
			Update_Movement(linkedStage, trackData, musicTime);

		}


		private void Update_Movement (Beatmap.Stage linkedStage, Beatmap.Track trackData, float musicTime) {

			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			float trackWidth = GetTrackWidth(trackData, musicTime);
			float stageWidth = Stage.GetStageWidth(linkedStage, musicTime);
			float stageHeight = Stage.GetStageHeight(linkedStage, musicTime);
			float disc = Stage.GetStageDisc(linkedStage, musicTime);

			// Movement
			transform.position = GetTrackWorldPosition(linkedStage, trackData, musicTime);
			transform.localRotation = GetTrackWorldRotation(linkedStage, trackData, musicTime);
			transform.localScale = disc < DISC_GAP ?
				new Vector3(
					zoneSize * trackWidth * stageWidth,
					zoneSize * stageHeight,
					1f
				) : new Vector3(
					zoneSize * stageWidth,
					zoneSize * stageWidth,
					1f
				);

			// Renderer
			MainRenderer.RendererEnable = true;
			m_TrayRenderer.RendererEnable = trackData.HasTray;
			MainRenderer.Type = SkinType.Track;
			MainRenderer.LifeTime = m_TrayRenderer.LifeTime = musicTime - Time + TRANSATION_DURATION;
			MainRenderer.Scale = new Vector2(stageWidth * trackWidth, stageWidth);
			MainRenderer.Tint = GetTrackColor(trackData, musicTime);
			MainRenderer.Alpha = m_TrayRenderer.Alpha = Stage.GetStageAlpha(linkedStage, musicTime, TRANSATION_DURATION) * GetTrackAlpha(trackData, musicTime, TRANSATION_DURATION);
			MainRenderer.Disc = disc;
			MainRenderer.DiscWidth = trackWidth;
			MainRenderer.DiscHeight = stageHeight / Mathf.Max(stageWidth, 0.0000001f);
			MainRenderer.Pivot = m_TrayRenderer.Pivot = new Vector3(0.5f, 0f);

		}


		// API
		public static Vector3 GetTrackWorldPosition (Beatmap.Stage stageData, Beatmap.Track trackData, float musicTime) {
			var (zoneMin, zoneMax, _) = GetZoneMinMax();
			var (pos, _, _) = Stage.Inside(stageData, musicTime, GetTrackX(trackData, musicTime), 0f);
			return Util.Vector3Lerp3(zoneMin, zoneMax, pos.x, pos.y);
		}


		public static Quaternion GetTrackWorldRotation (Beatmap.Stage stageData, Beatmap.Track trackData, float musicTime) {
			var (_, _, rotZ) = Stage.Inside(stageData, musicTime, GetTrackX(trackData, musicTime), 0f);
			return Quaternion.Euler(0f, 0f, rotZ) *
				Quaternion.Euler(GetTrackRotation(trackData, musicTime), 0, 0);
		}


		public static float GetTrackWidth (Beatmap.Track data, float musicTime) {
			return Mathf.Clamp(data.Width + Evaluate(data.Widths, musicTime - data.Time), 0f, 2f);
		}


		public static bool GetTrackActive (Beatmap.Track data, float musicTime) => musicTime > data.Time - TRANSATION_DURATION && musicTime < data.Time + data.Duration + TRANSATION_DURATION;


		public static (Vector3 pos, float rotX, float rotZ) Inside (Beatmap.Stage stage, Beatmap.Track track, float musicTime, float x01, float y01) {
			float trackX = GetTrackX(track, musicTime);
			float halfTrackWidth = GetTrackWidth(track, musicTime) * 0.5f;
			float trackRotX = GetTrackRotation(track, musicTime);
			var (pos, pivot, rotZ) = Stage.Inside(
				stage, musicTime,
				Mathf.LerpUnclamped(trackX - halfTrackWidth, trackX + halfTrackWidth, x01), y01
			);
			return (
				(Quaternion.AngleAxis(trackRotX, Quaternion.Euler(0, 0, rotZ) * Vector3.right) * (pos - pivot)) + (Vector3)pivot,
				trackRotX, rotZ
			);
		}


		// LGC
		private static Color GetTrackColor (Beatmap.Track data, float musicTime) {
			if (data.Colors is null) {
				return PaletteColor(data.Color);
			} else {
				return PaletteColor(data.Color) * EvaluateColor(data.Colors, musicTime - data.Time);
			}
		}


		private static float GetTrackAlpha (Beatmap.Track data, float musicTime, float transation) => Mathf.Clamp01(
			musicTime < data.Time ? (musicTime - data.Time + transation) / transation :
			musicTime > data.Time + data.Duration ? (data.Time + data.Duration - musicTime + transation) / transation :
			1f
		);


		private static float GetTrackRotation (Beatmap.Track data, float musicTime) {
			return data.Rotation + Evaluate(data.Rotations, musicTime - data.Time);
		}


		private static float GetTrackX (Beatmap.Track data, float musicTime) {
			return Mathf.Repeat(data.X + Evaluate(data.Xs, musicTime - data.Time), 1f);
		}


	}
}