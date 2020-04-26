namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[CreateAssetMenu(fileName = "Text Sheet", menuName = "Stager/Text Sheet", order = 1000)]
	public class TextSpriteSheet : ScriptableObject {


		// Api
		public readonly Dictionary<char, Sprite> CharSpriteMap = new Dictionary<char, Sprite>();

		// Ser
		[SerializeField] private Sprite[] m_Numbers = null;     // 0-9
		[SerializeField] private Sprite[] m_LettersLower = null;     // a-z
		[SerializeField] private Sprite[] m_LettersChap = null;     // A-Z
		[SerializeField] private Sprite m_Empty = null;
		[SerializeField] private Sprite m_Plus = null;          // +
		[SerializeField] private Sprite m_Pixel = null;
		[SerializeField] private Sprite m_Percentage = null;    // %
		[SerializeField] private Sprite[] m_Arrows = null;      // u d l r
		[SerializeField] private Sprite[] m_KeyChars = null;    //,./;'[]-=\`



		public void Init () {
			CharSpriteMap.Clear();
			CharSpriteMap.Add('\0', m_Pixel);
			CharSpriteMap.Add(',', m_KeyChars[0]);
			CharSpriteMap.Add('.', m_KeyChars[1]);
			CharSpriteMap.Add('/', m_KeyChars[2]);
			CharSpriteMap.Add(';', m_KeyChars[3]);
			CharSpriteMap.Add('\'', m_KeyChars[4]);
			CharSpriteMap.Add('[', m_KeyChars[5]);
			CharSpriteMap.Add(']', m_KeyChars[6]);
			CharSpriteMap.Add('-', m_KeyChars[7]);
			CharSpriteMap.Add('=', m_KeyChars[8]);
			CharSpriteMap.Add('\\', m_KeyChars[9]);
			CharSpriteMap.Add('`', m_KeyChars[10]);
			CharSpriteMap.Add(' ', m_Empty);
			CharSpriteMap.Add('↑', m_Arrows[0]);
			CharSpriteMap.Add('↓', m_Arrows[1]);
			CharSpriteMap.Add('←', m_Arrows[2]);
			CharSpriteMap.Add('→', m_Arrows[3]);
			CharSpriteMap.Add('+', m_Plus);
			CharSpriteMap.Add('%', m_Percentage);
			for (char c = '0'; c <= '9'; c++) {
				CharSpriteMap.Add(c, m_Numbers[c - '0']);
			}
			for (char c = 'a'; c <= 'z'; c++) {
				CharSpriteMap.Add(c, m_LettersLower[c - 'a']);
			}
			for (char c = 'A'; c <= 'Z'; c++) {
				CharSpriteMap.Add(c, m_LettersChap[c - 'A']);
			}
		}



		public Sprite Char_to_Sprite (char c) => CharSpriteMap.ContainsKey(c) ? CharSpriteMap[c] : null;


	}
}