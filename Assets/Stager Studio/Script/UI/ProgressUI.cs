namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using Stage;
	using Saving;


	public class ProgressUI : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {


		// SUB
		public delegate float FloatFloatIntHandler (float musicTime, int step);


		// API
		public static FloatFloatIntHandler GetSnapTime { get; set; } = null;
		public bool Snap { get; set; } = true;

		// Short
		private StageMusic Music => _Music != null ? _Music : (_Music = FindObjectOfType<StageMusic>());

		// Ser
		[SerializeField] private TimeLabelUI m_TimeLabel = null;
		[SerializeField] private Button m_Play = null;
		[SerializeField] private Button m_Pause = null;
		[SerializeField] private Button m_Replay = null;
		[SerializeField] private Button m_Repause = null;
		[SerializeField] private RectTransform m_NoMusicHint = null;
		[SerializeField] private Image m_PlayIcon = null;
		[SerializeField] private Image m_PauseIcon = null;
		[SerializeField] private Image m_ReplayIcon = null;
		[SerializeField] private Image m_RepauseIcon = null;
		[SerializeField] private Image m_Bar = null;
		[SerializeField] private Image m_Highlight = null;

		// Saving
		private SavingBool ShowBeatTime = new SavingBool("ProgressUI.ShowBeatTime", false);


		// Data
		private StageMusic _Music = null;
		private bool WasPlaying = false;


		// MSG
		private void Awake () {
			ShowBeatTime.Load();
			m_TimeLabel.ShowBeatTime = ShowBeatTime;
		}


		// API
		public void SetProgress (float time, float bpm = -1f) {
			m_Bar.fillAmount = Mathf.Clamp01(time / Music.Duration);
			if (bpm > 0f) {
				m_TimeLabel.BPM = bpm;
			}
			m_TimeLabel.Time = time;
		}


		public void RefreshControlUI () {
			bool isReady = Music.IsReady;
			bool isPlaying = Music.IsPlaying;
			m_NoMusicHint.gameObject.SetActive(!isReady);
			m_Play.gameObject.SetActive(!isPlaying);
			m_Pause.gameObject.SetActive(isPlaying);
			m_Replay.gameObject.SetActive(!isPlaying);
			m_Repause.gameObject.SetActive(isPlaying);
			m_Play.interactable = isReady;
			m_Pause.interactable = isReady;
			m_Replay.interactable = isReady;
			m_Repause.interactable = isReady;
			m_PlayIcon.color = isReady ? Color.white : Color.grey;
			m_PauseIcon.color = isReady ? Color.white : Color.grey;
			m_ReplayIcon.color = isReady ? Color.white : Color.grey;
			m_RepauseIcon.color = isReady ? Color.white : Color.grey;
		}


		public void UI_SwitchTimeLabelType () {
			m_TimeLabel.ShowBeatTime = !m_TimeLabel.ShowBeatTime;
			ShowBeatTime.Value = m_TimeLabel.ShowBeatTime;
		}


		public void OnPointerDown (PointerEventData e) {
			WasPlaying = Music.IsPlaying;
			Music.Pause();
			OnDrag(e);
		}


		public void OnDrag (PointerEventData e) {
			var rt = transform as RectTransform;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, e.pressEventCamera, out Vector2 localPoint);
			float time = Mathf.Clamp01(localPoint.x / rt.rect.width) * Music.Duration;
			if (Snap) {
				time = GetSnapTime(time, 1);
			}
			Music.Seek(time);
			SetProgress(time);
		}


		public void OnPointerUp (PointerEventData e) {
			if (WasPlaying) { Music.Play(); }
		}


		public void OnPointerEnter (PointerEventData eventData) => m_Highlight.enabled = true;


		public void OnPointerExit (PointerEventData eventData) => m_Highlight.enabled = false;


	}
}