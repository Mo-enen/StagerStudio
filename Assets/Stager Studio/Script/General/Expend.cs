namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class Expand {



		public static void TrySetActive (this GameObject g, bool active) {
			if (g.activeSelf != active) {
				g.SetActive(active);
			}
		}


		public static void SetAllChildActive (this Transform transform, bool active) {
			int len = transform.childCount;
			for (int i = 0; i < len; i++) {
				var tf = transform.GetChild(i);
				if (tf.gameObject.activeSelf != active) {
					tf.gameObject.SetActive(active);
				}
			}
		}


		public static void DestroyAllChildImmediately (this Transform transform) {
			int len = transform.childCount;
			for (int i = 0; i < len; i++) {
				UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject, false);
			}
		}


		public static void DestroyAllChild (this Transform transform) {
			int len = transform.childCount;
			for (int i = 0; i < len; i++) {
				UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
			}
		}


		public static void FixChildcountImmediately (this Transform transform, int count) {
			int len = Mathf.Max(transform.childCount - count, 0);
			for (int i = 0; i < len; i++) {
				UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject, false);
			}
		}


		public static void InactiveIfNoChildActive (this Transform tf) {
			int len = tf.childCount;
			for (int i = 0; i < len; i++) {
				if (tf.GetChild(i).gameObject.activeSelf) { return; }
			}
			tf.gameObject.SetActive(false);
		}


		public static Vector2 Get01Position (this RectTransform rt, Vector2 screenPos, Camera camera) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, camera, out Vector2 localPos);
			return new Vector2(
				(localPos.x - rt.rect.xMin) / Mathf.Max(rt.rect.width, float.Epsilon),
				(localPos.y - rt.rect.yMin) / Mathf.Max(rt.rect.height, float.Epsilon)
			);
		}


		public static Vector2 Clamp01 (this Vector2 v) {
			v.x = Mathf.Clamp01(v.x);
			v.y = Mathf.Clamp01(v.y);
			return v;
		}


		public static Vector2 Clamp (this Vector2 v, Vector2 min, Vector2 max) {
			v.x = Mathf.Clamp(v.x, min.x, max.x);
			v.y = Mathf.Clamp(v.y, min.y, max.y);
			return v;
		}


		public static Vector2 Clamp (this Vector2 v, float min, float max) {
			v.x = Mathf.Clamp(v.x, min, max);
			v.y = Mathf.Clamp(v.y, min, max);
			return v;
		}


		public static Vector3 Clamp01 (this Vector3 v) {
			v.x = Mathf.Clamp01(v.x);
			v.y = Mathf.Clamp01(v.y);
			v.z = Mathf.Clamp01(v.z);
			return v;
		}


		public static Vector2 Abs (this Vector2 v) {
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			return v;
		}


		public static Vector3 Abs (this Vector3 v) {
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			v.z = Mathf.Abs(v.z);
			return v;
		}


		public static Vector3 Muti (this Vector3 v, Vector3 muti) => new Vector3(
			v.x * muti.x,
			v.y * muti.y,
			v.z * muti.z
		);


		public static void SetPivotWithoutChangePosition (this RectTransform rectTransform, Vector2 pivot) {
			Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
			deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
			deltaPosition.Scale(rectTransform.localScale);          // apply scaling
			deltaPosition = rectTransform.transform.rotation * deltaPosition; // apply rotation

			rectTransform.pivot = pivot;                            // change the pivot
			rectTransform.localPosition -= deltaPosition;           // reverse the position change
		}


		public static bool TryParseFloatForInspector (this string text, out float result) {
			result = 0f;
			if (string.IsNullOrEmpty(text)) { return false; }
			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '.' && (i == 0 || text[i - 1] < '0' || text[i - 1] > '9')) {
					text = text.Insert(i, "0");
					i++;
				}
			}
			text = System.Text.RegularExpressions.Regex.Replace(text, @"[^0-9.+\-*/]+", string.Empty);
			text = new System.Data.DataTable().Compute(text, null).ToString();
			return float.TryParse(text, out result);
		}


		public static bool TryParseIntForInspector (this string text, out int result) {
			result = 0;
			if (string.IsNullOrEmpty(text)) { return false; }
			text = System.Text.RegularExpressions.Regex.Replace(text, @"[^0-9+\-*/]+", string.Empty);
			text = new System.Data.DataTable().Compute(text, null).ToString();
			bool success = float.TryParse(text, out float fResult);
			result = Mathf.RoundToInt(fResult);
			return success;
		}


		public static void LerpUI (this RectTransform rt, Vector2 aimAnchoredPos, float lerp = 20f) {
			if (
				Mathf.Abs(rt.anchoredPosition.x - aimAnchoredPos.x) > 0.001f ||
				Mathf.Abs(rt.anchoredPosition.y - aimAnchoredPos.y) > 0.001f
			) {
				var pos = rt.anchoredPosition;
				pos.x = Mathf.Lerp(pos.x, aimAnchoredPos.x, Mathf.Min(Time.deltaTime, 0.1f) * lerp);
				pos.y = Mathf.Lerp(pos.y, aimAnchoredPos.y, Mathf.Min(Time.deltaTime, 0.1f) * lerp);
				if (Mathf.Abs(pos.x - aimAnchoredPos.x) < 0.01f && Mathf.Abs(pos.y - aimAnchoredPos.y) < 0.01f) {
					pos.x = aimAnchoredPos.x;
					pos.y = aimAnchoredPos.y;
				}
				rt.anchoredPosition3D = pos;
			}
		}


	}
}