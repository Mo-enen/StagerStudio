namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class StageUndo {




		#region --- SUB ---


		public class StepData {
			public byte[] UndoData;
		}


		public delegate object ObjectDataHandler ();
		public delegate void VoidBytesHandler (byte[] step);


		#endregion




		#region --- VAR ---

		// Handler
		public static ObjectDataHandler GetObjectData { get; set; } = null;
		public static VoidBytesHandler OnUndo { get; set; } = null;

		// Data
		private readonly static Stack<StepData> UndoSteps = new Stack<StepData>();
		private readonly static Stack<StepData> RedoSteps = new Stack<StepData>();
		private static bool IsDirty = false;


		#endregion




		#region --- MSG ---


		public static void GlobalUpdate () {
			if (IsDirty && !Input.anyKey) {
				IsDirty = false;
				var obj = GetObjectData();
				RedoSteps.Clear();
				UndoSteps.Push(new StepData() {
					UndoData = obj is null ? null : Util.ObjectToBytes(obj),
				});
			}
		}


		#endregion




		#region --- API ---


		public static void RegisterUndo () => IsDirty = true;


		public static void ClearUndo () {
			UndoSteps.Clear();
			RedoSteps.Clear();
		}


		public static void Undo () {
			if (UndoSteps.Count < 2) { return; }
			var step = UndoSteps.Pop();
			RedoSteps.Push(step);
			step = UndoSteps.Peek();
			if (!(step is null)) {
				OnUndo(step.UndoData);
			}
		}


		public static void Redo () {
			if (RedoSteps.Count < 1) { return; }
			var step = RedoSteps.Pop();
			UndoSteps.Push(step);
			if (!(step is null)) {
				OnUndo(step.UndoData);
			}
		}


		#endregion




	}
}