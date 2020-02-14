namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;


	public class DragToMove : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {




		#region --- VAR ---


		// Ser
		[SerializeField] private RectTransform m_Target = null;

		// Data
		private Vector2 Offset = default;


		#endregion




		#region --- MSG ---


		private void OnRectTransformDimensionsChange () => Util.ClampRectTransform(m_Target);


		private void OnEnable () => Util.ClampRectTransform(m_Target);


		public void OnBeginDrag (PointerEventData e) => Offset = ScreenToLocal(e.position, e.pressEventCamera) - m_Target.anchoredPosition;


		public void OnDrag (PointerEventData e) => m_Target.anchoredPosition = ScreenToLocal(e.position, e.pressEventCamera) - Offset;


		public void OnEndDrag (PointerEventData e) => Util.ClampRectTransform(m_Target);


		#endregion




		#region --- LGC ---


		private Vector2 ScreenToLocal (Vector2 pos, Camera camera) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Target.parent as RectTransform, pos, camera, out Vector2 localPos);
			return localPos;
		}


		#endregion




	}
}