namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;



	public class Stage : StageObject {




		#region --- VAR ---


		// Const
		private const float ABREAST_GAP = 0.02f;

		// Api
		public static int StageCount { get; set; } = 0;
		public static int SortingLayerID_Stage { get; set; } = -1;
		public static int SortingLayerID_Judge { get; set; } = -1;

		// Ser
		[SerializeField] private ObjectRenderer m_JudgelineRenderer = null;


		#endregion




		#region --- MSG ---


		protected override void Awake () {
			base.Awake();
			ColRot = null;
			m_JudgelineRenderer.SkinData = Skin;
			MainRenderer.Type = SkinType.Stage;
			m_JudgelineRenderer.Type = SkinType.JudgeLine;
		}


		private void OnEnable () {
			Update();
		}


		private void Update () {


			MainRenderer.RendererEnable = false;
			m_JudgelineRenderer.RendererEnable = false;
			ColSize = null;

			// Get StageData
			int stageIndex = transform.GetSiblingIndex();
			var stageData = !(Beatmap is null) && stageIndex < Beatmap.Stages.Count ? Beatmap.Stages[stageIndex] : null;
			if (stageData is null) {
				gameObject.SetActive(false);
				return;
			}

			stageData.Active = false;
			stageData.SpeedMuti = GetStageSpeed(stageData);
			Time = stageData.Time;
			Duration = stageData.Duration;

			// Stage Active Check
			bool active = GetStageActive(stageData, stageIndex);
			stageData.Active = active;

			Update_Gizmos(active, stageIndex);

			if (!active) {
				gameObject.SetActive(false);
				return;
			}

			// Update
			Update_Movement(stageData, stageIndex);

		}


		private void Update_Movement (Beatmap.Stage stageData, int stageIndex) {

			var (zoneMin, zoneMax, zoneSize, _) = ZoneMinMax;
			float width = GetStageWidth(stageData);
			float height = GetStageHeight(stageData);
			var stagePos = GetStagePosition(stageData, stageIndex);
			var judgeLineSize = GetRectSize(SkinType.JudgeLine, stageData.ItemType);

			// Movement
			var stageRot = Quaternion.Euler(0f, 0f, GetStageWorldRotationZ(stageData));
			transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, stagePos.x, stagePos.y);
			transform.localRotation = stageRot;
			ColSize = MainRenderer.transform.localScale = new Vector3(zoneSize * width, Mathf.Max(zoneSize * height, 0.00001f), 1f);
			m_JudgelineRenderer.transform.localScale = new Vector3(
				zoneSize * width,
				Mathf.Max(zoneSize * judgeLineSize.y, 0.00001f),
				1f
			);

			// Renderer
			MainRenderer.RendererEnable = m_JudgelineRenderer.RendererEnable = true;
			MainRenderer.ItemType = stageData.ItemType;
			MainRenderer.Scale = new Vector2(width, height);
			MainRenderer.Duration = m_JudgelineRenderer.Duration = Duration;
			MainRenderer.LifeTime = m_JudgelineRenderer.LifeTime = MusicTime - Time;
			MainRenderer.Alpha = m_JudgelineRenderer.Alpha = GetStageAlpha(stageData);
			m_JudgelineRenderer.ItemType = stageData.ItemType;
			m_JudgelineRenderer.Scale = new Vector2(width, judgeLineSize.y);
			m_JudgelineRenderer.SetSortingLayer(SortingLayerID_Judge, GetSortingOrder());
			MainRenderer.SetSortingLayer(SortingLayerID_Stage, GetSortingOrder());

		}


		private void Update_Gizmos (bool stageActive, int stageIndex) {

			// Label
			if (Label != null) {
				bool active = ShowIndexLabel && !MusicPlaying && stageActive;
				Label.gameObject.SetActive(active);
				if (active) {
					Label.Text = stageIndex.ToString();
				}
			}

		}


		#endregion




		#region --- API ---


		protected override void RefreshRendererZone () {
			base.RefreshRendererZone();
			RefreshRendererZoneFor(m_JudgelineRenderer);
		}


		public static float GetStageWorldRotationZ (Beatmap.Stage stageData) => Mathf.LerpAngle(
			-Mathf.Repeat(stageData.Rotation + Evaluate(stageData.Rotations, MusicTime - stageData.Time), 360f),
			0f,
			Abreast.value
		);


		public static float GetStageWidth (Beatmap.Stage data) => Mathf.Lerp(
			Mathf.Max(data.Width + Evaluate(data.Widths, MusicTime - data.Time), 0f),
			Abreast.width - ABREAST_GAP,
			Abreast.value
		);


		public static float GetStageHeight (Beatmap.Stage data) => Mathf.Lerp(
			Mathf.Max(data.Height + Evaluate(data.Heights, MusicTime - data.Time), 0.00001f),
			Mathf.Clamp(1f / ZoneMinMax.ratio, 0f, 256f),
			Abreast.value
		);


		public static float GetStageAlpha (Beatmap.Stage data) => Mathf.Lerp(
			Mathf.Clamp01(
				VanishDuration < DURATION_GAP ? 1f :
				Mathf.Min(data.Time + data.Duration - MusicTime, MusicTime - data.Time) / VanishDuration
			), 1f,
			Abreast.value
		);


		public static float GetStageSpeed (Beatmap.Stage data) => Mathf.Lerp(
			data.Speed, 1f, Abreast.value
		);


		public static bool GetStageActive (Beatmap.Stage data, int stageIndex) =>
			Abreast.value >= 0.5f || (((stageIndex >= 0 && stageIndex < StageCount) || Abreast.index == stageIndex) && MusicTime >= data.Time && MusicTime <= data.Time + data.Duration);


		public static Vector2 GetStagePosition (Beatmap.Stage data, int stageIndex) {
			if (Abreast.value < 0.0001f) {
				return GetNormalPos();
			} else if (Abreast.value > 0.9999f) {
				return GetAbreastPos();
			} else {
				return Vector2.Lerp(GetNormalPos(), GetAbreastPos(), Abreast.value);
			}
			// === Func ===
			Vector3 GetNormalPos () => new Vector2(data.X, data.Y) + Evaluate(data.Positions, MusicTime - data.Time);
			Vector3 GetAbreastPos () {
				if (StageCount <= 1) {
					return new Vector2(0.5f, 0f);
				} else {
					return new Vector2(Util.Remap(0, StageCount - 1, 0.5f - (Abreast.index * Abreast.width), 0.5f + ((StageCount - Abreast.index - 1f) * Abreast.width), stageIndex), 0f);
				}
			}
		}


		// Matrix
		public static Vector3 LocalToZone (
			float x01, float y01, float z01,
			Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ
		) => Matrix4x4.TRS(
			stagePos,
			Quaternion.Euler(0f, 0f, stageRotZ),
			new Vector3(stageWidth, stageHeight, stageHeight)
		).MultiplyPoint3x4(new Vector3(x01 - 0.5f, y01, z01));


		public static Vector3 ZoneToLocal (
			float zoneX, float zoneY, float zoneZ,
			Vector2 stagePos, float stageWidth, float stageHeight, float stageRotZ
		) {
			var pos01 = Matrix4x4.TRS(
				stagePos,
				Quaternion.Euler(0f, 0f, stageRotZ),
				new Vector3(stageWidth, stageHeight, stageHeight)
			).inverse.MultiplyPoint3x4(new Vector3(zoneX, zoneY, zoneZ));
			pos01.x += 0.5f;
			return pos01;
		}


		#endregion




	}
}