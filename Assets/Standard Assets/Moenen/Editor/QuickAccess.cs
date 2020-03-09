

/*

	☆☆☆★  Quick Access v1.0.0  ★☆☆☆
	
	An extra panel in the corner of the scene shows your commonly used objects.


	How to use:
	- Copy QuickAccess.cs to anywhere in the Assets folder.
	- Add objects by dragging them into the quick access panel from Hierarchy, Project or Inspector.
	- Click an object to open or select it.
	- Right click an object to open the menu.
	- Click the button at the bottom to show or hide the panel (hotkey A).


	Created by 楠瓜Moenen, Free to use, do not sale, enjoy.
	

	Email moenen6@gmail.com, moenenn@163.com
	Twitter @_Moenen
	QQ 1182032752
	AssetStore: https://assetstore.unity.com/publishers/15506


*/


#if UNITY_EDITOR
namespace QuickAccess {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using System.IO;
	using System.Text;
	using UnityEditor.Experimental.SceneManagement;


	public static class QuickAccess {




		#region --- VAR ---


		// SUB
		private class ObjectComparer : IComparer<Object> {
			public int Compare (Object x, Object y) {
				if (x is GameObject gX && y is GameObject gY) {
					bool xIsPrefab = PrefabUtility.GetPrefabAssetType(x) == PrefabAssetType.Regular;
					bool yIsPrefab = PrefabUtility.GetPrefabAssetType(y) == PrefabAssetType.Regular;
					if (!xIsPrefab && !yIsPrefab) {
						int gapX = 0, gapY = 0;
						for (Transform tf = gX.transform; tf != null; tf = tf.parent) { gapX++; }
						for (Transform tf = gY.transform; tf != null; tf = tf.parent) { gapY++; }
						return gapX == gapY ? x.name.CompareTo(y.name) : gapX > gapY ? 1 : -1;
					} else {
						return xIsPrefab == yIsPrefab ? x.name.CompareTo(y.name) : xIsPrefab ? 1 : -1;
					}
				} else {
					int index = x.GetType().ToString().CompareTo(y.GetType().ToString());
					return index == 0 ? x.name.CompareTo(y.name) : index;
				}
			}
		}


