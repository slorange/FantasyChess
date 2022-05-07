using FantasyChess.Models;
using FantasyChess.ViewModels;
using FantasyChess.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FantasyChess.Views
{
	public partial class ChessPage : ContentPage
	{
		ChessViewModel _viewModel;

		public ChessPage()
		{
			InitializeComponent();

			BindingContext = _viewModel = new ChessViewModel();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_viewModel.OnAppearing();
		}
	}
}