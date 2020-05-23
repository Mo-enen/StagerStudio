namespace UIGadget {
	using UnityEngine;
	using UnityEngine.UI;


	[DisallowMultipleComponent]
	[AddComponentMenu("uGUI Plus/RaycastBlocker")]
	public class RaycastBlocker : Graphic {



		protected override void Reset () {
			raycastTarget = true;
		}



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



}
