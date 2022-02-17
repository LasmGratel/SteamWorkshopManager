using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWorkshopManager.Core;

/// <summary>
/// A summary of a workshop item
/// </summary>
public partial record WorkshopItem
{
    public long Id { get; set; }

    public string ImageUrl { get; set; }

    public string Name { get; set; }

    public string App { get; set; }

    public DateTime SubscribedDate { get; set; }

    public DateTime LastUpdatedDate { get; set; }
}