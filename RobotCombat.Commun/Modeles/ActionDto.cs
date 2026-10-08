namespace RobotCombat.Commun.Modeles;

/// <summary>
/// Payload carried inside Message.Donnees for an ACTION message sent by
/// the client. Not part of the original class diagram.
/// </summary>
public class ActionDto
{
    public Action Action { get; set; }
}
