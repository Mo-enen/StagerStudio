namespace Moenen.Tools {
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Reflection;
	using System.IO;

	public class MoenenTools {




		// Clear Console
		// & alt   % ctrl   # Shift
		[MenuItem("Tools/Clear Console #&x")]
		public static void ClearAndReStage () {
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			var type = assembly.GetType("UnityEditorInternal.LogEntries");
			if (type == null) {
				type = assembly.GetType("UnityEditor.LogEntries");
			}
			var method = type.GetMethod("Clear");
			method.Invoke(new object(), null);
		}


		[MenuItem("Tools/Copy Names")]
		public static void CopySelectingNames () {
			string result = "";
			foreach (var obj in Selection.objects) {
				if (!(obj is GameObject g)) { continue; }
				result += $"{g.name}\n";
			}
			GUIUtility.systemCopyBuffer = result;
		}


		
	}


}