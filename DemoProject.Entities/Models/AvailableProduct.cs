using System;
using System.Collections.Generic;

namespace DemoProject.Entities.Models;

public partial class AvailableProduct
{
    public long Id { get; set; }

    public long? ProductId { get; set; }

    public long? CountryId { get; set; }

    public long? CityId { get; set; }

    public bool? Available { get; set; }

    public virtual Country? Country { get; set; }
}
