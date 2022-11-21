global using System.Net;
global using Microsoft.AspNetCore.OpenApi;
global using Microsoft.AspNetCore.Mvc;

global using MagicVilla_CouponAPI;
global using MagicVilla_CouponAPI.Models;
global using MagicVilla_CouponAPI.Models.DTO;
global using MagicVilla_CouponAPI.Data;
global using MagicVilla_CouponAPI.Mapping;
global using MagicVilla_CouponAPI.Endpoints;
global using MagicVilla_CouponAPI.Repository;
global using MagicVilla_CouponAPI.Repository.IRepository;

global using AutoMapper;
global using FluentValidation;
global using Microsoft.OpenApi.Models;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using System.Text;