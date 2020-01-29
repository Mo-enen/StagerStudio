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
			public float Time;
			public float Duration;
			public float Speed;
			public float X;
			public float Y;
			public float Width;
			public float Height;
			public float Rotation;
			public float Disc;
			public byte Color;
			public List<TimeFloatFloatTween> Positions;
			public List<TimeFloatTween> Widths;
			public List<TimeFloatTween> Heights;
			public List<TimeFloatTween> Rotations;
			public List<TimeFloatTween> Discs;
			public List<TimeByteTween> Colors;
			// API
			public int GetMotionCount () =>
				(Positions is null ? 1 : (Positions.Count + 1)) +
				(Widths is null ? 1 : (Widths.Count + 1)) +
				(Heights is null ? 1 : (Heights.Count + 1)) +
				(Rotations is null ? 1 : (Rotations.Count + 1)) +
				(Discs is null ? 1 : (Discs.Count + 1)) +
				(Colors is null ? 1 : (Colors.Count + 1));
		}



		[System.Serializable]
		public class Track {
			public int StageIndex;
			public float Time;
			public float Duration;
			public bool HasTray;
			public float X;
			public float Rotation;
			public float Width;
			public byte Color;
			public List<TimeFloatTween> Xs;
			public List<TimeFloatTween> Rotations;
			public List<TimeFloatTween> Widths;
			public List<TimeByteTween> Colors;
			// API
			public int GetMotionCount () =>
				(Xs is null ? 1 : (Xs.Count + 1)) +
				(Rotations is null ? 1 : (Rotations.Count + 1)) +
				(Widths is null ? 1 : (Widths.Count + 1)) +
				(Colors is null ? 1 : (Colors.Count + 1));
		}



		[System.Serializable]
		public class Note {

			// API
			public bool? SwipeX {
				get {
					if (m_SwipeX == 0) {
						return null;
					} else {
						return m_SwipeX == 1;
					}
				}
				set {
					m_SwipeX = (byte)(value.HasValue ? value.Value ? 1 : 2 : 0);
				}
			}

			public bool? SwipeY {
				get {
					if (m_SwipeY == 0) {
						return null;
					} else {
						return m_SwipeY == 1;
					}
				}
				set {
					m_SwipeY = (byte)(value.HasValue ? value.Value ? 1 : 2 : 0);
				}
			}

			// SER-API
			public int TrackIndex;
			public int LinkedNoteIndex;
			public float Time;
			public float Duration;
			public float X;
			public float Width;
			public byte ClickSoundIndex;
			public bool Tap;
			public byte m_SwipeX;
			public byte m_SwipeY;

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
			Disc = 0f,
			Rotation = 0f,
			Height = 1f,
			Width = 1f,
			X = 0.5f,
			Y = 0f,
			Speed = 1f,
			Rotations = { },
			Colors = { },
			Discs = { },
			Positions = { },
			Heights = { },
			Widths = { },
		};
		public readonly static Track DEFAULT_TRACK = new Track() {
			Time = 0f,
			Duration = float.MaxValue,
			Color = PALETTE_CLEAR,
			X = 0.5f,
			Width = 1f,
			HasTray = false,
			StageIndex = -1,
			Rotation = 0f,
			Xs = { },
			Colors = { },
			Rotations = { },
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