using ReserV6.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ReserV6.Views.Pages
{
    public partial class UsersPage : INavigableView<UsersViewModel>
    {
        public UsersViewModel ViewModel { get; }

        public UsersPage(UsersViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
