﻿namespace MagicVilla_CouponAPI.Models.DTO
{
    public class LoginResponse
    {
        public User User { get; set; }
        public string Token { get; set; }
    }
}