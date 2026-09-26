using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;

namespace FarmaciaApp.Desktop.Services
{
    // Reemplazo multiplataforma de MessageBox y ShowDialog de WPF.
    public static class Dialogs
    {
        private static Window Owner
        {
            get
            {
                var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                return desktop?.Windows.FirstOrDefault(w => w.IsActive) ?? desktop?.MainWindow;
            }
        }

        public static Task Info(string message, string title = "Información") =>
            Show(message, title, "#0984e3", false);

        public static Task Error(string message, string title = "Error") =>
            Show(message, title, "#d63031", false);

        public static Task<bool> Confirm(string message, string title = "Confirmar") =>
            Show(message, title, "#e17055", true);

        // Abre una ventana de formulario como modal; devuelve true si el formulario se cerro con Close(true).
        public static async Task<bool> ShowForm(Window form)
        {
            var owner = Owner;
            if (owner == null)
            {
                form.Show();
                return false;
            }
            return await form.ShowDialog<bool>(owner);
        }

        private static async Task<bool> Show(string message, string title, string color, bool yesNo)
        {
            var window = new Window
            {
                Title = title,
                Width = 440,
                Background = Brushes.White,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Thickness(0, 20, 0, 0)
            };

            if (yesNo)
            {
                var yes = new Button { Content = "Sí", Width = 90, IsDefault = true };
                var no = new Button { Content = "No", Width = 90, IsCancel = true };
                no.Classes.Add("secundario");
                yes.Click += (_, _) => window.Close(true);
                no.Click += (_, _) => window.Close(false);
                buttons.Children.Add(yes);
                buttons.Children.Add(no);
            }
            else
            {
                var ok = new Button { Content = "Aceptar", Width = 100, IsDefault = true, IsCancel = true };
                ok.Click += (_, _) => window.Close(true);
                buttons.Children.Add(ok);
            }

            window.Content = new StackPanel
            {
                Margin = new Thickness(28, 24),
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontSize = 18,
                        FontWeight = FontWeight.Bold,
                        Foreground = Brush.Parse(color),
                        Margin = new Thickness(0, 0, 0, 10)
                    },
                    new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, FontSize = 14, Foreground = Brush.Parse("#2d3436"), LineHeight = 21 },
                    buttons
                }
            };

            var owner = Owner;
            if (owner == null)
            {
                window.Show();
                return false;
            }
            return await window.ShowDialog<bool>(owner);
        }
    }
}
