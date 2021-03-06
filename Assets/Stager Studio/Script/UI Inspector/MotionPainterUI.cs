﻿namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;
	using Object;
	using DebugLog;


	public class MotionPainterUI : Image {




		#region --- SUB ---


		// Handler
		public delegate Beatmap BeatmapHandler ();
		public delegate float FloatHandler ();
		public delegate int IntHandler ();
		public delegate void VoidHandler ();
		public delegate void VoidFloatHandler (float value);
		public delegate Sprite CharToSpriteHandler (char c);


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int[] BEAT_DIVs = { 4, 8, 16, };
		private const float DIV_LINE_SIZE = 1f;
		private const float SECTION_LINE_SIZE = 1f;
		private const float ITEM_HEIGHT = 24f;
		private const float LABEL_WIDTH = 6;
		private const float LABEL_HEIGHT = 8;
		private const float SCROLL_BAR_WIDTH = 2;

		// Handler
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetMusicDuration { get; set; } = null;
		public static FloatHandler GetBPM { get; set; } = null;
		public static IntHandler GetBeatPerSection { get; set; } = null;
		public static VoidFloatHandler SeekMusic { get; set; } = null;
		public static VoidHandler OnItemEdit { get; set; } = null;
		public static VoidHandler OnSelectionChanged { get; set; } = null;
		public static CharToSpriteHandler GetSprite { get; set; } = null;
		public static IntHandler GetPaletteCount { get; set; } = null;

		// Api
		public int ItemType => MotionType >= 0 && MotionType <= 4 ? 0 : MotionType >= 5 && MotionType <= 8 ? 1 : -1;
		public int ItemIndex { get; set; } = -1;
		public int MotionType { get; set; } = -1;
		public float ScrollValue { get; set; } = 0f;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Sprite m_Line = null;
		[SerializeField] private Sprite m_SectionLine = null;
		[SerializeField] private MotionItem[] m_ItemPrefabs = null;
		[SerializeField] private Sprite[] m_DivIcons = null;
		[SerializeField] private Image m_DivIMG = null;
		[SerializeField] private Transform m_MotionContainer = null;
		[SerializeField] private InputField m_ValueIF = null;
		[SerializeField] private InputField m_TweenIF = null;
		[SerializeField] private Selectable m_ValueLabel = null;
		[SerializeField] private Selectable m_TweenLabel = null;
		[SerializeField] private Color m_BgTintA = Color.white;
		[SerializeField] private Color m_BgTintB = Color.white;
		[SerializeField] private Color m_DivTint = Color.black;
		[SerializeField] private Color m_SectionTint = Color.black;
		[SerializeField] private Color m_SectionTint_End = Color.red;
		[SerializeField] private Color m_HighlightTint = Color.green;
		[SerializeField] private Color m_MusicHighlightTint = Color.green;
		[SerializeField] private Color m_ItemTint = Color.green;
		[SerializeField] private Color m_ItemTint_Highlight = Color.cyan;
		[SerializeField] private Color m_LabelTint = Color.white;
		[SerializeField] private Color m_ScrollbarTint = Color.white;

		// Data
		private static readonly UIVertex[] VertexCache = new UIVertex[4] { default, default, default, default };
		private (bool active, bool leftDown, bool rightDown, Vector3 pos, Vector2 pos01) Mouse = default;
		private int DivisionIndex = 1;
		private int HoveredBeat = -1;
		private int HoveredDiv = -1;
		private bool UIReady = true;
		private int PrevSelectingMotionIndex = -1;
		private Camera _Camera = null;


		#endregion




		#region --- MSG ---


		protected override void Awake () {
			base.Awake();
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			m_DivIMG.sprite = m_DivIcons[DivisionIndex];
		}


		protected override void OnEnable () {
			base.OnEnable();
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			RefreshFieldUI();
			// Seek
			var map = GetBeatmap();
			if (map != null) {
				float bps = GetBPM() / 60f;
				float localMusicBeat = (GetMusicTime() - map.GetTime(ItemType, ItemIndex)) * bps;
				var (_, _, _, beatDuration) = GetContentStartYEndY();
				int beatCount = Mathf.CeilToInt(beatDuration) + 1;
				if (beatCount > 0) {
					ScrollValue = Mathf.Clamp01(localMusicBeat / beatCount);
				}
			}
		}


		private void Update () {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			Update_Mouse();
			Update_Item();
			Update_UI();
		}


		private void Update_Mouse () {
			var rt = transform as RectTransform;
			var pos01 = rt.Get01Position(Input.mousePosition, Camera);
			pos01.y = 1f - pos01.y;
			if (pos01.x >= 0f && pos01.x <= 1f && pos01.y >= 0f && pos01.y <= 1f) {

				// Inside
				bool leftDown = Input.GetMouseButton(0);
				bool rightDown = Input.GetMouseButton(1);
				bool prevLeftDown = Mouse.leftDown;
				bool prevRightDown = Mouse.rightDown;

				// Pos Changed, Down Changed
				if (!Mouse.active || Input.mousePosition != Mouse.pos || leftDown != Mouse.leftDown || rightDown != Mouse.rightDown) {
					Mouse = (true, leftDown, rightDown, Input.mousePosition, pos01);
					SetVerticesDirty();
				}

				// Scroll
				if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f) {
					var (cStart, cEnd, _, _) = GetContentStartYEndY();
					if (cStart < cEnd) {
						ScrollValue = Mathf.Clamp01(ScrollValue - Input.mouseScrollDelta.y / (cEnd - cStart) * 0.1f);
						SetVerticesDirty();
					}
				}

				// L Down
				if (leftDown && !prevLeftDown) {
					Beatmap map = null;
					if (HoveredBeat >= 0 && HoveredDiv >= 0) {
						map = GetBeatmap();
						if (map != null) {
							// Add Motion
							float bps = GetBPM() / 60f;
							float localTimeL = (HoveredBeat + (HoveredDiv - 0.1f) / BEAT_DIVs[DivisionIndex]) / bps;
							float localTimeR = (HoveredBeat + (HoveredDiv + 0.1f) / BEAT_DIVs[DivisionIndex]) / bps;
							int motionCount = map.GetMotionCount(ItemIndex, MotionType);
							bool overlap = false;
							for (int i = 0; i < motionCount; i++) {
								if (map.GetMotionTime(ItemIndex, MotionType, i, out float time)) {
									if (time > localTimeL && time < localTimeR) {
										MotionItem.SelectingMotionIndex = i;
										SetVerticesDirty();
										overlap = true;
										break;
									}
								}
							}
							if (!overlap) {
								float localTime = (HoveredBeat + (HoveredDiv + 0f) / BEAT_DIVs[DivisionIndex]) / bps;
								var (hasA, hasB) = map.SearchMotionValueTween(ItemIndex, MotionType, localTime, out float valueA, out float valueB, out _, out _);
								map.AddMotion(ItemIndex, MotionType, localTime, hasA ? (float?)valueA : null, hasB ? (float?)valueB : null);
								MotionItem.SelectingMotionIndex = -1;
								SetVerticesDirty();
								OnItemEdit();
							}
						}
					}
					Log("L Down");
				}

				// R Down
				if (rightDown) {
					if (HoveredBeat >= 0 && HoveredDiv >= 0) {
						var map = GetBeatmap();
						if (map != null) {
							float bps = GetBPM() / 60f;
							float localTime = (HoveredBeat + (float)HoveredDiv / BEAT_DIVs[DivisionIndex]) / bps;
							float itemTime = map.GetTime(ItemType, ItemIndex);
							SeekMusic(itemTime + localTime);
							SetVerticesDirty();
						}
					}
					if (!prevRightDown) {
						Log("R Down");
					}
				}

			} else {
				// Outside
				if (Mouse.active || Mouse.leftDown || Mouse.rightDown) {
					Mouse.active = false;
					Mouse.leftDown = false;
					Mouse.rightDown = false;
					SetVerticesDirty();
				}
			}

			// Del
			if (Input.GetKeyDown(KeyCode.Delete) && MotionItem.SelectingMotionIndex >= 0) {
				var map = GetBeatmap();
				if (map != null) {
					map.DeleteMotion(ItemIndex, MotionType, MotionItem.SelectingMotionIndex);
					MotionItem.SelectingMotionIndex = -1;
					SetVerticesDirty();
					OnItemEdit();
				}
				Log("Delete");
			}

		}


		private void Update_Item () {
			var map = GetBeatmap();
			if (map != null && ItemType >= 0 && ItemIndex >= 0 && MotionType >= 0) {
				int motionCount = map.GetMotionCount(ItemIndex, MotionType);
				int childCount = m_MotionContainer.childCount;
				if (childCount < motionCount) {
					for (int i = childCount; i < motionCount; i++) {
						var item = Instantiate(m_ItemPrefabs[MotionType], m_MotionContainer);
						item.IndexCount = GetPaletteCount();
						item.ItemIndex = ItemIndex;
						item.MotionType = MotionType;
						if (!item.gameObject.activeSelf) {
							item.gameObject.SetActive(true);
						}
					}
				} else if (childCount > motionCount) {
					m_MotionContainer.FixChildcountImmediately(motionCount);
				}
			} else {
				m_MotionContainer.DestroyAllChildImmediately();
			}
		}


		private void Update_UI () {
			// Field
			bool fieldActive = ItemType >= 0 && ItemIndex >= 0 && MotionItem.SelectingMotionIndex >= 0;
			if (m_ValueIF.interactable != fieldActive) {
				m_ValueIF.interactable = fieldActive;
			}
			if (m_TweenIF.interactable != fieldActive) {
				m_TweenIF.interactable = fieldActive;
			}
			if (m_ValueLabel.interactable != fieldActive) {
				m_ValueLabel.interactable = fieldActive;
			}
			if (m_TweenLabel.interactable != fieldActive) {
				m_TweenLabel.interactable = fieldActive;
			}
			if (MotionItem.SelectingMotionIndex != PrevSelectingMotionIndex) {
				PrevSelectingMotionIndex = MotionItem.SelectingMotionIndex;
				RefreshFieldUI();
				OnSelectionChanged();
			}
		}


		protected override void OnDisable () {
			base.OnDisable();
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			if (m_MotionContainer) {
				m_MotionContainer.DestroyAllChildImmediately();
			}
		}


		protected override void OnPopulateMesh (VertexHelper toFill) {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { toFill.Clear(); return; }
#endif
			toFill.Clear();

			var (contentStartY01, contentEndY01, realScrollValue, beatDuration) = GetContentStartYEndY();
			if (contentStartY01 >= contentEndY01) { return; }
			var map = GetBeatmap();
			float bps = GetBPM() / 60f;
			int beatCount = Mathf.CeilToInt(beatDuration) + 1;
			int division = BEAT_DIVs[DivisionIndex];
			var rect = GetPixelAdjustedRect();

			// Main BG
			float itemHeight01 = ITEM_HEIGHT / rect.height;
			int beatPerSection = GetBeatPerSection();
			int highlightBeat = -1;
			float yMin = float.MaxValue;
			float yMax = float.MinValue;
			for (int beat = Mathf.FloorToInt(realScrollValue * beatCount); beat < beatCount; beat++) {
				var (up, down, y01) = GetItemUpDown(rect, contentStartY01, contentEndY01, beat, beatCount);
				if (up < rect.y) { break; }
				if (Mouse.pos01.y >= y01 && Mouse.pos01.y <= y01 + itemHeight01) {
					highlightBeat = beat;
				}
				yMin = Mathf.Min(down, yMin);
				yMax = Mathf.Max(up, yMax);
				// Main
				SetCachePosition(rect.x, rect.x + rect.width, down, up);
				SetCacheUV(sprite);
				SetCacheColor(beat % 2 == 0 ? m_BgTintA : m_BgTintB);
				toFill.AddUIVertexQuad(VertexCache);
				// Label
				Sprite labelSP = null;
				int labelBeat = beat % (beatPerSection % 3 == 0 ? 12 : 16);
				if (labelBeat < 10) {
					labelSP = GetSprite((char)(labelBeat + '0'));
				} else {
					labelSP = GetSprite((char)(labelBeat - 10 + 'A'));
				}
				if (labelSP) {
					float labelGap = (ITEM_HEIGHT - LABEL_HEIGHT) / 2f;
					SetCachePosition(
						rect.x - 10f, rect.x - 10f + LABEL_WIDTH,
						down + labelGap, up - labelGap
					);
					SetCacheUV(labelSP);
					SetCacheColor(m_LabelTint);
					toFill.AddUIVertexQuad(VertexCache);
				}
				// Section || End
				bool endBeat = beat == beatCount - 1;
				if (endBeat || beat % beatPerSection == 0) {
					down = up - (endBeat ? 2f * SECTION_LINE_SIZE : SECTION_LINE_SIZE);
					SetCachePosition(rect.x, rect.x + rect.width, down, up);
					SetCacheUV(m_SectionLine);
					SetCacheColor(endBeat ? m_SectionTint_End : m_SectionTint);
					toFill.AddUIVertexQuad(VertexCache);
				}
			}
			HoveredBeat = highlightBeat;

			// Scroll Bar
			if (contentStartY01 < contentEndY01) {
				SetCachePosition(
					rect.x + rect.width + SCROLL_BAR_WIDTH,
					rect.x + rect.width + SCROLL_BAR_WIDTH * 2f,
					rect.y + rect.height * ((contentEndY01 - 0f) / (contentEndY01 - contentStartY01)),
					rect.y + rect.height * ((contentEndY01 - 1f) / (contentEndY01 - contentStartY01))
				);
				SetCacheUV(sprite);
				SetCacheColor(m_ScrollbarTint);
				toFill.AddUIVertexQuad(VertexCache);
			}

			// Div Line
			if (yMin < yMax) {
				SetCacheUV(m_Line);
				SetCacheColor(m_DivTint);
				for (int i = 1; i < division; i++) {
					float x = Mathf.Lerp(rect.x, rect.x + rect.width, GetItemLeftRight(rect, i).x01);
					SetCachePosition(x - DIV_LINE_SIZE, x + DIV_LINE_SIZE, yMin, yMax);
					toFill.AddUIVertexQuad(VertexCache);
				}
			}

			// Items
			int motionCount = map.GetMotionCount(ItemIndex, MotionType);
			if (motionCount > 0) {
				SetCacheUV(sprite);
				SetCacheColor(m_ItemTint);
				int beatStart = Mathf.FloorToInt(realScrollValue * beatCount);
				for (int motion = 0; motion < motionCount; motion++) {
					if (map.GetMotionTime(ItemIndex, MotionType, motion, out float time)) {
						float beatTime = time * bps;
						int beat = Mathf.FloorToInt(
							Mathf.Round(beatTime * division * 2f) / (division * 2f)
						);
						if (beat < beatStart) { continue; }
						var (up, down, _) = GetItemUpDown(rect, contentStartY01, contentEndY01, beat, beatCount);
						if (up < rect.y) { break; }
						var (left, right, _) = GetItemLeftRight(rect, (beatTime - beat) * division);
						SetCachePosition(left, right, down, up);
						if (motion == MotionItem.SelectingMotionIndex) {
							SetCacheColor(m_ItemTint_Highlight);
							toFill.AddUIVertexQuad(VertexCache);
							SetCacheColor(m_ItemTint);
						} else {
							toFill.AddUIVertexQuad(VertexCache);
						}
					}
				}
			}

			// Music Highlight
			float localMusicBeat = (GetMusicTime() - map.GetTime(ItemType, ItemIndex)) * bps;
			if (localMusicBeat >= 0f && localMusicBeat <= beatDuration) {
				var (up, down, _) = GetItemUpDown(rect, contentStartY01, contentEndY01, (int)localMusicBeat, beatCount);
				var (left, _, _) = GetItemLeftRight(rect, (localMusicBeat % 1f) * division);
				SetCacheUV(sprite);
				SetCacheColor(m_MusicHighlightTint);
				SetCachePosition(left, left + SECTION_LINE_SIZE, down, up);
				toFill.AddUIVertexQuad(VertexCache);
			}

			// Highlight
			int hoverDiv = -1;
			if (Mouse.active && highlightBeat >= 0) {
				hoverDiv = Mathf.FloorToInt(Mathf.Clamp(Mouse.pos01.x, 0f, 0.9999f) * division);
				var (up, down, _) = GetItemUpDown(rect, contentStartY01, contentEndY01, highlightBeat, beatCount);
				var (left, right, _) = GetItemLeftRight(rect, hoverDiv);
				SetCacheUV(sprite);
				SetCacheColor(m_HighlightTint);
				SetCachePosition(left, right, down, up);
				toFill.AddUIVertexQuad(VertexCache);
			}
			HoveredDiv = hoverDiv;

		}


		#endregion




		#region --- API ---


		public void SwitchDivition () {
			DivisionIndex = (DivisionIndex + 1) % BEAT_DIVs.Length;
			m_DivIMG.sprite = m_DivIcons[DivisionIndex];
			SetVerticesDirty();
		}


		public void TrySetDirty () {
			if (ItemType >= 0 && ItemIndex >= 0) {
				SetVerticesDirty();
			}
		}


		public void UI_OnValueEdit (string text) {
			if (!UIReady) { return; }
			var map = GetBeatmap();
			if (map != null && ItemType >= 0 && ItemIndex >= 0 && MotionItem.SelectingMotionIndex >= 0) {
				if (MotionType == 0) {
					// Vector2
					if (text.TryParseVector2ForInspector(out Vector2 value)) {
						Debug.Log("UI_OnValueEdit " + text + " " + value.ToString("0.000"));
						map.SetMotionValueTween(ItemIndex, MotionType, MotionItem.SelectingMotionIndex, value.x, value.y);


						var (hasA, hasB) = map.GetMotionValueTween(ItemIndex, MotionType, MotionItem.SelectingMotionIndex, out float vA, out float vB, out _);
						if (hasA && hasB) {
							Debug.Log("UI_OnValueEdit(Get) " + vA + " " + vB);
						}


					}
				} else {
					// Float, Int
					if (text.TryParseFloatForInspector(out float value)) {
						map.SetMotionValueTween(ItemIndex, MotionType, MotionItem.SelectingMotionIndex, value);
					}
				}
			}
			RefreshFieldUI();
		}


		public void UI_OnTweenEdit (string text) {
			if (!UIReady) { return; }
			if (text.TryParseIntForInspector(out int tween)) {
				TrySetCurrentTween(tween);
			} else {
				RefreshFieldUI();
			}
		}


		public void RefreshFieldUI () {
			UIReady = false;
			try {
				var map = GetBeatmap();
				string valueRes = "--";
				string tweenRes = "--";
				if (map != null && ItemType >= 0 && ItemIndex >= 0 && MotionItem.SelectingMotionIndex >= 0) {
					var (hasA, hasB) = map.GetMotionValueTween(ItemIndex, MotionType, MotionItem.SelectingMotionIndex, out float valueA, out float valueB, out int tween);
					if (MotionType == 0 && hasA && hasB) {
						// Vector2
						valueRes = $"{Mathf.Round(valueA * 100f) / 100f}, {Mathf.Round(valueB * 100f) / 100f}";
					}
					if (MotionType != 0 && hasA) {
						// Float, Int
						valueRes = (Mathf.Round(valueA * 1000f) / 1000f).ToString();
					}
					if (hasA || hasB) {
						// Tween
						tweenRes = tween.ToString();
					}
				}
				m_ValueIF.text = valueRes;
				m_TweenIF.text = tweenRes;
			} catch { }
			UIReady = true;
		}


		public void TrySetCurrentTween (int tween) {
			var map = GetBeatmap();
			if (map != null && ItemType >= 0 && ItemIndex >= 0 && MotionItem.SelectingMotionIndex >= 0) {
				map.SetMotionValueTween(ItemIndex, MotionType, MotionItem.SelectingMotionIndex, null, null, tween);
			}
			RefreshFieldUI();
		}


		#endregion




		#region --- LGC ---


		private void SetCacheUV (Sprite sp) {
			VertexCache[0].uv0 = sp.uv[2];
			VertexCache[1].uv0 = sp.uv[0];
			VertexCache[2].uv0 = sp.uv[1];
			VertexCache[3].uv0 = sp.uv[3];
		}


		private void SetCacheColor (Color tint) => VertexCache[0].color = VertexCache[1].color = VertexCache[2].color = VertexCache[3].color = color * tint;


		private void SetCachePosition (float left, float right, float down, float up) {
			VertexCache[0].position.x = left;
			VertexCache[1].position.x = left;
			VertexCache[2].position.x = right;
			VertexCache[3].position.x = right;
			VertexCache[0].position.y = down;
			VertexCache[1].position.y = up;
			VertexCache[2].position.y = up;
			VertexCache[3].position.y = down;
		}


		private (float start, float end, float realScroll, float beatDuration) GetContentStartYEndY () {
			var map = GetBeatmap();
			if (map == null || ItemType < 0 || MotionType < 0 || ItemIndex < 0) { return (0f, -1f, 0f, 0f); }
			float beatDuration = Mathf.Min(map.GetDuration(ItemType, ItemIndex), GetMusicDuration()) * (GetBPM() / 60f);
			int beatCount = Mathf.CeilToInt(beatDuration) + 1;
			var rect = GetPixelAdjustedRect();
			if (beatCount <= 0 || rect.height < 0.001f) { return (0f, -1f, 0f, 0f); }
			float contentHeight01 = rect.height / (beatCount * ITEM_HEIGHT);
			float realScrollValue = contentHeight01 < 1f ? Mathf.Lerp(0f, 1f - contentHeight01, ScrollValue) : 0f;
			float contentStartY01 = -realScrollValue / contentHeight01;
			float contentEndY01 = 1f / contentHeight01 + contentStartY01;
			return (contentStartY01, contentEndY01, realScrollValue, beatDuration);
		}


		private (float up, float down, float y01) GetItemUpDown (Rect rect, float contentStartY01, float contentEndY01, int beat, int beatCount) {
			float y01 = Mathf.LerpUnclamped(
				contentStartY01,
				contentEndY01,
				beatCount > 1 ? (float)beat / beatCount : 0f
			);
			float up = Mathf.LerpUnclamped(rect.y + rect.height, rect.y, y01);
			return (up, up - ITEM_HEIGHT, y01);
		}


		private (float left, float right, float x01) GetItemLeftRight (Rect rect, float div) {
			int division = BEAT_DIVs[DivisionIndex];
			float x01 = div / division;
			float left = Mathf.LerpUnclamped(rect.x, rect.x + rect.width, x01);
			float right = left + rect.width / division;
			return (left, right, x01);
		}


		private void Log (string subTitle) {
			if (!DebugLog.UseLog) { return; }
			DebugLog.LogFormat("MotionPainterUI", subTitle, false,
				("HoveredBeat", HoveredBeat),
				("HoveredDiv", HoveredDiv),
				("Beatmap", GetBeatmap()),
				("MotionType", MotionType),
				("ItemIndex", ItemIndex),
				("SelectingMotionIndex", MotionItem.SelectingMotionIndex)
			);
		}


		#endregion





	}
}




#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEditor;
	using UI;
	[CustomEditor(typeof(MotionPainterUI))]
	public class MotionPainterUIInspector : Editor {
		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged","m_Type","m_PreserveAspect",
			"m_FillCenter","m_FillMethod","m_FillAmount","m_FillClockwise","m_FillOrigin","m_UseSpriteMesh",
			"m_PixelsPerUnitMultiplier",
		};
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif