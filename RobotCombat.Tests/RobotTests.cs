using RobotCombat.Serveur.Modeles;
using Xunit;

namespace RobotCombat.Tests;

/// <summary>
/// Covers the "Validation de la configuration du robot" row of the test plan:
/// exactly 10 points -> accepted; too few/too many or negative -> rejected.
/// </summary>
public class RobotTests
{
    [Fact]
    public void Configurer_AvecExactementDixPoints_EstValide()
    {
        // Arrange
        var robot = new Robot();

        // Act
        bool resultat = robot.Configurer(hp: 4, armure: 3, degat: 3);

        // Assert
        Assert.True(resultat);
    }

    [Fact]
    public void Configurer_AvecMoinsDeDixPoints_EstRejetee()
    {
        var robot = new Robot();

        bool resultat = robot.Configurer(hp: 2, armure: 2, degat: 2);

        Assert.False(resultat);
    }

    [Fact]
    public void Configurer_AvecValeurNegative_EstRejetee()
    {
        var robot = new Robot();

        bool resultat = robot.Configurer(hp: 12, armure: -2, degat: 0);

        Assert.False(resultat);
    }
}
