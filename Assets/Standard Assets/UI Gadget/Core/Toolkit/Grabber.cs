namespace UIGadget {
	using System.Linq;
	using UnityEngine;



	[AddComponentMenu("UIGadget/Grabber")]
	public class Grabber : MonoBehaviour {


		[SerializeField] private Component[] m_Components = null;


		public T Grab<T> (string key = "") where T : Component {
			if (string.IsNullOrEmpty(key)) { key = name; }
			T result = null;
			foreach (var com in m_Components) {
				if (com is T) {
					if (com.transform.name == key) {
						return com as T;
					}
					result = com as T;
				}
			}
			return result;
		}


		public void Add (Component com) {
			if (com is null) { return; }
			if (m_Components is null) { m_Components = new Component[0]; }
			m_Components = m_Components.Concat(new Component[] { com }).ToArray();
		}


	}
}
