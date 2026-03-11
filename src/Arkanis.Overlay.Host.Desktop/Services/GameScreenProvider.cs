namespace Arkanis.Overlay.Host.Desktop.Services;

using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;
using Workers;
using Point = Point;
using Size = Size;

public class GameScreenProvider(GameWindowTracker windowTracker, ILogger<GameScreenProvider> logger)
{
    public Bitmap GetGameScreen()
    {
        var windowPosition = windowTracker.CurrentWindowPosition;
        var windowSize = windowTracker.CurrentWindowSize;

        var bitmap = new Bitmap(windowSize.Width, windowSize.Height);
        using var graphics = Graphics.FromImage(bitmap);

        logger.LogDebug("Capturing screenshot from position {Position} with size {Size}", windowPosition, windowSize);
        graphics.CopyFromScreen(windowPosition, Point.Empty, windowSize, CopyPixelOperation.SourceCopy);
        return bitmap;
    }
}

public class GameScreenProcessor(GameScreenProvider screenProvider, GameWindowTracker windowTracker, ILogger<GameScreenProcessor> logger) : BackgroundService
{
    public static readonly Size CropSizeInfo1 = new(930, 468);
    public static readonly Size CropSizeInfo2 = new(930, 545);
    public static readonly Size CropSizeInfo3 = new(930, 562);

    private readonly PaddleOcrAll _ocr = new(LocalFullModels.EnglishV4, PaddleDevice.Mkldnn())
    {
        AllowRotateDetection = false,
        Enable180Classification = false,
    };

    private readonly DirectoryInfo _outputDirectory = new(@"D:\Git\github\ArkanisCorporation\ArkanisOverlay\GameScreens");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        windowTracker.WindowFocusChanged += WindowTrackerOnWindowFocusChanged;
        _outputDirectory.Create();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            if (!windowTracker.IsWindowFocused)
            {
                logger.LogDebug("Game window is not focused, skipping screenshot");
                continue;
            }

            try
            {
                CaptureAndProcessScreenshot();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while collecting game screens");
            }
        }
    }

    private void WindowTrackerOnWindowFocusChanged(object? sender, bool isFocused)
    {
        if (isFocused)
        {
            CaptureAndProcessScreenshot();
        }
    }

    internal void CaptureAndProcessScreenshot()
    {
        using var image = screenProvider.GetGameScreen();
        var cropPoint = new Point(image.Width - CropSizeInfo1.Width, 0);
        var cropRectangle = new Rectangle(cropPoint, CropSizeInfo1);

        using var imageCrop = image.Clone(cropRectangle, image.PixelFormat);
        var imageFileName = $"{DateTimeOffset.Now:s}.png".Replace(":", "-");
        var imageFilePath = $"{_outputDirectory.FullName}{Path.DirectorySeparatorChar}{imageFileName}";

        logger.LogDebug("Saving screenshot to {FilePath}", imageFilePath);
        image.Save(imageFilePath);

        ProcessImage(imageCrop, imageFilePath);
    }

    internal void ProcessImage(Bitmap image, string imageFilePath)
    {
        logger.LogDebug("Processing screenshot {Image}", image);
        using var imageMat = image.ToMat();
        using var outputImageMat = new Mat();

        if (imageMat.Channels() == 4)
        {
            Cv2.CvtColor(imageMat, outputImageMat, ColorConversionCodes.BGRA2RGB);
        }
        else if (imageMat.Channels() == 3)
        {
            Cv2.CvtColor(imageMat, outputImageMat, ColorConversionCodes.BGR2RGB);
        }

        using var hsv = new Mat();
        Cv2.CvtColor(imageMat, hsv, ColorConversionCodes.BGR2HSV);

        using var whiteMask = new Mat();
        Cv2.InRange(
            hsv,
            new Scalar(0, 0, 190),
            new Scalar(179, 45, 255),
            whiteMask
        );
        DebugImageHelpers.SaveStrip(imageFilePath, "_003_white-mask", imageMat, hsv, whiteMask);

// Neutral-color gate in BGR
        var ch = imageMat.Split();
        using var maxBg = new Mat();
        using var maxBgr = new Mat();
        using var minBg = new Mat();
        using var minBgr = new Mat();
        using var spread = new Mat();

        Cv2.Max(ch[0], ch[1], maxBg);
        Cv2.Max(maxBg, ch[2], maxBgr);
        Cv2.Min(ch[0], ch[1], minBg);
        Cv2.Min(minBg, ch[2], minBgr);
        Cv2.Subtract(maxBgr, minBgr, spread);

        using var neutralMask = new Mat();
        Cv2.Threshold(spread, neutralMask, 18, 255, ThresholdTypes.BinaryInv);

// Combined color mask
        using var colorMask = new Mat();
        Cv2.BitwiseAnd(whiteMask, neutralMask, colorMask);
        DebugImageHelpers.SaveStrip(imageFilePath, "_005_color-mask", imageMat, whiteMask, neutralMask, colorMask);

// Structure mask from V channel
        var hsvSplit = hsv.Split();
        using var v = hsvSplit[2];

        using var kernelTophat = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(9, 9));
        using var tophat = new Mat();
        Cv2.MorphologyEx(v, tophat, MorphTypes.TopHat, kernelTophat);
        DebugImageHelpers.SaveStrip(imageFilePath, "_006_tophat", imageMat, v, tophat);

        using var textMask = new Mat();
        Cv2.AdaptiveThreshold(
            tophat,
            textMask,
            255,
            AdaptiveThresholdTypes.GaussianC,
            ThresholdTypes.Binary,
            31,
            -3
        );
        DebugImageHelpers.SaveStrip(imageFilePath, "_007_text-mask", imageMat, tophat, textMask);