		// Data
		private readonly static List<Object> ItemList = new List<Object>();
		private static string ConfigPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "QuickAccess Config.txt");
		private static bool Folded = false;


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		public static void Init () {
			SceneView.duringSceneGui += OnSceneGUI;
			EditorApplication.playModeStateChanged += (state) => {
				if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode) {
					LoadFromDisk();
				}
			};
			Folded = EditorPrefs.GetBool("QuickAccess.QuickAccess.Folded", Folded);
			LoadFromDisk();
			// Func
			void LoadFromDisk () {
				ItemList.Clear();
				if (File.Exists(ConfigPath)) {
					StreamReader sr = File.OpenText(ConfigPath);
					string[] configs = sr.ReadToEnd().Split('\n');
					sr.Close();
					foreach (var con in configs) {
						if (int.TryParse(con, out int id)) {
							var obj = EditorUtility.InstanceIDToObject(id);
							if (!(obj is null)) {
								ItemList.Add(obj);
							}
						}
					}
				}
				SortItems();
			}
		}


		[MenuItem("Tools/Fold Quick Access _a")]
		public static void HotKeyG () => SetFolded(!Folded);


		private static void OnSceneGUI (SceneView view) {
			if (view is null || PrefabStageUtility.GetCurrentPrefabStage() != null) { return; }
			const float AREA_WIDTH = 160f;
			const float ITEM_HEIGHT = 18f;
			const float ITEM_SPACE = 2f;
			const float END_SPACE = 0f;
			const float SCENE_GAP_FIX = 22f;
			float AreaHeight = Mathf.Clamp(
				ItemList.Count * (ITEM_HEIGHT + ITEM_SPACE) + END_SPACE + 4,
				64f,
				view.position.height - SCENE_GAP_FIX - ITEM_HEIGHT
			);

			// Fold Button
			Handles.BeginGUI();
			GUILayout.BeginArea(new Rect(
				0,
				view.position.height - ITEM_HEIGHT - SCENE_GAP_FIX,
				AREA_WIDTH,
				ITEM_HEIGHT
			));
			if (GUI.Button(GUIRect(0, 0), Folded ? "▲  Quick Access  (A)  " : "▼  Quick Access  (A)  ", EditorStyles.miniButton)) {
				SetFolded(!Folded);
			}
			GUILayout.EndArea();
			Handles.EndGUI();

			// Main GUI
			Handles.BeginGUI();
			GUILayout.BeginArea(new Rect(
				!Folded ? 0 : -AREA_WIDTH,
				view.position.height - AreaHeight - ITEM_HEIGHT - SCENE_GAP_FIX,
				AREA_WIDTH,
				AreaHeight
			));
			var areaRect = new Rect(0, 0, AREA_WIDTH, AreaHeight);

			// Null Check
			for (int i = 0; i < ItemList.Count; i++) {
				var item = ItemList[i];
				if (item == null) {
					ItemList.RemoveAt(i);
					i--;
				}
			}

			// System
			bool mouseDown = Event.current.type == EventType.MouseDown;
			bool mouseDownInItem = false;
			bool mouseInArea = areaRect.Contains(Event.current.mousePosition);
			if (mouseInArea) {
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			}

			// Drag
			switch (Event.current.type) {
				case EventType.DragUpdated:
					if (mouseInArea && DragAndDrop.objectReferences.Length > 0) {
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
					}
					break;
				case EventType.DragPerform:
					if (mouseInArea && DragAndDrop.objectReferences.Length > 0) {
						foreach (var obj in DragAndDrop.objectReferences) {
							if (obj is null) { continue; }
							ItemList.Add(obj);
						}
						SortItems();
						Save();
						DragAndDrop.AcceptDrag();
						Event.current.Use();
					}
					break;
			}

			// Background
			{
				var oldColor = GUI.color;
				GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
				GUI.DrawTexture(areaRect, Texture2D.whiteTexture);
				GUI.color = oldColor;
			}

			// Content
			Space(2);
			for (int i = 0; i < ItemList.Count; i++) {
				int index = i;
				var item = ItemList[index];
				if (item is null) { continue; }
				var rect = GUIRect(0, ITEM_HEIGHT);
				rect.width += 18;
				if (mouseDown && !mouseDownInItem && mouseInArea && rect.Contains(Event.current.mousePosition)) {
					mouseDownInItem = true;
					if (Event.current.button == 0) {
						// Select
						InvokeItem(item);
					} else if (Event.current.button == 1) {
						// Menu
						SpawnMenu(index);
					}
					Event.current.Use();
				}
				// Field
				EditorGUI.ObjectField(rect, item, item.GetType(), true);
				// Highlight
				if (item == Selection.activeObject) {
					var oldColor = GUI.color;
					GUI.color = new Color(0.1f, 0.6f, 1f, 0.4f);
					GUI.DrawTexture(rect, Texture2D.whiteTexture);
					GUI.color = oldColor;
				}
				// Space
				Space(ITEM_SPACE);
			}

			// Helpbox
			if (ItemList.Count == 0) {
				EditorGUI.LabelField(GUIRect(0, 36), "Drag object here to add", EditorStyles.centeredGreyMiniLabel);
			} else {
				Space(END_SPACE);
			}

			GUILayout.EndArea();
			Handles.EndGUI();

			// Final
			if (mouseInArea && (Event.current.isMouse || Event.current.isKey || Event.current.isScrollWheel)) {
				Event.current.Use();
			}
			if (mouseInArea && mouseDown && !mouseDownInItem) {
				Selection.activeObject = null;
			}

		}


		#endregion



		#region --- LGC ---


		private static void SpawnMenu (int index) {
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Delete"), false, () => {
				if (EditorUtility.DisplayDialog("", "Remove \"" + ItemList[index].name + "\" from Quick Access?", "Delete", "Cancel")) {
					ItemList.RemoveAt(index);
					Save();
				}
			});
			menu.ShowAsContext();
		}


		private static void InvokeItem (Object item) {
			switch (item) {
				case MonoBehaviour mItem: {
						AssetDatabase.OpenAsset(MonoScript.FromMonoBehaviour(mItem).GetInstanceID());
					}
					break;
				case MonoScript sItem: {
						AssetDatabase.OpenAsset(sItem.GetInstanceID());
					}
					break;
				case GameObject gItem: {
						if (PrefabUtility.GetPrefabAssetType(gItem) == PrefabAssetType.Regular) {
							AssetDatabase.OpenAsset(gItem.GetInstanceID());
						} else {
							Selection.activeObject = item;
						}
					}
					break;
				default:
					Selection.activeObject = item;
					break;
			}
		}


		private static void Save () {
			StringBuilder config = new StringBuilder();
			foreach (var item in ItemList) {
				if (item is null) { continue; }
				config.AppendLine(item.GetInstanceID().ToString());
			}
			// Save to Disk
			FileStream fs = new FileStream(ConfigPath, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.Write(config);
			sw.Close();
			fs.Close();
		}


		private static void SortItems () => ItemList.Sort(new ObjectComparer());


		private static void SetFolded (bool folded) {
			Folded = folded;
			EditorPrefs.SetBool("QuickAccess.QuickAccess.Folded", Folded);
		}


		#endregion




		#region --- UTL ---


		private static Rect GUIRect (float width, float height) => GUILayoutUtility.GetRect(
			width, height,
			GUILayout.ExpandWidth(width == 0),
			GUILayout.ExpandHeight(height == 0)
		);


		private static void Space (float space = 4f) => GUILayout.Space(space);


		#endregion


	}
}
#endif