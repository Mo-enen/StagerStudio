namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class ObjectRenderer : ItemRenderer {


		public enum LoopType {
			ItemType = 0,
			Forward = 1,
			Loop = 2,
		}


		// API
		public SkinData SkinData {
			get => _Data;
			set {
				if (value != _Data) {
					_Data = value;
					SetDirty();
					ReCalculateFrame();
					// Material
					if (!(value is null)) {
						var mats = Renderer.materials;
						for (int i = 0; i < mats.Length; i++) {
							mats[i].mainTexture = value.Texture;
						}
					}
				}
			}
		}
		public SkinType Type {
			get => _TypeIndex;
			set {
				if (value != _TypeIndex) {
					_TypeIndex = value;
					Frame = 0;
					SetDirty();
				}
			}
		}
		public int ItemType {
			get => _ItemType;
			set {
				if (value != _ItemType) {
					_ItemType = value;
					ReCalculateFrame();
				}
			}
		}
		public float LifeTime {
			get => _LifeTime;
			set {
				if (Mathf.Abs(value - _LifeTime) > 0.0001f) {
					_LifeTime = value;
					ReCalculateFrame();
				}
			}
		}
		public float Duration {
			get => _Duration;
			set {
				if (Mathf.Abs(value - _Duration) > 0.0001f) {
					_Duration = value;
				}
			}
		}
		public int Loop {
			get => (int)m_LoopType;
			set => m_LoopType = (LoopType)value;
		}

		// Short
		private AnimatedItemData AniData => SkinData?.Items[(int)Type];


		// Ser
		[SerializeField] private bool m_Allow3D = true;
		[SerializeField] private LoopType m_LoopType = LoopType.ItemType;

		// Data
		private SkinData _Data = null;
		private SkinType _TypeIndex = SkinType.Stage;
		private float _LifeTime = 0f;
		private int _ItemType = 0;
		private float _Duration = 0f;

		// Cache
		private int Frame = 0;


		// MSG
		protected override void OnMeshFill () {

			// Data
			var ani = AniData;
			if (ani is null || SkinData.Texture is null || ani.Rects.Count == 0) { return; }

			// Calculate New
			var rData = ani.Rects[Frame];
			var scale = Scale * Mathf.Max(SkinData.ScaleMuti, 0f);
			if (rData.Width <= 0 || rData.Height <= 0) { return; }
			float tWidth = SkinData.Texture.width;
			float tHeight = SkinData.Texture.height;
			bool is3D = m_Allow3D && rData.Is3D;
			float thickness3D = is3D ? rData.Thickness3D / SkinData.ScaleMuti : 0f;
			Vector3 offset = new Vector3(0f, 0f, -thickness3D);
			// Add Rect Mesh Cache
			float uvL = rData.L / tWidth;
			float uvR = rData.R / tWidth;
			float uvD = rData.D / tHeight;
			float uvU = rData.U / tHeight;
			if (rData.BorderL <= 0 && rData.BorderR <= 0 && rData.BorderD <= 0 && rData.BorderU <= 0) {
				// No Border
				AddQuad01(0f, 1f, 0f, 1f, uvL, uvR, uvD, uvU, offset, Tint);
			} else {
				// Nine-Slice
				bool hasBorderL = rData.BorderL > 0;
				bool hasBorderR = rData.BorderR > 0;
				bool hasBorderD = rData.BorderD > 0;
				bool hasBorderU = rData.BorderU > 0;
				float _ornMinL = hasBorderL ? (rData.BorderL / (float)(rData.BorderL + rData.BorderR)) : 0f;
				float _ornMaxR = hasBorderR ? (rData.BorderR / (float)(rData.BorderL + rData.BorderR)) : 0f;
				float _ornMinD = hasBorderD ? (rData.BorderD / (float)(rData.BorderD + rData.BorderU)) : 0f;
				float _ornMaxU = hasBorderU ? (rData.BorderU / (float)(rData.BorderD + rData.BorderU)) : 0f;
				// Vs
				float _l = Mathf.Min(rData.BorderL / scale.x, _ornMinL);
				float _r = Mathf.Max(1f - (rData.BorderR / scale.x), 1f - _ornMaxR);
				float _d = Mathf.Min(rData.BorderD / scale.y, _ornMinD);
				float _u = Mathf.Max(1f - (rData.BorderU / scale.y), 1f - _ornMaxU);
				// UV
				float _uvL0 = rData.L / tWidth;
				float _uvR1 = rData.R / tWidth;
				float _uvD0 = rData.D / tHeight;
				float _uvU1 = rData.U / tHeight;
				float _uvL = hasBorderL ? (Util.Remap(
					0f,
					rData.BorderL / _ornMinL,
					rData.L,
					rData.L + rData.BorderL,
					scale.x
				) / tWidth) : _uvL0;
				float _uvR = hasBorderR ? (Util.Remap(
					0f,
					rData.BorderR / _ornMaxR,
					rData.R,
					rData.R - rData.BorderR,
					scale.x
				) / tWidth) : _uvR1;
				float _uvD = hasBorderD ? (Util.Remap(
					0f,
					rData.BorderD / _ornMinD,
					rData.D,
					rData.D + rData.BorderD,
					scale.y
				) / tHeight) : _uvD0;
				float _uvU = hasBorderU ? (Util.Remap(
					0f,
					rData.BorderU / _ornMaxU,
					rData.U,
					rData.U - rData.BorderU,
					scale.y
				) / tHeight) : _uvU1;
				// Quad
				AddNineCliceQuad(
					hasBorderL, hasBorderR, hasBorderD, hasBorderU,
					_l, _r, _d, _u,
					_uvL, _uvR, _uvD, _uvU,
					_uvL0, _uvR1, _uvD0, _uvU1,
					offset, Tint, true, true
				);
				if (is3D) {
					// Back
					AddNineCliceQuad(
						hasBorderL, hasBorderR, hasBorderD, hasBorderU,
						_l, _r, _d, _u,
						_uvL, _uvR, _uvD, _uvU,
						_uvL0, _uvR1, _uvD0, _uvU1,
						Vector3.zero, Tint, true, false
					);
				}
			}
			// 3D
			if (is3D) {
				float uvL3d = uvL - rData.Thickness3D / tWidth;
				float uvD3d = uvD - rData.Thickness3D / tHeight;
				// L
				AddQuad01(0f, 1f, 0f, thickness3D, uvL3d, uvL, uvD, uvU, 1, 2, new Vector3(-Pivot.x, 0f, -thickness3D), Tint, false);
				// R
				AddQuad01(1f, 0f, 0f, thickness3D, uvL3d, uvL, uvD, uvU, 1, 2, new Vector3(1f - Pivot.x, 0f, -thickness3D), Tint, false);
				// D
				AddQuad01(1f, 0f, 0f, thickness3D, uvL, uvR, uvD3d, uvD, 0, 2, new Vector3(0f, -Pivot.y, -thickness3D), Tint, true);
				// U
				AddQuad01(0f, 1f, 0f, thickness3D, uvL, uvR, uvD3d, uvD, 0, 2, new Vector3(0f, 1f - Pivot.y, -thickness3D), Tint, true);
			}

		}


		// LGC
		private void ReCalculateFrame () {
			var ani = AniData;
			int frame = ani is null ? 0 : ani.GetFrame(ItemType, (int)m_LoopType, LifeTime);
			if (frame != Frame) {
				Frame = frame;
				SetDirty();
			}
		}


	}
}