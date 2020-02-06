namespace StagerStudio.Data {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;



	[System.Serializable]
	public class Beatmap {



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
			// SER-API
			public float Time = 0f;
			public float Duration = 0f;
			public float Speed = 1f;
			public float X = 0f;
			public float Y = 0f;
			public float Width = 1f;
			public float Height = 1f;
			public float Rotation = 0f;
			public byte Color = 0;
			public float Angle = 0f;
			public List<TimeFloatTween> Rotations;
			public List<TimeFloatFloatTween> Positions;
			public List<TimeFloatTween> Widths;
			public List<TimeFloatTween> Heights;
			public List<TimeFloatTween> Angles;
			public List<TimeByteTween> Colors;
			// API
			public int GetMotionCount () =>
				(Positions is null ? 1 : (Positions.Count + 1)) +
				(Positions is null ? 1 : (Positions.Count + 1)) +
				(Widths is null ? 1 : (Widths.Count + 1)) +
				(Heights is null ? 1 : (Heights.Count + 1)) +
				(Angles is null ? 1 : (Angles.Count + 1)) +
				(Colors is null ? 1 : (Colors.Count + 1));
		}



		[System.Serializable]
		public class Track {
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
			// API
			public int GetMotionCount () =>
				(Xs is null ? 1 : (Xs.Count + 1)) +
				(Widths is null ? 1 : (Widths.Count + 1)) +
				(Colors is null ? 1 : (Colors.Count + 1));
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
			public byte ClickSoundIndex = 0;
			public bool Tap = true;
			public byte SwipeX = 1; // 0 = Left, 1 = None, 2 = Right
			public byte SwipeY = 1; // 0 = Down, 1 = None, 2 = Up

			// Cache
			[System.NonSerialized] public float AppearTime = 0f;
			[System.NonSerialized] public float SpeedMuti = float.MinValue;
			[System.NonSerialized] public float NoteDropStart = float.MinValue;
			[System.NonSerialized] public float NoteDropEnd = float.MinValue;

		}



		[System.Serializable]
		public class SpeedNote {
			public float Time;
			public float Duration;
			public float Speed;
			public SpeedNote (float time, float duration, float speed) {
				Time = time;
				Duration = duration;
				Speed = speed;
			}
		}


		// Data
		public const int PALETTE_CLEAR = 9;
		public readonly static Stage DEFAULT_STAGE = new Stage() {
			Time = 0f,
			Duration = float.MaxValue,
			Color = PALETTE_CLEAR,
			Rotation = 0f,
			Height = 1f,
			Width = 1f,
			X = 0.5f,
			Y = 0f,
			Speed = 1f,
			Angle = 0f,
			Rotations = { },
			Colors = { },
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
		public static Beatmap NewBeatmap () => new Beatmap() {
			CreatedTime = System.DateTime.Now.Ticks,
		};


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


	}

}