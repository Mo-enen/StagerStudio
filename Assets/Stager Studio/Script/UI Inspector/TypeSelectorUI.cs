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

		// Api
		public int TypeIndex {
			get => m_TypeIndex;
			set {
				if (m_TypeIndex != value) {
					m_TypeIndex = value;
					LocalDirtyID = GlobalDirtyID - 1;
				}
			}
		}

		// Ser
		[SerializeField] private Grabber m_TypePrefab = null;
		[SerializeField] private GridLayoutGroup m_Layout = null;
		[SerializeField] private int m_TypeIndex = 0;
		[Space, SerializeField] private IntEvent m_OnClick = null;

		// Data
		private readonly static List<(Sprite icon, Sprite iconAlt)[]> NoteSpritess = new List<(Sprite, Sprite)[]>();
		private static int GlobalDirtyID = 0;
		private int LocalDirtyID = -1;



		// MSG
		private void Update () {
			if (LocalDirtyID != GlobalDirtyID) {
				LocalDirtyID = GlobalDirtyID;
				var container = transform as RectTransform;
				container.DestroyAllChildImmediately();
				if (m_TypeIndex < 0 || m_TypeIndex >= NoteSpritess.Count) { return; }
				// Spawn
				var nsList = NoteSpritess[m_TypeIndex];
				for (int i = 0; i < nsList.Length; i++) {
					int index = i;
					var (icon, iconAlt) = nsList[index];
					var grab = Instantiate(m_TypePrefab, container);
					var rt = grab.transform as RectTransform;
					rt.anchoredPosition3D = rt.anchoredPosition;
					rt.localScale = Vector3.one;
					rt.localRotation = Quaternion.identity;
					var iconIMG = grab.Grab<Image>("Icon");
					var iconIMG_Alt = grab.Grab<Image>("Icon Alt");
					iconIMG.sprite = icon;
					iconIMG.color = icon != null ? Color.white : Color.clear;
					iconIMG_Alt.sprite = iconAlt;
					iconIMG_Alt.color = iconAlt != null ? Color.white : Color.clear;
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
			AddToList(SkinType.JudgeLine, SkinType.Stage);
			AddToList(SkinType.Track, SkinType.TrackTint);
			AddToList(SkinType.Note, SkinType.Pole);
			// === Func ===
			void AddToList (SkinType skinType, SkinType skinTypeAlt) {
				var resultList = new List<(Sprite icon, Sprite iconAlt)>();
				if ((int)skinType < skin.Items.Count && (int)skinTypeAlt < skin.Items.Count) {
					var noteRects = skin.Items[(int)skinType].Rects;
					var noteRects_Alt = skin.Items[(int)skinTypeAlt].Rects;
					Sprite iconSP;
					Sprite iconSP_Alt;
					for (int i = 0; i < noteRects.Count || i < noteRects_Alt.Count; i++) {
						// Icon
						if (i < noteRects.Count) {
							var rect = noteRects[i];
							iconSP = Sprite.Create(
								skin.Texture,
								new Rect(rect.X, rect.Y, rect.Width, rect.Height),
								Vector2.one * 0.5f,
								100,
								0,
								SpriteMeshType.FullRect,
								Vector4.zero,
								false
							);
						} else {
							iconSP = null;
						}
						// Icon Alt
						if (i < noteRects_Alt.Count) {
							var rect = noteRects_Alt[i];
							iconSP_Alt = Sprite.Create(
								skin.Texture,
								new Rect(rect.X, rect.Y, rect.Width, rect.Height),
								Vector2.one * 0.5f,
								100,
								0,
								SpriteMeshType.FullRect,
								Vector4.zero,
								false
							);
						} else {
							iconSP_Alt = null;
						}
						// Add
						resultList.Add((iconSP, iconSP_Alt));
					}
				}
				NoteSpritess.Add(resultList.ToArray());
			}
		}



	}
}