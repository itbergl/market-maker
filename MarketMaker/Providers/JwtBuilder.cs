using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MarketMaker.Services;

public class JwtBuilder
{
    private List<Claim> claims = new List<Claim>();

    public static JwtBuilder CreateNewBuilder()
    {
        return new JwtBuilder();
    }

    private JwtBuilder()
    {
        
    }
    
    public JwtBuilder SetAdmin(bool isAdmin)
    {
        claims.Add(new Claim("isAdmin", isAdmin ? "true" : "false"));
        return this;
    }

    public JwtBuilder SetUser(string name)
    {
        claims.Add(new Claim("name", name));
        return this;
    }

    public JwtBuilder SetMarket(string market)
    {
        claims.Add(new Claim("marketCode", market));
        return this;
    }

    public string Build()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dflksjdf;alskj;223423j23j0001232321212312321321312321"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); 
        
        var token = new JwtSecurityToken(
            issuer: "market-maker",
            audience: "web-client",
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}