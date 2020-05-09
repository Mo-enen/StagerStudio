namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class StageCommand : MonoBehaviour {




		#region --- SUB ---


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
			SetX = 2,
			SetWidth = 3,
			Delete = 4,

		}


		#endregion




		#region --- VAR ---




		#endregion



		#region --- API ---


		public void DoCommand (int target, int command, int index, float value) {


			Debug.Log(target + " " + command + " " + index + " " + value);


		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}