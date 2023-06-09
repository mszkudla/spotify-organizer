﻿namespace SpotifyOrganizer.Models
{
    public sealed class SpotifyUser
    {
        public int Id { get; set; }

        public string? UserName { get; set; }
        public string UserId { get; set; } = null!;

        public ICollection<UserAlbum> UserAlbums { get; set; } = null!;
    }
}