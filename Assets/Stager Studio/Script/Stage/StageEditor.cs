namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class StageEditor : MonoBehaviour {




		#region --- SUB ---


		public enum EditMode {
			Stage = 0,
			Track = 1,
			Note = 2,
			Speed = 3,
			Motion = 4,
		}


		public delegate void VoidEditModeHandler (EditMode mode);
		public delegate void VoidIntHandler (int a);
		public delegate Beatmap BeatmapHandler ();



		#endregion



		#region --- VAR ---


		// Handle
		public static VoidEditModeHandler OnEditModeChanged { get; set; } = null;
		public static VoidIntHandler OnSelectionChanged { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;

		// Api
		public EditMode TheEditMode { get; private set; } = EditMode.Stage;
		public int SelectingObjectIndex { get; private set; } = -1;
		public int HoveringObjectIndex { get; private set; } = -1;

		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Toggle[] m_EditModeTGs = null;
		[SerializeField] private Toggle[] m_EyeTGs = null;
		[SerializeField] private Toggle[] m_LockTGs = null;
		[SerializeField] private Transform[] m_Containers = null;
		[SerializeField] private RectTransform m_FocusCancel = null;
		[SerializeField] private Animator m_FocusAni = null;
		[SerializeField] private string m_FocusKey = "Focus";
		[SerializeField] private string m_UnfocusKey = "Unfocus";
		[SerializeField] private float m_Duration = 0.5f;

		// Data
		private readonly bool[] ItemLock = { false, false, false, false, false, false, };
		private bool FocusMode = false;
		private bool UIReady = true;
		private Coroutine FocusAniCor = null;
		private Camera _Camera = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			// Init Edit Mode TGs
			for (int i = 0; i < m_EditModeTGs.Length; i++) {
				int index = i;
				m_EditModeTGs[index].onValueChanged.AddListener((isOn) => {
					if (UIReady && isOn) {
						SetEditMode(index);
					}
				});
			}
			// Eye TGs
			for (int i = 0; i < m_EyeTGs.Length; i++) {
				int index = i;
				var tg = m_EyeTGs[index];
				tg.isOn = false;
				tg.onValueChanged.AddListener((isOn) => SetContainerActive(index, !isOn));
			}
			// Lock TGs
			for (int i = 0; i < m_LockTGs.Length; i++) {
				int index = i;
				var tg = m_LockTGs[index];
				tg.isOn = ItemLock[index];
				tg.onValueChanged.AddListener((locked) => {
					ItemLock[index] = locked;
				});
			}
		}


		private void LateUpdate () {
			var map = GetBeatmap();
			if (map is null) { return; }
			// Mouse
			HoveringObjectIndex = -1;
			var mouseRay = Camera.ScreenPointToRay(Input.mousePosition);
			switch (TheEditMode) {
				case EditMode.Stage:
					LateUpdate_Mouse_Stage(map.Stages, mouseRay);
					break;
				case EditMode.Track:
					LateUpdate_Mouse_Track(map.Tracks, mouseRay);
					break;
				case EditMode.Note:
					LateUpdate_Mouse_Note(map.Notes, mouseRay);
					break;
			}
		}


		private void LateUpdate_Mouse_Stage (List<Beatmap.Stage> stages, Ray mouseRay) {
			if (stages is null) { return; }
			int count = stages.Count;
			for (int i = count - 1; i >= 0; i--) {
				var stage = stages[i];
				if (!stage.Active) { continue; }
				if (stage.Zone.Cast(mouseRay)) {
					HoveringObjectIndex = i;
					break;
				}
			}
		}


		private void LateUpdate_Mouse_Track (List<Beatmap.Track> tracks, Ray mouseRay) {
			if (tracks is null) { return; }
			int count = tracks.Count;
			for (int i = count - 1; i >= 0; i--) {
				var track = tracks[i];
				if (!track.Active) { continue; }
				if (track.Zone.Cast(mouseRay)) {
					HoveringObjectIndex = i;
					break;
				}
			}
		}


		private void LateUpdate_Mouse_Note (List<Beatmap.Note> notes, Ray mouseRay) {
			if (notes is null) { return; }
			int count = notes.Count;
			for (int i = count - 1; i >= 0; i--) {
				var note = notes[i];
				if (!note.Active) { continue; }
				if (note.Zone.Cast(mouseRay)) {
					HoveringObjectIndex = i;
					break;
				}
			}
		}


		#endregion




		#region --- API ---


		// Edit Mode
		public void UI_SetEditMode (int index) {
			if (!UIReady) { return; }
			SetEditMode(index);
		}


		// Container Active
		public void UI_SwitchContainerActive (int index) => SetContainerActive(index, !GetContainerActive(index));


		public bool GetContainerActive (int index) => m_Containers[index].gameObject.activeSelf;


		public void SetContainerActive (int index, bool active) {
			m_Containers[index].gameObject.SetActive(active);
			m_EyeTGs[index].isOn = !active;
		}


		// Item Lock
		public bool GetItemLock (int item) => item >= 0 ? ItemLock[item] : false;


		public void UI_SwitchLock (int index) {
			ItemLock[index] = !ItemLock[index];
			m_LockTGs[index].isOn = ItemLock[index];
		}


		// Focus
		public void UI_SetFocus (bool focus) {
			if (!(FocusAniCor is null)) { return; }
			if (focus != FocusMode) {
				FocusMode = focus;
				m_FocusAni.enabled = true;
				m_FocusAni.SetTrigger(focus ? m_FocusKey : m_UnfocusKey);
				if (!(FocusAniCor is null)) {
					StopCoroutine(FocusAniCor);
					FocusAniCor = null;
				}
				FocusAniCor = StartCoroutine(AniCheck());
			}
			// Func
			IEnumerator AniCheck () {
				if (focus) {
					m_FocusCancel.gameObject.SetActive(true);
				}
				yield return new WaitForSeconds(m_Duration);
				yield return new WaitUntil(() =>
					m_FocusAni.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !m_FocusAni.IsInTransition(0)
				);
				if (!focus) {
					m_FocusCancel.gameObject.SetActive(false);
				}
				m_FocusAni.enabled = false;
				FocusAniCor = null;
			}
		}


		public void UI_SwitchFocus () => UI_SetFocus(!FocusMode);


		#endregion



		#region --- LGC ---


		private void SetSelection (int index) {
			index = Mathf.Max(index, -1);
			SelectingObjectIndex = index;
			OnSelectionChanged(index);
		}


		private void SetEditMode (int index) {
			TheEditMode = (EditMode)index;
			UIReady = false;
			try {
				for (int j = 0; j < m_EditModeTGs.Length; j++) {
					m_EditModeTGs[j].isOn = (int)TheEditMode == j;
				}
			} catch { }
			UIReady = true;
			OnEditModeChanged(TheEditMode);
			SetSelection(-1);
		}


		#endregion

	}
}