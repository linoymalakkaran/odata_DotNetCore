using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace MGSurvey.Odata.Api.Validators
{
	public class JWTInHeaderMiddleware
	{
		private readonly RequestDelegate _next;

		public JWTInHeaderMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var cookie = context.Request.Cookies["MAQTA-XSRF-TOKEN"];
			if (cookie != null)
			{
				context.Request.Headers.Append("Authorization", "Bearer " + cookie.Trim());
			}

			await _next.Invoke(context);
		}
	}

	/// <summary>
	/// Builder extention for using JWTInHeaderMiddleware
	/// </summary>
	public static class JWTInHeaderMiddlewareExtensions
	{
		public static IApplicationBuilder UseJWTInHeader(
			this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<JWTInHeaderMiddleware>();
		}


	}

	public static class JWTTokenDeserializer
	{
		public static bool IsValidPCSSuperUser(string authorizationParamValue)
		{
			var stream = authorizationParamValue.Split(" ")[1];
			var handler = new JwtSecurityTokenHandler();
			var jsonToken = handler.ReadToken(stream);
			var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
			return Convert.ToBoolean(tokenS.Claims.First(claim => claim.Type == "IsPCSSuperUser").Value);
		}

	}
}
