namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Data;


	public class TimingPreviewUI : Image {


		#region --- SUB ---


		public delegate Beatmap BeatmapHandler ();
		public delegate float FloatHandler ();
		public delegate bool BoolHandler ();


		#endregion




		#region --- VAR ---


		// Handler
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static FloatHandler GetMusicTime { get; set; } = null;
		public static FloatHandler GetSpeedMuti { get; set; } = null;
		public static BoolHandler ShowPreview { get; set; } = null;

		// Ser
		[SerializeField] private Color m_NegativeTint = default;

		// Data
		private readonly static UIVertex[] CacheVertex = { default, default, default, default, };


		#endregion



		#region --- MSG ---


		protected override void OnPopulateMesh (VertexHelper toFill) {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) {
				toFill.Clear();
				return;
			}
#endif
			toFill.Clear();
			if (!ShowPreview()) { return; }
			var map = GetBeatmap();
			if (map == null || map.Timings == null || map.Timings.Count == 0) { return; }
			float speedMuti = GetSpeedMuti();
			if (speedMuti <= 0.001f) { return; }
			var timings = map.Timings;
			int count = timings.Count;
			float musicTime = GetMusicTime();
			float endTime = musicTime + 1f / speedMuti;
			float firstMidTime = endTime;
			var rect = GetPixelAdjustedRect();
			CacheVertex[0].position.z = CacheVertex[1].position.z = CacheVertex[2].position.z = CacheVertex[3].position.z = 0f;

			// Mid
			for (int i = 0; i < count; i++) {
				var timing = timings[i];
				if (timing.Time >= musicTime && timing.Time < endTime) {
					firstMidTime = Mathf.Min(firstMidTime, timing.Time);
					int nextIndex = GetNextTiming(timing.Time);
					if (nextIndex >= 0) {
						AddQuad(timing.Time, timings[nextIndex].Time, timing.Speed);
					} else {
						AddQuad(timing.Time, float.MaxValue, timing.Speed);
					}
				}
			}

			// First
			int prevIndex = GetPrevTiming(musicTime);
			if (prevIndex >= 0) {
				var timing = timings[prevIndex];
				AddQuad(timing.Time, firstMidTime, timing.Speed);
			} else {
				int nextIndex = GetNextTiming(musicTime);
				float nextSpeed = 1f;
				if (nextIndex >= 0) {
					nextSpeed = timings[nextIndex].Speed;
				}
				AddQuad(0f, firstMidTime, nextSpeed);
			}

			// === Func ===
			int GetNextTiming (float time) {
				int index = -1;
				float nextTime = float.MaxValue;
				for (int i = 0; i < count; i++) {
					var t = timings[i];
					if (t.Time > time && t.Time < nextTime) {
						nextTime = t.Time;
						index = i;
					}
				}
				return index;
			}
			int GetPrevTiming (float time) {
				int index = -1;
				float prevTime = float.MinValue;
				for (int i = 0; i < count; i++) {
					var t = timings[i];
					if (t.Time < time && t.Time > prevTime) {
						prevTime = t.Time;
						index = i;
					}
				}
				return index;
			}
			void AddQuad (float time, float nextTime, float speed) {
				float y01 = Mathf.Clamp01(Util.RemapUnclamped(musicTime, endTime, 0f, 1f, time));
				float nextY01 = Mathf.Clamp01(Util.RemapUnclamped(musicTime, endTime, 0f, 1f, nextTime));
				// Y
				CacheVertex[0].position.y = CacheVertex[3].position.y = Mathf.Lerp(rect.yMin, rect.yMax, y01);
				CacheVertex[1].position.y = CacheVertex[2].position.y = Mathf.Lerp(rect.yMin, rect.yMax, nextY01);
				// X
				float x11 = Speed_to_UI(Mathf.Abs(speed));
				CacheVertex[0].position.x = CacheVertex[1].position.x = Mathf.LerpUnclamped(
					rect.xMin, rect.xMax, 0f
				);
				CacheVertex[2].position.x = CacheVertex[3].position.x = Mathf.LerpUnclamped(
					rect.xMin, rect.xMax, x11
				);
				// UV
				const float UV_SCALE = 18f;
				CacheVertex[0].uv0 = new Vector2(0f, y01 * rect.height / UV_SCALE);
				CacheVertex[1].uv0 = new Vector2(0f, nextY01 * rect.height / UV_SCALE);
				CacheVertex[2].uv0 = new Vector2(x11 * rect.width / UV_SCALE, nextY01 * rect.height / UV_SCALE);
				CacheVertex[3].uv0 = new Vector2(x11 * rect.width / UV_SCALE, y01 * rect.height / UV_SCALE);
				// Tint
				CacheVertex[0].color = CacheVertex[1].color = CacheVertex[2].color = CacheVertex[3].color = (speed > 0f ? color : m_NegativeTint);
				// Add
				toFill.AddUIVertexQuad(CacheVertex);
			}
			float Speed_to_UI (float speed) {
				if (speed < 2f) {
					return speed;
				} else if (speed < 16f) {
					return Util.Remap(2f, 16f, 2f, 3f, speed);
				} else {
					return 3f;
				}
			}
		}


		#endregion


	}
}



#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEditor;
	using UI;
	[CustomEditor(typeof(TimingPreviewUI))]
	public class SpeedPreviewUIInspector : Editor {
		private readonly static string[] Exclude = new string[] {
			"m_Script","m_OnCullStateChanged","m_Type","m_PreserveAspect",
			"m_FillCenter","m_FillMethod","m_FillAmount","m_FillClockwise","m_FillOrigin","m_UseSpriteMesh",
			"m_PixelsPerUnitMultiplier",
		};
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, Exclude);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif