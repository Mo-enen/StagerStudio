namespace StagerStudio.Rendering {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class GridRenderer : StageRenderer {



		// Handler
		public delegate float FloatFloatFloatHandler (float time, float area);

		// Api
		public static FloatFloatFloatHandler Fill { get; set; } = null;
		public float MusicTime {
			get => _MusicTime;
			set {
				if (value != _MusicTime) {
					_MusicTime = value;
					SetDirty();
				}
			}
		}
		public float Duration {
			get => _Duration;
			set {
				if (value != _Duration) {
					_Duration = value;
					SetDirty();
				}
			}
		}
		public int CountX {
			get => m_CountX;
			set {
				if (value != m_CountX) {
					m_CountX = value;
					SetDirty();
				}
			}
		}
		public float TimeGap {
			get => m_TimeGap;
			set {
				if (value != m_TimeGap) {
					m_TimeGap = value;
					SetDirty();
				}
			}
		}

		// Ser
		[SerializeField] private Sprite m_Sprite = null;
		[SerializeField] private int m_CountX = 0;
		[SerializeField] private float m_TimeGap = 1f;

		// Data
		private float _MusicTime = -1f;
		private float _Duration = 0f;


		// MSG
		protected override void OnMeshFill () {
			if (m_Sprite is null) { return; }
			Debug.Log(m_Sprite.uv[0] + " " + m_Sprite.uv[1] + " " + m_Sprite.uv[2] + " " + m_Sprite.uv[3]);
			var uvMin = m_Sprite.uv[0];
			var uvMax = m_Sprite.uv[2];
			// X
			{
				const int MAX_COUNT_X = 32;
				int countX = Mathf.Clamp(CountX + 2, 2, MAX_COUNT_X);
				float sizeX = 0.01f / Scale.x;
				for (int i = 0; i <= countX; i++) {
					float x01 = (float)i / countX;
					//AddQuad01(x01 - sizeX, x01 + sizeX, 0f, 1f,,,,);




				}
			}
			// Y
			//const int MAX_COUNT_Y = 64;



		}




	}
}