using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MvvmXamDataChart.Extensions
{
    public static class UiExtensions
    {
        public static DependencyObject GetDataTemplate(this DependencyObject obj, object model)
        {
            if (model != null)
            {
                var type = model.GetType();
                var element = obj as FrameworkElement;
                if (element != null)
                {
                    var key = new DataTemplateKey(type);
                    var template = element.TryFindResource(key) as DataTemplate;
                    return template.MakeObject(model);
                }
            }
            return null;
        }

        public static FrameworkElement MakeObject(this DataTemplate template, object dataContext)
        {
            if (template != null)
            {
                var control = template.LoadContent() as FrameworkElement;
                if (control != null)
                {
                    var binding = new Binding { Source = dataContext };
                    control.SetBinding(FrameworkElement.DataContextProperty, binding);
                    return control;
                }
            }
            return null;
        }

        public static bool IsPropertyValueEquals(this FrameworkElement element, DependencyProperty property, object value)
        {
            if (element != null && property != null)
            {
                if (element.GetValue(property) == value)
                    return true;

                var expression = element.GetBindingExpression(property);
                if (expression != null && expression.DataItem == value)
                    return true;
            }
            return false;
        }

        public static T FindParent<T>(this DependencyObject child, Predicate<T> predicate)
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null)
            {
                var parent = parentObject as T;
                if (parent != null && predicate(parent))
                    return parent;
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }
            return null;
        }

        public static void AddEventToCommand(this DependencyObject element, string eventName, Binding commandBinding, bool passEventArgsToCommand, IEventArgsConverter converter, object converterParameter)
        {
            if (element != null)
            {
                var trigger = new System.Windows.Interactivity.EventTrigger { EventName = eventName };
                var eventToCommand = new EventToCommand();
                BindingOperations.SetBinding(eventToCommand, EventToCommand.CommandProperty, commandBinding);
                eventToCommand.EventArgsConverter = converter;
                eventToCommand.EventArgsConverterParameter = converterParameter;
                eventToCommand.PassEventArgsToCommand = passEventArgsToCommand;

                trigger.Actions.Add(eventToCommand);

                System.Windows.Interactivity.Interaction.GetTriggers(element).Add(trigger);
            }
        }
    }
}
