using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StagerStudio {
	public class StageGame : MonoBehaviour {














	}
}



#if UNITY_EDITOR
namespace StagerStudio.Stage.Editor {
	using UnityEditor;
	[CustomEditor(typeof(StageGame))]
	public class StageGame_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif