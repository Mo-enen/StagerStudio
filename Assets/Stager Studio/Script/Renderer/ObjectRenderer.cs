namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class ObjectRenderer : StageRenderer {


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
		public float LifeTime {
			get => _LifeTime;
			set {
				if (Mathf.Abs(value - _LifeTime) > 0.0001f) {
					_LifeTime = value;
					ReCalculateFrame();
				}
			}
		}

		// Short
		private AnimatedItemData AniData => SkinData?.Items[(int)Type];

		// Data
		private SkinData _Data = null;
		private SkinType _TypeIndex = SkinType.Stage;
		private float _LifeTime = 0f;

		// Cache
		private int Frame = 0;


		// MSG
		protected override void OnMeshFill () {

			// Data
			var ani = AniData;
			if (ani is null || SkinData.Texture is null || ani.Rects.Count == 0) {
				ClearMeshCache();
				return;
			}

			// Calculate New
			var rData = ani.Rects[Frame];
			var scale = Scale * Mathf.Max(SkinData.ScaleMuti, 0f);
			if (rData.Width <= 0 || rData.Height <= 0) { return; }
			float tWidth = SkinData.Texture.width;
			float tHeight = SkinData.Texture.height;
			float thickness3D = Mathf.Abs(ani.Thickness3D);
			bool is3D = ani.Thickness3D > 0.0001f;
			Vector3 offset3D = Vector3.back * thickness3D;
			// Add Rect Mesh Cache
			if (rData.BorderL <= 0 && rData.BorderR <= 0 && rData.BorderD <= 0 && rData.BorderU <= 0) {
				// No Border
				AddQuad01_3D(0f, 1f, 0f, 1f, rData.L / tWidth, rData.R / tWidth, rData.D / tHeight, rData.U / tHeight, offset3D, thickness3D, is3D, is3D, is3D, is3D);
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
				if (hasBorderL) {
					if (hasBorderD) {
						// DL
						AddQuad01_3D(0f, _l, 0f, _d, _uvL0, _uvL, _uvD0, _uvD, offset3D, thickness3D, is3D, false, is3D, false);
					}
					if (hasBorderU) {
						// UL
						AddQuad01_3D(0f, _l, _u, 1f, _uvL0, _uvL, _uvU, _uvU1, offset3D, thickness3D, is3D, false, false, is3D);
					}
					// L Scale
					AddQuad01_3D(0f, _l, _d, _u, _uvL0, _uvL, _uvD, _uvU, offset3D, thickness3D, is3D, false, is3D && !hasBorderD, is3D && !hasBorderU);
				}
				if (hasBorderD) {
					// DM
					AddQuad01_3D(_l, _r, 0f, _d, _uvL, _uvR, _uvD0, _uvD, offset3D, thickness3D, false, false, is3D, is3D && !hasBorderU);
				}
				if (hasBorderU) {
					// UM
					AddQuad01_3D(_l, _r, _u, 1f, _uvL, _uvR, _uvU, _uvU1, offset3D, thickness3D, false, false, is3D && !hasBorderD, is3D);
				}
				// MM
				AddQuad01_3D(_l, _r, _d, _u, _uvL, _uvR, _uvD, _uvU, offset3D, thickness3D, is3D && !hasBorderL, is3D && !hasBorderR, is3D && !hasBorderD, is3D && !hasBorderU);
				if (hasBorderR) {
					if (hasBorderD) {
						// DR
						AddQuad01_3D(_r, 1f, 0f, _d, _uvR, _uvR1, _uvD0, _uvD, offset3D, thickness3D, false, is3D, is3D, false);
					}
					if (hasBorderU) {
						// UR
						AddQuad01_3D(_r, 1f, _u, 1f, _uvR, _uvR1, _uvU, _uvU1, offset3D, thickness3D, false, is3D, false, is3D);
					}
					// R
					AddQuad01_3D(_r, 1f, _d, _u, _uvR, _uvR1, _uvD, _uvU, offset3D, thickness3D, false, is3D, is3D && !hasBorderD, is3D && !hasBorderU);
				}
			}
		}


		// LGC
		private void ReCalculateFrame () {
			var ani = AniData;
			int frame = ani is null ? 0 : ani.GetFrame(_LifeTime);
			if (frame != Frame) {
				Frame = frame;
				SetDirty();
			}
		}


	}
}