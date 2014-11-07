using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace ImageSearch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            retriveAllPhotos();
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                this.matching_images.ItemsSource = null;
                string search_text = search_input.Text.Trim();

                if(search_text.Length == 0)
                {
                    retriveAllPhotos();
                }
                else
                {
                    retrieveResults(search_text);
                }
            }
        }

        private async void retrieveResults(String search_text)
        {
            StorageFolder index_files_folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("IMAGE_TEXT_INDEXES");
            IReadOnlyList<StorageFile> all_text_files = await index_files_folder.GetFilesAsync(); 
            List<ImageItem> dataSource1 = new List<ImageItem>();
            
            foreach(StorageFile file in all_text_files)
            {
                IList<string> text_in_file = await Windows.Storage.FileIO.ReadLinesAsync(file);

                foreach(String line in text_in_file)
                {
                    if(line.Contains(search_text))
                    {
                        String file_name = file.Name.Substring(0, file.Name.Length - 4) + ".jpg";
                        StorageFile matched_picture_file = await KnownFolders.CameraRoll.GetFileAsync(file_name);
                        
                        if(matched_picture_file != null)
                        {
                            BitmapImage bitmap_img = new BitmapImage();
                            StorageItemThumbnail thumbnail = await matched_picture_file.GetThumbnailAsync(ThumbnailMode.ListView);
                            bitmap_img.SetSource(thumbnail);
                            ImageItem img_item = new ImageItem() { Name = matched_picture_file.Name, ImagePath = bitmap_img };
                            dataSource1.Add(img_item);
                        }
                    }
                }
            }

            this.matching_images.ItemsSource = dataSource1;
        }

        private async void retriveAllPhotos()
        {
            IReadOnlyList<StorageFile> all_pictures = await KnownFolders.CameraRoll.GetFilesAsync();
            List<ImageItem> dataSource1 = new List<ImageItem>();

            foreach (StorageFile file in all_pictures)
            {
                BitmapImage bitmap_img = new BitmapImage();
                StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.ListView);
                bitmap_img.SetSource(thumbnail);

                ImageItem img_item = new ImageItem() { Name = file.Name, ImagePath = bitmap_img };
                dataSource1.Add(img_item);
            }

            this.matching_images.ItemsSource = dataSource1;

            new Indexer().indexImages();
        }

        public class ImageItem
        {
            public BitmapImage ImagePath
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }
        }
    }
}
