namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class VanishTextRenderer : TextRenderer {




		//Ser
		[SerializeField] private float m_Duration = 1f;

		// Data
		private Color NormalColor = Color.white;
		private Color ClearColor = Color.clear;
		private Vector3 StartPos = default;
		private float StartTime = 0f;


		private void Start () {
			NormalColor = ClearColor = Tint;
			ClearColor.a = 0f;
			StartTime = Time.time;
			StartPos = transform.position;
		}


		private void Update () {
			Tint = Color.Lerp(NormalColor, ClearColor, Mathf.Clamp01(2f * (Time.time - StartTime) / m_Duration));
			var oldPos = transform.position;
			var aimPos = new Vector3(
				StartPos.x,
				StartPos.y + (transform.parent.childCount - transform.GetSiblingIndex() - 1) * Scale.y * transform.localScale.y,
				StartPos.z
			);
			transform.position = Vector3.Lerp(oldPos, aimPos, Time.deltaTime * 20f);
			if (Time.time - StartTime > m_Duration) {
				Destroy(gameObject);
			}
		}




	}
}