using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.PanelUserControls;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonImage.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonImage : UserControlBase
    {
        private IGlobalHandler _globalHandler;
        private bool _isDirty = false;
        private EnumStreamDeckButtonActionType _actionType;






        public UserControlStreamDeckButtonImage()
        {
            InitializeComponent();
        }
        
        private void UserControlStreamDeckButtonImage_OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        public void SetFormState()
        {
            try
            {
                RadioButtonDCSBIOSOutput.Visibility = _actionType != EnumStreamDeckButtonActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonTextAndStyle.Visibility = RadioButtonText.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonImageToShow.Visibility = RadioButtonImage.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSImage.Visibility = RadioButtonDCSBIOSOutput.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelDCSBIOSBackgroundType.Visibility = RadioButtonDCSBIOSOutput?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonDCSBIOSBackgroundGeneratedImage.Visibility = RadioButtonDCSBIOSBackgroundGenerated.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSBackgroundExistingImage.Visibility = RadioButtonDCSBIOSBackgroundExisting.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonImageType.Visibility = _actionType != EnumStreamDeckButtonActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                StackPanelDCSBIOSBackgroundTypeSelection.Visibility = RadioButtonDCSBIOSOutput.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSBackgroundGeneratedImage.Visibility = RadioButtonDCSBIOSBackgroundGenerated.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSBackgroundExistingImage.Visibility = RadioButtonDCSBIOSBackgroundExisting.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }


        public void Update()
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void Clear()
        {
            _isDirty = false;
        }

        private void MouseDownFocusLogTextBox(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void RadioButtonDCSBIOSImage_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        public EnumStreamDeckButtonActionType ActionType
        {
            get => _actionType;
            set
            {
                _actionType = value;
                SetFormState();
            }
        }

        private void RadioButtonImageType_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        public IGlobalHandler GlobalHandler
        {
            get => _globalHandler;
            set => _globalHandler = value;
        }


        public bool HasConfig
        {
            get
            {
                return false;
            }
        }

        private void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }
    }
}
