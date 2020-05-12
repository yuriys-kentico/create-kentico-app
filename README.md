# create-kentico-app

A way to easily start a Kentico project. This is a CLI tool. The available parameters:
```
----------------------------------------------------------------------------------------------------------
| Name                     | Aliases | Type           | Description
----------------------------------------------------------------------------------------------------------
| --Help                   | --h     | Boolean        | Display this help text and ignore any other
                                                        parameters.
----------------------------------------------------------------------------------------------------------
| --Name                   | --n     | String         | App name.
----------------------------------------------------------------------------------------------------------
| --Path                   | --p     | String         | Solution path. Must be non-existent or empty.
----------------------------------------------------------------------------------------------------------
| --AdminDomain            | --ad    | String         | Admin domain. Must be unique in local IIS and 
                                                        different from appDomain.
----------------------------------------------------------------------------------------------------------
| --AppDomain              | --d     | String         | App domain. Must be unique in local IIS and 
                                                        different from adminDomain.
----------------------------------------------------------------------------------------------------------
| --AppTemplate            | --t     | String         | App template. Must be a template that comes with 
                                                        the Kentico installer.
----------------------------------------------------------------------------------------------------------
| --Version                | --v     | Boolean        | Version. Must exist as a version of Kentico. Can 
                                                        be a partial version like 12. If 12.0.X, must be
                                                        greater than 12.0.29.
----------------------------------------------------------------------------------------------------------
| --Source                 | --s     | Boolean        | Install source code. Must also set sourcePassword.
----------------------------------------------------------------------------------------------------------
| --SourcePassword         | --sp    | String         | Source code password.
----------------------------------------------------------------------------------------------------------
| --DatabaseName           | --db    | String         | Database name. Must be unique in database server.
----------------------------------------------------------------------------------------------------------
| --DatabaseServerName     | --ds    | String         | Database server name.
----------------------------------------------------------------------------------------------------------
| --DatabaseServerUser     | --du    | String         | Database server user. Must also set 
                                                        databaseServerPassword. If not set, Windows
                                                        Authentication will be used.
----------------------------------------------------------------------------------------------------------
| --DatabaseServerPassword | --dp    | String         | Database server password.
----------------------------------------------------------------------------------------------------------
| --License                | --l     | String         | License key. Must be valid for the version if 
                                                        version is set. If version is not set, must be 
                                                        valid for the latest version.
----------------------------------------------------------------------------------------------------------
 ```