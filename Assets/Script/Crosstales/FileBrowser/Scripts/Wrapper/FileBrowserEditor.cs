#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Crosstales.FB.Wrapper
{
   public class FileBrowserEditor : BaseFileBrowser
   {
      #region Implemented methods

      public override bool canOpenFile => true;
      public override bool canOpenFolder => true;
      public override bool canSaveFile => true;

      public override bool canOpenMultipleFiles => false;

      public override bool canOpenMultipleFolders => false;

      public override bool isPlatformSupported => Util.Helper.isWindowsPlatform || Util.Helper.isMacOSPlatform || Util.Helper.isLinuxPlatform || Util.Helper.isWSABasedPlatform;

      public override bool isWorkingInEditor => true;

      public override string CurrentOpenSingleFile { get; set; }
      public override string[] CurrentOpenFiles { get; set; }
      public override string CurrentOpenSingleFolder { get; set; }
      public override string[] CurrentOpenFolders { get; set; }
      public override string CurrentSaveFile { get; set; }

      public override string[] OpenFiles(string title, string directory, string defaultName, bool multiselect, params ExtensionFilter[] extensions)
      {
         if (Util.Helper.isMacOSEditor && extensions != null && extensions.Length > 1)
            Debug.LogWarning("Multiple 'extensions' are not supported in the Editor.");

         if (multiselect)
            Debug.LogWarning("'multiselect' for files is not supported in the Editor.");

         if (!string.IsNullOrEmpty(defaultName))
            Debug.LogWarning("'defaultName' is not supported in the Editor.");

         string path = string.Empty;

         path = extensions == null ? EditorUtility.OpenFilePanel(title, directory, string.Empty) : EditorUtility.OpenFilePanelWithFilters(title, directory, getFilterFromFileExtensionList(extensions));

         if (string.IsNullOrEmpty(path))
            return null;

         CurrentOpenSingleFile = Util.Helper.ValidateFile(path);
         CurrentOpenFiles = new[] {CurrentOpenSingleFile};

         return CurrentOpenFiles;
      }

      public override string[] OpenFolders(string title, string directory, bool multiselect)
      {
         if (multiselect)
            Debug.LogWarning("'multiselect' for folders is not supported in the Editor.");

         string path = EditorUtility.OpenFolderPanel(title, directory, string.Empty);

         if (string.IsNullOrEmpty(path))
            return null;

         CurrentOpenSingleFolder = Util.Helper.ValidatePath(path);
         CurrentOpenFolders = new[] {CurrentOpenSingleFolder};

         return CurrentOpenFolders;
      }

      public override string SaveFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions)
      {
         if (extensions != null && extensions.Length > 1)
            Debug.LogWarning("Multiple 'extensions' are not supported in the Editor.");

         string ext = extensions != null && extensions.Length > 0 ? extensions[0].Extensions[0].Equals("*") ? string.Empty : extensions[0].Extensions[0] : string.Empty;
         string name = string.IsNullOrEmpty(ext) ? defaultName : $"{defaultName}.{ext}";

         string path = EditorUtility.SaveFilePanel(title, directory, name, ext);

         if (string.IsNullOrEmpty(path))
            return null;

         CurrentSaveFile = Util.Helper.ValidateFile(path);

         return CurrentSaveFile;
      }

      public override void OpenFilesAsync(string title, string directory, string defaultName, bool multiselect, ExtensionFilter[] extensions, Action<string[]> cb)
      {
         Debug.LogWarning("'OpenFilesAsync' is running synchronously in the Editor.");
         cb?.Invoke(OpenFiles(title, directory, defaultName, multiselect, extensions));
      }

      public override void OpenFoldersAsync(string title, string directory, bool multiselect, Action<string[]> cb)
      {
         Debug.LogWarning("'OpenFoldersAsync' is running synchronously in the Editor.");
         cb?.Invoke(OpenFolders(title, directory, multiselect));
      }

      public override void SaveFileAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
      {
         Debug.LogWarning("'SaveFileAsync' is running synchronously in the Editor.");
         cb?.Invoke(SaveFile(title, directory, defaultName, extensions));
      }

      #endregion


      #region Private methods

      private static string[] getFilterFromFileExtensionList(ExtensionFilter[] extensions)
      {
         if (extensions != null && extensions.Length > 0)
         {
            string[] filters = new string[extensions.Length * 2];

            for (int ii = 0; ii < extensions.Length; ii++)
            {
               filters[ii * 2] = extensions[ii].Name;
               filters[ii * 2 + 1] = string.Join(",", extensions[ii].Extensions);
            }

            if (Util.Config.DEBUG)
               Debug.Log($"getFilterFromFileExtensionList: {filters.CTDump()}");

            return filters;
         }

         return new string[0];
      }

      #endregion
   }
}
#endif
// © 2017-2021 crosstales LLC (https://www.crosstales.com)