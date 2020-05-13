namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class MotionPainterUI : Image {





		#region --- VAR ---


		// Const
		private static readonly int[] BEAT_DIVs = { 4, 8, 16, };

		// Handler
		public delegate Beatmap BeatmapHandler ();
		public delegate float FloatHandler ();
		public delegate int IntHandler ();
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetBPM { get; set; } = null;
		public static IntHandler GetBeatPerSection { get; set; } = null;

		// Api
		public int ItemType { get; set; } = -1; // -1: None   0:Stage   1:Track
		public int ItemIndex { get; set; } = -1;
		public int MotionType { get; set; } = -1; // Stage: Pos Rot Width Height Color // Track: X Angle Width Color

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Image m_DivIMG = null;
		[SerializeField] private Sprite m_Line = null;
		[SerializeField] private Sprite m_SectionLine = null;
		[SerializeField] private Sprite[] m_DivIcons = null;
		[SerializeField] private Transform m_MotionContainer = null;
		[SerializeField] private float m_ItemHeight = 24f;
		[SerializeField] private float m_DivLineSize = 1f;
		[SerializeField] private float m_SectionLineSize = 1f;
		[SerializeField] private Color m_BgTintA = Color.white;
		[SerializeField] private Color m_BgTintB = Color.white;
		[SerializeField] private Color m_DivTint = Color.black;
		[SerializeField] private Color m_SectionTint = Color.black;
		[SerializeField] private Color m_HighlightTint = Color.green;

		// Data
		private static readonly UIVertex[] VertexCache = new UIVertex[4] { default, default, default, default };
		private (bool active, bool down, Vector3 pos, Vector2 pos01) Mouse = default;
		private float ScrollValue = 0f;
		private int DivisionIndex = 1;
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


		private void Update () {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			// Mouse
			var rt = transform as RectTransform;
			var pos01 = rt.Get01Position(Input.mousePosition, Camera);
			pos01.y = 1f - pos01.y;
			if (pos01.x >= 0f && pos01.x <= 1f && pos01.y >= 0f && pos01.y <= 1f) {
				bool getLeftDown = Input.GetMouseButton(0);
				if (!Mouse.active || Input.mousePosition != Mouse.pos || getLeftDown != Mouse.down) {
					SetVerticesDirty();
					Mouse = (true, getLeftDown, Input.mousePosition, pos01);
				}
				if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f) {
					ScrollValue = Mathf.Clamp01(ScrollValue - Input.mouseScrollDelta.y * 0.1f);
					SetVerticesDirty();
				}
			} else {
				if (Mouse.active || Mouse.down) {
					SetVerticesDirty();
					Mouse.active = false;
					Mouse.down = false;
				}
			}
			// Items
			// m_MotionContainer



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

			var (contentStartY01, contentEndY01) = GetContentStartYEndY();
			if (contentStartY01 >= contentEndY01) { return; }
			int beatCount = Mathf.CeilToInt(GetBeatmap().GetDuration(ItemType, ItemIndex) * (60f / GetBPM()));
			var rect = GetPixelAdjustedRect();

			// Main BG
			float itemHeight01 = m_ItemHeight / rect.height;
			int beatPerSection = GetBeatPerSection();
			int highlightBeat = -1;
			float yMin = float.MaxValue;
			float yMax = float.MinValue;
			for (int beat = 0; beat < beatCount; beat++) {
				float y01 = Mathf.LerpUnclamped(contentStartY01, contentEndY01, (float)beat / (beatCount - 1));
				float up = Mathf.LerpUnclamped(rect.y + rect.height, rect.y, y01);
				float down = up - m_ItemHeight;
				if (down > rect.y + rect.height || up < rect.y) { continue; }
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
				// Section
				if (beat % beatPerSection == 0) {
					down = up - m_SectionLineSize;
					SetCachePosition(rect.x, rect.x + rect.width, down, up);
					SetCacheUV(m_SectionLine);
					SetCacheColor(m_SectionTint);
					toFill.AddUIVertexQuad(VertexCache);
				}
			}

			// Div Line
			int div = BEAT_DIVs[DivisionIndex];
			if (yMin < yMax) {
				SetCacheUV(m_Line);
				SetCacheColor(m_DivTint);
				for (int i = 0; i < div; i++) {
					float x = Mathf.Lerp(rect.x, rect.x + rect.width, (float)i / div);
					SetCachePosition(x - m_DivLineSize, x + m_DivLineSize, yMin, yMax);
					toFill.AddUIVertexQuad(VertexCache);
				}
			}

			// Highlight
			if (Mouse.active && highlightBeat >= 0) {
				float y01 = Mathf.LerpUnclamped(contentStartY01, contentEndY01, (float)highlightBeat / (beatCount - 1));
				float up = Mathf.Lerp(rect.y + rect.height, rect.y, y01);
				float down = up - m_ItemHeight;
				float left = Mathf.Lerp(rect.x, rect.x + rect.width, Mathf.Floor(Mouse.pos01.x * div) / div);
				float right = left + rect.width / div;
				SetCacheUV(sprite);
				SetCacheColor(m_HighlightTint);
				SetCachePosition(left, right, down, up);
				toFill.AddUIVertexQuad(VertexCache);
			}


		}


		#endregion




		#region --- API ---


		public void SwitchDivition () {
			DivisionIndex = (DivisionIndex + 1) % BEAT_DIVs.Length;
			m_DivIMG.sprite = m_DivIcons[DivisionIndex];
			SetVerticesDirty();
		}


		#endregion




		#region --- LGC ---


		private void SetCacheUV (Sprite sp) {
			VertexCache[0].uv0 = sp.uv[3];
			VertexCache[1].uv0 = sp.uv[1];
			VertexCache[2].uv0 = sp.uv[0];
			VertexCache[3].uv0 = sp.uv[2];
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


		private (float start, float end) GetContentStartYEndY () {
			var map = GetBeatmap();
			if (map == null || ItemType < 0 || MotionType < 0 || ItemIndex < 0) { return (0f, -1f); }
			float beatDuration = map.GetDuration(ItemType, ItemIndex) * (60f / GetBPM());
			int beatCount = Mathf.CeilToInt(beatDuration);
			var rect = GetPixelAdjustedRect();
			if (beatCount <= 0 || rect.height < 0.001f) { return (0f, -1f); }
			float contentHeight01 = rect.height / (beatCount * m_ItemHeight);
			float realScrollValue = contentHeight01 < 1f ? Mathf.Lerp(0f, 1f - contentHeight01, ScrollValue) : 0f;
			float contentStartY01 = -realScrollValue / contentHeight01;
			float contentEndY01 = 1f / contentHeight01 + contentStartY01;
			return (contentStartY01, contentEndY01);
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