namespace Sisora.API.Helpers;

public static class InviteCodeHelper
{
    private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate()
    {
        var random = new Random();
        var code = new char[6];

        for (int i = 0; i < 6; i++)
            code[i] = Characters[random.Next(Characters.Length)];

        return $"SN-{new string(code)}";
    }
}