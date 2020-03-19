namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class GridRenderer : StageRenderer {


		// SUB
		private enum GridMode {
			XX = 0,
			X0 = 1,
			XY = 2,
		}


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
		public float ObjectSpeedMuti {
			get => _ObjectSpeedMuti;
			set {
				if (value != _ObjectSpeedMuti) {
					_ObjectSpeedMuti = value;
					SetDirty();
				}
			}
		}
		public int CountX {
			get => _CountX;
			set {
				if (value != _CountX) {
					_CountX = value;
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
		public int Mode {
			get => (int)m_Mode;
			set {
				if (value != (int)m_Mode) {
					m_Mode = (GridMode)value;
					SetDirty();
				}
			}
		}
		public bool GridShowed { get; private set; } = false;
		public bool GridEnabled { get; private set; } = false;

		// Ser
		[SerializeField] private Sprite m_SpriteV = null;
		[SerializeField] private Sprite m_SpriteH = null;
		[SerializeField] private Color[] m_Tints = default;
		[SerializeField] private GridMode m_Mode = GridMode.XX;
		[SerializeField] private bool m_UseX = true;
		[SerializeField] private bool m_UseY = true;

		// Data
		private int _CountX = 0;
		private float _TimeGap = 1f;
		private float _TimeOffset = 0f;
		private float _MusicTime = -1f;
		private float _SpeedMuti = 1f;
		private float _ObjectSpeedMuti = 1f;


		// MSG
		private void Awake () {
			SetSortingLayer(SortingLayer.NameToID("UI"), 0);
		}


		protected override void OnMeshFill () {

			if (m_SpriteV is null || m_SpriteH is null || (!m_UseX && !m_UseY)) { return; }

			const float THICK = 0.001f;
			var uvMin0 = m_SpriteV.uv[2];
			var uvMax0 = m_SpriteV.uv[1];
			var uvMin1 = m_SpriteH.uv[2];
			var uvMax1 = m_SpriteH.uv[1];
			Tint = m_Tints[(int)m_Mode];

			// X
			if (m_UseX) {
				int countX = Mathf.Clamp(CountX + 1, 1, 32);
				for (int i = 0; i <= countX; i++) {
					AddQuad01(
						(float)i / countX - THICK / Scale.x,
						(float)i / countX + THICK / Scale.x,
						0f, 1f,
						uvMin0.x, uvMax0.x,
						uvMin0.y, uvMax0.y,
						Vector3.zero
					);
				}
			}

			// Y
			if (m_UseY) {
				switch (m_Mode) {
					case GridMode.XX: {
							int countY = Mathf.Clamp(CountX + 1, 1, 32);
							for (int i = 0; i <= countY; i++) {
								AddQuad01(0f, 1f, (float)i / countY - THICK / Scale.y, (float)i / countY + THICK / Scale.y, uvMin1.x, uvMax1.x, uvMin1.y, uvMax1.y, Vector3.zero);
							}
						}
						break;
					case GridMode.X0:
						break;
					case GridMode.XY: {
							float speedMuti = SpeedMuti * ObjectSpeedMuti;
							float time = GetSnapedTime(MusicTime, TimeGap, TimeOffset);
							float y01 = Mathf.Sign(time - MusicTime) * GetAreaBetween(
								Mathf.Min(MusicTime, time),
								Mathf.Max(MusicTime, time),
								speedMuti
							);
							for (int i = 0; i < 64 && y01 < 1f && speedMuti > 0f; i++) {
								if (y01 > 0f) {
									AddQuad01(0f, 1f, y01 - THICK / Scale.y, y01 + THICK / Scale.y, uvMin1.x, uvMax1.x, uvMin1.y, uvMax1.y, Vector3.zero);
								}
								y01 += GetAreaBetween(time, time + TimeGap, speedMuti);
								time += TimeGap;
							}
						}
						break;
				}
			}

		}


		// API
		public void SetGridTransform (bool enable, Vector3 pos = default, Quaternion rot = default, Vector3 scale = default) {
			GridEnabled = enable;
			enabled = RendererEnable = GridShowed && GridEnabled;
			if (enable) {
				transform.position = pos;
				transform.rotation = rot;
				transform.localScale = scale;
				Scale = scale;
			}
		}


		public void SetShow (bool show) {
			GridShowed = show;
			enabled = RendererEnable = GridShowed && GridEnabled;
		}


	}
}