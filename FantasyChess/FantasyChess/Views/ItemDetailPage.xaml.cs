using FantasyChess.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace FantasyChess.Views
{
	public partial class ItemDetailPage : ContentPage
	{
		public ItemDetailPage()
		{
			InitializeComponent();
			BindingContext = new ItemDetailViewModel();
		}
	}
}