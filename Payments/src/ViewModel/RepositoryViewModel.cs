using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Payments.Model;
using Payments.Properties;
using Payments.Services;

namespace Payments.ViewModel {
    public class RepositoryViewModel : INotifyPropertyChanged {
        private Repository _repository;

        public ObservableCollection<Wallet> Wallets { get; } = new ObservableCollection<Wallet>();

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();

        public void CreateWallet(string name) {     
            var wallet = _repository.CreateWallet(name);

            Wallets.Add(wallet);
        }

        public void CreatePayment(int walletId, PaymentType type, string name, DateTime date, decimal amount, int[] categories) {
            _repository.CreatePayment(walletId, type, name, date, amount, categories);

            Reload();
        }

        public void CreateCategory(string name) {
            var category = _repository.CreateCategory(name);

            Categories.Add(category);
        }

        public void SetRepository(Repository repo) {
            _repository = repo;

            Reload();
        }

        public void Reload() {
            ReloadWallets();

            Categories.Clear();

            foreach (var category in _repository.GetCategories()) {
                Categories.Add(category);
            }
        }

        public void ReloadWallets() {
            Wallets.Clear();

            foreach (var wallet in _repository.GetWallets()) {
                Wallets.Add(wallet);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
