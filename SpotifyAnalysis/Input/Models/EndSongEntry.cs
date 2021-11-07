using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Input.Models
{
    public class EndSongEntry
    {
        /// <summary>
        /// This field is a timestamp indicating when the track stopped playing in UTC
        /// </summary>
        [JsonProperty("ts")]
        public DateTime TrackStopTimestamp { get; set; }

        /// <summary>
        /// This field is your Spotify username.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast)
        /// </summary>
        [JsonProperty("platform")]
        public string Platform { get; set; }

        /// <summary>
        /// This field is the number of milliseconds the stream was played.
        /// </summary>
        [JsonProperty("ms_played")]
        public int MsPlayed { get; set; }

        /// <summary>
        /// This field is the country code of the country where the stream was played(e.g.SE - Sweden).
        /// </summary>
        [JsonProperty("conn_country")]
        public string ConnectionCountry { get; set; }

        /// <summary>
        /// This field contains the IP address logged when streaming the track.
        /// </summary>
        [JsonProperty("ip_addr_decrypted")]
        public string DecryptedIpAddress { get; set; }

        /// <summary>
        /// This field contains the user agent used when streaming the track (e.g.a browser, like Mozilla Firefox, or Safari)
        /// </summary>
        [JsonProperty("user_agent_decrypted")]
        public string UserAgent { get; set; }

        /// <summary>
        /// This field is the name of the track.
        /// </summary>
        [JsonProperty("master_metadata_track_name")]
        public string TrackName { get; set; }

        /// <summary>
        /// This field is the name of the artist, band or podcast.
        /// </summary>
        [JsonProperty("master_metadata_album_artist_name")]
        public string ArtistName { get; set; }

        /// <summary>
        /// This field is the name of the album.
        /// </summary>
        [JsonProperty("master_metadata_album_album_name")]
        public string AlbumName { get; set; }

        /// <summary>
        /// A Spotify URI, uniquely identifying the track in the form of “spotify:track:[base-62 string]”
        /// <remarks>
        /// A Spotify URI is a resource identifier that you can enter, for example, in the Spotify Desktop client’s search box to locate an artist, album, or track.
        /// To find a Spotify URI simply right-click (on Windows) or Ctrl-Click (on a Mac) on the artist’s or album’s or track’s name.
        /// </remarks>
        /// </summary>
        [JsonProperty("spotify_track_uri")]
        public string TrackUri { get; set; }

        /// <summary>
        /// This field contains the name of the episode of the podcast
        /// </summary>
        [JsonProperty("episode_name")]
        public string EpisodeName { get; set; }

        /// <summary>
        /// This field contains the name of the show of the podcast.
        /// </summary>
        [JsonProperty("episode_show_name")]
        public string EpisodeShowName { get; set; }

        /// <summary>
        /// A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:[base-62 string]”
        /// </summary>
        [JsonProperty("spotify_episode_uri")]
        public string SpotifyEpisodeUri { get; set; }

        /// <summary>
        /// This field is a value telling why the track started (e.g. “trackdone”)

        /// </summary>
        [JsonProperty("reason_start")]
        public string ReasonStart { get; set; }

        /// <summary>
        /// This field is a value telling why the track ended (e.g. “endplay”).
        /// </summary>
        [JsonProperty("reason_end")]
        public string ReasonEnd { get; set; }

        /// <summary>
        /// This field has the value True or False depending on if shuffle mode was used when playing the track.
        /// </summary>
        [JsonProperty("shuffle")]
        public bool? Shuffle { get; set; }

        /// <summary>
        /// This field indicates if the user skipped to the next song.
        /// </summary>
        [JsonProperty("skipped")]
        public bool? Skipped { get; set; }

        /// <summary>
        /// This field indicates whether the track was played in offline mode (“True”) or not(“False”).
        /// </summary>
        [JsonProperty("offline")]
        public bool? Offline { get; set; }

        /// <summary>
        /// This field is a timestamp of when offline mode was used, if used.
        /// </summary>
        [JsonProperty("offline_timestamp")]
        public long OfflineTimestamp { get; set; }

        /// <summary>
        /// This field indicates whether the track was played in incognito mode(“True”) or not(“False”).
        /// </summary>
        [JsonProperty("incognito_mode")]
        public bool? IncognitoMode { get; set; }
    }
}