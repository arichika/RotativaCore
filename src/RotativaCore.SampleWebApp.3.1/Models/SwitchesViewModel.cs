using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RotativaCore.Options;

namespace RotativaCore.SampleWebApp._3._1.Models
{ 
    public class SwitchesViewModel
    {
        public string DocTitle { get; set; }
        public string DocContent { get; set; }
        public string Filename { get; set; }
        public Margins Margins { get; set; }
        public Size? PageSize { get; set; }
        public double? PageWidth { get; set; }
        public double? PageHeight { get; set; }
        public Orientation? PageOrientation { get; set; }
        public bool IsLowQuality { get; set; }
        public int? MinimumFontSize { get; set; }
    }
}