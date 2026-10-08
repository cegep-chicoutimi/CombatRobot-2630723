using System.Text.Json;
using RobotCombat.Commun.Affichage;
using RobotCombat.Commun.Modeles;
using RobotCombat.Commun.Reseau;
using Action = RobotCombat.Commun.Modeles.Action;

namespace RobotCombat.Client;

public class ProgrammeClient
{
    private Connexion connexion = new();

    private const int PointsARepartir = 10;


    public void Demarrer()
    {
        AffichageConsole.EcrireCouleur("=== Client Combat de Robots ===", ConsoleColor.Cyan);

        bool connecte = false;
        while (!connecte)
        {
            Console.Write("Adresse IP du serveur : ");
            string ip = Console.ReadLine() ?? "";
            int port = LireEntier("Port du serveur : ");

            connecte = SeConnecter(ip, port);

            if (!connecte)
                Console.WriteLine("Connexion échouée. Vérifie l'adresse et le port.");
        }

        Console.WriteLine("Connecté au serveur.");
        bool rejouer = true;

        while (rejouer)
        {
            try
            {
                ConfigurerRobot();
                AttendreDebutPartie();
                JouerPartie();
                rejouer = DemanderRejouer();

                connexion.Envoyer(new Message
                {
                    Type = rejouer ? TypeMessage.REJOUER : TypeMessage.QUITTER
                });
            }
            catch (IOException)
            {
                Console.WriteLine("La connexion avec le serveur a été perdue.");
                rejouer = false;
            }
        }

        connexion.Fermer();
    }

    public bool SeConnecter(string ip, int port)
    {
        connexion.IP = ip;
        connexion.Port = port;
        return connexion.Connecter();
    }

 
    public void ConfigurerRobot()
    {
        Console.WriteLine("Configure ton robot (10 points à répartir entre HP, Armure, Degat) :");
        bool confirme = false;

        do
        {
            int hp = LireEntier("HP : ");
            int armure = LireEntier("Armure : ");
            int degat = LireEntier("Degat : ");

            if (!RepartitionValide(hp, armure, degat))
            {
                Console.WriteLine("Configuration invalide : le total doit être exactement 10 points, tous positifs.");
            }
            else
            {
                connexion.Envoyer(new Message
                {
                    Type = TypeMessage.CONFIGURATION,
                    Donnees = JsonSerializer.Serialize(new ConfigurationRobotDto { Hp = hp, Armure = armure, Degat = degat })
                });

                Console.WriteLine("Configuration envoyée. En attente du serveur...");

                // The server stays the authority: it validates again and answers PRET.
                Message reponse = connexion.Recevoir();
                confirme = reponse.Type == TypeMessage.PRET && reponse.Donnees == "OK";

                if (!confirme)
                    Console.WriteLine("Configuration refusée par le serveur.");
            }
        }
        while (!confirme);
    }

    private static bool RepartitionValide(int hp, int armure, int degat)
    {
        return hp >= 0 && armure >= 0 && degat >= 0
            && hp + armure + degat == PointsARepartir;
    }

    private void AttendreDebutPartie()
    {
        Message message = connexion.Recevoir();

        if (message.Type != TypeMessage.DEBUT_PARTIE)
            throw new InvalidOperationException($"Message inattendu, DEBUT_PARTIE attendu, reçu : {message.Type}");

        Console.WriteLine("La partie commence !");
        Console.WriteLine("En attente de l'action de l'adversaire...");
    }

    private void JouerPartie()
    {
        bool terminee = false;
        bool actionEnvoyee = false;

        while (!terminee)
        {
            Message message = connexion.Recevoir();

            switch (message.Type)
            {
                case TypeMessage.ACTION:
                    // A second ACTION request with no result in between means the server refused our action.
                    if (actionEnvoyee)
                        Console.WriteLine("Action refusée par le serveur (énergie insuffisante ?). Choisis-en une autre.");
                    else
                        AffichageConsole.EcrireCouleur("C'est ton tour !", ConsoleColor.Cyan);

                    Action action = ChoisirAction();
                    connexion.Envoyer(new Message
                    {
                        Type = TypeMessage.ACTION,
                        Donnees = JsonSerializer.Serialize(new ActionDto { Action = action })
                    });
                    actionEnvoyee = true;
                    break;

                case TypeMessage.RESULTAT:
                case TypeMessage.MISE_A_JOUR:
                    // RESULTAT = result of our own action, MISE_A_JOUR = result of the opponent's action.
                    bool monAction = message.Type == TypeMessage.RESULTAT;
                    ResultatAction resultat = JsonSerializer.Deserialize<ResultatAction>(message.Donnees)!;
                    AfficherResultat(resultat, monAction);
                    actionEnvoyee = false;

                    if (monAction && resultat.HPAdversaire > 0)
                        Console.WriteLine("En attente de l'action de l'adversaire...");
                    break;

                case TypeMessage.FIN_PARTIE:
                    ResultatFinDto fin = JsonSerializer.Deserialize<ResultatFinDto>(message.Donnees)!;
                    bool victoire = fin.NomGagnant == ResultatFinDto.Client;
                    AffichageConsole.EcrireCouleur(
                        victoire ? "Partie terminée : tu as gagné !" : "Partie terminée : tu as perdu.",
                        victoire ? ConsoleColor.Green : ConsoleColor.Red);
                    terminee = true;
                    break;
            }
        }
    }

    public Action ChoisirAction()
    {
        //Avant quand on mettait rien on skippait juste le tour.
        Action? action = null;

        while (action is null)
        {
            AffichageConsole.AfficherMenuActions();

            action = Console.ReadLine()?.Trim() switch
            {
                "1" => Action.ATTAQUE,
                "2" => Action.ATTAQUE_PUISSANTE,
                "3" => Action.DEFENSE,
                "4" => Action.RECHARGE,
                _ => null
            };

            if (action is null)
                Console.WriteLine("Choix invalide : entre 1, 2, 3 ou 4.");
        }

        return action.Value;
    }

    public void AfficherResultat(ResultatAction resultat, bool monAction)
    {
        AffichageConsole.AfficherResultat(resultat, monAction);
    }

    public bool DemanderRejouer()
    {
        Console.Write("Veux-tu rejouer ? (o/n) : ");
        string? reponse = Console.ReadLine();
        return reponse?.Trim().ToLower() == "o";
    }

    private static int LireEntier(string invite)
    {
        while (true)
        {
            Console.Write(invite);

            if (int.TryParse(Console.ReadLine(), out int valeur))
                return valeur;

            Console.WriteLine("Veuillez entrer un nombre entier valide.");
        }
    }
}