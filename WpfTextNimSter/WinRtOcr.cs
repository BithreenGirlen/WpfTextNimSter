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
            Func<BitmapSource, MemoryStream> BmpSourceToStream
                = (bitmapSource) =>
                {
                    MemoryStream stream = new MemoryStream();
                    System.Windows.Media.Imaging.BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                    bitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapSource));
                    bitmapEncoder.Save(stream);
                    return stream;
                };

            Func<byte[], Task<Windows.Graphics.Imaging.SoftwareBitmap>> ArrayToSoftwareBitmap
                = async (bytes) =>
                {
                    var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                    var outStream = memStream.GetOutputStreamAt(0);
                    var dataWriter = new Windows.Storage.Streams.DataWriter(outStream);

                    dataWriter.WriteBytes(bytes);
                    await dataWriter.StoreAsync();

                    var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(memStream);

                    return await decoder.GetSoftwareBitmapAsync(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
                };

            Func<System.Windows.Media.Imaging.BitmapSource, Task<Windows.Graphics.Imaging.SoftwareBitmap>> BmpSourceToSoftwareBmp
                = async (bitmapSource)
                =>
                {
                    MemoryStream stream = BmpSourceToStream(bitmapSource);
                    Windows.Graphics.Imaging.SoftwareBitmap sofrwateBitmap = await ArrayToSoftwareBitmap(stream.ToArray());
                    return sofrwateBitmap;
                };
            // BitmapSource to string
            Func<System.Windows.Media.Imaging.BitmapSource, Task<string>> BmpToText
                = async (bitmapSource)
                =>
                {
                    Windows.Media.Ocr.OcrEngine ocrEngine = Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages();
                    Windows.Graphics.Imaging.SoftwareBitmap softwareBitmap = await BmpSourceToSoftwareBmp(bitmapSource);
                    Windows.Media.Ocr.OcrResult ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
                    return ocrResult.Text;
                };
            System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Clipboard.GetImage();
            return await BmpToText(bitmapSource);
#endif
        }
    }
}
