namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public abstract class ItemRenderer : MonoBehaviour {


		// Const
		private const float SCALE_GAP = 0.0001f;

		// API
		public Vector2 Scale {
			get => m_Scale;
			set {
				if (Mathf.Abs(value.x - m_Scale.x) > 0.0001f || Mathf.Abs(value.y - m_Scale.y) > 0.0001f) {
					m_Scale = value;
					SetDirty();
				}
			}
		}
		public Vector3 Pivot {
			get => m_Pivot;
			set {
				if (Mathf.Abs(value.x - m_Pivot.x) > 0.0001f || Mathf.Abs(value.y - m_Pivot.y) > 0.0001f) {
					m_Pivot = value;
					SetDirty();
				}
			}
		}
		public Color32 Tint {
			get => m_Tint;
			set {
				if (value.r != m_Tint.r || value.g != m_Tint.g || value.b != m_Tint.b || value.a != m_Tint.a) {
					m_Tint = value;
					SetDirty();
				}
			}
		}
		public float Alpha {
			get => m_Tint.a / (float)byte.MaxValue;
			set {
				byte a = (byte)Mathf.RoundToInt(value * byte.MaxValue);
				if (a != m_Tint.a) {
					m_Tint.a = a;
					SetDirty();
				}
			}
		}
		public bool RendererEnable {
			get => Renderer.enabled;
			set {
				if (value != Renderer.enabled) {
					Renderer.enabled = value;
				}
			}
		}
		public MeshRenderer Renderer => _Renderer != null ? _Renderer : (_Renderer = GetComponent<MeshRenderer>());

		// Short
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

		// Ser
		[SerializeField] private Color32 m_Tint = Color.white;
		[SerializeField] private Vector3 m_Pivot = new Vector3(0.5f, 0f, 0f);
		[SerializeField] private Vector2 m_Scale = Vector2.one;

		// Data
		private MeshFilter _Filter = null;
		private MeshRenderer _Renderer = null;
		private Mesh _Mesh = null;

		// Cache
		private readonly static List<Vector3> Vertices = new List<Vector3>();
		private readonly static List<Vector2> UVs = new List<Vector2>();
		private readonly static List<Color> Colors = new List<Color>();
		private readonly static List<int> Triangles = new List<int>();
		private bool MeshDirty = true;


		// MSG
		protected virtual void LateUpdate () {
			if (!MeshDirty) { return; }
			MeshDirty = false;
			RefreshMesh();
		}


		protected abstract void OnMeshFill ();


		// API
		public void SetSortingLayer (int layerID, int layerOrder) {
			if (layerID != Renderer.sortingLayerID) {
				Renderer.sortingLayerID = layerID;
			}
			if (layerOrder != Renderer.sortingOrder) {
				Renderer.sortingOrder = layerOrder;
			}
		}


		public void SetDirty () => MeshDirty = true;


		protected void AddNineCliceQuad (
			bool hasBorderL, bool hasBorderR, bool hasBorderD, bool hasBorderU,
			float _l, float _r, float _d, float _u,
			float _uvL, float _uvR, float _uvD, float _uvU,
			float _uvL0, float _uvR1, float _uvD0, float _uvU1,
			Vector3 offset, Color tint, bool normalUV = true, bool normalTri = true
		) {
			if (hasBorderL) {
				if (hasBorderD) {
					// DL
					AddQuad01(0f, _l, 0f, _d, _uvL0, _uvL, _uvD0, _uvD, offset, tint, normalUV, normalTri);
				}
				if (hasBorderU) {
					// UL
					AddQuad01(0f, _l, _u, 1f, _uvL0, _uvL, _uvU, _uvU1, offset, tint, normalUV, normalTri);
				}
				// L Scale
				AddQuad01(0f, _l, _d, _u, _uvL0, _uvL, _uvD, _uvU, offset, tint, normalUV, normalTri);
			}
			if (hasBorderD) {
				// DM
				AddQuad01(_l, _r, 0f, _d, _uvL, _uvR, _uvD0, _uvD, offset, tint, normalUV, normalTri);
			}
			if (hasBorderU) {
				// UM
				AddQuad01(_l, _r, _u, 1f, _uvL, _uvR, _uvU, _uvU1, offset, tint, normalUV, normalTri);
			}
			// MM
			AddQuad01(_l, _r, _d, _u, _uvL, _uvR, _uvD, _uvU, offset, tint, normalUV, normalTri);
			if (hasBorderR) {
				if (hasBorderD) {
					// DR
					AddQuad01(_r, 1f, 0f, _d, _uvR, _uvR1, _uvD0, _uvD, offset, tint, normalUV, normalTri);
				}
				if (hasBorderU) {
					// UR
					AddQuad01(_r, 1f, _u, 1f, _uvR, _uvR1, _uvU, _uvU1, offset, tint, normalUV, normalTri);
				}
				// R
				AddQuad01(_r, 1f, _d, _u, _uvR, _uvR1, _uvD, _uvU, offset, tint, normalUV, normalTri);
			}
		}


		protected void AddQuad01 (float l, float r, float d, float u, float uvL, float uvR, float uvD, float uvU, Vector3 offset, Color tint, bool normalUV = true, bool normalTri = true) => AddQuad01(l, r, d, u, uvL, uvR, uvD, uvU, 0, 1, offset, tint, normalUV, normalTri);


		protected void AddQuad01 (float a0, float a1, float b0, float b1, float uvL, float uvR, float uvD, float uvU, int axisA, int axisB, Vector3 offset, Color tint, bool normalUV = true, bool normalTri = true) {
			int vIndex = Vertices.Count;
			// Rect
			Vector3 v = Vector3.zero;
			v[axisA] = a0 - Pivot[axisA];
			v[axisB] = b0 - Pivot[axisB];
			Vertices.Add(v + offset);
			v[axisB] = b1 - Pivot[axisB];
			Vertices.Add(v + offset);
			v[axisA] = a1 - Pivot[axisA];
			Vertices.Add(v + offset);
			v[axisB] = b0 - Pivot[axisB];
			Vertices.Add(v + offset);
			// UV
			if (normalUV) {
				UVs.Add(new Vector2(uvL, uvD));
				UVs.Add(new Vector2(uvL, uvU));
				UVs.Add(new Vector2(uvR, uvU));
				UVs.Add(new Vector2(uvR, uvD));
			} else {
				UVs.Add(new Vector2(uvR, uvD));
				UVs.Add(new Vector2(uvL, uvD));
				UVs.Add(new Vector2(uvL, uvU));
				UVs.Add(new Vector2(uvR, uvU));
			}
			// Tri
			if (normalTri) {
				Triangles.Add(vIndex + 0);
				Triangles.Add(vIndex + 1);
				Triangles.Add(vIndex + 2);
				Triangles.Add(vIndex + 0);
				Triangles.Add(vIndex + 2);
				Triangles.Add(vIndex + 3);
			} else {
				Triangles.Add(vIndex + 0);
				Triangles.Add(vIndex + 2);
				Triangles.Add(vIndex + 1);
				Triangles.Add(vIndex + 0);
				Triangles.Add(vIndex + 3);
				Triangles.Add(vIndex + 2);
			}
			// Color
			Colors.Add(tint);
			Colors.Add(tint);
			Colors.Add(tint);
			Colors.Add(tint);
		}


		protected void ClearMeshCache () {
			Mesh.Clear();
			Vertices.Clear();
			Colors.Clear();
			UVs.Clear();
			Triangles.Clear();
		}


		protected void RefreshMesh () {
			ClearMeshCache();
			if (Scale.x < SCALE_GAP || Scale.y < SCALE_GAP) { return; }
			OnMeshFill();
			Mesh.SetVertices(Vertices);
			Mesh.SetColors(Colors);
			Mesh.SetUVs(0, UVs);
			Mesh.SetTriangles(Triangles, 0);
			Mesh.UploadMeshData(false);
		}


	}
}