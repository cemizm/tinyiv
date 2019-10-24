using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TinyIV.App.ViewModels;

namespace TinyIV.App.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public MainWindowViewModel ViewModel { get { return (MainWindowViewModel)DataContext; } }

        private void OnFileDialogButtonClick(object sender, RoutedEventArgs e)
        {
            ShowFileDialog();
        }

        private async Task ShowFileDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "";
            
            if(!string.IsNullOrEmpty(ViewModel.Filename))
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(ViewModel.Filename);
            else
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            dialog.InitialFileName = "pvserve.txt";

            ViewModel.Filename = await dialog.ShowAsync(this);
        }
    }
}