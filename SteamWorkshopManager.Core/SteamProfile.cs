using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWorkshopManager.Core;

public record SteamProfile
{
    public string Url { get; set; }

    public string Name { get; set; }

    public string Avatar { get; set; }
}