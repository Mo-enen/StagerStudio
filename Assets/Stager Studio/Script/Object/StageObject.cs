namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Rendering;



	public abstract class StageObject : MonoBehaviour {




		#region --- SUB ---


		public delegate float FloatFloatIntHandler (float time, int index);
		public delegate Color ColorIntHandler (int index);
		public delegate bool BoolHandler ();


		#endregion




		#region --- VAR ---


		// Const
		protected const float DURATION_GAP = 0.0001f;
		protected readonly static Color32 WHITE_32 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		// Handler
		public static FloatFloatIntHandler TweenEvaluate { get; set; } = null;
		public static ColorIntHandler PaletteColor { get; set; } = null;

		// API
		public static (Vector3 min, Vector3 max, float size, float ratio) ZoneMinMax { get; set; } = (default, default, 0f, 1f);
		public static (Vector2 min, Vector3 max) ScreenZoneMinMax { get; set; } = (default, default);
		public static Beatmap Beatmap { get; set; } = null;
		public static (float index, float value, float width) Abreast { get; set; } = (0f, 0f, 1f);
		public static float MusicTime { get; set; } = 0f;
		public static float MusicDuration { get; set; } = 0f;
		public static float GameSpeedMuti { get; set; } = 1f;
		public static bool MusicPlaying { get; set; } = false;
		public static bool ShowIndexLabel { get; set; } = true;
		public static bool ShowGrid { get; set; } = true;
		public static int MaterialZoneID { get; set; } = 0;
		public static int SortingLayerID_Gizmos { get; set; } = -1;
		public static bool TintNote { get; set; } = false;

		protected static float VanishDuration { get; private set; } = 0f;
		protected static Color32[] HighlightTints { get; set; } = default;
		protected ObjectRenderer MainRenderer => m_MainRenderer;
		protected TextRenderer Label => m_Label;
		protected SpriteRenderer Highlight { get; private set; } = null;
		protected virtual float Time { get; set; } = 0f;
		protected virtual float Duration { get; set; } = 0f;
		protected Vector2? ColSize { get; set; } = null;
		protected Quaternion? ColRot { get; set; } = null;

		// Ser
		[SerializeField] private ObjectRenderer m_MainRenderer = null;
		[SerializeField] private BoxCollider m_Col = null;
		[SerializeField] private TextRenderer m_Label = null;

		// Data
		protected static SkinData Skin = null;
		private static (Vector3 size, bool fixedRatio)[] RectSizes = default;
		private Quaternion PrevColRot = Quaternion.identity;
		private Vector2 PrevColSize = Vector3.zero;


		#endregion


		protected virtual void Awake () {
			if (m_Label != null) {
				m_Label.SetSortingLayer(SortingLayerID_Gizmos, 0);
			}
			MainRenderer.SkinData = Skin;
			VanishDuration = Skin != null ? Skin.VanishDuration : 0f;
		}


		protected virtual void LateUpdate () {
			LateUpdate_Col_Highlight();
			LateUpdate_Renderer();
		}


		private void LateUpdate_Col_Highlight () {
			if (m_Col == null) { return; }
			if (m_Col.gameObject.activeSelf != ColSize.HasValue) {
				m_Col.gameObject.SetActive(ColSize.HasValue);
			}
			if (MusicPlaying) { return; }
			// Collider
			if (ColSize.HasValue && (PrevColSize != ColSize.Value || (ColRot.HasValue && ColRot.Value != PrevColRot))) {
				var size = new Vector3(ColSize.Value.x, ColSize.Value.y, 0.01f);
				var offset = new Vector3(0f, ColSize.Value.y * 0.5f, 0f);
				PrevColSize = m_Col.size = size;
				m_Col.center = offset;
				if (ColRot.HasValue) {
					PrevColRot = m_Col.transform.rotation = ColRot.Value;
				}
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


		protected static Color EvaluateColor (List<Beatmap.TimeIntTween> data, float lifeTime, Color defaultColor) {
			if (data is null || data.Count == 0 || lifeTime < data[0].Time) {
				return defaultColor;
			} else if (data.Count == 1) {
				return PaletteColor(data[0].Value);
			} else {
				int index = Beatmap.TimeIntTween.Search(data, lifeTime);
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
				Screen.height - ScreenZoneMinMax.max.y,
				ScreenZoneMinMax.max.x,
				Screen.height - ScreenZoneMinMax.min.y
			));


		public static void LoadSkin (SkinData skin) {
			Skin = skin;
			int typeCount = System.Enum.GetNames(typeof(SkinType)).Length;
			RectSizes = new (Vector3, bool)[typeCount];
			HighlightTints = new Color32[typeCount];
			TintNote = skin.TintNote;
			if (skin == null) { return; }
			for (int i = 0; i < RectSizes.Length; i++) {
				// Sizes
				var size = skin.TryGetItemSize(i) / skin.ScaleMuti;
				size.x = Mathf.Max(size.x, 0f);
				size.y = Mathf.Max(size.y, 0.001f);
				size.z = Mathf.Max(size.z, 0f);
				RectSizes[i] = (size, skin.Items[i].FixedRatio);
				// Highlights
				HighlightTints[i] = skin.Items[i].HighlightTint;
			}

		}


		protected static Vector3 GetRectSize (SkinType type, bool scalableX = true, bool scalableY = false) {
			var (size, fix) = RectSizes[(int)type];
			if (!fix) {
				if (scalableX) {
					size.x = -1f;
				}
				if (scalableY) {
					size.y = -1f;
				}
			}
			return size;
		}


		protected int GetSortingOrder () => (int)Mathf.Lerp(32760, -32760, Time / MusicDuration);


		protected virtual void RefreshRendererZone () => RefreshRendererZoneFor(m_MainRenderer);


		protected void TrySetColliderLayer (int layer) {
			if (m_Col.gameObject.layer != layer) {
				m_Col.gameObject.layer = layer;
			}
		}


		#endregion




	}
}