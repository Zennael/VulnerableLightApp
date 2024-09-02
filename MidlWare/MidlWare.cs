﻿using Microsoft.Extensions.Options;
using VulnerableWebApplication.VLAIdentity;
using VulnerableWebApplication;
using Microsoft.IdentityModel.Tokens;

namespace VulnerableWebApplication.MidlWare
{
    public class XRealIPMiddleware
        {
        /*
        Ajoute le Header "X-Real-IP:<IP>" pour les logs de l'application
        */
        private readonly RequestDelegate _next;

        public XRealIPMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.Headers["X-Real-IP"] = context.Connection.RemoteIpAddress.ToString();
            await _next(context);
        }

    }


    public class ValidateJwtMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidateJwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            /*
                Authentifie les utilisateurs
            */

            string authHeader = context.Request.Headers["Authorization"];

            // URL Without Authentication
            var path = context.Request.Path.Value;
            if (path.Equals("/login", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // User Authentication
            if (authHeader.IsNullOrEmpty() || !VLAIdentity.VLAIdentity.VulnerableValidateToken(authHeader, configuration["Secret"]))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }


            // Admin Authentication
            if (path.StartsWith("/Patch", StringComparison.OrdinalIgnoreCase) && (authHeader.IsNullOrEmpty() || !VLAIdentity.VLAIdentity.VulnerableAdminValidateToken(authHeader, configuration["Secret"])) )
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next(context);
        }
    }

}
