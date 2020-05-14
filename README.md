# create-kentico-app

A way to easily start a Kentico project. This is a CLI tool.

## Installation

```pwsh
# Extract latest release NUPKG in a folder
# In the folder run
dotnet tool install -g --add-source ./ create-kentico-app
```

## Usage

```pwsh
create-kentico-app -n AppName -t DancingGoatMvc -ds ServerName -l DOMAIN:localhost`nPRODUCT:CX12`n ... # Full license key with newlines escaped as `n
```

## Parameters

| Key | Aliases | Type | Required | Description |
| --------------------- |:---------:|:---------:|:---------:|:----------------------:|
| `--help` | `-h` | `boolean` | No | Display help text and ignore any other parameters. |
| `--name` | `-n` | `string` | Yes | App name. |
| `--databaseServerName` | `-ds`| `string` | Yes | Database server name. |
| `--databaseName` | `-db`| `string` | No | Database name. Must be unique in database server. |
| `--databaseServerUser` | `-du`| `string` | No | Database server user. Must also set `databaseServerPassword`. If not set, Windows Authentication will be used. |
| `--databaseServerPassword` | `-dp`| `string` | No | Database server password. |
| `--version` | `-v` | `string` | No | Kentico version. Must exist as a version of Kentico. Can be a partial version like 12. If 12.0.X, must be greater than 12.0.29. |
| `--license` | `-l`| `string` | No | License key. Must be valid for the version if version is set. If version is not set, must be valid for the latest version. |
| `--path` | `-p` | `string` | No | Solution path. Must be non-existent or empty. |
| `--appTemplate` | `-t` | `string` | No | App template. Must be a template that comes with the Kentico installer. |
| `--appDomain` | `-d` | `string` | No | App domain. Must be unique in local IIS and different from adminDomain. |
| `--adminDomain` | `-ad` | `string` | No | Admin domain. Must be unique in local IIS and different from appDomain. |
| `--source` | `-s` | `boolean` | No | Install source code. Must also set `sourcePassword`. |
| `--sourcePassword` | `-sp`| `string` | No | Source code password. |
