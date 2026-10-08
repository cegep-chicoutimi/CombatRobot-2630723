namespace RobotCombat.Serveur.Modeles;


public class Joueur
{
    public string Nom { get; set; } = string.Empty;
    public Robot Robot { get; private set; } = new();
    public bool Pret { get; private set; }

 
    public bool ConfigurerRobot(int hp, int armure, int degat)
    {
        return Robot.Configurer(hp, armure, degat);
    }


    public void ConfirmerConfiguration()
    {
        if (Robot.ConfigurationValide())
            Pret = true;
    }

    public bool EstPret()
    {
        return Pret;
    }

    public void Reinitialiser()
    {
        Robot = new Robot();
        Pret = false;
    }
}
