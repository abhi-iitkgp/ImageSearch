using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPreview.Media.Ocr;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ImageSearch
{
    class Ocr
    {
        private OcrEngine ocrEngine;

        public Ocr()
        {
            ocrEngine = new OcrEngine(OcrLanguage.English);
        }

        public async Task<string> Recognise(StorageFile file)
        {
            WriteableBitmap bitmap;
            ImageProperties img_properties = await file.Properties.GetImagePropertiesAsync();
            String extracted_text = "";

            bitmap = new WriteableBitmap((int)img_properties.Width, (int)img_properties.Height);
            bitmap.SetSource(await file.OpenAsync(FileAccessMode.Read));
            
            try
            {
                var ocrResult = await ocrEngine.RecognizeAsync(img_properties.Height, img_properties.Width, bitmap.PixelBuffer.ToArray());
                if (ocrResult.Lines != null)
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
                }            
            }
            catch(Exception e)
            {
                extracted_text = e.ToString();
            }

            return extracted_text;
        }
    }
}