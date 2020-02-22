namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class Stage : StageObject {




		#region --- VAR ---


		// Api
		public static int LayerID_Stage { get; set; } = -1;
		public static int StageCount { get; set; } = 0;

		// Ser
		[SerializeField] private StageRenderer m_JudgelineRenderer = null;


		#endregion




		#region --- MSG ---


		protected override void Awake () {
			base.Awake();
			MainRenderer.Pivot = new Vector3(0.5f, 0f);
			MainRenderer.Tint = Color.white;
			m_JudgelineRenderer.Tint = Color.white;
			m_JudgelineRenderer.Pivot = new Vector2(0.5f, 0f);
			ColRot = null;
		}


		private void Update () {

			MainRenderer.RendererEnable = false;
			m_JudgelineRenderer.RendererEnable = false;
			ColSize = null;

			// Get StageData
			int stageIndex = transform.GetSiblingIndex();
			var stageData = !(Beatmap is null) && stageIndex < Beatmap.Stages.Count ? Beatmap.Stages[stageIndex] : null;
			if (stageData is null) { return; }

			bool oldSelecting = stageData.Selecting;
			stageData.Active = false;
			stageData.Selecting = false;
			Time = stageData.Time;
			Duration = stageData.Duration;

			// Stage Active Check
			bool active = GetStageActive(stageData, stageIndex);
			stageData.Active = active;

			Update_Gizmos(active, oldSelecting, stageIndex);

			if (!active) { return; }
			stageData.Selecting = oldSelecting;

			// Update
			Update_Movement(stageData, stageIndex);

		}


		private void Update_Movement (Beatmap.Stage stageData, int stageIndex) {

			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			float width = GetStageWidth(stageData);
			float height = GetStageHeight(stageData);
			var stagePos = GetStagePosition(stageData, stageIndex);

			// Movement
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, stagePos.x, stagePos.y);
			transform.localRotation = Quaternion.Euler(0f, 0f, GetStageWorldRotationZ(stageData));
			ColSize = MainRenderer.transform.localScale = new Vector3(zoneSize * width, Mathf.Max(zoneSize * height, 0.00001f), 1f);
			m_JudgelineRenderer.transform.localScale = new Vector3(
				zoneSize * width,
				Mathf.Max(zoneSize * Note.NoteThickness, 0.00001f),
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = m_JudgelineRenderer.RendererEnable = true;
			MainRenderer.Type = SkinType.Stage;
			MainRenderer.Scale = new Vector2(width, height);
			MainRenderer.LifeTime = m_JudgelineRenderer.LifeTime = MusicTime - Time + TRANSATION_DURATION;
			MainRenderer.Alpha = m_JudgelineRenderer.Alpha = GetStageAlpha(stageData);
			m_JudgelineRenderer.Type = SkinType.JudgeLine;
			m_JudgelineRenderer.Scale = new Vector2(width, Note.NoteThickness);
			m_JudgelineRenderer.SetSortingLayer(LayerID_Stage, GetSortingOrder());
			MainRenderer.SetSortingLayer(LayerID_Stage, GetSortingOrder());

		}


		private void Update_Gizmos (bool stageActive, bool selecting, int stageIndex) {

			// Label
			if (Label != null) {
				bool active = ShowIndexLabel && !MusicPlaying && stageActive;
				Label.gameObject.SetActive(active);
				if (active) {
					Label.text = stageIndex.ToString();
				}
			}

			// Highlight
			if (Highlight != null) {
				Highlight.enabled = !MusicPlaying && stageActive && selecting;
			}


		}


		#endregion




		#region --- API ---


		public override void SetSkinData (SkinData skin) {
			base.SetSkinData(skin);
			m_JudgelineRenderer.SkinData = skin;
		}


		public static float GetStageWorldRotationZ (Beatmap.Stage stageData) => Abreast.active ? 0f : -Mathf.Repeat(stageData.Rotation + Evaluate(stageData.Rotations, MusicTime - stageData.Time), 360f);


		public static float GetStageWidth (Beatmap.Stage data) => Abreast.active ? Abreast.all && StageCount > 0 ? 1f / StageCount : 1f : Mathf.Max(data.Width + Evaluate(data.Widths, MusicTime - data.Time), 0f);


		public static float GetStageHeight (Beatmap.Stage data) => Abreast.active ? 1f / ZoneMinMax.ratio : Mathf.Max(data.Height + Evaluate(data.Heights, MusicTime - data.Time), 0f);


		public static float GetStageAlpha (Beatmap.Stage data) => Abreast.active ? 1f : Mathf.Clamp01(MusicTime < data.Time ? (MusicTime - data.Time + TRANSATION_DURATION) / TRANSATION_DURATION : MusicTime > data.Time + data.Duration ? (data.Time + data.Duration - MusicTime + TRANSATION_DURATION) / TRANSATION_DURATION : 1f);


		public static bool GetStageActive (Beatmap.Stage data, int stageIndex) => (!Abreast.active || (Abreast.all && stageIndex >= 0 && stageIndex < StageCount) || Abreast.index == stageIndex) && MusicTime > data.Time - TRANSATION_DURATION && MusicTime < data.Time + data.Duration + TRANSATION_DURATION;


		public static Vector2 GetStagePosition (Beatmap.Stage data, int stageIndex) => Abreast.active ? new Vector2(Abreast.all && StageCount > 0 ? ((2 * stageIndex + 1) / (2f * StageCount)) : 0.5f, 0f) : (new Vector2(data.X, data.Y) + Evaluate(data.Positions, MusicTime - data.Time));


		public static float GetStageAngle (Beatmap.Stage data) => Abreast.active ? 0f : data.Angle + Evaluate(data.Angles, MusicTime - data.Time);


		public static (Vector2 pos, Vector2 zero, float rot) Inside (float x01, float y01, Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ) {
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


		#endregion




	}
}