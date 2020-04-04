namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[CreateAssetMenu(fileName = "Text Sheet", menuName = "Stager/Text Sheet", order = 1000)]
	public class TextSpriteSheet : ScriptableObject {



		public int Length => m_Sprites.Length;
		public Sprite this[int index] => m_Sprites[index];



		[SerializeField] private Sprite[] m_Sprites = null;     // 0-9 a-z A-Z
		[SerializeField] private Sprite m_Empty = null;
		[SerializeField] private Sprite m_Plus = null;          // +
		[SerializeField] private Sprite m_Percentage = null;    // %
		[SerializeField] private Sprite[] m_Arrows = null;      // u d l r
		[SerializeField] private Sprite[] m_KeyChars = null;    //,./;'[]-=\`
		[SerializeField] private bool m_LowerOnly = false;


		public Sprite Char_to_Sprite (char c) {
			switch (c) {
				case char _ when c - '0' < m_Sprites.Length && c >= '0' && c <= '9':
					return m_Sprites[c - '0'];
				case char _ when c - 'a' + 10 < m_Sprites.Length && c >= 'a' && c <= 'z':
					return m_Sprites[c - 'a' + 10];
				case char _ when c - 'A' + (m_LowerOnly ? 10 : 36) < m_Sprites.Length && c >= 'A' && c <= 'Z':
					return m_Sprites[c - 'A' + (m_LowerOnly ? 10 : 36)];
				case ' ':
					return m_Empty;
				case '↑':
					return m_Arrows[0];
				case '↓':
					return m_Arrows[1];
				case '←':
					return m_Arrows[2];
				case '→':
					return m_Arrows[3];
				case '+':
					return m_Plus;
				case '%':
					return m_Percentage;

				case ',':
					return m_KeyChars[0];
				case '.':
					return m_KeyChars[1];
				case '/':
					return m_KeyChars[2];
				case ';':
					return m_KeyChars[3];
				case '\'':
					return m_KeyChars[4];
				case '[':
					return m_KeyChars[5];
				case ']':
					return m_KeyChars[6];
				case '-':
					return m_KeyChars[7];
				case '=':
					return m_KeyChars[8];
				case '\\':
					return m_KeyChars[9];
				case '`':
					return m_KeyChars[10];


				default:
					return null;
			}



		}


	}
}