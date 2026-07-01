using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using Xunit.Abstractions;

namespace RotativaCore.WebTests
{
    public class PdfTester
    {
        private readonly ITestOutputHelper _output;

        private string pdfText;

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
                using var pdfReader = new PdfReader(pdfcontent);
                pdfText = string.Join(
                    Environment.NewLine,
                    Enumerable.Range(1, pdfReader.NumberOfPages)
                        .Select(page => ExtractTextFromPage(pdfReader, page)));

                PdfException = null;
                PdfIsValid = true;
            }
            catch (Exception ex) when (ex is InvalidPdfException || ex is ArgumentException || ex is IOException)
            {
                PdfException = ex;
                PdfIsValid = false;
                pdfText = string.Empty;
            }
        }

        public bool PdfContains(string text)
        {
            if (!PdfIsValid || string.IsNullOrEmpty(pdfText) || string.IsNullOrEmpty(text))
            {
                return false;
            }

            var contains = pdfText.Contains(text, StringComparison.Ordinal);
            if (!contains)
            {
                _output.WriteLine($"Expected text was not found in PDF: {text}");
                _output.WriteLine(pdfText);
            }

            return contains;
        }

        private static string ExtractTextFromPage(PdfReader pdfReader, int pageNumber)
        {
            var pageDictionary = pdfReader.GetPageN(pageNumber);
            var fonts = LoadFonts(pageDictionary);
            var content = pdfReader.GetPageContent(pageNumber);
            var parser = new PdfContentParser(new PrTokeniser(new RandomAccessFileOrArray(content)));
            var operations = new List<PdfObject>();
            var extractedText = new StringBuilder();
            FontDecoder currentFont = null;

            while (parser.Parse(operations).Count > 0)
            {
                if (operations[^1] is not PdfLiteral operation)
                {
                    operations.Clear();
                    continue;
                }

                switch (operation.ToString())
                {
                    case "Tf":
                        currentFont = ResolveFont(operations, fonts);
                        break;
                    case "Tj":
                        AppendDecodedText(extractedText, operations[0], currentFont);
                        break;
                    case "'":
                        AppendDecodedText(extractedText, operations[0], currentFont);
                        break;
                    case "\"":
                        AppendDecodedText(extractedText, operations[2], currentFont);
                        break;
                    case "TJ":
                        AppendDecodedText(extractedText, operations[0], currentFont);
                        break;
                }

                operations.Clear();
            }

            return extractedText.ToString();
        }

        private static Dictionary<string, FontDecoder> LoadFonts(PdfDictionary pageDictionary)
        {
            var fonts = new Dictionary<string, FontDecoder>(StringComparer.Ordinal);
            var resources = pageDictionary?.GetAsDict(PdfName.Resources);
            var fontDictionary = resources?.GetAsDict(PdfName.Font);
            if (fontDictionary == null)
            {
                return fonts;
            }

            foreach (PdfName fontName in fontDictionary.Keys)
            {
                var fontObject = fontDictionary.GetAsDict(fontName) ??
                    PdfReader.GetPdfObject(fontDictionary.Get(fontName)) as PdfDictionary;
                if (fontObject != null)
                {
                    fonts[fontName.ToString()] = CreateFontDecoder(fontObject);
                }
            }

            return fonts;
        }

        private static FontDecoder ResolveFont(IReadOnlyList<PdfObject> operations, IReadOnlyDictionary<string, FontDecoder> fonts)
        {
            if (operations.Count < 3 || operations[0] is not PdfName fontName)
            {
                return null;
            }

            fonts.TryGetValue(fontName.ToString(), out var font);
            return font;
        }

        private static void AppendDecodedText(StringBuilder extractedText, PdfObject operand, FontDecoder currentFont)
        {
            switch (operand)
            {
                case PdfString pdfString:
                    AppendText(extractedText, DecodePdfString(pdfString, currentFont));
                    break;
                case PdfArray pdfArray:
                    foreach (var item in pdfArray.ArrayList.OfType<PdfString>())
                    {
                        AppendText(extractedText, DecodePdfString(item, currentFont));
                    }
                    break;
            }
        }

        private static string DecodePdfString(PdfString pdfString, FontDecoder currentFont)
        {
            var directText = pdfString.ToUnicodeString();
            var directScore = ScoreText(directText);
            var bytes = pdfString.GetBytes();

            if (currentFont == null || bytes.Length == 0)
            {
                return directText;
            }

            var mappedText = DecodeWithFontMap(currentFont, bytes);
            return ScoreText(mappedText) > directScore ? mappedText : directText;
        }

        private static string DecodeWithFontMap(FontDecoder font, byte[] bytes)
        {
            if (font.UnicodeMap.Count == 0 || bytes.Length < font.CodeLength)
            {
                return string.Empty;
            }

            var text = new StringBuilder();
            var limit = bytes.Length - (bytes.Length % font.CodeLength);

            for (var index = 0; index < limit; index += font.CodeLength)
            {
                var code = 0;
                for (var offset = 0; offset < font.CodeLength; offset++)
                {
                    code = (code << 8) | bytes[index + offset];
                }

                if (!font.UnicodeMap.TryGetValue(code, out var mappedText))
                {
                    continue;
                }

                text.Append(mappedText);
            }

            return text.ToString();
        }

        private static FontDecoder CreateFontDecoder(PdfDictionary fontDictionary)
        {
            var toUnicode = PdfReader.GetPdfObject(fontDictionary.Get(PdfName.Tounicode)) as PrStream;
            if (toUnicode == null)
            {
                return FontDecoder.Empty;
            }

            var cmap = Encoding.ASCII.GetString(PdfReader.GetStreamBytes(toUnicode));
            return ParseToUnicodeMap(cmap);
        }

        private static FontDecoder ParseToUnicodeMap(string cmap)
        {
            var unicodeMap = new Dictionary<int, string>();
            var codeLength = ParseCodeLength(cmap);

            foreach (Match block in Regex.Matches(cmap, @"beginbfchar(?<body>.*?)endbfchar", RegexOptions.Singleline))
            {
                foreach (Match entry in Regex.Matches(block.Groups["body"].Value, @"<(?<src>[0-9A-Fa-f]+)>\s*<(?<dst>[0-9A-Fa-f]+)>"))
                {
                    unicodeMap[ParseHexNumber(entry.Groups["src"].Value)] = DecodeUnicodeHex(entry.Groups["dst"].Value);
                }
            }

            foreach (Match block in Regex.Matches(cmap, @"beginbfrange(?<body>.*?)endbfrange", RegexOptions.Singleline))
            {
                foreach (var line in block.Groups["body"].Value
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim()))
                {
                    if (line.Contains("[", StringComparison.Ordinal))
                    {
                        AddArrayRangeMappings(unicodeMap, line);
                        continue;
                    }

                    AddSequentialRangeMappings(unicodeMap, line);
                }
            }

            return new FontDecoder(codeLength, unicodeMap);
        }

        private static int ParseCodeLength(string cmap)
        {
            var codespace = Regex.Match(
                cmap,
                @"begincodespacerange(?<body>.*?)endcodespacerange",
                RegexOptions.Singleline);
            var range = Regex.Match(codespace.Groups["body"].Value, @"<(?<start>[0-9A-Fa-f]+)>\s*<(?<end>[0-9A-Fa-f]+)>");
            return range.Success ? Math.Max(1, range.Groups["start"].Value.Length / 2) : 1;
        }

        private static void AddArrayRangeMappings(Dictionary<int, string> unicodeMap, string line)
        {
            var match = Regex.Match(
                line,
                @"<(?<start>[0-9A-Fa-f]+)>\s*<(?<end>[0-9A-Fa-f]+)>\s*\[(?<targets>[^\]]+)\]");
            if (!match.Success)
            {
                return;
            }

            var start = ParseHexNumber(match.Groups["start"].Value);
            var targets = Regex.Matches(match.Groups["targets"].Value, @"<(?<dst>[0-9A-Fa-f]+)>")
                .Cast<Match>()
                .Select(entry => entry.Groups["dst"].Value)
                .ToArray();

            for (var index = 0; index < targets.Length; index++)
            {
                unicodeMap[start + index] = DecodeUnicodeHex(targets[index]);
            }
        }

        private static void AddSequentialRangeMappings(Dictionary<int, string> unicodeMap, string line)
        {
            var match = Regex.Match(
                line,
                @"<(?<start>[0-9A-Fa-f]+)>\s*<(?<end>[0-9A-Fa-f]+)>\s*<(?<dst>[0-9A-Fa-f]+)>");
            if (!match.Success)
            {
                return;
            }

            var start = ParseHexNumber(match.Groups["start"].Value);
            var end = ParseHexNumber(match.Groups["end"].Value);
            var unicode = ParseHexNumber(match.Groups["dst"].Value);

            for (var code = start; code <= end; code++)
            {
                var codePoint = unicode + (code - start);
                var mappedText = TryConvertFromUtf32(codePoint);
                if (mappedText == null)
                {
                    continue;
                }

                unicodeMap[code] = mappedText;
            }
        }

        private static int ParseHexNumber(string hex) =>
            Convert.ToInt32(hex, 16);

        private static string DecodeUnicodeHex(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
            {
                return string.Empty;
            }

            var bytes = Convert.FromHexString(hex);
            if (bytes.Length % 2 != 0)
            {
                return Encoding.ASCII.GetString(bytes);
            }

            return Encoding.BigEndianUnicode.GetString(bytes);
        }

        private static string TryConvertFromUtf32(int codePoint)
        {
            if (codePoint is < 0 or > 0x10FFFF)
            {
                return null;
            }

            if (codePoint >= 0xD800 && codePoint <= 0xDFFF)
            {
                return null;
            }

            return char.ConvertFromUtf32(codePoint);
        }

        private static int ScoreText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            var score = 0;

            foreach (var character in text)
            {
                if (char.IsLetterOrDigit(character))
                {
                    score += 3;
                    continue;
                }

                if (char.IsWhiteSpace(character))
                {
                    score += 1;
                    continue;
                }

                if (char.IsPunctuation(character) || char.IsSymbol(character))
                {
                    score += 2;
                    continue;
                }

                if (char.IsControl(character))
                {
                    score -= 2;
                }
            }

            return score;
        }

        private static void AppendText(StringBuilder extractedText, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            extractedText.Append(text);
        }

        private sealed class FontDecoder
        {
            public static FontDecoder Empty { get; } = new(1, new Dictionary<int, string>());

            public FontDecoder(int codeLength, Dictionary<int, string> unicodeMap)
            {
                CodeLength = codeLength;
                UnicodeMap = unicodeMap;
            }

            public int CodeLength { get; }
            public IReadOnlyDictionary<int, string> UnicodeMap { get; }
        }
    }
}
