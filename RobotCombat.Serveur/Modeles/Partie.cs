using RobotCombat.Commun.Modeles;
using Action = RobotCombat.Commun.Modeles.Action;
namespace RobotCombat.Serveur.Modeles;

public class Partie
{
    private const int MultiplicateurAttaquePuissante = 2;
    private const int MultiplicateurArmureDefense = 2;
    private static readonly Random random = new ();

    public Joueur Joueur1 { get; private set; } = new();
    public Joueur Joueur2 { get; private set; } = new();
    public Joueur? JoueurActif { get; private set; }
    public bool Terminee { get; private set; }


    public void Demarrer()
    {
        Terminee = false;
        JoueurActif = Joueur1;
    }


    public bool ValiderAction(Action action)
    {
        if (Terminee || JoueurActif is null)
            return false;

        if (!Enum.IsDefined(action))
            return false;

        if (action == Action.ATTAQUE_PUISSANTE && JoueurActif.Robot.Energie < Robot.CoutAttaquePuissante)
            return false;

        return true;
    }

  
    public ResultatAction AppliquerAction(Action action)
    {
        Joueur actif = JoueurActif!;
        Joueur adverse = ObtenirAdversaire(actif);
        int degatsInfliges = 0;
        bool estCritique = false;
        bool estEsquive = false;

        switch (action)
        {
            case Action.ATTAQUE:
                degatsInfliges = CalculerDegats(actif.Robot, adverse.Robot, multiplicateurDegat: 1, out estCritique, out estEsquive);
                adverse.Robot.RecevoirDegats(degatsInfliges);
                break;

            case Action.ATTAQUE_PUISSANTE:
                actif.Robot.DepenserEnergie(Robot.CoutAttaquePuissante);
                degatsInfliges = CalculerDegats(actif.Robot, adverse.Robot, MultiplicateurAttaquePuissante, out estCritique, out estEsquive);
                adverse.Robot.RecevoirDegats(degatsInfliges);
                break;

            case Action.DEFENSE:
                actif.Robot.ActiverDefense();
                break;

            case Action.RECHARGE:
                actif.Robot.RecupererEnergie();
                break;
        }

        if (EstTerminee())
            Terminee = true;
        else
            ChangerTour();

        return new ResultatAction
        {
            Action = action,
            DegatsInfliges = degatsInfliges,
            EnergieRestante = actif.Robot.Energie,
            HPAdversaire = adverse.Robot.HP,
            EstCritique = estCritique,
            EstEsquive = estEsquive
        };
    }


    private int CalculerDegats(Robot attaquant, Robot defenseur, int multiplicateurDegat, out bool estCritique, out bool estEsquive)
    {
        estEsquive = random.Next(100) < Robot.ChanceEsquivePourcent;

        if (estEsquive)
        {
            estCritique = false;
            return 0;
        }

        estCritique = random.Next(100) < Robot.ChanceCritiquePourcent;

        int degatsBase = attaquant.Degat * multiplicateurDegat;

        if (estCritique)
            degatsBase = (int)(degatsBase * Robot.MultiplicateurCritique);

        int armureEffective = defenseur.Armure * (defenseur.EnDefense ? MultiplicateurArmureDefense : 1);
        int degats = degatsBase - armureEffective;

        return Math.Max(0, degats);
    }

    public void ChangerTour()
    {
        JoueurActif = ObtenirAdversaire(JoueurActif!);
    }

    private Joueur ObtenirAdversaire(Joueur joueur)
    {
        return joueur == Joueur1 ? Joueur2 : Joueur1;
    }

    public bool EstTerminee()
    {
        return !Joueur1.Robot.EstVivant() || !Joueur2.Robot.EstVivant();
    }

    public Joueur ObtenirGagnant()
    {
        return Joueur1.Robot.EstVivant() ? Joueur1 : Joueur2;
    }

    public void Recommencer()
    {
        Joueur1.Reinitialiser();
        Joueur2.Reinitialiser();
        Terminee = false;
        JoueurActif = null;
    }
}
