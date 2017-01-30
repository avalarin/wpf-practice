using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Payments.Command;

namespace Payments.ViewModel {
    public class CreateCategoryViewModel : INotifyPropertyChanged {
        private readonly RepositoryViewModel _repositoryViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public CreateCategoryViewModel(RepositoryViewModel repositoryViewModel) {
            _repositoryViewModel = repositoryViewModel;
            CreateCommand = new InlineCommand(o => OnCreate((Window)o));
            CancelCommand = new InlineCommand(o => OnCancel((Window)o));
        }

        public ICommand CreateCommand { get; }

        public ICommand CancelCommand { get; }

        public string Name { get; set; }

        private void OnCreate(Window window) {
            _repositoryViewModel.CreateCategory(Name);

            window.Close();
        }

        private void OnCancel(Window window) {
            window.Close();
        }

    }
}