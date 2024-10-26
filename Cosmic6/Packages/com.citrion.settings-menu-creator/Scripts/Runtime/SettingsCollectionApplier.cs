using CitrioN.Common;
using UnityEngine;

namespace CitrioN.SettingsMenuCreator
{
  /// <summary>
  /// Helper behaviour that can apply the current setting values
  /// without requiring a menu. This is useful to apply the settings
  /// at the start of the game without having the scene with the menu
  /// loaded.
  /// </summary>
  [AddTooltips]
  [HeaderInfo("Allows the setting values of the referenced SettingsCollection to be applied on awake. " +
              "Useful if the settings menu is not in the starting scene but the saved setting values should " +
              "already be applied.")]
  [AddComponentMenu("CitrioN/Settings Menu Creator/Settings Collection Applier")]
  public class SettingsCollectionApplier : MonoBehaviour
  {
    [SerializeField]
    [Tooltip("The SettingsCollection for which to load and apply the setting values on awake.")]
    protected SettingsCollection collection;

    private void Awake()
    {
      ApplySettings();
    }

    public void ApplySettings()
    {
      if (collection == null) { return; }

      collection.Initialize();

      // Load and apply the default setting values
      collection.LoadSettings(true);

      // Load and apply the currently saved setting values
      collection.LoadSettings(false);
    }
  }
}