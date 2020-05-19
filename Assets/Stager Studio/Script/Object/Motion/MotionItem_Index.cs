namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class MotionItem_Index : MotionItem {



		// Ser
		[SerializeField] private float m_LineLength = 0.12f;
		[SerializeField] private Transform m_Handle = null;
		[SerializeField] private Transform m_Selection = null;



		// MSG
		private void OnEnable () => Update();


		private void Update () {
			var map = GetBeatmap();
			int motionIndex = transform.GetSiblingIndex();
			bool active = false;
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
							active = true;
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
									Stage.GetStagePosition(stage, track.StageIndex),
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
								active = true;
							}
						}
					}
				}

				// Slider Value
				if (active) {
					if (map.GetMotionValueTween(ItemIndex, MotionType, motionIndex, out float valueA, out _, out _).hasA) {
						SetSliderValue(Util.Remap(0f, IndexCount - 1, -1f, 1f, valueA));
					}
				}

				// Root
				Root.gameObject.TrySetActive(active);

				// Selection
				m_Selection.gameObject.TrySetActive(motionIndex == SelectingMotionIndex);

			}
		}


		protected override void InvokeAxis (Vector2 localPos) {
			var map = GetBeatmap();
			if (map != null) {
				float value = Util.Remap(-m_LineLength, m_LineLength, 0, IndexCount - 1, localPos.x);
				map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), value);
				SetLabelText(Mathf.RoundToInt(value).ToString());
				OnMotionChanged();
			}
		}


		// LGC
		private void SetSliderValue (float value11) => m_Handle.localPosition = new Vector3(
			Util.Remap(-1f, 1f, -m_LineLength, m_LineLength, Mathf.Clamp(value11, -1f, 1f)),
			0f, 0f
		);



	}
}