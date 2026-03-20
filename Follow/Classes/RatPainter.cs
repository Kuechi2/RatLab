using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace Follow
{
    public class RatImagePainter
    {
        private readonly Canvas _canvas;
        private readonly int _blockSize = 8;
        private readonly int _ratCount;
        public bool fastdraw = false;

        public RatImagePainter(Canvas canvas, int ratCount = 2)
        {
            _canvas = canvas;
            _ratCount = ratCount;
        }

        public async Task PaintImage(string imagePath)
        {
            BitmapSource bitmap = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            FormatConvertedBitmap grayBitmap = new FormatConvertedBitmap(bitmap, System.Windows.Media.PixelFormats.Gray8, null, 0);
            int widthInBlocks = (int)grayBitmap.PixelWidth / _blockSize;
            int heightInBlocks = (int)grayBitmap.PixelHeight / _blockSize;
            byte[] pixels = new byte[grayBitmap.PixelWidth * grayBitmap.PixelHeight];
            grayBitmap.CopyPixels(pixels, grayBitmap.PixelWidth, 0);
            List<Task> ratTasks = new List<Task>();
            int blocksPerRat = widthInBlocks / _ratCount;
            for (int i = 0; i < _ratCount; i++)
            {
                int startX = i * blocksPerRat;
                int endX = (i == _ratCount - 1) ? widthInBlocks : (i + 1) * blocksPerRat;

                ratTasks.Add(ProcessSection(startX, endX, heightInBlocks, grayBitmap.PixelWidth, pixels));
            }
            await Task.WhenAll(ratTasks);
        }

        private async Task ProcessSection(int startBlockX, int endBlockX, int heightInBlocks, int pixelWidth, byte[] pixels)
        {
            Rat worker = new Rat(_canvas, 3);
            worker.Width = 20; worker.Height = 20;
            for (int by = 0; by < heightInBlocks; by++)
            {
                for (int bx = startBlockX; bx < endBlockX; bx++)
                {
                    double brightness = GetBlockBrightness(bx, by, pixelWidth, pixels);
                    int level = 15 - (int)(brightness / 16);

                    if (level > 0)
                    {
                        await DrawPattern(worker, bx * _blockSize, by * _blockSize, level);
                    }
                }
            }
        }

        private double GetBlockBrightness(int bx, int by, int pixelWidth, byte[] pixels)
        {
            double sum = 0;
            for (int y = 0; y < _blockSize; y++)
                for (int x = 0; x < _blockSize; x++)
                    sum += pixels[(by * _blockSize + y) * pixelWidth + (bx * _blockSize + x)];

            return sum / (_blockSize * _blockSize);
        }

        private async Task DrawPattern(Rat rat, int x, int y, int level)
        {
            rat.Pos(x, y);
            if (level >= 1) await DrawLine(rat, x, y + 2, 8, 0);
            if (level >= 3) await DrawLine(rat, x, y + 6, 8, 0);
            if (level >= 5) await DrawLine(rat, x + 2, y, 8, 90);
            if (level >= 7) await DrawLine(rat, x + 6, y, 8, 90);
            if (level >= 9) await DrawLine(rat, x, y + 4, 8, 0);
            if (level >= 11) await DrawLine(rat, x + 4, y, 8, 90);
            if (level >= 13) await DrawLine(rat, x, y, 8, 0);
            if (level >= 15) await DrawLine(rat, x, y + 8, 8, 0);
            await rat.Warte();
        }

        private async Task DrawLine(Rat rat, double x, double y, double length, double angle)
        {
            rat.Pos(x, y);
            rat.Winkel(angle);
            rat.Vor(length);
            if(fastdraw) rat.Schnell();
        }
    }
}