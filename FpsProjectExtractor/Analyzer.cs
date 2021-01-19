using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace FpsProjectExtractor
{
    class Analyzer
    {
        private static readonly string RawFilename = "raw.csv";
        private static readonly string WriteFilename = "out_skips.csv";
        private string RawFilePath;
        private string WriteFilePath;
        private string OutDir;
        private bool WrapAround = true;
        private int MaxImage = 133;


        public Analyzer(string outDir)
        {
            OutDir = outDir;
            RawFilePath = Path.Join(outDir, RawFilename);
            WriteFilePath = Path.Join(OutDir, WriteFilename);

        }

        public string Analyze()
        {
            try
            {
                using (var reader = new StreamReader(RawFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    IEnumerable<Record> records = csv.GetRecords<Record>();
                    List<Skip> skipPoints = new List<Skip>();
                    int count = 0;
                    int? prevImageNumber = null;
                    Record? prevRecord = null;
                    foreach (Record record in records)
                    {
                        count++;
                        int imageNumber = record.Result1;
                        if (prevImageNumber != null)
                        {
                            if (prevImageNumber + 1 != imageNumber && prevImageNumber - 1 != imageNumber && prevImageNumber != imageNumber)
                            {
                                bool isWrap = ((prevImageNumber == MaxImage && imageNumber == 1) || (prevImageNumber == 1 || imageNumber == MaxImage));
                                if (WrapAround && !isWrap)

                                {
                                    // skip detected
                                    Skip skip = new Skip(prevRecord.Path, record.Path, prevImageNumber.Value, imageNumber);
                                    skipPoints.Add(skip);
                                }
                            }
                        }
                        prevImageNumber = imageNumber;
                        prevRecord = record;

                    }
                    WriteSkips(skipPoints);
                    return $"Record count: {count}; Skip count: {skipPoints.Count}";
                }
            }
            catch(System.IO.FileNotFoundException e)
            {
                return $"{RawFilePath} not found.";
            }

            //return $"Failed to load {RawFilePath}";
        }

        private void WriteSkips(List<Skip> skips)
        {
            using (var writer = new StreamWriter(WriteFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(skips);
            }
        }
    }
}
