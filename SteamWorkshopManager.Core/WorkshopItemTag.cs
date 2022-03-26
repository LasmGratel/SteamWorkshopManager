using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace SteamWorkshopManager.Core;

public class WorkshopItemTag
{
    [BsonId(true)]
    public int Id { get; set; }

    public string Name { get; set; }
}