This demo scene showcases the use of multiple setting parents.

Any GameObject (including input elements) can be used as a settings parent.
The settings parent component has an Identifier field (string) that you can
enter any string you like into. By default a setting is attached to
a setting object that has the identifier of 'settings-parent'.

In this demo another SettingObject with the identifier of 'settings-parent-2' is used.

Menu layout prefabs have parents already set up with the correct identifier.
For multiple columns/tabs etc it is recommended to use 'settings-parent', 'settings-parent-2',
'settings-parent-3' and so on.