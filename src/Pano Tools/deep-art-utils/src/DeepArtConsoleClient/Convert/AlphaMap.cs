using SkiaSharp;

namespace DeepArtConsoleClient.Convert
{
    internal class AlphaMap
    {
        public static AlphaMap ReadFromFile(string filePath)
        {
            using var image = SKBitmap.Decode(filePath);
            var result = new AlphaMap(image.Width, image.Height);
            for (int col = 0; col < image.Width; col++)
            {
                for (int row = 0; row < image.Height; row++)
                {
                    var alpha = image.GetPixel(col, row).Alpha;
                    if (alpha < 255)
                        result.SetAlpha(col, row, alpha);
                }

            }
            return result;
        }

        public int SourceWidth { get; }
        public int SourceHeight { get; }

        private readonly Dictionary<(int, int), byte> data = new Dictionary<(int, int), byte>();

        protected AlphaMap(int sourceWidth, int sourceHeight)
        {
            SourceWidth = sourceWidth;
            SourceHeight = sourceHeight;
        }


        public void SetAlpha(int x, int y, byte alpha)
        {
            if (x >= SourceWidth)
                throw new IndexOutOfRangeException("X must be less than SourceWidth");
            if (y >= SourceHeight)
                throw new IndexOutOfRangeException("Y must be less than SourceHeight");

            var pos = (x, y);

            if (data.ContainsKey(pos))
                data[pos] = alpha;
            else
                data.Add(pos, alpha);
        }

        public byte? GetAlpha(int x, int y)
        {
            if (data.TryGetValue((x, y), out var res))
                return res;
            return null;
        }

        public bool HasOpacity()
        {
            return data.Any();
        }

        public IEnumerable<(int x, int y, byte alpha)> GetValues()
        {
            foreach (var item in data.ToList())
                yield return new(item.Key.Item1, item.Key.Item2, item.Value);
        }

        public async Task ApplyToFile(string filePath, CancellationToken cancellationToken)
        {
            var tmpResultFileName = Path.GetTempFileName();

            try
            {
                var sourceImageInfo = SKBitmap.DecodeBounds(filePath);
                var imageInfo = new SKImageInfo(sourceImageInfo.Width, sourceImageInfo.Height, sourceImageInfo.ColorType, SKAlphaType.Premul);

                using (var image = SKBitmap.Decode(filePath, imageInfo))
                {
                    foreach (var alphaItem in GetValues())
                    {
                        var originalPixel = image.GetPixel(alphaItem.x, alphaItem.y);
                        image.SetPixel(alphaItem.x, alphaItem.y, new SKColor(originalPixel.Red, originalPixel.Green, originalPixel.Blue, alphaItem.alpha));
                    }

                    using (var output = File.Create(tmpResultFileName))
                    {
                        image.Encode(output, SKEncodedImageFormat.Png, 100);
                        await output.FlushAsync(cancellationToken);
                    }
                }

                if (File.Exists(filePath))
                    File.Delete(filePath);
                File.Move(tmpResultFileName, filePath);
            }
            finally
            {
                if (File.Exists(tmpResultFileName))
                    File.Delete(tmpResultFileName);
            }
        }
    }
}
