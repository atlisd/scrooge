using Microsoft.AspNetCore.Http;
using Scrooge.Api.Middleware;

namespace Scrooge.Api.Tests.Middleware;

public class SessionTokenTests
{
    private static HttpRequest Request(string? cookie = null, string? authorization = null)
    {
        var ctx = new DefaultHttpContext();
        if (cookie is not null)
            ctx.Request.Headers.Cookie = $"{SessionToken.CookieName}={cookie}";
        if (authorization is not null)
            ctx.Request.Headers.Authorization = authorization;
        return ctx.Request;
    }

    [Fact]
    public void Get_NoCredentials_ReturnsNull() =>
        Assert.Null(SessionToken.Get(Request()));

    [Fact]
    public void Get_CookieOnly_ReturnsCookie() =>
        Assert.Equal("abc", SessionToken.Get(Request(cookie: "abc")));

    [Fact]
    public void Get_BearerOnly_ReturnsBearer() =>
        Assert.Equal("xyz", SessionToken.Get(Request(authorization: "Bearer xyz")));

    [Fact]
    public void Get_Both_PrefersCookie() =>
        Assert.Equal("abc", SessionToken.Get(Request(cookie: "abc", authorization: "Bearer xyz")));

    [Theory]
    [InlineData("Basic xyz")]
    [InlineData("Bearer ")]
    [InlineData("xyz")]
    public void FromBearer_MalformedHeader_ReturnsNull(string header) =>
        Assert.Null(SessionToken.FromBearer(Request(authorization: header)));
}
