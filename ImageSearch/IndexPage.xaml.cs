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
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace ImageSearch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        public String INDEX_STORAGE_FOLDER_NAME = "IMAGE_DATA_INDEXES";
        public String FILENAME_INDEX = "INDEX.txt";
        public string FOLDER_WITH_INDEXED_FILES = "INDEXED_FILES";
        public IndexPage()
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
            indexImages();
        }

        public async void indexImages()
        {
            status_indexing.Text = "Files indexing is in progress";

            StorageFolder application_folder = ApplicationData.Current.LocalFolder;
            await application_folder.CreateFolderAsync(INDEX_STORAGE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
            await application_folder.CreateFolderAsync(FOLDER_WITH_INDEXED_FILES, CreationCollisionOption.OpenIfExists);

            StorageFolder indexed_filenames_folder = await application_folder.GetFolderAsync(FOLDER_WITH_INDEXED_FILES);
            StorageFolder index_storage_folder = await application_folder.GetFolderAsync(INDEX_STORAGE_FOLDER_NAME);
            StorageFile index_storage_file = await index_storage_folder.CreateFileAsync(FILENAME_INDEX, CreationCollisionOption.ReplaceExisting);

            List<String> all_lines = new List<string>();
            all_lines.Add("this is first line to be added");
            all_lines.Add("this is second line to be added");

            await Windows.Storage.FileIO.AppendLinesAsync(index_storage_file, all_lines);
            IReadOnlyList<StorageFile> all_camera_files = await KnownFolders.CameraRoll.GetFilesAsync();
            int num_camera_files = all_camera_files.Count;
            int num_files_indexed = 0;

            Ocr ocr = new Ocr();

            foreach (StorageFile file in all_camera_files)
            {
                Boolean file_exists = false;
                num_files_indexed++;

                try
                {
                    StorageFile file_indexed_proof = await indexed_filenames_folder.GetFileAsync(file.Name + ".txt");
                    file_exists = true;
                }
                catch(FileNotFoundException)
                {

                }

                if(!file_exists)
                {
                    string text_recognised = await ocr.Recognise(file);

                    if (text_recognised.Length > 0)
                    {
                        text_recognised = text_recognised.ToLower();
                        string file_name = file.Name.Substring(0, file.Name.Length - 4) + ".txt";
                        StorageFile text_storage_file = await index_storage_folder.CreateFileAsync(file_name, CreationCollisionOption.ReplaceExisting);
                        await Windows.Storage.FileIO.AppendTextAsync(text_storage_file, text_recognised);
                    }

                    await indexed_filenames_folder.CreateFileAsync(file.Name + ".txt");
                }

                if (num_files_indexed % 10 == 0)
                {
                    index_progress.Value = ((num_files_indexed) * 100) / (num_camera_files);
                }

            }

            index_progress.Value = 100;
            status_indexing.Text = " Done with indexing ";
            goto_searchpage_btn.IsEnabled = true;
        }

        private void goto_searchpage_btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
        }
    }
}
