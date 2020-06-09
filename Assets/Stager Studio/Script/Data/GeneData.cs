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
			public ActiveInt Time_Real;
			public ActiveInt Duration_Real;
			public ActiveInt X;
			public ActiveInt Y_Real;
			public ActiveInt Rotation;
			public ActiveInt Width;
			public ActiveInt Height_Real;
			public ActiveInt PivotY;
			public ActiveInt Speed;

			public static StageConfig AllInactive => new StageConfig() {
				X = ActiveInt.Inactive,
				Y_Real = ActiveInt.Inactive,
				Rotation = ActiveInt.Inactive,
				Height_Real = ActiveInt.Inactive,
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
			public ActiveInt Time_Real;
			public ActiveInt Duration_Real;
			public ActiveInt X;
			public ActiveInt Angle;
			public ActiveInt Width;
			public ActiveInt Speed;

			public static TrackConfig AllInactive => new TrackConfig() {
				X = ActiveInt.Inactive,
				Angle = ActiveInt.Inactive,
				Duration_Real = ActiveInt.Inactive,
				Speed = ActiveInt.Inactive,
				Width = ActiveInt.Inactive,
				Time_Real = ActiveInt.Inactive,
			};

		}


		// SER-API
		public string Key = "";
		public bool Allow_EditStage = true;
		public bool Allow_EditTrack = true;
		public bool Allow_EditTiming = true;
		public StageConfig Config_Stage = StageConfig.AllInactive;
		public TrackConfig Config_Track = TrackConfig.AllInactive;
		public StageConfig[] StaticConfigs_Stage = { };
		public TrackConfig[] StaticConfigs_Track = { };



	}
}