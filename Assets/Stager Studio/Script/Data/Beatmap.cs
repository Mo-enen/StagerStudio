namespace StagerStudio.Data {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;




	[System.Serializable]
	public class Beatmap {



		// Comparer
		public class StageComparer : IComparer<Stage> {
			public int Compare(Stage x, Stage y) => x.Time.CompareTo(y.Time);
		}
		public class TrackComparer : IComparer<Track> {
			public int Compare(Track x, Track y) => x.Time.CompareTo(y.Time);
		}
		public class NoteComparer : IComparer<Note> {
			public int Compare(Note x, Note y) => x.Time.CompareTo(y.Time);
		}
		public class TimingComparer : IComparer<Timing> {
			public int Compare(Timing x, Timing y) => x.Time.CompareTo(y.Time);
		}
		public class TimeFloatTweenComparer : IComparer<TimeFloatTween> {
			public int Compare(TimeFloatTween x, TimeFloatTween y) => x.Time.CompareTo(y.Time);
		}
		public class TimeIntTweenComparer : IComparer<TimeIntTween> {
			public int Compare(TimeIntTween x, TimeIntTween y) => x.Time.CompareTo(y.Time);
		}
		public class TimeFloatFloatTweenComparer : IComparer<TimeFloatFloatTween> {
			public int Compare(TimeFloatFloatTween x, TimeFloatFloatTween y) => x.Time.CompareTo(y.Time);
		}



		// SUB
		[System.Serializable]
		public struct TimeFloatTween {

			public float Time {
				get => time / 1000f;
				set {
					time = Mathf.RoundToInt(value * 1000f);
				}
			}
			public float Value {
				get => value / 1000f;
				set {
					this.value = Mathf.RoundToInt(value * 1000f);
				}
			}
			public int Tween {
				get => tween;
				set {
					tween = value;
				}
			}

			public int time;
			public int value;
			public int tween;

			public static int Search(List<TimeFloatTween> data, float time) {
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

			public static void FixOverlap(List<TimeFloatTween> data) {
				if (data == null) { return; }
				for (int i = 0; i < data.Count - 1; i++) {
					var a = data[i];
					while (i < data.Count - 1) {
						var b = data[i + 1];
						if (a.time == b.time && a.value == b.value) {
							data.RemoveAt(i + 1);
						} else {
							break;
						}
					}
				}
			}

		}


		[System.Serializable]
		public struct TimeIntTween {

			public float Time {
				get => time / 1000f;
				set {
					time = Mathf.RoundToInt(value * 1000f);
				}
			}
			public int Value {
				get => value;
				set => this.value = value;
			}
			public int Tween {
				get => tween;
				set {
					tween = value;
				}
			}

			public int time;
			public int value;
			public int tween;

			public static int Search(List<TimeIntTween> data, float time) {
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


			public static void FixOverlap(List<TimeIntTween> data) {
				if (data == null) { return; }
				for (int i = 0; i < data.Count - 1; i++) {
					var a = data[i];
					while (i < data.Count - 1) {
						var b = data[i + 1];
						if (a.time == b.time && a.value == b.value) {
							data.RemoveAt(i + 1);
						} else {
							break;
						}
					}
				}
			}


		}


		[System.Serializable]
		public struct TimeFloatFloatTween {

			public float Time {
				get => time / 1000f;
				set {
					time = Mathf.RoundToInt(value * 1000f);
				}
			}
			public float A {
				get => a / 1000f;
				set {
					a = Mathf.RoundToInt(value * 1000f);
				}
			}
			public float B {
				get => b / 1000f;
				set {
					b = Mathf.RoundToInt(value * 1000f);
				}
			}
			public int Tween {
				get => tween;
				set {
					tween = (byte)value;
				}
			}

			public int time;
			public int a;
			public int b;
			public byte tween;

			public static int Search(List<TimeFloatFloatTween> data, float time) {
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


			public static void FixOverlap(List<TimeFloatFloatTween> data) {
				if (data == null) { return; }
				for (int i = 0; i < data.Count - 1; i++) {
					var a = data[i];
					while (i < data.Count - 1) {
						var b = data[i + 1];
						if (a.time == b.time && a.a == b.a && a.b == b.b) {
							data.RemoveAt(i + 1);
						} else {
							break;
						}
					}
				}
			}


		}


		[System.Serializable]
		public class MapItem {


			// Api
			public float Time {
				get => time / 1000f;
				set => time = Mathf.RoundToInt(value * 1000f);
			}
			public float Duration {
				get => duration / 1000f;
				set => duration = Mathf.RoundToInt(Mathf.Clamp(value, 0f, int.MaxValue / 1000f - 1f) * 1000f);
			}
			public float X {
				get => x / 1000f;
				set => x = Mathf.RoundToInt(value * 1000f);
			}
			public int ItemType {
				get => itemType;
				set => itemType = value;
			}
			public float Speed {
				get => speed / 1000f;
				set => speed = Mathf.RoundToInt(value * 1000f);
			}

			// Ser
			public int itemType = 0;
			public int time = 0;
			public int duration = 0;
			public int x = 0;
			public int speed = 1000;

			// Cache
			[System.NonSerialized] public bool _Active = false;
			[System.NonSerialized] public bool _TimerActive = false;
			[System.NonSerialized] public float _SpeedMuti = 1f;


		}




		[System.Serializable]
		public class Stage : MapItem {


			// API
			public float Y {
				get => y / 1000f;
				set => y = Mathf.RoundToInt(value * 1000f);
			}
			public float Width {
				get => width / 1000f;
				set => width = Mathf.RoundToInt(value * 1000f);
			}
			public float Height {
				get => height / 1000f;
				set => height = Mathf.RoundToInt(value * 1000f);
			}
			public float Rotation {
				get => rotation;
				set => rotation = Mathf.RoundToInt(value);
			}
			public float PivotY {
				get => pivotY / 1000f;
				set => pivotY = Mathf.RoundToInt(value * 1000f);
			}
			public int Color {
				get => color;
				set => color = value;
			}
			public List<TimeFloatFloatTween> Positions {
				get => positions;
				set => positions = value;
			}
			public List<TimeFloatTween> Rotations {
				get => rotations;
				set => rotations = value;
			}
			public List<TimeIntTween> Colors {
				get => colors;
				set => colors = value;
			}
			public List<TimeFloatTween> Widths {
				get => widths;
				set => widths = value;
			}
			public List<TimeFloatTween> Heights {
				get => heights;
				set => heights = value;
			}

			// SER-API
			public int y = 0;
			public int width = 1000;
			public int height = 1000;
			public int rotation = 0;
			public int pivotY = 0;
			public int color = 0;
			public List<TimeFloatFloatTween> positions;
			public List<TimeFloatTween> rotations;
			public List<TimeIntTween> colors;
			public List<TimeFloatTween> widths;
			public List<TimeFloatTween> heights;

			// Cache
			[System.NonSerialized] public int c_TrackCount = 0;


			// API
			public void SortMotion(int motionType = -1) {
				if (motionType == -1) {
					Positions.Sort(new TimeFloatFloatTweenComparer());
					Rotations.Sort(new TimeFloatTweenComparer());
					Widths.Sort(new TimeFloatTweenComparer());
					Heights.Sort(new TimeFloatTweenComparer());
					Colors.Sort(new TimeIntTweenComparer());
				} else {
					switch (motionType) {
						case 0:
							Positions.Sort(new TimeFloatFloatTweenComparer());
							break;
						case 1:
							Rotations.Sort(new TimeFloatTweenComparer());
							break;
						case 2:
							Colors.Sort(new TimeIntTweenComparer());
							break;
						case 3:
							Widths.Sort(new TimeFloatTweenComparer());
							break;
						case 4:
							Heights.Sort(new TimeFloatTweenComparer());
							break;
					}
				}
			}


			public void FixOverlapMotion() {
				TimeFloatFloatTween.FixOverlap(Positions);
				TimeFloatTween.FixOverlap(Rotations);
				TimeIntTween.FixOverlap(Colors);
				TimeFloatTween.FixOverlap(Widths);
				TimeFloatTween.FixOverlap(Heights);
			}


		}



		[System.Serializable]
		public class Track : MapItem {



			// API
			public float Width {
				get => width / 1000f;
				set => width = Mathf.RoundToInt(value * 1000f);
			}
			public float Angle {
				get => angle;
				set => angle = Mathf.RoundToInt(value);
			}
			public int StageIndex {
				get => stageIndex;
				set => stageIndex = value;
			}
			public int Color {
				get => color;
				set => color = value;
			}
			public bool HasTray {
				get => hasTray;
				set => hasTray = value;
			}
			public List<TimeFloatTween> Xs {
				get => xs;
				set => xs = value;
			}
			public List<TimeFloatTween> Angles {
				get => angles;
				set => angles = value;
			}
			public List<TimeIntTween> Colors {
				get => colors;
				set => colors = value;
			}
			public List<TimeFloatTween> Widths {
				get => widths;
				set => widths = value;
			}

			// API - Ser
			public int width = 1000;
			public int angle = 0;
			public int stageIndex = -1;
			public int color = 0;
			public bool hasTray = false;
			public List<TimeFloatTween> xs;
			public List<TimeFloatTween> angles;
			public List<TimeIntTween> colors;
			public List<TimeFloatTween> widths;

			// Cache
			[System.NonSerialized] public (float min, float max) c_TrayX = (0.5f, 0.5f);
			[System.NonSerialized] public float c_TrayTime = float.MaxValue;
			[System.NonSerialized] public Color c_Tint = UnityEngine.Color.white;


			// API
			public void SortMotion(int motionType = -1) {
				if (motionType == -1) {
					Xs.Sort(new TimeFloatTweenComparer());
					Angles.Sort(new TimeFloatTweenComparer());
					Widths.Sort(new TimeFloatTweenComparer());
					Colors.Sort(new TimeIntTweenComparer());
				} else {
					switch (motionType) {
						case 5:
							Xs.Sort(new TimeFloatTweenComparer());
							break;
						case 6:
							Angles.Sort(new TimeFloatTweenComparer());
							break;
						case 7:
							Colors.Sort(new TimeIntTweenComparer());
							break;
						case 8:
							Widths.Sort(new TimeFloatTweenComparer());
							break;
					}
				}
			}


			public void FixOverlapMotion() {
				TimeFloatTween.FixOverlap(Xs);
				TimeFloatTween.FixOverlap(Angles);
				TimeIntTween.FixOverlap(Colors);
				TimeFloatTween.FixOverlap(Widths);
			}


		}



		[System.Serializable]
		public class Note : MapItem {

			// API
			public float Z {
				get => z / 1000f;
				set => z = Mathf.RoundToInt(value * 1000f);
			}
			public float Width {
				get => width / 1000f;
				set => width = Mathf.RoundToInt(value * 1000f);
			}
			public int TrackIndex {
				get => trackIndex;
				set => trackIndex = value;
			}
			public int LinkedNoteIndex {
				get => linkedNoteIndex;
				set => linkedNoteIndex = value;
			}
			public short ClickSoundIndex {
				get => clickSoundIndex;
				set => clickSoundIndex = value;
			}
			public byte SoundFxIndex {
				get => soundFxIndex;
				set => soundFxIndex = value;
			}
			public int SoundFxParamA {
				get => soundFxParamA;
				set => soundFxParamA = value;
			}
			public int SoundFxParamB {
				get => soundFxParamB;
				set => soundFxParamB = value;
			}

			// SER-API
			public int z = 0;
			public int width = 1000;
			public int trackIndex = -1;
			public int linkedNoteIndex = -1;
			public short clickSoundIndex = -1;
			public byte soundFxIndex = 0;
			public int soundFxParamA = 0;
			public int soundFxParamB = 0;

			// Cache
			[System.NonSerialized] public static int _CacheDirtyID = 1;
			[System.NonSerialized] public float _AppearTime = 0f;
			[System.NonSerialized] public float _SpeedOnDrop = 1f;
			[System.NonSerialized] public float _NoteDropStart = -1f;
			[System.NonSerialized] public float _NoteDropEnd = -1f;
			[System.NonSerialized] public float _CacheTime = -1f;
			[System.NonSerialized] public float _CacheDuration = -1f;
			[System.NonSerialized] public int _LocalCacheDirtyID = 0;

		}



		[System.Serializable]
		public class Timing : MapItem {

			// API
			public float Value {
				get => x / 100f;
				set => x = Mathf.RoundToInt(value * 100f);
			}
			public byte SoundFxIndex {
				get => soundFxIndex;
				set => soundFxIndex = value;
			}
			public int SoundFxParamA {
				get => soundFxParamA;
				set => soundFxParamA = value;
			}
			public int SoundFxParamB {
				get => soundFxParamB;
				set => soundFxParamB = value;
			}

			// SER
			public byte soundFxIndex = 0;
			public int soundFxParamA = 0;
			public int soundFxParamB = 0;

			// Cache
			[System.NonSerialized] public static int _CacheDirtyID = 1;
			[System.NonSerialized] public float _AppearTime = -1f;
			[System.NonSerialized] public float _NoteDropPos = -1f;
			[System.NonSerialized] public int _LocalCacheDirtyID = 0;
			[System.NonSerialized] public float _CacheTime = -1f;

			// API
			public Timing(float time, float value) {
				Time = time;
				Value = value;
			}

		}

		// Api
		public float Shift {
			get => shift / 1000f;
			set {
				shift = Mathf.RoundToInt(value * 1000f);
			}
		}
		public float Ratio {
			get => ratio / 1000f;
			set {
				ratio = Mathf.RoundToInt(value * 1000f);
			}
		}
		public int BPM {
			get => bpm;
			set => bpm = value;
		}
		public int Level {
			get => level;
			set => level = value;
		}
		public string Tag {
			get => tag;
			set => tag = value;
		}
		public long CreatedTime {
			get => createdTime;
			set => createdTime = value;
		}
		public List<Stage> Stages {
			get => stages;
			set => stages = value;
		}
		public List<Track> Tracks {
			get => tracks;
			set => tracks = value;
		}
		public List<Note> Notes {
			get => notes;
			set => notes = value;
		}
		public List<Timing> Timings {
			get => timings;
			set => timings = value;
		}

		// SER
		public int bpm = 120;
		public int level = 1;
		public string tag = "Normal";
		public long createdTime = 0;
		public int shift = 0;
		public int ratio = 1500;
		public List<Stage> stages = new List<Stage>();
		public List<Track> tracks = new List<Track>();
		public List<Note> notes = new List<Note>();
		public List<Timing> timings = new List<Timing>();


		// Beatmap
		public static Beatmap NewBeatmap() {
			var map = new Beatmap() {
				CreatedTime = System.DateTime.Now.Ticks,
			};
			map.FixEmpty();
			return map;
		}


		public void FixEmpty() {

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

			// Fix Overlap Motions
			foreach (var stage in Stages) {
				stage.FixOverlapMotion();
			}
			foreach (var track in Tracks) {
				track.FixOverlapMotion();
			}

			// Sort Motion 
			foreach (var stage in Stages) {
				stage.SortMotion();
			}
			foreach (var track in Tracks) {
				track.SortMotion();
			}
		}


		public void LoadFromOtherMap(Beatmap map, bool loadCreatedTime = true) {
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


		// Sort
		public void SortNotesByTime() {
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
		public MapItem GetItem(int type, int index) {
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
		public bool GetActive(int type, int index) {
			if (type == 4 || type == 5) {
				// Timer
				var item = GetItem(type - 4, index);
				return item != null && item._TimerActive;
			} else {
				// Item
				var item = GetItem(type, index);
				return item != null && item._Active;
			}
		}
		public float GetTime(int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item.Time : 0f;
		}
		public float GetDuration(int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item.Duration : 0f;
		}
		public int GetParentIndex(int type, int index) {
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
		public float GetX(int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item.X : 0f;
		}
		public float GetStageY(int index) {
			return index >= 0 && index < Stages.Count ? Stages[index].Y : 0f;
		}
		public float GetStagePivot(int index) {
			return index >= 0 && index < Stages.Count ? Stages[index].PivotY : 0f;
		}
		public int GetItemType(int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item.ItemType : 0;
		}


		// Set
		public void SetX(int type, int index, float x) {
			var item = GetItem(type, index);
			if (item != null) {
				item.X = x;
			}
		}
		public void SetTime(int type, int index, float time) {
			var item = GetItem(type, index);
			if (item != null) {
				item.Time = time;
			}
		}
		public void SetItemType(int type, int index, int itemType) {
			var item = GetItem(type, index);
			if (item != null) {
				item.ItemType = itemType;
			}
		}
		public void SetItemIndex(int type, int index, int newIndex) {
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
		public void SetDuration(int type, int index, float duration) {
			var item = GetItem(type, index);
			if (item != null) {
				item.Duration = duration;
			}
		}
		public void SetSpeed(int type, int index, float speed) {
			var item = GetItem(type, index);
			if (item != null) {
				item.Speed = speed;
			}
		}
		public float GetSpeedMuti(int type, int index) {
			var item = GetItem(type, index);
			return item != null ? item._SpeedMuti : 1f;
		}

		public void SetStageY(int index, float y) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Y = y;
			}
		}
		public void SetStagePivot(int index, float pivot) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].PivotY = pivot;
			}
		}
		public void SetStageRotation(int index, float rot) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Rotation = rot;
			}
		}
		public void SetStageWidth(int index, float width) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Width = width;
			}
		}
		public void SetStageHeight(int index, float height) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Height = height;
			}
		}
		public void SetStageColor(int index, int color) {
			if (index >= 0 && index < Stages.Count) {
				Stages[index].Color = color;
			}
		}

		public void SetTrackWidth(int index, float width) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].Width = width;
			}
		}
		public void SetTrackAngle(int index, float angle) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].Angle = angle;
			}
		}
		public void SetTrackColor(int index, int color) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].Color = color;
			}
		}
		public void SetTrackTray(int index, bool tray) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].HasTray = tray;
			}
		}
		public void SetTrackStageIndex(int index, int stageIndexForTrack) {
			if (index >= 0 && index < Tracks.Count) {
				Tracks[index].StageIndex = stageIndexForTrack;
			}
		}

		public void SetNoteWidth(int index, float width) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].Width = width;
			}
		}
		public void SetNoteTrackIndex(int index, int trackIndexForNote) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].TrackIndex = trackIndexForNote;
			}
		}
		public void SetNoteLinkedIndex(int index, int linkedIndex) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].LinkedNoteIndex = linkedIndex;
			}
		}
		public void SetNoteZ(int index, float z) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].Z = z;
			}
		}
		public void SetNoteClickIndex(int index, short click) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].ClickSoundIndex = click;
			}
		}
		public void SetNoteSfxIndex(int index, byte sfx) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].SoundFxIndex = sfx;
			}
		}
		public void SetNoteParamA(int index, int param) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].SoundFxParamA = param;
			}
		}
		public void SetNoteParamB(int index, int param) {
			if (index >= 0 && index < Notes.Count) {
				Notes[index].SoundFxParamB = param;
			}
		}

		public void SetTimingX(int index, int x) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].x = x;
			}
		}
		public void SetTimingSfxIndex(int index, byte sfx) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].SoundFxIndex = sfx;
			}
		}
		public void SetTimingParamA(int index, int param) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].SoundFxParamA = param;
			}
		}
		public void SetTimingParamB(int index, int param) {
			if (index >= 0 && index < Timings.Count) {
				Timings[index].SoundFxParamB = param;
			}
		}


		// Item - Add
		public void AddStage(
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


		public void AddTrack(
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


		public void AddNote(
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


		public void AddTiming(
			float time, float speed, float duration = 0f
		) => Timings.Add(new Timing(time, speed) {
			Duration = duration,
			ItemType = 0,
			SoundFxIndex = 0,
			SoundFxParamA = 0,
			SoundFxParamB = 0,
		});


		public void AddStage(Stage stage) => Stages.Add(stage);
		public void AddTrack(Track track) => Tracks.Add(track);
		public void AddNote(Note note) => Notes.Add(note);
		public void AddTiming(Timing timing) => Timings.Add(timing);


		// Item - Delete
		public bool DeleteItem(int type, int index) {
			switch (type) {
				case 0:
					if (index >= 0 && index < Stages.Count) {
						for (int i = 0; i < Tracks.Count; i++) {
							var track = Tracks[i];
							if (track.StageIndex == index && DeleteItem(1, i)) {
								i--;
							}
						}
						foreach (var track in Tracks) {
							if (track.StageIndex > index) {
								track.StageIndex--;
							}
						}
						Stages.RemoveAt(index);
						return true;
					}
					break;
				case 1:
					if (index >= 0 && index < Tracks.Count) {
						for (int i = 0; i < Notes.Count; i++) {
							var note = Notes[i];
							if (note.TrackIndex == index && DeleteItem(2, i)) {
								i--;
							}
						}
						foreach (var note in Notes) {
							if (note.TrackIndex > index) {
								note.TrackIndex--;
							}
						}
						Tracks.RemoveAt(index);
						return true;
					}
					break;
				case 2:
					if (index >= 0 && index < Notes.Count) {
						foreach (var note in Notes) {
							if (note.LinkedNoteIndex > index) {
								note.LinkedNoteIndex--;
							} else if (note.LinkedNoteIndex == index) {
								note.LinkedNoteIndex = -1;
							}
						}
						Notes.RemoveAt(index);
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


		// Motion - List
		public IList GetMotionList(int itemIndex, int motionType) {
			if (motionType >= 0 && motionType <= 4) {
				// Stage
				if (itemIndex < 0 || itemIndex >= Stages.Count) { return null; }
				var stage = Stages[itemIndex];
				switch (motionType) {
					case 0: // Pos
						return stage.Positions; // -1f～1f
					case 1: // Angle
						return stage.Rotations; // -360f～360f
					case 2: // Index
						return stage.Colors; // 0～...
					case 3: // Width
						return stage.Widths; // 0～1f
					case 4: // Height
						return stage.Heights; // 0～1f
					default:
						return null;
				}
			} else if (motionType >= 5 && motionType <= 8) {
				// Track
				if (itemIndex < 0 || itemIndex >= Tracks.Count) { return null; }
				var track = Tracks[itemIndex];
				switch (motionType) {
					case 5: // X
						return track.Xs; // -1f～1f
					case 6: // Angle
						return track.Angles; // -360f～360f
					case 7: // Index
						return track.Colors; // 0～...
					case 8: // Width
						return track.Widths; // 0～1f
					default:
						return null;
				}
			} else {
				return null;
			}
		}


		public int GetMotionCount(int itemIndex, int motionType) {
			var list = GetMotionList(itemIndex, motionType);
			return list != null ? list.Count : 0;
		}


		// Motion - Item
		public object GetMotion(int itemIndex, int motionType, int motionIndex) {
			var list = GetMotionList(itemIndex, motionType);
			return list != null && motionIndex >= 0 && motionIndex < list.Count ? list[motionIndex] : null;
		}


		public void SetMotion(int itemIndex, int motionType, int motionIndex, object item) {
			var list = GetMotionList(itemIndex, motionType);
			if (list != null && motionIndex >= 0 && motionIndex < list.Count) {
				list[motionIndex] = item;
			}
		}


		public void AddMotion(int itemIndex, int motionType, float time, float? valueA, float? valueB = null, int? tween = null) {
			if (motionType >= 0 && motionType <= 4) {
				// Stage
				if (itemIndex >= 0 && itemIndex < Stages.Count) {
					var stage = Stages[itemIndex];
					switch (motionType) {
						case 0:
							stage.Positions.Add(new TimeFloatFloatTween() {
								Time = time,
								Tween = tween ?? 0,
								A = valueA ?? 0f,
								B = valueB ?? 0f,
							});
							break;
						case 1:
							stage.Rotations.Add(new TimeFloatTween() {
								Time = time,
								Tween = tween ?? 0,
								Value = valueA ?? 0f,
							});
							break;
						case 2:
							stage.Colors.Add(new TimeIntTween() {
								Time = time,
								Tween = tween ?? 0,
								Value = Mathf.RoundToInt(valueA ?? 0),
							});
							break;
						case 3:
							stage.Widths.Add(new TimeFloatTween() {
								Time = time,
								Tween = tween ?? 0,
								Value = valueA ?? 1f,
							});
							break;
						case 4:
							stage.Heights.Add(new TimeFloatTween() {
								Time = time,
								Tween = tween ?? 0,
								Value = valueA ?? 1f,
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
								Tween = tween ?? 0,
								Value = valueA ?? 0f,
							});
							break;
						case 6:
							track.Angles.Add(new TimeFloatTween() {
								Time = time,
								Tween = tween ?? 0,
								Value = valueA ?? 0f,
							});
							break;
						case 7:
							track.Colors.Add(new TimeIntTween() {
								Time = time,
								Tween = tween ?? 0,
								Value = (int)(valueA ?? 0),
							});
							break;
						case 8:
							track.Widths.Add(new TimeFloatTween() {
								Time = time,
								Tween = tween ?? 0,
								Value = valueA ?? 1f,
							});
							break;

					}
					track.SortMotion(motionType);
				}
			}
		}


		public bool DeleteMotion(int itemIndex, int motionType, int motionIndex) {
			var list = GetMotionList(itemIndex, motionType);
			if (list != null && motionIndex >= 0 && motionIndex < list.Count) {
				list.RemoveAt(motionIndex);
				return true;
			}
			return false;
		}


		// Motion - Time
		public bool GetMotionTime(int itemIndex, int motionType, int motionIndex, out float time) {
			var motionObj = GetMotion(itemIndex, motionType, motionIndex);
			time = 0f;
			if (motionObj == null) { return false; }
			if (motionObj is TimeIntTween tMotionObj) {
				time = tMotionObj.Time;
				return true;
			} else if (motionObj is TimeFloatTween fMotionObj) {
				time = fMotionObj.Time;
				return true;
			} else if (motionObj is TimeFloatFloatTween ffMotionObj) {
				time = ffMotionObj.Time;
				return true;
			} else {
				return false;
			}
		}


		public void SetMotionTime(int itemIndex, int motionType, int motionIndex, float time) {
			var motionObj = GetMotion(itemIndex, motionType, motionIndex);
			if (motionObj == null) { return; }
			if (motionObj is TimeIntTween tiItem) {
				tiItem.Time = time;
				SetMotion(itemIndex, motionType, motionIndex, tiItem);
			} else if (motionObj is TimeFloatTween tfItem) {
				tfItem.Time = time;
				SetMotion(itemIndex, motionType, motionIndex, tfItem);
			} else if (motionObj is TimeFloatFloatTween tffItem) {
				tffItem.Time = time;
				SetMotion(itemIndex, motionType, motionIndex, tffItem);
			}
		}


		// Motion - Search
		public int MotionSearch(IList data, float time) {
			int start = 0;
			int end = data.Count - 1;
			int mid;
			if (end < 0) { return 0; }
			var zero = data[0];
			if (zero is TimeIntTween) {
				while (start <= end) {
					mid = (start + end) / 2;
					var m = (TimeIntTween)data[mid];
					if (m.Time < time) {
						start = mid + 1;
					} else if (m.Time > time) {
						end = mid - 1;
					} else {
						return mid;
					}
				}
			} else if (zero is TimeFloatTween) {
				while (start <= end) {
					mid = (start + end) / 2;
					var m = (TimeFloatTween)data[mid];
					if (m.Time < time) {
						start = mid + 1;
					} else if (m.Time > time) {
						end = mid - 1;
					} else {
						return mid;
					}
				}
			} else if (zero is TimeFloatFloatTween) {
				while (start <= end) {
					mid = (start + end) / 2;
					var m = (TimeFloatFloatTween)data[mid];
					if (m.Time < time) {
						start = mid + 1;
					} else if (m.Time > time) {
						end = mid - 1;
					} else {
						return mid;
					}
				}
			}
			return (start + end) / 2;
		}


		// Motion - Value Tween
		public (bool hasA, bool hasB) GetMotionValueTween(object motionObj, out float valueA, out float valueB, out int tween) {
			valueA = valueB = 0f;
			tween = 0;
			if (motionObj == null) { return (false, false); }
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


		public (bool hasA, bool hasB) GetMotionValueTween(int itemIndex, int motionType, int motionIndex, out float valueA, out float valueB, out int tween) {
			var motionObj = GetMotion(itemIndex, motionType, motionIndex);
			return GetMotionValueTween(motionObj, out valueA, out valueB, out tween);
		}


		public (bool hasA, bool hasB) SearchMotionValueTween(int itemIndex, int motionType, float motionTime, out float valueA, out float valueB, out int tween, out int motionIndex) {
			valueA = valueB = tween = 0;
			motionIndex = -1;
			var list = GetMotionList(itemIndex, motionType);
			if (list != null) {
				motionIndex = MotionSearch(list, motionTime);
				return motionIndex >= 0 && motionIndex < list.Count - 1 ? GetMotionValueTween(list[motionIndex], out valueA, out valueB, out tween) : (false, false);
			}
			return (false, false);
		}


		public void SetMotionValueTween(int itemIndex, int motionType, int motionIndex, float? valueA = null, float? valueB = null, int? tween = null) {
			var list = GetMotionList(itemIndex, motionType);
			if (list != null && motionIndex >= 0 && motionIndex < list.Count) {
				if (valueA.HasValue) {
					var item = list[motionIndex];
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
					var item = list[motionIndex];
					if (item is TimeFloatFloatTween ffItem) {
						ffItem.B = valueB.Value;
						list[motionIndex] = ffItem;
					}
				}
				if (tween.HasValue) {
					var item = list[motionIndex];
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


	}
}