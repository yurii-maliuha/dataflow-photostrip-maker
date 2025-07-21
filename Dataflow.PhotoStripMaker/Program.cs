// See https://aka.ms/new-console-template for more information
using Dataflow.PhotoStripMaker;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading.Tasks.Dataflow;

const int stripPadding = 15;
const int stripImageCount = 4;
const int imageHeight = 300;
const int imageWidth = 200;

using var httpClient = new HttpClient() { BaseAddress = new Uri("https://picsum.photos") };
var random = new Random();

var downloader = new TransformBlock<int, ImageModel>(async (int index) =>
{
    await Task.Delay(1000);
    var (width, height) = (random.Next(400, 900), random.Next(600, 1000));
    byte[] imageData = await httpClient.GetByteArrayAsync($"{width}/{height}");
    var image = new ImageModel()
    {
        Id = index,
        Image = Image.Load(imageData),
        Type = ImageType.OriginalImage
    };
    Console.WriteLine($"Downloaded image: {image}");
    return image;
});

var broadcaster = new BroadcastBlock<ImageModel>(image => image);

var resizer = new TransformBlock<ImageModel, ImageModel>(async (ImageModel imageModel) =>
{
    await Task.Delay(200);
    var croppedSize = new Size(width: imageWidth, height: imageHeight);
    imageModel.Image.Mutate(x => x.Resize(new ResizeOptions()
    {
        Size = croppedSize,
        Mode = ResizeMode.Crop
    }));

    imageModel.Type = ImageType.CroppedImage;

    Console.WriteLine($"Resized image: {imageModel}");
    return imageModel;
});

var darkener = new TransformBlock<ImageModel, ImageModel>(async (ImageModel imageModel) =>
{
    await Task.Delay(300);
    imageModel.Image.Mutate(x => x.Saturate(0.2f));
    imageModel.Type = ImageType.DarkenedImage;

    Console.WriteLine($"Darkened image: {imageModel}");
    return imageModel;
});

var logoStamper = new TransformBlock<ImageModel, ImageModel>(async (ImageModel imageModel) =>
{
    var logo = Image.Load("otter-logo.jpg");
    await Task.Delay(400);
    var position = new Point(imageModel.Image.Width - logo.Width - 5, imageModel.Image.Height - logo.Height - 5);
    imageModel.Image.Mutate(x => x.DrawImage(logo, position, 1f));

    imageModel.Type = ImageType.ImageWithLogo;
    Console.WriteLine($"Added logo: {imageModel}");
    return imageModel;
}, new ExecutionDataflowBlockOptions
{
    MaxDegreeOfParallelism = 4,
    BoundedCapacity = 10
});

var batcher = new BatchBlock<ImageModel>(stripImageCount);

var stripMaker = new TransformBlock<ImageModel[], ImageModel>((ImageModel[] imagesModel) =>
{
    int totalWidth = stripImageCount * imageWidth + stripPadding * (stripImageCount + 1);
    int bottomPadding = 50;
    int totalHeight = imageHeight + stripPadding + bottomPadding;
    var imagesStrip = new Image<Rgba32>(totalWidth, totalHeight, Color.Black);

    int currentX = stripPadding;
    var images = imagesModel.Select(x => x.Image).ToList();
    foreach (var img in images)
    {
        imagesStrip.Mutate(ctx => ctx.DrawImage(img, new Point(currentX, stripPadding), 1f));
        currentX += img.Width + stripPadding;
    }

    var imageModel = new ImageModel()
    {
        Id = 1000 + imagesModel.Sum(x => x.Id),
        Image = imagesStrip,
        Type = ImageType.PhotoStrip
    };

    Console.WriteLine($"Image strip: {imageModel}");
    return imageModel;
});


var textStamper = new TransformBlock<ImageModel, ImageModel>(async (ImageModel imageModel) =>
{
    await Task.Delay(200);
    var font = SystemFonts.CreateFont("Arial", 20);
    var textOptions = new RichTextOptions(font)
    {
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Bottom,
        Origin = new PointF(imageModel.Image.Width - stripPadding, imageModel.Image.Height - stripPadding),
        WrappingLength = imageModel.Image.Width
    };

    imageModel.Image.Mutate(ctx =>
        ctx.DrawText(textOptions, GreetingPhrases.PickRandomPhrase(), Color.White)
    );

    imageModel.Type = ImageType.StampedStrip;

    Console.WriteLine($"Added stamp to image: {imageModel}");
    return imageModel;
});

var saver = new ActionBlock<ImageModel>(async (ImageModel imageModel) =>
{
    string baseDir = AppContext.BaseDirectory;
    var path = Path.Combine(baseDir, "generated-images");
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }


    await imageModel.Image.SaveAsync(Path.Combine(path, $"{imageModel.Type.ToString().ToLower()}-{imageModel.Id}.jpg"));
    Console.WriteLine($"Saved image: {imageModel}");
});

downloader.LinkTo(broadcaster, new DataflowLinkOptions() { PropagateCompletion = true });
broadcaster.LinkTo(resizer, new DataflowLinkOptions() { PropagateCompletion = true });
broadcaster.LinkTo(saver, new DataflowLinkOptions() { PropagateCompletion = true });

resizer.LinkTo(darkener, new DataflowLinkOptions() { PropagateCompletion = true });

darkener.LinkTo(logoStamper, new DataflowLinkOptions() { PropagateCompletion = true });

logoStamper.LinkTo(batcher, new DataflowLinkOptions() { PropagateCompletion = true });

batcher.LinkTo(stripMaker, new DataflowLinkOptions() { PropagateCompletion = true });

stripMaker.LinkTo(textStamper, new DataflowLinkOptions() { PropagateCompletion = true });

textStamper.LinkTo(saver, new DataflowLinkOptions() { PropagateCompletion = true });

for (int i = 0; i < 4; i++)
{
    await downloader.SendAsync(i);
}

downloader.Complete();

try
{
    await saver.Completion;
}
catch (AggregateException ae)
{
    foreach (var ex in ae.Flatten().InnerExceptions)
        Console.WriteLine($"Pipeline failed with: {ex.Message}. Stack trace: {ex.StackTrace}");
}

Console.WriteLine("Job completed");