namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class MotionItem_X : MotionItem {




		// Ser
		[SerializeField] private Transform m_Handle = null;
		[SerializeField] private Transform m_Selection = null;
		[SerializeField] private SpriteRenderer m_Line = null;
		[SerializeField] private SpriteRenderer m_Highlight = null;
		[SerializeField] private BoxCollider m_Col = null;


		// MSG
		private void OnEnable () => Update();


		private void Update () {
			var map = GetBeatmap();
			int motionIndex = transform.GetSiblingIndex();
			bool active = false;
			if (map != null) {
				// Pos
				var (zoneMin, zoneMax, zoneSize, _) = GetZoneMinMax();
				int itemType = MotionType >= 0 && MotionType <= 4 ? 0 : 1;
				var item = map.GetItem(itemType, ItemIndex);
				if (item != null && itemType == 1) {
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
								float stageWidth = Stage.GetStageWidth(stage);
								float stageRot = Stage.GetStageWorldRotationZ(stage);
								var zonePos = Stage.LocalToZone(
									0.5f, y01, 0f,
									Stage.GetStagePosition(stage, track.StageIndex),
									stageWidth,
									Stage.GetStageHeight(stage),
									Stage.GetStagePivotY(stage),
									stageRot
								);
								transform.position = Util.Vector3Lerp3(zoneMin, zoneMax, zonePos.x, zonePos.y, zonePos.z);
								transform.rotation = Quaternion.Euler(0f, 0f, stageRot) * Quaternion.Euler(trackAngle, 0f, 0f);
								SetSliderWidth(zoneSize * stageWidth);
								active = true;
							}
						}
					}
				}

				// Slider Value
				if (active) {
					if (map.GetMotionValueTween(ItemIndex, MotionType, motionIndex, out float valueA, out _, out _).hasA) {
						SetSliderValue(valueA);
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
				float value = Util.RemapUnclamped(-m_Line.size.x / 2f, m_Line.size.x / 2f, -1f, 1f, localPos.x);
				if (GetGridEnabled()) {
					value = Util.Snap(value, 16f);
				}
				map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), value);
				SetLabelText(value.ToString("0.00"));
				OnMotionChanged();
			}
		}


		private void SetSliderValue (float value11) {
			m_Handle.localPosition = new Vector3(
				Util.RemapUnclamped(-1f, 1f, -m_Line.size.x / 2f, m_Line.size.x / 2f, Mathf.Clamp(value11, -2f, 2f)),
				0f, 0f
			);
		}


		private void SetSliderWidth (float width) {
			m_Line.size = new Vector2(width, m_Line.size.y);
			m_Col.size = new Vector3(width, m_Col.size.y, m_Col.size.z);
			m_Highlight.size = new Vector2(width / m_Highlight.transform.localScale.x, m_Highlight.size.y);
		}


	}
}