namespace UndoRedo {
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;


	public static class UndoRedo {




		#region --- SUB ---


		public delegate object ObjectDataHandler ();
		public delegate void UndoRedoHandler (object step);


		#endregion




		#region --- VAR ---


		// Const
		private const int DEFAULT_CACHE_LENGTH = 1024 * 1024 * 100; // 100MB

		// Handler
		public static ObjectDataHandler GetStepData { get; set; } = null;
		public static UndoRedoHandler OnUndoRedo { get; set; } = null;

		// Api
		public static int CacheLength {
			get {
				return UndoCache.Length;
			}
			set {
				UndoCache = new byte[System.Math.Max(value, 0)];
				ClearUndoLogic();
			}
		}
		public static byte[] UndoCache { get; private set; } = new byte[DEFAULT_CACHE_LENGTH];
		public static bool IsDirty { get; private set; } = false;

		// Data
		private readonly static List<(int start, int length)> UndoSteps = new List<(int start, int length)>();
		private readonly static List<(int start, int length)> RedoSteps = new List<(int start, int length)>();
		private static int CacheStart = 0;
		private static int CacheEnd = DEFAULT_CACHE_LENGTH - 1;


		#endregion




		#region --- API ---


		public static void RegisterUndoIfDirty () {
			if (IsDirty) {
				RegisteredUndoLogic();
			}
		}


		public static void RegisterUndo () => RegisteredUndoLogic();


		public static void SetDirty () => IsDirty = true;


		public static void ClearUndo () => ClearUndoLogic();


		public static void Undo () => UndoLogic();


		public static void Redo () => RedoLogic();


		#endregion




		#region --- LGC ---


		private static void UndoLogic () {
			if (UndoSteps.Count < 2) { return; }
			var (start, len) = UndoSteps[UndoSteps.Count - 1];
			UndoSteps.RemoveAt(UndoSteps.Count - 1);
			RedoSteps.Add((start, len));
			(start, len) = UndoSteps[UndoSteps.Count - 1];
			var stepObj = BytesToObject(start, len);
			if (stepObj != null) {
				OnUndoRedo(stepObj);
			}
		}


		public static void RedoLogic () {
			if (RedoSteps.Count < 1) { return; }
			var (start, len) = RedoSteps[RedoSteps.Count - 1];
			RedoSteps.RemoveAt(RedoSteps.Count - 1);
			UndoSteps.Add((start, len));
			var stepObj = BytesToObject(start, len);
			if (stepObj != null) {
				OnUndoRedo(stepObj);
			}
		}


		private static void ClearUndoLogic () {
			UndoSteps.Clear();
			RedoSteps.Clear();
			CacheStart = 0;
			CacheEnd = UndoCache.Length - 1;
		}


		private static void RegisteredUndoLogic () {
			IsDirty = false;
			var obj = GetStepData();
			int oldStart = CacheStart;
			int newStart = ObjectToBytes(obj, CacheStart);
			if (newStart <= oldStart || newStart >= UndoCache.Length) {
				// Reach End
				oldStart = CacheStart = CacheEnd = 0;
				newStart = ObjectToBytes(obj, CacheStart);
			}
			if (newStart > oldStart) {
				// Normal
				CacheStart = newStart;
				if (CacheStart >= CacheEnd) {
					for (int i = 0; i < UndoSteps.Count && CacheStart >= CacheEnd; i++) {
						var (start, length) = UndoSteps[i];
						int end = start + length;
						if (start <= CacheStart && start + length >= CacheEnd) {
							UndoSteps.RemoveAt(i);
							CacheEnd = UndoSteps.Count > 0 ? end : UndoCache.Length - 1;
							i--;
						}
					}
				}
				RedoSteps.Clear();
				UndoSteps.Add((oldStart, newStart - oldStart));
			}
		}


		#endregion



		#region --- UTL ---


		private static object BytesToObject (int offset, int len) {
			if (UndoCache == null || UndoCache.Length == 0) { return null; }
			try {
				using (var memStream = new MemoryStream()) {
					memStream.Write(UndoCache, offset, len);
					memStream.Seek(0, SeekOrigin.Begin);
					var obj = new BinaryFormatter().Deserialize(memStream);
					return obj;
				}
			} catch { }
			return null;
		}


		private static int ObjectToBytes (object obj, int offset) {
			if (obj == null) { return offset; }
			int len = 0;
			int realLen = 0;
			try {
				using (var ms = new MemoryStream()) {
					new BinaryFormatter().Serialize(ms, obj);
					ms.Seek(0, SeekOrigin.Begin);
					len = (int)ms.Length;
					if (offset + len < UndoCache.Length) {
						realLen = ms.Read(UndoCache, offset, len);
						if (realLen == 0) {
							realLen = len;
						}
					}
				}
			} catch { }
			return len == realLen ? offset + len : offset;
		}


		#endregion



	}
}