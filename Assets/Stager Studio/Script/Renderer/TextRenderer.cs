namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class TextRenderer : StageRenderer {



		// Api
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
		[SerializeField] private TextSpriteSheet m_SpriteSheet = null;
		[SerializeField] private string m_Text = "";




		private void OnEnable () {
			SetDirty();
		}


		protected override void OnMeshFill () {

			if (m_SpriteSheet is null || string.IsNullOrEmpty(m_Text)) { return; }

			var textLength = m_Text.Length;
			for (int i = 0; i < textLength; i++) {
				Sprite sprite = m_SpriteSheet.Char_to_Sprite(m_Text[i]);
				if (sprite == null) { continue; }
				var uvMin = sprite.uv[2];
				var uvMax = sprite.uv[1];
				var offset = new Vector3(Pivot.x - Pivot.x * textLength, 0f, 0f);
				AddQuad01(
					i, i + 1f, 0f, Scale.y,
					uvMin.x, uvMax.x, uvMin.y, uvMax.y,
					offset
				);
			}


		}




	}
}