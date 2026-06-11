using ArcShared;

namespace ArcPortal;

public static class CommandParser
{
    
    public static ArcCommand Parse(string command)
    {
        string[] splitCommand = command.Split(["--"], StringSplitOptions.RemoveEmptyEntries);
        string action = splitCommand[0].Trim();

        var parameters = new Dictionary<string, string>();

        for (int i = 1; i < splitCommand.Length; i++)
        {
            var parts = splitCommand[i].Split(' ', 2);
            if (parts.Length == 2)
                parameters[parts[0].Trim()] = parts[1].Trim();
        }

        return new ArcCommand(action, ArcConstants.ArcPortalIdentifier, Environment.UserName) { Parameters = parameters,  };
    }
}