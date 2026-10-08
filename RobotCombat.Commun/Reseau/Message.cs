using System.Text.Json;
using System.Text.Json.Serialization;

namespace RobotCombat.Commun.Reseau;

public class Message
{
    private static readonly JsonSerializerOptions optionsJson = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public TypeMessage Type { get; set; }
    public string Donnees { get; set; } = string.Empty;

    public string Serialiser()
    {
        return JsonSerializer.Serialize(this, optionsJson);
    }

    public static Message Deserialiser(string donnees)
    {
        return JsonSerializer.Deserialize<Message>(donnees, optionsJson)
            ?? throw new FormatException("Le message reçu est vide ou invalide.");
    }
}