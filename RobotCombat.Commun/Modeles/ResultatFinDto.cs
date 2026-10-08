namespace RobotCombat.Commun.Modeles;

/// <summary>
/// Payload carried inside Message.Donnees for a FIN_PARTIE message.
/// Not part of the original class diagram.
/// </summary>
public class ResultatFinDto
{

    // Agreed values so that any client can tell who won.
    public const string Serveur = "SERVEUR";
    public const string Client = "CLIENT";

    public string NomGagnant { get; set; } = string.Empty;
}
