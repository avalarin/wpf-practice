using System;
using System.Windows;
using System.Windows.Markup;

namespace Payments.Adapters {
    public class Resolve : MarkupExtension {
        private readonly Type _service;

        public Resolve(Type service) {
            _service = service;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var app = Application.Current as xaml.App;
            return app?.Services?.GetService(_service);
        }
    }
}