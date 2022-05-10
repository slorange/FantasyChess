using FantasyChess.Models;
using FantasyChess.ViewModels;
using FantasyChess.Views;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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

		private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
		{
			var info = args.Info;
			var surface = args.Surface;
			var canvas = surface.Canvas;

			canvas.Clear();

			// In this example, we will draw a circle in the middle of the canvas
			var paint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = Color.Red.ToSKColor(), // Alternatively: SKColors.Red
			};
			canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);

			//var source = ImageSource.FromResource("ba.png");
			var image = new Image();
			image.Source = "ba.png";
			layout.Children.Add(image, () => new Rectangle(160, 160, 30, 30));
			
			//var x = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			

		}
	}
}