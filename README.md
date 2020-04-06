## Acad Bill of Quantities
This is an AutoCad plug-in to help preparing Bill of Quantities.  

### Getting started
This is a lightweight plug-in without installation executable.
1. To start application open AutoCad and run "NETLOAD" command.  
2. Select AutocadBillOfQuantities.Core.dll.  
3. Warning will be displayed since plug-in is not digitally signed.  
4. You can select "Always Load" so that warning will not be displayed anymore.  
5. Run command "STARTACADBILLOFQUANTITIES"
6. User Interface will be displayed.

### How it works
Application uses polylines and layers to distinct entities to count.  

When you click button next to category description in the UI polyline 
command is invoked in drawing area.
Layer will be picked automatically and if not present yet it will get created.

Categories listed in the user interface are defined in AppSettings.xml file
so that all are customizable.
Layer colors are picked by hash algorithm based on entry name.
Thanks to this mechanism colors don't have to be defined manually 
in configuration file and are always the same at each runtime.

In order to export data to clipboard simply press "To Clipboard" button.
Export also works on selection if you don't want all data to be retrieved.
![](doc/introduction.gif)

