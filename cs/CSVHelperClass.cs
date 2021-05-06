using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace GenericBLESensor
{
    public class CSVHelperClass
    {



        public class CSVDataFrame
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }

        public string filename = "file.csv";
        

        public int CreateCSVFile()
        {
            List<CSVDataFrame> records = new List<CSVDataFrame> { };



            using (var writer = new StreamWriter(new FileStream("c:/Temp.csv", FileMode.Create), Encoding.UTF8))
            {
                writer.WriteLine("ID,A,B,C");
            }


            return 0;
        }

    }
}
