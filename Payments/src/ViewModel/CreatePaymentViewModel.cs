using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Payments.Command;
using Payments.Model;
using Payments.Properties;

namespace Payments.ViewModel {
    public class CreatePaymentViewModel : INotifyPropertyChanged {
        private readonly RepositoryViewModel _repositoryViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public CreatePaymentViewModel(RepositoryViewModel repositoryViewModel) {
            _repositoryViewModel = repositoryViewModel;
            CreateCommand = new InlineCommand(o => OnCreate((Window)o));
            CancelCommand = new InlineCommand(o => OnCancel((Window)o));

            Wallet = _repositoryViewModel.Wallets.FirstOrDefault();
            Date = DateTime.Today;
            Amount = 0;
        }

        public ICommand CreateCommand { get; }

        public ICommand CancelCommand { get; }

        public PaymentType Type { get; set; }

        public bool IsIncome {
            get { return Type == PaymentType.Income; }
            set {
                if (value) {
                    Type = PaymentType.Income;
                    OnPropertyChanged(nameof(IsIncome));
                    OnPropertyChanged(nameof(IsOutcome));
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public bool IsOutcome {
            get { return Type == PaymentType.Outcome; }
            set {
                if (value) {
                    Type = PaymentType.Outcome;
                    OnPropertyChanged(nameof(IsIncome));
                    OnPropertyChanged(nameof(IsOutcome));
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public Wallet Wallet { get; set; }

        public Category Category { get; set; }

        public decimal Amount { get; set; }

        private void OnCreate(Window window) {
            var categories = Category == null ? new int[0] : new[] {Category.Id};

            _repositoryViewModel.CreatePayment(Wallet.Id, Type, Name, Date, Amount, categories);

            window.Close();
        }

        private void OnCancel(Window window) {
            window.Close();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}