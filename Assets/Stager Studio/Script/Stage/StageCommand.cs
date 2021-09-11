namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public class StageCommand : MonoBehaviour {



		// SUB
		public enum TargetType {
			None = 0,
			Stage = 1,
			Track = 2,
			TrackInside = 3,
			Note = 4,
			NoteInside = 5,
			Timing = 6,

		}


		public enum CommandType {
			None = 0,
			SetTime = 1,
			AddTime = 2,
			SetX = 3,
			AddX = 4,
			SetWidth = 5,
			AddWidth = 6,

		}


		public delegate void VoidHandler ();
		public static VoidHandler OnCommandDone { get; set; } = null;


		// API
		public static bool DoCommand (Beatmap map, TargetType target, CommandType command, int index, float value) {
			if (map == null) { return false; }
			switch (target) {
				case TargetType.Stage:
					return DoStageCommand(map, command, value);
				case TargetType.Track:
					return DoTrackCommand(map, command, -1, value);
				case TargetType.TrackInside:
					return DoTrackCommand(map, command, index, value);
				case TargetType.Note:
					return DoNoteCommand(map, command, -1, value);
				case TargetType.NoteInside:
					return DoNoteCommand(map, command, index, value);
				case TargetType.Timing:
					return DoTimingCommand(map, command, value);
			}
			return false;
		}


		// LGC
		private static bool DoStageCommand (Beatmap map, CommandType command, float value) {
			if (map.Stages == null || map.Stages.Count == 0) { return false; }
			var targets = new List<Beatmap.Stage>(map.Stages.ToArray());
			for (int i = 0; i < targets.Count; i++) {
				var target = targets[i];
				switch (command) {
					default:
						return false;
					case CommandType.SetTime:
						target.Time = Mathf.Max(value, 0f);
						break;
					case CommandType.AddTime:
						target.Time = Mathf.Max(target.Time + value, 0f);
						break;
					case CommandType.SetX:
						target.X = value;
						break;
					case CommandType.AddX:
						target.X += value;
						break;
					case CommandType.SetWidth:
						target.Width = Mathf.Max(value, 0f);
						break;
					case CommandType.AddWidth:
						target.Width = Mathf.Max(target.Width + value, 0f);
						break;
				}
			}
			OnCommandDone();
			return true;
		}


		private static bool DoTrackCommand (Beatmap map, CommandType command, int index, float value) {
			if (map.Tracks == null || map.Tracks.Count == 0) { return false; }
			var targets = new List<Beatmap.Track>();
			if (index >= 0) {
				foreach (var track in map.Tracks) {
					if (track.StageIndex == index) {
						targets.Add(track);
					}
				}
			} else {
				targets.AddRange(map.Tracks.ToArray());
			}
			for (int i = 0; i < targets.Count; i++) {
				var target = targets[i];
				switch (command) {
					default:
						return false;
					case CommandType.SetTime:
						target.Time = Mathf.Max(value, 0f);
						break;
					case CommandType.AddTime:
						target.Time = Mathf.Max(target.Time + value, 0f);
						break;
					case CommandType.SetX:
						target.X = value;
						break;
					case CommandType.AddX:
						target.X += value;
						break;
					case CommandType.SetWidth:
						target.Width = Mathf.Max(value, 0f);
						break;
					case CommandType.AddWidth:
						target.Width = Mathf.Max(target.Width + value, 0f);
						break;
				}
			}
			return true;
		}


		private static bool DoNoteCommand (Beatmap map, CommandType command, int index, float value) {
			if (map.Notes == null || map.Notes.Count == 0) { return false; }
			var targets = new List<Beatmap.Note>();
			if (index >= 0) {
				foreach (var note in map.Notes) {
					if (note.TrackIndex == index) {
						targets.Add(note);
					}
				}
			} else {
				targets.AddRange(map.Notes.ToArray());
			}
			for (int i = 0; i < targets.Count; i++) {
				var target = targets[i];
				switch (command) {
					default:
						return false;
					case CommandType.SetTime:
						target.Time = Mathf.Max(value, 0f);
						break;
					case CommandType.AddTime:
						target.Time = Mathf.Max(target.Time + value, 0f);
						break;
					case CommandType.SetX:
						target.X = value;
						break;
					case CommandType.AddX:
						target.X += value;
						break;
					case CommandType.SetWidth:
						target.Width = Mathf.Max(value, 0f);
						break;
					case CommandType.AddWidth:
						target.Width = Mathf.Max(target.Width + value, 0f);
						break;
				}
			}
			return true;
		}


		private static bool DoTimingCommand (Beatmap map, CommandType command, float value) {
			if (map.Timings == null || map.Timings.Count == 0) { return false; }
			var targets = new List<Beatmap.Timing>(map.Timings.ToArray());
			for (int i = 0; i < targets.Count; i++) {
				var target = targets[i];
				switch (command) {
					default:
						return false;
					case CommandType.SetTime:
						target.Time = Mathf.Max(value, 0f);
						break;
					case CommandType.AddTime:
						target.Time = Mathf.Max(target.Time + value, 0f);
						break;
				}
			}
			return true;
		}


	}
}