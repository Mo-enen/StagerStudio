namespace StagerStudio.UGUIPlus {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using UnityEngine.UI;

	public static class EditorUtil {



		#region --- API ---

		
		// Handler GUI
		public static void SimpleGUI (string title, ref bool m_PanelOpen, float space, params SerializedProperty[] sps) {
			LayoutF(() => {
				foreach (var s in sps) {
					if (s != null) {
						EditorGUILayout.PropertyField(s);
					}
				}
			}, title, ref m_PanelOpen, true);
		}


		public static void SimpleUseGUI (string title, ref bool m_PanelOpen, float space, SerializedProperty useThis, params SerializedProperty[] sps) {
			LayoutF(() => {
				EditorGUILayout.PropertyField(useThis);
				if (useThis.boolValue) {
					foreach (var s in sps) {
						if (s != null) {
							EditorGUILayout.PropertyField(s);
						}
					}
				}
			}, title, ref m_PanelOpen, true);
		}


		public static void VertexColorGUI (SerializedProperty m_UseVertexColor, SerializedProperty m_VertexTopLeft, SerializedProperty m_VertexTopRight, SerializedProperty m_VertexBottomLeft, SerializedProperty m_VertexBottomRight, SerializedProperty m_VertexColorFilter, SerializedProperty m_VertexColorOffset, ref bool m_VertexColorPanelOpen) {
			LayoutF(() => {
				EditorGUILayout.PropertyField(m_UseVertexColor);
				if (m_UseVertexColor.boolValue) {
					Space();
					LayoutH(() => {
						EditorGUI.PropertyField(GUIRect(0, 18), m_VertexTopLeft, new GUIContent());
						Space();
						EditorGUI.PropertyField(GUIRect(0, 18), m_VertexTopRight, new GUIContent());
					});
					Space();
					LayoutH(() => {
						EditorGUI.PropertyField(GUIRect(0, 18), m_VertexBottomLeft, new GUIContent());
						Space();
						EditorGUI.PropertyField(GUIRect(0, 18), m_VertexBottomRight, new GUIContent());
					});
					Space();
					m_VertexColorFilter.enumValueIndex = (int)(VertexColorHandler.ColorFilterType)EditorGUILayout.EnumPopup(
						new GUIContent("Filter"), (VertexColorHandler.ColorFilterType)m_VertexColorFilter.enumValueIndex
					);
					Vector2 newOffset = EditorGUILayout.Vector2Field("Offset", m_VertexColorOffset.vector2Value);
					newOffset.x = Mathf.Clamp(newOffset.x, -1f, 1f);
					newOffset.y = Mathf.Clamp(newOffset.y, -1f, 1f);
					m_VertexColorOffset.vector2Value = newOffset;
					Space();
				}
			}, "Vertex Color", ref m_VertexColorPanelOpen, true);
		}


		public static void MinMaxSliderGUI (float value, bool wholeNumber, SerializedProperty m_UseMinMaxSlider, SerializedProperty m_LeftValue, SerializedProperty m_MinHandleRect, ref bool MinMaxSliderPanelOpen) {
			LayoutF(() => {
				EditorGUILayout.PropertyField(m_UseMinMaxSlider);
				if (m_UseMinMaxSlider.boolValue) {
					EditorGUILayout.Slider(m_LeftValue, 0f, value);
					if (wholeNumber) {
						m_LeftValue.floatValue = Mathf.Round(m_LeftValue.floatValue);
					}
					EditorGUILayout.PropertyField(m_MinHandleRect);
				}
			}, "Min-Max Slider", ref MinMaxSliderPanelOpen, true);
		}




		#endregion




		#region --- UTL ---


		private static Rect GUIRect (float width, float height) {
			return GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(width <= 0), GUILayout.ExpandHeight(height <= 0));
		}


		private static void Space (float space = 4f) {
			GUILayout.Space(space);
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


		private static void LayoutH (System.Action action, bool box = false) {
			if (box) {
				GUIStyle style = new GUIStyle(GUI.skin.box);
				GUILayout.BeginHorizontal(style);
			} else {
				GUILayout.BeginHorizontal();
			}
			action();
			GUILayout.EndHorizontal();
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


		private static void ResetInCanvasFor (RectTransform root) {
			root.SetParent(Selection.activeTransform);
			if (!InCanvas(root)) {
				Transform canvasTF = GetCreateCanvas();
				root.SetParent(canvasTF);
			}
			if (!Transform.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()) {
				GameObject eg = new GameObject("EventSystem");
				eg.AddComponent<UnityEngine.EventSystems.EventSystem>();
				eg.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
			}
			root.localScale = Vector3.one;
			root.localPosition = new Vector3(root.localPosition.x, root.localPosition.y, 0f);
			Selection.activeGameObject = root.gameObject;
		}


		private static bool InCanvas (Transform tf) {
			while (tf.parent) {
				tf = tf.parent;
				if (tf.GetComponent<Canvas>()) {
					return true;
				}
			}
			return false;
		}


		private static Transform GetCreateCanvas () {
			Canvas c = Object.FindObjectOfType<Canvas>();
			if (c) {
				return c.transform;
			} else {
				GameObject g = new GameObject("Canvas");
				c = g.AddComponent<Canvas>();
				c.renderMode = RenderMode.ScreenSpaceOverlay;
				g.AddComponent<CanvasScaler>();
				g.AddComponent<GraphicRaycaster>();
				return g.transform;
			}
		}



		#endregion


	}



	public struct SpriteUtil {


		private const string STANDARD_SPRITE_PATH = "UI/Skin/UISprite.psd";
		private const string BACKGROUND_SPRITE_PATH = "UI/Skin/Background.psd";
		private const string INPUTFIELD_BACKGROUND_PATH = "UI/Skin/InputFieldBackground.psd";
		private const string KNOB_PATH = "UI/Skin/Knob.psd";
		private const string CHECKMARK_PATH = "UI/Skin/Checkmark.psd";
		private const string DROPDOWN_ARROW_PATH = "UI/Skin/DropdownArrow.psd";
		private const string DUI_MASK_PATH = "UI/Skin/UIMask.psd";

		public static Sprite UISprite {
			get {
				if (!uiSprite) {
					uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(STANDARD_SPRITE_PATH);
				}
				return uiSprite;
			}
		}
		private static Sprite uiSprite = null;

		public static Sprite BackGroundSprite {
			get {
				if (!backGroundSprite) {
					backGroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BACKGROUND_SPRITE_PATH);
				}
				return backGroundSprite;
			}
		}
		private static Sprite backGroundSprite = null;

		public static Sprite InputFieldBackGroundSprite {
			get {
				if (!inputFieldBackGroundSprite) {
					inputFieldBackGroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(INPUTFIELD_BACKGROUND_PATH);
				}
				return inputFieldBackGroundSprite;
			}
		}
		private static Sprite inputFieldBackGroundSprite = null;

		public static Sprite KnobSprite {
			get {
				if (!knobSprite) {
					knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(KNOB_PATH);
				}
				return knobSprite;
			}
		}
		private static Sprite knobSprite = null;

		public static Sprite CheckMarkSprite {
			get {
				if (!checkMarkSprite) {
					checkMarkSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(CHECKMARK_PATH);
				}
				return checkMarkSprite;
			}
		}
		private static Sprite checkMarkSprite = null;

		public static Sprite DropdownArrowSprite {
			get {
				if (!dropdownArrow) {
					dropdownArrow = AssetDatabase.GetBuiltinExtraResource<Sprite>(DROPDOWN_ARROW_PATH);
				}
				return dropdownArrow;
			}
		}
		private static Sprite dropdownArrow = null;

		public static Sprite UIMaskSprite {
			get {
				if (!uiMaskSprite) {
					uiMaskSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(DUI_MASK_PATH);
				}
				return uiMaskSprite;
			}
		}
		private static Sprite uiMaskSprite = null;



	}




}
