namespace UIGadget.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	[CustomEditor(typeof(MountableLabel), true), CanEditMultipleObjects]
	public class MountableLabelInspector : Editor {



		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged",
		};
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
		}


	}
}