namespace Moenen.UIGadgets {
#if UNITY_EDITOR
	using UnityEditor;
#endif
	using UnityEngine;
	using UnityEngine.UI;
	 

	[DisallowMultipleComponent]
	[AddComponentMenu("uGUI Plus/RaycastBlocker")]
	public class RaycastBlocker : Graphic {


#if UNITY_EDITOR
		protected override void Reset () {
			raycastTarget = true;
		}
#endif


		protected override void Awake () {
			base.Awake();
			raycastTarget = true;
		}


		public override void SetMaterialDirty () { return; }

		public override void SetVerticesDirty () { return; }

		protected override void OnPopulateMesh (VertexHelper vh) {
			vh.Clear();
			return;
		}


	}


#if UNITY_EDITOR
	[CustomEditor(typeof(RaycastBlocker))]
	public class RaycastBlockerEditor : Editor {
		public override void OnInspectorGUI () { }
	}
#endif

}