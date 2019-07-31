using System.Windows.Controls;
using Windows.Media.Control;
using System;
using System.Windows.Media.Imaging;
using System.IO;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AudioFlyout
{
    /// <summary>
    /// Interaction logic for SessionControl.xaml
    /// </summary>
    public partial class SessionControl : UserControl
    {
        private GlobalSystemMediaTransportControlsSession _SMTCSession;

        public GlobalSystemMediaTransportControlsSession SMTCSession
        {
            get => _SMTCSession;
            set
            {
                if (value != null)
                {
                    UpdateSessionInfo(value);
                    value.MediaPropertiesChanged += Session_MediaPropertiesChanged;
                    value.PlaybackInfoChanged += Value_PlaybackInfoChanged;
                }
                else
                {
                    //Gute Nacht
                    if (_SMTCSession != null)
                    {
                        _SMTCSession.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
                        _SMTCSession.PlaybackInfoChanged -= Value_PlaybackInfoChanged;
                    }
                }
                _SMTCSession = value;
            }
        }

        public SessionControl()
        {
            InitializeComponent();
        }

        private async void Value_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession session, PlaybackInfoChangedEventArgs args)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (session != null && session.GetPlaybackInfo() != null)
                    UpdatePlayPauseButtonIcon(session);
            }));
        }

        private async void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
            {
                if (session != null && session.GetPlaybackInfo() != null)
                    UpdateSessionInfo(session);

            }));
        }

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_SMTCSession != null)
                    await _SMTCSession.TrySkipPreviousAsync();
            }
            catch (Exception)
            {
                //Because who knows, documentation is very little.
            }
        }

        private async void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_SMTCSession != null)
                {
                    var playback = _SMTCSession.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                            await _SMTCSession.TryPauseAsync();
                        else if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                            await _SMTCSession.TryPlayAsync();
                    }
                }
            }
            catch (Exception)
            {
                //Because who knows, documentation is very little.
            }
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_SMTCSession != null)
                    await _SMTCSession.TrySkipNextAsync();
            }
            catch (Exception)
            {
                //Because who knows, documentation is very little.
            }
        }

        private void UpdatePlayPauseButtonIcon(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                if (session != null)
                {
                    var playback = session.GetPlaybackInfo();
                    if (playback != null)
                    {
                        if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                            PlayPause.Content = "\uE769";
                        else if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                            PlayPause.Content = "\uE768";
                    }
                }
            }
            catch (Exception)
            {
                //ew
            }
        }

        private async void UpdateSessionInfo(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                var mediaInfo = await session.TryGetMediaPropertiesAsync();
                SongName.Text = mediaInfo.Title;
                SongArtist.Text = mediaInfo.Artist;

                var playback = session.GetPlaybackInfo();

                if (playback != null)
                {
                    Next.IsEnabled = session.GetPlaybackInfo().Controls.IsNextEnabled;
                    Back.IsEnabled = session.GetPlaybackInfo().Controls.IsPreviousEnabled;
                    PlayPause.IsEnabled = session.GetPlaybackInfo().Controls.IsPauseEnabled || session.GetPlaybackInfo().Controls.IsPlayEnabled;
                }

                UpdatePlayPauseButtonIcon(session);

                await SetThumbnailAsync(mediaInfo.Thumbnail);

            }
            catch (Exception)
            {
                //ew
            }
        }

        private async Task SetThumbnailAsync(IRandomAccessStreamReference thumbnail)
        {
            if (thumbnail != null)
            {
                using (var strm = await thumbnail.OpenReadAsync())
                {
                    if (strm != null)
                    {
                        using (var nstream = strm.AsStream())
                        {
                            if (nstream != null && nstream.Length > 0)
                            {
                                thumb.ImageSource = BitmapFrame.Create(nstream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            }
                            else
                            {
                                thumb.ImageSource = null;
                            }
                        }
                    }
                    else
                    {
                        thumb.ImageSource = null;
                    }
                }
            }
        }
    }
}
