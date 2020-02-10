namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Stage : StageObject {




		#region --- VAR ---


		// Api
		public static int LayerID_Stage { get; set; } = -1;

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

			// Get StageData
			int index = transform.GetSiblingIndex();
			var beatmap = GetBeatmap();
			var stageData = !(beatmap is null) && index < beatmap.Stages.Count ? beatmap.Stages[index] : null;
			if (stageData is null) { return; }

			Time = stageData.Time;
			Duration = stageData.Duration;

			// Stage Active Check
			if (!GetStageActive(stageData)) { return; }

			// Update
			Update_Movement(stageData);

		}


		private void Update_Movement (Beatmap.Stage stageData) {

			var (zoneMin, zoneMax, zoneSize) = GetZoneMinMax();
			float width = GetStageWidth(stageData);
			float height = GetStageHeight(stageData);
			var stagePos = GetStagePosition(stageData);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, stagePos.x, stagePos.y);
			transform.localRotation = Quaternion.Euler(0f, 0f, GetStageWorldRotationZ(stageData));
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
			MainRenderer.LifeTime = JudgelineRenderer.LifeTime = MusicTime - Time + TRANSATION_DURATION;
			MainRenderer.Alpha = JudgelineRenderer.Alpha = GetStageAlpha(stageData, TRANSATION_DURATION);
			MainRenderer.Tint = GetStageColor(stageData);
			JudgelineRenderer.Type = SkinType.JudgeLine;
			JudgelineRenderer.Scale = new Vector2(width, Note.NoteThickness);
			JudgelineRenderer.SetSortingLayer(LayerID_Stage, GetSortingOrder());
			MainRenderer.SetSortingLayer(LayerID_Stage, GetSortingOrder());

		}


		#endregion




		#region --- API ---


		public override void SetSkinData (SkinData skin) {
			base.SetSkinData(skin);
			JudgelineRenderer.SkinData = skin;
		}


		public static float GetStageWorldRotationZ (Beatmap.Stage stageData) {
			return -Mathf.Repeat(
				stageData.Rotation + Evaluate(stageData.Rotations, MusicTime - stageData.Time),
				360f
			);
		}


		public static float GetStageWidth (Beatmap.Stage data) {
			return Mathf.Max(data.Width + Evaluate(data.Widths, MusicTime - data.Time), 0f);
		}


		public static float GetStageHeight (Beatmap.Stage data) {
			return Mathf.Max(data.Height + Evaluate(data.Heights, MusicTime - data.Time), 0f);
		}


		public static float GetStageAlpha (Beatmap.Stage data, float transation) => Mathf.Clamp01(
			MusicTime < data.Time ? (MusicTime - data.Time + transation) / transation :
			MusicTime > data.Time + data.Duration ? (data.Time + data.Duration - MusicTime + transation) / transation :
			1f
		);


		public static bool GetStageActive (Beatmap.Stage data) => MusicTime > data.Time - TRANSATION_DURATION && MusicTime < data.Time + data.Duration + TRANSATION_DURATION;


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


		public static Vector2 GetStagePosition (Beatmap.Stage data) =>
			new Vector2(data.X, data.Y) + Evaluate(data.Positions, MusicTime - data.Time);


		public static float GetStageAngle (Beatmap.Stage data) {
			return data.Angle + Evaluate(data.Angles, MusicTime - data.Time);
		}


		#endregion




		#region --- LGC ---


		private static Color GetStageColor (Beatmap.Stage data) {
			if (data.Colors is null) {
				return PaletteColor(data.Color);
			} else {
				return PaletteColor(data.Color) * EvaluateColor(data.Colors, MusicTime - data.Time);
			}
		}


		#endregion




	}
}