using System.IO;
using System.Windows.Media.Imaging;

namespace WpfTextNimSter
{
    internal class WinRtOcr
    {
        public static async Task<string> RecogniseTextInImage()
        {
#if !WINDOWS10_0_17763_0_OR_GREATER
            return "";
#else
            Func<BitmapSource, MemoryStream>
                BmpSourceToStream
                = (bitmapSource) =>
                {
                    MemoryStream stream = new MemoryStream();
                    System.Windows.Media.Imaging.BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                    bitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapSource));
                    bitmapEncoder.Save(stream);
                    return stream;
                };

            Func<byte[], Task<Windows.Graphics.Imaging.SoftwareBitmap>>
                ArrayToSoftwareBitmap
                = async (bytes) =>
                {
                    var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();

                    var dataWriter = new Windows.Storage.Streams.DataWriter(memStream.GetOutputStreamAt(0));
                    dataWriter.WriteBytes(bytes);
                    await dataWriter.StoreAsync();

                    var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(memStream);
                    return await decoder.GetSoftwareBitmapAsync(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Ignore);
                };

            Func<System.Windows.Media.Imaging.BitmapSource, Task<Windows.Graphics.Imaging.SoftwareBitmap>>
                BmpSourceToSoftwareBmp
                = async (bitmapSource)
                =>
                {
                    MemoryStream stream = BmpSourceToStream(bitmapSource);
                    Windows.Graphics.Imaging.SoftwareBitmap sofrwateBitmap = await ArrayToSoftwareBitmap(stream.ToArray());
                    return sofrwateBitmap;
                };

            System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Clipboard.GetImage();
            Windows.Graphics.Imaging.SoftwareBitmap softwareBitmap = await BmpSourceToSoftwareBmp(bitmapSource);

            Windows.Media.Ocr.OcrEngine ocrEngine;
            if (string.IsNullOrEmpty(_languageTag))
            {
                ocrEngine = Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages();
            }
            else
            {
                ocrEngine = Windows.Media.Ocr.OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language(_languageTag));
            }

            Windows.Media.Ocr.OcrResult ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
            return ocrResult.Text;
#endif
        }

#if WINDOWS10_0_17763_0_OR_GREATER
        private static string? _languageTag;

        /// <summary>
        /// Get list of langugaes of which OCR is installed.
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<string, string>> GetAvailableLanguageList()
        {
            List<Tuple<string, string>> tags = [];
            var languages = Windows.Media.Ocr.OcrEngine.AvailableRecognizerLanguages;
            foreach (var language in languages)
            {
                tags.Add(Tuple.Create(language.LanguageTag, language.NativeName));
            }
            return tags;
        }

        public static void SetLanguageByIndex(int index)
        {
            if(index < 0)_languageTag = null;
            else
            {
                var tags = GetAvailableLanguageList();
                if (index >= tags.Count) _languageTag = null;
                else
                {
                    _languageTag = tags.ElementAt(index).Item1;
                }
            }
        }
#endif
    }
}
