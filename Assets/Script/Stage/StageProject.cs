using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StagerStudio.Stage {
	public class StageProject : MonoBehaviour {




		#region --- SUB ---


		// Handler
		public static StringStringHandler GetLanguage { get; set; } = null;


		#endregion




		#region --- VAR ---




		#endregion




		#region --- MSG ---




		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---




		#endregion




		#region --- UTL ---




		#endregion




	}
}



#if UNITY_EDITOR
namespace StagerStudio.Stage.Editor {
	using UnityEditor;
	[CustomEditor(typeof(StageProject))]
	public class StageProject_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif