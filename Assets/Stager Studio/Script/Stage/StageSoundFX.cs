namespace StagerStudio.Stage {
	using global::StagerStudio.Saving;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class StageSoundFX : MonoBehaviour {



		// SUB
		public enum FxType {
			None = 0,
			Retrigger = 1,



		}



		// VAR
		public SavingBool UseFX { get; private set; } = new SavingBool("StageSoundFX.UseFX", true);



		// API
		public void PlayFX (FxType type, int param) {
			switch (type) {
				case FxType.Retrigger:




					break;
			}
		}



		public void SetUseFX (bool use) {
			UseFX.Value = use;
		}



	}
}