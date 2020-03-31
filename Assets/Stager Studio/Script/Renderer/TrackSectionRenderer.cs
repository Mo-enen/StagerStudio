namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class TrackSectionRenderer : StageRenderer {



		// Handler
		public delegate float Float4Handler (float a, float b, float c);

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
		[SerializeField] private Sprite m_SpriteH = null;
		[SerializeField] private Sprite m_SectionBody = null;
		[SerializeField] private ushort m_Thickness = 1;
		[SerializeField] private ushort m_Width = 40;
		[SerializeField] private Color m_SectionColor = default;
		[SerializeField] private Color[] m_RulerColors = default;

		// Data
		private float _TimeGap = 1f;
		private float _TimeOffset = 0f;
		private float _MusicTime = -1f;
		private float _SpeedMuti = 1f;
		private int _BeatPerSection = 4;



		protected override void OnMeshFill () {

			if (BeatPerSection <= 0 || Scale.x < 0.0001f || SpeedMuti <= 0f) { return; }

			float thick = m_Thickness / 1000f;
			var uvMin0 = m_SectionBody.uv[2];
			var uvMax0 = m_SectionBody.uv[1];
			var uvMin1 = m_SpriteH.uv[2];
			var uvMax1 = m_SpriteH.uv[1];
			float sectionTimeGap = BeatPerSection * TimeGap;

			// Line
			float time = GetSnapedTime(MusicTime, sectionTimeGap, TimeOffset);
			float y01 = Mathf.Sign(time - MusicTime) * GetAreaBetween(
				Mathf.Min(MusicTime, time),
				Mathf.Max(MusicTime, time),
				SpeedMuti
			);
			Tint = m_SectionColor;
			for (int i = 0; i < 64 && y01 < 1f; i++) {
				if (y01 > 0f) {
					AddQuad01(
						0f, 1f, y01 - thick, y01 + thick,
						uvMin1.x, uvMax1.x, uvMin1.y, uvMax1.y, Vector3.zero
					);
				}
				y01 += GetAreaBetween(time, time + sectionTimeGap, SpeedMuti);
				time += sectionTimeGap;
			}

			// Ruler
			time = GetSnapedTime(MusicTime, sectionTimeGap, TimeOffset) - sectionTimeGap;
			y01 = Mathf.Sign(time - MusicTime) * GetAreaBetween(
				Mathf.Min(MusicTime, time),
				Mathf.Max(MusicTime, time),
				SpeedMuti
			);
			for (int i = 0; i < 64 && y01 < 1f; i++) {
				for (int j = 0; j < BeatPerSection; j++) {
					Tint = m_RulerColors[j % m_RulerColors.Length];
					float nextY01 = y01 + GetAreaBetween(time, time + TimeGap, SpeedMuti);
					if (time >= MusicTime - TimeGap) {
						AddQuad01(
							0f, m_Width / 1000f / Scale.x,
							Mathf.Max(y01, 0f),
							Mathf.Min(nextY01, 1f),
							uvMin0.x, uvMax0.x, uvMin0.y, uvMax0.y, Vector3.zero
						);
					}
					y01 = nextY01;
					time += TimeGap;
				}
			}

		}



	}
}