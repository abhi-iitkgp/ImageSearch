using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageSearch
{
    class Indexer
    {
        public String INDEX_STORAGE_FOLDER_NAME = "IMAGE_TEXT_INDEXES";
        public String FILENAME_INDEX = "INDEX.txt";
        public Indexer()
        {

        }

        public async void indexImages()
        {
            StorageFolder application_folder = ApplicationData.Current.LocalFolder;
            await application_folder.CreateFolderAsync(INDEX_STORAGE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);

            StorageFolder index_storage_folder = await application_folder.GetFolderAsync(INDEX_STORAGE_FOLDER_NAME);
            StorageFile index_storage_file = await index_storage_folder.CreateFileAsync(FILENAME_INDEX, CreationCollisionOption.ReplaceExisting);

            List<String> all_lines = new List<string>();
            all_lines.Add("this is first line to be added");
            all_lines.Add("this is second line to be added");

            await Windows.Storage.FileIO.AppendLinesAsync(index_storage_file, all_lines);
            IReadOnlyList<StorageFile> all_camera_files = await KnownFolders.CameraRoll.GetFilesAsync();

            Ocr ocr = new Ocr();

            foreach(StorageFile file in all_camera_files)
            {
                string text_recognised = await ocr.Recognise(file);

                if(text_recognised.Length > 0)
                {
                    text_recognised = text_recognised.ToLower();
                    string file_name = file.Name.Substring(0, file.Name.Length - 4) + ".txt";
                    StorageFile text_storage_file = await index_storage_folder.CreateFileAsync(file_name, CreationCollisionOption.ReplaceExisting);
                    await Windows.Storage.FileIO.AppendTextAsync(text_storage_file, text_recognised);
                }
            }
        }
    }
}
