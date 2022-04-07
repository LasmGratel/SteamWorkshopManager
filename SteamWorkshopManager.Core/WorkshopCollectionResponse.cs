using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SteamWorkshopManager.Core;

public class WorkshopCollectionResponse
{
    public record AllResponse
    {

    }

    public record Details
    {
        public record TheTag
        {
            public string Tag { get; set; }

            [JsonProperty(PropertyName = "display_name")]
            public string DisplayName { get; set; }
        }

        public record Child
        {
            [JsonProperty(PropertyName = "publishedfileid")]
            public string PublishedFileId { get; set; }

            [JsonProperty(PropertyName = "sortorder")]
            public int SortOrder { get; set; }

            [JsonProperty(PropertyName = "file_type")]
            public int FileType { get; set; }
        }
    }
}