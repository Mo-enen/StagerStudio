

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
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using UnityEditor;
	using UnityEditor.Experimental.SceneManagement;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEditor.SceneManagement;


	public static class QuickAccess {




		#region --- VAR ---


		// SUB
		private class ObjectComparer : IComparer<(Object obj, int)> {
			public int Compare ((Object obj, int) x, (Object obj, int) y) {
				var xObj = x.obj;
				var yObj = y.obj;
				if (xObj is GameObject gX && yObj is GameObject gY) {
					bool xIsPrefab = PrefabUtility.GetPrefabAssetType(xObj) == PrefabAssetType.Regular;
					bool yIsPrefab = PrefabUtility.GetPrefabAssetType(yObj) == PrefabAssetType.Regular;
					if (!xIsPrefab && !yIsPrefab) {
						int gapX = 0, gapY = 0;
						for (Transform tf = gX.transform; tf != null; tf = tf.parent) { gapX++; }
						for (Transform tf = gY.transform; tf != null; tf = tf.parent) { gapY++; }
						return gapX == gapY ? xObj.name.CompareTo(yObj.name) : gapX > gapY ? 1 : -1;
					} else {
						return xIsPrefab == yIsPrefab ? xObj.name.CompareTo(yObj.name) : xIsPrefab ? 1 : -1;
					}
				} else {
					int index = xObj.GetType().ToString().CompareTo(yObj.GetType().ToString());
					return index == 0 ? xObj.name.CompareTo(yObj.name) : index;
				}
			}
		}


		[System.Serializable]
		public class ItemsJsonData {


			//SUB
			[System.Serializable]
			public class ItemData {
				public bool IsAsset => string.IsNullOrEmpty(ScenePath);
				public string GUID = ""; // Asset or Scene
				public string ScenePath = ""; // Scene Object Path eg: rootGameobject/TestObject/Test
				public string ComponentType = ""; // Component Only
				public string ComponentAssembly = ""; // Component Only
			}


			// VAR
			public List<ItemData> Items = new List<ItemData>();


			// API
			public static List<(Object, int)> DataToList (ItemsJsonData data) {
				var list = new List<(Object, int)>();
				if (data != null) {
					for (int i = 0; i < data.Items.Count; i++) {
						var item = data.Items[i];
						if (item is null) { continue; }
						try {
							var obj = DataToObject(item);
							if (obj != null) {
								list.Add((obj, i));
							}
						} catch { }
					}
				}
				return list;
			}


			public static ItemData ObjectToData (Object obj) {
				if (obj == null) { return null; }
				string path = AssetDatabase.GetAssetPath(obj);
				if (string.IsNullOrEmpty(path)) {
					try {
						string type = "";
						string assembly = "";
						string guid = "";
						string scenePath = "";
						// Scene
						switch (obj) {
							case Component cItem:
								type = obj.GetType().ToString();
								assembly = obj.GetType().Assembly.ToString();
								scenePath = GetScenePath(cItem.gameObject);
								guid = AssetDatabase.AssetPathToGUID(SceneManager.GetSceneByName(cItem.gameObject.scene.name).path);
								break;
							case GameObject gItem:
								scenePath = GetScenePath(gItem);
								guid = AssetDatabase.AssetPathToGUID(SceneManager.GetSceneByName(gItem.scene.name).path);
								break;
						}
						// Add
						if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(scenePath)) {
							return new ItemData() {
								GUID = guid,
								ComponentType = type,
								ComponentAssembly = assembly,
								ScenePath = scenePath,
							};
						}
					} catch { }
				} else {
					// Asset
					return new ItemData() {
						GUID = AssetDatabase.AssetPathToGUID(path),
						ComponentType = "",
						ScenePath = "",
					};
				}
				return null;
				// Func
				string GetScenePath (GameObject g) {
					var builder = new StringBuilder();
					for (Transform tf = g.transform; tf != null; tf = tf.parent) {
						builder.Insert(0, tf.parent != null ? $"/{tf.GetSiblingIndex()}" : tf.GetSiblingIndex().ToString());
					}
					return builder.ToString();
				}
			}


			private static Object DataToObject (ItemData item) {
				var path = AssetDatabase.GUIDToAssetPath(item.GUID);
				if (!string.IsNullOrEmpty(path)) {
					var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
					if (obj != null) {
						if (item.IsAsset) {
							return obj;
						} else if (obj is SceneAsset) {
							var scene = SceneManager.GetSceneByPath(path);
							var g = GetGameObject(scene, item.ScenePath);
							if (g != null) {
								if (string.IsNullOrEmpty(item.ComponentType)) {
									// GameObject
									return g;
								} else {
									// Component
									var type = System.Type.GetType($"{item.ComponentType},{item.ComponentAssembly}");
									var com = g.GetComponent(type);
									if (com != null) {
										return com;
									}
								}
							}
						}
					}
				}
				return null;
				// Func
				GameObject GetGameObject (Scene scene, string scenePath) {
					if (!scene.IsValid()) { return null; }
					var gs = scene.GetRootGameObjects();
					var paths = scenePath.Split('/');
					Transform tf = null;
					for (int i = 0; paths != null && i < paths.Length; i++) {
						var indexStr = paths[i];
						if (string.IsNullOrEmpty(indexStr) || !int.TryParse(indexStr, out int index)) { continue; }
						if (i == 0) {
							if (index >= 0 && index < gs.Length) {
								tf = gs[index].transform;
							} else {
								return null;
							}
						} else if (tf != null) {
							if (index >= 0 && index < tf.childCount) {
								tf = tf.GetChild(index);
							} else {
								return null;
							}
						} else {
							return null;
						}
					}
					return tf.gameObject;
				}
			}


			public void RemoveEmpty () {
				for (int i = 0; i < Items.Count; i++) {
					var item = Items[i];
					if (item == null || AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(item.GUID)) == null) {
						Items.RemoveAt(i);
						i--;
						continue;
					}
				}
			}


		}


		// Data
		private readonly static ItemsJsonData ItemData = new ItemsJsonData();
		private readonly static List<(Object obj, int index)> ItemList = new List<(Object, int)>();
		private static string ConfigPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "QuickAccess Config.json");
		private static bool Folded = false;


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		public static void Init () {
			SceneView.duringSceneGui += OnSceneGUI;
			EditorSceneManager.activeSceneChangedInEditMode += (sceneA, sceneB) => {
				RefreshList();
				SaveToDisk();
			};
			EditorSceneManager.sceneSaved += (scene) => SaveToDisk();
			EditorApplication.playModeStateChanged += (state) => {
				if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode) {
					LoadFromDisk();
					RefreshList();
				}
			};
			Folded = EditorPrefs.GetBool("QuickAccess.QuickAccess.Folded", Folded);
			LoadFromDisk();
			RefreshList();
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
			if (GUI.Button(GUIRect(0, 0), Folded ? "▲  Quick Access (A)  " : "▼  Quick Access (A)  ", EditorStyles.miniButton)) {
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
							var itemData = ItemsJsonData.ObjectToData(obj);
							if (itemData != null) {
								ItemData.Items.Add(itemData);
							}
						}
						RefreshList();
						SaveToDisk();
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
				var item = ItemList[index].obj;
				if (item == null) { continue; }
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
				var item = ItemList[index];
				if (item.obj == null) { return; }
				if (EditorUtility.DisplayDialog("", "Remove \"" + item.obj.name + "\" from Quick Access?", "Delete", "Cancel")) {
					if (item.index >= 0 && item.index < ItemData.Items.Count) {
						ItemData.Items.RemoveAt(item.index);
					}
					RefreshList();
					SaveToDisk();
				}
			});
			menu.ShowAsContext();
		}


		private static void InvokeItem (Object item) {
			switch (item) {
				case MonoBehaviour mItem: {
						var script = MonoScript.FromMonoBehaviour(mItem);
						if (AssetDatabase.GetAssetPath(script).StartsWith("Assets")) {
							AssetDatabase.OpenAsset(script.GetInstanceID());
						} else {
							Selection.activeObject = mItem.gameObject;
						}
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


		private static void LoadFromDisk () {
			try {
				ItemList.Clear();
				if (File.Exists(ConfigPath)) {
					StreamReader sr = File.OpenText(ConfigPath);
					JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), ItemData);
					sr.Close();
				}
			} catch { }
		}


		private static void SaveToDisk () {
			try {
				FileStream fs = new FileStream(ConfigPath, FileMode.Create);
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
				ItemData.RemoveEmpty();
				sw.Write(JsonUtility.ToJson(ItemData, true));
				sw.Close();
				fs.Close();
			} catch { }
		}


		private static void RefreshList () {
			ItemList.Clear();
			ItemList.AddRange(ItemsJsonData.DataToList(ItemData));
			ItemList.Sort(new ObjectComparer());
		}


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