// Final mask
        using var finalMask = new Mat();
        Cv2.BitwiseAnd(colorMask, textMask, finalMask);
        DebugImageHelpers.SaveStrip(imageFilePath, "_008_final-mask", imageMat, colorMask, textMask, finalMask);

// Small reconnect
        using var kernelClose = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, 2));
        Cv2.MorphologyEx(finalMask, finalMask, MorphTypes.Close, kernelClose);
        DebugImageHelpers.SaveStrip(imageFilePath, "_009_closed", imageMat, finalMask);

        _ocr.Detector.UnclipRatio = 2.0f;
        var result = _ocr.Run(outputImageMat);
        Console.WriteLine("Detected all texts: \n" + result.Text);
        File.WriteAllText(imageFilePath.Replace(".png", "_009_ocr-result.txt"), result.Text);

        foreach (var region in result.Regions)
        {
            Console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}");
        }
    }
}

public static class DebugImageHelpers
{
    public static void SaveStrip(string imageFilePath, string suffix, params Mat[] mats)
    {
        var dumpPath = imageFilePath.Replace(".png", $"{suffix}.png");

        using var strip = CombineHorizontal(mats);
        Cv2.ImWrite(dumpPath, strip);
    }

    public static Mat CombineHorizontal(params Mat[] images)
    {
        if (images is null || images.Length == 0)
        {
            throw new ArgumentException("No images provided.", nameof(images));
        }

        if (images.Any(x => x.Empty()))
        {
            throw new ArgumentException("One or more images are empty.", nameof(images));
        }

        // Pick a common output height.
        var targetHeight = images.Max(x => x.Rows);

        // Normalize everything to 8-bit 3-channel BGR with the same height.
        var prepared = new List<Mat>(images.Length);

        try
        {
            prepared.AddRange(images.Select(src => PrepareForConcat(src, targetHeight)));

            var result = new Mat();
            Cv2.HConcat(prepared, result);
            return result;
        }
        catch
        {
            foreach (var mat in prepared)
            {
                mat.Dispose();
            }

            throw;
        }
    }

    private static Mat PrepareForConcat(Mat src, int targetHeight)
    {
        Mat current;

        // Normalize depth for visualization.
        if (src.Depth() != MatType.CV_8U)
        {
            current = new Mat();
            Cv2.Normalize(src, current, 0, 255, NormTypes.MinMax);
            current.ConvertTo(current, MatType.CV_8U);
        }
        else
        {
            current = src.Clone();
        }

        // Normalize channels to 3-channel BGR.
        if (current.Channels() == 1)
        {
            var bgr = new Mat();
            Cv2.CvtColor(current, bgr, ColorConversionCodes.GRAY2BGR);
            current.Dispose();
            current = bgr;
        }
        else if (current.Channels() == 3)
        {
            // Keep as-is. BGR and HSV are both 3-channel 8-bit for concat purposes.
        }
        else if (current.Channels() == 4)
        {
            var bgr = new Mat();
            Cv2.CvtColor(current, bgr, ColorConversionCodes.BGRA2BGR);
            current.Dispose();
            current = bgr;
        }
        else
        {
            current.Dispose();
            throw new NotSupportedException($"Unsupported channel count for visualization: {src.Channels()}");
        }

        // Normalize height.
        if (current.Rows != targetHeight)
        {
            var resized = new Mat();
            var scale = (double)targetHeight / current.Rows;
            var targetWidth = Math.Max(1, (int)Math.Round(current.Cols * scale));

            Cv2.Resize(
                current,
                resized,
                new OpenCvSharp.Size(targetWidth, targetHeight),
                0,
                0,
                InterpolationFlags.Nearest
            );

            current.Dispose();
            current = resized;
        }

        return current;
    }
}
