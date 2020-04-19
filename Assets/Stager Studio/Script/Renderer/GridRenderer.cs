namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class GridRenderer : StageRenderer {


		// SUB
		private enum GridMode {
			XX = 0,
			X_ = 1,
			XY = 2,
			_Y = 3,
		}


		// Handler
		public delegate float Float3BoolHandler (float a, float b, float c, bool bo);
		public delegate float Float3Handler (float a, float b, float c);

		// Api
		public static Float3BoolHandler GetAreaBetween { get; set; } = null;
		public static Float3Handler GetSnapedTime { get; set; } = null;
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
		public float TimeGap {
			get => _TimeGap;
			set {
				if (value != _TimeGap) {
					_TimeGap = value;
					SetDirty();
				}
			}
		}
		public float TimeGap_Main { get; set; } = 1f;
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
		public bool UseDynamicSpeed { get; set; } = true;
		public int CountX => _CountXs[Mode];
		public bool Visible { get; set; } = true;

		// Ser
		[SerializeField] private Sprite m_SpriteV = null;
		[SerializeField] private Sprite m_SpriteH = null;
		[SerializeField] private GridMode m_Mode = GridMode.XX;
		[SerializeField] private ushort m_Thickness = 1;
		 
		// Data
		private readonly int[] _CountXs = { 0, 0, 0, 0, };
		private float _TimeGap = 1f;
		private float _TimeOffset = 0f;
		private float _MusicTime = -1f;
		private float _SpeedMuti = 1f;
		private float _ObjectSpeedMuti = 1f;


		protected override void OnMeshFill () {

			if (!Visible) { return; }

			float thick = m_Thickness / 1000f;
			var uvMin0 = m_SpriteV.uv[2];
			var uvMax0 = m_SpriteV.uv[1];
			var uvMin1 = m_SpriteH.uv[2];
			var uvMax1 = m_SpriteH.uv[1];

			// X
			if (Mode != (int)GridMode._Y) {
				ForAllX((x) => AddQuad01(
					x - thick / Scale.x,
					x + thick / Scale.x,
					0f, 1f,
					uvMin0.x, uvMax0.x,
					uvMin0.y, uvMax0.y,
					Vector3.zero
				));
			}

			// Y
			ForAllY((y) => AddQuad01(
				0f, 1f, y - thick / Scale.y, y + thick / Scale.y,
				uvMin1.x, uvMax1.x, uvMin1.y, uvMax1.y, Vector3.zero
			), false, UseDynamicSpeed);

		}


		// API
		public void SetGridTransform (bool enable, bool visible, Vector3 pos = default, Quaternion rot = default, Vector3 scale = default) {
			GridEnabled = enable;
			Visible = visible;
			RendererEnable = GridShowed && GridEnabled;
			if (gameObject.activeSelf != RendererEnable) {
				gameObject.SetActive(RendererEnable);
			}
			if (enable) {
				transform.position = pos;
				transform.rotation = rot;
				transform.localScale = Scale = scale;
				SetDirty();
			}
		}


		public void SetShow (bool show) {
			GridShowed = show;
			gameObject.SetActive(GridShowed && GridEnabled);
			RendererEnable = GridShowed && GridEnabled;
		}


		public Vector3 SnapWorld (Vector3 pos, bool groundY = false, bool floorToIntY = false, bool useDynamicSpeed = true) {
			if (!RendererEnable) { return pos; }
			pos = transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
			pos.x = CountX > 1 ? Util.Snap(pos.x, CountX + 1) : 0f;
			if (groundY) {
				pos.y = 0f;
			} else {
				float minDis = float.MaxValue;
				float resY = pos.y;
				ForAllY((y) => {
					if ((!floorToIntY || y < pos.y) && Mathf.Abs(pos.y - y) < minDis) {
						resY = y;
						minDis = Mathf.Abs(pos.y - y);
					}
				}, false, useDynamicSpeed);
				pos.y = resY;
			}
			pos.z = 0f;
			pos = transform.localToWorldMatrix.MultiplyPoint3x4(pos);
			return pos;
		}


		public void SetCountX (int mode, int value) {
			if (value != _CountXs[mode]) {
				_CountXs[mode] = value;
				SetDirty();
			}
		}


		// LGC
		private void ForAllX (System.Action<float> action) {
			int countX = Mathf.Clamp(CountX + 1, 1, 32);
			for (int i = CountX > 1 ? 0 : 1; i <= (CountX > 1 ? countX : countX - 1); i++) {
				action((float)i / countX);
			}
		}


		private void ForAllY (System.Action<float> action, bool doZero = false, bool useDynamicSpeed = true) {
			switch (m_Mode) {
				case GridMode.XX: {
						if (doZero) {
							action(0f);
						}
						int countY = Mathf.Clamp(CountX + 1, 1, 32);
						for (int i = 0; i <= countY; i++) {
							action((float)i / countY);
						}
					}
					break;
				case GridMode.XY:
				case GridMode._Y: {
						float speedMuti = SpeedMuti * ObjectSpeedMuti;
						float time = GetSnapedTime(MusicTime, TimeGap, TimeOffset);
						float y01 = Mathf.Sign(time - MusicTime) * GetAreaBetween(
							Mathf.Min(MusicTime, time),
							Mathf.Max(MusicTime, time),
							speedMuti, useDynamicSpeed
						);
						if (doZero) {
							action(0f);
						}
						for (int i = 0; i < 64 && y01 < 1f && speedMuti > 0f; i++) {
							if (y01 >= 0f) { action(y01); }
							y01 += GetAreaBetween(time, time + TimeGap, speedMuti, useDynamicSpeed);
							time += TimeGap;
						}
					}
					break;
			}
		}



	}
}