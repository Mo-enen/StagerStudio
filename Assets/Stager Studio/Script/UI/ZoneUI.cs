namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class ZoneUI : MonoBehaviour {


		// SUB
		public delegate string StringStringHandler (string str);

		public class LanguageData {


		}




		// Handler
		public static StringStringHandler GetLanguage { get; set; } = null;


		// API
		public Vector3 ZoneMinPos { get; private set; } = default;
		public Vector3 ZoneMaxPos { get; private set; } = default;
		public bool IsShowing => m_ZoneGraphics[0].enabled;

		// Ser
		[SerializeField] private Transform m_ZoneMin = null;
		[SerializeField] private Transform m_ZoneMax = null;
		[SerializeField] private AspectRatioFitter m_ZoneFitter = null;
		[SerializeField] private Graphic[] m_ZoneGraphics = null;

		// Data
		private Coroutine ZoneCor = null;



		// MSG
		private void Update () {
			CacheUpdate();

		}


		void CacheUpdate () {
			var min = m_ZoneMin.position;
			var max = m_ZoneMax.position;
			max.y = min.y + max.x - min.x;
			if (min != ZoneMinPos || max != ZoneMaxPos) {
				ZoneMinPos = min;
				ZoneMaxPos = max;
			}
		}



		// API
		public void SetFitterRatio (float newRatio) {
			if (ZoneCor != null) {
				StopCoroutine(ZoneCor);
				ZoneCor = null;
			}
			ZoneCor = StartCoroutine(SetRatio(newRatio));
			IEnumerator SetRatio (float ratio) {
				float r = m_ZoneFitter.aspectRatio;
				while (Mathf.Abs(r - ratio) > 0.005f) {
					r = Mathf.Lerp(r, ratio, Time.deltaTime * 4f);
					m_ZoneFitter.aspectRatio = r;
					yield return new WaitForEndOfFrame();
				}
				m_ZoneFitter.aspectRatio = ratio;
				ZoneCor = null;
			}
		}


		public void ShowZone (bool show) {
			foreach (var g in m_ZoneGraphics) {
				g.enabled = show;
			}
		}


		public (Vector3, Vector3, float) GetZoneMinMax () => (ZoneMinPos, ZoneMaxPos, ZoneMaxPos.x - ZoneMinPos.x);




	}
}