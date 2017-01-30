using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Payments.Adapters;
using Payments.Command;
using Payments.Model;
using Payments.Properties;
using Payments.Services;
using Payments.xaml.windows;
using CreateWalletWindow = Payments.xaml.windows.CreateWalletWindow;

namespace Payments.ViewModel {
    public class CurrentBookViewModel : INotifyPropertyChanged {
        private readonly BookService _bookService;
        private IModelWrapper _currentWallet;
        private IModelWrapper _currentCategory;
        private Repository _repository;

        public event PropertyChangedEventHandler PropertyChanged;

        public CurrentBookViewModel(BookService bookService, RepositoryViewModel repository) {
            _bookService = bookService;
            Repository = repository;
            CreateBookCommand = new InlineCommand(OnCreateBook);
            OpenBookCommand = new InlineCommand(OnOpenBook);

            CreateWalletCommand = new InlineCommand(o => {
                var window = new CreateWalletWindow();
                window.Show();
            });

            CreatePaymentCommand = new InlineCommand(o => {
                var window = new CreatePaymentWindow();
                window.Show();
            });
        }

        public ICommand CreateBookCommand { get; }

        public ICommand OpenBookCommand { get; }

        public ICommand CreateWalletCommand { get; }

        public ICommand CreatePaymentCommand { get; }

        public RepositoryViewModel Repository { get; }

        public bool HasBook { get; set; }

        public ObservableCollection<Payment> CurrentWalletPayments { get; } = new ObservableCollection<Payment>();

        public DynamicCollection Wallets { get; private set; }

        public DynamicCollection Categories { get; private set; }

        public IModelWrapper CurrentWallet {
            get { return _currentWallet; }
            set {
                _currentWallet = value;
                OnPropertyChanged(nameof(CurrentWallet));

                RefreshPayments();
            }
        }

        public IModelWrapper CurrentCategory {
            get { return _currentCategory; }
            set {
                if (value is CreateItemModelWrapper) {
                    _currentCategory = null;
                    new CreateCategoryWindow().Show();
                }
                else {
                    _currentCategory = value;
                }

                OnPropertyChanged(nameof(CurrentCategory));

                RefreshPayments();
            }
        }

        private void RefreshPayments() {
            CurrentWalletPayments.Clear();

            var walletId = ((CurrentWallet as ItemModelWrapper)?.Item as Wallet)?.Id;
            var categoryId = ((CurrentCategory as ItemModelWrapper)?.Item as Category)?.Id;

            foreach (var payment in _repository.GetPayments(walletId, categoryId)) {
                CurrentWalletPayments.Add(payment);
            }
        }

        private void OnCreateBook(object o) {
            var dlg = new Microsoft.Win32.SaveFileDialog {
                FileName = "Новая книга",
                DefaultExt = ".pbook",
                Filter = "Книги (.pbook)|*.pbook"
            };

            var result = dlg.ShowDialog();

            if (result != true) {
                return;
            }

            var repo = _bookService.CreateBook(dlg.FileName);

            SetRepository(repo);
        }

        private void OnOpenBook(object o) {
            var dlg = new Microsoft.Win32.OpenFileDialog() {
                Filter = "Книги (.pbook)|*.pbook"
            };

            var result = dlg.ShowDialog();

            if (result != true) {
                return;
            }

            var repo = _bookService.OpenBook(dlg.FileName);

            SetRepository(repo);
        }

        private void SetRepository(Repository repo) {
            HasBook = true;
            _repository = repo;
            Repository.SetRepository(repo);

            Wallets = new DynamicCollection(Repository.Wallets, true, false);
            Categories = new DynamicCollection(Repository.Categories, true, true);

            OnPropertyChanged(nameof(HasBook));
            OnPropertyChanged(nameof(Wallets));
            OnPropertyChanged(nameof(Categories));
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}