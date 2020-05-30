namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Audio;


	public class StageMusic : MonoBehaviour {




		#region --- SUB ---


		public delegate void VoidHandler ();
		public delegate void VoidFloatFloatHandler (float a, float b);
		public delegate void VoidBoolHandler (bool a);


		#endregion




		#region --- VAR ---


		// Handler
		public static VoidBoolHandler OnMusicPlayPause { get; set; } = null;
		public static VoidFloatFloatHandler OnMusicTimeChanged { get; set; } = null;
		public static VoidHandler OnMusicClipLoaded { get; set; } = null;
		public static VoidHandler OnPitchChanged { get; set; } = null;

		// API
		public bool IsReady {
			get; private set;
		} = false;

		public bool IsPlaying => IsReady && Source.isPlaying;

		public float Time {
			get => IsReady && IsPlaying ? Source.time : _Time;
			private set => _Time = value;
		}

		public float Pitch {
			get => Source.pitch;
			set {
				if (value != Source.pitch) {
					Source.pitch = value;
					OnPitchChanged();
				}
			}
		}

		public float Duration {
			get;
			private set;
		} = DURATION_MIN;

		public float Volume {
			get => Mathf.Sqrt(Source.volume);
			set => Source.volume = Mathf.Max(value * value, 0f);
		}

		public float SfxVolume {
			get => Mathf.Sqrt(_SfxVolume);
			set {
				_SfxVolume = value * value;
			}
		}

		public bool Mute {
			get {
				return Source.mute;
			}
			set {
				Source.mute = value;
			}
		}


		// Short Cut
		private AudioSource Source {
			get {
				if (!_Source) {
					if (!GetComponent<AudioListener>()) {
						gameObject.AddComponent<AudioListener>();
					}
					_Source = gameObject.AddComponent<AudioSource>();
					_Source.playOnAwake = false;
					_Source.loop = false;
					_Source.outputAudioMixerGroup = m_MixerGroup;
				}
				return _Source;
			}
		}

		private AudioSource[] ClickSoundSources {
			get {
				if (_ClickSource == null && gameObject != null) {
					if (transform.childCount > 0) {
						var tf = transform.GetChild(0);
						_ClickSource = tf.GetComponents<AudioSource>();
					}
				}
				return _ClickSource;
			}
		}

		// Ser
		[SerializeField] private AudioClip m_DefaultSfx = null;
		[SerializeField] private AudioMixerGroup m_MixerGroup = null;

		// Data
		private const float DURATION_MIN = 0.001f;
		private AudioSource _Source = null;
		private AudioSource[] _ClickSource = null;
		private readonly List<AudioClip> ClickSoundClips = new List<AudioClip>();
		private float _Time = 0f;
		private float _SfxVolume = 1f;
		private float LastUpdateTime = 0f;
		private float LastPlayTime = 0f;
		private float LastClickSoundTime = float.MinValue;
		private bool PrevPlaying = false;


		#endregion



		#region --- MSG ---


		private void Update () {
			if (IsReady) {
				if (Time != LastUpdateTime) {
					OnMusicTimeChanged(Time, Duration);
					LastUpdateTime = Time;
				}
			}
		}


		#endregion



		#region --- API ---


		public void Play () => PlayLogic();


		public void Pause () => PauseLogic();


		public void Stop () => StopLogic();


		public void Repause () => RepauseLogic();


		public void SetClip (AudioClip clip) => SetClipLogic(clip);


		public void UseMixer (bool use) => Source.outputAudioMixerGroup = use ? m_MixerGroup : null;


		public void PlayPause () {
			if (IsPlaying) {
				PauseLogic();
			} else {
				PlayLogic();
			}
		}


		public void PlayRepause () {
			if (IsPlaying) {
				RepauseLogic();
			} else {
				PlayLogic();
			}
		}


		public void StartSeek01 (float time01) => StartSeek(time01 * Duration);


		public void StartSeek (float time) {
			PrevPlaying = IsPlaying;
			PauseLogic();
			SeekLogic(time);
		}


		public void Seek01 (float time01) => Seek(time01 * Duration);


		public void Seek (float time) => SeekLogic(time);


		public void StopSeek01 (float time01) => StopSeek(time01 * Duration);


		public void StopSeek (float time) {
			SeekLogic(time);
			if (PrevPlaying) {
				PlayLogic();
			} else {
				PauseLogic();
			}
		}


		public void SetClickSounds (List<AudioClip> clips) {
			if (clips is null || clips.Count == 0) {
				clips = new List<AudioClip>() { m_DefaultSfx };
			}
			ClickSoundClips.Clear();
			ClickSoundClips.AddRange(clips);
			_ClickSource = null;
			if (transform.childCount > 0) {
				transform.DestroyAllChildImmediately();
			}
			var root = new GameObject().transform;
			root.SetParent(transform);
			root.localPosition = Vector3.zero;
			foreach (var clip in clips) {
				var source = root.gameObject.AddComponent<AudioSource>();
				source.playOnAwake = false;
				source.loop = false;
				source.volume = _SfxVolume;
				source.clip = clip;
			}
		}


		public void PlayClickSound (int index, float volume) {
			if (ClickSoundSources is null || UnityEngine.Time.time <= LastClickSoundTime + 0.001f) { return; }
			LastClickSoundTime = UnityEngine.Time.time;
			int len = ClickSoundSources.Length;
			if (len == 0) { return; }
			index = Mathf.Clamp(index, 0, len - 1);
			var source = ClickSoundSources[index];
			source.volume = volume * SfxVolume;
			source.PlayDelayed(0);
		}


		public void StopClickSounds () {
			if (ClickSoundSources == null) { return; }
			foreach (var source in ClickSoundSources) {
				if (source is null) { continue; }
				source.Stop();
			}
		}


		#endregion




		#region --- UI ---


		public void UI_Play () => PlayLogic();


		public void UI_Pause () => PauseLogic();


		public void UI_PlayPause () => PlayPause();


		public void UI_PlayRepause () => PlayRepause();


		public void UI_Repause () => RepauseLogic();


		public void UI_StartSeek01 (float time01) => StartSeek01(time01);


		public void UI_Seek01 (float time01) => Seek01(time01);


		public void UI_StopSeek01 (float time01) => StopSeek01(time01);


		#endregion




		#region --- LGC ---



		// Control
		private void PlayLogic () {
			if (!IsReady) { return; }
			float time = Mathf.Clamp(Time, 0f, Duration - 0.01f);
			if (!IsPlaying) {
				Source.time = time;
			}
			Source.PlayDelayed(Source.time);
			//Source.UnPause();
			LastPlayTime = time;
			OnMusicPlayPause(IsPlaying);
			ResetInvoke();
		}


		private void PauseLogic () {
			if (!IsReady) { return; }
			if (IsPlaying) {
				Time = Source.time;
			}
			Source.Pause();
			OnMusicPlayPause(IsPlaying);
			ResetInvoke();
		}


		private void SeekLogic (float time) {
			if (!IsReady) { return; }
			time = Mathf.Clamp(time, 0f, Duration - 0.01f);
			Time = time;
			if (IsPlaying) {
				Source.time = time;
			}
			ResetInvoke();
		}


		private void RepauseLogic () {
			if (!IsReady) { return; }
			Source.Pause();
			Source.time = LastPlayTime;
			Time = LastPlayTime;
			OnMusicPlayPause(IsPlaying);
			ResetInvoke();
		}


		private void StopLogic () {
			if (!IsReady) { return; }
			Source.Pause();
			Source.time = 0f;
			Time = 0f;
			OnMusicPlayPause(IsPlaying);
			ResetInvoke();
		}


		// System
		private void SetClipLogic (AudioClip clip) {
			Source.clip = clip;
			if (clip) {
				while (Source.clip.loadState == AudioDataLoadState.Loading) { }
				Duration = Mathf.Max(clip.length, DURATION_MIN);
			}
			IsReady = clip && clip.loadState == AudioDataLoadState.Loaded;
			if (IsReady) {
				SeekLogic(0);
				Source.Play();
				Source.Pause();
				SeekLogic(0);
			}
			OnMusicClipLoaded?.Invoke();
			ResetInvoke();
		}


		// Invoke
		private void ResetInvoke () {
			if (IsPlaying && Pitch != 0f) {
				if (!IsInvoking("PauseForEnd")) {
					Invoke(
						"PauseForEnd",
						Pitch > 0f ?
							(Duration - Time - float.Epsilon) / Pitch :
							(float.Epsilon - Time) / Pitch
					);
				}
			} else {
				if (IsInvoking("PauseForEnd")) {
					CancelInvoke("PauseForEnd");
				}
			}
		}


		private void PauseForEnd () {
			Source.Pause();
			Time = Source.time;
			OnMusicPlayPause(IsPlaying);
		}



		#endregion




	}
}