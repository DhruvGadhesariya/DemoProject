using System;
using System.Collections.Generic;

namespace DemoProject.Entities.Models;

public partial class User
{
    public long UserId { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public long? CountryId { get; set; }

    public long? CityId { get; set; }

    public long? Pincode { get; set; }

    public long? Phonenumber { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsAdmin { get; set; }
}
