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

		// Ser
		[SerializeField] private Sprite m_SpriteV = null;
		[SerializeField] private Sprite m_SpriteH = null;
		[SerializeField] private Color[] m_Tints = default;
		[SerializeField] private GridMode m_Mode = GridMode.XX;
		[SerializeField] private ushort m_Thickness = 1;

		// Data
		private int _CountX = 0;
		private float _TimeGap = 1f;
		private float _TimeOffset = 0f;
		private float _MusicTime = -1f;
		private float _SpeedMuti = 1f;
		private float _ObjectSpeedMuti = 1f;
		private (Vector3 pos, Quaternion rot, Vector3 scale)? TransformDirty = null;



		protected override void LateUpdate () {
			if (TransformDirty.HasValue) {
				transform.position = TransformDirty.Value.pos;
				transform.rotation = TransformDirty.Value.rot;
				transform.localScale = TransformDirty.Value.scale;
				Scale = TransformDirty.Value.scale;
				TransformDirty = null;
			}
			base.LateUpdate();
		}


		protected override void OnMeshFill () {

			float thick = m_Thickness / 1000f;
			var uvMin0 = m_SpriteV.uv[2];
			var uvMax0 = m_SpriteV.uv[1];
			var uvMin1 = m_SpriteH.uv[2];
			var uvMax1 = m_SpriteH.uv[1];
			Tint = m_Tints[(int)m_Mode];

			// X
			ForAllX((x) => AddQuad01(
				x - thick / Scale.x,
				x + thick / Scale.x,
				0f, 1f,
				uvMin0.x, uvMax0.x,
				uvMin0.y, uvMax0.y,
				Vector3.zero
			));

			// Y
			ForAllY((y) => AddQuad01(
				0f, 1f, y - thick / Scale.y, y + thick / Scale.y,
				uvMin1.x, uvMax1.x, uvMin1.y, uvMax1.y, Vector3.zero
			));

		}


		// API
		public void SetGridTransform (bool enable, Vector3 pos = default, Quaternion rot = default, Vector3 scale = default) {
			GridEnabled = enable;
			gameObject.SetActive(GridShowed && GridEnabled);
			RendererEnable = GridShowed && GridEnabled;
			if (enable) {
				TransformDirty = (pos, rot, scale);
			}
		}


		public void SetShow (bool show) {
			GridShowed = show;
			gameObject.SetActive(GridShowed && GridEnabled);
			RendererEnable = GridShowed && GridEnabled;
		}


		public Vector3 SnapWorld (Vector3 pos, bool groundY = false, bool floorToIntY = false) {
			if (!RendererEnable) { return pos; }
			pos = transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
			pos.x = Util.Snap(pos.x, CountX + 1);
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
				}, true);
				pos.y = resY;
			}
			pos.z = 0f;
			pos = transform.localToWorldMatrix.MultiplyPoint3x4(pos);
			return pos;
		}


		// LGC
		private void ForAllX (System.Action<float> action) {
			int countX = Mathf.Clamp(CountX + 1, 1, 32);
			for (int i = 0; i <= countX; i++) {
				action((float)i / countX);
			}
		}


		private void ForAllY (System.Action<float> action, bool doZero = false) {
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
				case GridMode.XY: {
						float speedMuti = SpeedMuti * ObjectSpeedMuti;
						float time = GetSnapedTime(MusicTime, TimeGap, TimeOffset);
						float y01 = Mathf.Sign(time - MusicTime) * GetAreaBetween(
							Mathf.Min(MusicTime, time),
							Mathf.Max(MusicTime, time),
							speedMuti
						);
						if (doZero) {
							action(0f);
						}
						for (int i = 0; i < 64 && y01 < 1f && speedMuti > 0f; i++) {
							if (y01 > 0f) { action(y01); }
							y01 += GetAreaBetween(time, time + TimeGap, speedMuti);
							time += TimeGap;
						}
					}
					break;
			}
		}



	}
}