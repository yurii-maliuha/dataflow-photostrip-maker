using SixLabors.ImageSharp;

namespace Dataflow.PhotoStripMaker
{
    public class ImageModel
    {
        public int Id { get; set; }
        public ImageType Type { get; set; }
        public Image Image { get; set; }

        public override string? ToString()
        {
            return $"[Id: {Id}, dimension {Image.Width}x{Image.Height}]";
        }
    }

    public enum ImageType
    {
        Unkown,
        OriginalImage,
        CroppedImage,
        DarkenedImage,
        ImageWithLogo,
        PhotoStrip,
        StampedStrip,
    }
}
