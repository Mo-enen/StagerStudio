namespace StagerStudio.Object {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;


	public abstract class ObjectTimer : MonoBehaviour {




		#region --- VAR ---


		// Api
		public static Beatmap Beatmap { get; set; } = null;
		public static (Vector3 min, Vector3 max, float size, float ratio) ZoneMinMax { get; set; } = (default, default, 0f, 1f);
		public static float MusicTime { get; set; } = 0f;
		public static float SpeedMuti { get; set; } = 1f;
		public static bool MusicPlaying { get; set; } = false;

		// Short
		protected Transform Head => m_Head;
		protected Transform Tail => m_Tail;
		protected Transform Line => m_Line;

		// Ser
		[SerializeField] private Transform m_Head = null;
		[SerializeField] private Transform m_Tail = null;
		[SerializeField] private Transform m_Line = null;
		[SerializeField] private SpriteRenderer m_LineRenderer = null;

		// Data
		protected float LateTime = 0f;
		protected float LateDuration = 0f;
		protected float LateScaleY = 0f;


		#endregion



		protected void LateUpdate () {

			// Head
			bool headActive = GetTimerActive(LateTime);
			if (Head.gameObject.activeSelf != headActive) {
				Head.gameObject.SetActive(headActive);
			}
			if (headActive) {
				Head.localPosition = new Vector3(
					0f, Util.Remap(LateTime, LateTime - 1f, 0f, 1f, MusicTime) * LateScaleY, 0f
				);
			}

			// Tail
			float endTime = LateTime + LateDuration;
			bool tailActive = GetTimerActive(endTime);
			if (Tail.gameObject.activeSelf != tailActive) {
				Tail.gameObject.SetActive(tailActive);
			}
			if (tailActive) {
				Tail.localPosition = new Vector3(
					0f, Util.Remap(endTime, endTime - 1f, 0f, 1f, MusicTime) * LateScaleY, 0f
				);
			}

			// Line
			bool lineActive = headActive || tailActive;
			if (Line.gameObject.activeSelf != lineActive) {
				Line.gameObject.SetActive(lineActive);
			}
			if (m_LineRenderer.gameObject.activeSelf != lineActive) {
				m_LineRenderer.gameObject.SetActive(lineActive);
			}
			if (lineActive) {
				Line.localScale = new Vector3(Line.localScale.x, LateScaleY, 1f);
				var size = m_LineRenderer.size;
				size.y = LateScaleY / m_LineRenderer.transform.localScale.y;
				m_LineRenderer.size = size;
			}
		}


		// API
		public static bool GetTimerActive (float time) => MusicTime >= time - 1f && MusicTime <= time;


		


	}
}