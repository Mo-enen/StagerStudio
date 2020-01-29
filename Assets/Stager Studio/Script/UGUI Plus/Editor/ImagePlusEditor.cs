namespace StagerStudio.UGUIPlus {
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.AnimatedValues;
	using UnityEngine.UI;
	using UnityEditor.UI;



	[CustomEditor(typeof(ImagePlus), true)]
	[CanEditMultipleObjects]
	public class ImagePlusEditor : GraphicEditor {

		private static bool m_VertexColorPanelOpen = false;
		private static bool m_PositionOffsetPanelOpen = false;
		private static bool m_ShadowPanelOpen = false;

		SerializedProperty m_FillMethod;
		SerializedProperty m_FillOrigin;
		SerializedProperty m_FillAmount;
		SerializedProperty m_FillClockwise;
		SerializedProperty m_Type;
		SerializedProperty m_FillCenter;
		SerializedProperty m_Sprite;
		SerializedProperty m_PreserveAspect;
		// Plus
		SerializedProperty m_VertexColorFilter;
		SerializedProperty m_VertexColorOffset;
		SerializedProperty m_VertexTopLeft;
		SerializedProperty m_VertexTopRight;
		SerializedProperty m_VertexBottomLeft;
		SerializedProperty m_VertexBottomRight;
		SerializedProperty m_PositionOffsetTopLeft;
		SerializedProperty m_PositionOffsetTopRight;
		SerializedProperty m_PositionOffsetBottomLeft;
		SerializedProperty m_PositionOffsetBottomRight;
		SerializedProperty m_ShadowColor;
		SerializedProperty m_ShadowClearColor;
		SerializedProperty m_ShadowLeft;
		SerializedProperty m_ShadowRight;
		SerializedProperty m_ShadowTop;
		SerializedProperty m_ShadowBottom;
		SerializedProperty m_UseShadow;
		SerializedProperty m_SharpCorner;
		SerializedProperty m_UseVertexColor;
		SerializedProperty m_UsePositionOffset;
		// End Plus
		GUIContent m_SpriteContent;
		GUIContent m_SpriteTypeContent;
		GUIContent m_ClockwiseContent;
		AnimBool m_ShowSlicedOrTiled;
		AnimBool m_ShowSliced;
		AnimBool m_ShowFilled;
		AnimBool m_ShowType;


		protected override void OnEnable () {
			base.OnEnable();

			m_SpriteContent = new GUIContent("Source Image");
			m_SpriteTypeContent = new GUIContent("Image Type");
			m_ClockwiseContent = new GUIContent("Clockwise");

			m_Sprite = serializedObject.FindProperty("m_Sprite");
			m_Type = serializedObject.FindProperty("m_Type");
			m_FillCenter = serializedObject.FindProperty("m_FillCenter");
			m_FillMethod = serializedObject.FindProperty("m_FillMethod");
			m_FillOrigin = serializedObject.FindProperty("m_FillOrigin");
			m_FillClockwise = serializedObject.FindProperty("m_FillClockwise");
			m_FillAmount = serializedObject.FindProperty("m_FillAmount");
			m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

			// Plus
			// VertexColor
			m_UseVertexColor = serializedObject.FindProperty("m_VertexColorHandler.m_UseVertexColor");
			m_VertexColorFilter = serializedObject.FindProperty("m_VertexColorHandler.m_VertexColorFilter");
			m_VertexTopLeft = serializedObject.FindProperty("m_VertexColorHandler.m_VertexTopLeft");
			m_VertexTopRight = serializedObject.FindProperty("m_VertexColorHandler.m_VertexTopRight");
			m_VertexBottomLeft = serializedObject.FindProperty("m_VertexColorHandler.m_VertexBottomLeft");
			m_VertexBottomRight = serializedObject.FindProperty("m_VertexColorHandler.m_VertexBottomRight");
			m_VertexColorOffset = serializedObject.FindProperty("m_VertexColorHandler.m_VertexColorOffset");
			// Pos Offset
			m_UsePositionOffset = serializedObject.FindProperty("m_PositionOffsetHandler.m_UsePositionOffset");
			m_PositionOffsetTopLeft = serializedObject.FindProperty("m_PositionOffsetHandler.m_TopLeft");
			m_PositionOffsetTopRight = serializedObject.FindProperty("m_PositionOffsetHandler.m_TopRight");
			m_PositionOffsetBottomLeft = serializedObject.FindProperty("m_PositionOffsetHandler.m_BottomLeft");
			m_PositionOffsetBottomRight = serializedObject.FindProperty("m_PositionOffsetHandler.m_BottomRight");
			// Shadow
			m_UseShadow = serializedObject.FindProperty("m_ShadowHandler.m_UseShadow");
			m_ShadowColor = serializedObject.FindProperty("m_ShadowHandler.m_Color");
			m_ShadowClearColor = serializedObject.FindProperty("m_ShadowHandler.m_ClearColor");
			m_ShadowLeft = serializedObject.FindProperty("m_ShadowHandler.m_Left");
			m_ShadowRight = serializedObject.FindProperty("m_ShadowHandler.m_Right");
			m_ShadowTop = serializedObject.FindProperty("m_ShadowHandler.m_Top");
			m_ShadowBottom = serializedObject.FindProperty("m_ShadowHandler.m_Bottom");
			m_SharpCorner = serializedObject.FindProperty("m_ShadowHandler.m_SharpCorner");

			// Panel Open
			m_VertexColorPanelOpen = EditorPrefs.GetBool("uGUIPlus.m_VertexColorPanelOpen", m_VertexColorPanelOpen);
			m_PositionOffsetPanelOpen = EditorPrefs.GetBool("uGUIPlus.m_PositionOffsetPanelOpen", m_PositionOffsetPanelOpen);
			m_ShadowPanelOpen = EditorPrefs.GetBool("uGUIPlus.m_ShadowPanelOpen", m_ShadowPanelOpen);
			// End Plus

			m_ShowType = new AnimBool(m_Sprite.objectReferenceValue != null);
			m_ShowType.valueChanged.AddListener(Repaint);

			var typeEnum = (Image.Type)m_Type.enumValueIndex;

			m_ShowSlicedOrTiled = new AnimBool(!m_Type.hasMultipleDifferentValues && typeEnum == Image.Type.Sliced);
			m_ShowSliced = new AnimBool(!m_Type.hasMultipleDifferentValues && typeEnum == Image.Type.Sliced);
			m_ShowFilled = new AnimBool(!m_Type.hasMultipleDifferentValues && typeEnum == Image.Type.Filled);
			m_ShowSlicedOrTiled.valueChanged.AddListener(Repaint);
			m_ShowSliced.valueChanged.AddListener(Repaint);
			m_ShowFilled.valueChanged.AddListener(Repaint);

			SetShowNativeSize(true);
		}

		protected override void OnDisable () {
			m_ShowType.valueChanged.RemoveListener(Repaint);
			m_ShowSlicedOrTiled.valueChanged.RemoveListener(Repaint);
			m_ShowSliced.valueChanged.RemoveListener(Repaint);
			m_ShowFilled.valueChanged.RemoveListener(Repaint);
		}

		public override void OnInspectorGUI () {

			serializedObject.Update();

			SpriteGUI();
			AppearanceControlsGUI();
			RaycastControlsGUI();

			m_ShowType.target = m_Sprite.objectReferenceValue != null;
			if (EditorGUILayout.BeginFadeGroup(m_ShowType.faded))
				TypeGUI();
			EditorGUILayout.EndFadeGroup();

			SetShowNativeSize(false);
			if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded)) {
				//EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(m_PreserveAspect);
				//EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFadeGroup();
			NativeSizeButtonGUI();

			// Plus
			PlusGUI();

			serializedObject.ApplyModifiedProperties();
		}


		void PlusGUI () {
			EditorUtil.VertexColorGUI(
				m_UseVertexColor,
				m_VertexTopLeft,
				m_VertexTopRight,
				m_VertexBottomLeft,
				m_VertexBottomRight,
				m_VertexColorFilter,
				m_VertexColorOffset,
				ref m_VertexColorPanelOpen
			);
			EditorUtil.SimpleUseGUI(
				"Position Offset",
				ref m_PositionOffsetPanelOpen,
				4f,
				m_UsePositionOffset,
				m_PositionOffsetTopLeft,
				m_PositionOffsetTopRight,
				m_PositionOffsetBottomLeft,
				m_PositionOffsetBottomRight
			);
			if ((target as Image).sprite) {
				LayoutF(() => {
					EditorGUILayout.HelpBox("Shadow only work without source image.", MessageType.Info);
				}, "Shadow", ref m_ShadowPanelOpen, true);
			} else {
				EditorUtil.SimpleUseGUI(
					"Shadow",
					ref m_ShadowPanelOpen,
					0f,
					m_UseShadow,
					m_SharpCorner,
					m_ShadowColor,
					m_ShadowClearColor,
					m_ShadowLeft,
					m_ShadowRight,
					m_ShadowTop,
					m_ShadowBottom
				);
			}

			if (GUI.changed) {
				EditorPrefs.SetBool("uGUIPlus.m_VertexColorPanelOpen", m_VertexColorPanelOpen);
				EditorPrefs.SetBool("uGUIPlus.m_PositionOffsetPanelOpen", m_PositionOffsetPanelOpen);
				EditorPrefs.SetBool("uGUIPlus.m_ShadowPanelOpen", m_ShadowPanelOpen);

			}
		}


		void SetShowNativeSize (bool instant) {
			Image.Type type = (Image.Type)m_Type.enumValueIndex;
			bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled);
			base.SetShowNativeSize(showNativeSize, instant);
		}

		/// <summary>
		/// Draw the atlas and Image selection fields.
		/// </summary>

		protected void SpriteGUI () {
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(m_Sprite, m_SpriteContent);
			if (EditorGUI.EndChangeCheck()) {
				var newSprite = m_Sprite.objectReferenceValue as Sprite;
				if (newSprite) {
					Image.Type oldType = (Image.Type)m_Type.enumValueIndex;
					if (newSprite.border.SqrMagnitude() > 0) {
						m_Type.enumValueIndex = (int)Image.Type.Sliced;
					} else if (oldType == Image.Type.Sliced) {
						m_Type.enumValueIndex = (int)Image.Type.Simple;
					}
				}
			}
		}

		/// <summary>
		/// Sprites's custom properties based on the type.
		/// </summary>

		protected void TypeGUI () {
			EditorGUILayout.PropertyField(m_Type, m_SpriteTypeContent);

			++EditorGUI.indentLevel;
			{
				Image.Type typeEnum = (Image.Type)m_Type.enumValueIndex;

				bool showSlicedOrTiled = (!m_Type.hasMultipleDifferentValues && (typeEnum == Image.Type.Sliced || typeEnum == Image.Type.Tiled));
				if (showSlicedOrTiled && targets.Length > 1)
					showSlicedOrTiled = targets.Select(obj => obj as Image).All(img => img.hasBorder);

				m_ShowSlicedOrTiled.target = showSlicedOrTiled;
				m_ShowSliced.target = (showSlicedOrTiled && !m_Type.hasMultipleDifferentValues && typeEnum == Image.Type.Sliced);
				m_ShowFilled.target = (!m_Type.hasMultipleDifferentValues && typeEnum == Image.Type.Filled);

				Image image = target as Image;
				if (EditorGUILayout.BeginFadeGroup(m_ShowSlicedOrTiled.faded)) {
					if (image.hasBorder)
						EditorGUILayout.PropertyField(m_FillCenter);
				}
				EditorGUILayout.EndFadeGroup();

				if (EditorGUILayout.BeginFadeGroup(m_ShowSliced.faded)) {
					if (image.sprite != null && !image.hasBorder)
						EditorGUILayout.HelpBox("This Image doesn't have a border.", MessageType.Warning);
				}
				EditorGUILayout.EndFadeGroup();

				if (EditorGUILayout.BeginFadeGroup(m_ShowFilled.faded)) {
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(m_FillMethod);
					if (EditorGUI.EndChangeCheck()) {
						m_FillOrigin.intValue = 0;
					}
					switch ((Image.FillMethod)m_FillMethod.enumValueIndex) {
						case Image.FillMethod.Horizontal:
							m_FillOrigin.intValue = (int)(Image.OriginHorizontal)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginHorizontal)m_FillOrigin.intValue);
							break;
						case Image.FillMethod.Vertical:
							m_FillOrigin.intValue = (int)(Image.OriginVertical)EditorGUILayout.EnumPopup("Fill Origin", (Image.OriginVertical)m_FillOrigin.intValue);
							break;
						case Image.FillMethod.Radial90:
							m_FillOrigin.intValue = (int)(Image.Origin90)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin90)m_FillOrigin.intValue);
							break;
						case Image.FillMethod.Radial180:
							m_FillOrigin.intValue = (int)(Image.Origin180)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin180)m_FillOrigin.intValue);
							break;
						case Image.FillMethod.Radial360:
							m_FillOrigin.intValue = (int)(Image.Origin360)EditorGUILayout.EnumPopup("Fill Origin", (Image.Origin360)m_FillOrigin.intValue);
							break;
					}
					EditorGUILayout.PropertyField(m_FillAmount);
					if ((Image.FillMethod)m_FillMethod.enumValueIndex > Image.FillMethod.Vertical) {
						EditorGUILayout.PropertyField(m_FillClockwise, m_ClockwiseContent);
					}
				}
				EditorGUILayout.EndFadeGroup();
			}
			--EditorGUI.indentLevel;
		}

		/// <summary>
		/// All graphics have a preview.
		/// </summary>

		public override bool HasPreviewGUI () { return true; }

		/// <summary>
		/// Draw the Image preview.
		/// </summary>

		public override void OnPreviewGUI (Rect rect, GUIStyle background) {
			Image image = target as Image;
			if (image == null)
				return;

			Sprite sf = image.sprite;
			if (sf == null)
				return;

			//SpriteDrawUtility.DrawSprite(sf, rect, image.canvasRenderer.GetColor());
		}

		/// <summary>
		/// Info String drawn at the bottom of the Preview
		/// </summary>

		public override string GetInfoString () {
			Image image = target as Image;
			Sprite sprite = image.sprite;

			int x = (sprite != null) ? Mathf.RoundToInt(sprite.rect.width) : 0;
			int y = (sprite != null) ? Mathf.RoundToInt(sprite.rect.height) : 0;

			return string.Format("Image Size: {0}x{1}", x, y);
		}



		private static void LayoutV (System.Action action, bool box = false) {
			if (box) {
				GUIStyle style = new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(6, 6, 2, 2)
				};
				GUILayout.BeginVertical(style);
			} else {
				GUILayout.BeginVertical();
			}
			action();
			GUILayout.EndVertical();
		}


		private static void LayoutF (System.Action action, string label, ref bool open, bool box = false) {
			bool _open = open;
			LayoutV(() => {
				_open = GUILayout.Toggle(
					_open,
					label,
					GUI.skin.GetStyle("foldout"),
					GUILayout.ExpandWidth(true),
					GUILayout.Height(18)
				);
				if (_open) {
					action();
				}
			}, box);
			open = _open;
		}


	}
}
