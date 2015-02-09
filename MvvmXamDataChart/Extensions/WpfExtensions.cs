using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace MvvmXamDataChart.Extensions
{
    public static class WpfExtensions
    {
        public static void TryAddCollectionChangedHandler(this DependencyPropertyChangedEventArgs sender, NotifyCollectionChangedEventHandler handler)
        {
            if (handler != null)
            {
                var oldCollection = sender.OldValue as INotifyCollectionChanged;
                if (oldCollection != null)
                    oldCollection.CollectionChanged -= handler;
                handler(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                var newIList = sender.NewValue as IList;
                if (newIList == null)
                {
                    var newICollection = sender.NewValue as ICollection;
                    if (newICollection != null)
                        newIList = new ArrayList(newICollection);
                }

                if (newIList != null)
                    handler(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newIList));

                var newCollection = sender.NewValue as INotifyCollectionChanged;
                if (newCollection != null)
                    newCollection.CollectionChanged += handler;
            }
        }

        public static void TryAddPropertyChangedHandler(this DependencyPropertyChangedEventArgs sender, PropertyChangedEventHandler handler)
        {
            if (handler != null)
            {
                var oldProperty = sender.OldValue as INotifyPropertyChanged;
                if (oldProperty != null)
                    oldProperty.PropertyChanged -= handler;

                var newProperty = sender.NewValue as INotifyPropertyChanged;
                if (newProperty != null)
                    newProperty.PropertyChanged += handler;
            }
        }
    }
}
