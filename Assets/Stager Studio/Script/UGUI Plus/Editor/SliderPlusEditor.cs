namespace StagerStudio.UGUIPlus {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEditor.UI;
	using UnityEngine;
	using UnityEngine.UI;

	[CustomEditor(typeof(SliderPlus))]
	[CanEditMultipleObjects]
	public class SliderPlusEditor : SelectableEditor {

		SerializedProperty m_Direction;
		SerializedProperty m_FillRect;
		SerializedProperty m_HandleRect;
		SerializedProperty m_MinValue;
		SerializedProperty m_MaxValue;
		SerializedProperty m_WholeNumbers;
		SerializedProperty m_Value;
		SerializedProperty m_OnValueChanged;

		// Plus
		private static bool m_EventPanelOpen = false;
		private static bool m_MinMaxSliderPanelOpen = false;

		// Event
		SerializedProperty m_OnPointerDownProperty;
		SerializedProperty m_OnPointerUpProperty;
		SerializedProperty m_OnPointerEnterProperty;
		SerializedProperty m_OnPointerExitProperty;
		SerializedProperty m_OnSelectProperty;
		SerializedProperty m_OnDeselectProperty;
		SerializedProperty m_OnClickProperty;
		// Min Max Slider
		SerializedProperty m_UseMinMaxSlider;
		SerializedProperty m_MinHandleRect;
		SerializedProperty m_LeftValue;


		protected override void OnEnable () {
			base.OnEnable();
			m_FillRect = serializedObject.FindProperty("m_FillRect");
			m_HandleRect = serializedObject.FindProperty("m_HandleRect");
			m_Direction = serializedObject.FindProperty("m_Direction");
			m_MinValue = serializedObject.FindProperty("m_MinValue");
			m_MaxValue = serializedObject.FindProperty("m_MaxValue");
			m_WholeNumbers = serializedObject.FindProperty("m_WholeNumbers");
			m_Value = serializedObject.FindProperty("m_Value");
			m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");

			// Plus
			m_OnPointerDownProperty = serializedObject.FindProperty("m_PointerEventHandler.m_OnPointerDown");
			m_OnPointerUpProperty = serializedObject.FindProperty("m_PointerEventHandler.m_OnPointerUp");
			m_OnPointerEnterProperty = serializedObject.FindProperty("m_PointerEventHandler.m_OnPointerEnter");
			m_OnPointerExitProperty = serializedObject.FindProperty("m_PointerEventHandler.m_OnPointerExit");
			m_OnSelectProperty = serializedObject.FindProperty("m_PointerEventHandler.m_OnSelect");
			m_OnDeselectProperty = serializedObject.FindProperty("m_PointerEventHandler.m_OnDeselect");
			m_OnClickProperty = serializedObject.FindProperty("m_PointerEventHandler.m_OnPointerClick");
			m_EventPanelOpen = EditorPrefs.GetBool("uGUIPlus.m_EventPanelOpen", m_EventPanelOpen);

			m_UseMinMaxSlider = serializedObject.FindProperty("m_UseMinMaxSlider");
			m_MinHandleRect = serializedObject.FindProperty("m_MinHandleRect");
			m_LeftValue = serializedObject.FindProperty("m_LeftValue");
			m_MinMaxSliderPanelOpen = EditorPrefs.GetBool("uGUIPlus.m_MinMaxSliderPanelOpen", m_MinMaxSliderPanelOpen);

		}


		public override void OnInspectorGUI () {

			EditorGUILayout.Space();

			serializedObject.Update();

			EditorGUILayout.PropertyField(m_FillRect);
			EditorGUILayout.PropertyField(m_HandleRect);

			if (m_FillRect.objectReferenceValue != null || m_HandleRect.objectReferenceValue != null) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_Direction);
				if (EditorGUI.EndChangeCheck()) {
					Slider.Direction direction = (Slider.Direction)m_Direction.enumValueIndex;
					foreach (var obj in serializedObject.targetObjects) {
						Slider slider = obj as Slider;
						slider.SetDirection(direction, true);
					}
				}

				EditorGUILayout.PropertyField(m_MinValue);
				EditorGUILayout.PropertyField(m_MaxValue);
				EditorGUILayout.PropertyField(m_WholeNumbers);
				EditorGUILayout.Slider(m_Value, m_UseMinMaxSlider.boolValue ? m_LeftValue.floatValue : m_MinValue.floatValue, m_MaxValue.floatValue);

				bool warning = false;
				foreach (var obj in serializedObject.targetObjects) {
					Slider slider = obj as Slider;
					Slider.Direction dir = slider.direction;
					if (dir == Slider.Direction.LeftToRight || dir == Slider.Direction.RightToLeft)
						warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnLeft() != null || slider.FindSelectableOnRight() != null));
					else
						warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnDown() != null || slider.FindSelectableOnUp() != null));
				}

				if (warning)
					EditorGUILayout.HelpBox("The selected slider direction conflicts with navigation. Not all navigation options may work.", MessageType.Warning);

				// Draw the event notification options
				EditorGUILayout.Space();
				//EditorGUILayout.PropertyField(m_OnValueChanged);
			} else {
				EditorGUILayout.HelpBox("Specify a RectTransform for the slider fill or the slider handle or both. Each must have a parent RectTransform that it can slide within.", MessageType.Info);
			}

			PlusGUI();
			serializedObject.ApplyModifiedProperties();
		}



		private void PlusGUI () {

			EditorUtil.SimpleGUI(
				"Event",
				ref m_EventPanelOpen,
				0f,
				m_OnPointerDownProperty,
				m_OnPointerUpProperty,
				m_OnPointerEnterProperty,
				m_OnPointerExitProperty,
				m_OnSelectProperty,
				m_OnDeselectProperty,
				m_OnClickProperty,
				m_OnValueChanged
			);

			EditorUtil.MinMaxSliderGUI(
				m_Value.floatValue,
				m_WholeNumbers.boolValue,
				m_UseMinMaxSlider,
				m_LeftValue,
				m_MinHandleRect,
				ref m_MinMaxSliderPanelOpen
			);

			foreach (var t in targets) {
				(t as SliderPlus).UpdateMinHandle();
				(t as SliderPlus).UpdateFill();
			}

			if (GUI.changed) {
				EditorPrefs.SetBool("uGUIPlus.m_EventPanelOpen", m_EventPanelOpen);
				EditorPrefs.SetBool("uGUIPlus.m_MinMaxSliderPanelOpen", m_MinMaxSliderPanelOpen);
			}
		}





	}
}
