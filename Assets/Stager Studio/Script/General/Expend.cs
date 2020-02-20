namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class Expand {





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


		public static void SetPivotWithoutChangePosition (this RectTransform rectTransform, Vector2 pivot) {
			Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
			deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
			deltaPosition.Scale(rectTransform.localScale);          // apply scaling
			deltaPosition = rectTransform.transform.rotation * deltaPosition; // apply rotation

			rectTransform.pivot = pivot;                            // change the pivot
			rectTransform.localPosition -= deltaPosition;           // reverse the position change
		}


	}
}