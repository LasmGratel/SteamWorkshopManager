using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using SteamWorkshopManager.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamWorkshopManager.Pages
{
    public sealed partial class CreateNewCollectionDialog : ContentDialog
    {
        public CreateNewCollectionDialogViewModel ViewModel { get; }

        public CreateNewCollectionDialog(Workshop? app = null)
        {
            ViewModel = new CreateNewCollectionDialogViewModel();
            this.InitializeComponent();
            ViewModel.App = app;
            ViewModel.CanEditApp = app == null;
        }

        private void CreateNewCollectionDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(ViewModel.Name))
            {
                ViewModel.ShowError = true;
                ViewModel.ErrorInfo = "Name is empty";

                args.Cancel = true;
            }
            else if (ViewModel.App == null)
            {
                ViewModel.ShowError = true;
                ViewModel.ErrorInfo = "Game not selected";

                args.Cancel = true;
            }

            AppContext.CollectionDatabase.Insert(new WorkshopCollection
            {
                AppId = ViewModel.App!.AppId,
                CreatedDate = DateTime.Now,
                Name = ViewModel.Name!,
                ImageUrl = ViewModel.App.CapsuleImageUrl,
            });
        }

        private bool _chosenSelection;

        private void GameSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.ResetFilter(sender.Text);
            if (!_chosenSelection)
                ViewModel.App = null;
            else
                _chosenSelection = false;
        }

        private void GameSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ViewModel.App = (Workshop)args.SelectedItem;
            _chosenSelection = true;
        }

        private void GameSuggestBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            GameSuggestBox.IsSuggestionListOpen = true;
        }

        private void GameSuggestBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            GameSuggestBox.IsSuggestionListOpen = false;
        }
    }
}
