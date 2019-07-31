using AudioFlyout.Classes;
using System;
using System.Windows;
using NAudio.CoreAudioApi;
using System.Threading;

namespace AudioFlyout
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static Mutex mutex = new Mutex(true, "{509f191a-0d84-4385-bc3d-fc817e75d2be}AudioFlyout");

        private static AudioDeviceNotificationClient client;
        private static MMDeviceEnumerator enumerator;
        private static MainWindow vlFly;
        private static HookEngine kbh;
        
        WindowInBandWrapper w;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                //You may want to hide Volume HUD (again?)
                VolumeSMTC.ForceFindSMTCAndHide();

                Environment.Exit(0);
                return; //UwU
            }

            VolumeSMTC.ForceFindSMTCAndHide();

            //Flyout
            vlFly = new MainWindow();
            vlFly.HideFlyoutButton.Click += HideFlyoutButton_Click;
            vlFly.MovableAreaBorder.MouseLeftButtonDown += MovableAreaBorder_MouseLeftButtonDown;
            vlFly.MouseEnter += VlFly_MouseEnter;
            vlFly.MouseLeave += VlFly_MouseLeave;

            //Audio Device
            client = new AudioDeviceNotificationClient();
            client.DefaultDeviceChanged += Client_DefaultDeviceChanged;

            enumerator = new MMDeviceEnumerator();
            enumerator.RegisterEndpointNotificationCallback(client);

            if (enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
                vlFly.UpdateDevice(enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia));

            //Windows In Band Wrapper
            w = new WindowInBandWrapper(vlFly);
            w.CreateWindowInBand();
            w.SetWindowPosition(48, 48);

            //Key hook
            kbh = new HookEngine();
            kbh.OnKeyPressed += kbh_OnKeyPressed;
            kbh.HookKeyboard();

            //Kind of shutdown
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        private void VlFly_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => w.MouseLeave();

        private void VlFly_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) => w.MouseEnter();

        private void MovableAreaBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => w.DragMove();

        private void HideFlyoutButton_Click(object sender, RoutedEventArgs e) => w.Hide();

        private void Client_DefaultDeviceChanged(object sender, string e)
        {
            if (e != null)
                vlFly?.UpdateDevice(enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia));
            else
                vlFly?.UpdateDevice(null);
        }

        private void kbh_OnKeyPressed(object sender, VirtualKeyShort e)
        {
            if (e == VirtualKeyShort.VOLUME_UP || e == VirtualKeyShort.VOLUME_DOWN ||
                e == VirtualKeyShort.VOLUME_MUTE ||
                e == VirtualKeyShort.MEDIA_NEXT_TRACK || e == VirtualKeyShort.MEDIA_PREV_TRACK ||
                e == VirtualKeyShort.MEDIA_PLAY_PAUSE || e == VirtualKeyShort.MEDIA_STOP)
                w.Show();
        }
    }
}
