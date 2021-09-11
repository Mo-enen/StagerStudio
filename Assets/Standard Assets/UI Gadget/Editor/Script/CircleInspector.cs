namespace UIGadget.Editor {
	using UnityEngine;
	using UnityEditor;
	using UIGadget;
	[CustomEditor(typeof(Circle), true), CanEditMultipleObjects]
	public class CircleInspector : Editor {

		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged","m_Sprite","m_Type","m_PreserveAspect",
			"m_FillCenter","m_FillMethod","m_FillAmount","m_FillClockwise","m_FillOrigin","m_UseSpriteMesh",
			"m_PixelsPerUnitMultiplier",
		};

		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
		}

		public override bool HasPreviewGUI () => false;


	}
}