namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public abstract class StageObject : MonoBehaviour {




		#region --- SUB ---


		public delegate (Vector3 min, Vector3 max, float size) Vector3MinMaxSizeHandler ();
		public delegate float FloatHandler ();
		public delegate float FloatFloatIntHandler (float time, int index);
		public delegate Color ColorByteHandler (byte index);
		public delegate Beatmap BeatmapHandler ();


		#endregion




		#region --- VAR ---


		// Const
		protected const float DISC_GAP = 0.001f;
		protected const float DURATION_GAP = 0.0001f;

		// Handler
		public static Vector3MinMaxSizeHandler GetZoneMinMax { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatFloatIntHandler TweenEvaluate { get; set; } = null;
		public static ColorByteHandler PaletteColor { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;

		// API
		public StageRenderer MainRenderer => m_MainRenderer;
		public virtual float Time { get; protected set; } = 0f;
		public virtual float Duration { get; protected set; } = 0f;

		// Ser
		[SerializeField] private StageRenderer m_MainRenderer = null;


		#endregion




		#region --- API ---


		protected static float Evaluate (List<Beatmap.TimeFloatTween> data, float lifeTime) {
			if (data is null || data.Count == 0) {
				return 0;
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
							Util.Remap(l.Time, r.Time, 0f, 1f, lifeTime),
							l.Tween
						)
					);
				} else {
					return data[data.Count - 1].Value;
				}
			}
		}


		protected static Vector2 Evaluate (List<Beatmap.TimeFloatFloatTween> data, float lifeTime) {
			if (data is null || data.Count == 0) {
				return default;
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
							Util.Remap(l.Time, r.Time, 0f, 1f, lifeTime),
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
			if (data is null || data.Count == 0) {
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
							Util.Remap(l.Time, r.Time, 0f, 1f, lifeTime),
							l.Tween
						)
					);
				} else {
					return PaletteColor(data[data.Count - 1].Value);
				}
			}
		}


		public virtual void SetSkinData (SkinData skin, int layerID, int orderID) {
			MainRenderer.SkinData = skin;
			MainRenderer.SetSortingLayer(layerID, orderID);
		}


		#endregion




	}
}