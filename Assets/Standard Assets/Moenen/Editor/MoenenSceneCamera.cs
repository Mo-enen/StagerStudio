namespace Moenen.Tools {
	using UnityEngine;
	using UnityEditor;
	using System.Collections;

	public class MoenenSceneCamera {



		[InitializeOnLoadMethod]
		public static void EditorInit () {
#if UNITY_2017 || UNITY_2018
			SceneView.onSceneGUIDelegate -= SceneUpdate;
			SceneView.onSceneGUIDelegate += SceneUpdate;
#else
			SceneView.duringSceneGui -= SceneUpdate;
			SceneView.duringSceneGui += SceneUpdate;
#endif

		}



		private static void SceneUpdate (SceneView sceneView) {

			if (sceneView.in2DMode) {
				return;
			}

			//sceneView.SetSceneViewFiltering(false);


			switch (Event.current.type) {

				case EventType.MouseDrag:

					if (Event.current.button == 1) {
						// Mosue Right Drag
						if (!Event.current.alt) {
							// View Rotate
							Vector2 del = Event.current.delta * 0.2f;
							float angle = sceneView.camera.transform.rotation.eulerAngles.x + del.y;
							angle = angle > 89 && angle < 180 ? 89 : angle;
							angle = angle > 180 && angle < 271 ? 271 : angle;
							sceneView.LookAt(
								sceneView.pivot,
								Quaternion.Euler(
									angle,
									sceneView.camera.transform.rotation.eulerAngles.y + del.x,
									0f
								),
								sceneView.size,
								sceneView.orthographic,
								true
							);
							Event.current.Use();
						}
					}
					break;
			}



		}



	}
}