using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StagerStudio.Data {



	#region --- Note ---


	[System.Serializable]
	public class Note {



	}


	#endregion




	#region --- Track ---


	[System.Serializable]
	public class Track {



	}


	#endregion




	#region --- Stage ---


	[System.Serializable]
	public class Stage {



	}


	#endregion




	#region --- Timing ---


	[System.Serializable]
	public class Timing {



	}


	#endregion



	#region --- Beatmap ---


	[System.Serializable]
	public class Beatmap {



		// SUB
		[System.Serializable]
		public class BpmSection {
			public int Time = 0;
			public int BPM = 240;
			public BpmSection (int time, int bpm) {
				Time = time;
				BPM = bpm;
			}
		}


		// Ser-Api
		public string Tag = "Normal";
		public int Difficulty = 1;
		public int BeatPerMinute = 240;
		public int BeatShift = 0;
		public List<BpmSection> BpmSections = new List<BpmSection>();
		public List<Stage> Stages = new List<Stage>();
		public List<Track> Tracks = new List<Track>();
		public List<Note> Notes = new List<Note>();
		public List<Timing> Timings = new List<Timing>();




	}


	#endregion




}
