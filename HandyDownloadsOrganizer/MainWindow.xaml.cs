using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Reflection;

namespace HandyDownloadsOrganizer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public class HandyDownload
		{
			public string FullFolderPath { get; set; }
			public string RelativePathToSubcategory { get; set; }
			public HandyDownload(string FullFolderPath, string RelativePathToSubcategory)
			{
				this.FullFolderPath = FullFolderPath;
				this.RelativePathToSubcategory = RelativePathToSubcategory;
			}
			public override string ToString()
			{
				return this.RelativePathToSubcategory;				
			}
		}

		//Main category, sub category, handy download
		Dictionary<string, Dictionary<string, List<HandyDownload>>> HandyDownloads = new Dictionary<string, Dictionary<string, List<HandyDownload>>>(StringComparer.InvariantCultureIgnoreCase);

		string rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Dropbox\Other\Handy downloads");
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ShowIndeterminateProgress("Scanning through Handy Downloads folder, please wait...");
			ThreadingInterop.PerformVoidFunctionSeperateThread(() =>
			{
				foreach (var maincategory in Directory.GetDirectories(rootDir))
					foreach (var subcategory in Directory.GetDirectories(maincategory))
						foreach (var handydownloadpath in Directory.GetDirectories(subcategory))
						{
							string maincat = Path.GetFileName(maincategory);
							string subcat = Path.GetFileName(subcategory);

							if (!HandyDownloads.ContainsKey(maincat))
								HandyDownloads.Add(maincat, new Dictionary<string,List<HandyDownload>>(StringComparer.InvariantCultureIgnoreCase));
							if (!HandyDownloads[maincat].ContainsKey(subcat))
								HandyDownloads[maincat].Add(subcat, new List<HandyDownload>());

							string relativeToSubcategoryPath = handydownloadpath.Substring(subcategory.TrimEnd('\\').Length + 1);
							HandyDownloads[maincat][subcat].Add(
							    new HandyDownload(handydownloadpath, relativeToSubcategoryPath));
						}
				InvokeOnSeparateThread(delegate
				{
					treeviewHandyDownloads.ItemsSource = HandyDownloads;
				});
				HideIndeterminateProgress(true);
			},
			false);
		}

		private void ShowIndeterminateProgress(string message, bool fromSeparateThread = false)
		{
			Action act = delegate
			{
				statusLabel.Content = message;
				progressBarIndeterminate.Visibility = System.Windows.Visibility.Visible;
			};
			if (!fromSeparateThread)
				act();
			else
				InvokeOnSeparateThread(act);
		}

		private void HideIndeterminateProgress(bool fromSeparateThread = false)
		{
			Action act = delegate
			{
				statusLabel.Content = "";
				progressBarIndeterminate.Visibility = System.Windows.Visibility.Hidden;
			};
			if (!fromSeparateThread)
				act();
			else
				InvokeOnSeparateThread(act);
		}

		private void InvokeOnSeparateThread(Action action)
		{
			this.Dispatcher.Invoke(action);
		}

		private void handedownloadBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Border border = sender as Border;
			if (border == null) return;
			HandyDownload handydownload = border.DataContext as HandyDownload;
			dockpanelForSelectedItem.DataContext = new SelectedItemDetails(handydownload);
		}

		public class SelectedItemDetails
		{
			public string Name { get; set; }
			public string DownloadLink { get; set; }
			public SelectedItemDetails(HandyDownload handyDownload)
			{
				this.Name = Path.GetFileName(handyDownload.FullFolderPath);
				this.DownloadLink = File.ReadAllText(Path.Combine(handyDownload.FullFolderPath, "Download link.txt"));
			}
		}
	}
}
