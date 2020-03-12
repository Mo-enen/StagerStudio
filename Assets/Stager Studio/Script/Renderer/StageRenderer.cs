namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public abstract class StageRenderer : MonoBehaviour {


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
		public Vector2 Pivot {
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
		[SerializeField] private Vector2 m_Pivot = new Vector2(0.5f, 0f);
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
		private void LateUpdate () {
			if (!MeshDirty) { return; }
			MeshDirty = false;
			ClearMeshCache();
			if (Scale.x < SCALE_GAP || Scale.y < SCALE_GAP) { return; }
			OnMeshFill();
			Mesh.SetVertices(Vertices);
			Mesh.SetColors(Colors);
			Mesh.SetUVs(0, UVs);
			Mesh.SetTriangles(Triangles, 0);
			Mesh.UploadMeshData(false);
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


		protected void AddQuad01 (float l, float r, float d, float u, float uvL, float uvR, float uvD, float uvU) {
			int vIndex = Vertices.Count;
			// Rect
			Vertices.Add(new Vector3(l - Pivot.x, d - Pivot.y, 0f));
			Vertices.Add(new Vector3(l - Pivot.x, u - Pivot.y, 0f));
			Vertices.Add(new Vector3(r - Pivot.x, u - Pivot.y, 0f));
			Vertices.Add(new Vector3(r - Pivot.x, d - Pivot.y, 0f));
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
			Colors.Add(Tint);
			Colors.Add(Tint);
			Colors.Add(Tint);
			Colors.Add(Tint);
		}


		protected void ClearMeshCache () {
			Mesh.Clear();
			Vertices.Clear();
			Colors.Clear();
			UVs.Clear();
			Triangles.Clear();
		}


	}
}