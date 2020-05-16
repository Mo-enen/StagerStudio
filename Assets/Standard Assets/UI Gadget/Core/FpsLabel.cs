namespace UIGadget {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class FpsLabel : Text {



		// Ser
		[SerializeField] private float m_Gap = 0.618f;
		[SerializeField] private int m_Max = 999;

		// Data
		private float LastFpsTime = 0f;
		private float DeltaTime = 0f;
		private float DeltaCount = 0f;


		protected override void Start () {
			base.Start();
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			LastFpsTime = Time.time;
		}


		private void Update () {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) { return; }
#endif
			if (Time.time > LastFpsTime + m_Gap && DeltaCount > 0) {
				int fps = (int)(1f / (DeltaTime / DeltaCount));
				text = fps > m_Max ? (m_Max + "+") : fps.ToString();
				DeltaTime = 0f;
				DeltaCount = 0f;
				LastFpsTime = Time.time;
			} else {
				DeltaTime += Time.deltaTime;
				DeltaCount++;
			}
		}



	}
}