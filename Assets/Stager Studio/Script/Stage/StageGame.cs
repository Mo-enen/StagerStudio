namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Curve;
	using Saving;
	using Data;
	using Object;


	public class StageGame : MonoBehaviour {




		#region --- SUB ---


		public delegate void VoidHandler ();
		public delegate void VoidFloatHandler (float ratio);
		public delegate float FloatHandler ();
		public delegate void VoidBoolIntIntHandler (bool value, int a, int b);
		public delegate string StringStringHandler (string str);
		public delegate void VoidStringBoolHandler (string s, bool b);
		public delegate bool BoolHandler ();
		public delegate bool BoolIntHandler (int i);
		public delegate Beatmap BeatmapHandler ();


		#endregion




		#region --- VAR ---


		// Const
		private readonly static float[] ABREAST_WIDTHS = { 0.25f, 0.33f, 0.618f, 1f, };
		private const string GAME_DROP_SPEED_HINT = "Game.Hint.GameDropSpeed";
		private const string MUSIC_PITCH_HINT = "Game.Hint.Pitch";
		private const string ABREAST_WIDTH_HINT = "Game.Hint.AbreastWidth";
		private const string SHOW_GRID_HINT = "Game.Hint.ShowGrid";

		// Handler
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static VoidHandler OnItemCountChanged { get; set; } = null;
		public static VoidHandler OnSpeedChanged { get; set; } = null;
		public static VoidHandler OnAbreastChanged { get; set; } = null;
		public static VoidHandler OnGridChanged { get; set; } = null;
		public static VoidFloatHandler OnRatioChanged { get; set; } = null;
		public static VoidStringBoolHandler LogHint { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static VoidFloatHandler MusicSeek { get; set; } = null;
		public static BoolHandler MusicIsPlaying { get; set; } = null;
		public static FloatHandler GetPitch { get; set; } = null;
		public static VoidFloatHandler SetPitch { get; set; } = null;
		public static VoidHandler MusicPlay { get; set; } = null;
		public static VoidHandler MusicPause { get; set; } = null;
		public static BoolIntHandler GetItemLock { get; set; } = null;

		// API
		public float Ratio {
			get => _Ratio;
			set {
				_Ratio = Mathf.Clamp(value, 0.1f, 10f);
				OnRatioChanged(_Ratio);
			}
		}
		public float SPB => 60f / Mathf.Max(BPM, 0.001f);
		public float BPM {
			get => _BPM;
			set {
				_BPM = Mathf.Clamp(value, 1f, 1024f);
			}
		}
		public float Shift { get; set; } = 0f;
		public float GameDropSpeed => Mathf.Clamp(_GameDropSpeed, 0.1f, 64f);
		public bool UseDynamicSpeed => !UseAbreast;
		public bool UseAbreast => AbreastValue > 0.5f;
		public float AbreastValue { get; private set; } = 0f;
		public float AbreastIndex { get; private set; } = 0f;
		public float AbreastWidth { get; private set; } = 0.618f;
		public bool PositiveScroll { get; set; } = true;

		// Short
		private ConstantFloat SpeedCurve { get; } = new ConstantFloat();
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);
		private Transform StageContainer => m_Containers[0];
		private Transform TrackContainer => m_Containers[1];
		private Transform NoteContainer => m_Containers[2];
		private Transform TimingContainer => m_Containers[3];
		private Transform LuminousContainer => m_Containers[4];
		private Transform StageHeadContainer => m_Containers[5];
		private Transform TrackHeadContainer => m_Containers[6];
		private Transform Prefab_Stage => m_Prefabs[0];
		private Transform Prefab_Track => m_Prefabs[1];
		private Transform Prefab_Note => m_Prefabs[2];
		private Transform Prefab_Timing => m_Prefabs[3];
		private Transform Prefab_Luminous => m_Prefabs[4];
		private Transform Prefab_Stage_Head => m_Prefabs[5];
		private Transform Prefab_Track_Head => m_Prefabs[6];

		// Ser
		[SerializeField] private Transform[] m_AntiMouseTF = null;
		[SerializeField] private Transform[] m_Prefabs = null;
		[SerializeField] private Transform[] m_Containers = null;
		[SerializeField] private RectTransform m_ZoneRT = null;

		// Data
		private Camera _Camera = null;
		private Coroutine AbreastValueCor = null;
		private Coroutine AbreastWidthCor = null;
		private Coroutine AbreastIndexCor = null;
		private Vector2? MouseMiddlePrevPos = null;
		private float _BPM = 120f;
		private float _Ratio = 1.5f;
		private float _GameDropSpeed = 1f;
		private bool SpeedCurveDirty = true;
		private float MouseDownMusicTime = -1f;
		private bool MusicPlayingForMouseDrag = false;

		// Saving
		public SavingBool ShowGrid { get; private set; } = new SavingBool("StageGame.ShowGrid", true);
		public SavingInt GridCountX0 { get; private set; } = new SavingInt("StageGame.GridCountX0", 7);
		public SavingInt GridCountX1 { get; private set; } = new SavingInt("StageGame.GridCountX1", 7);
		public SavingInt GridCountX2 { get; private set; } = new SavingInt("StageGame.GridCountX2", 7);
		public SavingInt GridCountY { get; private set; } = new SavingInt("StageGame.GridCountY", 4);
		public SavingInt BeatPerSection { get; private set; } = new SavingInt("StageGame.BeatPerSection", 4);
		public SavingInt AbreastWidthIndex { get; private set; } = new SavingInt("StageGame.AbreastWidthIndex", 1);
		public SavingBool ShowTimerOnPlay { get; private set; } = new SavingBool("StageGame.ShowTimerOnPlay", false);


		#endregion




#if UNITY_EDITOR
		[Header("Test"), SerializeField] private Beatmap m_TestBeatmap = null;
		public void SetTestBeatmap (Beatmap map) => m_TestBeatmap = map;
		public Beatmap GetTestBeatmap () => m_TestBeatmap;
#endif




		#region --- MSG ---


		private void Start () {
			SetShowGrid(ShowGrid);
			SetGridCountX0(GridCountX0);
			SetGridCountX1(GridCountX1);
			SetGridCountX2(GridCountX2);
			SetGridCountY(GridCountY);
			SetBeatPerSection(BeatPerSection);
			SetAbreastWidth(AbreastWidthIndex);
		}


		private void Update () {
			Update_Beatmap();
			Update_Stage();
			Update_Track();
			Update_Note();
			Update_Timing();
			Update_SpeedCurve();
			Update_Mouse();
		}


		private void Update_Beatmap () {
			if (MusicIsPlaying()) { return; }
			var map = GetBeatmap();
			if (map != null) {
				bool changed = false;
				// Delete Parent-Empty Items
				int stageCount = map.Stages.Count;
				int trackCount = map.Tracks.Count;
				int noteCount = map.Notes.Count;
				for (int i = 0; i < trackCount; i++) {
					var track = map.Tracks[i];
					if (track.StageIndex < 0 || track.StageIndex >= stageCount) {
						map.Tracks.RemoveAt(i);
						i--;
						trackCount--;
						changed = true;
					}
				}
				for (int i = 0; i < noteCount; i++) {
					var note = map.Notes[i];
					if (note.TrackIndex < 0 || note.TrackIndex >= trackCount) {
						map.Notes.RemoveAt(i);
						i--;
						noteCount--;
						changed = true;
					}
				}
				// Sync Container Active
				if (StageHeadContainer.gameObject.activeSelf != StageContainer.gameObject.activeSelf) {
					StageHeadContainer.gameObject.SetActive(StageContainer.gameObject.activeSelf);
				}
				if (TrackHeadContainer.gameObject.activeSelf != TrackContainer.gameObject.activeSelf) {
					TrackHeadContainer.gameObject.SetActive(TrackContainer.gameObject.activeSelf);
				}
				// Has Beatmap
				changed = FixObject(Prefab_Stage, StageContainer, map.Stages.Count) || changed;
				changed = FixObject(Prefab_Track, TrackContainer, map.Tracks.Count) || changed;
				changed = FixObject(Prefab_Note, NoteContainer, map.Notes.Count) || changed;
				changed = FixObject(Prefab_Timing, TimingContainer, map.Timings.Count) || changed;
				changed = FixObject(Prefab_Luminous, LuminousContainer, map.Notes.Count) || changed;
				changed = FixObject(Prefab_Stage_Head, StageHeadContainer, map.Stages.Count) || changed;
				changed = FixObject(Prefab_Track_Head, TrackHeadContainer, map.Tracks.Count) || changed;
				if (changed) {
					OnItemCountChanged();
				}
			} else {
				// No Beatmap
				ClearAllContainers();
			}
		}


		private void Update_Stage () {
			var map = GetBeatmap();
			if (map == null) { return; }
			int stageCount = map.Stages.Count;
			var container = StageContainer;
			var headContainer = StageHeadContainer;
			float musicTime = GetMusicTime();
			bool musicPlaying = MusicIsPlaying();
			bool stageLocked = GetItemLock(0);
			float gameSpeedMuti = GameDropSpeed;
			for (int i = 0; i < stageCount; i++) {
				// Stage
				var stageData = map.Stages[i];
				var stageTF = container.GetChild(i);
				bool timerActive = !stageLocked && (ShowTimerOnPlay.Value || !musicPlaying) && !UseAbreast && !StageObject.Solo.active && musicTime >= stageData.Time - 1f && musicTime <= stageData.Time + stageData.Duration;
				Stage.UpdateCache(stageData, i, timerActive, gameSpeedMuti);
				stageData.c_TrackCount = 0;
				if (!stageTF.gameObject.activeSelf) {
					if (stageData._Active) {
						stageTF.gameObject.SetActive(true);
					}
				}
				// Timer
				var timerTF = headContainer.GetChild(i);
				if (!timerTF.gameObject.activeSelf && stageData._TimerActive) {
					timerTF.gameObject.SetActive(true);
				}
			}
		}


		private void Update_Track () {
			var map = GetBeatmap();
			if (map == null) { return; }
			int stageCount = map.Stages.Count;
			int trackCount = map.Tracks.Count;
			var container = TrackContainer;
			var headContainer = TrackHeadContainer;
			float musicTime = GetMusicTime();
			bool musicPlaying = MusicIsPlaying();
			bool trackLocked = GetItemLock(1);
			for (int i = 0; i < trackCount; i++) {
				var tf = container.GetChild(i);
				var trackData = map.Tracks[i];
				if (trackData.StageIndex < 0 || trackData.StageIndex >= stageCount) { continue; }
				var stageData = map.Stages[trackData.StageIndex];
				stageData.c_TrackCount++;
				bool timerActive = !trackLocked && (ShowTimerOnPlay.Value || !musicPlaying) && !StageObject.Solo.active && musicTime >= trackData.Time - 1f && musicTime <= trackData.Time + trackData.Duration;
				Track.UpdateCache(trackData, stageData._Active, i, timerActive, stageData._SpeedMuti);
				if (!tf.gameObject.activeSelf) {
					if (trackData._Active) {
						tf.gameObject.SetActive(true);
					}
				}
				// Timer
				var timerTF = headContainer.GetChild(i);
				if (!timerTF.gameObject.activeSelf && trackData._TimerActive) {
					timerTF.gameObject.SetActive(true);
				}
			}
		}


		private void Update_Note () {
			var map = GetBeatmap();
			if (map == null) { return; }
			int noteCount = map.Notes.Count;
			var container = NoteContainer;
			var lumContainer = LuminousContainer;
			Beatmap.Note linkedNote;
			Beatmap.Track linkedTrack;
			Beatmap.Stage linkedStage;
			for (int i = 0; i < noteCount; i++) {
				var noteData = map.Notes[i];
				// Note
				var tf = container.GetChild(i);
				linkedNote = noteData.LinkedNoteIndex >= 0 && noteData.LinkedNoteIndex < noteCount ? map.Notes[noteData.LinkedNoteIndex] : null;
				linkedTrack = map.Tracks[noteData.TrackIndex];
				linkedStage = map.Stages[linkedTrack.StageIndex];
				Note.Update_Cache(noteData, linkedNote, linkedTrack._Active, linkedTrack._SpeedMuti);
				if (!tf.gameObject.activeSelf) {
					if (linkedStage._Active && linkedTrack._Active && noteData._Active) {
						tf.gameObject.SetActive(true);
					}
				}
				// Lum
				tf = lumContainer.GetChild(i);
				if (!tf.gameObject.activeSelf) {
					if (linkedStage._Active && linkedTrack._Active && Luminous.GetLumActive(noteData)) {
						tf.gameObject.SetActive(true);
					}
				}
			}

		}


		private void Update_Timing () {
			var map = GetBeatmap();
			if (map == null) { return; }
			int timingCount = map.Timings.Count;
			var container = TimingContainer;
			for (int i = 0; i < timingCount; i++) {
				var tf = container.GetChild(i);
				var tData = map.Timings[i];
				TimingNote.Update_Cache(tData);
				if (!tf.gameObject.activeSelf) {
					tData._Active = TimingNote.GetActive(tData);
					if (tData._Active) {
						tf.gameObject.SetActive(true);
					}
				}
			}
		}


		private void Update_SpeedCurve () {
			if (MusicIsPlaying()) { return; }
			// Speed Curve
			if (GetBeatmap() is null) {
				// No Map
				if (SpeedCurve.Count > 0) {
					SpeedCurve.Clear();
					SpeedCurveDirty = true;
				}
			} else {
				// Has Beatmap
				var speedNotes = GetBeatmap().Timings;
				if (speedNotes is null || speedNotes.Count == 0) {
					// No SpeedNote
					if (SpeedCurve.Count > 0) {
						SpeedCurve.Clear();
					}
					// Init Speed Note
					if (speedNotes is null) {
						GetBeatmap().Timings = new List<Beatmap.Timing>() { new Beatmap.Timing(0, 1), };
					} else {
						speedNotes.Add(new Beatmap.Timing(0, 1));
					}
					SpeedCurveDirty = true;
				} else if (SpeedCurve.Count != speedNotes.Count || SpeedCurveDirty) {
					// Reset Speed Curve
					SpeedCurve.Clear();
					foreach (var note in speedNotes) {
						float time = note.Time;
						while (SpeedCurve.ContainsKey(time)) { time += 0.0001f; }
						SpeedCurve.Add(time, note.Value);
					}
					SpeedCurveDirty = true;
				}
			}
			// Dirty
			if (SpeedCurveDirty) {
				OnSpeedChanged();
				SpeedCurveDirty = false;
			}
		}


		private void Update_Mouse () {
			// Reset Zoom
			if (Input.GetMouseButtonDown(2) && Input.GetKey(KeyCode.LeftControl) && CheckAntiMouse()) {
				SetGameDropSpeed(1f);
				LogGameHint_Key(GAME_DROP_SPEED_HINT, _GameDropSpeed.ToString("0.#"), false);
			}
			// Wheel
			if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.01f) {
				if (CheckAntiMouse()) {
					if (Input.GetKey(KeyCode.LeftControl)) {
						// Zoom Speed
						SetGameDropSpeed(GameDropSpeed + Input.mouseScrollDelta.y * (PositiveScroll ? 0.1f : -0.1f));
						LogGameHint_Key(GAME_DROP_SPEED_HINT, _GameDropSpeed.ToString("0.#"), false);
					} else {
						// Seek Music
						float delta = Input.mouseScrollDelta.y * (PositiveScroll ? -0.1f : 0.1f) / GameDropSpeed;
						if (Input.GetKey(KeyCode.LeftAlt)) {
							delta *= 0.1f;
						} else if (Input.GetKey(KeyCode.LeftShift)) {
							delta *= 10f;
						}
						MusicSeek(GetMusicTime() + delta * (60f / BPM));
					}
				}
			}
			// Right
			if (Input.GetMouseButton(1)) {
				// Seek Music
				if (MouseDownMusicTime < -0.5f) {
					// Right Down
					if (MouseDownMusicTime > -1.5f) {
						MouseDownMusicTime = CheckAntiMouse(true) ? GetMusicTimeAt(Input.mousePosition) : -2f;
						MusicPlayingForMouseDrag = MusicIsPlaying();
						MusicPause();
					}
				} else {
					// Right Drag
					float mouseTime = GetMusicTimeAt(Input.mousePosition);
					float musicTime = GetMusicTime();
					MusicSeek(FillTime(
						MouseDownMusicTime,
						Mathf.Sign(musicTime - mouseTime) * AreaBetween(mouseTime, musicTime, GameDropSpeed),
						GameDropSpeed
					));
				}
			} else {
				if (MouseDownMusicTime > -0.5f) {
					// Up
					if (MusicPlayingForMouseDrag) {
						MusicPlay();
					}
				}
				MouseDownMusicTime = -1f;
			}
			// Middle
			if (Input.GetMouseButton(2)) {
				if (MouseMiddlePrevPos.HasValue) {
					// Swipe Abreast Index
					if (UseAbreast) {
						var map = GetBeatmap();
						if (map != null) {
							AbreastIndex = Mathf.Clamp(
								AbreastIndex - (Input.mousePosition.x - MouseMiddlePrevPos.Value.x) / AbreastWidth * 0.001f,
								0, map.Stages.Count - 1
							);
						}
					}
				}
				MouseMiddlePrevPos = Input.mousePosition;
			} else if (MouseMiddlePrevPos.HasValue) {
				MouseMiddlePrevPos = null;
			}

			// Func
			bool CheckAntiMouse (bool mustInZone = true) {
				// Transform
				foreach (var tf in m_AntiMouseTF) {
					if (tf.gameObject.activeSelf) {
						return false;
					}
				}
				// Hover In Zone
				if (mustInZone) {
					var pos01 = m_ZoneRT.Get01Position(Input.mousePosition, Camera);
					if (pos01.x < 0 || pos01.x > 1 || pos01.y < 0 || pos01.y > 1) { return false; }
				}
				// Final
				return true;
			}
			float GetMusicTimeAt (Vector2 screenPos) => FillTime(GetMusicTime(), m_ZoneRT.Get01Position(screenPos, Camera).y, GameDropSpeed);
		}


		#endregion




		#region --- API ---


		public void ForceUpdateZone () {
			Update_Beatmap();
			Update_Stage();
			Update_Track();
			Update_Note();
			Update_Timing();
			Update_SpeedCurve();
		}


		public void ClearAllContainers () {
			for (int i = 0; i < m_Containers.Length; i++) {
				m_Containers[i].DestroyAllChildImmediately();
			}
		}


		public int GetItemCount (int index) => m_Containers[index].childCount;


		// Speed
		public void SetGameDropSpeed (float speed) {
			speed = Mathf.Clamp(speed, 0.1f, 10f);
			speed = Mathf.Round(speed * 10f) / 10f;
			_GameDropSpeed = speed;
			OnSpeedChanged();
		}


		// Speed Curve
		public void SetSpeedCurveDirty () => SpeedCurveDirty = true;


		public float GetDropSpeedAt (float time) => UseDynamicSpeed ? SpeedCurve.Evaluate(time) : 1f;


		public float FillTime (float time, float fill, float muti) =>
			UseDynamicSpeed ? SpeedCurve.Fill(time, fill, muti) : time + fill / muti;


		public float AreaBetween (float timeA, float timeB, float muti) =>
			UseDynamicSpeed ? SpeedCurve.GetAreaBetween(timeA, timeB, muti) : Mathf.Abs(timeA - timeB) * muti;


		// Abreast
		public void SwitchUseAbreastView () => SetUseAbreastView(!UseAbreast, true);


		public void SetUseAbreastView (bool abreast, bool animation = false) {
			if (AbreastValueCor != null) {
				StopCoroutine(AbreastValueCor);
			}
			var items = StageContainer.parent.GetComponentsInChildren<StageObject>();
			var speedNotes = TimingContainer.GetComponentsInChildren<TimingNote>();
			AbreastValueCor = StartCoroutine(AbreastValueing());
			// === Func ===
			IEnumerator AbreastValueing () {
				float aimAbreast = abreast ? 1f : 0f;
				if (!abreast) {
					AbreastValue = 0.98f;
				}
				if (animation) {
					for (float t = 0f; t < 4f && Mathf.Abs(AbreastValue - aimAbreast) > 0.005f; t += Time.deltaTime) {
						AbreastValue = Mathf.Lerp(AbreastValue, aimAbreast, Time.deltaTime * 8f);
						OnAbreastChanged();
						yield return new WaitForEndOfFrame();
					}
				}
				AbreastValue = aimAbreast;
				OnAbreastChanged();
				OnSpeedChanged();
				AbreastValueCor = null;
			}
		}


		public void SetAbreastIndex (float newIndex) {
			if (AbreastIndexCor != null) {
				StopCoroutine(AbreastIndexCor);
			}
			AbreastIndexCor = StartCoroutine(AbreastIndexing());
			// === Func ===
			IEnumerator AbreastIndexing () {
				float aimIndex = Mathf.Max(newIndex, 0);
				for (float t = 0f; t < 4f && Mathf.Abs(AbreastIndex - aimIndex) > 0.005f; t += Time.deltaTime) {
					AbreastIndex = Mathf.Lerp(AbreastIndex, aimIndex, Time.deltaTime * 12f);
					OnAbreastChanged();
					yield return new WaitForEndOfFrame();
				}
				AbreastIndex = aimIndex;
				OnAbreastChanged();
				AbreastIndexCor = null;
			}
		}


		public void SetAbreastWidth (int index, bool animation = false) {
			index = Mathf.Clamp(index, 0, ABREAST_WIDTHS.Length - 1);
			AbreastWidthIndex.Value = index;
			if (AbreastWidthCor != null) {
				StopCoroutine(AbreastWidthCor);
			}
			AbreastWidthCor = StartCoroutine(AbreastWidthing());
			// === Func ===
			IEnumerator AbreastWidthing () {
				if (animation) {
					float to = ABREAST_WIDTHS[index];
					for (float t = 0f; t < 4f && Mathf.Abs(AbreastWidth - to) > 0.005f; t += Time.deltaTime) {
						AbreastWidth = Mathf.LerpUnclamped(AbreastWidth, to, Time.deltaTime * 12f);
						OnAbreastChanged();
						yield return new WaitForEndOfFrame();
					}
				}
				AbreastWidth = ABREAST_WIDTHS[index];
				OnAbreastChanged();
				AbreastWidthCor = null;
			}
		}


		public void UI_SetAbreastWidth (int index) {
			SetAbreastWidth(index, true);
			// Hint
			string str = "% ";
			for (int i = 0; i < ABREAST_WIDTHS.Length; i++) {
				str += i == AbreastWidthIndex ? '■' : '□';
			}
			LogGameHint_Key(ABREAST_WIDTH_HINT, (ABREAST_WIDTHS[AbreastWidthIndex] * 100f).ToString("0") + str, false);
		}


		// Grid
		public float SnapTime (float time, int step) => SnapTime(time, 60f / BPM / step, Mathf.Repeat(Shift, 60f / BPM));


		public float SnapTime (float time, float gap, float offset) => Mathf.Round((time - offset) / gap) * gap + offset;


		public void SwitchShowGrid () {
			SetShowGrid(!ShowGrid);
			LogGameHint_Key(SHOW_GRID_HINT, ShowGrid.Value ? "ON" : "OFF", true);
		}


		public void SetShowGrid (bool show) {
			ShowGrid.Value = show;
			OnGridChanged();
		}


		public void SetGridCountX0 (int x) {
			GridCountX0.Value = Mathf.Clamp(x, 1, 32);
			OnGridChanged();
		}


		public void SetGridCountX1 (int x) {
			GridCountX1.Value = Mathf.Clamp(x, 1, 32);
			OnGridChanged();
		}


		public void SetGridCountX2 (int x) {
			GridCountX2.Value = Mathf.Clamp(x, 1, 32);
			OnGridChanged();
		}


		public void SetGridCountY (int y) {
			GridCountY.Value = Mathf.Clamp(y, 1, 8);
			OnGridChanged();
		}


		// Music
		public void SeekMusic_BPM (float muti) => MusicSeek(GetMusicTime() + 60f / BPM * muti);


		public void SetMusicPitch (float delta) {
			if (Mathf.Abs(delta) < 0.05f) {
				SetPitch(1f);
			} else {
				float pitch = GetPitch() + Mathf.Round(delta * 10f) / 10f;
				pitch = Mathf.Clamp(pitch, -5f, 5f);
				SetPitch(Mathf.Round(pitch * 10f) / 10f);
			}
			LogGameHint_Key(MUSIC_PITCH_HINT, GetPitch().ToString("0.#"), false);
		}


		public void SetBeatPerSection (int bps) {
			BeatPerSection.Value = Mathf.Clamp(bps, 1, 16);
			OnGridChanged();
		}


		#endregion




		#region --- LGC ---


		// Beatmap Update
		private bool FixObject (Transform prefab, Transform container, int count) {
			bool changed = false;
			int conCount = container.childCount;
			if (conCount > count) {
				container.FixChildcountImmediately(count);
				changed = true;
			} else if (conCount < count) {
				count -= conCount;
				if (prefab is null) {
					// Spawn Container
					for (int i = 0; i < count; i++) {
						var tf = new GameObject("").transform;
						tf.SetParent(container);
						tf.localPosition = Vector3.zero;
					}
				} else if (!(prefab is null)) {
					// Spawn Stage Object
					for (int i = 0; i < count; i++) {
						Instantiate(prefab, container);
					}
				}
				changed = true;
			}
			return changed;
		}


		// Misc
		private void LogGameHint_Key (string key, string arg, bool flash) => LogGameHint_Message(string.Format(GetLanguage(key), arg), flash);


		private void LogGameHint_Message (string msg, bool flash) {
			try {
				LogHint(msg, flash);
			} catch { }
		}


		#endregion




	}
}


#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEngine;
	using UnityEditor;
	using Stage;
	[CustomEditor(typeof(StageGame))]
	public class StageGameInspector : Editor {
		private readonly static string[] Exclude = new string[] { "m_TestBeatmap" };
		private void Awake () {
			if (EditorApplication.isPlaying) {
				(target as StageGame).SetTestBeatmap(FindObjectOfType<StageProject>().Beatmap);
			}
		}
		public override void OnInspectorGUI () {
			if (EditorApplication.isPlaying) {
				base.OnInspectorGUI();
				if (GUI.changed) {
					(target as StageGame).SetSpeedCurveDirty();
					StageGame.OnItemCountChanged();
				}
			} else {
				serializedObject.Update();
				DrawPropertiesExcluding(serializedObject, Exclude);
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
#endif