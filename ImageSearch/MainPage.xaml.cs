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
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Media.Ocr;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Activation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace ImageSearch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        private OcrEngine ocrEngine;
        WriteableBitmap bitmap;
        ImageProperties img_properties;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            ocrEngine = new OcrEngine(OcrLanguage.English);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("Assets\\SQuotes.jpg");
            LoadImage(file);      
        }
        
        private async void LoadImage(Windows.Storage.StorageFile file)
        {
            img_properties = await file.Properties.GetImagePropertiesAsync();
            var file_stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            bitmap = new WriteableBitmap((int)img_properties.Width, (int)img_properties.Height);
            bitmap.SetSource(file_stream);
            img_selected.Source = bitmap;
        }
        
        private async void btn_getText_Click(object sender, RoutedEventArgs e)
        {
            String extracted_text = "";            
            var ocrResult = await ocrEngine.RecognizeAsync(img_properties.Height, img_properties.Width, bitmap.PixelBuffer.ToArray());

            if(ocrResult.Lines != null)
            {
                foreach (OcrLine ocrLine in ocrResult.Lines)
                {
                    var ocrWords = ocrLine.Words;

                    foreach (OcrWord ocrWord in ocrWords)
                    {
                        extracted_text += ocrWord.Text;
                        extracted_text += " ";
                    }

                    extracted_text += "\r\n";
                }

                txt_extracted.Text = extracted_text;
            }
        }

        private void btn_filepick_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker file_picker = new FileOpenPicker();
            file_picker.ViewMode = PickerViewMode.Thumbnail;
            file_picker.FileTypeFilter.Add(".jpg");
            file_picker.FileTypeFilter.Add(".jpeg");
            file_picker.FileTypeFilter.Add(".png");
            file_picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            file_picker.PickSingleFileAndContinue();
        }

        public void filepick_continue(FileOpenPickerContinuationEventArgs args)
        {
            if(args.Files.Count > 0)
            {
                LoadImage(args.Files[0]);
            }
        }
    }
}