using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class CustomLayoutView : ReactiveUserControl<CustomLayoutViewModel>
    {
        public CustomLayoutView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.Name, v => v.NameTextBox.Text)
                    .DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.Chars, v => v.CharsTextBox.Text)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Delete, v => v.DeleteButton)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TextBox NameTextBox { get; set; } = null!;
        private TextBox CharsTextBox { get; set; } = null!;
        private Button DeleteButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.NameTextBox = this.FindControl<TextBox>(nameof(this.NameTextBox));
            this.CharsTextBox = this.FindControl<TextBox>(nameof(this.CharsTextBox));
            this.DeleteButton = this.FindControl<Button>(nameof(this.DeleteButton));
        }
    }
}
