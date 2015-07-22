using System.Windows;
using SWF = System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace CipherPad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool unsavedChanges = false;

        public MainWindow()
        {
            InitializeComponent();
            editor.TextChanged += delegate (object sender, TextChangedEventArgs e)
            {
                unsavedChanges = true;
            };
        }

        private void ShowFontDialog(object sender, RoutedEventArgs e)
        {
            using (var fd = new SWF.FontDialog())
            {
                fd.ShowEffects = false;
                fd.Font = new Font(editor.FontFamily.ToString(), (float)(editor.FontSize / 96.0 * 72.0));
                var result = fd.ShowDialog();
                if (result != SWF.DialogResult.Cancel)
                {
                    editor.FontFamily = new System.Windows.Media.FontFamily(fd.Font.Name);
                    editor.FontSize = fd.Font.Size * 96.0 / 72.0;
                    editor.FontWeight = fd.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
                    editor.FontStyle = fd.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
                }
            }
        }

        private void LoadFile(string path)
        {
            // TODO detect encoding
            editor.Text = File.ReadAllText(path);
            unsavedChanges = false;
        }

        private bool PromptUnsavedChanges()
        {
            if (!unsavedChanges) return true;
            var result = MessageBox.Show("Do you want to save changes?", "Save Changes", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Cancel:
                    return false;
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Yes:
                    ShowSaveFileDialog();
                    return true;
                default:
                    return false;
            }
        }

        private void ShowOpenFileDialog()
        {
            if (!PromptUnsavedChanges()) return;
            using (var fd = new SWF.OpenFileDialog())
            {
                if (fd.ShowDialog() == SWF.DialogResult.OK)
                {
                    LoadFile(fd.FileName);
                }
            }
        }

        private void ShowSaveFileDialog()
        {
            using (var fd = new SWF.SaveFileDialog())
            {
                if (fd.ShowDialog() == SWF.DialogResult.OK)
                {
                    SaveFile(fd.FileName);
                }
            }
        }

        private void SaveFile(string filePath)
        {
            File.WriteAllText(filePath, editor.Text);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ToggleWordWrap(object sender, RoutedEventArgs e)
        {
            editor.TextWrapping = (editor.TextWrapping == TextWrapping.Wrap) ? TextWrapping.NoWrap : TextWrapping.Wrap;
            wordwrap.IsChecked = (editor.TextWrapping == TextWrapping.Wrap);
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowOpenFileDialog();
        }
    }
}
