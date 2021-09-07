#if UNITY_EDITOR
namespace Crosstales.FB.EditorUtil
{
   /// <summary>Collected editor constants of very general utility for the asset.</summary>
   public static class EditorConstants
   {
      #region Constant variables

      public const string KEY_UPDATE_CHECK = Util.Constants.KEY_PREFIX + "UPDATE_CHECK";
      public const string KEY_COMPILE_DEFINES = Util.Constants.KEY_PREFIX + "COMPILE_DEFINES";
      public const string KEY_PREFAB_AUTOLOAD = Util.Constants.KEY_PREFIX + "PREFAB_AUTOLOAD";
      public const string KEY_HIERARCHY_ICON = Util.Constants.KEY_PREFIX + "HIERARCHY_ICON";
      public const string KEY_MODIFY_MANIFEST = Util.Constants.KEY_PREFIX + "MODIFY_MANIFEST";

      // Keys for the configuration of the asset
      public const string KEY_UPDATE_DATE = Util.Constants.KEY_PREFIX + "UPDATE_DATE";

      // Default values
      public const string DEFAULT_ASSET_PATH = "/Plugins/crosstales/FileBrowser/";
      public const bool DEFAULT_UPDATE_CHECK = false;
      public const bool DEFAULT_COMPILE_DEFINES = true;
      public const bool DEFAULT_PREFAB_AUTOLOAD = false;
      public const bool DEFAULT_HIERARCHY_ICON = false;
      public const bool DEFAULT_MODIFY_MANIFEST = true;

      #endregion


      #region Changable variables

      /// <summary>Sub-path to the prefabs.</summary>
      public static string PREFAB_SUBPATH = "Resources/Prefabs/";

      #endregion


      #region Properties

      /// <summary>Returns the URL of the asset in UAS.</summary>
      /// <returns>The URL of the asset in UAS.</returns>
      public static string ASSET_URL => Util.Constants.ASSET_PRO_URL;

      /// <summary>Returns the ID of the asset in UAS.</summary>
      /// <returns>The ID of the asset in UAS.</returns>
      public static string ASSET_ID => "98713";

      /// <summary>Returns the UID of the asset.</summary>
      /// <returns>The UID of the asset.</returns>
      public static System.Guid ASSET_UID => new System.Guid("f9c139be-4da6-4d0f-894a-0675635af15f");

      #endregion
   }
}
#endif
// © 2017-2021 crosstales LLC (https://www.crosstales.com)