using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWorkshopManager.Core;

public record WorkshopCollection
{
    public long Id { get; set; }

    public string ImageUrl { get; set; }

    public string Name { get; set; }

    public string App { get; set; }
    
    public DateTime LastUpdatedDate { get; set; }

    public string? Description { get; set; }

    public long Visitors { get; set; }

    public long Subscribers { get; set; }

    public long Favorites { get; set; }
}