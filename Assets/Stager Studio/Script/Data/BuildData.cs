namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	[CreateAssetMenu(fileName = "Build Data", menuName = "Stager/Build Data", order = 1000)]
	public class BuildData : ScriptableObject {


		// VAR
		public string BuildTime = "";


		// API
		public void Refresh () {
			BuildTime = System.DateTime.Now.ToString("yyyy-MM-dd");
		}


		// EDT
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void Init () {
			foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t: BuildData")) {
				try {
					var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
					var build = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildData>(path);
					build.Refresh();
					UnityEditor.EditorUtility.SetDirty(build);
				} catch (System.Exception ex) {
					Debug.LogError(ex);
				}
			}
			UnityEditor.AssetDatabase.Refresh();
			UnityEditor.AssetDatabase.SaveAssets();
		}
#endif

	}
}