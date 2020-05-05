namespace StagerStudio.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


	public class ZoneUI : MonoBehaviour {


		// Short
		private Camera Camera => _Camera != null ? _Camera : (_Camera = Camera.main);

		// Ser
		[SerializeField] private Transform m_ZoneMin = null;
		[SerializeField] private Transform m_ZoneMax = null;
		[SerializeField] private AspectRatioFitter m_ZoneFitter = null;

		// Data
		private Coroutine ZoneCor = null;
		private Camera _Camera = null;
		private Vector3 ZoneMinPos = default;
		private Vector3 ZoneMaxPos = default;
		private Vector3 ZoneMaxPos_Real = default;
		private float ZoneSize = 0f;
		private float Ratio = 1f;


		// MSG
		private void Update () {
			var min = m_ZoneMin.position;
			var max = m_ZoneMax.position;
			Ratio = Mathf.Clamp(
				Mathf.Abs(max.y - min.y) > 0.0001f ? (max.x - min.x) / (max.y - min.y) : float.MaxValue,
				0.0001f,
				float.MaxValue
			);
			ZoneSize = max.x - min.x;
			max.z = min.z + ZoneSize;
			ZoneMinPos = min;
			ZoneMaxPos_Real = ZoneMaxPos = max;
			ZoneMaxPos.y = ZoneMinPos.y + ZoneSize;
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


		public (Vector3, Vector3, float, float) GetZoneMinMax (bool real = false) => (
			ZoneMinPos, real ? ZoneMaxPos_Real : ZoneMaxPos, ZoneSize, Ratio
		);


		public (Vector2, Vector2) GetScreenZoneMinMax () {
			var max = ZoneMaxPos_Real;
			max.z = ZoneMinPos.z;
			return (
				Camera.WorldToScreenPoint(ZoneMinPos),
				Camera.WorldToScreenPoint(max)
			);
		}
	}
}