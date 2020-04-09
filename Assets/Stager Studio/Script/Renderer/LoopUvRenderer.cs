namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class LoopUvRenderer : StageRenderer {


		// Api
		public Vector2 Size { get; set; } = Vector2.one;

		// Ser
		[SerializeField] private Vector2 m_Speed = Vector2.one;


		// MSG
		private void Update () => SetDirty();


		protected override void OnMeshFill () {
			float uvL = Mathf.Repeat(Time.time * m_Speed.x, 1f);
			float uvD = Mathf.Repeat(Time.time * m_Speed.y, 1f);
			AddQuad01(
				0f, 1f, 0f, 1f,
				uvL, uvL + Size.x * Scale.x, uvD, uvD + Size.y * Scale.y,
				Vector2.zero
			);
		}



	}
}