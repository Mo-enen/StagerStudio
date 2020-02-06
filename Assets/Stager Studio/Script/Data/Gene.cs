namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;



	public class Gene {




		#region --- SUB ---



		public enum PermissionType {

			Allow_Stage_Edit,
			Allow_Stage_Color,

			Allow_Track_Edit,
			Allow_Track_Stageless,
			Allow_Track_Color,
			Allow_Track_Tray,

			Allow_Note_Edit,
			Allow_Note_Trackless,
			Allow_Note_Link,
			Allow_Note_Duration,
			Allow_Note_SwipeX,
			Allow_Note_SwipeY,
			Allow_Note_Tapless,
			Allow_Note_ClickSound,

			Allow_MotionNote_Edit,

			Allow_SpeedNote_Edit,
		}



		public enum RangeType {

			Range_Map_Ratio,
			Range_Map_Speed,

			Range_Stage_PositionX,
			Range_Stage_PositionY,
			Range_Stage_Rotation,
			Range_Stage_Size,
			Range_Stage_Speed,

			Range_Track_Position,
			Range_Track_Rotation,
			Range_Track_Width,
			Range_Track_TrayPosition,

			Range_Note_Position,
			Range_Note_Width,
		}



		public enum LimitationType {
			Limit_Stage,
			Limit_Track,
			Limit_Note,
		}



		[System.Serializable]
		public struct GeneJsonData {
			[System.Serializable]
			public struct PermissionData {
				public string Type;
				public bool Allow;
				public PermissionData (PermissionType type, bool allow) {
					Type = type.ToString();
					Allow = allow;
				}
			}
			[System.Serializable]
			public struct RangeData {
				public string Type;
				public Range Range;
				public RangeData (RangeType type, Range range) {
					Type = type.ToString();
					Range = range;
				}
			}
			[System.Serializable]
			public struct LimitationData {
				public string Type;
				public int Limitation;
				public LimitationData (LimitationType type, int limitation) {
					Type = type.ToString();
					Limitation = limitation;
				}
			}
			public PermissionData[] Permissions;
			public RangeData[] Ranges;
			public LimitationData[] Limitations;
		}



		#endregion




		#region --- Ser ---


		private readonly Dictionary<PermissionType, bool> PermissionMap = new Dictionary<PermissionType, bool>();
		private readonly Dictionary<RangeType, Range> RangeMap = new Dictionary<RangeType, Range>();
		private readonly Dictionary<LimitationType, int> LimitationMap = new Dictionary<LimitationType, int>();


		#endregion




		#region --- API ---


		// Data
		public static Gene JsonToGene (string json) {
			try {
				var gData = JsonUtility.FromJson<GeneJsonData>(json);
				var gene = new Gene();
				foreach (var pair in gData.Permissions) {
					if (System.Enum.TryParse(pair.Type, out PermissionType result) && !gene.PermissionMap.ContainsKey(result)) {
						gene.PermissionMap.Add(result, pair.Allow);
					}
				}
				foreach (var pair in gData.Ranges) {
					if (System.Enum.TryParse(pair.Type, out RangeType result) && !gene.RangeMap.ContainsKey(result)) {
						gene.RangeMap.Add(result, pair.Range);
					}
				}
				foreach (var pair in gData.Limitations) {
					if (System.Enum.TryParse(pair.Type, out LimitationType result) && !gene.LimitationMap.ContainsKey(result)) {
						gene.LimitationMap.Add(result, pair.Limitation);
					}
				}
				gene.FillEmptyData();
				return gene;
			} catch {
				return null;
			}
		}


		public static string GeneToJson (Gene gene) {
			try {
				gene.FillEmptyData();
				var pList = new List<GeneJsonData.PermissionData>();
				var rList = new List<GeneJsonData.RangeData>();
				var lList = new List<GeneJsonData.LimitationData>();
				foreach (var pair in gene.PermissionMap) {
					pList.Add(new GeneJsonData.PermissionData(pair.Key, pair.Value));
				}
				foreach (var pair in gene.RangeMap) {
					rList.Add(new GeneJsonData.RangeData(pair.Key, pair.Value));
				}
				foreach (var pair in gene.LimitationMap) {
					lList.Add(new GeneJsonData.LimitationData(pair.Key, pair.Value));
				}
				return JsonUtility.ToJson(new GeneJsonData() {
					Permissions = pList.ToArray(),
					Ranges = rList.ToArray(),
					Limitations = lList.ToArray(),
				}, false);
			} catch {
				return "";
			}
		}


		public void FillEmptyData () {
			int pLen = System.Enum.GetNames(typeof(PermissionType)).Length;
			int rLen = System.Enum.GetNames(typeof(RangeType)).Length;
			int lLen = System.Enum.GetNames(typeof(LimitationType)).Length;
			for (int i = 0; i < pLen; i++) {
				var type = (PermissionType)i;
				if (!PermissionMap.ContainsKey(type)) {
					PermissionMap.Add(type, true);
				}
			}
			for (int i = 0; i < rLen; i++) {
				var type = (RangeType)i;
				if (!RangeMap.ContainsKey(type)) {
					RangeMap.Add(type, Range.Unlimited);
				}
			}
			for (int i = 0; i < lLen; i++) {
				var type = (LimitationType)i;
				if (!LimitationMap.ContainsKey(type)) {
					LimitationMap.Add(type, -1);
				}
			}
		}


		// Get
		public bool GetPermission (string id) => System.Enum.TryParse(id, out PermissionType result) ? GetPermission(result) : true;
		public bool GetPermission (PermissionType type) => PermissionMap.ContainsKey(type) ? PermissionMap[type] : true;


		public Range GetRange (string id) => System.Enum.TryParse(id, out RangeType result) ? GetRange(result) : Range.Unlimited;
		public Range GetRange (RangeType type) => RangeMap.ContainsKey(type) ? RangeMap[type] : Range.Unlimited;


		public int GetLimitation (string id) => System.Enum.TryParse(id, out LimitationType result) ? GetLimitation(result) : -1;
		public int GetLimitation (LimitationType type) => LimitationMap.ContainsKey(type) ? LimitationMap[type] : -1;


		// Set
		public void SetPermisstion (string id, bool allow) {
			if (System.Enum.TryParse(id, out PermissionType result)) {
				SetPermisstion(result, allow);
			}
		}
		public void SetPermisstion (PermissionType type, bool allow) {
			if (PermissionMap.ContainsKey(type)) {
				PermissionMap[type] = allow;
			} else {
				PermissionMap.Add(type, allow);
			}
		}


		public void SetRange (string id, Range range) {
			if (System.Enum.TryParse(id, out RangeType result)) {
				SetRange(result, range);
			}
		}
		public void SetRange (RangeType type, Range range) {
			if (RangeMap.ContainsKey(type)) {
				RangeMap[type] = range;
			} else {
				RangeMap.Add(type, range);
			}
		}


		public void SetLimitation (string id, int limit) {
			if (System.Enum.TryParse(id, out LimitationType result)) {
				SetLimitation(result, limit);
			}
		}
		public void SetLimitation (LimitationType type, int limit) {
			if (LimitationMap.ContainsKey(type)) {
				LimitationMap[type] = limit;
			} else {
				LimitationMap.Add(type, limit);
			}
		}


		// Use
		public bool PermissionAllow (PermissionType type) => GetPermission(type);


		public float RangeClamp (RangeType type, float value) => GetRange(type).Clamp(value);


		public bool LimitationCheck (LimitationType type, int count) {
			int l = GetLimitation(type);
			return l == -1 || count <= l;
		}


		#endregion




	}
}