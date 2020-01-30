namespace UGUIPlus {
	using UnityEngine;
	using UnityEngine.UI;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine.EventSystems;


	[DisallowMultipleComponent]
	[AddComponentMenu("uGUI Plus/Image Plus")]
	public class ImagePlus : Image {


		// Short Cut
		public VertexColorHandler VertexColorHandler {
			get {
				return m_VertexColorHandler;
			}
		}

		public PositionOffsetHandler PositionOffsetHandler {
			get {
				return m_PositionOffsetHandler;
			}
		}

		public ShadowHandler ShadowHandler {
			get {
				return m_ShadowHandler;
			}
		}



		// Ser
		[SerializeField] VertexColorHandler m_VertexColorHandler = new VertexColorHandler();
		[SerializeField] PositionOffsetHandler m_PositionOffsetHandler = new PositionOffsetHandler();
		[SerializeField] ShadowHandler m_ShadowHandler = new ShadowHandler();



		protected override void OnPopulateMesh (VertexHelper toFill) {
			if (color.a > 0.001f) {
				base.OnPopulateMesh(toFill);
			} else {
				toFill.Clear();
			}
			m_VertexColorHandler.PopulateMesh(toFill, rectTransform, color);
			if (!sprite) {
				m_ShadowHandler.PopulateMesh(toFill, rectTransform);
			}
			m_PositionOffsetHandler.PopulateMesh(toFill, rectTransform);
		}








	}

}
