﻿namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using Saving;


	public class ProgressUI : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {


		// SUB
		public delegate float FloatFloatIntHandler (float musicTime, int step);
		public delegate float FloatHandler ();
		public delegate (bool isReady, bool isPlaying) ReadyPlayHandler ();
		public delegate void VoidHandler ();
		public delegate void VoidFloatHandler (float value);


		// API
		public static FloatFloatIntHandler GetSnapTime { get; set; } = null;
		public static FloatHandler GetDuration { get; set; } = null;
		public static FloatHandler GetBPM { get; set; } = null;
		public static FloatHandler GetShift { get; set; } = null;
		public static ReadyPlayHandler GetReadyPlay { get; set; } = null;
		public static VoidHandler PlayMusic { get; set; } = null;
		public static VoidHandler PauseMusic { get; set; } = null;
		public static VoidFloatHandler SeekMusic { get; set; } = null;
		public bool Snap { get; set; } = true;

		// Ser
		[SerializeField] private TimeLabelUI m_TimeLabel = null;
		[SerializeField] private Button m_Play = null;
		[SerializeField] private Button m_Pause = null;
		[SerializeField] private Button m_Replay = null;
		[SerializeField] private Button m_Repause = null;
		[SerializeField] private Button m_PitchUp = null;
		[SerializeField] private Button m_PitchDown = null;
		[SerializeField] private RectTransform m_NoMusicHint = null;
		[SerializeField] private Image[] m_ControlIcons = null;
		[SerializeField] private Image m_Bar = null;
		[SerializeField] private Image m_Highlight = null;

		// Saving
		private SavingBool ShowBeatTime = new SavingBool("ProgressUI.ShowBeatTime", false);


		// Data
		private bool WasPlaying = false;


		// MSG
		private void Awake () {
			m_TimeLabel.ShowBeatTime = ShowBeatTime;
		}


		// API
		public void SetProgress (float time) {
			m_Bar.fillAmount = Mathf.Clamp01(time / GetDuration());
			m_TimeLabel.Time = time;
			m_TimeLabel.BPM = GetBPM();
			m_TimeLabel.Shift = GetShift();
		}


		public void RefreshControlUI () {
			var (isReady, isPlaying) = GetReadyPlay();
			m_NoMusicHint.gameObject.SetActive(!isReady);
			m_Play.gameObject.SetActive(!isPlaying);
			m_Pause.gameObject.SetActive(isPlaying);
			m_Replay.gameObject.SetActive(!isPlaying);
			m_Repause.gameObject.SetActive(isPlaying);
			m_Play.interactable = isReady;
			m_Pause.interactable = isReady;
			m_Replay.interactable = isReady;
			m_Repause.interactable = isReady;
			m_PitchUp.interactable = isReady;
			m_PitchDown.interactable = isReady;
			foreach (var icon in m_ControlIcons) {
				icon.color = isReady ? Color.white : Color.grey;
			}
		}


		public void UI_SwitchTimeLabelType () {
			m_TimeLabel.ShowBeatTime = !m_TimeLabel.ShowBeatTime;
			ShowBeatTime.Value = m_TimeLabel.ShowBeatTime;
		}


		public void OnPointerDown (PointerEventData e) {
			if (e.button == PointerEventData.InputButton.Left) {
				WasPlaying = GetReadyPlay().isPlaying;
				PauseMusic();
				OnDrag(e);
			}
		}


		public void OnDrag (PointerEventData e) {
			if (e.button == PointerEventData.InputButton.Left) {
				var rt = transform as RectTransform;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, e.pressEventCamera, out Vector2 localPoint);
				float time = Mathf.Clamp01(localPoint.x / rt.rect.width) * GetDuration();
				if (Snap) {
					time = GetSnapTime(time, 1);
				}
				SeekMusic(time);
				SetProgress(time);
			}
		}


		public void OnPointerUp (PointerEventData e) {
			if (WasPlaying) { PlayMusic(); }
		}


		public void OnPointerEnter (PointerEventData eventData) => m_Highlight.enabled = true;


		public void OnPointerExit (PointerEventData eventData) => m_Highlight.enabled = false;


	}
}