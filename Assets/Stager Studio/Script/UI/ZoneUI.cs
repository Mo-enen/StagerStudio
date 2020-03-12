namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class ZoneUI : MonoBehaviour {


		// API
		public bool IsShowing => m_ZoneGraphics[0].enabled;

		// Short
		private Vector3 ZoneMinPos { get; set; } = default;
		private Vector3 ZoneMaxPos { get; set; } = default;
		private Vector3 ZoneMaxPos_Real { get; set; } = default;
		private float Ratio { get; set; } = 1f;
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Transform m_ZoneMin = null;
		[SerializeField] private Transform m_ZoneMax = null;
		[SerializeField] private AspectRatioFitter m_ZoneFitter = null;
		[SerializeField] private Graphic[] m_ZoneGraphics = null;

		// Data
		private Coroutine ZoneCor = null;
		private Camera _Camera = null;


		// MSG
		private void Update () {
			var min = m_ZoneMin.position;
			var max = m_ZoneMax.position;
			Ratio = Mathf.Clamp(
				Mathf.Abs(max.y - min.y) > 0.0001f ? (max.x - min.x) / (max.y - min.y) : float.MaxValue,
				0.0001f,
				float.MaxValue
			);
			ZoneMaxPos_Real = max;
			max.y = min.y + max.x - min.x;
			ZoneMinPos = min;
			ZoneMaxPos = max;
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


		public (Vector3, Vector3, float, float) GetZoneMinMax (bool real = false) => (ZoneMinPos, real ? ZoneMaxPos_Real : ZoneMaxPos, ZoneMaxPos.x - ZoneMinPos.x, Ratio);


		public (Vector2, Vector2) GetScreenZoneMinMax () => (
			Camera.WorldToScreenPoint(ZoneMinPos),
			Camera.WorldToScreenPoint(ZoneMaxPos_Real)
		);


	}
}