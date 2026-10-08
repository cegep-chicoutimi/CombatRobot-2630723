namespace RobotCombat.Commun.Modeles;

/// <summary>
/// Payload carried inside Message.Donnees for a CONFIGURATION message.
/// Not part of the original class diagram — needed to serialize the point
/// distribution the player chose.
/// </summary>
public class ConfigurationRobotDto
{
    public int Hp { get; set; }
    public int Armure { get; set; }
    public int Degat { get; set; }
}
