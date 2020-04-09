namespace StagerStudio.Data {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;



	[System.Serializable]
	public class Beatmap {



		// Comparer
		public class StageComparer : IComparer<Stage> {
			public int Compare (Stage x, Stage y) => x.Time.CompareTo(y.Time);
		}
		public class TrackComparer : IComparer<Track> {
			public int Compare (Track x, Track y) => x.Time.CompareTo(y.Time);
		}
		public class NoteComparer : IComparer<Note> {
			public int Compare (Note x, Note y) => x.Time.CompareTo(y.Time);
		}
		public class SpeedNoteComparer : IComparer<SpeedNote> {
			public int Compare (SpeedNote x, SpeedNote y) => x.Time.CompareTo(y.Time);
		}


		// SUB
		[System.Serializable]
		public struct TimeFloatTween {

			public float Time {
				get => m_Time / 1000f;
				set {
					m_Time = (int)(value * 1000f);
				}
			}
			public float Value {
				get => m_Value / 1000f;
				set {
					m_Value = (int)(value * 1000f);
				}
			}
			public int Tween {
				get => m_Tween;
				set {
					m_Tween = value;
				}
			}

			public int m_Time;
			public int m_Value;
			public int m_Tween;

			public static int Search (List<TimeFloatTween> data, float time) {
				int start = 0;
				int end = data.Count - 1;
				int mid;
				while (start <= end) {
					mid = (start + end) / 2;
					if (data[mid].Time < time) {
						start = mid + 1;
					} else if (data[mid].Time > time) {
						end = mid - 1;
					} else {
						return mid;
					}
				}
				return (start + end) / 2;
			}

		}


		[System.Serializable]
		public struct TimeIntTween {

			public float Time {
				get => m_Time / 1000f;
				set {
					m_Time = (int)(value * 1000f);
				}
			}
			public int Value {
				get => m_Value;
				set => m_Value = value;
			}
			public int Tween {
				get => m_Tween;
				set {
					m_Tween = value;
				}
			}

			public int m_Time;
			public int m_Value;
			public int m_Tween;

			public static int Search (List<TimeIntTween> data, float time) {
				int start = 0;
				int end = data.Count - 1;
				int mid;
				while (start <= end) {
					mid = (start + end) / 2;
					if (data[mid].Time < time) {
						start = mid + 1;
					} else if (data[mid].Time > time) {
						end = mid - 1;
					} else {
						return mid;
					}
				}
				return (start + end) / 2;
			}

		}


		[System.Serializable]
		public struct TimeFloatFloatTween {

			public float Time {
				get => m_Time / 1000f;
				set {
					m_Time = (int)(value * 1000f);
				}
			}
			public float A {
				get => m_A / 1000f;
				set {
					m_A = (int)(value * 1000f);
				}
			}
			public float B {
				get => m_B / 1000f;
				set {
					m_B = (int)(value * 1000f);
				}
			}
			public int Tween {
				get => m_Tween;
				set {
					m_Tween = (byte)value;
				}
			}

			public int m_Time;
			public int m_A;
			public int m_B;
			public byte m_Tween;

			public static int Search (List<TimeFloatFloatTween> data, float time) {
				int start = 0;
				int end = data.Count - 1;
				int mid;
				while (start <= end) {
					mid = (start + end) / 2;
					if (data[mid].Time < time) {
						start = mid + 1;
					} else if (data[mid].Time > time) {
						end = mid - 1;
					} else {
						return mid;
					}
				}
				return (start + end) / 2;
			}

		}



		[System.Serializable]
		public class Stage {

			public const int MOTION_COUNT = 5;
			public enum MotionType {
				Position = 0,
				Rotation = 1,
				Width = 2,
				Height = 3,
			}

			// API
			public float Time {
				get => m_Time / 1000f;
				set => m_Time = (int)(value * 1000f);
			}
			public float Duration {
				get => m_Duration / 1000f;
				set => m_Duration = (int)(Mathf.Clamp(value, 0f, int.MaxValue / 1000f - 1f) * 1000f);
			}
			public float Speed {
				get => m_Speed / 1000f;
				set => m_Speed = (int)(value * 1000f);
			}
			public float X {
				get => m_X / 1000f;
				set => m_X = (int)(value * 1000f);
			}
			public float Y {
				get => m_Y / 1000f;
				set => m_Y = (int)(value * 1000f);
			}
			public float Width {
				get => m_Width / 1000f;
				set => m_Width = (int)(value * 1000f);
			}
			public float Height {
				get => m_Height / 1000f;
				set => m_Height = (int)(value * 1000f);
			}
			public float Rotation {
				get => m_Rotation;
				set => m_Rotation = (int)value;
			}

			// SER-API
			public int m_Time = 0;
			public int m_Duration = 0;
			public int m_Speed = 1000;
			public int m_X = 0;
			public int m_Y = 0;
			public int m_Width = 1000;
			public int m_Height = 1000;
			public int m_Rotation = 0;
			public List<TimeFloatFloatTween> Positions;
			public List<TimeFloatTween> Rotations;
			public List<TimeFloatTween> Widths;
			public List<TimeFloatTween> Heights;

			// Cache
			[System.NonSerialized] public bool Active = false;
			[System.NonSerialized] public bool Selecting = false;
			[System.NonSerialized] public float SpeedMuti = 1f;

			// API
			public int GetMotionCount (MotionType type) {
				switch (type) {
					default:
						return 0;
					case MotionType.Position:
						return Positions is null ? 0 : Positions.Count;
					case MotionType.Rotation:
						return Rotations is null ? 0 : Rotations.Count;
					case MotionType.Width:
						return Widths is null ? 0 : Widths.Count;
					case MotionType.Height:
						return Heights is null ? 0 : Heights.Count;

				}
			}

		}



		[System.Serializable]
		public class Track {

			public const int MOTION_COUNT = 3;
			public enum MotionType {
				X = 0,
				Width = 1,
				Color = 2,
				Angle = 3,
			}

			// API
			public float Time {
				get => m_Time / 1000f;
				set => m_Time = (int)(value * 1000f);
			}
			public float Duration {
				get => m_Duration / 1000f;
				set => m_Duration = (int)(Mathf.Clamp(value, 0f, int.MaxValue / 1000f - 1f) * 1000f);
			}
			public float X {
				get => m_X / 1000f;
				set => m_X = (int)(value * 1000f);
			}
			public float Width {
				get => m_Width / 1000f;
				set => m_Width = (int)(value * 1000f);
			}
			public float Angle {
				get => m_Angle;
				set => m_Angle = (int)value;
			}

			// API - Ser
			public int m_Time = 0;
			public int m_Duration = 0;
			public int m_X = 0;
			public int m_Width = 1000;
			public int m_Angle = 0;
			public int StageIndex = -1;
			public int Color = 0;
			public bool HasTray = false;
			public List<TimeFloatTween> Xs;
			public List<TimeFloatTween> Widths;
			public List<TimeIntTween> Colors;
			public List<TimeFloatTween> Angles;

			// Cache
			[System.NonSerialized] public bool Active = false;
			[System.NonSerialized] public bool Selecting = false;
			[System.NonSerialized] public (float min, float max) TrayX = (0.5f, 0.5f);
			[System.NonSerialized] public float TrayTime = float.MaxValue;

			// API
			public int GetMotionCount (MotionType type) {
				switch (type) {
					default:
						return 0;
					case MotionType.X:
						return Xs is null ? 0 : Xs.Count;
					case MotionType.Width:
						return Widths is null ? 0 : Widths.Count;
					case MotionType.Color:
						return Colors is null ? 0 : Colors.Count;
					case MotionType.Angle:
						return Angles is null ? 0 : Angles.Count;
				}
			}

		}



		[System.Serializable]
		public class Note {

			// API
			public float Time {
				get => m_Time / 1000f;
				set => m_Time = (int)(value * 1000f);
			}
			public float Duration {
				get => m_Duration / 1000f;
				set => m_Duration = (int)(Mathf.Clamp(value, 0f, int.MaxValue / 1000f - 1f) * 1000f);
			}
			public float X {
				get => m_X / 1000f;
				set => m_X = (int)(value * 1000f);
			}
			public float Z {
				get => m_Z / 1000f;
				set => m_Z = (int)(value * 1000f);
			}
			public float Width {
				get => m_Width / 1000f;
				set => m_Width = (int)(value * 1000f);
			}

			// SER-API
			public int m_Time = 0;
			public int m_Duration = 0;
			public int m_X = 0;
			public int m_Z = 0;
			public int m_Width = 1000;
			public int TrackIndex = -1;
			public int LinkedNoteIndex = -1;
			public short ClickSoundIndex = -1;
			public byte SoundFxIndex = 0;
			public byte SoundFxParam = 0;
			public bool Tap = true;
			public byte SwipeX = 1; // 0 = Left, 1 = None, 2 = Right
			public byte SwipeY = 1; // 0 = Down, 1 = None, 2 = Up
			public string Comment = "";

			// Cache
			[System.NonSerialized] public bool Active = false;
			[System.NonSerialized] public bool Selecting = false;
			[System.NonSerialized] public float AppearTime = 0f;
			[System.NonSerialized] public float SpeedMuti = -1f;
			[System.NonSerialized] public float SpeedOnDrop = 1f;
			[System.NonSerialized] public float NoteDropStart = -1f;
			[System.NonSerialized] public float NoteDropEnd = -1f;
			[System.NonSerialized] public float CacheTime = -1f;
			[System.NonSerialized] public float CacheDuration = -1f;
			[System.NonSerialized] public static byte CacheDirtyID = 1;
			[System.NonSerialized] public byte LocalCacheDirtyID = 0;

		}



		[System.Serializable]
		public class SpeedNote {

			// API
			public float Time {
				get => m_Time / 1000f;
				set => m_Time = (int)(value * 1000f);
			}
			public float Speed {
				get => m_Speed / 100f;
				set => m_Speed = (int)(value * 100f);
			}

			// SER
			public int m_Time = 0;
			public int m_Speed = 0;

			// Cache
			[System.NonSerialized] public bool Active = false;
			[System.NonSerialized] public bool Selecting = false;
			[System.NonSerialized] public float AppearTime = -1f;
			[System.NonSerialized] public float SpeedMuti = -1f;
			[System.NonSerialized] public float NoteDropPos = -1f;

			// API
			public SpeedNote (float time, float speed) {
				Time = time;
				Speed = speed;
			}

		}


		// API-SER
		public readonly static Stage DEFAULT_STAGE = new Stage() {
			Time = 0f,
			Duration = float.MaxValue,
			Rotation = 0f,
			Height = 1f,
			Width = 1f,
			X = 0.5f,
			Y = 0f,
			Speed = 1f,
			Rotations = { },
			Positions = { },
			Heights = { },
			Widths = { },
		};
		public readonly static Track DEFAULT_TRACK = new Track() {
			Time = 0f,
			Duration = float.MaxValue,
			Color = -1,
			X = 0.5f,
			Width = 1f,
			HasTray = false,
			StageIndex = -1,
			Angle = 0f,
			Xs = { },
			Colors = { },
			Widths = { },
			Angles = { },
		};
		public float DropSpeed {
			get => m_DropSpeed / 1000f;
			set {
				m_DropSpeed = (int)(value * 1000f);
			}
		}
		public float Shift {
			get => m_Shift / 1000f;
			set {
				m_Shift = (int)(value * 1000f);
			}
		}
		public float Ratio {
			get => m_Ratio / 1000f;
			set {
				m_Ratio = (int)(value * 1000f);
			}
		}
		public int BPM = 120;
		public int Level = 1;
		public string Tag = "Normal";
		public long CreatedTime = 0;
		public List<Stage> Stages = new List<Stage>();
		public List<Track> Tracks = new List<Track>();
		public List<Note> Notes = new List<Note>();
		public List<SpeedNote> SpeedNotes = new List<SpeedNote>();

		// SER
		public int m_DropSpeed = 1000;
		public int m_Shift = 0;
		public int m_Ratio = 1500;

		// Beatmap
		public static Beatmap NewBeatmap () {
			var map = new Beatmap() {
				CreatedTime = System.DateTime.Now.Ticks,
			};
			map.FixEmpty();
			return map;
		}


		public void FixEmpty () {
			if (Stages is null) {
				Stages = new List<Stage>();
			}
			if (Tracks is null) {
				Tracks = new List<Track>();
			}
			if (Notes is null) {
				Notes = new List<Note>();
			}
			if (SpeedNotes is null) {
				SpeedNotes = new List<SpeedNote>();
			}
		}


		public void LoadFromBytes (byte[] bytes) {
			if (bytes is null) { return; }
			LoadFromOtherMap(Util.BytesToObject(bytes) as Beatmap);
		}


		public void LoadFromOtherMap (Beatmap map, bool loadCreatedTime = true) {
			if (map == null) { return; }
			Tag = map.Tag;
			Level = map.Level;
			DropSpeed = map.DropSpeed;
			BPM = map.BPM;
			Shift = map.Shift;
			Ratio = map.Ratio;
			if (loadCreatedTime) {
				CreatedTime = map.CreatedTime;
			}
			Stages = map.Stages;
			Tracks = map.Tracks;
			Notes = map.Notes;
			SpeedNotes = map.SpeedNotes;
		}


		public Stage GetStageAt (int index) {
			var stage = index >= 0 && index < Stages.Count ? Stages[index] : null;
			return stage is null ? DEFAULT_STAGE : stage;
		}


		public Track GetTrackAt (int index) {
			var track = index >= 0 && index < Tracks.Count ? Tracks[index] : null;
			return track is null ? DEFAULT_TRACK : track;
		}


		public Note GetNoteAt (int index) => index >= 0 && index < Notes.Count ? Notes[index] : null;


		public static implicit operator bool (Beatmap map) => map != null;


		public void SortNotesByTime () {
			// OldID / Note Map
			var oldID_Note = new Dictionary<int, Note>();
			int noteCount = Notes.Count;
			for (int i = 0; i < noteCount; i++) {
				oldID_Note.Add(i, Notes[i]);
			}
			// Sort
			Notes.Sort(new NoteComparer());
			// Note / NewID Map
			var note_NewID = new Dictionary<Note, int>();
			for (int i = 0; i < noteCount; i++) {
				var note = Notes[i];
				if (note_NewID.ContainsKey(note)) { continue; }
				note_NewID.Add(note, i);
			}
			for (int i = 0; i < noteCount; i++) {
				var note = Notes[i];
				if (!oldID_Note.ContainsKey(note.LinkedNoteIndex)) { continue; }
				var linkedNote = oldID_Note[note.LinkedNoteIndex];
				if (!note_NewID.ContainsKey(linkedNote)) { continue; }
				note.LinkedNoteIndex = note_NewID[linkedNote];
			}
		}



	}

}