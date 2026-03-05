using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ursus.Xbox.Models
{
    /// <summary>
    /// Request body for catalog API
    /// </summary>
    public class CatalogRequest
    {
        [JsonPropertyName("Products")]
        public List<string> Products { get; set; }
    }

    /// <summary>
    /// Response from catalog API
    /// </summary>
    public class CatalogResponse
    {
        [JsonPropertyName("Products")]
        public Dictionary<string, Product> Products { get; set; }
    }

    /// <summary>
    /// Product information
    /// </summary>
    public class Product
    {
        [JsonPropertyName("StoreId")]
        public string StoreId { get; set; }

        [JsonPropertyName("ProductId")]
        public string ProductId { get; set; }

        [JsonPropertyName("ProductTitle")]
        public string ProductTitle { get; set; }

        [JsonPropertyName("ProductDescription")]
        public string ProductDescription { get; set; }

        [JsonPropertyName("ProductType")]
        public string ProductType { get; set; }

        [JsonPropertyName("DeveloperName")]
        public string DeveloperName { get; set; }

        [JsonPropertyName("PublisherName")]
        public string PublisherName { get; set; }

        [JsonPropertyName("ReleaseDate")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("XCloudTitleId")]
        public string XCloudTitleId { get; set; }

        [JsonPropertyName("Image_Tile")]
        public ProductImage Image_Tile { get; set; }

        [JsonPropertyName("Images")]
        public List<ProductImage> Images { get; set; }

        [JsonPropertyName("Categories")]
        public List<string> Categories { get; set; }

        [JsonPropertyName("Properties")]
        public Dictionary<string, JsonElement> Properties { get; set; }

        [JsonPropertyName("LocalizedProperties")]
        public List<LocalizedProperty> LocalizedProperties { get; set; }

        [JsonPropertyName("MarketProperties")]
        public List<MarketProperty> MarketProperties { get; set; }
    }

    /// <summary>
    /// Product image information
    /// </summary>
    public class ProductImage
    {
        [JsonPropertyName("ImageType")]
        public string ImageType { get; set; }

        [JsonPropertyName("URL")]
        public string Url { get; set; }

        [JsonPropertyName("Width")]
        public int Width { get; set; }

        [JsonPropertyName("Height")]
        public int Height { get; set; }
    }

    /// <summary>
    /// Localized product properties
    /// </summary>
    public class LocalizedProperty
    {
        [JsonPropertyName("Language")]
        public string Language { get; set; }

        [JsonPropertyName("ProductTitle")]
        public string ProductTitle { get; set; }

        [JsonPropertyName("ProductDescription")]
        public string ProductDescription { get; set; }

        [JsonPropertyName("ShortDescription")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("DeveloperName")]
        public string DeveloperName { get; set; }

        [JsonPropertyName("PublisherName")]
        public string PublisherName { get; set; }
    }

    /// <summary>
    /// Market-specific product properties
    /// </summary>
    public class MarketProperty
    {
        [JsonPropertyName("MarketCode")]
        public string MarketCode { get; set; }

        [JsonPropertyName("OriginalReleaseDate")]
        public string OriginalReleaseDate { get; set; }
    }
}