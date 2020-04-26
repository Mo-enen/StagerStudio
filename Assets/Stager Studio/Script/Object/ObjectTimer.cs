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
		protected SpriteRenderer HeadRenderer => m_HeadRenderer;
		protected SpriteRenderer TailRenderer => m_TailRenderer;
		protected SpriteRenderer LineRenderer => m_LineRenderer;

		// Ser
		[SerializeField] private SpriteRenderer m_HeadRenderer = null;
		[SerializeField] private SpriteRenderer m_TailRenderer = null;
		[SerializeField] private SpriteRenderer m_LineRenderer = null;

		// Data
		protected float Sublength = 0f;


		#endregion




		#region --- MSG ---


		private void Awake () {
			Sublength = m_LineRenderer.size.y * m_LineRenderer.transform.localScale.y;
		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---




		#endregion




	}
}