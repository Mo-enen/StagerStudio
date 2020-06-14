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
		public delegate float FloatHandler ();
		public delegate int IntHandler ();


		#endregion




		#region --- VAR ---


		// Handler
		public static GeneHandler GetGene { get; set; } = null;
		public static BeatmapHandler GetBeatmap { get; set; } = null;
		public static EyeLockHandler SetContainerActive { get; set; } = null;
		public static EyeLockHandler SetUseLock { get; set; } = null;
		public static FloatHandler GetMusicDuration { get; set; } = null;
		public static IntHandler GetSelectingItemType { get; set; } = null;
		public static IntHandler GetSelectingItemIndex { get; set; } = null;

		// Ser
		[Header("Brush")]
		[SerializeField] private RectTransform m_StageBrushRoot = null;
		[SerializeField] private RectTransform m_TrackBrushRoot = null;
		[SerializeField] private RectTransform m_NoteBrushRoot = null;
		[SerializeField] private RectTransform m_TimingBrushRoot = null;
		[Header("Inspector Map")]
		[SerializeField] private RectTransform m_Inspector_Map_Ratio = null;
		[SerializeField] private RectTransform[] m_Inspector_SFX = null;
		[SerializeField] private RectTransform[] m_Inspector_TimingID = null;
		[Header("Inspector Stage")]
		[SerializeField] private RectTransform m_Inspector_Stage_ItemType = null;
		[SerializeField] private RectTransform m_Inspector_Stage_ItemTypeSelector = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Time = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Beat = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Duration = null;
		[SerializeField] private RectTransform m_Inspector_Stage_X = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Y = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Rotation = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Width = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Height = null;
		[SerializeField] private RectTransform m_Inspector_Stage_PivotY = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Speed = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Motion_Pos = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Motion_Rot = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Motion_Color = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Motion_Width = null;
		[SerializeField] private RectTransform m_Inspector_Stage_Motion_Height = null;
		[Header("Inspector Track")]
		[SerializeField] private RectTransform m_Inspector_Track_ItemType = null;
		[SerializeField] private RectTransform m_Inspector_Track_ItemTypeSelector = null;
		[SerializeField] private RectTransform m_Inspector_Track_StageIndex = null;
		[SerializeField] private RectTransform m_Inspector_Track_Time = null;
		[SerializeField] private RectTransform m_Inspector_Track_Beat = null;
		[SerializeField] private RectTransform m_Inspector_Track_Duration = null;
		[SerializeField] private RectTransform m_Inspector_Track_X = null;
		[SerializeField] private RectTransform m_Inspector_Track_Angle = null;
		[SerializeField] private RectTransform m_Inspector_Track_Width = null;
		[SerializeField] private RectTransform m_Inspector_Track_Speed = null;
		[SerializeField] private RectTransform m_Inspector_Track_Tray = null;
		[SerializeField] private RectTransform m_Inspector_Track_Motion_X = null;
		[SerializeField] private RectTransform m_Inspector_Track_Motion_Angle = null;
		[SerializeField] private RectTransform m_Inspector_Track_Motion_Color = null;
		[SerializeField] private RectTransform m_Inspector_Track_Motion_Width = null;
		[Header("Inspector Note")]
		[SerializeField] private RectTransform m_Inspector_Note_ItemType = null;
		[SerializeField] private RectTransform m_Inspector_Note_ItemTypeSelector = null;
		[SerializeField] private RectTransform m_Inspector_Note_X = null;
		[SerializeField] private RectTransform m_Inspector_Note_Z = null;
		[SerializeField] private RectTransform m_Inspector_Note_Duration = null;
		[SerializeField] private RectTransform m_Inspector_Note_Width = null;
		[SerializeField] private RectTransform m_Inspector_Note_LinkIndex = null;
		[SerializeField] private RectTransform m_Inspector_Note_Speed = null;
		[SerializeField] private RectTransform m_Inspector_Note_ClickSound = null;


		#endregion




		#region --- MSG ---


		private void Update () {
			var map = GetBeatmap();
			if (map == null) { return; }
			// Stage
			var gene = GetGene();
			int staticStageCount = gene.StaticConfigs_Stage.Length;
			if (staticStageCount > 0 && map.Stages.Count != staticStageCount) {
				var stages = map.stages;
				int count = stages.Count;
				if (count > staticStageCount) {
					while (stages.Count > staticStageCount) {
						stages.RemoveAt(stages.Count - 1);
					}
				} else if (count < staticStageCount) {
					var stage = new Beatmap.Stage();
					FixStageLogic(map, gene, stage, stages.Count);
					stages.Add(stage);
				}
			}
			// Track
			int staticTrackCount = gene.StaticConfigs_Track.Length;
			if (staticTrackCount > 0 && map.Tracks.Count != staticTrackCount) {
				var tracks = map.tracks;
				int count = tracks.Count;
				if (count > staticTrackCount) {
					while (tracks.Count > staticTrackCount) {
						tracks.RemoveAt(tracks.Count - 1);
					}
				} else if (count < staticTrackCount) {
					tracks.Add(new Beatmap.Track());
				}
				count = tracks.Count;
				for (int i = 0; i < count; i++) {
					FixTrackLogic(map, gene, tracks[i], i);
				}
			}
		}


		#endregion




		#region --- API ---


		// Refresh
		public void RefreshGene () {

			var gene = GetGene();

			// Brush Root
			m_StageBrushRoot.gameObject.TrySetActive(gene.StageAccessable);
			m_TrackBrushRoot.gameObject.TrySetActive(gene.TrackAccessable);
			m_NoteBrushRoot.gameObject.TrySetActive(gene.NoteAccessable);
			m_TimingBrushRoot.gameObject.TrySetActive(gene.TimingAccessable);

			// Eye Lock
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

			// Inspector
			m_Inspector_Map_Ratio.TrySetActive(!gene.Ratio.Active);
			foreach (var sfx in m_Inspector_SFX) {
				sfx.TrySetActive(gene.SfxAccessable);
			}
			foreach (var tID in m_Inspector_TimingID) {
				tID.TrySetActive(gene.TimingAccessable);
			}
		}


		public void RefreshInspector () {
			switch (GetSelectingItemType()) {
				case 0: RefreshStageInspector(); break;
				case 1: RefreshTrackInspector(); break;
				case 2: RefreshNoteInspector(); break;
			}
		}


		// Fix Map Item
		public void FixMapDataFromGene () {
			var map = GetBeatmap();
			if (map == null) { return; }
			var gene = GetGene();
			int stageCount = map.Stages.Count;
			int trackCount = map.Tracks.Count;
			int noteCount = map.Notes.Count;
			int timingCount = map.Timings.Count;
			for (int i = 0; i < stageCount; i++) {
				FixStageLogic(map, gene, map.Stages[i], i);
			}
			for (int i = 0; i < trackCount; i++) {
				FixTrackLogic(map, gene, map.Tracks[i], i);
			}
			for (int i = 0; i < noteCount; i++) {
				FixNoteLogic(gene, map.Notes[i]);
			}
			for (int i = 0; i < timingCount; i++) {
				FixTimingLogic(gene, map.Timings[i]);
			}
		}


		public void FixMapInfoFromGene () {
			var map = GetBeatmap();
			if (map == null) { return; }
			var gene = GetGene();
			if (gene.Ratio.Active) {
				map.ratio = gene.Ratio.Value;
			}
		}


		public void FixItemFromGene (int itemType, int itemIndex) {
			var map = GetBeatmap();
			if (map == null) { return; }
			var gene = GetGene();
			switch (itemType) {
				case 0: // Stage
					if (itemIndex >= 0 && itemIndex < map.Stages.Count) {
						FixStageLogic(map, gene, map.Stages[itemIndex], itemIndex);
					}
					break;
				case 1: // Track
					if (itemIndex >= 0 && itemIndex < map.Tracks.Count) {
						var track = map.Tracks[itemIndex];
						FixTrackLogic(map, gene, track, itemIndex);
						if (track.stageIndex >= 0 && track.stageIndex < gene.StaticConfigs_Stage.Length && gene.StaticConfigs_Stage[track.stageIndex].TileTrack) {
							FixAllStagesFromGeneLogic(map, gene);
						}
					}
					break;
				case 2: // Note
					if (itemIndex >= 0 && itemIndex < map.Notes.Count) {
						FixNoteLogic(gene, map.Notes[itemIndex]);
					}
					break;
				case 3: // Timing
					if (itemIndex >= 0 && itemIndex < map.Timings.Count) {
						FixTimingLogic(gene, map.Timings[itemIndex]);
					}
					break;
			}
		}


		public void FixAllStagesFromGene () {
			var map = GetBeatmap();
			if (map == null) { return; }
			var gene = GetGene();
			FixAllStagesFromGeneLogic(map, gene);
		}


		// Fix UI Logic
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


		public bool FixEditorAxis (int itemType, int configIndex, int axis) {
			if (configIndex < 0) { return false; }
			var gene = GetGene();
			switch (itemType) {
				case 0: { // Stage
					var config = GetStageConfig(gene, configIndex);
					return gene.StageAccessable && (!config.UseConfig || (
						axis == 0 ? !config.X.Active :
						axis == 1 ? !config.Y.Active :
						axis == 2 ? !config.X.Active || !config.Y.Active :
						axis == 3 ? !config.Width.Active :
						axis == 4 ? !config.Height.Active :
						axis != 5 || !config.Rotation.Active
					));
				}
				case 1: { // Track
					var config = GetTrackConfig(gene, configIndex);
					return gene.TrackAccessable && (!config.UseConfig || (
						axis == 0 ? !config.X.Active :
						axis == 1 ? !config.Time_Real.Active :
						axis == 2 ? !config.X.Active || !config.Time_Real.Active :
						axis == 3 ? !config.Width.Active :
						axis != 5 || !config.Angle.Active
					));
				}
				case 2: { // Note
					var config = GetNoteConfig(gene, configIndex);
					return gene.NoteAccessable && (!config.UseConfig || (
						axis == 0 ? !config.X.Active :
						axis == 3 ? !config.Width.Active :
						axis != 4 || !config.Duration.Active
					));
				}
				case 3: // Timing
					return gene.TimingAccessable;
			}
			return true;
		}


		public bool FixAllowNoteLink (int noteIndex) {
			if (noteIndex < 0) { return false; }
			var gene = GetGene();
			var map = GetBeatmap();
			if (map == null) { return false; }
			var config = GetNoteConfig(gene, map.GetParentIndex(2, noteIndex));
			return gene.NoteAccessable && !config.LinkedNoteIndex.Active;
		}


		// Check
		public bool CheckTileTrack (int stageIndex) => stageIndex >= 0 && GetStageConfig(GetGene(), stageIndex).TileTrack;


		#endregion




		#region --- LGC ---


		private GeneData.StageConfig GetStageConfig (GeneData gene, int index) => index >= 0 && index < gene.StaticConfigs_Stage.Length ? gene.StaticConfigs_Stage[index] : gene.Config_Stage;


		private GeneData.TrackConfig GetTrackConfig (GeneData gene, int index) => index >= 0 && index < gene.StaticConfigs_Track.Length ? gene.StaticConfigs_Track[index] : gene.Config_Track;


		private GeneData.NoteConfig GetNoteConfig (GeneData gene, int configIndex) => configIndex >= 0 && configIndex < gene.StaticConfigs_Note.Length ? gene.StaticConfigs_Note[configIndex] : gene.Config_Note;


		private void FixStageLogic (Beatmap map, GeneData gene, Beatmap.Stage stage, int index) {

			var config = GetStageConfig(gene, index);

			if (config.UseConfig) {
				if (config.Time_Real.Active) {
					stage.time = (int)(config.Time_Real.Value * GetMusicDuration());
				}
				if (config.Duration_Real.Active) {
					stage.duration = (int)(config.Duration_Real.Value * GetMusicDuration());
				}
				if (config.ItemType.Active) {
					stage.itemType = config.ItemType.Value;
				}
				if (config.X.Active) {
					stage.x = config.X.Value;
				}
				if (config.Y.Active) {
					stage.y = config.Y.Value;
				}
				if (config.Rotation.Active) {
					stage.rotation = config.Rotation.Value;
				}
				if (config.Width.Active) {
					stage.width = config.Width.Value;
				}
				if (config.Height.Active) {
					stage.height = config.Height.Value;
				}
				if (config.PivotY.Active) {
					stage.pivotY = config.PivotY.Value;
				}
				if (config.Speed.Active) {
					stage.speed = config.Speed.Value;
				}
				if (!config.Motion_Pos) {
					stage.positions.Clear();
				}
				if (!config.Motion_Rot) {
					stage.rotations.Clear();
				}
				if (!config.Motion_Width) {
					stage.widths.Clear();
				}
				if (!config.Motion_Height) {
					stage.heights.Clear();
				}
				if (!config.Motion_Color) {
					stage.colors.Clear();
				}
				// Tile Tracks
				if (config.TileTrack) {
					int currentTrackIndex = 0;
					int trackCount = map.Tracks.Count;
					int trackCountInStage = 0;
					for (int i = 0; i < trackCount; i++) {
						if (map.Tracks[i].StageIndex == index) {
							trackCountInStage++;
						}
					}
					for (int i = 0; i < trackCount && trackCountInStage > 0; i++) {
						var track = map.Tracks[i];
						if (track.StageIndex != index) { continue; }
						int trackWidth = 1000 / trackCountInStage;
						track.width = trackWidth;
						track.x = (int)(((float)currentTrackIndex / trackCountInStage) * 1000f + (trackWidth / 2f));
						currentTrackIndex++;
					}
				}
			}
		}


		private void FixTrackLogic (Beatmap map, GeneData gene, Beatmap.Track track, int index) {

			var config = GetTrackConfig(gene, index);

			if (config.UseConfig) {

				bool tiling = GetStageConfig(gene, track.stageIndex).TileTrack;

				if (config.Time_Real.Active) {
					track.time = (int)(config.Time_Real.Value * GetMusicDuration());
				}
				if (config.Duration_Real.Active) {
					track.duration = (int)(config.Duration_Real.Value * map.GetDuration(0, track.stageIndex));
				}
				if (config.ItemType.Active) {
					track.itemType = config.ItemType.Value;
				}
				if (config.X.Active && !tiling) {
					track.x = config.X.Value;
				}
				if (config.Angle.Active) {
					track.angle = config.Angle.Value;
				}
				if (config.Width.Active && !tiling) {
					track.width = config.Width.Value;
				}
				if (config.Speed.Active) {
					track.speed = config.Speed.Value;
				}
				if (config.StageIndex.Active) {
					track.stageIndex = config.StageIndex.Value;
				}
				if (config.HasTray.Active) {
					track.hasTray = config.HasTray.Value == 1;
				}
				if (!config.Motion_X) {
					track.xs.Clear();
				}
				if (!config.Motion_Angle) {
					track.angles.Clear();
				}
				if (!config.Motion_Width) {
					track.widths.Clear();
				}
				if (!config.Motion_Color) {
					track.colors.Clear();
				}
			}
		}


		private void FixNoteLogic (GeneData gene, Beatmap.Note note) {
			var config = GetNoteConfig(gene, note.trackIndex);
			if (config.UseConfig) {
				if (config.ItemType.Active) {
					note.itemType = config.ItemType.Value;
				}
				if (config.X.Active) {
					note.x = config.X.Value;
				}
				if (config.Z.Active) {
					note.z = config.Z.Value;
				}
				if (config.Duration.Active) {
					note.duration = config.Duration.Value;
				}
				if (config.Width.Active) {
					note.width = config.Width.Value;
				}
				if (config.LinkedNoteIndex.Active) {
					note.linkedNoteIndex = config.LinkedNoteIndex.Value;
				}
				if (!gene.SfxAccessable) {
					note.soundFxIndex = 0;
				}
				if (config.ClickSound.Active) {
					note.clickSoundIndex = (short)config.ClickSound.Value;
				}
				if (!gene.TimingAccessable) {
					note.TimingID = 0;
				}
			}
		}


		private void FixTimingLogic (GeneData gene, Beatmap.Timing timing) {
			if (!gene.TimingAccessable) {
				timing.Speed = 1f;
				timing.TimingID = 0;
			}
			if (!gene.SfxAccessable) {
				timing.soundFxIndex = 0;
			}
		}


		private void FixAllStagesFromGeneLogic (Beatmap map, GeneData gene) {
			int count = map.Stages.Count;
			for (int i = 0; i < count; i++) {
				FixStageLogic(map, gene, map.Stages[i], i);
			}
		}


		private void RefreshStageInspector () {
			int index = GetSelectingItemIndex();
			if (index < 0) { return; }
			var gene = GetGene();
			var config = GetStageConfig(gene, index);
			bool useConfig = config.UseConfig;
			m_Inspector_Stage_ItemType.TrySetActive(!useConfig || !config.ItemType.Active);
			m_Inspector_Stage_ItemTypeSelector.TrySetActive(!useConfig || !config.ItemType.Active);
			m_Inspector_Stage_Time.TrySetActive(!useConfig || !config.Time_Real.Active);
			m_Inspector_Stage_Beat.TrySetActive(!useConfig || !config.Time_Real.Active);
			m_Inspector_Stage_Duration.TrySetActive(!useConfig || !config.Duration_Real.Active);
			m_Inspector_Stage_X.TrySetActive(!useConfig || !config.X.Active);
			m_Inspector_Stage_Y.TrySetActive(!useConfig || !config.Y.Active);
			m_Inspector_Stage_Rotation.TrySetActive(!useConfig || !config.Rotation.Active);
			m_Inspector_Stage_Width.TrySetActive(!useConfig || !config.Width.Active);
			m_Inspector_Stage_Height.TrySetActive(!useConfig || !config.Height.Active);
			m_Inspector_Stage_PivotY.TrySetActive(!useConfig || !config.PivotY.Active);
			m_Inspector_Stage_Speed.TrySetActive(!useConfig || !config.Speed.Active);
			m_Inspector_Stage_Motion_Pos.TrySetActive(!useConfig || config.Motion_Pos);
			m_Inspector_Stage_Motion_Rot.TrySetActive(!useConfig || config.Motion_Rot);
			m_Inspector_Stage_Motion_Color.TrySetActive(!useConfig || config.Motion_Color);
			m_Inspector_Stage_Motion_Width.TrySetActive(!useConfig || config.Motion_Width);
			m_Inspector_Stage_Motion_Height.TrySetActive(!useConfig || config.Motion_Height);
		}


		private void RefreshTrackInspector () {
			int index = GetSelectingItemIndex();
			if (index < 0) { return; }
			var gene = GetGene();
			var config = GetTrackConfig(gene, index);
			bool useConfig = config.UseConfig;
			m_Inspector_Track_ItemType.TrySetActive(!useConfig || !config.ItemType.Active);
			m_Inspector_Track_ItemTypeSelector.TrySetActive(!useConfig || !config.ItemType.Active);
			m_Inspector_Track_StageIndex.TrySetActive(!useConfig || !config.StageIndex.Active);
			m_Inspector_Track_Time.TrySetActive(!useConfig || !config.Time_Real.Active);
			m_Inspector_Track_Beat.TrySetActive(!useConfig || !config.Time_Real.Active);
			m_Inspector_Track_Duration.TrySetActive(!useConfig || !config.Duration_Real.Active);
			m_Inspector_Track_X.TrySetActive(!useConfig || !config.X.Active);
			m_Inspector_Track_Angle.TrySetActive(!useConfig || !config.Angle.Active);
			m_Inspector_Track_Width.TrySetActive(!useConfig || !config.Width.Active);
			m_Inspector_Track_Speed.TrySetActive(!useConfig || !config.Speed.Active);
			m_Inspector_Track_Tray.TrySetActive(!useConfig || !config.HasTray.Active);
			m_Inspector_Track_Motion_X.TrySetActive(!useConfig || config.Motion_X);
			m_Inspector_Track_Motion_Angle.TrySetActive(!useConfig || config.Motion_Angle);
			m_Inspector_Track_Motion_Color.TrySetActive(!useConfig || config.Motion_Color);
			m_Inspector_Track_Motion_Width.TrySetActive(!useConfig || config.Motion_Width);
		}


		private void RefreshNoteInspector () {
			int index = GetSelectingItemIndex();
			if (index < 0) { return; }
			var gene = GetGene();
			var map = GetBeatmap();
			if (map == null) { return; }
			var config = GetNoteConfig(gene, map.GetParentIndex(2, index));
			bool useConfig = config.UseConfig;
			m_Inspector_Note_ItemType.TrySetActive(!useConfig || !config.ItemType.Active);
			m_Inspector_Note_ItemTypeSelector.TrySetActive(!useConfig || !config.ItemType.Active);
			m_Inspector_Note_X.TrySetActive(!useConfig || !config.X.Active);
			m_Inspector_Note_Z.TrySetActive(!useConfig || !config.Z.Active);
			m_Inspector_Note_Duration.TrySetActive(!useConfig || !config.Duration.Active);
			m_Inspector_Note_Width.TrySetActive(!useConfig || !config.Width.Active);
			m_Inspector_Note_LinkIndex.TrySetActive(!useConfig || !config.LinkedNoteIndex.Active);
			m_Inspector_Note_Speed.TrySetActive(!useConfig || !config.Speed.Active);
			m_Inspector_Note_ClickSound.TrySetActive(!useConfig || !config.ClickSound.Active);
		}


		#endregion




	}
}