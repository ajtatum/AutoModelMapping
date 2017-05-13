# AutoModelMapping

This WPF Application was designed on the fly to help speed up the process of creating models and mappings from a database. This was geared towards Entity Framework Code First.

## How To Use
* On the **Get Tables** tab you can either type in a database string or you can create a ConnectionStrings.config file and leave the text box blank. ConnectionStrings.config would look similar to:

```xml
<connectionStrings>
  <add name="defaultdb" providerName="System.Data.SqlClient" connectionString="YourConnectionStringHere"/>
</connectionStrings>
```

* Once you click the "**Get Tables**" button, all the tables from that database will be listed. From there you can either select all or select multiple tables.

* After you've selected the tables, go to the "**Get Data Models and Mapping**" tab. Enter your namespaces and select a folder where you'd like the .cs files to be generated. Then simply click the "**Get Models and Mapping**" button and it will begin to show the text generated and will create the files you need in a Models and Mapping folder.

**Any contributions would be awesome!**

_Thank you,  
[AJ Tatum](https://ajtatum.com)  
[My LinkedIn Profile](https://www.linkedin.com/in/ajtatum/)_
