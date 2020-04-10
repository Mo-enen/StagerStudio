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
		public delegate void VoidBoolIntIntHandler (bool value, int a, int b);
		public delegate string StringStringHandler (string str);
		public delegate void VoidStringBoolHandler (string s, bool b);
		public delegate Beatmap BeatmapHandler ();


		#endregion




		#region --- VAR ---


		// Const
		private const string GAME_DROP_SPEED_HINT = "Game.Hint.GameDropSpeed";
		private const string MUSIC_PITCH_HINT = "Game.Hint.Pitch";
		private const string ABREAST_WIDTH_HINT = "Game.Hint.AbreastWidth";
		private readonly static float[] ABREAST_WIDTHS = { 0.25f, 0.33f, 0.618f, 1f, };

		// Handler
		public static StringStringHandler GetLanguage { get; set; } = null;
		public static VoidHandler OnStageObjectChanged { get; set; } = null;
		public static VoidHandler OnSpeedChanged { get; set; } = null;
		public static VoidHandler OnAbreastChanged { get; set; } = null;
		public static VoidHandler OnGridChanged { get; set; } = null;
		public static VoidFloatHandler OnRatioChanged { get; set; } = null;
		public static VoidStringBoolHandler LogHint { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;

		// API
		public float Ratio {
			get => _Ratio;
			set {
				_Ratio = Mathf.Clamp(value, 0.1f, 10f);
				OnRatioChanged(_Ratio);
			}
		}
		public float SPB => 60f / Mathf.Max(BPM, 0.001f);
		public float BPM { get; set; } = 120f;
		public float Shift { get; set; } = 0f;
		public float MapDropSpeed { get; set; } = 1f;
		public float GameDropSpeed => Mathf.Clamp(_GameDropSpeed, 0.1f, 10f);
		public bool UseDynamicSpeed => !UseAbreast;
		public bool UseAbreast => AbreastValue > 0.5f;
		public float AbreastValue { get; private set; } = 0f;
		public float AbreastIndex { get; private set; } = 0f;
		public float AbreastWidth { get; private set; } = 0.618f;
		public bool PositiveScroll { get; set; } = true;

		// Short
		private StageMusic Music => _Music != null ? _Music : (_Music = FindObjectOfType<StageMusic>());
		private ConstantFloat SpeedCurve { get; } = new ConstantFloat();
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Transform m_Prefab_Stage = null;
		[SerializeField] private Transform m_Prefab_Track = null;
		[SerializeField] private Transform m_Prefab_Note = null;
		[SerializeField] private Transform m_Prefab_Speed = null;
		[SerializeField] private Transform m_Prefab_Motion = null;
		[SerializeField] private Transform m_Prefab_Luminous = null;
		[SerializeField] private RectTransform m_ZoneRT = null;
		[SerializeField] private Transform[] m_AntiMouseTF = null;
		[SerializeField] private Transform[] m_Containers = null;

		// Data
		private StageMusic _Music = null;
		private Camera _Camera = null;
		private Coroutine AbreastValueCor = null;
		private Coroutine AbreastWidthCor = null;
		private Coroutine AbreastIndexCor = null;
		private float _Ratio = 1.5f;
		private float _GameDropSpeed = 1f;
		private bool SpeedCurveDirty = true;
		private float MouseDownMusicTime = -1f;
		private bool MusicPlayingForMouseDrag = false;

		// Saving
		public SavingBool ShowGrid { get; private set; } = new SavingBool("StageGame.ShowGrid", true);
		public SavingInt GridCountX { get; private set; } = new SavingInt("StageGame.GridCountX", 3);
		public SavingInt GridCountY { get; private set; } = new SavingInt("StageGame.GridCountY", 1);
		public SavingInt BeatPerSection { get; private set; } = new SavingInt("StageGame.BeatPerSection", 4);
		public SavingInt AbreastWidthIndex { get; private set; } = new SavingInt("StageGame.AbreastWidthIndex", 1);


		#endregion




#if UNITY_EDITOR
		[Header("Test"), SerializeField] private Beatmap m_TestBeatmap = null;
		public void SetTestBeatmap (Beatmap map) => m_TestBeatmap = map;
		public Beatmap GetTestBeatmap () => m_TestBeatmap;
#endif




		#region --- MSG ---


		private void Start () {
			SetShowGrid(ShowGrid);
			SetGridCountX(GridCountX);
			SetGridCountY(GridCountY);
			SetBeatPerSection(BeatPerSection);
			SetAbreastWidth(AbreastWidthIndex);
		}


		private void Update () {
			Update_Beatmap();
			Update_StageActive();
			Update_TrackActive();
			Update_NoteLumActive();
			Update_SpeedCurve();
			Update_Mouse();
		}


		private void Update_Beatmap () {
			if (Music.IsPlaying) { return; }
			var map = GetBeatmap();
			if (map != null) {
				// Has Beatmap
				bool changed = false;
				changed = FixObject(m_Prefab_Stage, null, m_Containers[0], map.Stages.Count) || changed;
				changed = FixObject(m_Prefab_Track, null, m_Containers[1], map.Tracks.Count) || changed;
				changed = FixObject(m_Prefab_Note, null, m_Containers[2], map.Notes.Count) || changed;
				changed = FixObject(null, m_Prefab_Speed, m_Containers[3], map.TimingNotes.Count) || changed;
				changed = FixStageMotionObject(m_Prefab_Motion, m_Containers[4].GetChild(0), map.Stages) || changed;
				changed = FixTrackMotionObject(m_Prefab_Motion, m_Containers[4].GetChild(1), map.Tracks) || changed;
				changed = FixObject(m_Prefab_Luminous, null, m_Containers[5], map.Notes.Count) || changed;
				if (changed) {
					OnStageObjectChanged();
				}
			} else {
				// No Beatmap
				ClearAllContainers();
			}
		}


		private void Update_StageActive () {
			var map = GetBeatmap();
			if (map == null) { return; }
			int stageCount = map.Stages.Count;
			var container = m_Containers[0];
			for (int i = 0; i < stageCount; i++) {
				var tf = container.GetChild(i);
				if (!tf.gameObject.activeSelf) {
					var stageData = map.Stages[i];
					stageData.Active = Stage.GetStageActive(stageData, i);
					if (stageData.Active) {
						tf.gameObject.SetActive(true);
					}
				}
			}
		}


		private void Update_TrackActive () {
			var map = GetBeatmap();
			if (map == null) { return; }
			int trackCount = map.Tracks.Count;
			var container = m_Containers[1];
			for (int i = 0; i < trackCount; i++) {
				var tf = container.GetChild(i);
				if (!tf.gameObject.activeSelf) {
					var trackData = map.Tracks[i];
					trackData.Active = Track.GetTrackActive(trackData);
					if (trackData.Active) {
						tf.gameObject.SetActive(true);
					}
				}
			}
		}


		private void Update_NoteLumActive () {
			var map = GetBeatmap();
			if (map == null) { return; }
			int noteCount = map.Notes.Count;
			var container = m_Containers[2];
			var lumContainer = m_Containers[5];
			Beatmap.Note linkedNote;
			Beatmap.Track linkedTrack;
			Beatmap.Stage linkedStage;
			float gameSpeedMuti = MapDropSpeed * GameDropSpeed;
			for (int i = 0; i < noteCount; i++) {
				var noteData = map.Notes[i];
				// Note
				var tf = container.GetChild(i);
				linkedNote = map.GetNoteAt(noteData.LinkedNoteIndex);
				linkedTrack = map.GetTrackAt(noteData.TrackIndex);
				linkedStage = map.GetStageAt(linkedTrack.StageIndex);
				Note.Update_Cache(noteData, gameSpeedMuti * linkedStage.SpeedMuti);
				if (!tf.gameObject.activeSelf) {
					noteData.Active = Note.GetNoteActive(noteData, linkedNote, noteData.AppearTime);
					if (linkedStage.Active && linkedTrack.Active && noteData.Active) {
						tf.gameObject.SetActive(true);
					}
				}
				// Lum
				tf = lumContainer.GetChild(i);
				if (!tf.gameObject.activeSelf) {
					if (linkedStage.Active && linkedTrack.Active && Luminous.GetLumActive(noteData)) {
						tf.gameObject.SetActive(true);
					}
				}
			}

		}


		private void Update_SpeedCurve () {
			if (Music.IsPlaying) { return; }
			// Speed Curve
			if (GetBeatmap() is null) {
				// No Map
				if (SpeedCurve.Count > 0) {
					SpeedCurve.Clear();
					SpeedCurveDirty = true;
				}
			} else {
				// Has Beatmap
				var speedNotes = GetBeatmap().TimingNotes;
				if (speedNotes is null || speedNotes.Count == 0) {
					// No SpeedNote
					if (SpeedCurve.Count > 0) {
						SpeedCurve.Clear();
					}
					// Init Speed Note
					if (speedNotes is null) {
						GetBeatmap().TimingNotes = new List<Beatmap.TimingNote>() { new Beatmap.TimingNote(0, 1), };
					} else {
						speedNotes.Add(new Beatmap.TimingNote(0, 1));
					}
					SpeedCurveDirty = true;
				} else if (SpeedCurve.Count != speedNotes.Count || SpeedCurveDirty) {
					// Reset Speed Curve
					SpeedCurve.Clear();
					foreach (var note in speedNotes) {
						float time = note.Time;
						while (SpeedCurve.ContainsKey(time)) { time += 0.0001f; }
						SpeedCurve.Add(time, note.Speed);
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
				LogGameHint_Key(GAME_DROP_SPEED_HINT, _GameDropSpeed.ToString("0.0"), false);
			}
			// Wheel
			if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.01f) {
				if (CheckAntiMouse()) {
					if (Input.GetKey(KeyCode.LeftControl)) {
						// Zoom
						SetGameDropSpeed(GameDropSpeed + Input.mouseScrollDelta.y * (PositiveScroll ? 0.1f : -0.1f));
						LogGameHint_Key(GAME_DROP_SPEED_HINT, _GameDropSpeed.ToString("0.0"), false);
					} else {
						// Seek
						float delta = Input.mouseScrollDelta.y * (PositiveScroll ? -0.1f : 0.1f) / GameDropSpeed;
						if (Input.GetKey(KeyCode.LeftAlt)) {
							delta *= 0.1f;
						} else if (Input.GetKey(KeyCode.LeftShift)) {
							delta *= 10f;
						}
						Music.Seek(Music.Time + delta * (60f / BPM));
					}
				}
			}
			// Right
			if (Input.GetMouseButton(1)) {
				if (MouseDownMusicTime < -0.5f) {
					// Right Down
					if (MouseDownMusicTime > -1.5f) {
						MouseDownMusicTime = CheckAntiMouse() ? GetMusicTimeAt(Input.mousePosition) : -2f;
						MusicPlayingForMouseDrag = Music.IsPlaying;
						Music.Pause();
					}
				} else {
					// Right Drag
					float mouseTime = GetMusicTimeAt(Input.mousePosition);
					Music.Seek(FillDropTime(
						MouseDownMusicTime,
						Mathf.Sign(Music.Time - mouseTime) * AreaBetween(mouseTime, Music.Time, GameDropSpeed * MapDropSpeed),
						GameDropSpeed * MapDropSpeed
					));
				}
			} else {
				if (MouseDownMusicTime > -0.5f) {
					// Up
					if (MusicPlayingForMouseDrag) {
						Music.Play();
					}
				}
				MouseDownMusicTime = -1f;
			}
			// Func
			bool CheckAntiMouse () {
				// Transform
				foreach (var tf in m_AntiMouseTF) {
					if (tf.gameObject.activeSelf) {
						return false;
					}
				}
				// Hover In Zone
				var pos01 = m_ZoneRT.Get01Position(Input.mousePosition, Camera);
				if (pos01.x < 0 || pos01.x > 1 || pos01.y < 0 || pos01.y > 1) { return false; }
				// Final
				return true;
			}
			float GetMusicTimeAt (Vector2 screenPos) => FillDropTime(Music.Time, m_ZoneRT.Get01Position(screenPos, Camera).y, GameDropSpeed * MapDropSpeed);
		}


		#endregion




		#region --- API ---


		public void ClearAllContainers () {
			for (int i = 0; i < m_Containers.Length; i++) {
				var container = m_Containers[i];
				if (i == 4) {
					container.GetChild(0).DestroyAllChildImmediately();
					container.GetChild(1).DestroyAllChildImmediately();
				} else {
					container.DestroyAllChildImmediately();
				}
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


		public float FillDropTime (float time, float fill, float muti) =>
			UseDynamicSpeed ? SpeedCurve.Fill(time, fill, muti) : time + fill / muti;


		public float AreaBetweenDrop (float time, float muti) => AreaBetween(0f, time, muti);


		public float AreaBetween (float timeA, float timeB, float muti) =>
			UseDynamicSpeed ? SpeedCurve.GetAreaBetween(timeA, timeB, muti) : Mathf.Abs(timeA - timeB) * muti;


		// Grid
		public float SnapTime (float time, int step) => SnapTime(time, 60f / BPM / step, Mathf.Repeat(Shift, 60f / BPM));


		public float SnapTime (float time, float gap, float offset) => Mathf.Round((time - offset) / gap) * gap + offset;


		// Abreast
		public void SwitchUseAbreastView () => SetUseAbreastView(!UseAbreast, true);


		public void SetUseAbreastView (bool abreast, bool animation = false) {
			if (AbreastValueCor != null) {
				StopCoroutine(AbreastValueCor);
			}
			var items = m_Containers[0].parent.GetComponentsInChildren<StageObject>();
			var speedNotes = m_Containers[3].GetComponentsInChildren<TimingNote>();
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


		public void SwitchAbreastWidth () {
			SetAbreastWidth((AbreastWidthIndex + 1) % ABREAST_WIDTHS.Length, true);
			// Hint
			string str = "% ";
			for (int i = 0; i < ABREAST_WIDTHS.Length; i++) {
				str += i == AbreastWidthIndex ? '■' : '□';
			}
			LogGameHint_Key(ABREAST_WIDTH_HINT, (ABREAST_WIDTHS[AbreastWidthIndex] * 100f).ToString("0") + str, false);
		}


		// Grid
		public void SwitchShowGrid () => SetShowGrid(!ShowGrid);


		public void SetShowGrid (bool show) {
			ShowGrid.Value = show;
			OnGridChanged();
		}


		public void SetGridCountX (int x) {
			GridCountX.Value = Mathf.Clamp(x, 1, 32);
			OnGridChanged();
		}


		public void SetGridCountY (int y) {
			GridCountY.Value = Mathf.Clamp(y, 1, 8);
			OnGridChanged();
		}


		// Music
		public void SeekMusic_BPM (float muti) => Music.Seek(Music.Time + 60f / BPM * muti);


		public void SetMusicPitch (float delta) {
			if (Mathf.Abs(delta) < 0.05f) {
				Music.Pitch = 1f;
			} else {
				float pitch = Music.Pitch + Mathf.Round(delta * 10f) / 10f;
				pitch = Mathf.Clamp(pitch, -5f, 5f);
				Music.Pitch = Mathf.Round(pitch * 10f) / 10f;
			}
			LogGameHint_Key(MUSIC_PITCH_HINT, Music.Pitch.ToString("0.0"), false);
		}


		public void SetBeatPerSection (int bps) {
			BeatPerSection.Value = Mathf.Clamp(bps, 1, 16);
			OnGridChanged();
		}


		#endregion




		#region --- LGC ---


		// Beatmap Update
		private bool FixObject (Transform prefab, Transform subPrefab, Transform container, int count) {
			bool changed = false;
			int conCount = container.childCount;
			if (conCount > count) {
				container.FixChildcountImmediately(count);
				changed = true;
			} else if (conCount < count) {
				count -= conCount;
				if (prefab is null && subPrefab is null) {
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
				} else {
					// Spawn Transform Object
					for (int i = 0; i < count; i++) {
						Instantiate(subPrefab, container);
					}
				}
				changed = true;
			}
			return changed;
		}


		private bool FixStageMotionObject (Transform prefab, Transform container, List<Beatmap.Stage> stages) {
			bool changed = false;
			int count = stages.Count;
			changed = FixObject(null, null, container, count) || changed;
			for (int i = 0; i < count; i++) {
				var stageCon = container.GetChild(i);
				changed = FixObject(null, null, stageCon, Beatmap.Stage.MOTION_COUNT) || changed;
				for (int j = 0; j < Beatmap.Stage.MOTION_COUNT; j++) {
					changed = FixObject(null, prefab, stageCon.GetChild(j), stages[i].GetMotionCount((Beatmap.Stage.MotionType)j)) || changed;
				}
			}
			return changed;
		}


		private bool FixTrackMotionObject (Transform prefab, Transform container, List<Beatmap.Track> tracks) {
			bool changed = false;
			int count = tracks.Count;
			changed = FixObject(null, null, container, count) || changed;
			for (int i = 0; i < count; i++) {
				var trackCon = container.GetChild(i);
				changed = FixObject(null, null, trackCon, Beatmap.Track.MOTION_COUNT) || changed;
				for (int j = 0; j < Beatmap.Track.MOTION_COUNT; j++) {
					changed = FixObject(null, prefab, trackCon.GetChild(j), tracks[i].GetMotionCount((Beatmap.Track.MotionType)j)) || changed;
				}
			}
			return changed;
		}


		// Misc
		private void LogGameHint_Key (string key, string arg, bool flash) {
			try {
				LogGameHint_Message(string.Format(GetLanguage(key), arg), flash);
			} catch { }
		}


		private void LogGameHint_Message (string msg, bool flash) => LogHint(msg, flash);


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
					StageGame.OnStageObjectChanged();
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