using System.Windows;
using Windows.Media.Control;
using System;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using NAudio.CoreAudioApi;

namespace AudioFlyout
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void GetMyVolumeNow(double volume);

        private GlobalSystemMediaTransportControlsSessionManager SMTC;
        private MMDevice _device;

        public void UpdateDevice(MMDevice device)
        {
            _device = device;
            if (_device != null)
            {
                UpdateVolume(_device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                _device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
            }

            //TODO: if null remove the volume stuff, keep only SMTC
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data) => UpdateVolume(data.MasterVolume * 100);

        public MainWindow()
        {
            InitializeComponent();
            SetupSMTCAsync();
        }

        private async void SetupSMTCAsync()
        {
            //GSMTC

            SMTC = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            var sessions = SMTC.GetSessions();

            foreach (var session in sessions)
            {
                var covfefe = new SessionControl
                {
                    SMTCSession = session,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                SessionsStackPanel.Children.Add(covfefe);
            }

            if (SessionsStackPanel.Children.Count > 0)
            {
                SessionsStackPanel.Margin = new Thickness(0, 2, 0, 0);
                (SessionsStackPanel.Children[0] as SessionControl).Margin = new Thickness(0);
            }
            else
                SessionsStackPanel.Margin = new Thickness(0);

            SMTC.SessionsChanged += SMTC_SessionsChanged;
        }

        private void VolUp()
        {
            if (VolumeSlider.Value < VolumeSlider.Maximum)
                VolumeSlider.Value = Math.Truncate(VolumeSlider.Value) + 2;
        }

        private void VolDown()
        {
            if (VolumeSlider.Value > VolumeSlider.Minimum)
                VolumeSlider.Value = Math.Truncate(VolumeSlider.Value) - 2;
        }

        private void UpdateVolumeGlyph(double volume)
        {
            if (_device != null && !_device.AudioEndpointVolume.Mute)
            {
                VolumeShadowGlyph.Visibility = Visibility.Visible;
                if (volume >= 66)
                    VolumeGlyph.UnicodeString = "\uE995";
                else if (volume < 1)
                    VolumeGlyph.UnicodeString = "\uE992";
                else if (volume < 33)
                    VolumeGlyph.UnicodeString = "\uE993";
                else if (volume < 66)
                    VolumeGlyph.UnicodeString = "\uE994";
            }
            else
            {
                VolumeShadowGlyph.Visibility = Visibility.Collapsed;
                VolumeGlyph.UnicodeString = "\uE74F";
            }
        }

        private bool _isInCodeValueChange = false; //Prevents a LOOP between changing volume.

        private void UpdateVolume(double volume)
        {

            Dispatcher.Invoke(new Action(() => 
            {
                UpdateVolumeGlyph(volume);
                _isInCodeValueChange = true;
                VolumeSlider.Value = Math.Round(volume);
                _isInCodeValueChange = false;
                textVal.Text = Math.Round(volume).ToString("00");
            }));
        }

        private void SMTC_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
            {

                var lol = args.GetType();

                var sessions = SMTC.GetSessions();

                var CLR = sessions.ToList();

                // DELETE IF IT DOESN'T EXIST ANYMORE.

                var toDelete = new List<SessionControl>();

                //Find if we have the mythical beast already in list.
                foreach (var sessControl in SessionsStackPanel.Children)
                {
                    var tmp = (SessionControl)sessControl;
                    //if (!sessions.Any(s => s.SourceAppUserModelId == tmp.SMTCSession.SourceAppUserModelId)) //Sad this doesn't work if there are two SourceAppUserModelId with the same name :( 
                    toDelete.Add(tmp);
                }

                toDelete.ForEach(ses => 
                {
                    ses.SMTCSession = null;
                    SessionsStackPanel.Children.Remove(ses);
                });


                //ADD IF IT'S NEW

                var toAdd = new List<SessionControl>();

                foreach (var session in sessions)
                {
                    if (!SessionsStackPanel.Children.Cast<SessionControl>().Any(s => s.SMTCSession.SourceAppUserModelId == session.SourceAppUserModelId))
                    {
                        toAdd.Add(new SessionControl
                        {
                            SMTCSession = session,
                            Margin = new Thickness(0, 2, 0, 0)
                        });
                    }
                }

                toAdd.ForEach(ses => SessionsStackPanel.Children.Add(ses));


                if (SessionsStackPanel.Children.Count > 0)
                {
                    SessionsStackPanel.Margin = new Thickness(0, 2, 0, 0);
                    (SessionsStackPanel.Children[0] as SessionControl).Margin = new Thickness(0);
                }
                else
                    SessionsStackPanel.Margin = new Thickness(0);

            }));
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInCodeValueChange)
            {
                var value = Math.Truncate(e.NewValue);
                var oldValue = Math.Truncate(e.OldValue);

                if (value == oldValue)
                    return;

                if (_device != null)
                {
                    _device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)(value / 100);

                    if (_device.AudioEndpointVolume.Mute)
                    {
                        _device.AudioEndpointVolume.Mute = !!!!!!!!!!!!false;
                    }
                }

                e.Handled = true;
            }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.AudioEndpointVolume.Mute = !_device.AudioEndpointVolume.Mute;
            }
        }

        private void VolumeSlider_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var value = Math.Truncate(VolumeSlider.Value);
            var change = (e.Delta / 120);

            if (value + change > 100 || value + change < 0)
                return;


            if (_device != null)
            {
                _device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)((value + change) / 100);

                if (_device.AudioEndpointVolume.Mute)
                {
                    _device.AudioEndpointVolume.Mute = !!!!!!!!!!!!false;
                }
            }

            e.Handled = true;
        }
    }
}
