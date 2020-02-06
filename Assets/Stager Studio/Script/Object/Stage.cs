namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Stage : StageObject {




		#region --- VAR ---


		// Short
		private StageRenderer JudgelineRenderer => m_JudgelineRenderer;

		// Ser
		[SerializeField] private StageRenderer m_JudgelineRenderer = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			MainRenderer.Pivot = new Vector3(0.5f, 0f);
			JudgelineRenderer.Tint = Color.white;
			JudgelineRenderer.Pivot = new Vector2(0.5f, 0f);

		}


		private void Update () {

			MainRenderer.RendererEnable = false;
			m_JudgelineRenderer.RendererEnable = false;
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
			float width = GetStageWidth(stageData, musicTime);
			float height = GetStageHeight(stageData, musicTime);
			var stagePos = GetStagePosition(stageData, musicTime);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, stagePos.x, stagePos.y);
			transform.localRotation = Quaternion.Euler(0f, 0f, GetStageWorldRotationZ(stageData, musicTime));
			MainRenderer.transform.localScale = new Vector3(zoneSize * width, Mathf.Max(zoneSize * height, 0.00001f), 1f);
			JudgelineRenderer.transform.localScale = new Vector3(
				zoneSize * width,
				Mathf.Max(zoneSize * Note.NoteThickness, 0.00001f),
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = JudgelineRenderer.RendererEnable = true;
			MainRenderer.Type = SkinType.Stage;
			MainRenderer.Scale = new Vector2(width, height);
			MainRenderer.LifeTime = JudgelineRenderer.LifeTime = musicTime - Time + TRANSATION_DURATION;
			MainRenderer.Alpha = JudgelineRenderer.Alpha = GetStageAlpha(stageData, musicTime, TRANSATION_DURATION);
			MainRenderer.Tint = GetStageColor(stageData, musicTime);
			JudgelineRenderer.Type = SkinType.JudgeLine;
			JudgelineRenderer.Scale = new Vector2(width, Note.NoteThickness);

		}


		#endregion




		#region --- API ---


		public override void SetSkinData (SkinData skin, int layerID, int orderID) {
			base.SetSkinData(skin, layerID, orderID);
			JudgelineRenderer.SkinData = skin;
			JudgelineRenderer.SetSortingLayer(layerID, orderID + 1);
		}


		public static float GetStageWorldRotationZ (Beatmap.Stage stageData, float musicTime) {
			return -Mathf.Repeat(
				stageData.Rotation + Evaluate(stageData.Rotations, musicTime - stageData.Time),
				360f
			);
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


		public static (Vector2 pos, Vector2 zero, float rot) Inside (
			float x01, float y01, Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ
		) {
			float halfWidth = stageWidth * 0.5f;
			x01 = Mathf.LerpUnclamped(stagePos.x - halfWidth, stagePos.x + halfWidth, x01);
			return (
				(Vector2)(Quaternion.Euler(0f, 0f, stageRotZ) * (
				new Vector2(
					x01,
					Mathf.LerpUnclamped(stagePos.y, stagePos.y + stageHeight, y01)
				) - stagePos)) + stagePos,
				(Vector2)(Quaternion.Euler(0f, 0f, stageRotZ) * (new Vector2(x01, stagePos.y) - stagePos)) + stagePos,
				stageRotZ
			);
		}


		public static Vector2 GetStagePosition (Beatmap.Stage data, float musicTime) =>
			new Vector2(data.X, data.Y) + Evaluate(data.Positions, musicTime - data.Time);


		public static float GetStageAngle (Beatmap.Stage data, float musicTime) {
			return data.Angle + Evaluate(data.Angles, musicTime - data.Time);
		}


		#endregion




		#region --- LGC ---


		private static Color GetStageColor (Beatmap.Stage data, float musicTime) {
			if (data.Colors is null) {
				return PaletteColor(data.Color);
			} else {
				return PaletteColor(data.Color) * EvaluateColor(data.Colors, musicTime - data.Time);
			}
		}


		#endregion




	}
}