This demo scene showcases the use of specific settings parent prefabs.

The use of those allows for example the visual grouping of settings.

To achieve this we use the container provider and give those parents 
the correct identifiers to then attach our settings to.

Basic example of identifiers:

Root Object: settings-parent
- Container 1: settings-parent-2
  - Setting 1
  - Setting 2
- Container 2: settings-parent-3
  - Setting 3
  - Setting 4
  - ...