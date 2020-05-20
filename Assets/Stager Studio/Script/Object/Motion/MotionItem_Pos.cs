namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;

	public class MotionItem_Pos : MotionItem {


		// Ser
		[SerializeField] private Transform m_Selection = null;
		[SerializeField] private TextRenderer m_Index = null;
		[SerializeField] private SpriteRenderer m_Line = null;


		// MSG
		private void OnEnable () => Update();


		private void Update () {
			var map = GetBeatmap();
			int motionIndex = transform.GetSiblingIndex();
			var stage = map != null ? map.GetItem(0, ItemIndex) as Beatmap.Stage : null;
			bool active = false;
			if (stage != null) {
				float musicTime = GetMusicTime();
				var itemTime = map.GetTime(0, ItemIndex);
				var (hasA, hasB) = map.SearchMotionValueTween(ItemIndex, MotionType, musicTime - itemTime, out float valuA, out float valueB, out int tween, out int aimMotionIndex);
				if (hasA && hasB && aimMotionIndex >= 0) {
					active = motionIndex >= aimMotionIndex && motionIndex <= aimMotionIndex + 1;
				}
				if (active && map.GetMotionTime(ItemIndex, MotionType, motionIndex, out float motionTime)) {
					// Pos
					var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
					var zonePos = Stage.GetStagePosition(stage, ItemIndex, itemTime + motionTime);
					var worldPos = Util.Vector3Lerp3(zoneMin, zoneMax, zonePos.x, zonePos.y);
					transform.position = worldPos;
					// Line
					if (motionIndex < aimMotionIndex + 1 && map.GetMotionTime(ItemIndex, MotionType, motionIndex + 1, out float nextMotionTime)) {
						var nextPos = Stage.GetStagePosition(stage, ItemIndex, itemTime + nextMotionTime);
						var nextWorldPos = Util.Vector3Lerp3(zoneMin, zoneMax, nextPos.x, nextPos.y);
						m_Line.transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, nextWorldPos - worldPos));
						m_Line.size = new Vector2(Vector2.Distance(worldPos, nextWorldPos) / m_Line.transform.localScale.x, m_Line.size.y);
						m_Line.gameObject.TrySetActive(true);
					} else {
						m_Line.gameObject.TrySetActive(false);
					}
				}
			}
			// Index
			m_Index.RendererEnable = active;
			if (active) {
				m_Index.Text = motionIndex.ToString();
			}
			// Root
			Root.gameObject.TrySetActive(active);
			// Selection
			m_Selection.gameObject.TrySetActive(motionIndex == SelectingMotionIndex);
		}


		protected override void InvokeAxis (Vector2 localPos, Vector3 worldPos) {
			var map = GetBeatmap();
			if (map != null) {
				var (zoneMin, zoneMax, _, _) = GetZoneMinMax();
				var zonePos = Util.Vector3InverseLerp3(zoneMin, zoneMax, worldPos.x, worldPos.y, worldPos.z);
				if (GetGridEnabled()) {
					zonePos.x = Util.Snap(zonePos.x, 20f);
					zonePos.y = Util.Snap(zonePos.y, 20f);
				}
				map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), zonePos.x, zonePos.y);
				SetLabelText($"{zonePos.x.ToString("0.00")}, {zonePos.y.ToString("0.00")}");
				OnMotionChanged();
			}
		}



	}
}