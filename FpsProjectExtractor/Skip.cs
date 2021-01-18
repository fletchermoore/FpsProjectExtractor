using System;
using System.Collections.Generic;
using System.Text;

namespace FpsProjectExtractor
{
    class Skip
    {
        public string StartPath { get; set; }
        public string EndPath { get; set; }
    public int StartImage { get; set; }
public int EndImage { get; set; }

public Skip(string startPath, string endPath, int startImage, int endImage)
        {
            StartPath = startPath;
            EndPath = endPath;
            StartImage = startImage;
            EndImage = endImage;
        }
    }
}
