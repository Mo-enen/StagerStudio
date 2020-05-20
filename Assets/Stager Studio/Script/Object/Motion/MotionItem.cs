namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;



	public abstract class MotionItem : MonoBehaviour {



		// SUB
		public delegate (Vector3 min, Vector3 max, float size, float ratio) ZoneHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate float FloatHandler ();
		public delegate void VoidHandler ();
		public delegate bool BoolHandler ();


		// Handler
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetSpeedMuti { get; set; } = null;
		public static VoidHandler OnMotionChanged { get; set; } = null;
		public static BoolHandler GetGridEnabled { get; set; } = null;

		// Api
		public static int SelectingMotionIndex { get; set; } = -1;
		public int ItemIndex { get; set; } = -1;
		public int MotionType { get; set; } = -1;
		public int IndexCount { get; set; } = 0;

		// Short
		protected Transform Root => transform.GetChild(0);
		protected Transform Handle => m_Handle;
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private TextRenderer m_Label = null;
		[SerializeField] private Transform m_Handle = null;


		// Data
		private Camera _Camera = null;
		private Vector3? HandlePosOffset = null;



		// MSG
		private void LateUpdate () {
			// Label
			if (!Input.GetMouseButton(0)) {
				m_Label.RendererEnable = false;
			}
		}


		// API
		public void OnAxisDownOrDrag (bool down) {
			int motionIndex = transform.GetSiblingIndex();
			SelectingMotionIndex = motionIndex;
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var pos = Util.GetRayPosition(Camera.ScreenPointToRay(Input.mousePosition), transform);
			if (down) {
				HandlePosOffset = null;
			}
			if (pos.HasValue) {
				if (!HandlePosOffset.HasValue) {
					HandlePosOffset = pos.Value - Handle.transform.position;
				}
				var worldPos = pos.Value - HandlePosOffset.Value;
				InvokeAxis(transform.worldToLocalMatrix.MultiplyPoint(worldPos), worldPos);
			}
		}



		// ABS
		protected abstract void InvokeAxis (Vector2 localPos, Vector3 worldPos);



		// LGC
		protected void SetLabelText (string text) {
			m_Label.Text = $" {text} ";
			m_Label.RendererEnable = true;
		}


		protected Vector3? GetPosition (Beatmap map, int motionIndex, bool useRotation = true) {
			// Pos
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			int itemType = MotionType >= 0 && MotionType <= 4 ? 0 : 1;
			int stageIndex = ItemIndex;
			var item = map.GetItem(itemType, ItemIndex);
			if (item == null) { return null; }
			if (itemType == 1) {
				stageIndex = (item as Beatmap.Track).StageIndex;
			}
			var stage = map.GetItem(0, stageIndex) as Beatmap.Stage;
			if (stage != null) {
				float musicTime = GetMusicTime();
				var itemTime = map.GetTime(itemType, ItemIndex);
				if (map.GetMotionTime(ItemIndex, MotionType, motionIndex, out float motionTime) && itemTime + motionTime + 0.001f > musicTime) {
					float speedMuti = GetSpeedMuti();
					float y01 = (itemTime + motionTime - musicTime) * speedMuti;
					if (y01 >= 0f) {
						float stageRot = useRotation ? Stage.GetStageWorldRotationZ(stage) : 0f;
						var zonePos = Stage.LocalToZone(
							0.5f, y01, 0f,
							Stage.GetStagePosition(stage, stageIndex),
							Stage.GetStageWidth(stage),
							Stage.GetStageHeight(stage),
							Stage.GetStagePivotY(stage),
							stageRot
						);
						return Util.Vector3Lerp3(zoneMin, zoneMax, zonePos.x, zonePos.y, zonePos.z);
					}
				}
			}
			return null;
		}


	}
}