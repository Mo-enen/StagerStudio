namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	public class CursorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


		// SUB
		public enum CursorType {
			Normal = -1,
			Hand = 0,
			Beam = 1,
			Plus = 2,
			Reduce = 3,
			Move = 4,
			R = 5,
			U = 6,
			LU = 7,
			RU = 8,
			Grab = 9,

		}


		// VAR
		public delegate (Texture2D cursor, Vector2 offset) TextureVector2IntHandler (int index);
		public static TextureVector2IntHandler GetCursorTexture { get; set; } = null;
		private static RectTransform CurrentRT = null;
		private static CursorType CurrentType = CursorType.Normal;
		private static CursorType PrevType = CursorType.Normal;

		[SerializeField] private CursorType m_Type = CursorType.Hand;
		private Selectable Select = null;


		// MSG-API
		public static void GlobalUpdate () {
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) { return; }
			if (CurrentRT is null) {
				if (CurrentType != PrevType) {
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
					PrevType = CurrentType = CursorType.Normal;
				}
			} else {
				if (CurrentType != PrevType) {
					var (cursor, offset) = GetCursorTexture((int)CurrentType);
					Cursor.SetCursor(cursor, offset, CursorMode.Auto);
					PrevType = CurrentType;
				}
			}
		}


		// MSG
		private void Awake () {
			Select = GetComponent<Selectable>();
		}


		private void OnDisable () {
			if (CurrentRT == transform) {
				CurrentRT = null;
				CurrentType = CursorType.Normal;
			}
		}


		public void OnPointerEnter (PointerEventData eventData) {
			if (Select && !Select.interactable) { return; }
			CurrentRT = transform as RectTransform;
			CurrentType = m_Type;
		}


		public void OnPointerExit (PointerEventData eventData) {
			if (CurrentRT == transform) {
				CurrentRT = null;
				CurrentType = CursorType.Normal;
			}
		}


	}
}