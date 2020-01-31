﻿namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class StageRenderer : MonoBehaviour {




		#region --- VAR ---

		// Const
		private const float DISC_GAP = 0.001f;
		private const float SCALE_GAP = 0.0001f;

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
					SetDirty();
				}
			}
		}
		public Vector2 Scale {
			get => _Scale;
			set {
				if (Mathf.Abs(value.x - _Scale.x) > 0.0001f || Mathf.Abs(value.y - _Scale.y) > 0.0001f) {
					_Scale = value;
					SetDirty();
				}
			}
		}
		public Vector2 Pivot {
			get => _Pivot;
			set {
				if (Mathf.Abs(value.x - _Pivot.x) > 0.0001f || Mathf.Abs(value.y - _Pivot.y) > 0.0001f) {
					_Pivot = value;
					SetDirty();
				}
			}
		}
		public Color32 Tint {
			get => _Tint;
			set {
				if (value.r != _Tint.r || value.g != _Tint.g || value.b != _Tint.b || value.a != _Tint.a) {
					_Tint = value;
					SetDirty();
				}
			}
		}
		public float Alpha {
			get => _Tint.a / (float)byte.MaxValue;
			set {
				byte a = (byte)Mathf.RoundToInt(value * byte.MaxValue);
				if (a != _Tint.a) {
					_Tint.a = a;
					SetDirty();
				}
			}
		}
		public float Disc {
			get => _Disc;
			set {
				if (Mathf.Abs(value - _Disc) > 0.0001f) {
					_Disc = value;
					SetDirty();
					ReCalculateFrame();
				}
			}
		}
		public float DiscWidth {
			get => _DiscWidth; set {
				if (Mathf.Abs(value - _DiscWidth) > 0.0001f) {
					_DiscWidth = value;
					SetDirty();
				}
			}
		}
		public float DiscHeight {
			get => _DiscHeight; set {
				if (Mathf.Abs(value - _DiscHeight) > 0.0001f) {
					_DiscHeight = value;
					SetDirty();
				}
			}
		}
		public float DiscOffsetY {
			get => _DiscOffsetY; set {
				if (Mathf.Abs(value - _DiscOffsetY) > 0.0001f) {
					_DiscOffsetY = value;
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
		public int SortingLayer => Renderer.sortingLayerID;
		public bool RendererEnable {
			get => Renderer.enabled;
			set {
				if (value != Renderer.enabled) {
					Renderer.enabled = value;
				}
			}
		}

		// Short
		private AnimatedItemData AniData => SkinData?.Items[(int)Type];
		private MeshRenderer Renderer => _Renderer != null ? _Renderer : (_Renderer = GetComponent<MeshRenderer>());
		private MeshFilter Filter => _Filter != null ? _Filter : (_Filter = GetComponent<MeshFilter>());
		private Mesh Mesh {
			get {
				if (_Mesh is null) {
					_Mesh = Filter.mesh = new Mesh();
					_Mesh.MarkDynamic();
				}
				return _Mesh;
			}
		}

		// Data
		private SkinData _Data = null;
		private MeshFilter _Filter = null;
		private MeshRenderer _Renderer = null;
		private Mesh _Mesh = null;
		private Vector2 _Pivot = default;
		private Vector2 _Scale = Vector2.one;
		private Color32 _Tint = Color.white;
		private SkinType _TypeIndex = SkinType.Stage;
		private float _Disc = 0f;
		private float _DiscWidth = 1f;
		private float _DiscHeight = 1f;
		private float _DiscOffsetY = 0f;
		private float _LifeTime = 0f;

		// Cache
		private readonly static List<Vector3> Vertices = new List<Vector3>();
		private readonly static List<Vector2> UVs = new List<Vector2>();
		private readonly static List<Color> Colors = new List<Color>();
		private readonly static List<int> Triangles = new List<int>();
		private int Frame = 0;
		private bool MeshDirty = true;


		#endregion




		#region --- MSG ---


		private void LateUpdate () {

			// Dirty
			if (!MeshDirty) { return; }
			MeshDirty = false;

			// Data
			var data = AniData;
			if (data is null || SkinData.Texture is null || data.Rects.Count == 0) {
				ClearMeshCache();
				return;
			}
			ClearMeshCache();

			// Calculate New
			var rData = data.Rects[Frame];
			var scale = Scale * Mathf.Max(SkinData.ScaleMuti, 0f);
			if (scale.x < SCALE_GAP || scale.y < SCALE_GAP || rData.Width <= 0 || rData.Height <= 0) { return; }
			Color tint = Tint;
			float pivotX = Pivot.x;
			float pivotY = Pivot.y;
			float disc = Mathf.Clamp(Disc, 0f, 360f);
			float tWidth = SkinData.Texture.width;
			float tHeight = SkinData.Texture.height;
			bool disced = disc >= DISC_GAP;

			// Add Rect Mesh Cache
			if (rData.BorderL <= 0 && rData.BorderR <= 0 && rData.BorderD <= 0 && rData.BorderU <= 0) {
				// No Border
				AddQuad01(0f, 1f, 0f, 1f, rData.L / tWidth, rData.R / tWidth, rData.D / tHeight, rData.U / tHeight);
			} else {
				// Nine-Slice
				float _ornMinL = rData.BorderL > 0 ? (rData.BorderL / (float)(rData.BorderL + rData.BorderR)) : 0f;
				float _ornMinD = rData.BorderD > 0 ? (rData.BorderD / (float)(rData.BorderD + rData.BorderU)) : 0f;
				float _l = Mathf.Min((float)rData.BorderL / rData.Width / scale.x, _ornMinL);
				float _r = Mathf.Max(
					1f - ((float)rData.BorderR / rData.Width / scale.x),
					1f - (rData.BorderR > 0 ? (rData.BorderR / (float)(rData.BorderL + rData.BorderR)) : 0f)
				);
				float _d = Mathf.Min((float)rData.BorderD / rData.Height / scale.y, _ornMinD);
				float _u = Mathf.Max(
					1f - ((float)rData.BorderU / rData.Height / scale.y),
					1f - (rData.BorderU > 0 ? (rData.BorderU / (float)(rData.BorderD + rData.BorderU)) : 0f)
				);
				float _uvL0 = rData.L / tWidth;
				float _uvL = Util.Remap(0f, rData.BorderL / (rData.Width * _ornMinL), rData.L, rData.L + rData.BorderL, scale.x) / tWidth;
				float _uvR = Util.Remap(0f, rData.BorderL / (rData.Width * _ornMinL), rData.R, rData.R - rData.BorderR, scale.x) / tWidth;
				float _uvR1 = rData.R / tWidth;
				float _uvD0 = rData.D / tHeight;
				float _uvD = Util.Remap(0f, rData.BorderD / (rData.Height * _ornMinD), rData.D, rData.D + rData.BorderD, scale.y) / tHeight;
				float _uvU = Util.Remap(0f, rData.BorderD / (rData.Height * _ornMinD), rData.U, rData.U - rData.BorderU, scale.y) / tHeight;
				float _uvU1 = rData.U / tHeight;

				// Quad
				if (rData.BorderL > 0) {
					if (rData.BorderD > 0) {
						AddQuad01(0f, _l, 0f, _d, _uvL0, _uvL, _uvD0, _uvD);
					}
					if (rData.BorderU > 0) {
						AddQuad01(0f, _l, _u, 1f, _uvL0, _uvL, _uvU, _uvU1);
					}
					AddQuad01(0f, _l, _d, _u, _uvL0, _uvL, _uvD, _uvU);
				}
				if (rData.BorderD > 0) {
					AddQuad01(_l, _r, 0f, _d, _uvL, _uvR, _uvD0, _uvD);
				}
				if (rData.BorderU > 0) {
					AddQuad01(_l, _r, _u, 1f, _uvL, _uvR, _uvU, _uvU1);
				}
				AddQuad01(_l, _r, _d, _u, _uvL, _uvR, _uvD, _uvU);
				if (rData.BorderR > 0) {
					if (rData.BorderD > 0) {
						AddQuad01(_r, 1f, 0f, _d, _uvR, _uvR1, _uvD0, _uvD);
					}
					if (rData.BorderU > 0) {
						AddQuad01(_r, 1f, _u, 1f, _uvR, _uvR1, _uvU, _uvU1);
					}
					AddQuad01(_r, 1f, _d, _u, _uvR, _uvR1, _uvD, _uvU);
				}
			}

			// Final
			Mesh.SetVertices(Vertices);
			Mesh.SetColors(Colors);
			Mesh.SetUVs(0, UVs);
			Mesh.SetTriangles(Triangles, 0);
			Mesh.UploadMeshData(false);

			// Func
			void ClearMeshCache () {
				Mesh.Clear();
				Vertices.Clear();
				Colors.Clear();
				UVs.Clear();
				Triangles.Clear();
			}
			void AddQuad01 (float l, float r, float d, float u, float uvL, float uvR, float uvD, float uvU) {
				const float BASIC_STEP_ANGLE = 6f;
				int vIndex = Vertices.Count;
				if (disced) {
					// Rect
					int step = Mathf.RoundToInt((r - l) * Mathf.Max(disc / BASIC_STEP_ANGLE, 12f)) + 1;
					float x = 0f;
					Vector2 a, b;
					for (int i = 0; i <= step; i++, x += 1f / step) {
						// Vert
						a = Util.GetDisc01(new Vector2(Mathf.Lerp(l, r, x), d),
							disc, DiscWidth, DiscHeight, DiscOffsetY
						);
						b = Util.GetDisc01(new Vector2(Mathf.Lerp(l, r, x), u),
							disc, DiscWidth, DiscHeight, DiscOffsetY
						);
						a.x -= pivotX;
						b.x -= pivotX;
						a.y -= pivotY;
						b.y -= pivotY;
						Vertices.Add(a);
						Vertices.Add(b);
						// Tri
						if (i < step) {
							if (x > 0.5f) {
								Triangles.Add(vIndex + i * 2 + 0);
								Triangles.Add(vIndex + i * 2 + 1);
								Triangles.Add(vIndex + i * 2 + 2);
								Triangles.Add(vIndex + i * 2 + 2);
								Triangles.Add(vIndex + i * 2 + 1);
								Triangles.Add(vIndex + i * 2 + 3);
							} else {
								Triangles.Add(vIndex + i * 2 + 0);
								Triangles.Add(vIndex + i * 2 + 1);
								Triangles.Add(vIndex + i * 2 + 3);
								Triangles.Add(vIndex + i * 2 + 0);
								Triangles.Add(vIndex + i * 2 + 3);
								Triangles.Add(vIndex + i * 2 + 2);
							}
						}
						// UV
						UVs.Add(new Vector2(Mathf.Lerp(uvL, uvR, x), uvD));
						UVs.Add(new Vector2(Mathf.Lerp(uvL, uvR, x), uvU));
						// Color
						Colors.Add(tint);
						Colors.Add(tint);
					}
				} else {
					// Rect
					Vertices.Add(new Vector3(l - pivotX, d - pivotY, 0f));
					Vertices.Add(new Vector3(l - pivotX, u - pivotY, 0f));
					Vertices.Add(new Vector3(r - pivotX, u - pivotY, 0f));
					Vertices.Add(new Vector3(r - pivotX, d - pivotY, 0f));
					// UV
					UVs.Add(new Vector2(uvL, uvD));
					UVs.Add(new Vector2(uvL, uvU));
					UVs.Add(new Vector2(uvR, uvU));
					UVs.Add(new Vector2(uvR, uvD));
					// Tri
					Triangles.Add(vIndex + 0);
					Triangles.Add(vIndex + 1);
					Triangles.Add(vIndex + 2);
					Triangles.Add(vIndex + 0);
					Triangles.Add(vIndex + 2);
					Triangles.Add(vIndex + 3);
					// Color
					Colors.Add(tint);
					Colors.Add(tint);
					Colors.Add(tint);
					Colors.Add(tint);
				}
			}
		}


		#endregion




		#region --- API ---


		public void SetSortingLayer (int layerID, int layerOrder) {
			Renderer.sortingLayerID = layerID;
			Renderer.sortingOrder = layerOrder;
		}


		public void SetDirty () => MeshDirty = true;


		#endregion




		#region --- LGC ---


		private void ReCalculateFrame () {
			var ani = AniData;
			int frame = ani is null ? 0 : ani.GetFrame(_LifeTime);
			if (frame != Frame) {
				Frame = frame;
				SetDirty();
			}
		}


		#endregion




	}
}