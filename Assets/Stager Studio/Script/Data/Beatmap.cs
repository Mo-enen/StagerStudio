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
			public float Time;
			public float Value;
			public byte Tween;
			public TimeFloatTween (float time, float value, byte tween) {
				Time = time;
				Value = value;
				Tween = tween;
			}
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
		public struct TimeByteTween {
			public float Time;
			public byte Value;
			public byte Tween;
			public TimeByteTween (float time, byte value, byte tween) {
				Time = time;
				Value = value;
				Tween = tween;
			}
			public static int Search (List<TimeByteTween> data, float time) {
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
			public float Time;
			public float A;
			public float B;
			public byte Tween;
			public TimeFloatFloatTween (float time, float a, float b, byte tween) {
				Time = time;
				A = a;
				B = b;
				Tween = tween;
			}
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
				Angle = 4,
			}
			// SER-API
			public float Time = 0f;
			public float Duration = 0f;
			public float Speed = 1f;
			public float X = 0f;
			public float Y = 0f;
			public float Width = 1f;
			public float Height = 1f;
			public float Rotation = 0f;
			public float Angle = 0f;
			public List<TimeFloatFloatTween> Positions;
			public List<TimeFloatTween> Rotations;
			public List<TimeFloatTween> Widths;
			public List<TimeFloatTween> Heights;
			public List<TimeFloatTween> Angles;
			// Cache
			[System.NonSerialized] public bool Active = false;
			[System.NonSerialized] public bool Selecting = false;
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
					case MotionType.Angle:
						return Angles is null ? 0 : Angles.Count;
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
			}
			// API - Ser
			public int StageIndex = -1;
			public float Time = 0f;
			public float Duration = 0f;
			public bool HasTray = false;
			public float X = 0f;
			public float Width = 1f;
			public byte Color = 0;
			public List<TimeFloatTween> Xs;
			public List<TimeFloatTween> Widths;
			public List<TimeByteTween> Colors;
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
				}
			}
		}



		[System.Serializable]
		public class Note {

			// SER-API
			public int TrackIndex = -1;
			public int LinkedNoteIndex = -1;
			public float Time = 0f;
			public float Duration = 0f;
			public float X = 0f;
			public float Width = 1f;
			public short ClickSoundIndex = -1;
			public bool Tap = true;
			public byte SwipeX = 1; // 0 = Left, 1 = None, 2 = Right
			public byte SwipeY = 1; // 0 = Down, 1 = None, 2 = Up

			// Cache
			[System.NonSerialized] public bool Active = false;
			[System.NonSerialized] public bool Selecting = false;
			[System.NonSerialized] public float AppearTime = 0f;
			[System.NonSerialized] public float SpeedMuti = float.MinValue;
			[System.NonSerialized] public float NoteDropStart = float.MinValue;
			[System.NonSerialized] public float NoteDropEnd = float.MinValue;

		}



		[System.Serializable]
		public class SpeedNote {
			public float Time;
			public float Speed;
			public SpeedNote (float time, float speed) {
				Time = time;
				Speed = speed;
			}
		}


		// Data
		public const int PALETTE_CLEAR = 9;
		public readonly static Stage DEFAULT_STAGE = new Stage() {
			Time = 0f,
			Duration = float.MaxValue,
			Rotation = 0f,
			Height = 1f,
			Width = 1f,
			X = 0.5f,
			Y = 0f,
			Speed = 1f,
			Angle = 0f,
			Rotations = { },
			Positions = { },
			Heights = { },
			Widths = { },
			Angles = { },
		};
		public readonly static Track DEFAULT_TRACK = new Track() {
			Time = 0f,
			Duration = float.MaxValue,
			Color = PALETTE_CLEAR,
			X = 0.5f,
			Width = 1f,
			HasTray = false,
			StageIndex = -1,
			Xs = { },
			Colors = { },
			Widths = { },
		};
		public string Tag = "Normal";
		public int Level = 1;
		public float DropSpeed = 1f;
		public float BPM = 120f;
		public float Shift = 0f;
		public float Ratio = 1.5f;
		public long CreatedTime = 0;
		public List<Stage> Stages = new List<Stage>();
		public List<Track> Tracks = new List<Track>();
		public List<Note> Notes = new List<Note>();
		public List<SpeedNote> SpeedNotes = new List<SpeedNote>();


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

			var map = Util.BytesToObject(bytes) as Beatmap;
			if (map is null) { return; }

			Tag = map.Tag;
			Level = map.Level;
			DropSpeed = map.DropSpeed;
			BPM = map.BPM;
			Shift = map.Shift;
			Ratio = map.Ratio;
			CreatedTime = map.CreatedTime;
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


		public static implicit operator bool (Beatmap map) => map != null;


		public void SortByTime () {
			SortStagesByTime();
			SortTracksByTime();
			SortNotesByTime();
			SortSpeedNotesByTime();
		}
		public void SortStagesByTime () => Stages.Sort(new StageComparer());
		public void SortTracksByTime () => Tracks.Sort(new TrackComparer());
		public void SortNotesByTime () => Notes.Sort(new NoteComparer());
		public void SortSpeedNotesByTime () => SpeedNotes.Sort(new SpeedNoteComparer());



	}

}