using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
//using Windows.ApplicationModel.Resources;

namespace GenericBLESensor
{
    public class CSVHelper
    {
        private MainPage rootPage = MainPage.Current;
        private StorageFolder storageFolder;
        private StorageFile file;
        private string tempFilename;
        private string pathtofile;
        private List<CSVDataFrame> DataReceived;
        private int Idtag;

        public class CSVDataFrame
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }


        public CSVHelper()
        {
            storageFolder = ApplicationData.Current.LocalFolder;

            tempFilename = String.Format("{0:yy-MM-dd HH-mm-ss}", DateTime.Now);
                       

            DataReceived = new List<CSVDataFrame>();
            Idtag = 0;
        }


        //public async Task<int> CreateCSVFileAsync()
        //{
        //    //List<CSVDataFrame> records = new List<CSVDataFrame> { };
        //    //using (var writer = new StreamWriter(new FileStream("C:\\Users\\natan\\temp.csv", FileMode.Create), Encoding.UTF8))
        //    //{
        //    //    writer.WriteLine("ID,A,B,C");
        //    //}

        //    //storageFolder = ApplicationData.Current.LocalFolder;
        //    // string pathtofile = storageFolder.Path;
        //    rootPage.NotifyUser($"Saving to: {pathtofile}", NotifyType.StatusMessage);
        //    //sampleFile = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        //    //await FileIO.WriteTextAsync(sampleFile, "ID,A,B,C\n");
        //    //Windows.Storage.Streams.IRandomAccessStream stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
        //    //using (Windows.Storage.Streams.IOutputStream outputStream = stream.GetOutputStreamAt(0))
        //    //{
        //    //    using (Windows.Storage.Streams.DataWriter dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
        //    //    {
        //    //        dataWriter.WriteString("ID,A,B,C");
        //    //        //dataWriter.WriteLine();
        //    //        await dataWriter.StoreAsync();
        //    //        await outputStream.FlushAsync();
        //    //    }
                
        //    //}
        //    //stream.Dispose();
        //    return 0;
        //}

        public int SaveData(string values)
        {
            //List<CSVDataFrame> records = new List<CSVDataFrame> { };
            //using (var writer = new StreamWriter(new FileStream("C:\\Users\\natan\\temp.csv", FileMode.Create), Encoding.UTF8))
            //{
            //    writer.WriteLine("ID,A,B,C");
            //}

            List<int> numbers = values.Split(',').Select(int.Parse).ToList();
            CSVDataFrame temp = new CSVDataFrame
            {
                Id = Idtag++,
                A = numbers[0],
                B = numbers[1],
                C = numbers[2]
            };
            DataReceived.Add(temp);

            return 0;
        }

        public async Task SaveTempCSVAsync()
        {
            //List<CSVDataFrame> records = new List<CSVDataFrame> { };
            //using (var writer = new StreamWriter(new FileStream("C:\\Users\\natan\\temp.csv", FileMode.Create), Encoding.UTF8))
            //{
            //    writer.WriteLine("ID,A,B,C");
            //}

            string pathtofile = storageFolder.Path;
            rootPage.NotifyUser($"Saving to: {pathtofile}", NotifyType.StatusMessage);
            file = await storageFolder.CreateFileAsync(tempFilename + " temp.csv", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, "ID, A, B, C" + Environment.NewLine);

            string fileBody = "ID, A, B, C" + Environment.NewLine;


            foreach (var row in DataReceived)
            {
                fileBody = fileBody + row.Id.ToString() + ", " + 
                                      row.A.ToString()  + ", " +
                                      row.B.ToString()  + ", " +
                                      row.C.ToString()  + Environment.NewLine;
            }

            await FileIO.WriteTextAsync(file, fileBody);

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Comma separated values (CSV) file", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = tempFilename + " data.csv";
            Windows.Storage.StorageFile fileFromPicker = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(fileFromPicker);
                // write to file
                await FileIO.WriteTextAsync(fileFromPicker, fileBody);



                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(fileFromPicker);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    rootPage.NotifyUser($"File " + fileFromPicker.Name + " was saved.", NotifyType.StatusMessage);
                }
                else
                {
                    rootPage.NotifyUser($"File " + fileFromPicker.Name + " couldn't be saved.", NotifyType.StatusMessage);
                }
            }
            else
            {
                rootPage.NotifyUser($"Operation cancelled.", NotifyType.StatusMessage);

            }
        }

    }
}
