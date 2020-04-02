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
		private readonly static float[] ABREAST_WIDTHS = { 1f, 0.75f, 0.618f, 0.5f, };

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
		public float BPM { get; set; } = 120f;
		public float Shift { get; set; } = 0f;
		public float MapDropSpeed { get; set; } = 1f;
		public float GameDropSpeed => Mathf.Clamp(_GameDropSpeed, 0.1f, 10f);
		public bool UseDynamicSpeed { get; private set; } = true;
		public bool UseAbreast { get; private set; } = false;
		public bool AllStageAbreast { get; private set; } = false;
		public int AbreastIndex => Mathf.Max(_AbreastIndex, 0);
		public bool PositiveScroll { get; set; } = true;
		public float AbreastWidth { get; private set; } = 1f;

		// Short
		private StageMusic Music => _Music != null ? _Music : (_Music = FindObjectOfType<StageMusic>());
		private ConstantFloat SpeedCurve { get; } = new ConstantFloat();
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private StageObject m_Prefab_Stage = null;
		[SerializeField] private StageObject m_Prefab_Track = null;
		[SerializeField] private StageObject m_Prefab_Note = null;
		[SerializeField] private Transform m_Prefab_Speed = null;
		[SerializeField] private Transform m_Prefab_Motion = null;
		[SerializeField] private StageObject m_Prefab_Luminous = null;
		[SerializeField] private Transform[] m_AntiMouseTF = null;
		[SerializeField] private Transform m_Level = null;
		[SerializeField] private RectTransform m_ZoneRT = null;

		// Data
		private StageMusic _Music = null;
		private Camera _Camera = null;
		private Transform[] Containers = null;
		private float _Ratio = 1.5f;
		private int _AbreastIndex = 0;
		private float _GameDropSpeed = 1f;
		private bool SpeedCurveDirty = true;
		private float MouseDownMusicTime = -1f;
		private bool MusicPlayingForMouseDrag = false;

		// Saving
		public SavingBool ShowGrid { get; private set; } = new SavingBool("StageGame.ShowGrid", true);
		public SavingInt GridCountX { get; private set; } = new SavingInt("StageGame.GridCountX", 3);
		public SavingInt GridCountY { get; private set; } = new SavingInt("StageGame.GridCountY", 1);
		public SavingInt BeatPerSection { get; private set; } = new SavingInt("StageGame.BeatPerSection", 4);
		public SavingInt AbreastWidthIndex { get; private set; } = new SavingInt("StageGame.AbreastWidthIndex", 0);


		#endregion




#if UNITY_EDITOR
		[Header("Test"), SerializeField] private Beatmap m_TestBeatmap = null;
		public void SetTestBeatmap (Beatmap map) => m_TestBeatmap = map;
		public Beatmap GetTestBeatmap () => m_TestBeatmap;
