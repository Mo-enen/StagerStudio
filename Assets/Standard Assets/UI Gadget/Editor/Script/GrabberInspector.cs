namespace UIGadget.Editor {
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using UIGadget;


	[CustomEditor(typeof(Grabber), true), CanEditMultipleObjects]
	public class GrabberInspector : Editor {


		// SUB
		private class ComponentComp : IComparer<Component> {
			public int Compare (Component x, Component y) => x.GetType().ToString().CompareTo(y.GetType().ToString());
		}


		// VAR
		private readonly static System.Type[] BLACK_LIST = {
			typeof(CanvasRenderer),


		};
		private readonly List<Component> CacheComs = new List<Component>();


		// MSG
		public override void OnInspectorGUI () {
			base.OnInspectorGUI();
			if (CacheComs.Count == 0) {
				// Drag Mode
				Space(4);
				var rect = GUIRect(0, 48);
				GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
				switch (Event.current.type) {
					case EventType.DragUpdated:
						if (rect.Contains(Event.current.mousePosition)) {
							DragAndDrop.visualMode = DragAndDropVisualMode.Link;
						}
						break;
					case EventType.DragPerform:
						if (rect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length > 0) {
							CacheComs.Clear();
							foreach (var obj in DragAndDrop.objectReferences) {
								if (obj is Component _com) {
									if (BlackListAllow(_com)) {
										CacheComs.Add(_com);
									}
									continue;
								}
								if (!(obj is GameObject g) || !InsideTargets(g)) { continue; }
								// Add All Coms To List
								var coms = g.GetComponents<Component>();
								foreach (var com in coms) {
									if (BlackListAllow(com)) {
										CacheComs.Add(com);
									}
								}
							}
							CacheComs.Sort(new ComponentComp());
							DragAndDrop.AcceptDrag();
						}
						break;
				}
			} else {
				const int HEIGHT = 18;
				Space(8);
				// List Mode
				int needRemove = -1;
				for (int i = 0; i < CacheComs.Count; i++) {
					var com = CacheComs[i];
					LayoutH(() => {
						var rect = GUIRect(0, HEIGHT);
						// Add Button
						if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) {
							var grabTraget = InsideTargets(com.gameObject);
							if (!(grabTraget is null)) {
								grabTraget.Add(com);
							}
							needRemove = i;
						}
						// Field
						EditorGUI.ObjectField(rect, com, com.GetType(), true);
						Space(4);
						// Remove Button
						if (GUI.Button(GUIRect(HEIGHT, HEIGHT), "×", EditorStyles.miniButtonMid)) {
							needRemove = i;
						}
					});
					Space(2);
				}
				if (needRemove >= 0) {
					CacheComs.RemoveAt(needRemove);
					Repaint();
				}
				Space(4);
				// Buttons
				LayoutH(() => {
					// State
					GUI.Label(GUIRect(0, HEIGHT), $"{Selection.gameObjects.Length}/{CacheComs.Count}");
					// Clear
					if (GUI.Button(GUIRect(64, HEIGHT), "Clear", EditorStyles.miniButtonLeft)) {
						CacheComs.Clear();
						Repaint();
					}
					// Apply
					if (GUI.Button(GUIRect(64, HEIGHT), "Apply", EditorStyles.miniButtonRight)) {
						foreach (var com in CacheComs) {
							var grabTraget = InsideTargets(com.gameObject);
							if (grabTraget is null) { continue; }
							grabTraget.Add(com);
						}
						CacheComs.Clear();
						Repaint();
					}
				});
			}
			Space(4);
			// Final
			if (GUI.changed) {
				foreach (var t in targets) {
					EditorUtility.SetDirty(t);
				}
			}
		}




		#region --- UTL ---


		private Rect GUIRect (float width, float height) => GUILayoutUtility.GetRect(
			width, height,
			GUILayout.ExpandWidth(width == 0),
			GUILayout.ExpandHeight(height == 0)
		);



		private void LayoutH (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = GUI.skin.box;
			}
			if (style != null) {
				GUILayout.BeginHorizontal(style);
			} else {
				GUILayout.BeginHorizontal();
			}
			action();
			GUILayout.EndHorizontal();
		}


		private void Space (float space = 4f) => GUILayout.Space(space);


		private bool BlackListAllow (Component com) {
			if (com is null) { return false; }
			var type = com.GetType();
			for (int i = 0; i < BLACK_LIST.Length; i++) {
				if (type == BLACK_LIST[i] || type.IsSubclassOf(BLACK_LIST[i])) {
					return false;
				}
			}
			return true;
		}


		private Grabber InsideTargets (GameObject g) {
			foreach (var tar in targets) {
				var grab = tar as Grabber;
				if (g.transform == grab.transform || g.transform.IsChildOf(grab.transform)) {
					return grab;
				}
			}
			return null;
		}


		#endregion


	}
}