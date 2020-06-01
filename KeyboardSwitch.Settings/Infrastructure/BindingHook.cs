using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Infrastructure
{
    public sealed class BindingHook : IPropertyBindingHook
    {
        private static readonly FuncDataTemplate DefaultItemTemplate = new FuncDataTemplate<object>(
            (x, _) =>
            {
                var control = new ViewModelViewHost { PageTransition = null };
                var context = control.GetObservable(StyledElement.DataContextProperty);
                control.Bind(ViewModelViewHost.ViewModelProperty, context);
                control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                control.VerticalContentAlignment = VerticalAlignment.Stretch;
                return control;
            },
            true);

        public bool ExecuteHook(
            object source,
            object target,
            Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
            Func<IObservedChange<object, object>[]> getCurrentViewProperties,
            BindingDirection direction)
        {
            var viewProperties = getCurrentViewProperties();
            var lastViewProperty = viewProperties.LastOrDefault();

            if (!(lastViewProperty?.Sender is ItemsControl itemsControl))
            {
                return true;
            }

            var propertyName = viewProperties.Last().GetPropertyName();
            if (propertyName != "Items" && propertyName != "ItemsSource")
            {
                return true;
            }

            if (itemsControl.ItemTemplate != null)
            {
                return true;
            }

            itemsControl.ItemTemplate = DefaultItemTemplate;
            return true;
        }
    }
}
