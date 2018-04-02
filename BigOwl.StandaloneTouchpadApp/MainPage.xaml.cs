using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BigOwl.StandaloneTouchpadApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private OwlMasterController.Owl _owl;

        ThreadPoolTimer _startupDelayTimer = null;

        ThreadPoolTimer _autoClockTimer = null;
        public int autoActionMinutesInterval = 10;
        DateTime _lastAutoActionInitiated;

        Windows.System.Display.DisplayRequest _displayRequest;

        private TranslateTransform screensaverTranslation;

        DateTime startupDateTime;

        private bool? IsTimerEnabled;

        private Dictionary<int, OwlCommand.Commands> ActionSchedule;


        public MainPage()
        {
            ActionSchedule = new Dictionary<int, OwlCommand.Commands>();
            this.InitializeComponent();

            //don't let the app go idle
            _displayRequest = new Windows.System.Display.DisplayRequest();
            _displayRequest.RequestActive();

            _owl = new OwlMasterController.Owl();

            _owl.DeviceError += _component_DeviceError;
            _owl.MoveCompleted += _component_MoveCompleted;

            _owl.DeviceInfoMessage += _owl_DeviceInfoMessage;

            _owl.Initialize();


            //Return the initializer call ASAP

            Task.Factory.StartNew(() =>
            {
                ActionSchedule.Add(0, OwlCommand.Commands.RandomFull);
                ActionSchedule.Add(10, OwlCommand.Commands.HeadLeft);
                ActionSchedule.Add(20, OwlCommand.Commands.HeadRight);
                ActionSchedule.Add(30, OwlCommand.Commands.Surprise);
                ActionSchedule.Add(40, OwlCommand.Commands.Wink);
                ActionSchedule.Add(50, OwlCommand.Commands.SmallWiggle);



                StringBuilder sbList = new StringBuilder();
                sbList.AppendLine("SCHEDULE");
                sbList.AppendLine("=======================================");
                ActionSchedule.ToList().ForEach(v =>
                {
                    String line = String.Format(":{0} - {1}", v.Key.ToString("00"), v.Value.ToString());
                    sbList.AppendLine(line);
                });
                this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    startupDateTime = DateTime.Now;
                    _startupDelayTimer = ThreadPoolTimer.CreatePeriodicTimer(__startupDelayTimer_Tick, TimeSpan.FromMilliseconds(500));
                    _autoClockTimer = ThreadPoolTimer.CreatePeriodicTimer(_clockTimer_Tick, TimeSpan.FromMilliseconds(5000));
                    SetStatusLight(Colors.Green);
                    scheduleTextBlock.Text = sbList.ToString();
                });
            });
        }

        #region ############################# STARTUP Screen

        private void __startupDelayTimer_Tick(ThreadPoolTimer timer)
        {
            double secondsRemaining = (startupDateTime.AddSeconds(20) - DateTime.Now).TotalSeconds;
            if (secondsRemaining > 0)
            {
                this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    startupStatusLabel.Text = "Calibration starting in " + ((int)secondsRemaining).ToString() + " seconds.";
                });
            }
            else
            {
                this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SetStatusLight(Colors.Orange);
                    startupStatusLabel.Text = "CALIBRATING NOW.  PLEASE WAIT.";
                });

                _startupDelayTimer.Cancel();

                CalibrateButton_Click(null, null);
            }
        }

        #endregion

        #region ############################# AUTOMATED Screen

        private void _clockTimer_Tick(ThreadPoolTimer timer)
        {
            string currentTimetext = DateTime.Now.ToShortTimeString();
            if (IsTimerEnabled.GetValueOrDefault())
            {
                bool triggerEvent = false;
                DateTime nextTime = RoundTimeForwardByMinutes(DateTime.Now, autoActionMinutesInterval);

                string nextTimeText = nextTime.ToShortTimeString();

                string nextActionName = "(unkown)";
                if (ActionSchedule.ContainsKey(nextTime.Minute))
                {
                    nextActionName = ActionSchedule[nextTime.Minute].ToString();
                }

                //Update the display. Use a dispatcher if needed
                var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Do something on the dispatcher thread
                    currentTimeValueLabel.Text = currentTimetext;
                    nextEventTimeValueLabel.Text = nextTimeText;
                    nextActionValueLabel.Text = nextActionName;
                });

                DoIntervalAction(DateTime.Now.Minute);
            }
            else
            {
                var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Do something on the dispatcher thread
                    currentTimeValueLabel.Text = currentTimetext;
                    nextEventTimeValueLabel.Text = "(disabled)";
                    nextActionValueLabel.Text = "(disabled)";
                });
            }
        }

        private void DoIntervalAction(int minutes, bool setLastAutoExecutedTime = true)
        {
            if (((DateTime.Now - _lastAutoActionInitiated).Minutes > 2) || !setLastAutoExecutedTime)
            {
                if (ActionSchedule.ContainsKey(minutes))
                {
                    //don't let the automated trigger stack up events
                    if (setLastAutoExecutedTime)
                        _lastAutoActionInitiated = DateTime.Now;

                    Task.Factory.StartNew(() =>
                    {
                        OwlCommand c = new OwlCommand();
                        c.Command = ActionSchedule[minutes];
                        _owl.RunCommand(c);
                    });
                }
            }
        }

        private void DoNextNowButton_Click(object sender, RoutedEventArgs e)
        {
            DoNextAutomatedAction();
        }

        private void ToggleAutoTimeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleAutoTimerOnOff();
        }

        private void DoNextAutomatedAction()
        {
            DateTime nextTime = RoundTimeForwardByMinutes(DateTime.Now, autoActionMinutesInterval);
            //do a preview, do not set the last executed time when we do it
            DoIntervalAction(nextTime.Minute, false);
        }

        private void ToggleAutoTimerOnOff()
        {
            if (IsTimerEnabled.HasValue)
            {
                if (IsTimerEnabled.Value == true)
                {
                    IsTimerEnabled = false;
                    ToggleAutoTimeButton.Content = "Enable Auto";
                    ToggleAutoTimeButton.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    IsTimerEnabled = true;
                    ToggleAutoTimeButton.Content = "Disable Auto";
                    ToggleAutoTimeButton.Background = new SolidColorBrush(Colors.LightBlue);
                }
            }
            else
            {
                ToggleAutoTimeButton.Content = "Unavailable";
            }
        }

        #endregion


        private void _owl_DeviceInfoMessage(object sender, string message)
        {
            SetStatusLabel("DEVICE MESSAGE: " + message);
        }

        private void _component_MoveCompleted(object sender)
        {
            SetStatusLabel("Move Completed");
        }

        private void _component_DeviceError(object sender, string error)
        {
            SetStatusLabel("Error");
        }

        private async void BlinkStatusLight()
        {
            SetStatusLight(Colors.Red);
            await Task.Delay(500);
            SetStatusLight(Colors.Green);
        }

        private async void SetStatusLight(Color c)
        {
            this.statusLight.Fill = new SolidColorBrush(c);
        }

        private void SetStatusLabel(string msg)
        {
            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- " + msg;
                BlinkStatusLight();
            });
        }

        private void RightScrollButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainPivotContainer.SelectedIndex < mainPivotContainer.Items.Count - 1)
                mainPivotContainer.SelectedIndex = mainPivotContainer.SelectedIndex + 1;
            else
                mainPivotContainer.SelectedIndex = 0;
        }

        private void LeftScrollButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainPivotContainer.SelectedIndex > 0)
                mainPivotContainer.SelectedIndex = mainPivotContainer.SelectedIndex - 1;
            else
                mainPivotContainer.SelectedIndex = mainPivotContainer.Items.Count - 1;

        }

        private async void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("CalibrateButton_Click START");
            _startupDelayTimer?.Cancel();

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                startupStatusLabel.Text = "Calibrating now...";
            });

            _owl.Recalibrate();

            IsTimerEnabled = true;

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetStatusLight(Colors.Green);
                CancelAutoCalibrateButton.Visibility = Visibility.Collapsed;
                startupStatusLabel.Text = "CALIBRATING COMPLETE";
                mainPivotContainer.SelectedIndex = 1;
            });

            SetStatusLabel("CalibrateButton_Click FINISHED");
        }

        private async void CancelAutoCalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            _startupDelayTimer?.Cancel();
            CancelAutoCalibrateButton.Visibility = Visibility.Collapsed;
            SetStatusLight(Colors.Orange);
            startupStatusLabel.Text = "Calibration CANCELED.  Please Calibrate before continuing.";
        }


        #region Behavior BUTTONS


        private async void WinkButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WinkButton_Click Start");
            RunEyeTest();
            SetStatusLabel("WinkButton_Click FINISHED");
        }

        private async void WiggleButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WiggleButton_Click Start");
            RunWiggleTest();
            SetStatusLabel("WiggleButton_Click FINISHED");
        }

        private async void HeadLeftButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("HeadLeftButton_Click Start");
            RunHeadLeftTest();
            SetStatusLabel("HeadLeftButton_Click FINISHED");
        }


        private async void HeadRightButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("HeadRightButton_Click Start");
            RunHeadRightTest();
            SetStatusLabel("HeadRightButton_Click FINISHED");
        }

        private async void WingFlapButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WingFlapButton_Click Start");
            RunWingFlapTest();
            SetStatusLabel("WingFlapButton_Click FINISHED");
        }
        private async void HeadTurnButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("HeadTurnButton_Click Start");
            RunHeadRightTest();
            RunHeadLeftTest();
            SetStatusLabel("HeadTurnButton_Click FINISHED");
        }



        #endregion

        #region BEHAVIORS Test


        private async void RunEyeTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.Wink;
            _owl.RunCommand(c);
        }

        private async void RunWiggleTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.SmallWiggle;
            _owl.RunCommand(c);
        }

        private async void RunHeadLeftTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.HeadLeft;
            _owl.RunCommand(c);
        }

        private async void RunHeadRightTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.HeadRight;
            _owl.RunCommand(c);
        }

        private async void RunWingFlapTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.Flap;
            _owl.RunCommand(c);
        }

        #endregion

        #region datetime utilities

        static DateTime RoundTimeForwardByMinutes(DateTime dateTime, int minutes)
        {

            return dateTime.AddMinutes((((dateTime.Minute + minutes - 1) / minutes) * minutes) - dateTime.Minute);
        }

        static DateTime RoundTimeBackwardByMinutes(DateTime dateTime, int minutes)
        {
            return dateTime.AddMinutes(((((dateTime.Minute + minutes) / minutes) * minutes) - minutes) - dateTime.Minute);
        }

        #endregion

        private void ActivateScreensaver()
        {
            screensaverTranslation.X = 100;
            screensaverTranslation.Y = 100;
            ScreensaverImage.Visibility = Visibility.Visible;

        }

        private void DeactivateScreensaver()
        {
            ScreensaverImage.Visibility = Visibility.Collapsed;
        }

        private async void exitButton_Click(object sender, RoutedEventArgs e)
        {
            _owl.Shutdown();

            ContentDialog confirmExitDialog = new ContentDialog
            {
                Title = "Confirm Exit",
                Content = "Are you sure that you want to exit?",
                PrimaryButtonText = "Shutdown the Owl",
                CloseButtonText = "Stay Here"
            };

            ContentDialogResult result = await confirmExitDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Quit
                Application.Current.Exit();
            }
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

    }
}
