namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class TextRenderer : StageRenderer {



		// Api
		public string Text {
			get => m_Text;
			set => m_Text = value;
		}


		// Ser
		[SerializeField] private Sprite[] m_SpriteSets = null;  // 0-9 A-Z a-z
		[SerializeField] private string m_Text = "";
		[SerializeField] private float m_Gap = 0.1f;



		private void Awake () {
			Pivot = new Vector3(0.5f, Pivot.y, Pivot.z);
		}


		protected override void OnMeshFill () {

			if (m_SpriteSets is null || string.IsNullOrEmpty(m_Text)) { return; }

			var textLength = m_Text.Length;
			for (int i = 0; i < textLength; i++) {
				int index = Char_to_Index(m_Text[i]);
				if (index < 0 || index >= m_SpriteSets.Length) { continue; }
				var sprite = m_SpriteSets[index];
				var uvMin = sprite.uv[2];
				var uvMax = sprite.uv[1];
				float l = i * (1f + m_Gap);
				var offset = new Vector3(-(textLength - 1) / 2f * (1f + m_Gap), 0f, 0f);
				AddQuad01(
					l, l + 1f, 0f, Scale.y,
					uvMin.x, uvMax.x, uvMin.y, uvMax.y,
					offset
				);
			}


		}



		private int Char_to_Index (char c) {
			if (c >= '0' && c <= '9') {
				return c - '0';
			} else if (c >= 'A' && c <= 'Z') {
				return c - 'A' + 10;
			} else if (c >= 'a' && c <= 'z') {
				return c - 'a' + 36;
			} else {
				return -1;
			}
		}



	}
}