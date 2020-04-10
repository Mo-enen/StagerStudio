namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Saving;



	public class StageSoundFX : MonoBehaviour {




		#region --- SUB ---


		public enum FxType {
			None = 0,
			Retrigger = 1,
			Flange = 2,


		}


		public delegate void VoidBoolHandler (bool b);
		public delegate void VoidFloatHandler (float f);
		public delegate float FloatHandler ();
		public delegate bool BoolHandler ();
		public delegate void VoidHandler ();


		#endregion




		#region --- VAR ---


		// Handler
		public static VoidBoolHandler SetMusicMute { get; set; } = null;
		public static VoidFloatHandler SetMusicVolume { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetMusicVolume { get; set; } = null;
		public static FloatHandler GetMusicPitch { get; set; } = null;
		public static FloatHandler GetSecondPerBeat { get; set; } = null;
		public static BoolHandler GetMusicPlaying { get; set; } = null;
		public static VoidHandler OnUseFxChanged { get; set; } = null;

		// Api
		public float Volume {
			get => Mathf.Sqrt(Source.volume);
			set => Source.volume = Mathf.Max(value * value, 0f);
		}


		// Short
		private AudioSource Source {
			get {
				if (!_Source) {
					_Source = gameObject.AddComponent<AudioSource>();
					_Source.playOnAwake = false;
					_Source.loop = false;
				}
				return _Source;
			}
		}

		// Data 
		private readonly static Coroutine[] FxCors = { null, null, null, };
		private readonly static WaitForEndOfFrame WAIT_FOR_END_OF_FRAME = new WaitForEndOfFrame();
		private AudioSource _Source = null;

		// Saving
		public SavingBool UseFX { get; private set; } = new SavingBool("StageSoundFX.UseFX", true);


		#endregion




		#region --- MSG ---



		#endregion




		#region --- API ---


		public void PlayFX (byte typeByte, int noteDuration, int paramA, int paramB) {
			if (!UseFX || !GetMusicPlaying()) { return; }
			var type = (FxType)typeByte;
			switch (type) {
				case FxType.Retrigger:
					TryStopFx(type);
					FxCors[(int)FxType.Retrigger] = StartCoroutine(
						Retrigger(noteDuration / 1000f, paramA * GetSecondPerBeat() / 100f, paramB / 100f)
					);
					break;
				case FxType.Flange:



					break;
			}
			// === Func ===
			IEnumerator Retrigger (float duration, float gap, float volume) {
				float startTime = Source.time = GetMusicTime();
				volume = Mathf.Clamp01(volume);
				Volume = GetMusicVolume();
				Source.pitch = GetMusicPitch();
				Source.mute = false;
				Volume = GetMusicVolume();
				SetMusicVolume(volume);
				Source.Play();
				for (float t = 0f, prevT = 0f; t < duration; t += Time.deltaTime) {
					if (t > prevT + gap) {
						Source.time = startTime;
						prevT += gap;
					}
					yield return WAIT_FOR_END_OF_FRAME;
				}
				TryStopFx(FxType.Retrigger);
			}



		}



		public void SetUseFX (bool use) {
			UseFX.Value = use;
			StopAllFx();
			OnUseFxChanged();
		}


		public void StopAllFx () {
			for (int i = 1; i < FxCors.Length; i++) {
				TryStopFx((FxType)i);
			}
		}


		public void SetClip (AudioClip clip) {
			Source.clip = clip;
			if (clip) {
				while (Source.clip.loadState == AudioDataLoadState.Loading) { }
			}
			Source.Play();
			Source.Pause();
			Source.time = 0f;
			StopAllFx();
		}


		#endregion




		#region --- LGC ---


		private void TryStopFx (FxType type) {
			// Stop Fx
			switch (type) {
				case FxType.Retrigger:
					SetMusicMute(false);
					SetMusicVolume(1f);
					Source.mute = true;
					Source.Pause();
					break;
				case FxType.Flange:


					break;
			}
			// Stop Cor
			var cor = FxCors[(int)type];
			if (cor != null) {
				StopCoroutine(cor);
				FxCors[(int)type] = null;
			}
		}



		#endregion




	}
}