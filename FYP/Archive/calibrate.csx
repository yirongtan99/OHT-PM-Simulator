#r "nuget: OpenCvSharp4, 4.13.0.20260627"
#r "nuget: OpenCvSharp4.runtime.win, 4.13.0.20260627"

using System;
using OpenCvSharp;
using System.IO;

var imgPath = @"C:\Users\yiron\Desktop\FYProject\PM Sensor Reading\OBS Center\Center Detect\OHT461 OBS C D.JPG";
if (!File.Exists(imgPath)) {
    Console.WriteLine("Image not found: " + imgPath);
    return;
}

using (var src = new Mat(imgPath, ImreadModes.Color)) {
    Console.WriteLine($"Loaded Image: {src.Width}x{src.Height}");

    using (var gray = new Mat()) {
        Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

        using (var thresh = new Mat()) {
            Cv2.Threshold(gray, thresh, 150, 255, ThresholdTypes.Binary);

            using (var points = new Mat()) {
                Cv2.FindNonZero(thresh, points);
                if (points.Rows > 0)
                {
                    var rect = Cv2.BoundingRect(points);
                    Console.WriteLine($"Graph block bounding box: X={rect.X}, Y={rect.Y}, Width={rect.Width}, Height={rect.Height}");
                }
                else
                {
                    Console.WriteLine("Could not find bright graph block.");
                }
            }
        }
    }
}
