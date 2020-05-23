namespace UIGadget {
	using System.Linq;
	using UnityEngine;


	public class Grabber : MonoBehaviour {


		[SerializeField] private Component[] m_Components = null;


		public T Grab<T> (string key = "") where T : Component {
			if (string.IsNullOrEmpty(key)) { key = name; }
			foreach (var com in m_Components) {
				if (com.transform.name == key && com is T) {
					return com as T;
				}
			}
			return null;
		}


		public void Add (Component com) {
			if (com is null) { return; }
			if (m_Components is null) { m_Components = new Component[0]; }
			m_Components = m_Components.Concat(new Component[] { com }).ToArray();
		}


	}
}
