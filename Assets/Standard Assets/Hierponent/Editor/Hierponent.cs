namespace Hierponent { // v1.1.0
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Collections;
	using System.IO;
	using System.Text;

	public class Hierponent {



		#region --- VAR ---


		private const int MAX_ICON_NUM = 4;

		private static List<System.Type> HideTypes = new List<System.Type>() {
			typeof(Transform),
			typeof(ParticleSystemRenderer),
			typeof(CanvasRenderer),
		};
		private static Transform OffsetObject = null;
		private static int Offset = 0;
		private static bool ShowActiveToggle = true;


		#endregion



		#region --- MSG ---



		[InitializeOnLoadMethod]
		public static void Init () {
			EditorApplication.hierarchyWindowItemOnGUI += HieGUI;
			LoadConfig();
			SaveConfig();
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
			float globalOffsetX = PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab ? 0 : -18;
			if (ShowActiveToggle) {
				globalOffsetX -= 18;
			}

			// Main
			var oldC = GUI.color;
			for (int i = 0; i + offset < coms.Count && i < MAX_ICON_NUM; i++) {

				Component com = coms[i + offset];
				Texture2D texture = AssetPreview.GetMiniThumbnail(com);

				if (texture) {
					GUI.color = com.gameObject.activeInHierarchy ? Color.white : new Color(1, 1, 1, 0.4f);
					var _r = new Rect(
						rect.width - (iconSize + 1) * i + globalOffsetX,
						rect.y + y,
						iconSize,
						iconSize
					);
					GUI.Box(_r, GUIContent.none);
					GUI.DrawTexture(_r, texture);
				}
			}
			GUI.color = oldC;


			// One More Thing
			if (coms.Count == MAX_ICON_NUM + 1) {
				// One More Icon
				Texture2D texture = AssetPreview.GetMiniThumbnail(coms[coms.Count - 1]);
				if (texture) {
					GUI.DrawTexture(new Rect(
						rect.width - (iconSize + 1) * (coms.Count - 1) + globalOffsetX,
						rect.y + y,
						iconSize,
						iconSize
					), texture);
				}
			} else if (coms.Count > MAX_ICON_NUM) {
				// "..." Button
				GUIStyle style = new GUIStyle(GUI.skin.label) {
					fontSize = 9,
					alignment = TextAnchor.MiddleCenter
				};

				if (GUI.Button(new Rect(
					rect.width - (iconSize + 2) * (MAX_ICON_NUM) + globalOffsetX,
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


			if (ShowActiveToggle) {
				// Active Toggle
				rect.x = rect.width + globalOffsetX + 18;
				rect.width = rect.height;
				bool active = GUI.Toggle(rect, obj.activeSelf, GUIContent.none);
				if (active != obj.activeSelf) {
					obj.SetActive(active);
					EditorUtility.SetDirty(obj);
				}
			}



		}


		[MenuItem("Tools/Hierponent/Show Active Toggle")]
		public static void SetShowActiveToggle_True () {
			ShowActiveToggle = true;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}


		[MenuItem("Tools/Hierponent/Hide Active Toggle")]
		public static void SetShowActiveToggle_False () {
			ShowActiveToggle = false;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
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


		private static void SaveConfig () {
			EditorPrefs.SetBool("Hierponent.ShowActiveToggle", ShowActiveToggle);
		}


		private static void LoadConfig () {
			try {
				ShowActiveToggle = EditorPrefs.GetBool("Hierponent.ShowActiveToggle", ShowActiveToggle);
			} catch { }
		}


		#endregion



	}
}