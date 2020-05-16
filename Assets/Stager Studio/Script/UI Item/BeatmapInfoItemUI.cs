namespace StagerStudio.UI {
	using UnityEngine;
	using UnityEngine.UI;
	using UIGadget;

	public class BeatmapInfoItemUI : Grabber {
		public string Tag { get; set; } = "";
		public int Level { get; set; } = 0;
		public long CreatedTime { get; set; } = 0;
		public Text[] LanguageTexts => m_LanguageTexts;
		public TriggerUI[] MenuTriggers => m_MenuTriggers;
		[SerializeField] private Text[] m_LanguageTexts = null;
		[SerializeField] private TriggerUI[] m_MenuTriggers = null;
	}
}