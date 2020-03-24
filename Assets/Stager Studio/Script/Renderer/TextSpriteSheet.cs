namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[CreateAssetMenu(fileName = "Text Sheet", menuName = "Stager/Text Sheet", order = 0)]
	public class TextSpriteSheet : ScriptableObject {



		public int Length => m_Sprites.Length;
		public Sprite this[int index] => m_Sprites[index];


		[SerializeField] private Sprite[] m_Sprites = null; // 0-9 a-z A-Z
		[SerializeField] private Sprite m_Empty = null;
		[SerializeField] private Sprite m_Slash = null;



		public Sprite Char_to_Sprite (char c) {
			switch (c) {
				case char _ when c - '0' < m_Sprites.Length && c >= '0' && c <= '9':
					return m_Sprites[c - '0'];
				case char _ when c - 'a' + 10 < m_Sprites.Length && c >= 'a' && c <= 'z':
					return m_Sprites[c - 'a' + 10];
				case char _ when c - 'A' + 36 < m_Sprites.Length && c >= 'A' && c <= 'Z':
					return m_Sprites[c - 'A' + 36];
				case ' ':
					return m_Empty;
				case '/':
					return m_Slash;


				default:
					return null;
			}



		}


	}
}