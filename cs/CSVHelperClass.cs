using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GenericBLESensor
{
    public class CSVHelperClass
    {

        StorageFolder storageFolder;
        StorageFile sampleFile;


        public CSVHelperClass()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        }

        public class CSVDataFrame
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }

        public string filename = "file.csv";



        public async Task<int> CreateCSVFileAsync()
        {
            //List<CSVDataFrame> records = new List<CSVDataFrame> { };
            //using (var writer = new StreamWriter(new FileStream("C:\\Users\\natan\\temp.csv", FileMode.Create), Encoding.UTF8))
            //{
            //    writer.WriteLine("ID,A,B,C");
            //}

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            var pathtofile = storageFolder.Path;
            StorageFile sampleFile = await storageFolder.CreateFileAsync("temp.csv", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, "ID,A,B,C\n");

            return 0;
        }

    }
}
