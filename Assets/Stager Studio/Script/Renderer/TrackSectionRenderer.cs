namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class TrackSectionRenderer : ItemRenderer {



		// Handler
		public delegate float Float4Handler (float a, float b, float c);
		public delegate float FloatHandler ();

		// Api
		public static Float4Handler GetAreaBetween { get; set; } = null;
		public static Float4Handler GetSnapedTime { get; set; } = null;
		public float MusicTime {
			get => _MusicTime;
			set {
				if (value != _MusicTime) {
					_MusicTime = value;
					SetDirty();
				}
			}
		}
		public float SpeedMuti {
			get => _SpeedMuti;
			set {
				if (value != _SpeedMuti) {
					_SpeedMuti = value;
					SetDirty();
				}
			}
		}
		public float TimeGap {
			get => _TimeGap;
			set {
				if (value != _TimeGap) {
					_TimeGap = value;
					SetDirty();
				}
			}
		}
		public float TimeOffset {
			get => _TimeOffset;
			set {
				if (value != _TimeOffset) {
					_TimeOffset = value;
					SetDirty();
				}
			}
		}
		public int BeatPerSection {
			get => _BeatPerSection;
			set {
				if (value != _BeatPerSection) {
					_BeatPerSection = value;
					SetDirty();
				}
			}
		}

		// Ser
		[SerializeField] private Sprite m_SectionBody = null;
		[SerializeField] private ushort m_Thickness = 1;
		[SerializeField] private Color[] m_RulerColors = default;

		// Data
		private float _TimeGap = 1f;
		private float _TimeOffset = 0f;
		private float _MusicTime = -1f;
		private float _SpeedMuti = 1f;
		private int _BeatPerSection = 4;



		protected override void OnMeshFill () {

			if (BeatPerSection <= 0 || Scale.x < 0.0001f || SpeedMuti <= 0f) { return; }

			float thickA = m_Thickness / 618f;
			float thickB = m_Thickness / 1000f;
			var uvMin = m_SectionBody.uv[2];
			var uvMax = m_SectionBody.uv[1];
			float sectionTimeGap = BeatPerSection * TimeGap;

			// Lines
			float time = GetSnapedTime(MusicTime, sectionTimeGap, TimeOffset) - sectionTimeGap;
			float y01 = Mathf.Sign(time - MusicTime) * GetAreaBetween(
				Mathf.Min(MusicTime, time),
				Mathf.Max(MusicTime, time),
				SpeedMuti
			);
			Color tint;
			for (int i = 0; i < 64 && y01 < 1f; i++) {
				for (int j = 0; j < BeatPerSection && y01 < 1f; j++) {
					tint = m_RulerColors[j % m_RulerColors.Length];
					tint.a = Alpha;
					if (time >= MusicTime - TimeGap && y01 > 0f) {
						AddQuad01(
							0f, 1f,
							y01 - (j == 0 ? thickA : thickB),
							y01 + (j == 0 ? thickA : thickB),
							uvMin.x, uvMax.x, uvMin.y, uvMax.y, Vector3.zero, tint
						);
					}
					y01 += GetAreaBetween(time, time + TimeGap, SpeedMuti);
					time += TimeGap;
				}
			}

		}



	}
}