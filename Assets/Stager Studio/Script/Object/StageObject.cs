namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;



	public abstract class StageObject : MonoBehaviour {




		#region --- SUB ---


		public delegate float FloatFloatIntHandler (float time, int index);
		public delegate Color ColorByteHandler (byte index);
		public delegate float SpeedMutiHandler ();


		#endregion




		#region --- VAR ---


		// Const
		protected const float DURATION_GAP = 0.0001f;

		// Handler
		public static SpeedMutiHandler GetGameSpeedMuti { get; set; } = null;
		public static FloatFloatIntHandler TweenEvaluate { get; set; } = null;
		public static ColorByteHandler PaletteColor { get; set; } = null;

		// API
		public static (Vector3 min, Vector3 max, float size, float ratio) ZoneMinMax { get; set; } = (default, default, 0f, 1f);
		public static (Vector2 min, Vector3 max) ScreenZoneMinMax { get; set; } = (default, default);
		public static Beatmap Beatmap { get; set; } = null;
		public static (int index, bool active, bool all) Abreast { get; set; } = (0, false, false);
		public static int LayerID_UI { get; set; } = -1;
		public static float MusicTime { get; set; } = 0f;
		public static float MusicDuration { get; set; } = 0f;
		public static bool MusicPlaying { get; set; } = false;
		public static bool ShowIndexLabel { get; set; } = true;
		public static int MaterialZoneID { get; set; } = 0;
		protected static float VanishDuration { get; private set; } = 0f;
		protected ObjectRenderer MainRenderer => m_MainRenderer;
		protected TextMesh Label => m_Label;
		protected SpriteRenderer Highlight => m_Highlight;
		protected virtual float Time { get; set; } = 0f;
		protected virtual float Duration { get; set; } = 0f;
		protected Vector2? ColSize { get; set; } = null;
		protected Quaternion? ColRot { get; set; } = null;

		// Ser
		[SerializeField] private ObjectRenderer m_MainRenderer = null;
		[SerializeField] private BoxCollider m_Col = null;
		[SerializeField] private TextMesh m_Label = null;
		[SerializeField] private SpriteRenderer m_Highlight = null;

		// Data
		private Quaternion PrevColRot = Quaternion.identity;
		private Vector2 PrevColSize = Vector3.zero;
		private float HighlightScaleMuti = 0f;


		#endregion


		protected virtual void Awake () {
			if (m_Label != null) {
				m_Label.GetComponent<MeshRenderer>().sortingLayerID = LayerID_UI;
			}
			HighlightScaleMuti = m_Highlight != null ? 1f / m_Highlight.transform.localScale.x : 0f;
		}


		protected virtual void LateUpdate () {
			LateUpdate_Col_Highlight();
			LateUpdate_Renderer();
		}


		private void LateUpdate_Col_Highlight () {
			if (m_Col == null) { return; }
			if (m_Col.enabled != ColSize.HasValue) {
				m_Col.enabled = ColSize.HasValue;
			}
			if (ColSize.HasValue && (PrevColSize != ColSize.Value || (ColRot.HasValue && ColRot.Value != PrevColRot))) {
				var size = new Vector3(ColSize.Value.x, ColSize.Value.y, 0.01f);
				var offset = new Vector3(0f, ColSize.Value.y * 0.5f, 0f);
				PrevColSize = m_Col.size = size;
				m_Col.center = offset;
				m_Highlight.transform.localPosition = offset;

				if (ColRot.HasValue) {
					PrevColRot = m_Col.transform.rotation = ColRot.Value;
				}
			}
			if (m_Highlight.enabled) {
				m_Highlight.size = Vector2.Max(ColSize.Value * HighlightScaleMuti, Vector2.one * 0.36f) + Vector2.one * Mathf.PingPong(UnityEngine.Time.time / 6f, 0.1f);
			}
		}


		private void LateUpdate_Renderer () {
			RefreshRendererZone();
		}


		#region --- API ---


		protected static float Evaluate (List<Beatmap.TimeFloatTween> data, float lifeTime, float defaultValue = 0f) {
			if (data is null || data.Count == 0 || lifeTime < data[0].Time) {
				return defaultValue;
			} else if (data.Count == 1) {
				return data[0].Value;
			} else {
				int index = Beatmap.TimeFloatTween.Search(data, lifeTime);
				if (index < data.Count - 1) {
					var l = data[index];
					var r = data[index + 1];
					return Mathf.LerpUnclamped(
						l.Value,
						r.Value,
						TweenEvaluate(
							Util.RemapUnclamped(l.Time, r.Time, 0f, 1f, lifeTime),
							l.Tween
						)
					);
				} else {
					return data[data.Count - 1].Value;
				}
			}
		}


		protected static Vector2 Evaluate (List<Beatmap.TimeFloatFloatTween> data, float lifeTime) {
			if (data is null || data.Count == 0 || lifeTime < data[0].Time) {
				return Vector2.zero;
			} else if (data.Count == 1) {
				var d = data[0];
				return new Vector2(d.A, d.B);
			} else {
				int index = Beatmap.TimeFloatFloatTween.Search(data, lifeTime);
				if (index < data.Count - 1) {
					var l = data[index];
					var r = data[index + 1];
					return Vector2.LerpUnclamped(
						new Vector2(l.A, l.B),
						new Vector2(r.A, r.B),
						TweenEvaluate(
							Util.RemapUnclamped(l.Time, r.Time, 0f, 1f, lifeTime),
							l.Tween
						)
					);
				} else {
					var d = data[data.Count - 1];
					return new Vector2(d.A, d.B);
				}
			}
		}


		protected static Color EvaluateColor (List<Beatmap.TimeByteTween> data, float lifeTime) {
			if (data is null || data.Count == 0 || lifeTime < data[0].Time) {
				return Color.white;
			} else if (data.Count == 1) {
				return PaletteColor(data[0].Value);
			} else {
				int index = Beatmap.TimeByteTween.Search(data, lifeTime);
				if (index < data.Count - 1) {
					var l = data[index];
					var r = data[index + 1];
					return Color.LerpUnclamped(
						PaletteColor(l.Value),
						PaletteColor(r.Value),
						TweenEvaluate(
							Util.RemapUnclamped(l.Time, r.Time, 0f, 1f, lifeTime),
							l.Tween
						)
					);
				} else {
					return PaletteColor(data[data.Count - 1].Value);
				}
			}
		}


		protected static void RefreshRendererZoneFor (StageRenderer renderer) =>
			renderer.Renderer.material.SetVector(MaterialZoneID, new Vector4(
				ScreenZoneMinMax.min.x,
				ScreenZoneMinMax.min.y,
				ScreenZoneMinMax.max.x,
				ScreenZoneMinMax.max.y
			));


		protected int GetSortingOrder () => (int)Mathf.Lerp(-32760, 32760, Time / MusicDuration);


		public virtual void SetSkinData (SkinData skin) {
			MainRenderer.SkinData = skin;
			VanishDuration = skin != null ? skin.VanishDuration : 0f;
		}


		protected virtual void RefreshRendererZone () => RefreshRendererZoneFor(m_MainRenderer);


		protected void TrySetColliderLayer (int layer) {
			if (m_Col.gameObject.layer != layer) {
				m_Col.gameObject.layer = layer;
			}
		}



		#endregion




	}
}