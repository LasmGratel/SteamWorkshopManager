using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWorkshopManager.Core;

public record IronyModCollection
{
    public string Name { get; set; }
    public string Game { get; set; }
    public List<string> ModNames { get; set; }
    public List<string> Mods { get; set; }

    public WorkshopCollection ToCollection()
    {
        var appId = Game switch
        {
            "Stellaris" => 281990,
            "HeartsofIronIV" => 394360,
            "EuropaUniversalisIV" => 236850,
            "ImperatorRome" => 859580,
            "CrusaderKings3" => 1158310,
            _ => 0
        };
        var items = Mods
            .Where(x => x.StartsWith("mod/ugc_"))
            .Select(x => long.Parse(x[8..^4]))
            .ToHashSet();
        return new WorkshopCollection
        {
            AppId = appId,
            Name = Name,
            ImageUrl = "ms-appx:///Assets/IronyModManager.png",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            Description = "Imported from Irony Mod Manager",
            ShortDescription = "Imported from Irony Mod Manager",
            IsOnline = false,
            Items = items
        };
    }
}