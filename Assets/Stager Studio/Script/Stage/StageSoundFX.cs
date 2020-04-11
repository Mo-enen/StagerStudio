namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Saving;
	using UnityEngine.Audio;

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


		// Const
		private const string MUSIC_VOLUME_KEYWORD = "Music_Volume";
		private const string FLANGE_VOLUME_KEYWORD = "Flange_Volume";
		private const string FLANGE_DEPTH_KEYWORD = "Flange_Depth";

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
					_Source.outputAudioMixerGroup = m_MixerGroup;
				}
				return _Source;
			}
		}
		private AudioMixer Mixer => _Mixer != null ? _Mixer : (_Mixer = m_MixerGroup.audioMixer);

		// Ser
		[SerializeField] private AudioMixerGroup m_MixerGroup = null;

		// Data 
		private readonly static Coroutine[] FxCors = { null, null, null, };
		private readonly static WaitForSeconds WAIT_FOR_001_SECOND = new WaitForSeconds(0.01f);
		private AudioSource _Source = null;
		private AudioMixer _Mixer = null;

		// Saving
		public SavingBool UseFX { get; private set; } = new SavingBool("StageSoundFX.UseFX", true);


		#endregion




		#region --- MSG ---



		#endregion




		#region --- API ---


		public void PlayFX (int time, byte typeByte, int fxDuration, int paramA, int paramB) {

			if (!UseFX || !GetMusicPlaying()) { return; }

			var type = (FxType)typeByte;
			switch (type) {
				case FxType.Retrigger:
					TryStopFx(type);
					FxCors[(int)FxType.Retrigger] = StartCoroutine(
						Retrigger(time / 1000f, fxDuration / 1000f, paramA * GetSecondPerBeat() / 100f, paramB / 100f)
					);
					break;
				case FxType.Flange:
					TryStopFx(type);
					FxCors[(int)FxType.Flange] = StartCoroutine(
						Flange(fxDuration / 1000f, paramA / GetSecondPerBeat() / 100f)
					);
					break;



			}

			// === Func ===
			IEnumerator Retrigger (float startTime, float duration, float gap, float volume) {
				duration = Mathf.Min(duration, 128f);
				gap = Mathf.Max(gap, 0.001f);
				Source.time = startTime;
				volume = Mathf.Clamp01(volume);
				Volume = GetMusicVolume();
				Source.pitch = GetMusicPitch();
				Source.mute = false;
				Volume = GetMusicVolume();
				SetMusicVolume(volume);
				Source.Play();
				float endTime = Time.unscaledTime + duration + gap / 2f;
				for (float prevT = Time.unscaledTime; Time.unscaledTime < endTime;) {
					if (Time.unscaledTime > prevT + gap) {
						Source.time = startTime;
						prevT = Time.unscaledTime;
					}
					yield return WAIT_FOR_001_SECOND;
				}
				TryStopFx(FxType.Retrigger);
			}

			IEnumerator Flange (float duration, float speed) {
				duration = Mathf.Min(duration, 128f);
				float endTime = Time.unscaledTime + duration;
				Mixer.SetFloat(FLANGE_VOLUME_KEYWORD, 0f);
				Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, -80f);
				while (Time.unscaledTime < endTime) {
					Mixer.SetFloat(FLANGE_DEPTH_KEYWORD, Mathf.PingPong(Time.unscaledTime * speed, 0.5f) + 0.5f);
					yield return WAIT_FOR_001_SECOND;
				}
				TryStopFx(FxType.Flange);
			}

		}


		public void SetUseFX (bool use) {
			UseFX.Value = use;
			Source.outputAudioMixerGroup = use ? m_MixerGroup : null;
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
					Mixer.SetFloat(FLANGE_VOLUME_KEYWORD, -80f);
					Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, 0f);
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