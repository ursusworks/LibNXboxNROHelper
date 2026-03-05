using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ursus.Xbox.Models
{

    public class TitleDetailHeader
    {
        public string xuid { get; set; }
        public SingleTitleDetail[] titles { get; set; }
    }

    public class SingleTitleDetail
    {
        public string titleId { get; set; }
        public string pfn { get; set; }
        public object bingId { get; set; }
        public object windowsPhoneProductId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string[] devices { get; set; }
        public string displayImage { get; set; }
        public string mediaItemType { get; set; }
        public string modernTitleId { get; set; }
        public bool isBundle { get; set; }
        public object achievement { get; set; }
        public object stats { get; set; }
        public object gamePass { get; set; }
        public object images { get; set; }
        public object titleHistory { get; set; }
        public object titleRecord { get; set; }
        public Detail detail { get; set; }
        public object friendsWhoPlayed { get; set; }
        public object[] alternateTitleIds { get; set; }
        public object contentBoards { get; set; }
        public string xboxLiveTier { get; set; }
    }

    public class Detail
    {
        public object[] attributes { get; set; }
        public Availability[] availabilities { get; set; }
        public string[] capabilities { get; set; }
        public string description { get; set; }
        public string developerName { get; set; }
        public object[] genres { get; set; }
        public int minAge { get; set; }
        public string publisherName { get; set; }
        public DateTime releaseDate { get; set; }
        public string shortDescription { get; set; }
        public object vuiDisplayName { get; set; }
        public bool xboxLiveGoldRequired { get; set; }
    }

    public class Availability
    {
        public string[] Actions { get; set; }
        public string AvailabilityId { get; set; }
        public string[] Platforms { get; set; }
        public string SkuId { get; set; }
        public string ProductId { get; set; }
    }

}
