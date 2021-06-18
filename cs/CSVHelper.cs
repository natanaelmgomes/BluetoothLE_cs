using System;
using System.Diagnostics;
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
        //private string pathtofile;
        private List<CSVDataFrame> DataReceived;
        private List<CSVDataFrame> LeftDataReceived;
        private List<CSVDataFrame> RightDataReceived;
        private int Idtag;
        private int LeftIdtag, RightIdtag;
        private bool FirstLeft, FirstRight;
        private int FisrtLeftId, FirstRightId;

        bool JustRightFoot;

        public class CSVDataFrame
        {
            public int Id { get; set; }
            public int ReceivedId { get; set; }
            public int ReceivedIdL { get; set; }
            public int ReceivedIdR { get; set; }
            public long TimeStamp { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
            public int D { get; set; }
            public int E { get; set; }
            public int F { get; set; }
        }


        public CSVHelper(bool JustRight)
        {
            storageFolder = ApplicationData.Current.LocalFolder;

            tempFilename = String.Format("{0:yy-MM-dd HH-mm-ss}", DateTime.Now);
                       

            DataReceived = new List<CSVDataFrame>();
            LeftDataReceived = new List<CSVDataFrame>();
            RightDataReceived = new List<CSVDataFrame>();

            JustRightFoot = JustRight;

            Idtag = 0;
            LeftIdtag = 0;
            RightIdtag = 0;

            FirstLeft = JustRightFoot ? true : false;
            //FirstLeft = false;
            FirstRight = false;
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

        public int SaveData(Int16[] values, string side, DateTimeOffset milliseconds)
        {
            //List<CSVDataFrame> records = new List<CSVDataFrame> { };
            //using (var writer = new StreamWriter(new FileStream("C:\\Users\\natan\\temp.csv", FileMode.Create), Encoding.UTF8))
            //{
            //    writer.WriteLine("ID,A,B,C");
            //}

            //List<int> numbers = values
            //.Split(',').Select(int.Parse).ToList();
            CSVDataFrame temp = null;
            //CSVDataFrame temp2 = null;
            if (side == "left")
            {
                FirstLeft = true;
                long timestamp = milliseconds.ToUnixTimeMilliseconds();
                temp = new CSVDataFrame
                {
                    Id = LeftIdtag++,
                    ReceivedId = values[0],
                    TimeStamp = timestamp,
                    A = values[1],
                    B = values[2],
                    C = values[3]
                };
                if (!(FirstLeft && FirstRight))
                {
                    FisrtLeftId = values[0];
                }
            }
            else if (side == "right")
            {
                FirstRight = true;
                long timestamp = milliseconds.ToUnixTimeMilliseconds();
                temp = new CSVDataFrame
                {
                    Id = RightIdtag++,
                    TimeStamp = timestamp,
                    ReceivedId = values[0],
                    A = values[1],
                    B = values[2],
                    C = values[3]
                };
                if (!(FirstLeft && FirstRight))
                {
                    FirstRightId = values[0];
                }
            }

            if (FirstLeft && FirstRight)
            {
                if (side == "left")
                {
                    LeftDataReceived.Add(temp);
                }
                else if (side == "right")
                {
                    RightDataReceived.Add(temp);
                    //if (JustRightFoot)
                    //{
                    //    long timestamp = milliseconds.ToUnixTimeMilliseconds();
                    //temp = new CSVDataFrame
                    //{
                    //    Id = RightIdtag++,
                    //    TimeStamp = timestamp,
                    //    A = values[0],
                    //    B = values[1],
                    //    C = values[2],
                    //    D = values[3],
                    //    E = values[4],
                    //    F = values[5]
                    //};
                    //DataReceived.Add(temp);
                    //        }
                    //}
                }
            }
                return 0;
        }

        public async Task SaveTempCSVAsync()
        {
            //List<CSVDataFrame> records = new List<CSVDataFrame> { };
            //using (var writer = new StreamWriter(new FileStream("C:\\Users\\natan\\temp.csv", FileMode.Create), Encoding.UTF8))
            //{
            //    writer.WriteLine("ID,A,B,C");
            //}
            int count = LeftDataReceived.Count < RightDataReceived.Count ? LeftDataReceived.Count : RightDataReceived.Count;

            //if (LeftDataReceived.Count != RightDataReceived.Count)
            //{
            //    while (LeftDataReceived.Count > RightDataReceived.Count)
            //    {
            //        LeftDataReceived.RemoveAt(LeftDataReceived.Count - 1);
            //    }
            //    while (LeftDataReceived.Count < RightDataReceived.Count)
            //    {
            //        RightDataReceived.RemoveAt(RightDataReceived.Count - 1);
            //    }
            //}

            int kl = 0;
            int kr = 0;

            for (int i = 0; i < count; i++)
            {
                CSVDataFrame temp = new CSVDataFrame
                {
                    Id = i,
                    TimeStamp = RightDataReceived[i-kr].TimeStamp,
                    ReceivedIdL = LeftDataReceived[i-kl].ReceivedId,
                    ReceivedIdR = RightDataReceived[i-kr].ReceivedId,
                    A = LeftDataReceived[i-kl].A,
                    B = LeftDataReceived[i-kl].B,
                    C = LeftDataReceived[i-kl].C,
                    D = RightDataReceived[i-kr].A,
                    E = RightDataReceived[i-kr].B,
                    F = RightDataReceived[i-kr].C,
                };
                DataReceived.Add(temp);

                if (!(LeftDataReceived[i - kl].ReceivedId + 1 == LeftDataReceived[i - kl + 1].ReceivedId))
                {
                    Debug.WriteLine(String.Format("Missing Left {0}", LeftDataReceived[i - kl].ReceivedId + 1));
                    kl++;

                }
                if (!(RightDataReceived[i - kr].ReceivedId + 1 == RightDataReceived[i - kr + 1].ReceivedId))
                {
                    Debug.WriteLine(String.Format("Missing Left {0}", RightDataReceived[i - kr].ReceivedId + 1));
                    kr++;
                }
            }




            string pathtofile = storageFolder.Path;
            rootPage.NotifyUser($"Saving to: {pathtofile}", NotifyType.StatusMessage);
            file = await storageFolder.CreateFileAsync(tempFilename + " temp.csv", CreationCollisionOption.ReplaceExisting);
            //await FileIO.WriteTextAsync(file, "ID, A, B, C" + Environment.NewLine);

            string fileBody = "Time, A, B, C, D" + Environment.NewLine;


            foreach (var row in DataReceived)
            {
                fileBody = fileBody + //row.Id.ToString()        + ", " +
                                      row.TimeStamp.ToString() + ", " +
                                      row.ReceivedIdL.ToString() + ", " +
                                      row.A.ToString()         + ", " +
                                      row.B.ToString()         + ", " +
                                      //row.C.ToString()         + ", " +
                                      row.ReceivedIdR.ToString() + ", " +
                                      row.D.ToString()         + ", " +
                                      row.E.ToString()         + ", " +
                                      //row.F.ToString()  +
                                      Environment.NewLine;
            }

            await FileIO.WriteTextAsync(file, fileBody);

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Comma separated values (CSV) file", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = tempFilename + " data.csv";
            Windows.Storage.StorageFile fileFromPicker = await savePicker.PickSaveFileAsync();
            if (fileFromPicker != null)
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
