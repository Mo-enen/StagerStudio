namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Saving;
	using UnityEngine.Audio;

	public class StageSoundFX : MonoBehaviour {




		#region --- SUB ---


		private const byte FX_COUNT = 14;
		public enum FxType {
			None = 0,
			Glitch = 1,
			Gate = 2,
			SideChain = 3,
			Flange = 4,
			PitchShift = 5,
			LowPass = 6,
			HighPass = 7,
			Knob = 8,
			KnobShoot = 9,
			Wobble = 10,
			BitCrush = 11,
			TapeScratch = 12,
			ShortDelay = 13,

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
		private const string PITCH_VOLUME_KEYWORD = "Pitch_Volume";
		private const string LOW_VOLUME_KEYWORD = "Low_Volume";
		private const string HIGH_VOLUME_KEYWORD = "High_Volume";
		private const string KNOB_VOLUME_KEYWORD = "Knob_Volume";
		private const string PITCH_PARAM_KEYWORD = "Pitch_ParamA";
		private const string LOW_PARAM_KEYWORD = "Low_ParamA";
		private const string HIGH_PARAM_KEYWORD = "High_ParamA";
		private const string KNOB_PARAM_KEYWORD = "Knob_ParamA";
		private const string WOBBLE_VOLUME_KEYWORD = "Wobble_Volume";
		private const string WOBBLE_DEPTH_KEYWORD = "Wobble_Depth";
		private const string WOBBLE_PARAM_KEYWORD = "Wobble_ParamA";



		// Handler
		public static VoidBoolHandler SetMusicMute { get; set; } = null;
		public static VoidFloatHandler SetMusicVolume { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetMusicVolume { get; set; } = null;
		public static FloatHandler GetMusicPitch { get; set; } = null;
		public static FloatHandler GetSecondPerBeat { get; set; } = null;
		public static BoolHandler GetMusicPlaying { get; set; } = null;
		public static BoolHandler GetMusicMute { get; set; } = null;
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
		private AudioSource SourceShoot {
			get {
				if (!_SourceShoot) {
					_SourceShoot = gameObject.AddComponent<AudioSource>();
					_SourceShoot.playOnAwake = false;
					_SourceShoot.loop = false;
				}
				return _SourceShoot;
			}
		}

		private AudioMixer Mixer => _Mixer != null ? _Mixer : (_Mixer = m_MixerGroup.audioMixer);

		// Ser
		[SerializeField] private AudioMixerGroup m_MixerGroup = null;
		[SerializeField] private AudioClip m_KnobShoot = null;

		// Data 
		private readonly static Coroutine[] FxCors = new Coroutine[FX_COUNT] {
			null, null, null, null, null, null, null, null, null, null, null,null, null, null,
		};
		private readonly static WaitForSeconds WAIT_FOR_001_SECOND = new WaitForSeconds(0.01f);
		private AudioSource _Source = null;
		private AudioSource _SourceShoot = null;
		private AudioMixer _Mixer = null;
		private float GlitchMusicTime = 0f;
		private float GlitchEnd = 0f;
		private float GlitchNext = -1f;
		private float GlitchGap = -1f;
		private float GateNext = -1f;
		private float GateGap = -1f;
		private float GateEnd = -1f;
		private float SideChainNext = -1f;
		private float SideChainEnd = -1f;
		private float SideChainGap = -1f;

		// Saving
		public SavingBool UseFX { get; private set; } = new SavingBool("StageSoundFX.UseFX", true);


		#endregion




		#region --- MSG ---


		private void Awake () {
			SourceShoot.clip = m_KnobShoot;
			while (SourceShoot.clip.loadState == AudioDataLoadState.Loading) { }
			SourceShoot.Play();
			SourceShoot.Pause();
			SourceShoot.time = 0f;
		}


		private void FixedUpdate () {
			// Glitch
			if (GlitchGap > 0f && Time.unscaledTime > GlitchNext) {
				Source.time = GlitchMusicTime;
				GlitchNext += GlitchGap;
				if (Time.unscaledTime > GlitchEnd) {
					TryStopFx(FxType.Glitch);
				}
			}
			// Gate
			if (GateGap > 0f && Time.unscaledTime > GateNext) {
				SetMusicMute(!GetMusicMute());
				GateNext += GateGap;
				if (Time.unscaledTime > GateEnd) {
					TryStopFx(FxType.Gate);
				}
			}
			// Side Chain
			if (SideChainGap > 0f) {
				SetMusicVolume(
					Mathf.Lerp(0f, 1f, (Time.unscaledTime - SideChainNext + SideChainGap) / SideChainGap)
				);
				if (Time.unscaledTime > SideChainNext) {
					SideChainNext += SideChainGap;
				}
				if (Time.unscaledTime > SideChainEnd) {
					TryStopFx(FxType.SideChain);
				}
			}
		}


		#endregion




		#region --- API ---


		public void PlayFX (byte typeByte, int fxDuration, int paramA, int paramB) {

			if (!UseFX || !GetMusicPlaying()) { return; }

			typeByte = (byte)Mathf.Clamp(typeByte, 0, FX_COUNT - 1);
			fxDuration = Mathf.Min(fxDuration, 128000);
			var type = (FxType)typeByte;
			TryStopFx(type);
			switch (type) {
				case FxType.Glitch: {
						float duration = Mathf.Min(fxDuration / 1000f, 128f);
						float gap = Mathf.Max(paramA * GetSecondPerBeat() / 100f, 0.001f);
						Volume = GetMusicVolume();
						Source.pitch = GetMusicPitch();
						Source.mute = false;
						Source.time = GlitchMusicTime = GetMusicTime();
						SetMusicMute(true);
						Source.Play();
						GlitchGap = gap;
						GlitchNext = Time.unscaledTime + gap;
						GlitchEnd = Time.unscaledTime + duration;
					}
					break;
				case FxType.Gate: {
						float duration = Mathf.Min(fxDuration / 1000f, 128f);
						float gap = Mathf.Max(paramA * GetSecondPerBeat() / 200f, 0.001f);
						GateGap = gap;
						GateNext = Time.unscaledTime + gap;
						GateEnd = Time.unscaledTime + duration;
					}
					break;
				case FxType.SideChain: {
						float duration = Mathf.Min(fxDuration / 1000f, 128f);
						float gap = Mathf.Max(paramA * GetSecondPerBeat() / 100f, 0.001f);
						SideChainGap = gap;
						SideChainNext = Time.unscaledTime + gap;
						SideChainEnd = Time.unscaledTime + duration;
					}
					break;
				case FxType.Flange:
					FxCors[(int)type] = StartCoroutine(
						Flange(fxDuration / 1000f)
					);
					break;
				case FxType.PitchShift:
					FxCors[(int)type] = StartCoroutine(
						PitchShift(fxDuration / 1000f, paramA / 100f)
					);
					break;
				case FxType.LowPass:
				case FxType.HighPass:
					FxCors[(int)type] = StartCoroutine(
						LowHighPass(type == FxType.LowPass, fxDuration / 1000f, paramA / 100f, paramB / GetSecondPerBeat() / 100f)
					);
					break;
				case FxType.Knob:
					FxCors[(int)type] = StartCoroutine(
						Knob(fxDuration / 1000f, paramA / GetSecondPerBeat() / 100f)
					);
					break;
				case FxType.KnobShoot: {
						float volume = GetMusicVolume() * paramA / 100f;
						SourceShoot.volume = Mathf.Max(volume * volume, 0f);
						SourceShoot.time = 0f;
						SourceShoot.Play();
					}
					break;
				case FxType.Wobble:
					FxCors[(int)type] = StartCoroutine(
						Wobble(fxDuration / 1000f, paramA / 100f)
					);
					break;
				case FxType.BitCrush:



					break;
				case FxType.TapeScratch:



					break;
				case FxType.ShortDelay:



					break;

			}

			// === Func ===
			IEnumerator Flange (float duration) {
				float startTime = Time.unscaledTime;
				float endTime = startTime + duration;
				Mixer.SetFloat(FLANGE_VOLUME_KEYWORD, 0f);
				Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, -80f);
				float speed = 1f / GetSecondPerBeat();
				while (Time.unscaledTime < endTime) {
					Mixer.SetFloat(FLANGE_DEPTH_KEYWORD, Mathf.PingPong((Time.unscaledTime - startTime) * speed, 1f));
					yield return WAIT_FOR_001_SECOND;
				}
				TryStopFx(FxType.Flange);
			}
			IEnumerator PitchShift (float duration, float pitch) {
				float endTime = Time.unscaledTime + duration;
				Mixer.SetFloat(PITCH_VOLUME_KEYWORD, 0f);
				Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, -80f);
				Mixer.SetFloat(PITCH_PARAM_KEYWORD, pitch);
				yield return new WaitUntil(() => Time.unscaledTime > endTime);
				TryStopFx(FxType.PitchShift);
			}
			IEnumerator LowHighPass (bool low, float duration, float cutoff, float delta) {
				float startTime = Time.unscaledTime;
				float endTime = startTime + duration;
				cutoff = Mathf.Clamp01(cutoff);
				Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, -80f);
				Mixer.SetFloat(low ? LOW_VOLUME_KEYWORD : HIGH_VOLUME_KEYWORD, 0f);
				while (Time.unscaledTime < endTime) {
					float t01 = cutoff - Mathf.PingPong((Time.unscaledTime - startTime) * delta, cutoff);
					Mixer.SetFloat(low ? LOW_PARAM_KEYWORD : HIGH_PARAM_KEYWORD, Util.Remap(
						0f, 1f,
						low ? 18000f : 10f,
						low ? 480f : 6800f,
						t01 * t01 * t01
					));
					yield return WAIT_FOR_001_SECOND;
				}
				TryStopFx(low ? FxType.LowPass : FxType.HighPass);
			}
			IEnumerator Knob (float duration, float delta) {
				float startTime = Time.unscaledTime;
				float endTime = startTime + duration;
				Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, -80f);
				Mixer.SetFloat(KNOB_VOLUME_KEYWORD, 0f);
				while (Time.unscaledTime < endTime) {
					float t01 = 1f - Mathf.PingPong((Time.unscaledTime - startTime) * delta, 1f);
					Mixer.SetFloat(KNOB_PARAM_KEYWORD, Mathf.Lerp(
						20f, 22000f, t01 * t01 * t01
					));
					yield return WAIT_FOR_001_SECOND;
				}
				TryStopFx(FxType.Knob);
			}
			IEnumerator Wobble (float duration, float rate) {
				float startTime = Time.unscaledTime;
				float endTime = startTime + duration;
				float spb = GetSecondPerBeat();
				Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, -80f);
				Mixer.SetFloat(WOBBLE_VOLUME_KEYWORD, 0f);
				Mixer.SetFloat(WOBBLE_PARAM_KEYWORD, Mathf.Lerp(6f, 20f, rate * rate));
				while (Time.unscaledTime < endTime) {
					Mixer.SetFloat(
						WOBBLE_DEPTH_KEYWORD,
						1f - Mathf.PingPong((Time.unscaledTime - startTime) / spb, 1f)
					);
					yield return WAIT_FOR_001_SECOND;
				}
				TryStopFx(FxType.Wobble);
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
				case FxType.Glitch:
					GlitchGap = -1f;
					GlitchNext = -1f;
					SetMusicMute(false);
					SetMusicVolume(1f);
					Source.mute = true;
					Source.Pause();
					break;
				case FxType.Gate:
					GateGap = -1f;
					SetMusicMute(false);
					SetMusicVolume(1f);
					break;
				case FxType.SideChain:
					SideChainGap = -1f;
					SetMusicMute(false);
					SetMusicVolume(1f);
					break;
				case FxType.Flange:
					Mixer.SetFloat(FLANGE_VOLUME_KEYWORD, -80f);
					break;
				case FxType.PitchShift:
					Mixer.SetFloat(PITCH_VOLUME_KEYWORD, -80f);
					break;
				case FxType.LowPass:
					Mixer.SetFloat(LOW_VOLUME_KEYWORD, -80f);
					break;
				case FxType.HighPass:
					Mixer.SetFloat(HIGH_VOLUME_KEYWORD, -80f);
					break;
				case FxType.Knob:
					Mixer.SetFloat(KNOB_VOLUME_KEYWORD, -80f);
					break;
				case FxType.KnobShoot:
					SourceShoot.Pause();
					break;
				case FxType.Wobble:
					Mixer.SetFloat(WOBBLE_VOLUME_KEYWORD, -80f);
					break;
				case FxType.BitCrush:

					break;
				case FxType.TapeScratch:

					break;
				case FxType.ShortDelay:

					break;
			}
			// Stop Cor
			var cor = FxCors[(int)type];
			if (cor != null) {
				StopCoroutine(cor);
				FxCors[(int)type] = null;
			}
			// Music Volume Back
			bool hasCor = false;
			for (int i = 4; i < FxCors.Length; i++) {
				if (FxCors[i] != null) {
					hasCor = true;
					break;
				}
			}
			if (!hasCor) {
				Mixer.SetFloat(MUSIC_VOLUME_KEYWORD, 0f);
			}
		}


		#endregion




	}
}