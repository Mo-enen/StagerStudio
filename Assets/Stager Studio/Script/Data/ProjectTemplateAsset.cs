namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public enum ProjectTemplateType {
		Stager = 0,
		Voez = 1,
		Dynamix = 2,
		Deemo = 3,
		Mania = 4,
		SDVX = 5,
		Phigros = 6,
		Arcaea = 7,

	}



	[CreateAssetMenu(fileName = "Template", menuName = "Stager/Project Template", order = 1000)]
	public class ProjectTemplateAsset : ScriptableObject {
		public ProjectTemplateType Type = ProjectTemplateType.Stager;
		public TextAsset Beatmap = null;
		public Texture2D Palette = null;
		public TextAsset Tween = null;
		public GeneData Gene = null;
	}
}




#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Data;



	[CustomEditor(typeof(ProjectTemplateAsset), true)]
	public class ProjectTemplateAsset_Inspector : Editor {


		private readonly static string[] Exclude = new string[] {
			"m_Script",
		};

		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
		}


	}
}
#endif