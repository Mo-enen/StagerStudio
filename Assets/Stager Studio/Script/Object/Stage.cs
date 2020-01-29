﻿namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Stage : StageObject {


		// Var
		private const float TRANSATION_DURATION = 0.22f;



		// MSG
		private void Update () {

			MainRenderer.RendererEnable = false;
			float musicTime = GetMusicTime();

			// Get StageData
			int index = transform.GetSiblingIndex();
			var beatmap = GetBeatmap();
			var stageData = !(beatmap is null) && index < beatmap.Stages.Count ? beatmap.Stages[index] : null;
			if (stageData is null) { return; }

			Time = stageData.Time;
			Duration = stageData.Duration;

			// Stage Active Check
			if (!GetStageActive(stageData, musicTime)) { return; }

			// Update
			Update_Movement(stageData);

		}


		private void Update_Movement (Beatmap.Stage stageData) {

			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			float musicTime = GetMusicTime();
			float disc = GetStageDisc(stageData, musicTime);
			float width = GetStageWidth(stageData, musicTime);
			float height = GetStageHeight(stageData, musicTime);
			bool normalDisc = disc < DISC_GAP;

			// Movement
			transform.position = GetStageWorldPosition(stageData, musicTime);
			transform.localRotation = Quaternion.Euler(0f, 0f, GetStageWorldRotationZ(stageData, musicTime));
			transform.localScale = new Vector3(
				zoneSize * width,
				zoneSize * (normalDisc ? height : width),
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = true;
			MainRenderer.Type = SkinType.Stage;
			MainRenderer.Scale = new Vector2(width, height);
			MainRenderer.LifeTime = musicTime - Time + TRANSATION_DURATION;
			MainRenderer.Alpha = GetStageAlpha(stageData, musicTime, TRANSATION_DURATION);
			MainRenderer.Disc = disc;
			MainRenderer.DiscWidth = 1f;
			MainRenderer.DiscHeight = height / Mathf.Max(width, 0.0000001f);
			MainRenderer.Tint = GetStageColor(stageData, musicTime);
			MainRenderer.Pivot = new Vector3(0.5f, normalDisc ? 0f : disc / 720f);

		}


		// API
		public static Vector3 GetStageWorldPosition (Beatmap.Stage data, float musicTime) {
			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			var stagePos = new Vector2(data.X, data.Y) + Evaluate(data.Positions, musicTime - data.Time);
			return Util.Vector3Lerp3(zoneMin, zoneMax, stagePos.x, stagePos.y);
		}


		public static float GetStageWorldRotationZ (Beatmap.Stage stageData, float musicTime) {
			return -Mathf.Repeat(
				stageData.Rotation + Evaluate(stageData.Rotations, musicTime - stageData.Time),
				360f
			);
		}


		public static float GetStageDisc (Beatmap.Stage data, float musicTime) {
			return Mathf.Clamp(data.Disc + Evaluate(data.Discs, musicTime - data.Time), 0f, 360f);
		}


		public static float GetStageWidth (Beatmap.Stage data, float musicTime) {
			return Mathf.Max(data.Width + Evaluate(data.Widths, musicTime - data.Time), 0f);
		}


		public static float GetStageHeight (Beatmap.Stage data, float musicTime) {
			return Mathf.Max(data.Height + Evaluate(data.Heights, musicTime - data.Time), 0f);
		}


		public static float GetStageAlpha (Beatmap.Stage data, float musicTime, float transation) => Mathf.Clamp01(
			musicTime < data.Time ? (musicTime - data.Time + transation) / transation :
			musicTime > data.Time + data.Duration ? (data.Time + data.Duration - musicTime + transation) / transation :
			1f
		);


		public static bool GetStageActive (Beatmap.Stage data, float musicTime) => musicTime > data.Time - TRANSATION_DURATION && musicTime < data.Time + data.Duration + TRANSATION_DURATION;


		public static (Vector2 pos, Vector2 zero, float rot) Inside (Beatmap.Stage stage, float musicTime, float x01, float y01) {
			Vector2 stagePos = GetStagePosition(stage, musicTime);
			float stageWidth = GetStageWidth(stage, musicTime);
			float stageHeight = GetStageHeight(stage, musicTime);
			float rotZ = GetStageWorldRotationZ(stage, musicTime);
			float disc = GetStageDisc(stage, musicTime);
			float halfWidth = stageWidth * 0.5f;
			if (disc < DISC_GAP) {
				x01 = Mathf.LerpUnclamped(stagePos.x - halfWidth, stagePos.x + halfWidth, x01);
				return (
					(Vector2)(Quaternion.Euler(0f, 0f, rotZ) * (
					new Vector2(
						x01,
						Mathf.LerpUnclamped(stagePos.y, stagePos.y + stageHeight, y01)
					) - stagePos)) + stagePos,
					(Vector2)(Quaternion.Euler(0f, 0f, rotZ) * (new Vector2(x01, stagePos.y) - stagePos)) + stagePos,
					rotZ
				);
			} else {
				float scope = stageWidth > 0.0001f ? stageHeight / stageWidth : 0f;
				Vector2 newZero = Util.GetDisc01(new Vector2(x01, 0f), disc, 1f, scope, 0);
				Vector2 newOne = Util.GetDisc01(new Vector2(x01, 1f), disc, 1f, scope, 0);
				newZero.x = Mathf.LerpUnclamped(stagePos.x - halfWidth, stagePos.x + halfWidth, newZero.x);
				newOne.x = Mathf.LerpUnclamped(stagePos.x - halfWidth, stagePos.x + halfWidth, newOne.x);
				newZero.y -= disc / 720f;
				newOne.y -= disc / 720f;
				Vector2 zero = (Vector2)(Quaternion.Euler(0f, 0f, rotZ) * (new Vector2(
					newZero.x,
					Mathf.LerpUnclamped(stagePos.y, stagePos.y + stageWidth, newZero.y)
				) - stagePos)) + stagePos;
				Vector2 one = (Vector2)(Quaternion.Euler(0f, 0f, rotZ) * (new Vector2(
					newOne.x,
					Mathf.LerpUnclamped(stagePos.y, stagePos.y + stageWidth, newOne.y)
				) - stagePos)) + stagePos;
				return (
					Vector3.LerpUnclamped(zero, one, y01),
					zero,
					Quaternion.FromToRotation(Vector3.up, one - zero).eulerAngles.z
				);
			}
		}


		// LGC
		private static Vector2 GetStagePosition (Beatmap.Stage data, float musicTime) =>
			new Vector2(data.X, data.Y) + Evaluate(data.Positions, musicTime - data.Time);


		private static Color GetStageColor (Beatmap.Stage data, float musicTime) {
			if (data.Colors is null) {
				return PaletteColor(data.Color);
			} else {
				return PaletteColor(data.Color) * EvaluateColor(data.Colors, musicTime - data.Time);
			}
		}


	}
}