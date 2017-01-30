using System.Windows;
using System.Windows.Controls;

namespace Payments.Adapters {
    public interface IModelWrapper {
        object Item { get; }
    }

    public class ItemModelWrapper : IModelWrapper {
        public object Item { get; }

        public ItemModelWrapper(object item) {
            Item = item;
        }
    }

    public class NoItemModelWrapper : IModelWrapper {
        public object Item { get; } = null;
    }

    public class CreateItemModelWrapper : IModelWrapper {
        public object Item { get; } = null;
    }

    public class ModelWrapperTemplateSelector : DataTemplateSelector {
        public DataTemplate ItemTemplate { get; set; }

        public DataTemplate NoItemTemplate { get; set; }

        public DataTemplate CreateItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item is NoItemModelWrapper) {
                return NoItemTemplate;
            }
            if (item is CreateItemModelWrapper) {
                return CreateItemTemplate;
            }
            return ItemTemplate;
        }
    }
}