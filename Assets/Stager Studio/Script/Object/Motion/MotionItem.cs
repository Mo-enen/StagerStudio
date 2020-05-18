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
		public delegate float Float4Handler (float a, float b, float c);


		// Handler
		public static ZoneHandler GetZoneMinMax { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetSpeedMuti { get; set; } = null;
		public static VoidHandler OnMotionChanged { get; set; } = null;

		// Api
		public static int SelectingMotionIndex { get; set; } = -1;
		public int ItemIndex { get; set; } = -1;
		public int MotionType { get; set; } = -1;
		public int IndexCount { get; set; } = 0;

		// Short
		protected bool Active { get; set; } = false;
		protected Transform Root => transform.GetChild(0);
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Transform m_Selection = null;
		[SerializeField] private TextRenderer m_Label = null;

		// Data
		private Camera _Camera = null;



		// MSG
		protected virtual void Update () {
			var map = GetBeatmap();
			int motionIndex = transform.GetSiblingIndex();
			Active = false;
			if (map != null) {
				// Pos
				var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
				int itemType = MotionType >= 0 && MotionType <= 4 ? 0 : 1;
				var item = map.GetItem(itemType, ItemIndex);
				if (item != null && itemType == 0) {
					// Stage
					float musicTime = GetMusicTime();
					var itemTime = map.GetTime(itemType, ItemIndex);
					if (map.GetMotionTime(ItemIndex, MotionType, motionIndex, out float motionTime) && itemTime + motionTime > musicTime) {
						var stage = item as Beatmap.Stage;
						float speedMuti = GetSpeedMuti();
						float y01 = (itemTime + motionTime - musicTime) * speedMuti;
						if (y01 >= 0f) {
							float stageRot = Stage.GetStageWorldRotationZ(stage);
							var zonePos = Stage.LocalToZone(
								0.5f, y01, 0f,
								Stage.GetStagePosition(stage, ItemIndex),
								Stage.GetStageWidth(stage),
								Stage.GetStageHeight(stage),
								Stage.GetStagePivotY(stage),
								stageRot
							);
							transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, zonePos.x, zonePos.y, zonePos.z);
							transform.rotation = Quaternion.Euler(0f, 0f, stageRot);
							Active = true;
						}
					}
				} else if (item != null && itemType == 1) {
					// Track
					var track = item as Beatmap.Track;
					var stage = map.GetItem(0, track.StageIndex) as Beatmap.Stage;
					if (stage != null) {
						float musicTime = GetMusicTime();
						var itemTime = map.GetTime(itemType, ItemIndex);
						if (map.GetMotionTime(ItemIndex, MotionType, motionIndex, out float motionTime) && itemTime + motionTime > musicTime) {
							float speedMuti = GetSpeedMuti();
							float y01 = (itemTime + motionTime - musicTime) * speedMuti;
							if (y01 >= 0f) {
								float trackAngle = Track.GetTrackAngle(track);
								float stageRot = Stage.GetStageWorldRotationZ(stage);
								var zonePos = Track.LocalToZone(
									0.5f, y01, 0f,
									Stage.GetStagePosition(stage, ItemIndex),
									Stage.GetStageWidth(stage),
									Stage.GetStageHeight(stage),
									Stage.GetStagePivotY(stage),
									stageRot,
									Track.GetTrackX(track),
									Track.GetTrackWidth(track),
									trackAngle
								);
								transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, zonePos.x, zonePos.y, zonePos.z);
								transform.rotation = Quaternion.Euler(0f, 0f, stageRot) * Quaternion.Euler(trackAngle, 0f, 0f);
								Active = true;
							}
						}
					}
				}
			}

			// Root
			Root.gameObject.TrySetActive(Active);

			// Selection
			m_Selection.gameObject.TrySetActive(motionIndex == SelectingMotionIndex);

			// Label
			if (!Input.GetMouseButton(0)) {
				m_Label.RendererEnable = false;
			}
		}



		// API
		public void OnAxisDownOrDrag () {
			int motionIndex = transform.GetSiblingIndex();
			SelectingMotionIndex = motionIndex;
			var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
			var pos = Util.GetRayPosition(Camera.ScreenPointToRay(Input.mousePosition), transform);
			if (pos.HasValue) {
				InvokeAxis(transform.worldToLocalMatrix.MultiplyPoint(pos.Value));
			}
		}



		// ABS
		protected abstract void InvokeAxis (Vector2 localPos);



		// LGC
		protected void SetLabelText (string text) {
			m_Label.Text = $" {text} ";
			m_Label.RendererEnable = true;
		}


	}
}