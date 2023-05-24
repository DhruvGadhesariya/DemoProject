using System;
using System.Collections.Generic;

namespace DemoProject.Entities.Models;

public partial class UserOtp
{
    public long OtpId { get; set; }

    public string Email { get; set; } = null!;

    public long Otp { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiredAt { get; set; }
}
