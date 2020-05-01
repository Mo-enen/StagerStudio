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
			ShortDelay = 3,
			SideChain = 4,
			BitCrush = 5,

			KnobShoot = 6,
			TapeScratch = 7,

			Flange = 8,
			PitchShift = 9,
			LowPass = 10,
			HighPass = 11,
			Knob = 12,
			Wobble = 13,

		}


		public delegate void VoidBoolHandler (bool b);
		public delegate void VoidFloatHandler (float f);
		public delegate float FloatHandler ();
		public delegate bool BoolHandler ();
		public delegate void VoidHandler ();


		#endregion




		#region --- VAR ---


		// Const
		private const string VOLUME_MUSIC = "Music_Volume";
		private const string VOLUME_FLANGE = "Flange_Volume";
		private const string VOLUME_PITCH = "Pitch_Volume";
		private const string VOLUME_LOW = "Low_Volume";
		private const string VOLUME_HIGH = "High_Volume";
		private const string VOLUME_KNOB = "Knob_Volume";
		private const string VOLUME_WOBBLE = "Wobble_Volume";

		private const string PARAM_PITCH = "Pitch_ParamA";
		private const string PARAM_LOW = "Low_ParamA";
		private const string PARAM_HIGH = "High_ParamA";
		private const string PARAM_KNOB = "Knob_ParamA";
		private const string PARAM_WOBBLE = "Wobble_ParamA";

		private const string DEPTH_FLANGE = "Flange_Depth";
		private const string DEPTH_WOBBLE = "Wobble_Depth";


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
		public float SourceVolume {
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
					var tf = new GameObject("Shoot").transform;
					tf.SetParent(transform);
					_SourceShoot = tf.gameObject.AddComponent<AudioSource>();
					_SourceShoot.playOnAwake = false;
					_SourceShoot.loop = false;
				}
				return _SourceShoot;
			}
		}
		private AudioSource SourceTapeScratch {
			get {
				if (!_SourceTapeScratch) {
					var tf = new GameObject("Tape Scratch").transform;
					tf.SetParent(transform);
					_SourceTapeScratch = tf.gameObject.AddComponent<AudioSource>();
					_SourceTapeScratch.playOnAwake = false;
					_SourceTapeScratch.loop = false;
				}
				return _SourceTapeScratch;
			}
		}
		private AudioMixer Mixer => _Mixer != null ? _Mixer : (_Mixer = m_MixerGroup.audioMixer);

		// Ser
		[SerializeField] private AudioMixerGroup m_MixerGroup = null;
		[SerializeField] private AudioClip m_KnobShoot = null;
		[SerializeField] private AudioClip m_TapeScratch = null;

		// Data 
		private readonly static Coroutine[] FxCors = new Coroutine[FX_COUNT] {
			null, null, null, null, null, null, null, null, null, null, null,null, null, null,
		};
		private readonly static WaitForSeconds WAIT_FOR_001_SECOND = new WaitForSeconds(0.01f);
		private AudioSource _Source = null;
		private AudioSource _SourceShoot = null;
		private AudioSource _SourceTapeScratch = null;
		private AudioMixer _Mixer = null;
		private FxType SfxType = FxType.None;
		private float SfxMusicTime = 0f;
		private float SfxEnd = 0f;
		private float SfxNext = -1f;
		private float SfxGap = -1f;
		private float BitCrushEnd = -1f;
		private int BitCrushAmount = 0;
		private int BitCrushDelta = 0;

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
			SourceTapeScratch.clip = m_TapeScratch;
			while (SourceTapeScratch.clip.loadState == AudioDataLoadState.Loading) { }
			SourceTapeScratch.Play();
			SourceTapeScratch.Pause();
			SourceTapeScratch.time = 0f;
		}


		private void FixedUpdate () {
			// Glitch
			if (SfxGap > 0f) {
				// Fxing
				switch (SfxType) {
					case FxType.Glitch:
						if (Time.unscaledTime > SfxNext) {
							Source.time = SfxMusicTime;
							SfxNext += SfxGap;
						}
						break;
					case FxType.Gate:
						if (Time.unscaledTime > SfxNext) {
							Source.mute = !Source.mute;
							SfxNext += SfxGap / 2f;
						}
						break;
					case FxType.SideChain:
						SourceVolume = GetMusicVolume() * Mathf.Lerp(0f, 1f, (Time.unscaledTime - SfxNext + SfxGap) / SfxGap);
						if (Time.unscaledTime > SfxNext) {
							SfxNext += SfxGap;
						}
						break;
					case FxType.ShortDelay:
						SourceVolume = GetMusicVolume() * Mathf.Lerp(1f, 0f, (Time.unscaledTime - SfxNext + SfxGap) / SfxGap);
						if (Time.unscaledTime > SfxNext) {
							Source.time = SfxMusicTime;
							SfxNext += SfxGap;
						}
						break;
				}
				// End
				if (Time.unscaledTime > SfxEnd) {
					SfxGap = -1f;
					TryStopFx(SfxType);
				}
			}
			// BitCrush
			if (BitCrushEnd >= 0f) {
				BitCrushAmount = Mathf.Clamp(
					BitCrushAmount + (int)(BitCrushDelta * Time.fixedDeltaTime), 1, 2048
				);
				if (Time.unscaledTime > BitCrushEnd) {
					TryStopFx(FxType.BitCrush);
				}
			}
		}


		private void OnAudioFilterRead (float[] data, int channel) {
			if (BitCrushEnd < 0f || data == null) { return; }
			int len = data.Length / channel;
			float value = 0f;
			for (int j = 0; j < channel; j++) {
				for (int i = 0; i < len; i++) {
					if (i % BitCrushAmount == 0) {
						value = data[i * channel + j];
					} else {
						data[i * channel + j] = value;
					}
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

				// Source
				case FxType.Glitch:
				case FxType.Gate:
				case FxType.ShortDelay:
				case FxType.SideChain: {
						SfxType = type;
						float duration = Mathf.Min(fxDuration / 1000f, 128f);
						float gap = Mathf.Max(fxDuration / 1000f / paramA, 0.001f);
						SetMusicMute(true);
						SourceVolume = GetMusicVolume();
						Source.pitch = GetMusicPitch();
						Source.mute = false;
						Source.time = SfxMusicTime = GetMusicTime();
						Source.Play();
						SfxGap = gap;
						SfxNext = Time.unscaledTime + gap;
						SfxEnd = Time.unscaledTime + duration;
					}
					break;

				case FxType.BitCrush: {
						BitCrushEnd = Time.unscaledTime + fxDuration / 1000f;
						BitCrushAmount = Mathf.Max(paramA, 1);
						BitCrushDelta = paramB;
						Source.time = GetMusicTime();
						Source.mute = false;
						Source.pitch = GetMusicPitch();
						SourceVolume = GetMusicVolume();
						Source.Play();
						SetMusicMute(true);
					}
					break;


				// Play Clip
				case FxType.KnobShoot: {
						float volume = GetMusicVolume() * paramA / 100f;
						SourceShoot.volume = Mathf.Max(volume * volume, 0f);
						SourceShoot.time = 0f;
						SourceShoot.Play();
					}
					break;
				case FxType.TapeScratch: {
						SourceTapeScratch.pitch = Mathf.Clamp(paramA / 100f, 0f, 10f);
						SourceTapeScratch.time = 0f;
						SourceTapeScratch.Play();
					}
					break;


				// Mixer
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

				case FxType.Wobble:
					FxCors[(int)type] = StartCoroutine(
						Wobble(fxDuration / 1000f, paramA / 100f)
					);
					break;
			}

			// === Func ===
			IEnumerator Flange (float duration) {
				float startTime = Time.unscaledTime;
				float endTime = startTime + duration;
				Mixer.SetFloat(VOLUME_FLANGE, 0f);
				Mixer.SetFloat(VOLUME_MUSIC, -80f);
				float speed = 1f / GetSecondPerBeat();
				while (Time.unscaledTime < endTime) {
					Mixer.SetFloat(DEPTH_FLANGE, Mathf.PingPong((Time.unscaledTime - startTime) * speed, 1f));
					yield return WAIT_FOR_001_SECOND;
				}
				TryStopFx(FxType.Flange);
			}
			IEnumerator PitchShift (float duration, float pitch) {
				float endTime = Time.unscaledTime + duration;
				Mixer.SetFloat(VOLUME_PITCH, 0f);
				Mixer.SetFloat(VOLUME_MUSIC, -80f);
				Mixer.SetFloat(PARAM_PITCH, pitch);
				yield return new WaitUntil(() => Time.unscaledTime > endTime);
				TryStopFx(FxType.PitchShift);
			}
			IEnumerator LowHighPass (bool low, float duration, float cutoff, float delta) {
				float startTime = Time.unscaledTime;
				float endTime = startTime + duration;
				cutoff = Mathf.Clamp01(cutoff);
				Mixer.SetFloat(VOLUME_MUSIC, -80f);
				Mixer.SetFloat(low ? VOLUME_LOW : VOLUME_HIGH, 0f);
				while (Time.unscaledTime < endTime) {
					float t01 = cutoff - Mathf.PingPong((Time.unscaledTime - startTime) * delta, cutoff);
					Mixer.SetFloat(low ? PARAM_LOW : PARAM_HIGH, Util.Remap(
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
				Mixer.SetFloat(VOLUME_MUSIC, -80f);
				Mixer.SetFloat(VOLUME_KNOB, 0f);
				while (Time.unscaledTime < endTime) {
					float t01 = 1f - Mathf.PingPong((Time.unscaledTime - startTime) * delta, 1f);
					Mixer.SetFloat(PARAM_KNOB, Mathf.Lerp(
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
				Mixer.SetFloat(VOLUME_MUSIC, -80f);
				Mixer.SetFloat(VOLUME_WOBBLE, 0f);
				Mixer.SetFloat(PARAM_WOBBLE, Mathf.Lerp(6f, 20f, rate * rate));
				while (Time.unscaledTime < endTime) {
					Mixer.SetFloat(
						DEPTH_WOBBLE,
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
				case FxType.Gate:
				case FxType.ShortDelay:
				case FxType.SideChain:
					SfxGap = -1f;
					SetMusicMute(false);
					SetMusicVolume(1f);
					Source.mute = true;
					Source.Pause();
					break;


				case FxType.KnobShoot:
					SourceShoot.Pause();
					break;
				case FxType.TapeScratch:
					SourceTapeScratch.Pause();
					break;


				case FxType.Flange:
					Mixer.SetFloat(VOLUME_FLANGE, -80f);
					break;
				case FxType.PitchShift:
					Mixer.SetFloat(VOLUME_PITCH, -80f);
					break;
				case FxType.LowPass:
					Mixer.SetFloat(VOLUME_LOW, -80f);
					break;
				case FxType.HighPass:
					Mixer.SetFloat(VOLUME_HIGH, -80f);
					break;
				case FxType.Knob:
					Mixer.SetFloat(VOLUME_KNOB, -80f);
					break;
				case FxType.Wobble:
					Mixer.SetFloat(VOLUME_WOBBLE, -80f);
					break;
				case FxType.BitCrush:
					BitCrushEnd = -1f;
					SetMusicMute(false);
					SetMusicVolume(1f);
					Source.mute = true;
					Source.Pause();
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
			for (int i = 7; i < FxCors.Length; i++) {
				if (FxCors[i] != null) {
					hasCor = true;
					break;
				}
			}
			if (!hasCor) {
				Mixer.SetFloat(VOLUME_MUSIC, 0f);
			}
		}


		#endregion




	}
}