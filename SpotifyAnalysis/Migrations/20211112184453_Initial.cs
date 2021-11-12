using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpotifyAnalysis.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    SpotifyId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReleaseDate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.SpotifyId);
                });

            migrationBuilder.CreateTable(
                name: "Artist",
                columns: table => new
                {
                    SpotifyId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Popularity = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artist", x => x.SpotifyId);
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    SpotifyId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AlbumId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrackLengthMs = table.Column<int>(type: "integer", nullable: false),
                    Explicit = table.Column<bool>(type: "boolean", nullable: false),
                    PreviewUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Acousticness = table.Column<float>(type: "real", nullable: false),
                    Danceability = table.Column<float>(type: "real", nullable: false),
                    Energy = table.Column<float>(type: "real", nullable: false),
                    Instrumentalness = table.Column<float>(type: "real", nullable: false),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Liveness = table.Column<float>(type: "real", nullable: false),
                    Loudness = table.Column<float>(type: "real", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Speechiness = table.Column<float>(type: "real", nullable: false),
                    EstimatedTempo = table.Column<float>(type: "real", nullable: false),
                    TimeSignature = table.Column<float>(type: "real", nullable: false),
                    Valence = table.Column<float>(type: "real", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Track", x => x.SpotifyId);
                    table.ForeignKey(
                        name: "FK_Track_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "SpotifyId");
                });

            migrationBuilder.CreateTable(
                name: "AlbumArtist",
                columns: table => new
                {
                    AlbumsSpotifyId = table.Column<string>(type: "character varying(100)", nullable: false),
                    ArtistsSpotifyId = table.Column<string>(type: "character varying(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumArtist", x => new { x.AlbumsSpotifyId, x.ArtistsSpotifyId });
                    table.ForeignKey(
                        name: "FK_AlbumArtist_Album_AlbumsSpotifyId",
                        column: x => x.AlbumsSpotifyId,
                        principalTable: "Album",
                        principalColumn: "SpotifyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumArtist_Artist_ArtistsSpotifyId",
                        column: x => x.ArtistsSpotifyId,
                        principalTable: "Artist",
                        principalColumn: "SpotifyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistTrack",
                columns: table => new
                {
                    ArtistsSpotifyId = table.Column<string>(type: "character varying(100)", nullable: false),
                    TracksSpotifyId = table.Column<string>(type: "character varying(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistTrack", x => new { x.ArtistsSpotifyId, x.TracksSpotifyId });
                    table.ForeignKey(
                        name: "FK_ArtistTrack_Artist_ArtistsSpotifyId",
                        column: x => x.ArtistsSpotifyId,
                        principalTable: "Artist",
                        principalColumn: "SpotifyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistTrack_Track_TracksSpotifyId",
                        column: x => x.TracksSpotifyId,
                        principalTable: "Track",
                        principalColumn: "SpotifyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stream",
                columns: table => new
                {
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrackId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    ReasonStart = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ReasonEnd = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Shuffle = table.Column<bool>(type: "boolean", nullable: false),
                    IncognitoMode = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stream", x => new { x.Username, x.TrackId, x.End });
                    table.ForeignKey(
                        name: "FK_Stream_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "SpotifyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArtist_ArtistsSpotifyId",
                table: "AlbumArtist",
                column: "ArtistsSpotifyId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistTrack_TracksSpotifyId",
                table: "ArtistTrack",
                column: "TracksSpotifyId");

            migrationBuilder.CreateIndex(
                name: "IX_Stream_TrackId",
                table: "Stream",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Track_AlbumId",
                table: "Track",
                column: "AlbumId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumArtist");

            migrationBuilder.DropTable(
                name: "ArtistTrack");

            migrationBuilder.DropTable(
                name: "Stream");

            migrationBuilder.DropTable(
                name: "Artist");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "Album");
        }
    }
}
