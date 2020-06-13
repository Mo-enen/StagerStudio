namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;




	[System.Serializable]
	public class GeneData {


		// SUB
		[System.Serializable]
		public struct ActiveInt {
			public bool Active;
			public int Value;
			public static ActiveInt Inactive => new ActiveInt(0) { Active = false };
			public ActiveInt (int value) {
				Active = true;
				Value = value;
			}
		}



		[System.Serializable]
		public struct StageConfig {

			// API-SER
			public bool UseConfig;
			public bool Motion;
			public bool TileTrack;
			public ActiveInt ItemType;
			public ActiveInt Time_Real;
			public ActiveInt Duration_Real;
			public ActiveInt X;
			public ActiveInt Y;
			public ActiveInt Rotation;
			public ActiveInt Width;
			public ActiveInt Height;
			public ActiveInt PivotY;
			public ActiveInt Speed;

			public static StageConfig IgnoreAll => new StageConfig() {
				UseConfig = false,
				Motion = true,
				TileTrack = false,
				ItemType = ActiveInt.Inactive,
				X = ActiveInt.Inactive,
				Y = ActiveInt.Inactive,
				Rotation = ActiveInt.Inactive,
				Height = ActiveInt.Inactive,
				PivotY = ActiveInt.Inactive,
				Width = ActiveInt.Inactive,
				Speed = ActiveInt.Inactive,
				Duration_Real = ActiveInt.Inactive,
				Time_Real = ActiveInt.Inactive,
			};

		}



		[System.Serializable]
		public struct TrackConfig {

			// API-SER
			public bool UseConfig;
			public bool Motion;
			public ActiveInt ItemType;
			public ActiveInt StageIndex;
			public ActiveInt Time_Real;
			public ActiveInt Duration_Real;
			public ActiveInt X;
			public ActiveInt Angle;
			public ActiveInt Width;
			public ActiveInt Speed;
			public ActiveInt HasTray;

			public static TrackConfig IgnoreAll => new TrackConfig() {
				UseConfig = false,
				Motion = true,
				StageIndex = ActiveInt.Inactive,
				ItemType = ActiveInt.Inactive,
				X = ActiveInt.Inactive,
				Angle = ActiveInt.Inactive,
				Duration_Real = ActiveInt.Inactive,
				Speed = ActiveInt.Inactive,
				Width = ActiveInt.Inactive,
				Time_Real = ActiveInt.Inactive,
				HasTray = ActiveInt.Inactive,
			};

		}



		[System.Serializable]
		public struct NoteConfig {


			// API-SER
			public bool UseConfig;
			public ActiveInt ItemType;
			public ActiveInt X;
			public ActiveInt Z;
			public ActiveInt Duration;
			public ActiveInt Width;
			public ActiveInt LinkedNoteIndex;
			public ActiveInt Speed;
			public ActiveInt ClickSound;


			public static NoteConfig IgnoreAll => new NoteConfig() {
				UseConfig = false,
				ItemType = ActiveInt.Inactive,
				X = ActiveInt.Inactive,
				Z = ActiveInt.Inactive,
				Duration = ActiveInt.Inactive,
				Width = ActiveInt.Inactive,
				LinkedNoteIndex = ActiveInt.Inactive,
				Speed = ActiveInt.Inactive,
				ClickSound = ActiveInt.Inactive,
			};



		}



		// SER-API
		public string Key = "";
		public bool StageAccessable = true;
		public bool TrackAccessable = true;
		public bool NoteAccessable = true;
		public bool TimingAccessable = true;
		public bool SfxAccessable = true;
		public ActiveInt Ratio = ActiveInt.Inactive;
		public StageConfig Config_Stage = StageConfig.IgnoreAll;
		public TrackConfig Config_Track = TrackConfig.IgnoreAll;
		public NoteConfig Config_Note = NoteConfig.IgnoreAll;
		public StageConfig[] StaticConfigs_Stage = { };
		public TrackConfig[] StaticConfigs_Track = { };
		public NoteConfig[] StaticConfigs_Note = { };


	}
}