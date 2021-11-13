# Spotify Extended Streaming History Extraction Tool
This program can be run on the JSON files provided by a GDPR request to spotify for your extended (all-time) streaming history. These files are usually named in the format endsong_0.json, endsong_1.json etc.

This program takes these files as input and cross-references them with data from the Spotify API to save to a database a fuller picture of your streaming history details for further analysis.

## How To Run
You will need to configure an appsettings file with the following details, this file should be named appsettings.development.json for development config and appsettings.production.json for production config.

* Database Connection String (PostgreSQL)
* Spotify API Client Id
* Spotify API Client Secret

In the following format:

```
"AppConfig": {
    "Database": {
        "ConnectionString": ""
    },
    "SpotifyClientId": "",
    "SpotifyClientSecret": ""
}
```