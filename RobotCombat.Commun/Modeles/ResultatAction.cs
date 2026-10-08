namespace RobotCombat.Commun.Modeles;

public class ResultatAction
{
    public Action Action { get; set; }
    public int DegatsInfliges { get; set; }
    public int EnergieRestante { get; set; }
    public int HPAdversaire { get; set; }
    //Cette propriete ne sert qua laffichage.
    public int HPMaxAdversaire { get; set; }

    //Ces proprietes sont pour laffichage cote client.
    public int HPJoueur { get; set; }
    public int HPMaxJoueur { get; set; }

    //Fonctionnalité supplementaire
    public bool EstCritique { get; set; }
    public bool EstEsquive { get; set; }
}
