using System;
using System.Windows;
using Payments.Services;
using Payments.ViewModel;
using SimpleInjector;

namespace Payments.xaml {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {


        public IServiceProvider Services { get; }

        public App() {
            var container = new Container();
            Services = container;

            container.Register<CurrentBookViewModel, CurrentBookViewModel>(Lifestyle.Singleton);
            container.Register<RepositoryViewModel, RepositoryViewModel>(Lifestyle.Singleton);

            container.Register<CreateWalletViewModel, CreateWalletViewModel>(Lifestyle.Transient);
            container.Register<CreatePaymentViewModel, CreatePaymentViewModel>(Lifestyle.Transient);

            container.Register<BookService, BookService>(Lifestyle.Singleton);

            container.Verify();
        }
    }
}
