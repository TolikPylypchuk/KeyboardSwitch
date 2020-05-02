using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class CharMappingView : ReactiveUserControl<CharMappingViewModel>
    {
        public CharMappingView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Layouts, v => v.Layouts.Items)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ItemsControl Layouts { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.Layouts = this.FindControl<ItemsControl>(nameof(Layouts));
        }
    }
}
