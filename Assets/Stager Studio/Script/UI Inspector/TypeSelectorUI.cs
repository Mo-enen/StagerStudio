namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;
	using UIGadget;
	using UnityEngine.Events;

	public class TypeSelectorUI : MonoBehaviour {


		// Event
		[System.Serializable] public class IntEvent : UnityEvent<int> { }


		// Ser
		[SerializeField] private Grabber m_TypePrefab = null;
		[SerializeField] private GridLayoutGroup m_Layout = null;
		[SerializeField] private int m_TypeIndex = 0;
		[Space, SerializeField] private IntEvent m_OnClick = null;

		// Data
		private readonly static List<Sprite[]> NoteSpritess = new List<Sprite[]>();
		private static int GlobalDirtyID = 0;
		private int LocalDirtyID = -1;


		// MSG
		private void OnEnable () {
			if (LocalDirtyID != GlobalDirtyID) {
				LocalDirtyID = GlobalDirtyID;
				var container = transform as RectTransform;
				container.DestroyAllChildImmediately();
				if (m_TypeIndex < 0 || m_TypeIndex >= NoteSpritess.Count) { return; }
				// Spawn
				var nsList = NoteSpritess[m_TypeIndex];
				for (int i = 0; i < nsList.Length; i++) {
					int index = i;
					var sprite = nsList[index];
					var grab = Instantiate(m_TypePrefab, container);
					var rt = grab.transform as RectTransform;
					rt.anchoredPosition3D = rt.anchoredPosition;
					rt.localScale = Vector3.one;
					rt.localRotation = Quaternion.identity;
					grab.Grab<Image>("Icon").sprite = sprite;
					grab.Grab<Button>().onClick.AddListener(() => m_OnClick.Invoke(index));
				}
				// Resize
				container.SetSizeWithCurrentAnchors(
					RectTransform.Axis.Vertical,
					m_Layout.padding.top + m_Layout.padding.bottom + Mathf.CeilToInt((float)nsList.Length / m_Layout.constraintCount) * (m_Layout.cellSize.y + m_Layout.spacing.y)
				);
			}
		}


		// API
		public static void CalculateSprites (SkinData skin) {
			GlobalDirtyID++;
			NoteSpritess.Clear();
			if (skin == null || skin.Texture == null) { return; }
			AddToList(SkinType.Stage);
			AddToList(SkinType.Track);
			AddToList(SkinType.Note);
			// === Func ===
			void AddToList (SkinType skinType) {
				var resultList = new List<Sprite>();
				if (skin.Items.Count > (int)skinType) {
					var noteRects = skin.Items[(int)skinType].Rects;
					foreach (var rect in noteRects) {
						resultList.Add(Sprite.Create(
							skin.Texture,
							new Rect(rect.X, rect.Y, rect.Width, rect.Height),
							Vector2.one * 0.5f,
							100,
							0,
							SpriteMeshType.FullRect,
							Vector4.zero,
							false
						));
					}
				}
				NoteSpritess.Add(resultList.ToArray());
			}
		}


	}
}