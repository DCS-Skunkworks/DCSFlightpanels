using System;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClassLibraryCommon.CustomControls
{
    /// <summary>
    /// Interaction logic for UserControlSpinningWheel.xaml
    /// </summary>
    public partial class UserControlSpinningWheel : IDisposable
    {
        public static readonly DependencyProperty DoSpinProperty = DependencyProperty.Register(
            nameof(DoSpin), typeof(bool), typeof(UserControlSpinningWheel), new PropertyMetadata(default(bool)));

        private readonly Timer _stopGearTimer = new(5000);
        public bool DoSpin
        {
            get { return (bool)GetValue(DoSpinProperty); }
            set
            {
                SetValue(DoSpinProperty, value);
                ImageConnected.IsEnabled = value;
            }
        }

        public UserControlSpinningWheel()
        {
            InitializeComponent();

            if (DarkMode.DarkModeEnabled)
            {
                ImageConnected.Source = new BitmapImage(new Uri("/ClassLibraryCommon;component/Images/gear-image-darkmode.png", UriKind.Relative));
            }
            _stopGearTimer.Elapsed += TimerStopRotation;
        }

        public void Dispose()
        {
            _stopGearTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
        
        private void TimerStopRotation(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() => ImageConnected.IsEnabled = false));
                _stopGearTimer.Stop();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void Stop()
        {
            _stopGearTimer.Stop();
        }

        public void RotateGear(int howLong = 5000)
        {
            try
            {
                if (ImageConnected.IsEnabled)
                {
                    return;
                }

                ImageConnected.IsEnabled = true;
                if (_stopGearTimer.Enabled)
                {
                    _stopGearTimer.Stop();
                }

                _stopGearTimer.Interval = howLong;
                _stopGearTimer.Start();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
