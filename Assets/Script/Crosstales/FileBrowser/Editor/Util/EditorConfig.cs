#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Crosstales.FB.EditorUtil
{
   /// <summary>Editor configuration for the asset.</summary>
   [InitializeOnLoad]
   public static class EditorConfig
   {
      #region Variables

      /// <summary>Enable or disable update-checks for the asset.</summary>
      public static bool UPDATE_CHECK = EditorConstants.DEFAULT_UPDATE_CHECK;

      /// <summary>Enable or disable adding compile defines "CT_FB" and "CT_FB_PRO" for the asset.</summary>
      public static bool COMPILE_DEFINES = EditorConstants.DEFAULT_COMPILE_DEFINES;

      /// <summary>Automatically load and add the prefabs to the scene.</summary>
      public static bool PREFAB_AUTOLOAD = EditorConstants.DEFAULT_PREFAB_AUTOLOAD;

      /// <summary>Enable or disable the icon in the hierarchy.</summary>
      public static bool HIERARCHY_ICON = EditorConstants.DEFAULT_HIERARCHY_ICON;

      /// <summary>Enable or disable the modifications of the Package.appxmanifest.</summary>
      public static bool MODIFY_MANIFEST = EditorConstants.DEFAULT_MODIFY_MANIFEST;

      /// <summary>Is the configuration loaded?</summary>
      public static bool isLoaded;

      private static string assetPath;
      private const string idPath = "Documentation/id/";
      private static readonly string idName = $"{EditorConstants.ASSET_UID}.txt";

      #endregion


      #region Constructor

      static EditorConfig()
      {
         if (!isLoaded)
         {
            Load();
         }
      }

      #endregion


      #region Properties

      /// <summary>Returns the path to the asset inside the Unity project.</summary>
      /// <returns>The path to the asset inside the Unity project.</returns>
      public static string ASSET_PATH
      {
         get
         {
            if (assetPath == null)
            {
               try
               {
                  if (System.IO.File.Exists(Application.dataPath + EditorConstants.DEFAULT_ASSET_PATH + idPath + idName))
                  {
                     assetPath = EditorConstants.DEFAULT_ASSET_PATH;
                  }
                  else
                  {
                     string[] files = System.IO.Directory.GetFiles(Application.dataPath, idName, System.IO.SearchOption.AllDirectories);

                     if (files.Length > 0)
                     {
                        string name = files[0].Substring(Application.dataPath.Length);
                        assetPath = name.Substring(0, name.Length - idPath.Length - idName.Length).Replace("\\", "/");
                     }
                     else
                     {
                        Debug.LogWarning($"Could not locate the asset! File not found: {idName}");
                        assetPath = EditorConstants.DEFAULT_ASSET_PATH;
                     }
                  }

                  Common.Util.CTPlayerPrefs.SetString(Util.Constants.KEY_ASSET_PATH, assetPath);
                  Common.Util.CTPlayerPrefs.Save();
               }
               catch (System.Exception ex)
               {
                  Debug.LogWarning($"Could not locate asset: {ex}");
               }
            }

            return assetPath;
         }
      }

      /// <summary>Returns the path of the prefabs.</summary>
      /// <returns>The path of the prefabs.</returns>
      public static string PREFAB_PATH => ASSET_PATH + EditorConstants.PREFAB_SUBPATH;

      #endregion


      #region Public static methods

      /// <summary>Resets all changeable variables to their default value.</summary>
      public static void Reset()
      {
         UPDATE_CHECK = EditorConstants.DEFAULT_UPDATE_CHECK;
         COMPILE_DEFINES = EditorConstants.DEFAULT_COMPILE_DEFINES;
         PREFAB_AUTOLOAD = EditorConstants.DEFAULT_PREFAB_AUTOLOAD;
         HIERARCHY_ICON = EditorConstants.DEFAULT_HIERARCHY_ICON;
         MODIFY_MANIFEST = EditorConstants.DEFAULT_MODIFY_MANIFEST;
      }

      /// <summary>Loads the all changeable variables.</summary>
      public static void Load()
      {
         if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_UPDATE_CHECK))
            UPDATE_CHECK = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_UPDATE_CHECK);

         if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_COMPILE_DEFINES))
            COMPILE_DEFINES = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_COMPILE_DEFINES);

         if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_PREFAB_AUTOLOAD))
            PREFAB_AUTOLOAD = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_PREFAB_AUTOLOAD);

         if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_HIERARCHY_ICON))
            HIERARCHY_ICON = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_HIERARCHY_ICON);

         if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_MODIFY_MANIFEST))
            MODIFY_MANIFEST = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_MODIFY_MANIFEST);

         isLoaded = true;
      }

      /// <summary>Saves the all changeable variables.</summary>
      public static void Save()
      {
         Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_UPDATE_CHECK, UPDATE_CHECK);
         Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_COMPILE_DEFINES, COMPILE_DEFINES);
         Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_PREFAB_AUTOLOAD, PREFAB_AUTOLOAD);
         Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_HIERARCHY_ICON, HIERARCHY_ICON);
         Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_MODIFY_MANIFEST, MODIFY_MANIFEST);

         Common.Util.CTPlayerPrefs.Save();
      }

      #endregion
   }
}
#endif
// © 2017-2021 crosstales LLC (https://www.crosstales.com)