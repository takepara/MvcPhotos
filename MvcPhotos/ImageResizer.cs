using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace MvcPhotos
{
    public enum ResizeMode
    {
        Original,
        Optimize,
        Fit,
        Clip
    }

    public class ImageResizer
    {
        public static int OptimizeSize = 640;
        public static long OptimizeQuality = 90L;

        //MimeTypeで指定されたImageCodecInfoを探して返す
        public static ImageCodecInfo GetEncoderInfo(string mime)
        {
            //GDI+ に組み込まれたイメージ エンコーダに関する情報をすべて取得
            var encoders = ImageCodecInfo.GetImageEncoders();
            //指定されたMimeTypeを探して見つかれば返す
            return encoders.FirstOrDefault(info => info.MimeType == mime);
        }

        private readonly static Dictionary<string, ImageFormat> FormatMap = new Dictionary<string, ImageFormat>
        {
            {"jpg",ImageFormat.Jpeg},
            {"gif",ImageFormat.Gif},
            {"png",ImageFormat.Png}
        };

        private readonly static Dictionary<string, ImageFormat> MimeMap = new Dictionary<string, ImageFormat>
        {
            {"image/jpeg",ImageFormat.Jpeg},
            {"image/pjpeg",ImageFormat.Jpeg},
            {"image/gif",ImageFormat.Gif},
            {"image/png",ImageFormat.Png},
        };

        public static string GetExtentionToMime(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
                return null;

            ext = ext.Replace(".", "").ToLower();
            var format = FormatMap.ContainsKey(ext) ? FormatMap[ext] : null;
            if (format == null)
                return null;

            return MimeMap.ContainsValue(format) ? MimeMap.First(kv => kv.Value == format).Key : null;
        }

        public static ImageFormat GetMimeToImageFormat(string mime)
        {
            return MimeMap.ContainsKey(mime.ToLower()) ? MimeMap[mime.ToLower()] : null;
        }

        public static string GetMimeToExtension(string mime)
        {
            var format = MimeMap.ContainsKey(mime.ToLower()) ? MimeMap[mime.ToLower()] : null;
            if (format == null || !FormatMap.ContainsValue(format))
                return "unknown";

            return FormatMap.First(kv => kv.Value == format).Key;
        }

        // 比率を維持して計算
        public static int RatioSize(int low, int high, int size)
        {
            var ratio = low / (double)high;
            return (int)(size * ratio);
        }

        // 縦横比を維持したまま長い辺をサイズ指定して縮小サイズを算出
        public static Rectangle FitSize(Rectangle original, int size)
        {
            var resize = new Rectangle(0, 0, original.Width, original.Height);
            // 縦長
            if (original.Width < original.Height && original.Height > size)
            {
                resize.Height = size;
                resize.Width = RatioSize(resize.Height, original.Height, original.Width);
            }
            // 横長
            if (original.Width >= original.Height && original.Width > size)
            {
                resize.Width = size;
                resize.Height = RatioSize(size, original.Width, original.Height);
            }

            return resize;
        }

        // 縦横比を維持したまま短い辺をサイズ指定して縮小サイズを算出
        public static Rectangle ClipSize(Rectangle original, int size)
        {
            var resize = new Rectangle(0, 0, original.Width, original.Height);
            // 縦長
            if (original.Width < original.Height && original.Height > size)
            {
                resize.Width = size;
                resize.Height = RatioSize(size, original.Width, original.Height);
            }
            // 横長
            if (original.Width >= original.Height && original.Width > size)
            {
                resize.Height = size;
                resize.Width = RatioSize(resize.Height, original.Height, original.Width);
            }

            return resize;
        }

        private void InternalBitmapResize(Bitmap source, Bitmap destination, Rectangle resize)
        {
            using (var g = Graphics.FromImage(destination))
            {
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                const InterpolationMode im = InterpolationMode.HighQualityBicubic;
                g.InterpolationMode = im;

                g.FillRectangle(Brushes.White, 0, 0, resize.Width, resize.Height);
                g.DrawImage(source, 0, 0, resize.Width, resize.Height);
            }
        }

        // 縦横比を維持したまま長い辺を指定して縮小
        public Bitmap FitResize(string path, int size)
        {
            Bitmap destinationBitmap;

            using (var sourceBitmap = new Bitmap(path))
            {
                var sourceSize = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                var resize = FitSize(sourceSize, size);

                // 縮小
                destinationBitmap = new Bitmap(resize.Width, resize.Height, PixelFormat.Format24bppRgb);
                InternalBitmapResize(sourceBitmap, destinationBitmap, resize);
            }

            return destinationBitmap;
        }

        public static Rectangle ClipRePositionSource(Rectangle original, int size)
        {
            var position = new Rectangle(0, 0, size, size);

            if (original.Width > size)
                position.X = (original.Width - size) / 2;
            if (original.Height > size)
                position.Y = (original.Height - size) / 2;

            return position;
        }

        public static Rectangle ClipRePositionDestination(Rectangle original, int size)
        {
            var position = new Rectangle(0, 0, size, size);

            // 基本サイズじゃない背景イメージならセンタリング
            if (original.Width < size)
                position.X = (size - original.Width) / 2;
            if (original.Height < size)
                position.Y = (size - original.Height) / 2;

            return position;
        }

        // 縦横比を固定して、短い辺を指定して縮小したものをセンター切り出し
        public Bitmap ClipResize(string path, int size)
        {
            Bitmap destinationBitmap;

            using (var sourceBitmap = new Bitmap(path))
            {
                var sourceSize = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                var resize = ClipSize(sourceSize, size);

                // 縮小
                using (var resizeBitmap = new Bitmap(resize.Width, resize.Height, PixelFormat.Format24bppRgb))
                {
                    InternalBitmapResize(sourceBitmap, resizeBitmap, resize);

                    // 切り取り
                    destinationBitmap = new Bitmap(size, size, PixelFormat.Format24bppRgb);
                    using (var g = Graphics.FromImage(destinationBitmap))
                    {
                        g.FillRectangle(Brushes.White, 0, 0, size, size);

                        // 基本サイズじゃない背景イメージならセンタリング
                        var source = ClipRePositionSource(resize, size);
                        var destination = ClipRePositionDestination(resize, size);

                        g.DrawImage(resizeBitmap, destination, source, GraphicsUnit.Pixel);
                    }
                }
            }

            return destinationBitmap;
        }

        private string GetConcurrentFileName(string fileName)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            return string.Format("{0}.{1}{2}", fileName, threadId, Path.GetExtension(fileName));
        }

        private bool MoveConcurrentFile(string sourcePath, string destPath)
        {
            var deleted = true;
            if (!File.Exists(destPath))
            {
                try
                {
                    File.Move(sourcePath, destPath);
                }
                catch(IOException)
                {
                    // なかなか起こせない．．．。
                    Trace.WriteLine(" file i/o exception! - " + DateTime.Now);
                    deleted = false;
                }
            }

            if (!deleted || File.Exists(sourcePath))
                File.Delete(sourcePath);

            return File.Exists(destPath);
        }

        private bool SaveConcurrentFile(string path, Action<string> save)
        {
            var tempName = GetConcurrentFileName(path);
            
            save(tempName);
            
            return MoveConcurrentFile(tempName, path);
        }

        public bool Resize(string path, ResizeMode mode, int size, string resizePath)
        {
            var mime = GetExtentionToMime(resizePath);

            return Resize(path, mode, size, resizePath, mime);
        }

        public bool Resize(string path, ResizeMode mode, int size, string resizePath, string mime)
        {
            using (var bmp = ResizeBitmap(path, mode, size))
            {
                var ici = GetEncoderInfo(mime);
                var eps = new EncoderParameters(1);
                var ep = new EncoderParameter(Encoder.Quality, OptimizeQuality);
                eps.Param[0] = ep;

                return SaveConcurrentFile(resizePath, fileName =>
                {
                    bmp.Save(fileName, ici, eps);
                });
            }
        }

        public Bitmap ResizeBitmap(string path, ResizeMode mode, int size)
        {
            Bitmap bitmap = null;
            switch (mode)
            {
                case ResizeMode.Fit:
                    bitmap = FitResize(path, size);
                    break;
                case ResizeMode.Clip:
                    bitmap = ClipResize(path, size);
                    break;
            }

            return bitmap;
        }

        public bool IsEmptyFile(string path)
        {
            var fi = new FileInfo(path);
            return fi.Length == 0;
        }

        public bool IsExactOptimize(string path, string mime)
        {
            bool exact = false;
            var format = GetMimeToImageFormat(mime);

            if (format == ImageFormat.Jpeg)
            {
                using (var bmp = new Bitmap(path))
                {
                    if (bmp.Width > OptimizeSize || bmp.Height > OptimizeSize)
                        exact = true;
                }

                if (!exact)
                {
                    if (GetExifOrientation(path) != ExifOrientation.Unknown)
                        exact = true;
                }
            }

            return exact;
        }

        public bool Optimize(string originalPath, string optimizePath)
        {
            var tmpPath = GetConcurrentFileName(originalPath);
            if(Rotate(originalPath, tmpPath))
            {
                Resize(tmpPath, ResizeMode.Fit, OptimizeSize, optimizePath);

                File.Delete(tmpPath);
            }

            return File.Exists(optimizePath);
        }

        private bool Rotate(string originalPath, string optimizePath)
        {
            var mime = GetExtentionToMime(optimizePath);
            var format = GetMimeToImageFormat(mime);

            var orientation = GetExifOrientation(originalPath);
            if (orientation != ExifOrientation.Unknown)
            {
                var functor = new Dictionary<ExifOrientation, Action<Bitmap>>
                                  {
                                      // 左右反転
                                      {ExifOrientation.TopRightSide, src=>src.RotateFlip(RotateFlipType.RotateNoneFlipX)},
                                      // 180度回転
                                      {ExifOrientation.BottomRightSide, src=>src.RotateFlip(RotateFlipType.Rotate180FlipNone)},
                                      // 上下反転
                                      {ExifOrientation.BottomLeftSide, src=>src.RotateFlip(RotateFlipType.RotateNoneFlipY)},
                                      // 90度回転左右反転
                                      {ExifOrientation.LeftSideTop, src=>src.RotateFlip(RotateFlipType.Rotate90FlipX)},
                                      // 90度回転
                                      {ExifOrientation.RightSideTop, src=>src.RotateFlip(RotateFlipType.Rotate90FlipNone)},
                                      // 270度回転左右反転
                                      {ExifOrientation.RightSideBottom, src=>src.RotateFlip(RotateFlipType.Rotate270FlipX)},
                                      // 270度回転
                                      {ExifOrientation.LeftSideBottom, src=>src.RotateFlip(RotateFlipType.Rotate270FlipNone)}
                                  };

                using (var bmp = new Bitmap(originalPath))
                {
                    if (functor.ContainsKey(orientation))
                    {
                        functor[orientation](bmp);
                    }
                    
                    SaveConcurrentFile(optimizePath, fileName =>
                    {
                        bmp.Save(fileName, format);
                    });
                }
            }
            else
            {
                File.Copy(originalPath, optimizePath);
            }

            return File.Exists(optimizePath);
        }

        public static ExifOrientation GetExifOrientation(string path)
        {
            ExifOrientation orientation;
            using (var ei = new ExifInfo(path))
            {
                orientation = ei.Orientation;

                // メーカーと機種をチェックして補正する。
                var maker = ei.Maker.ToLower();

                // シャープの携帯はおかしい
                if (maker == "sharp")
                {
                    if (orientation == ExifOrientation.LeftSideBottom)
                        orientation = ExifOrientation.TopLeftSide;
                }
            }

            return orientation;
        }

        public static DateTime? GetExifTakenAt(string path)
        {
            var takenAt = default(DateTime);
            using (var ei = new ExifInfo(path))
            {
                var dto = ei.GetPropertyString(ExifTagNames.ExifDTOrig);
                if (string.IsNullOrEmpty(dto))
                    return null;

                dto = string.Join(" ", dto.Split(' ').Select((val, idx) => idx == 0 ? val.Replace(":", "/") : val));
                if (!DateTime.TryParse(dto, out takenAt))
                    return null;
            }

            return takenAt;
        }
    }
}
