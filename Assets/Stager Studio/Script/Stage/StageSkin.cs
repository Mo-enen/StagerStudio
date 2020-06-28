namespace StagerStudio.Stage {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Data;
	using Saving;


	public class StageSkin : MonoBehaviour {




		#region --- SUB ---


		public delegate void VoidSkinHandler (SkinData skin);
		public delegate void VoidHandler ();
		public delegate string StringStringHandler (string key);
		public delegate void VoidStringStringBoolHandler (string msg, string detail, bool isWarning);


		private static class LanguageData {
			public const string ERROR_SkinNotLoaded = "Skin.Error.SkinNotLoaded";
			public const string ERROR_FailToSaveSkin = "Skin.Error.FailToSaveSkin";
			public const string CONFIRM_DeleteSkinConfirm = "Skin.Confirm.DeleteSkinConfirm";
			public const string CONFIRM_NewSkinConfirm = "Skin.Confirm.NewSkinConfirm";
		}


		#endregion




		#region --- VAR ---


		// Handler
		public static VoidSkinHandler OnSkinLoaded { get; set; } = null;
		public static VoidHandler OnSkinDeleted { get; set; } = null;
		public static StringStringHandler GetLanguage { get; set; } = null;

		// API
		public static (SkinData Data, string Name) Data {
			get {
				if (_Skin.data == null) {
					var stageSkin = FindObjectOfType<StageSkin>();
					_Skin = (stageSkin.m_DefaultSkin, "");
					_Skin.data.SetPng(stageSkin.m_DefaultTexture);
				}
				return _Skin;
			}
			private set => _Skin = value;
		}

		public string[] AllSkinNames {
			get {
				if (SkinNames == null || SkinNames.Length == 0) {
					var files = Util.GetFilesIn(SkinFolderPath, true, "*" + SKIN_EX);
					SkinNames = new string[files.Length];
					for (int i = 0; i < files.Length; i++) {
						SkinNames[i] = Util.GetNameWithoutExtension(files[i].Name);
					}
				}
				return SkinNames;
			}
		}

		public string CurrentSkinName => SkinName;

		// Short
		private string SkinFolderPath => Util.CombinePaths(Util.GetParentPath(Application.dataPath), "Skins");

		// Ser
		[SerializeField] private SkinData m_DefaultSkin = null;
		[SerializeField] private Texture2D m_DefaultTexture = null;

		// Data
		private const string SKIN_EX = ".stagerskin";
		private static (SkinData data, string name) _Skin = (null, "");
		private string[] SkinNames = null;

		// Saving
		private SavingString SkinName = new SavingString("StageSkin.SkinName", "");


		#endregion




		#region --- MSG ---


		private void Awake () {
			LoadSkin(SkinName);
		}


		#endregion




		#region --- API ---


		public void RefreshAllSkinNames () => SkinNames = null;


		public SkinData GetSkinFromDisk (string name) {
			try {
				var path = Util.CombinePaths(SkinFolderPath, name + SKIN_EX);
				if (Util.FileExists(path)) {
					return SkinData.ByteToSkin(Util.FileToByte(path));
				}
			} catch { }
			return null;
		}


		public string GetPath (string name) => Util.CombinePaths(SkinFolderPath, name + SKIN_EX);


		public void ReloadSkin () => LoadSkin(SkinName);


		public void LoadSkin (string name) {
			if (!string.IsNullOrEmpty(name)) {
				// Load Skin
				try {
					Data = (GetSkinFromDisk(name), name);
				} catch { }
				// Load Built-in Default
				try {
					if (Data.Data == null) {
						LogMessage(LanguageData.ERROR_SkinNotLoaded, true);
						Data = (m_DefaultSkin, "");
						name = "";
					}
				} catch { }
			} else {
				Data = (m_DefaultSkin, "");
			}
			SkinName.Value = name;
			OnSkinLoaded(Data.Data);
		}


		public string SaveSkin (SkinData skin, string name) {
			if (skin == null || string.IsNullOrEmpty(name)) { return ""; }
			string path = "";
			try {
				var bytes = SkinData.SkinToByte(skin);
				if (!(bytes is null) && bytes.Length > 0) {
					path = Util.CombinePaths(SkinFolderPath, name + SKIN_EX);
					Util.ByteToFile(bytes, path);
				}
			} catch {
				LogMessage(LanguageData.ERROR_FailToSaveSkin, true);
			}
			RefreshAllSkinNames();
			return path;
		}


		public void UI_NewSkin () => DialogUtil.Dialog_OK_Cancel(LanguageData.CONFIRM_NewSkinConfirm, DialogUtil.MarkType.Info, () => {
			var name = "Skin_" + Util.GetTimeString();
			var skin = new SkinData();
			skin.Fillup();
			SaveSkin(skin, name);
			LoadSkin(name);
		});


		public void UI_LoadSkin (object skinNameObj) {
			if (!(skinNameObj is RectTransform)) { return; }
			LoadSkin((skinNameObj as RectTransform).name);
		}


		public void UI_DeleteSkin (object skinNameObj) {
			if (!(skinNameObj is RectTransform)) { return; }
			string skinName = (skinNameObj as RectTransform).name;
			DialogUtil.Open(
				string.Format(GetLanguage(LanguageData.CONFIRM_DeleteSkinConfirm), skinName),
				DialogUtil.MarkType.Warning,
				() => {
					try {
						Util.DeleteFile(Util.CombinePaths(SkinFolderPath, skinName + SKIN_EX));
					} catch { }
					RefreshAllSkinNames();
					OnSkinDeleted();
				}, null, null, null, () => { });
		}


		#endregion




		#region --- LGC ---


		private void LogMessage (string key, bool warning = false) => DialogUtil.Open(GetLanguage(key), warning ? DialogUtil.MarkType.Warning : DialogUtil.MarkType.Info, () => { });


		#endregion




		#region --- EDT ---
#if UNITY_EDITOR
		public void EDITOR_SetDefaultSkin (SkinData skin, Texture2D texture) {
			m_DefaultSkin = skin;
			if (texture) {
				var path = @"Assets\Stager Studio\Image\UI\Stager Skin.png";
				Util.ByteToFile(texture.EncodeToPNG(), Util.GetFullPath(path));
				UnityEditor.EditorUtility.SetDirty(texture);
			}
		}
#endif
		#endregion




	}
}


#if UNITY_EDITOR
namespace StagerStudio.Editor {
	using UnityEngine;
	using UnityEditor;
	using Stage;
	[CustomEditor(typeof(StageSkin))]
	public class StagerSkinInspector : Editor {
		private void OnDisable () {
			try {
				var skin = (target as StageSkin).GetSkinFromDisk("Stager");
				(target as StageSkin).EDITOR_SetDefaultSkin(skin, skin.Texture);
			} catch { }
		}
	}
}
#endif