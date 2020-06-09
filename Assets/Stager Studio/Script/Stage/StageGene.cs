namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class StageGene : MonoBehaviour {




		#region --- SUB ---


		// Handler
		public delegate GeneData GeneHandler ();
		public delegate Beatmap BeatmapHandler ();


		#endregion




		#region --- VAR ---


		// Handler
		public static GeneHandler GetGene { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;


		#endregion




		#region --- MSG ---





		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---




		#endregion




	}
}