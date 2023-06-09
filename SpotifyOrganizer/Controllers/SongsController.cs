﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyOrganizer.Data;
using SpotifyOrganizer.Models;
using SpotifyOrganizer.Services;

namespace SpotifyOrganizer.Controllers;

/// <summary>
/// Class <c>SongsController</c> is responsible for handling requests related to songs in the Spotify Organizer application.
/// It depends on the ApplicationDbContext class, which represents the database context.
/// Contains the following action methods: Index, Details, Create, Edit, and Delete and handles their exceptions.
/// It also implements a sorting function. Sort can be done by song name, release date, and artist name.
/// </summary>

public class SongsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SongsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Songs
    public async Task<IActionResult> Index(string sortOrder)
    {
        ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["ArtistSortParm"] = sortOrder == "Artist" ? "artist_desc" : "Artist";
        var songs = from s in _context.Songs select s;
        switch (sortOrder)
        {
            case "name_desc":
                songs = songs.OrderByDescending(s => s.SongName);
                break;
            case "Date":
                songs = songs.OrderBy(s => s.ReleaseDate);
                break;
            case "date_desc":
                songs = songs.OrderByDescending(s => s.ReleaseDate);
                break;
            case "Artist":
                songs = songs.OrderBy(s => s.Artist);
                break;
            case "artist_desc":
                songs = songs.OrderByDescending(s => s.Artist);
                break;
            default:
                songs = songs.OrderBy(s => s.SongName);
                break;
        }

        return View(await songs.AsNoTracking().ToListAsync());
        // return _context.Songs != null
        //   ? View(await _context.Songs.ToListAsync())
        //  : Problem("Entity set 'ApplicationDbContext.Songs'  is null.");
    }

    // GET: Songs/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Songs == null) return NotFound();

        var song = await _context.Songs
            .FirstOrDefaultAsync(m => m.Id == id);
        if (song == null) return NotFound();

        return View(song);
    }

    // GET: Songs/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Songs/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SongName")] Song searchSong)
    {
        var trackName = searchSong.SongName;
        var spotifyApiService = new SpotifyApiService();
        var track = await spotifyApiService.SearchTrack(trackName);

        if (track == null) return NotFound();
        
        var existingSong = await _context.Songs.FirstOrDefaultAsync(s => s.SpotifyId == track.Id);
        if (existingSong != null) return RedirectToAction(nameof(Index));
        
        var song = new Song
        {
            SpotifyId = track.Id,
            SongName = track.Name,
            Artist = track.Artists[0].Name,
            ReleaseDate = track.Album.ReleaseDate
        };
        

        if (ModelState.IsValid) return View(song);
        _context.Add(song);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    // GET: Songs/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Songs == null) return NotFound();

        var song = await _context.Songs.FindAsync(id);
        if (song == null) return NotFound();

        return View(song);
    }

    // POST: Songs/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,SpotifyId,SongName,Artist,ReleaseDate,AddDate")]
        Song song)
    {
        if (id != song.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            try
            {
                _context.Update(song);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SongExists(song.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(song);
    }

    // GET: Songs/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Songs == null) return NotFound();

        var song = await _context.Songs
            .FirstOrDefaultAsync(m => m.Id == id);
        if (song == null) return NotFound();

        return View(song);
    }

    // POST: Songs/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Songs == null) return Problem("Entity set 'ApplicationDbContext.Songs'  is null.");

        var song = await _context.Songs.FindAsync(id);
        if (song != null) _context.Songs.Remove(song);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SongExists(int id)
    {
        return (_context.Songs?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}