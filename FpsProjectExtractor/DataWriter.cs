using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;

namespace FpsProjectExtractor
{
    class DataWriter
    {
        public DataWriter()
        {

        }

        public void Write(List<Record> records, string destPath)
        {
            foreach(Record record in records)
            {
                (record.Result1, record.Result2) = Parse(record.Text);
                
            }

            using (var writer = new StreamWriter(destPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }

        //private void WriteResults(List<string> fileNames, List<string> results)
        //{
        //    List<string> lines = new List<string>(fileNames.Count);
        //    for (int i = 0; i < results.Count; i++)
        //    {
        //        string parsed = Parse(results[i]);
        //        lines.Add(CreateLine(fileNames[i], parsed));
        //    }
        //    System.IO.File.WriteAllLines(System.IO.Path.Join(OutDir, "out.csv"), lines);

        //    using (var writer = new StreamWriter("path\\to\\file.csv"))
        //    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        //    {
        //        csv.WriteRecords(records);
        //    }
        //}

        private (int, int) Parse(string result)
        {
            // Define a regular expression for repeated words.
            Regex rx = new Regex(@"\b\d+\b",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find matches.
            MatchCollection matches = rx.Matches(result);
            if (matches.Count == 1)
            {
                return (Int32.Parse(matches[0].Value), -1);
            }
            else if (matches.Count > 1)
            {
                return (Int32.Parse(matches[0].Value), Int32.Parse(matches[1].Value));
                
            }
            return (-1,-1);
        }
    }
}
