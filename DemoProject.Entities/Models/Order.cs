using System;
using System.Collections.Generic;

namespace DemoProject.Entities.Models;

public partial class Order
{
    public long OrderId { get; set; }

    public long? ProductId { get; set; }

    public long? UserId { get; set; }

    public DateTime? OrderdAt { get; set; }

    public long? CountryId { get; set; }

    public long? CityId { get; set; }

    public DateTime? UtcTime { get; set; }

    public virtual City? City { get; set; }
}
