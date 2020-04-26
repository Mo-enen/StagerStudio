namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class TextRenderer : StageRenderer {



		// Handler
		public delegate Sprite CharToSpriteHandler (char c);

		// Api
		public static CharToSpriteHandler GetSprite { get; set; } = null;
		public string Text {
			get => m_Text;
			set {
				if (value != m_Text) {
					m_Text = value;
					SetDirty();
				}
			}
		}


		// Ser
		[SerializeField] private string m_Text = "";
		[SerializeField] private Color m_Background = default;




		private void OnEnable () {
			SetDirty();
		}


		protected override void OnMeshFill () {

			if (string.IsNullOrEmpty(m_Text)) { return; }
			var textLength = m_Text.Length;
			var offset = new Vector3(Pivot.x - Pivot.x * textLength, 0f, 0f);

			// BG
			if (m_Background.a > 0.005f) {
				Sprite sprite = GetSprite('\0');
				var uvMin = sprite.uv[2];
				var uvMax = sprite.uv[1];
				AddQuad01(
					0f, textLength, 0f, Scale.y,
					uvMin.x, uvMax.x, uvMin.y, uvMax.y,
					offset, m_Background
				);
			}

			// Text
			for (int i = 0; i < textLength; i++) {
				Sprite sprite = GetSprite(m_Text[i]);
				if (sprite == null) { continue; }
				var uvMin = sprite.uv[2];
				var uvMax = sprite.uv[1];
				AddQuad01(
					i, i + 1f, 0f, Scale.y,
					uvMin.x, uvMax.x, uvMin.y, uvMax.y,
					offset, Tint
				);
			}

		}




	}
}