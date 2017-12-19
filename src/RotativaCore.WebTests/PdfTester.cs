using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using Xunit.Abstractions;

namespace RotativaCore.WebTests
{
    public class PdfTester
    {
        private readonly ITestOutputHelper _output;

        private byte[] pdfContent;
        private string pdfText;
        private PdfReader pdfReader;

        public PdfTester(ITestOutputHelper output)
        {
            _output = output;
        }

        public bool PdfIsValid { get; set; }
        public Exception PdfException { get; set; }

        public void LoadPdf(byte[] pdfcontent)
        {
            try
            {
                this.pdfReader = new PdfReader(pdfcontent);
                var parser = new PdfParser();
                var parsed = parser.ExtractTextFromPDFBytes(pdfcontent);
                this.PdfIsValid = true;
            }
            catch (InvalidPdfException ex)
            {
                this.PdfException = ex;
                this.PdfIsValid = false;
            }
        }

        public bool PdfContains(string text)
        {
            var buf = new StringBuilder();

            for (var page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                var streamBytes = pdfReader.GetPageContent(1);
                var tokenizer = new PrTokeniser(new RandomAccessFileOrArray(streamBytes));

                var stringsList = new List<string>();
                while (tokenizer.NextToken())
                {
                    if (tokenizer.TokenType == PrTokeniser.TK_STRING)
                    {
                        stringsList.Add(tokenizer.StringValue);
                        _output.WriteLine(stringsList.Last());

                        var currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF8, Encoding.UTF8, Encoding.UTF8.GetBytes(tokenizer.StringValue)));
                        _output.WriteLine(currentText);
                    }
                }
                tokenizer.Close();
            }

            pdfReader.Close();

            //if (stringsList.Contains(text))
            //    eturn true;

            return false;
        }
    }
}
