using ReserV6.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ReserV6.Views.Pages
{
    public partial class RoomsPage : INavigableView<RoomsViewModel>
    {
        public RoomsViewModel ViewModel { get; }

        public RoomsPage(RoomsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
