using Avalonia;
using Avalonia.Controls;
using HeavyTracks.Models;
using HeavyTracks.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace HeavyTracks.ViewModels
{
    public class HeavyTracksVM : ViewModelBase
    {
        public HeavyTracksVM()
        {
            if (!Design.IsDesignMode)
            {
                if (!weigher.loadClientId(id_file))
                {
                    // TODO: should display an error popup requesting a client id? 
                    // this is only hit if something major goes wrong, or if the user manually edited the creds file.
                }

                weigher.beginSession().ConfigureAwait(false);

                Playlists = weigher.getPlaylists().Result;

                UserName = weigher.getUserName().Result ?? "";
            }
            else
                Playlists = new();

            
        }

        readonly string id_file = "creds.toml";

        SpotifyWeigher weigher = new();
        List<Playlist> Playlists { get; set; }

        Playlist? selected_playlist = null;
        Playlist? SelectedPlaylist
        {
            get => selected_playlist;
            set
            {
                SelectedPlaylistTracks = null;
                this.RaisePropertyChanged(nameof(SelectedPlaylistTracks));

                new Task(async () =>
                {
                    this.RaiseAndSetIfChanged(ref selected_playlist, value);
                    SelectedPlaylistTracks = await weigher.getPlaylistTracks(selected_playlist!);

                    this.RaisePropertyChanged(nameof(SelectedPlaylistTracks));
                }).Start();
            }
        }

        string UserName { get; set; }

        List<Track>? SelectedPlaylistTracks { get; set; }

        public async Task userClicked()
        {
            await weigher.beginSession();
        }

        public async Task apply()
        {
            if(SelectedPlaylist is not null && SelectedPlaylistTracks is not null)
                await weigher.pushTracks(SelectedPlaylistTracks, SelectedPlaylist, true);
        }

        public async Task sync()
        {
            if (SelectedPlaylist is not null)
            {
                SelectedPlaylistTracks = await weigher.getPlaylistTracks(SelectedPlaylist);
                this.RaisePropertyChanged(nameof(SelectedPlaylistTracks));
            }
        }

    }
}
