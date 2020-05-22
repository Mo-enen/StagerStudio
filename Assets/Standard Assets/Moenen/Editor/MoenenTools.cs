namespace Moenen.Tools {
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Reflection;
	using System.IO;
	using UnityEditor.SceneManagement;


	public class MoenenTools {




		// Clear Console
		// & alt   % ctrl   # Shift
		[MenuItem("Tools/Do the Thing _F5")]
		public static void ClearAndReStage () {
			// Clear Console
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			var type = assembly.GetType("UnityEditorInternal.LogEntries");
			if (type == null) {
				type = assembly.GetType("UnityEditor.LogEntries");
			}
			var method = type.GetMethod("Clear");
			method.Invoke(new object(), null);
			// Deselect
			Selection.activeObject = null;
			// Save
			if (!EditorApplication.isPlaying) {
				EditorSceneManager.SaveOpenScenes();
			}
		}








	}


}