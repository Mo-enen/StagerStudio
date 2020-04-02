namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class VanishTextRenderer : TextRenderer {




		//Ser
		[SerializeField] private float m_Duration = 1f;
		[SerializeField] private Vector3 m_Speed = default;

		// Data
		private Color NormalColor = Color.white;
		private Color ClearColor = Color.clear;
		private float StartTime = 0f;


		private void Start () {
			NormalColor = ClearColor = Tint;
			ClearColor.a = 0f;
			StartTime = Time.time;
		}


		private void Update () {
			Tint = Color.Lerp(NormalColor, ClearColor, Mathf.Clamp01((Time.time - StartTime) / m_Duration));
			transform.position += m_Speed * Time.deltaTime;
			if (Time.time - StartTime > m_Duration) {
				Destroy(gameObject);
			}
		}




	}
}