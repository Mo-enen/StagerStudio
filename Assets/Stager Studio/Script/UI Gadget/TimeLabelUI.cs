namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	[RequireComponent(typeof(Text))]
	public class TimeLabelUI : MonoBehaviour {


		// API
		public float Time {
			get => _Time;
			set {
				_Time = value;
				RefreshUI();
			}
		}
		public bool ShowBeatTime {
			get => _ShowBeatTime;
			set {
				_ShowBeatTime = value;
				RefreshUI();
			}
		}
		public float BPM {
			get => _BPM;
			set => _BPM = Mathf.Max(value, 1f);
		}
		public float Shift { get; set; } = 0f;

		// Short
		private Text Label => _Label != null ? _Label : (_Label = GetComponent<Text>());

		// Data
		private Text _Label = null;
		private float _Time = 0f;
		private bool _ShowBeatTime = false;
		private float _BPM = 120f;


		// MSG
		private void Start () {
			RefreshUI();
		}


		// LGC
		private void RefreshUI () {
			Label.text = _ShowBeatTime ?
				$"{Util.Time_to_Beat(Time, BPM, Shift).ToString("0.##")} B" :
				$"{Mathf.FloorToInt(_Time / 60f).ToString()}\' {Mathf.FloorToInt(_Time % 60f).ToString("00")}\" {Mathf.FloorToInt((_Time % 1f) * 100f).ToString("00")}";
		}



	}
}