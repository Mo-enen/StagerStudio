namespace StagerStudio.LinerCurve {
	using System.Collections.Generic;


	public abstract class LinerCurve<Item> {




		#region --- VAR ---


		// API
		public int Count => Items.Count;


		// Data
		private SortedList<float, (Item Value, float? Area, float? PositiveArea)> Items = new SortedList<float, (Item Value, float? Area, float? PositiveArea)>();


		#endregion




		#region --- API ---


		// Init
		public void Load (SortedList<float, Item> items) {
			Items.Clear();
			foreach (var pair in items) {
				Items.Add(pair.Key, (pair.Value, null, null));
			}
		}


		public void Load (Dictionary<float, Item> items) {
			Items.Clear();
			foreach (var pair in items) {
				Items.Add(pair.Key, (pair.Value, null, null));
			}
		}


		public void Load (List<(float, Item)> items) {
			Items.Clear();
			foreach (var pair in items) {
				if (Items.ContainsKey(pair.Item1)) { continue; }
				Items.Add(pair.Item1, (pair.Item2, null, null));
			}
		}


		// Get
		public bool ContainsKey (float key) => Items.ContainsKey(key);


		public Item GetValue (float key) => Items[key].Value;


		public Item GetValueAt (int index) => Items.Values[index].Value;


		public float GetKeyAt (int index) => Items.Keys[index];


		public int IndexOf (float key) => Items.IndexOfKey(key);


		public int SearchKey (float key) => Search(key);


		public Item Evaluate (float key, float muti = 1f) => Items.Count == 0 ? default : EvaluateKey(key, muti);


		public float GetAreaBetween (float keyA, float keyB, float muti = 1f) => Items.Count == 0 ? 0f : GetAreaBetweenKeys(keyA, keyB, muti);


		public float Fill (float key, float area, float muti = 1f) {

			if (Items.Count == 0 || area == 0f) { return key; }

			float deltaArea;
			bool right = area > 0f;
			area = right ? area : -area;

			// Start Overlap
			if (key < Items.Keys[0]) {
				float firstValue = ItemToFloat(ItemMuti(Items.Values[0].Value, muti));
				if (right) {
					// Right-Moving Left Overlap
					deltaArea = (Items.Keys[0] - key) * firstValue;
					if (area >= deltaArea) {
						area -= deltaArea;
						key = Items.Keys[0];
					} else {
						return key + Abs(area / firstValue);
					}
				} else {
					// Left-Moving Left Overlap
					return firstValue > 0f ? key - (area / firstValue) : key;
				}
			} else if (key > Items.Keys[Items.Count - 1]) {
				float lastValue = ItemToFloat(ItemMuti(Items.Values[Items.Count - 1].Value, muti));
				if (right) {
					// Right-Moving Right Overlap
					return lastValue > 0f ? key + (area / lastValue) : key;
				} else {
					// Left-Moving Right Overlap
					deltaArea = (key - Items.Keys[Items.Count - 1]) * lastValue;
					if (area >= deltaArea) {
						area -= deltaArea;
						key = Items.Keys[Items.Count - 1];
					} else {
						return key - Abs(area / lastValue);
					}
				}
			}

			// Main
			if (Items.Count > 1) {
				float keyAlt;
				float value = ItemToFloat(EvaluateKey(key, muti));
				float valueAlt;
				bool rootFlag = false;
				for (
					int index = right ? Search(key) + 1 : Search(key);
					index >= 0 && index < Items.Count;
					index += right ? 1 : -1
				) {
					keyAlt = Items.Keys[index];
					valueAlt = ItemToFloat(ItemMuti(Items[keyAlt].Value, muti));
					if (value > 0f && valueAlt < 0f) {
						deltaArea = GetAreaAt(right ? index - 1 : index, true, muti);
						float prevKey = Items.Keys[right ? index - 1 : index + 1];
						deltaArea -= 0.5f * Abs(key - prevKey) * (value + ItemToFloat(ItemMuti(Items[prevKey].Value, muti)));
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
				float firstValue = ItemToFloat(ItemMuti(Items.Values[0].Value, muti));
				float lastValue = ItemToFloat(ItemMuti(Items.Values[Items.Count - 1].Value, muti));
				if (right && lastValue > 0f) {
					// Right-Moving Overlap
					return Items.Keys[Items.Count - 1] + (area / lastValue);
				} else if (!right && firstValue > 0f) {
					// Left-Moving Overlap
					return Items.Keys[0] - (area / firstValue);
				}
			}

			return key;
		}


		// Set
		public void SetValue (float key, Item value) {
			Items[key] = (value, null, null);
			ClearAreas(key);
		}


		public void SetValueAt (int index, Item value) {
			Items[Items.Keys[index]] = (value, null, null);
			ClearAreasAt(index);
		}


		public void Add (float key, Item value) {
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


		public void AddSet (float key, Item value) {
			if (Items.ContainsKey(key)) {
				Items[key] = (value, null, null);
			} else {
				Items.Add(key, (value, null, null));
			}
			ClearAreas(key);
		}


		public void Clear () => Items.Clear();


		#endregion




		#region --- ABS ---


		protected abstract float ItemToFloat (Item item);


		protected abstract Item Lerp (Item l, Item r, float t);


		protected abstract Item ItemMuti (Item item, float muti);


		#endregion




		#region --- LGC ---


		// Get
		private float GetAreaAt (int index, bool positiveOnly, float muti) {
			float key = Items.Keys[index];
			var pair = Items[key];
			if (!pair.Area.HasValue) {
				if (index < Items.Count - 1) {
					float nextKey = Items.Keys[index + 1];
					float value = ItemToFloat(Items[key].Value);
					float nextValue = ItemToFloat(Items[nextKey].Value);
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


		private Item EvaluateKey (float key, float muti) {
			if (key <= Items.Keys[0]) {
				return ItemMuti(Items.Values[0].Value, muti);
			} else if (key >= Items.Keys[Items.Count - 1]) {
				return ItemMuti(Items.Values[Items.Count - 1].Value, muti);
			} else {
				if (Items.ContainsKey(key)) {
					return ItemMuti(Items[key].Value, muti);
				} else {
					int indexL = Search(key);
					float keyL = Items.Keys[indexL];
					float keyR = Items.Keys[indexL + 1];
					return ItemMuti(Lerp(
						Items[keyL].Value,
						Items[keyR].Value,
						(key - keyL) / (keyR - keyL)
					), muti);
				}
			}
		}


		private float GetAreaBetweenKeys (float keyA, float keyB, float muti) {
			if (keyA == keyB) {
				return 0f;
			} else if (keyA > keyB) {
				return GetAreaBetweenKeys(keyB, keyA, muti);
			} else {
				float area = 0f;
				int indexA = Search(keyA);
				int indexB = Search(keyB);
				if (indexA == indexB) {
					return 0.5f * (keyB - keyA) * (ItemToFloat(EvaluateKey(keyA, muti)) + ItemToFloat(EvaluateKey(keyB, muti)));
				} else {
					float keyL = Items.Keys[0];
					float keyR = Items.Keys[Items.Count - 1];
					if (keyA < keyL) {
						area += ItemToFloat(ItemMuti(Items.Values[0].Value, muti)) * (keyL - keyA);
						keyA = keyL;
					}
					if (keyB > keyR) {
						area += ItemToFloat(ItemMuti(Items.Values[Items.Count - 1].Value, muti)) * (keyB - keyR);
						keyB = keyR;
					}
					keyL = Items.Keys[indexA + 1];
					keyR = Items.Keys[indexB];
					if (keyL > keyA) {
						area += 0.5f * (keyL - keyA) * (ItemToFloat(ItemMuti(Items[keyL].Value, muti)) + ItemToFloat(EvaluateKey(keyA, muti)));
					}
					if (keyB > keyR) {
						area += 0.5f * (keyB - keyR) * (ItemToFloat(ItemMuti(Items[keyR].Value, muti)) + ItemToFloat(EvaluateKey(keyB, muti)));
					}
					for (int i = indexA + 1; i < indexB; i++) {
						area += GetAreaAt(i, false, muti);
					}
					return area;
				}
			}
		}


		// Set
		private void ClearAllAreas () {
			for (int i = 0; i < Items.Count; i++) {
				ClearAreasAt(i);
			}
		}


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


		private int Search (float key) {
			int start = 0;
			int end = Items.Count - 1;
			int mid;
			while (start <= end) {
				mid = (start + end) / 2;
				if (Items.Keys[mid] < key) {
					start = mid + 1;
				} else if (Items.Keys[mid] > key) {
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