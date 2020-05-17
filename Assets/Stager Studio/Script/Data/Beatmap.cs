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
		public class TimingComparer : IComparer<Timing> {
			public int Compare (Timing x, Timing y) => x.Time.CompareTo(y.Time);
		}
		public class TimeFloatTweenComparer : IComparer<TimeFloatTween> {
			public int Compare (TimeFloatTween x, TimeFloatTween y) => x.Time.CompareTo(y.Time);
		}
		public class TimeIntTweenComparer : IComparer<TimeIntTween> {
			public int Compare (TimeIntTween x, TimeIntTween y) => x.Time.CompareTo(y.Time);
		}
		public class TimeFloatFloatTweenComparer : IComparer<TimeFloatFloatTween> {
			public int Compare (TimeFloatFloatTween x, TimeFloatFloatTween y) => x.Time.CompareTo(y.Time);
		}



		// SUB
		[System.Serializable]
		public struct TimeFloatTween {

			public float Time {
				get => m_Time / 1000f;
				set {
					m_Time = Mathf.RoundToInt(value * 1000f);
				}
			}
			public float Value {
				get => m_Value / 1000f;
				set {
					m_Value = Mathf.RoundToInt(value * 1000f);
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
					m_Time = Mathf.RoundToInt(value * 1000f);
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
					m_Time = Mathf.RoundToInt(value * 1000f);
				}
			}
			public float A {
				get => m_A / 1000f;
				set {
					m_A = Mathf.RoundToInt(value * 1000f);
				}
			}
			public float B {
				get => m_B / 1000f;
				set {
					m_B = Mathf.RoundToInt(value * 1000f);
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
		public class MapItem {


			// Api
			public float Time {
				get => m_Time / 1000f;
				set => m_Time = Mathf.RoundToInt(value * 1000f);
			}
			public float Duration {
				get => m_Duration / 1000f;
				set => m_Duration = Mathf.RoundToInt(Mathf.Clamp(value, 0f, int.MaxValue / 1000f - 1f) * 1000f);
			}
			public float X {
				get => m_X / 1000f;
				set => m_X = Mathf.RoundToInt(value * 1000f);
			}

			// Api_Ser
			public int ItemType = 0;
			public int m_Time = 0;
			public int m_Duration = 0;
			public int m_X = 0;

			// Cache
			[System.NonSerialized] public bool Active = false;
			[System.NonSerialized] public bool TimerActive = false;
			[System.NonSerialized] public float SpeedMuti = 1f;


		}




		[System.Serializable]
		public class Stage : MapItem {


			// API
			public float Speed {
				get => m_Speed / 1000f;
				set => m_Speed = Mathf.RoundToInt(value * 1000f);
			}

			public float Y {
				get => m_Y / 1000f;
				set => m_Y = Mathf.RoundToInt(value * 1000f);
			}
			public float Width {
				get => m_Width / 1000f;
				set => m_Width = Mathf.RoundToInt(value * 1000f);
			}
			public float Height {
				get => m_Height / 1000f;
				set => m_Height = Mathf.RoundToInt(value * 1000f);
			}
			public float Rotation {
				get => m_Rotation;
				set => m_Rotation = Mathf.RoundToInt(value);
			}
			public float PivotY {
				get => m_PivotY / 1000f;
				set => m_PivotY = Mathf.RoundToInt(value * 1000f);
			}

			// SER-API
			public int m_Speed = 1000;
			public int m_Y = 0;
			public int m_Width = 1000;
			public int m_Height = 1000;
			public int m_Rotation = 0;
			public int m_PivotY = 0;
			public int Color = 0;
			public List<TimeFloatFloatTween> Positions;
			public List<TimeFloatTween> Rotations;
			public List<TimeIntTween> Colors;
			public List<TimeFloatTween> Widths;
			public List<TimeFloatTween> Heights;

			// Cache
			[System.NonSerialized] public int TrackCount = 0;


			// API
			public void SortMotion (int index = -1) {
				if (index == -1) {
					Positions.Sort(new TimeFloatFloatTweenComparer());
					Rotations.Sort(new TimeFloatTweenComparer());
					Widths.Sort(new TimeFloatTweenComparer());
					Heights.Sort(new TimeFloatTweenComparer());
					Colors.Sort(new TimeIntTweenComparer());
				} else {
					switch (index) {
						case 0:
							Positions.Sort(new TimeFloatFloatTweenComparer());
							break;
						case 1:
							Rotations.Sort(new TimeFloatTweenComparer());
							break;
						case 2:
							Widths.Sort(new TimeFloatTweenComparer());
							break;
						case 3:
							Heights.Sort(new TimeFloatTweenComparer());
							break;
						case 4:
							Colors.Sort(new TimeIntTweenComparer());
							break;
					}
				}
			}


		}



		[System.Serializable]
		public class Track : MapItem {



			// API
			public float Width {
				get => m_Width / 1000f;
				set => m_Width = Mathf.RoundToInt(value * 1000f);
			}
			public float Angle {
				get => m_Angle;
				set => m_Angle = Mathf.RoundToInt(value);
			}

			// API - Ser
			public int m_Width = 1000;
			public int m_Angle = 0;
			public int StageIndex = -1;
			public int Color = 0;
			public bool HasTray = false;
			public List<TimeFloatTween> Xs;
			public List<TimeFloatTween> Angles;
			public List<TimeIntTween> Colors;
			public List<TimeFloatTween> Widths;

			// Cache
			[System.NonSerialized] public (float min, float max) TrayX = (0.5f, 0.5f);
			[System.NonSerialized] public float TrayTime = float.MaxValue;
			[System.NonSerialized] public Color Tint = UnityEngine.Color.white;


			// API
			public void SortMotion (int index = -1) {
				if (index == -1) {
					Xs.Sort(new TimeFloatTweenComparer());
					Angles.Sort(new TimeFloatTweenComparer());
					Widths.Sort(new TimeFloatTweenComparer());
					Colors.Sort(new TimeIntTweenComparer());
				} else {
					switch (index) {
						case 0:
							Xs.Sort(new TimeFloatTweenComparer());
							break;
						case 1:
							Angles.Sort(new TimeFloatTweenComparer());
							break;
						case 2:
							Widths.Sort(new TimeFloatTweenComparer());
							break;
						case 3:
							Colors.Sort(new TimeIntTweenComparer());
							break;
					}
				}
			}


		}



		[System.Serializable]
		public class Note : MapItem {

			// API
			public float Z {
				get => m_Z / 1000f;
				set => m_Z = Mathf.RoundToInt(value * 1000f);
			}
			public float Width {
				get => m_Width / 1000f;
				set => m_Width = Mathf.RoundToInt(value * 1000f);
			}

			// SER-API
			public int m_Z = 0;
			public int m_Width = 1000;
			public int TrackIndex = -1;
			public int LinkedNoteIndex = -1;
			public short ClickSoundIndex = -1;
			public byte SoundFxIndex = 0;
			public int SoundFxParamA = 0;
			public int SoundFxParamB = 0;

			// Cache
			[System.NonSerialized] public float AppearTime = 0f;
			[System.NonSerialized] public float SpeedOnDrop = 1f;
			[System.NonSerialized] public float NoteDropStart = -1f;
			[System.NonSerialized] public float NoteDropEnd = -1f;
			[System.NonSerialized] public float CacheTime = -1f;
			[System.NonSerialized] public float CacheDuration = -1f;
			[System.NonSerialized] public static int CacheDirtyID = 1;
			[System.NonSerialized] public int LocalCacheDirtyID = 0;

		}



		[System.Serializable]
		public class Timing : MapItem {

			// API
			public float Speed {
				get => m_X / 100f;
				set => m_X = Mathf.RoundToInt(value * 100f);
			}

			// SER
			public byte SoundFxIndex = 0;
			public int SoundFxParamA = 0;
			public int SoundFxParamB = 0;

			// Cache
			[System.NonSerialized] public float AppearTime = -1f;
			[System.NonSerialized] public float NoteDropPos = -1f;
			[System.NonSerialized] public static int CacheDirtyID = 1;
			[System.NonSerialized] public int LocalCacheDirtyID = 0;
			[System.NonSerialized] public float CacheTime = -1f;

			// API
			public Timing (float time, float speed) {
				Time = time;
				Speed = speed;
			}

		}


		// API-SER
		public float Shift {
			get => m_Shift / 1000f;
			set {
				m_Shift = Mathf.RoundToInt(value * 1000f);
			}
		}
		public float Ratio {
			get => m_Ratio / 1000f;
			set {
				m_Ratio = Mathf.RoundToInt(value * 1000f);
			}
		}

		public int BPM = 120;
		public int Level = 1;
		public string Tag = "Normal";
		public long CreatedTime = 0;
		public List<Stage> Stages = new List<Stage>();
		public List<Track> Tracks = new List<Track>();
		public List<Note> Notes = new List<Note>();
		public List<Timing> Timings = new List<Timing>();

		// SER
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
			if (Timings is null) {
				Timings = new List<Timing>();
			}
			// Sort Motion 
			foreach (var stage in Stages) {
				stage.SortMotion();
			}
			foreach (var track in Tracks) {
				track.SortMotion();
			}
		}


		public void LoadFromOtherMap (Beatmap map, bool loadCreatedTime = true) {
			if (map == null) { return; }
			Tag = map.Tag;
			Level = map.Level;
			BPM = map.BPM;
			Shift = map.Shift;
			Ratio = map.Ratio;
			if (loadCreatedTime) {
				CreatedTime = map.CreatedTime;
			}
			Stages = map.Stages;
			Tracks = map.Tracks;
			Notes = map.Notes;
			Timings = map.Timings;
		}


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


		// Get
		public MapItem GetItem (int type, int index) {
			switch (type) {
				case 0:
					return index >= 0 && index < Stages.Count ? Stages[index] : null;
				case 1:
					return index >= 0 && index < Tracks.Count ? Tracks[index] : null;
				case 2:
					return index >= 0 && index < Notes.Count ? Notes[index] : null;
				case 3:
					return index >= 0 && index < Timings.Count ? Timings[index] : null;
			}
			return null;
		}
		public bool GetActive (int type, int index) {
			if (type == 4 || type == 5) {
				// Timer
				var item = GetItem(type - 4, index);
				return item != null ? item.TimerActive : false;
			} else {
				// Item
				var item = GetItem(type, index);
				return item != null ? item.Active : false;
			}
		}
		public float GetTime (int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item.Time : 0f;
		}
		public float GetDuration (int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item.Duration : 0f;
		}
		public int GetParentIndex (int type, int index) {
			switch (type) {
				case 1:
					if (index >= 0 && index < Tracks.Count) {
						return Tracks[index].StageIndex;
					}
					break;
				case 2:
					if (index >= 0 && index < Notes.Count) {
						return Notes[index].TrackIndex;
					}
					break;
			}
			return -1;
		}


		// Set
		public void SetX (int type, int index, float x) {
			var item = GetItem(type, index);
			if (item != null) {
				item.X = x;
			}
		}
		public void SetTime (int type, int index, float time) {
			var item = GetItem(type, index);
			if (item != null) {
				item.Time = time;
			}
		}
		public void SetItemType (int type, int index, int itemType) {
			var item = GetItem(type, index);
			if (item != null) {
				item.ItemType = itemType;
			}
		}
		public void SetItemIndex (int type, int index, int newIndex) {
			switch (type) {
				case 0: // Stage
					if (index >= 0 && index < Stages.Count && newIndex >= 0 && newIndex < Stages.Count) {
						var temp = Stages[index];
						Stages[index] = Stages[newIndex];
						Stages[newIndex] = temp;
						for (int i = 0; i < Tracks.Count; i++) {
							var track = Tracks[i];
							if (track.StageIndex == index) {
								track.StageIndex = newIndex;
							} else if (track.StageIndex == newIndex) {
								track.StageIndex = index;
							}
						}
					}
					break;
				case 1: // Track
					if (index >= 0 && index < Tracks.Count && newIndex >= 0 && newIndex < Tracks.Count) {
						var temp = Tracks[index];
						Tracks[index] = Tracks[newIndex];
						Tracks[newIndex] = temp;
						for (int i = 0; i < Notes.Count; i++) {
							var note = Notes[i];
							if (note.TrackIndex == index) {
								note.TrackIndex = newIndex;
							} else if (note.TrackIndex == newIndex) {
								note.TrackIndex = index;
							}
						}
					}
					break;
				case 2: // Note
					if (index >= 0 && index < Notes.Count && newIndex >= 0 && newIndex < Notes.Count) {
						var temp = Notes[index];
						Notes[index] = Notes[newIndex];
						Notes[newIndex] = temp;
						for (int i = 0; i < Notes.Count; i++) {
							var note = Notes[i];
							if (note.LinkedNoteIndex == index) {
								note.LinkedNoteIndex = newIndex;
							} else if (note.LinkedNoteIndex == newIndex) {
								note.LinkedNoteIndex = index;
							}
						}
					}
					break;
				case 3: // Timing
					if (index >= 0 && index < Timings.Count && newIndex >= 0 && newIndex < Timings.Count) {
						var temp = Timings[index];
						Timings[index] = Timings[newIndex];
						Timings[newIndex] = temp;
					}
					break;
			}




		}
		public void SetDuration (int type, int index, float duration) {
			var item = GetItem(type, index);
			if (item != null) {
				item.Duration = duration;
			}
		}
		public float GetSpeedMuti (int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item.SpeedMuti : 1f;
		}

		public void SetStageY (int index, float y) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Y = y;
			}
		}
		public void SetStageSpeed (int index, float speed) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Speed = speed;
			}
		}
		public void SetStagePivot (int index, float pivot) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].PivotY = pivot;
			}
		}
		public void SetStageRotation (int index, float rot) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Rotation = rot;
			}
		}
		public void SetStageWidth (int index, float width) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Width = width;
			}
		}
		public void SetStageHeight (int index, float height) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Height = height;
			}
		}
		public void SetStageColor (int index, int color) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Color = color;
			}
		}

		public void SetTrackWidth (int index, float width) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].Width = width;
			}
		}
		public void SetTrackAngle (int index, float angle) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].Angle = angle;
			}
		}
		public void SetTrackColor (int index, int color) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].Color = color;
			}
		}
		public void SetTrackTray (int index, bool tray) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].HasTray = tray;
			}
		}
		public void SetTrackStageIndex (int index, int stageIndexForTrack) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].StageIndex = stageIndexForTrack;
			}
		}

		public void SetNoteWidth (int index, float width) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].Width = width;
			}
		}
		public void SetNoteTrackIndex (int index, int trackIndexForNote) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].TrackIndex = trackIndexForNote;
			}
		}
		public void SetNoteLinkedIndex (int index, int linkedIndex) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].LinkedNoteIndex = linkedIndex;
			}
		}
		public void SetNoteZ (int index, float z) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].Z = z;
			}
		}
		public void SetNoteClickIndex (int index, short click) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].ClickSoundIndex = click;
			}
		}
		public void SetNoteSfxIndex (int index, byte sfx) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].SoundFxIndex = sfx;
			}
		}
		public void SetNoteParamA (int index, int param) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].SoundFxParamA = param;
			}
		}
		public void SetNoteParamB (int index, int param) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].SoundFxParamB = param;
			}
		}

		public void SetTimingSpeed (int index, int speed) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].m_X = speed;
			}
		}
		public void SetTimingSfxIndex (int index, byte sfx) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].SoundFxIndex = sfx;
			}
		}
		public void SetTimingParamA (int index, int param) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].SoundFxParamA = param;
			}
		}
		public void SetTimingParamB (int index, int param) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].SoundFxParamB = param;
			}
		}

		// Add
		public void AddStage (
			float time, float duration,
			float x = 0f, float y = 0f,
			float width = 1f, float height = 1f,
			int itemType = 0, float pivotY = 0f,
			float rotation = 0f, float speed = 1f,
			int color = 0
		) => Stages.Add(new Stage() {
			Time = time,
			Duration = duration,
			X = x,
			Y = y,
			Width = width,
			Height = height,
			ItemType = itemType,
			PivotY = pivotY,
			Rotation = rotation,
			Speed = speed,
			Color = color,
			Rotations = { },
			Widths = { },
			Heights = { },
			Colors = { },
			Positions = { },
		});


		public void AddTrack (
			int stageIndex, float time, float duration,
			float x = 0f, float width = 0f, float angle = 0f,
			int color = 0, int itemType = 0,
			bool hasTray = false
		) => Tracks.Add(new Track() {
			StageIndex = stageIndex,
			Time = time,
			Duration = duration,
			X = x,
			Width = width,
			Angle = angle,
			Color = color,
			ItemType = itemType,
			Widths = { },
			Xs = { },
			HasTray = hasTray,
			Colors = { },
			Angles = { },
		});


		public void AddNote (
			int trackIndex, float time, float duration,
			float x = 0f, float width = 0f,
			int linkedNoteIndex = -1, int itemType = 0,
			float z = 0f, byte clickSoundIndex = 0
		) => Notes.Add(new Note() {
			TrackIndex = trackIndex,
			Time = time,
			Duration = duration,
			X = x,
			Z = z,
			Width = width,
			ItemType = itemType,
			ClickSoundIndex = clickSoundIndex,
			LinkedNoteIndex = linkedNoteIndex,
			SoundFxIndex = 0,
			SoundFxParamA = 0,
			SoundFxParamB = 0,
		});


		public void AddTiming (
			float time, float speed, float duration = 0f
		) => Timings.Add(new Timing(time, speed) {
			Duration = duration,
			ItemType = 0,
			SoundFxIndex = 0,
			SoundFxParamA = 0,
			SoundFxParamB = 0,
		});


		// Delete
		public bool DeleteItem (int type, int index) {
			switch (type) {
				case 0:
					if (index >= 0 && index < Stages.Count) {
						Stages.RemoveAt(index);
						foreach (var track in Tracks) {
							if (track.StageIndex > index) {
								track.StageIndex--;
							}
						}
						for (int i = 0; i < Tracks.Count; i++) {
							var track = Tracks[i];
							if (track.StageIndex == index && DeleteItem(1, i)) {
								i--;
							}
						}
						return true;
					}
					break;
				case 1:
					if (index >= 0 && index < Tracks.Count) {
						Tracks.RemoveAt(index);
						foreach (var note in Notes) {
							if (note.TrackIndex > index) {
								note.TrackIndex--;
							}
						}
						for (int i = 0; i < Notes.Count; i++) {
							var note = Notes[i];
							if (note.TrackIndex == index && DeleteItem(2, i)) {
								i--;
							}
						}
						return true;
					}
					break;
				case 2:
					if (index >= 0 && index < Notes.Count) {
						Notes.RemoveAt(index);
						foreach (var note in Notes) {
							if (note.LinkedNoteIndex > index) {
								note.LinkedNoteIndex--;
							} else if (note.LinkedNoteIndex == index) {
								note.LinkedNoteIndex = -1;
							}
						}
						return true;
					}
					break;
				case 3:
					if (index >= 0 && index < Timings.Count) {
						Timings.RemoveAt(index);
						return true;
					}
					break;
			}
			return false;
		}


		// Motion
		private (IList list, MapItem item) GetMotionList (int itemIndex, int motionType) {
			if (motionType >= 0 && motionType <= 4) {
				// Stage
				if (itemIndex < 0 || itemIndex >= Stages.Count) { return (null, null); }
				var stage = Stages[itemIndex];
				switch (motionType) {
					case 0: // Pos
						return (stage.Positions, stage); // -1f～1f
					case 1: // Angle
						return (stage.Rotations, stage); // -360f～360f
					case 2: // Index
						return (stage.Colors, stage); // 0～...
					case 3: // Width
						return (stage.Widths, stage); // 0～1f
					case 4: // Height
						return (stage.Heights, stage); // 0～1f
					default:
						return (null, null);
				}
			} else if (motionType >= 5 && motionType <= 8) {
				// Track
				if (itemIndex < 0 || itemIndex >= Tracks.Count) { return (null, null); }
				var track = Tracks[itemIndex];
				switch (motionType) {
					case 5: // X
						return (track.Xs, track); // -1f～1f
					case 6: // Angle
						return (track.Angles, track); // -360f～360f
					case 7: // Index
						return (track.Colors, track); // 0～...
					case 8: // Width
						return (track.Widths, track); // 0～1f
					default:
						return (null, null);
				}
			} else {
				return (null, null);
			}
		}


		public int GetMotionCount (int itemIndex, int motionType) {
			var list = GetMotionList(itemIndex, motionType).list;
			return list != null ? list.Count : 0;
		}


		private (object motionObj, MapItem item) GetMotionItem (int itemIndex, int motionType, int motionIndex) {
			var (list, item) = GetMotionList(itemIndex, motionType);
			return (list != null && motionIndex >= 0 && motionIndex < list.Count ? list[motionIndex] : null, item);
		}


		private void SetMotionItem (int itemIndex, int motionType, int motionIndex, object item) {
			var list = GetMotionList(itemIndex, motionType).list;
			if (list != null && motionIndex >= 0 && motionIndex < list.Count) {
				list[motionIndex] = item;
			}
		}


		public bool GetMotionTime (int itemIndex, int motionType, int motionIndex, out float time, out float itemTime) {
			var (motionObj, item) = GetMotionItem(itemIndex, motionType, motionIndex);
			time = itemTime = 0f;
			if (motionObj == null || item == null) { return false; }
			itemTime = item.Time;
			if (motionObj is TimeIntTween) {
				time = ((TimeIntTween)motionObj).Time;
				return true;
			} else if (motionObj is TimeFloatTween) {
				time = ((TimeFloatTween)motionObj).Time;
				return true;
			} else if (motionObj is TimeFloatFloatTween) {
				time = ((TimeFloatFloatTween)motionObj).Time;
				return true;
			} else {
				return false;
			}
		}


		public void SetMotionTime (int itemIndex, int motionType, int motionIndex, float time) {
			var motionObj = GetMotionItem(itemIndex, motionType, motionIndex).motionObj;
			if (motionObj == null) { return; }
			if (motionObj is TimeIntTween tiItem) {
				tiItem.Time = time;
				SetMotionItem(itemIndex, motionType, motionIndex, tiItem);
			} else if (motionObj is TimeFloatTween tfItem) {
				tfItem.Time = time;
				SetMotionItem(itemIndex, motionType, motionIndex, tfItem);
			} else if (motionObj is TimeFloatFloatTween tffItem) {
				tffItem.Time = time;
				SetMotionItem(itemIndex, motionType, motionIndex, tffItem);
			}
		}


		public (bool hasA, bool hasB) GetMotionValue (int itemIndex, int motionType, int motionIndex, out float valueA, out float valueB, out int tween) {
			valueA = valueB = 0f;
			tween = 0;
			var (motionObj, item) = GetMotionItem(itemIndex, motionType, motionIndex);
			if (motionObj == null || item == null) { return (false, false); }
			if (motionObj is TimeIntTween iItem) {
				valueA = valueB = iItem.Value;
				tween = iItem.Tween;
				return (true, false);
			} else if (motionObj is TimeFloatTween fItem) {
				valueA = valueB = fItem.Value;
				tween = fItem.Tween;
				return (true, false);
			} else if (motionObj is TimeFloatFloatTween ffItem) {
				valueA = ffItem.A;
				valueB = ffItem.B;
				tween = ffItem.Tween;
				return (true, true);
			} else {
				return (false, false);
			}
		}


		public void SetMotionValueTween (int itemIndex, int motionType, int motionIndex, float? valueA = null, float? valueB = null, int? tween = null) {
			var list = GetMotionList(itemIndex, motionType).list;
			if (list != null && motionIndex >= 0 && motionIndex < list.Count) {
				var item = list[motionIndex];
				if (valueA.HasValue) {
					if (item is TimeIntTween tItem) {
						tItem.Value = Mathf.RoundToInt(valueA.Value);
						list[motionIndex] = tItem;
					} else if (item is TimeFloatTween fItem) {
						fItem.Value = valueA.Value;
						list[motionIndex] = fItem;
					} else if (item is TimeFloatFloatTween ffItem) {
						ffItem.A = valueA.Value;
						list[motionIndex] = ffItem;
					}
				}
				if (valueB.HasValue) {
					if (item is TimeFloatFloatTween ffItem) {
						ffItem.B = valueB.Value;
						list[motionIndex] = ffItem;
					}
				}
				if (tween.HasValue) {
					if (item is TimeIntTween tItem) {
						tItem.Tween = tween.Value;
						list[motionIndex] = tItem;
					} else if (item is TimeFloatTween fItem) {
						fItem.Tween = tween.Value;
						list[motionIndex] = fItem;
					} else if (item is TimeFloatFloatTween ffItem) {
						ffItem.Tween = tween.Value;
						list[motionIndex] = ffItem;
					}
				}
			}
		}


		public void AddMotion (int itemIndex, int motionType, float time, float? value) {
			if (motionType >= 0 && motionType <= 4) {
				// Stage
				if (itemIndex >= 0 && itemIndex < Stages.Count) {
					var stage = Stages[itemIndex];
					switch (motionType) {
						case 0:
							stage.Positions.Add(new TimeFloatFloatTween() {
								Time = time,
								Tween = 0,
								A = value ?? 0f,
								B = value ?? 0f,
							});
							break;
						case 1:
							stage.Rotations.Add(new TimeFloatTween() {
								Time = time,
								Tween = 0,
								Value = value ?? 0f,
							});
							break;
						case 2:
							stage.Colors.Add(new TimeIntTween() {
								Time = time,
								Tween = 0,
								Value = (int)(value ?? 0),
							});
							break;
						case 3:
							stage.Widths.Add(new TimeFloatTween() {
								Time = time,
								Tween = 0,
								Value = value ?? 1f,
							});
							break;
						case 4:
							stage.Heights.Add(new TimeFloatTween() {
								Time = time,
								Tween = 0,
								Value = value ?? 1f,
							});
							break;

					}
					stage.SortMotion(motionType);
				}
			} else if (motionType >= 5 && motionType <= 8) {
				// Track
				if (itemIndex >= 0 && itemIndex < Tracks.Count) {
					var track = Tracks[itemIndex];
					switch (motionType) {
						case 5:
							track.Xs.Add(new TimeFloatTween() {
								Time = time,
								Tween = 0,
								Value = value ?? 0f,
							});
							break;
						case 6:
							track.Angles.Add(new TimeFloatTween() {
								Time = time,
								Tween = 0,
								Value = value ?? 0f,
							});
							break;
						case 7:
							track.Colors.Add(new TimeIntTween() {
								Time = time,
								Tween = 0,
								Value = (int)(value ?? 0),
							});
							break;
						case 8:
							track.Widths.Add(new TimeFloatTween() {
								Time = time,
								Tween = 0,
								Value = value ?? 1f,
							});
							break;

					}
					track.SortMotion(motionType);
				}
			}
		}


		public bool DeleteMotion (int itemIndex, int motionType, int motionIndex) {
			var list = GetMotionList(itemIndex, motionType).list;
			if (list != null && motionIndex >= 0 && motionIndex < list.Count) {
				list.RemoveAt(motionIndex);
				return true;
			}
			return false;
		}


	}
}