namespace Hierponent { // v1.0.4
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Collections;


	public class Hierponent {



		#region --- VAR ---


		private const int MAX_ICON_NUM = 4;
		private const int GLOBAL_OFFSET = -6;

		private static List<System.Type> HideTypes = new List<System.Type>() {
			typeof(Transform),
			typeof(ParticleSystemRenderer),
			typeof(CanvasRenderer),
		};
		private static Transform OffsetObject = null;
		private static int Offset = 0;


		#endregion



		#region --- MSG ---



		[InitializeOnLoadMethod]
		public static void Init () {
			EditorApplication.hierarchyWindowItemOnGUI += HieGUI;
		}


		public static void HieGUI (int instanceID, Rect rect) {


			// Check
			Object tempObj = EditorUtility.InstanceIDToObject(instanceID);
			if (!tempObj) {
				return;
			}


			// fix rect
			rect.width += rect.x;
			rect.x = 0;


			// Logic
			GameObject obj = tempObj as GameObject;
			List<Component> coms = new List<Component>(obj.GetComponents<Component>());
			for (int i = 0; i < coms.Count; i++) {
				if (!coms[i]) {
					continue;
				}
				if (TypeCheck(coms[i].GetType())) {
					coms.RemoveAt(i);
					i--;
				}
			}

			int iconSize = 16;
			int y = 1;
			int offset = obj.transform == OffsetObject ? Offset : 0;

			// Main
			var oldC = GUI.color;
			for (int i = 0; i + offset < coms.Count && i < MAX_ICON_NUM; i++) {

				Component com = coms[i + offset];

				// Logic
				Texture2D texture = AssetPreview.GetMiniThumbnail(com);

				if (texture) {
					GUI.color = com.gameObject.activeSelf ? Color.white : new Color(1, 1, 1, 0.4f);
					var _r = new Rect(
						rect.width - (iconSize + 1) * i + GLOBAL_OFFSET,
						rect.y + y,
						iconSize,
						iconSize
					);
					GUI.Box(_r, GUIContent.none);
					GUI.DrawTexture(_r, texture);
				}
			}
			GUI.color = oldC;

			// More Button
			if (coms.Count == MAX_ICON_NUM + 1) {
				Texture2D texture = AssetPreview.GetMiniThumbnail(coms[coms.Count - 1]);
				if (texture) {
					GUI.DrawTexture(new Rect(
						rect.width - (iconSize + 1) * (coms.Count - 1) + GLOBAL_OFFSET,
						rect.y + y,
						iconSize,
						iconSize
					), texture);
				}
			} else if (coms.Count > MAX_ICON_NUM) {

				GUIStyle style = new GUIStyle(GUI.skin.label) {
					fontSize = 9,
					alignment = TextAnchor.MiddleCenter
				};

				if (GUI.Button(new Rect(
					rect.width - (iconSize + 2) * (MAX_ICON_NUM) + GLOBAL_OFFSET,
					rect.y + y,
					22,
					iconSize
				), "•••", style)) {
					if (OffsetObject != obj.transform) {
						OffsetObject = obj.transform;
						Offset = 0;
					}
					Offset += MAX_ICON_NUM;
					if (Offset >= coms.Count) {
						Offset = 0;
					}
				}

			}


		}


		#endregion



		#region --- LGC ---


		private static bool TypeCheck (System.Type type) {
			for (int i = 0; i < HideTypes.Count; i++) {
				if (type == HideTypes[i] || type.IsSubclassOf(HideTypes[i])) {
					return true;
				}
			}
			return false;
		}


		#endregion


	}
}