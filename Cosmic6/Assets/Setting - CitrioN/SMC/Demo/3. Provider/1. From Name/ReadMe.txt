This demo scene showcases how to change the provider for a single setting.

This is done on the SettingsCollection.
For each setting you can specify what provider for the input element should be used.
This option is available for both UGUI and UI Toolkit in the advanced options tab
for each setting. By default the auto option is used which select the provider 
based on the setting value. Usually this is all you need but in this case we want to specifically
use a previous next selector for the audio enabled setting.

By default this would use the provider assigned for the boolean type because audio enabled
uses the boolean type. To make it use the previous next selector instead we change it in 
the options to use it by name. This will use the provider with the corresponding name 
on the provider collection.

Check out the advanced tab for the 'Audio Setting' on the SettingsCollection for details.