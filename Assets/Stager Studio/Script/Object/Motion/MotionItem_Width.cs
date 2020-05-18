namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class MotionItem_Width : MotionItem {


		// Short
		


		protected override void InvokeAxis (Vector2 localPos) {
			var map = GetBeatmap();
			if (map != null) {
				//float value = Util.Remap(-m_LineLength, m_LineLength, 0, IndexCount - 1, localPos.x);
				//map.SetMotionValueTween(ItemIndex, MotionType, transform.GetSiblingIndex(), value);
				//SetLabelText(Mathf.RoundToInt(value).ToString());
				OnMotionChanged();
			}
		}
	}
}