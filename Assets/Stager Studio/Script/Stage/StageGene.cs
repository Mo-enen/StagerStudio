namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;



	public class StageGene : MonoBehaviour {




		#region --- SUB ---


		// Handler
		public delegate GeneData GeneHandler ();
		public delegate Beatmap BeatmapHandler ();
		public delegate void EyeLockHandler (int index, bool use);


		#endregion




		#region --- VAR ---


		// Handler
		public static GeneHandler GetGene { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static EyeLockHandler SetContainerActive { get; set; } = null;
		public static EyeLockHandler SetUseLock { get; set; } = null;

		// Ser
		[SerializeField] private RectTransform m_StageBrushRoot = null;
		[SerializeField] private RectTransform m_TrackBrushRoot = null;
		[SerializeField] private RectTransform m_NoteBrushRoot = null;
		[SerializeField] private RectTransform m_TimingBrushRoot = null;


		#endregion




		#region --- MSG ---





		#endregion




		#region --- API ---


		public void RefreshUI () {

			var gene = GetGene();

			m_StageBrushRoot.gameObject.TrySetActive(gene.StageAccessable);
			m_TrackBrushRoot.gameObject.TrySetActive(gene.TrackAccessable);
			m_NoteBrushRoot.gameObject.TrySetActive(gene.NoteAccessable);
			m_TimingBrushRoot.gameObject.TrySetActive(gene.TimingAccessable);

			if (!gene.StageAccessable) {
				SetContainerActive(0, true);
				SetUseLock(0, true);
			}
			if (!gene.TrackAccessable) {
				SetContainerActive(1, true);
				SetUseLock(1, true);
			}
			if (!gene.NoteAccessable) {
				SetContainerActive(2, true);
				SetUseLock(2, true);
			}
			if (!gene.TimingAccessable) {
				SetContainerActive(3, true);
				SetUseLock(3, true);
			}

		}


		public void FixMapFromGene () {
			var map = GetBeatmap();
			if (map == null) { return; }





		}


		public void FixItemFromGene (int itemType, int itemIndex) {
			var map = GetBeatmap();
			if (map == null) { return; }





		}


		public int FixBrushIndexFromGene (int brushIndex) {
			var gene = GetGene();
			switch (brushIndex) {
				case 0: // Stage
					if (!gene.StageAccessable) {
						brushIndex = -1;
					}
					break;
				case 1: // Track
					if (!gene.TrackAccessable) {
						brushIndex = -1;
					}
					break;
				case 2: // Note
					if (!gene.NoteAccessable) {
						brushIndex = -1;
					}
					break;
				case 3: // Timing
					if (!gene.TimingAccessable) {
						brushIndex = -1;
					}
					break;
			}
			return brushIndex;
		}


		public bool FixContainerFromGene (int index, bool active) {
			var gene = GetGene();
			switch (index) {
				case 0:
					active = !gene.StageAccessable || active;
					break;
				case 1:
					active = !gene.TrackAccessable || active;
					break;
				case 2:
					active = !gene.NoteAccessable || active;
					break;
				case 3:
					active = !gene.TimingAccessable || active;
					break;
			}
			return active;
		}


		public bool FixLockFromGene (int index, bool locked) {
			var gene = GetGene();
			switch (index) {
				case 0:
					locked = !gene.StageAccessable || locked;
					break;
				case 1:
					locked = !gene.TrackAccessable || locked;
					break;
				case 2:
					locked = !gene.NoteAccessable || locked;
					break;
				case 3:
					locked = !gene.TimingAccessable || locked;
					break;
			}
			return locked;
		}






		#endregion




		#region --- LGC ---




		#endregion




	}
}