#endif




		#region --- MSG ---


		private void Awake () {
			// Sorting Layer ID
			Stage.SortingLayerID_Stage = SortingLayer.NameToID("Stage");
			Stage.SortingLayerID_Judge = SortingLayer.NameToID("Judge");
			Track.SortingLayerID_TrackTint = SortingLayer.NameToID("TrackTint");
			Track.SortingLayerID_Track = SortingLayer.NameToID("Track");
			Track.SortingLayerID_Tray = SortingLayer.NameToID("Tray");
			Note.SortingLayerID_Pole = SortingLayer.NameToID("Pole");
			Note.SortingLayerID_Note_Hold = SortingLayer.NameToID("HoldNote");
			Note.SortingLayerID_Note = SortingLayer.NameToID("Note");
			Note.LayerID_Note = LayerMask.NameToLayer("Note");
			Note.LayerID_Note_Hold = LayerMask.NameToLayer("HoldNote");
			Note.SortingLayerID_Arrow = SortingLayer.NameToID("Arrow");
			StageObject.SortingLayerID_UI = SortingLayer.NameToID("UI");
			SpeedNote.SortingLayerID_Speed = SortingLayer.NameToID("Speed");
			MotionNote.SortingLayerID_Motion = SortingLayer.NameToID("Motion");
			Luminous.SortingLayerID_Lum = SortingLayer.NameToID("Luminous");
			// Misc
			const int CONTAINER_COUNT = 6;
			Containers = new Transform[CONTAINER_COUNT];
			for (int i = 0; i < CONTAINER_COUNT; i++) {
				Containers[i] = m_Level.GetChild(i);
			}
		}


		private void Start () {
			SetShowGrid(ShowGrid);
			SetGridCountX(GridCountX);
			SetGridCountY(GridCountY);
			SetBeatPerSection(BeatPerSection);
			SetAbreastWidth(AbreastWidthIndex);
		}


		private void Update () {
			Update_Beatmap();
			Update_SpeedCurve();
			Update_Mouse();
		}


		private void Update_Beatmap () {
			if (Music.IsPlaying) { return; }
			var map = GetBeatmap();
			if (!(map is null)) {
				// Has Beatmap
				bool changed = false;
				changed = FixObject(m_Prefab_Stage, null, Containers[0], map.Stages.Count) || changed;
				changed = FixObject(m_Prefab_Track, null, Containers[1], map.Tracks.Count) || changed;
				changed = FixObject(m_Prefab_Note, null, Containers[2], map.Notes.Count) || changed;
				changed = FixObject(null, m_Prefab_Speed, Containers[3], map.SpeedNotes.Count) || changed;
				changed = FixStageMotionObject(m_Prefab_Motion, Containers[4].GetChild(0), map.Stages) || changed;
				changed = FixTrackMotionObject(m_Prefab_Motion, Containers[4].GetChild(1), map.Tracks) || changed;
				changed = FixObject(m_Prefab_Luminous, null, Containers[5], map.Notes.Count) || changed;
				if (changed) {
					OnStageObjectChanged();
				}
			} else {
				// No Beatmap
				ClearAllContainers();
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
				var speedNotes = GetBeatmap().SpeedNotes;
				if (speedNotes is null || speedNotes.Count == 0) {
					// No SpeedNote
					if (SpeedCurve.Count > 0) {
						SpeedCurve.Clear();
					}
					// Init Speed Note
					if (speedNotes is null) {
						GetBeatmap().SpeedNotes = new List<Beatmap.SpeedNote>() { new Beatmap.SpeedNote(0, 1), };
					} else {
						speedNotes.Add(new Beatmap.SpeedNote(0, 1));
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
			for (int i = 0; i < Containers.Length; i++) {
				var container = Containers[i];
				if (i == 4) {
					container.GetChild(0).DestroyAllChildImmediately();
					container.GetChild(1).DestroyAllChildImmediately();
				} else {
					container.DestroyAllChildImmediately();
				}
			}
		}


		public int GetItemCount (int index) => Containers[index].childCount;


		// Speed
		public void SetGameDropSpeed (float speed) {
			speed = Mathf.Clamp(speed, 0.1f, 10f);
			speed = Mathf.Round(speed * 10f) / 10f;
			_GameDropSpeed = speed;
			OnSpeedChanged();
		}


		// Speed Curve
		public void SetSpeedCurveDirty () => SpeedCurveDirty = true;


		public float FillDropTime (float time, float fill, float muti) =>
			UseDynamicSpeed ? SpeedCurve.Fill(time, fill, muti) : time + fill / muti;


		public float AreaBetweenDrop (float time, float muti) => AreaBetween(0f, time, muti);


		public float AreaBetween (float timeA, float timeB, float muti) =>
			UseDynamicSpeed ? SpeedCurve.GetAreaBetween(timeA, timeB, muti) : Mathf.Abs(timeA - timeB) * muti;


		// Grid
		public float SnapTime (float time, int step) => SnapTime(time, 60f / BPM / step, Mathf.Repeat(Shift, 60f / BPM));


		public float SnapTime (float time, float gap, float offset) => Mathf.Round((time - offset) / gap) * gap + offset;


		// UI
		public void SwitchUseDynamicSpeed () => SetUseDynamicSpeed(!UseDynamicSpeed);


		public void SwitchUseAbreastView () => SetUseAbreastView(!UseAbreast);


		public void SwitchShowGrid () => SetShowGrid(!ShowGrid);


		public void SetUseDynamicSpeed (bool use) {
			UseDynamicSpeed = use;
			OnSpeedChanged();
		}


		public void SetUseAbreastView (bool abreast) {
			UseAbreast = abreast;
			Camera.orthographic = abreast;
			OnAbreastChanged();
		}


		public void SwitchAllAbreast () => SetAllStageAbreast(!AllStageAbreast);


		public void SetAllStageAbreast (bool abreast) {
			AllStageAbreast = abreast;
			OnAbreastChanged();

		}


		public void SetAbreastIndex (int newIndex) {
			_AbreastIndex = Mathf.Max(newIndex, 0);
			OnAbreastChanged();
		}


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


		public void SwitchAbreastWidth () {
			SetAbreastWidth((AbreastWidthIndex + 1) % ABREAST_WIDTHS.Length);
			// Hint
			string str = "% ";
			for (int i = 0; i < ABREAST_WIDTHS.Length; i++) {
				str += i == AbreastWidthIndex ? '■' : '□';
			}
			LogGameHint_Key(ABREAST_WIDTH_HINT, (AbreastWidth * 100f).ToString("0") + str, false);
		}


		public void SetAbreastWidth (int index) {
			index = Mathf.Clamp(index, 0, ABREAST_WIDTHS.Length - 1);
			AbreastWidthIndex.Value = index;
			AbreastWidth = ABREAST_WIDTHS[index];
			OnAbreastChanged();
		}


		#endregion




		#region --- LGC ---


		// Beatmap Update
		private bool FixObject (StageObject prefab, Transform subPrefab, Transform container, int count) {
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
						Instantiate(prefab, container).SetSkinData(StageSkin.Data.Data);
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