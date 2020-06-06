///*

#if UNITY_EDITOR
namespace Curve {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;



	[System.Serializable]
	public class ConstantFloatSer : ConstantFloat, ISerializationCallbackReceiver {



		[SerializeField] private List<float> m_Keys = null;
		[SerializeField] private List<float> m_Values = null;


		public void OnAfterDeserialize () {
			if (m_Keys != null && m_Values != null) {
				Clear();
				for (int i = 0; i < m_Keys.Count; i++) {
					float key = m_Keys[i];
					float value = i < m_Values.Count ? m_Values[i] : default;
					if (!ContainsKey(key)) {
						Add(key, value);
					} else if (!ContainsKey(key + 1f)) {
						Add(key + 1f, value);
					}
				}
			}
		}



		public void OnBeforeSerialize () {
			m_Keys = new List<float>();
			m_Values = new List<float>();
			for (int i = 0; i < Count; i++) {
				m_Keys.Add(GetKeyAt(i));
				m_Values.Add(GetValueAt(i));
			}
		}


	}





	public class ConstantCurveTester : MonoBehaviour {




		[SerializeField] private ConstantFloatSer m_Curve = null;
		[SerializeField] private float m_EvaluateKey = 0f;
		[SerializeField] private float m_SearchKey = 0f;
		[SerializeField] private Vector2 m_AreaBetween = new Vector2();
		[SerializeField] private Vector2 m_Fill = new Vector2();
		[SerializeField, Range(0.01f, 10f)] private float m_Muti = 1f;


		private void OnDrawGizmos () {

			DrawCoord();

			if (m_Curve == null || m_Curve.Count == 0) { return; }

			// Draw
			DrawCurve();
			DrawEvaluate();
			DrawSearch();
			DrawArea();
			DrawFill();

		}




		private void DrawCoord () {
			const int size = 10;
			var oldC = Gizmos.color;
			for (int i = 0; i <= size; i++) {
				Gizmos.color = new Color(0.5f, 0.5f, 0.5f, i == 0 || i == size ? 1f : 0.4f);
				DrawLine(i, 0, i, size);
				DrawLine(0, i, size, i);
			}
			Gizmos.color = oldC;
		}


		private void DrawCurve () {
			if (m_Curve.Count > 0) {
				var oldC = Gizmos.color;
				Gizmos.color = Color.white;
				float prevKey = m_Curve.GetKeyAt(0);
				float prevValue = m_Curve.GetValue(prevKey) * m_Muti;
				DrawLabel(prevKey, prevValue);
				DrawCube(prevKey, prevValue);
				if (prevKey > 0f) {
					DrawLine(0, prevValue, prevKey, prevValue);
				}
				for (int i = 1; i < m_Curve.Count; i++) {
					float key = m_Curve.GetKeyAt(i);
					float value = m_Curve.GetValue(key) * m_Muti;
					DrawLine(prevKey, prevValue, key, prevValue);
					DrawLabel(key, value);
					DrawCube(key, value);
					prevKey = key;
					prevValue = value;
				}
				if (prevKey < 10f) {
					DrawLine(prevKey, prevValue, 10, prevValue);
				}
				Gizmos.color = oldC;
			}
		}


		private void DrawEvaluate () {
			var oldC = Gizmos.color;
			Gizmos.color = Color.blue;
			float ev = m_Curve.Evaluate(m_EvaluateKey, m_Muti);
			DrawLine(m_EvaluateKey, 0, m_EvaluateKey, ev);
			DrawLabel(ev.ToString(), m_EvaluateKey, ev * 0.5f, Color.cyan);
			Gizmos.color = oldC;
		}


		private void DrawSearch () {
			var oldC = Gizmos.color;
			Gizmos.color = Color.green;
			int index = m_Curve.SearchKey(m_SearchKey);
			DrawLine(m_SearchKey, 0, m_SearchKey, -0.25f);
			DrawLine(m_Curve.GetKeyAt(index), 0, m_Curve.GetKeyAt(index), 0.25f);
			Gizmos.color = oldC;
		}


		private void DrawArea () {
			var oldC = Gizmos.color;
			Gizmos.color = Color.red;
			float valueL = m_Curve.Evaluate(m_AreaBetween.x, m_Muti);
			float valueR = m_Curve.Evaluate(m_AreaBetween.y, m_Muti);
			DrawLine(m_AreaBetween.x, 0, m_AreaBetween.x, valueL);
			DrawLine(m_AreaBetween.y, 0, m_AreaBetween.y, valueR);
			float area = m_Curve.GetAreaBetween(m_AreaBetween.x, m_AreaBetween.y, m_Muti);
			DrawLabel(area.ToString(), (m_AreaBetween.x + m_AreaBetween.y) * 0.5f, -0.4f);
			Gizmos.color = oldC;
		}


		private void DrawFill () {
			var oldC = Gizmos.color;
			Gizmos.color = Color.magenta;
			DrawLine(m_Fill.x, 0, m_Fill.x, m_Curve.Evaluate(m_Fill.x, m_Muti));
			var forKey = m_Curve.Fill(m_Fill.x, m_Fill.y, m_Muti);
			DrawLine(forKey, 0, forKey, m_Curve.Evaluate(forKey, m_Muti));
			DrawLabel(forKey.ToString(), forKey, -0.4f);
			Gizmos.color = oldC;
		}


		// UTL
		private void DrawLine (float keyA, float valueA, float keyB, float valueB) {
			Gizmos.DrawLine(
				transform.position + new Vector3(keyA, valueA),
				transform.position + new Vector3(keyB, valueB)
			);
		}


		private void DrawCube (float key, float value, float size = 0.1f) {
			Gizmos.DrawCube(transform.position + new Vector3(key, value), Vector3.one * size);
		}


		private void DrawLabel (string text, float key, float value, Color color) {
			Handles.Label(
				transform.position + new Vector3(key, value + 0.25f), text,
				new GUIStyle(GUI.skin.label) { normal = new GUIStyleState() { textColor = color, } }
			);
		}


		private void DrawLabel (string text, float key, float value) => DrawLabel(text, key, value, Color.grey);


		private void DrawLabel (float key, float value) => DrawLabel($"({key} , {value})", key, value);


	}


}
#endif
//*/