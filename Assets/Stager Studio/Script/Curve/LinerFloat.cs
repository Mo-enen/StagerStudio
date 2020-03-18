namespace StagerStudio.Curve {
	using System.Collections.Generic;


	public class LinerFloat {




		#region --- VAR ---


		// API
		public int Count => Items.Count;


		// Data
		private SortedList<float, (float Value, float? Area, float? PositiveArea)> Items = new SortedList<float, (float Value, float? Area, float? PositiveArea)>();


		#endregion




		#region --- API ---


		// Get
		public bool ContainsKey (float key) => Items.ContainsKey(key);


		public float GetValue (float key) => Items[key].Value;


		public float GetValueAt (int index) => Items.Values[index].Value;


		public float GetKeyAt (int index) => Items.Keys[index];


		public int IndexOf (float key) => Items.IndexOfKey(key);


		public int SearchKey (float key) => Search(Items.Keys, key);


		public float Evaluate (float key, float muti = 1f) => Items.Count == 0 ? default : EvaluateKey(Items.Keys, Items.Values, key, muti);


		public float GetAreaBetween (float keyA, float keyB, float muti = 1f) => Items.Count == 0 ? 0f : GetAreaBetweenKeys(keyA, keyB, muti);


		public float Fill (float key, float area, float muti = 1f) {

			if (Items.Count == 0 || area == 0f) { return key; }

			float deltaArea;
			bool right = area > 0f;
			area = right ? area : -area;
			var itemKeys = Items.Keys;
			var itemValues = Items.Values;

			// Start Overlap
			if (key < itemKeys[0]) {
				float firstValue = itemValues[0].Value * muti;
				if (right) {
					// Right-Moving Left Overlap
					deltaArea = (itemKeys[0] - key) * firstValue;
					if (area >= deltaArea) {
						area -= deltaArea;
						key = itemKeys[0];
					} else {
						return key + Abs(area / firstValue);
					}
				} else {
					// Left-Moving Left Overlap
					return firstValue > 0f ? key - (area / firstValue) : key;
				}
			} else if (key > itemKeys[Items.Count - 1]) {
				float lastValue = itemValues[Items.Count - 1].Value * muti;
				if (right) {
					// Right-Moving Right Overlap
					return lastValue > 0f ? key + (area / lastValue) : key;
				} else {
					// Left-Moving Right Overlap
					deltaArea = (key - itemKeys[Items.Count - 1]) * lastValue;
					if (area >= deltaArea) {
						area -= deltaArea;
						key = itemKeys[Items.Count - 1];
					} else {
						return key - Abs(area / lastValue);
					}
				}
			}

			// Main
			if (Items.Count > 1) {
				float keyAlt;
				float value = EvaluateKey(itemKeys, itemValues, key, muti);
				float valueAlt;
				bool rootFlag = false;
				for (
					int index = right ? Search(itemKeys, key) + 1 : Search(itemKeys, key);
					index >= 0 && index < Items.Count;
					index += right ? 1 : -1
				) {
					keyAlt = itemKeys[index];
					valueAlt = Items[keyAlt].Value * muti;
					if (value > 0f && valueAlt < 0f) {
						deltaArea = GetAreaAt(right ? index - 1 : index, true, muti);
						float prevKey = itemKeys[right ? index - 1 : index + 1];
						deltaArea -= 0.5f * Abs(key - prevKey) * (value + Items[prevKey].Value * muti);
						if (area <= deltaArea) {
							rootFlag = true;
						}
					}
					if (!rootFlag) {
						deltaArea = GetAreaBetweenKeys(key, keyAlt, muti);
						if (area <= deltaArea) {
							rootFlag = true;
						} else {
							area -= deltaArea;
							key = keyAlt;
							value = valueAlt;
						}
					}

					// End With Root
					if (rootFlag) {
						float slope = (valueAlt - value) / (keyAlt - key);
						if (Abs(slope) < 0.0001f) {
							return right ? key + area / value : key - area / value;
						} else {
							var delta = GetEquationSolution(
								right ? slope : -slope,
								2f * value,
								-2f * area,
								0f,
								(right ? 1f : -1f) * (keyAlt - key)
							);
							if (!delta.HasValue) { return key; }
							return right ? key + delta.Value : key - delta.Value;
						}
					}
				}
			}

			// End Overlap
			if (area > 0f) {
				float firstValue = itemValues[0].Value * muti;
				float lastValue = itemValues[Items.Count - 1].Value * muti;
				if (right && lastValue > 0f) {
					// Right-Moving Overlap
					return itemKeys[Items.Count - 1] + (area / lastValue);
				} else if (!right && firstValue > 0f) {
					// Left-Moving Overlap
					return itemKeys[0] - (area / firstValue);
				}
			}

			return key;
		}


		// Set
		public void SetValue (float key, float value) {
			Items[key] = (value, null, null);
			ClearAreas(key);
		}


		public void SetValueAt (int index, float value) {
			Items[Items.Keys[index]] = (value, null, null);
			ClearAreasAt(index);
		}


		public void Add (float key, float value) {
			Items.Add(key, (value, null, null));
			ClearAreas(key);
		}


		public void Remove (float key) {
			ClearAreas(key);
			Items.Remove(key);
		}


		public void RemoveAt (int index) {
			ClearAreasAt(index);
			Items.RemoveAt(index);
		}


		public void RemoveRange (int newCount) {
			int oldCount = Items.Count;
			for (int i = newCount; i < oldCount; i++) {
				ClearAreasAt(0);
				Items.RemoveAt(0);
			}
		}


		public void AddSet (float key, float value) {
			if (Items.ContainsKey(key)) {
				Items[key] = (value, null, null);
			} else {
				Items.Add(key, (value, null, null));
			}
			ClearAreas(key);
		}


		public void Clear () => Items.Clear();


		#endregion




		#region --- LGC ---


		// Get
		private float GetAreaAt (int index, bool positiveOnly, float muti) {
			var itemKeys = Items.Keys;
			float key = itemKeys[index];
			var pair = Items[key];
			if (!pair.Area.HasValue) {
				if (index < Items.Count - 1) {
					float nextKey = itemKeys[index + 1];
					float value = (Items[key].Value);
					float nextValue = (Items[nextKey].Value);
					pair.Area = 0.5f * (nextKey - key) * (value + nextValue);
					if (value >= 0f == nextValue >= 0f) {
						pair.PositiveArea = Max(pair.Area.Value, 0f);
					} else if (Abs(pair.Area.Value) < 0.0001f) {
						pair.PositiveArea = 0.25f * (nextKey - key) * Max(value, nextValue);
					} else {
						pair.PositiveArea = value > nextValue ?
							(pair.Area * value * value) / (value * value - nextValue * nextValue) :
							(pair.Area * nextValue * nextValue) / (nextValue * nextValue - value * value);
					}
				} else {
					pair.Area = 0f;
					pair.PositiveArea = 0f;
				}
				Items[key] = pair;
			}
			return positiveOnly ? pair.PositiveArea.Value * muti : pair.Area.Value * muti;
		}


		private float EvaluateKey (IList<float> itemKeys, IList<(float Value, float? Area, float? PositiveArea)> itemValues, float key, float muti) {
			if (key <= itemKeys[0]) {
				return itemValues[0].Value * muti;
			} else if (key >= itemKeys[Items.Count - 1]) {
				return itemValues[Items.Count - 1].Value * muti;
			} else {
				if (Items.ContainsKey(key)) {
					return Items[key].Value * muti;
				} else {
					int indexL = Search(itemKeys, key);
					float keyL = itemKeys[indexL];
					float keyR = itemKeys[indexL + 1];
					float valueL = Items[keyL].Value;
					return ((Items[keyR].Value - valueL) * (key - keyL) / (keyR - keyL) + valueL) * muti;
				}
			}
		}


		private float GetAreaBetweenKeys (float keyA, float keyB, float muti) {
			if (keyA == keyB) {
				return 0f;
			} else if (keyA > keyB) {
				return GetAreaBetweenKeys(keyB, keyA, muti);
			} else {
				var itemKeys = Items.Keys;
				var itemValues = Items.Values;
				float area = 0f;
				int indexA = Search(itemKeys, keyA);
				int indexB = Search(itemKeys, keyB);
				if (indexA == indexB) {
					return 0.5f * (keyB - keyA) * ((EvaluateKey(itemKeys, itemValues, keyA, muti)) + (EvaluateKey(itemKeys, itemValues, keyB, muti)));
				} else {
					float keyL = itemKeys[0];
					float keyR = itemKeys[Items.Count - 1];
					if (keyA < keyL) {
						area += itemValues[0].Value * muti * (keyL - keyA);
						keyA = keyL;
					}
					if (keyB > keyR) {
						area += itemValues[Items.Count - 1].Value * muti * (keyB - keyR);
						keyB = keyR;
					}
					keyL = itemKeys[indexA + 1];
					keyR = itemKeys[indexB];
					if (keyL > keyA) {
						area += 0.5f * (keyL - keyA) * (Items[keyL].Value * muti + (EvaluateKey(itemKeys, itemValues, keyA, muti)));
					}
					if (keyB > keyR) {
						area += 0.5f * (keyB - keyR) * (Items[keyR].Value * muti + (EvaluateKey(itemKeys, itemValues, keyB, muti)));
					}
					for (int i = indexA + 1; i < indexB; i++) {
						area += GetAreaAt(i, false, muti);
					}
					return area;
				}
			}
		}


		// Set
		private void ClearAreasAt (int index) => ClearAreas(Items.Keys[index], index);


		private void ClearAreas (float key, int index = -1) {
			var pair = Items[key];
			pair.Area = null;
			pair.PositiveArea = null;
			Items[key] = pair;
			index = index >= 0 ? index : Items.Keys.IndexOf(key);
			if (index > 0) {
				key = Items.Keys[index - 1];
				pair = Items[key];
				pair.Area = null;
				pair.PositiveArea = null;
				Items[key] = pair;
			}
		}


		#endregion




		#region --- UTL ---


		private int Search (IList<float> itemKeys, float key) {
			int start = 0;
			int end = Items.Count - 1;
			int mid;
			while (start <= end) {
				mid = (start + end) / 2;
				if (itemKeys[mid] < key) {
					start = mid + 1;
				} else if (itemKeys[mid] > key) {
					end = mid - 1;
				} else {
					return mid;
				}
			}
			return (start + end) / 2;
		}


		private float Abs (float number) => System.Math.Abs(number);


		private float Min (float a, float b) => System.Math.Min(a, b);
		private float Max (float a, float b) => System.Math.Max(a, b);


		private float? GetEquationSolution (float a, float b, float c, float limitL, float limitR) {
			float dt = b * b - 4f * a * c;
			if (dt < 0) {
				return null;
			} else if (dt == 0) {
				return -b / (2f * a);
			} else {
				float x1 = (-b - (float)System.Math.Sqrt(dt)) / (2f * a);
				float x2 = (-b + (float)System.Math.Sqrt(dt)) / (2f * a);
				if (x1 >= limitL && x1 <= limitR) {
					if (x2 >= limitL && x2 <= limitR) {
						return Min(x1, x2);
					} else {
						return x1;
					}
				} else if (x2 >= limitL && x2 <= limitR) {
					return x2;
				} else {
					return null;
				}
			}
		}


		#endregion


	}
}