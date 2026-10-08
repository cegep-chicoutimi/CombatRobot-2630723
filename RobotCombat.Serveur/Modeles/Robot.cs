namespace RobotCombat.Serveur.Modeles;


public class Robot
{
    private const int HPBase = 100;
    private const int ArmureBase = 0;
    private const int DegatBase = 10;
    private const int HPParPoint = 10;
    private const int ArmureParPoint = 2;
    private const int DegatParPoint = 2;
    private const int PointsARepartir = 10;


    private const int EnergieMaximum = 100;
    private const int EnergieDepart = 0;
    public const int CoutAttaquePuissante = 30;
    public const int GainRecharge = 25;
    //Partie 2
    public const int ChanceEsquivePourcent = 10;
    public const int ChanceCritiquePourcent = 15;
    public const double MultiplicateurCritique = 1.5;
        
    private bool estConfigure;

    public int Id { get; private set; }
    public int HP { get; private set; }
    public int Armure { get; private set; }
    public int Degat { get; private set; }
    public int Energie { get; private set; }
    public int EnergieMax { get; private set; }
    public int PointsConfiguration { get; private set; }

    public bool EnDefense { get; private set; }

    //Cette^propriete ne sert qua laffichage.
    public int HPMax { get; private set; }

    public bool Configurer(int hp, int armure, int degat)
    {
        if (hp < 0 || armure < 0 || degat < 0)
            return false;

        if (hp + armure + degat != PointsARepartir)
            return false;

        HP = HPBase + hp * HPParPoint;
        //On linitialise ici
        HPMax = HP;
        Armure = ArmureBase + armure * ArmureParPoint;
        Degat = DegatBase + degat * DegatParPoint;
        Energie = EnergieDepart;
        EnergieMax = EnergieMaximum;
        PointsConfiguration = hp + armure + degat;
        estConfigure = true;

        return true;
    }

    public bool ConfigurationValide()
    {
        return estConfigure;
    }

    public bool EstVivant()
    {
        return HP > 0;
    }

    public void RecevoirDegats(int degats)
    {
        HP = Math.Max(0, HP - degats);
        EnDefense = false;
    }

    public void ActiverDefense()
    {
        EnDefense = true;
    }
 
    public bool DepenserEnergie(int montant)
    {
        if (Energie < montant)
            return false;

        Energie -= montant;
        return true;
    }
    public void RecupererEnergie()
    {
        Energie = Math.Min(EnergieMax, Energie + GainRecharge);
    }
}
