using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Ursus.Xbox;
using Ursus.Xbox.Gamepass;
using Ursus.xCloudNROHelper.Auth;
using static System.Net.Mime.MediaTypeNames;
using Image = SixLabors.ImageSharp.Image;

var msal = await MsalTokenProvider.AcquireOrRefreshAsync(
"1f907974-e22b-4810-a9de-d9647380c97e", // Xbox app client ID (has xCloud transfer scope),
scopes: new[] { "XboxLive.signin", "offline_access" });

var auth = new XboxAuthClient(new HttpClient());

var user = await auth.GetUserTokenAsync(msal.AccessToken);
var xsts = await auth.GetXstsTokenAsync(user.Token);
var gssv = await auth.GetGssvTokenAsync(user.Token);

var productList = new List<string>();
var xCloudTitleIds = new List<string>();

var searchClient = new MicrosoftStoreSearchClient();

Console.WriteLine("Enter title name...");
string searchText = Console.ReadLine();



while (searchText != "")
{
    var searchResults = await searchClient.SearchProductsAsync(searchText, market: "US");

    foreach (var product in searchResults)
    {
        Console.WriteLine($"Found product: {product.Title} (ID: {product.ProductId})");
        productList.Add(product.ProductId);
    }

    Console.WriteLine("Enter title name... (blank (enter) to continue)");
    searchText = Console.ReadLine();


}


var cli = new XboxApiClient(new HttpClient(), xsts.DisplayClaims.Xui.First().Uhs, xsts.Token);

string authToken = $"XBL3.0 x={xsts.DisplayClaims.Xui.First().Uhs};{xsts.Token}";
var catalogCli = new GamePassCatalogClient(appName: "Ursus.XboxStream", appVersion: "1.0.0", httpClient: new HttpClient());

var title = await catalogCli.GetProductsAsync(
                    productList,
                    authToken
                );





var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "ProcessedImages");
//var nspForwardersDir = Path.Combine(Directory.GetCurrentDirectory(), "NSPForwarders");

Directory.CreateDirectory(outputDir);
//Directory.CreateDirectory(nspForwardersDir);

var overlayPath = Path.Combine(Directory.GetCurrentDirectory(), "overlay.png");
using var httpClient = new HttpClient();

Image<Rgba32> overlayImage = null;
if (File.Exists(overlayPath))
{
    overlayImage = await Image.LoadAsync<Rgba32>(overlayPath);
    overlayImage.Mutate(x => x.Resize(256, 256));
}
else
{
    Console.WriteLine($"Warning: Overlay not found at {overlayPath}");
}



foreach (var product in title.Products.Values)
{
    if (product.ProductTitle.ToLower().Contains(" deluxe") || product.ProductTitle.ToLower().Contains(" celebration") || (product.ProductTitle.ToLower().Contains(" edition") && !product.ProductTitle.ToLower().Contains("mass effect")))
    {
        continue;
    }


    if (xCloudTitleIds.Exists(id => id == product.XCloudTitleId))
    {
        

        continue;
    }



    if (product.XCloudTitleId is null)
    {
        //Console.WriteLine($"Skipping {product.ProductTitle} - no xCloudTitleId");
        continue;
    }

    xCloudTitleIds.Add(product.XCloudTitleId);

    Console.WriteLine($"Title: {product.ProductTitle}");
    Console.WriteLine($"xCloud: {product.XCloudTitleId}");

    File.AppendAllLines("titles.txt", new[] { $"Name: {product.ProductTitle}" });
    File.AppendAllLines("titles.txt", new[] { $"xCloud ID: {product.XCloudTitleId}" });

    if (product.Image_Tile?.Url != null)
    {
        var imageUrl = $"https:{product.Image_Tile.Url}";
        //Console.WriteLine($"ImageHero: {imageUrl}");

        try
        {
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

            using var image = Image.Load<Rgba32>(imageBytes);
            image.Mutate(x => x.Resize(256, 256));

            if (overlayImage != null)
            {
                image.Mutate(ctx => ctx.DrawImage(overlayImage, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver, 1f));
            }

            var outputPath = Path.Combine(outputDir, $"{product.XCloudTitleId}.png");
            await image.SaveAsPngAsync(outputPath);


            Console.WriteLine($"Saved processed image to: {outputPath}");


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing image for {product.ProductTitle}: {ex.Message}");
        }
    }

    Console.WriteLine();
}

overlayImage?.Dispose();


Console.WriteLine(Environment.NewLine);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();


