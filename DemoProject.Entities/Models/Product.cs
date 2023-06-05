using System;
using System.Collections.Generic;

namespace DemoProject.Entities.Models;

public partial class Product
{
    public long ProductId { get; set; }

    public string? ProductName { get; set; }

    public bool? Shared { get; set; }
}
