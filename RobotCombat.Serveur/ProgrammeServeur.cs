using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using RobotCombat.Commun.Affichage;
using RobotCombat.Commun.Modeles;
using RobotCombat.Commun.Reseau;
using RobotCombat.Serveur.Modeles;
using Action = RobotCombat.Commun.Modeles.Action;
namespace RobotCombat.Serveur;


public class ProgrammeServeur
{
    private Connexion connexion = new();
    private Partie partie = new();
    private TcpListener? ecouteur;

    public ProgrammeServeur()
    {
        partie.Joueur1.Nom = ResultatFinDto.Serveur;
        partie.Joueur2.Nom = ResultatFinDto.Client;

    }


    public void Demarrer()
    {
        AffichageConsole.EcrireCouleur("=== Serveur combat de robot ===", ConsoleColor.Cyan);
        int port = LireEntier("Port d'écoute : ");

        ecouteur = new TcpListener(IPAddress.Any, port);
        ecouteur.Start();

        string? ip = ObtenirIpLocale();
        Console.WriteLine(ip is null
            ? "IP introuvable : vérifie ta connexion réseau."
            : $"IP du serveur à donner au client : {ip}");

        while (true)
        {
            AttendreClient();

            try
            {
                JouerUnePartieComplete();
            }
            catch (IOException)
            {
                GererDeconnexion();
            }
            catch (Exception ex) when (ex is FormatException or JsonException or InvalidDataException)
            {
                Console.WriteLine($"Message invalide reçu du client : {ex.Message}");
                GererDeconnexion();
            }
        }
    }


    public void AttendreClient()
    {
        Console.WriteLine("En attente d'un client...");
        TcpClient clientAccepte = ecouteur!.AcceptTcpClient();
        connexion.Attacher(clientAccepte);
        Console.WriteLine("Client connecté.");
    }

    private void JouerUnePartieComplete()
    {
        bool rejouer = true;

        while (rejouer)
        {
            partie.Recommencer();
            ConfigurerRobotServeur();
            AttendreConfigurationClient();

            partie.Demarrer();
            connexion.Envoyer(new Message { Type = TypeMessage.DEBUT_PARTIE });
            Console.WriteLine("Les deux configurations sont valides. Début de la partie !");

            while (!partie.Terminee)
            {
                if (partie.JoueurActif == partie.Joueur1)
                    JouerTourServeur();
                else
                {
                    Console.WriteLine("En attente de l'action du client...");
                    JouerTourClient();
                }
                    
            }

            EnvoyerFinPartie();
            rejouer = AttendreChoixRejouer();
        }

        connexion.Fermer();
    }

    private void ConfigurerRobotServeur()
    {
        Console.WriteLine("Configure ton robot (10 points à répartir entre HP, Armure, Degat) :");
        bool valide = false;

        while (!valide)
        {
            int hp = LireEntier("HP : ");
            int armure = LireEntier("Armure : ");
            int degat = LireEntier("Degat : ");

            valide = partie.Joueur1.ConfigurerRobot(hp, armure, degat);

            if (!valide)
                Console.WriteLine("Configuration invalide : le total doit être exactement 10 points, tous positifs.");
        }

        partie.Joueur1.ConfirmerConfiguration();
    }


    private void AttendreConfigurationClient()
    {
        Console.WriteLine("Configuration confirmée. En attente du client...");
        bool valide = false;

        while (!valide)
        {
            Message messageRecu = connexion.Recevoir();
            valide = GererMessage(messageRecu);
        }
    }


    public bool GererMessage(Message message)
    {
        if (message.Type != TypeMessage.CONFIGURATION)
            throw new InvalidDataException($"CONFIGURATION attendu, reçu : {message.Type}");

        ConfigurationRobotDto? config = JsonSerializer.Deserialize<ConfigurationRobotDto>(message.Donnees);
        bool valide = config is not null && partie.Joueur2.ConfigurerRobot(config.Hp, config.Armure, config.Degat);

        if (valide)
            partie.Joueur2.ConfirmerConfiguration();

        connexion.Envoyer(new Message
        {
            Type = TypeMessage.PRET,
            Donnees = valide ? "OK" : "INVALIDE"
        });

        return valide;
    }

    private void JouerTourServeur()
    {
        AffichageConsole.EcrireCouleur("C'est ton tour !", ConsoleColor.Cyan);
        Action action = DemanderActionConsole();

        while (!partie.ValiderAction(action))
        {
            Console.WriteLine("Action impossible (énergie insuffisante ?). Choisis-en une autre.");
            action = DemanderActionConsole();
        }

        ResultatAction resultat = partie.AppliquerAction(action);
        AfficherResultatServeur(resultat, monAction: true);
        EnvoyerResultat(resultat, TypeMessage.MISE_A_JOUR);
    }

    private void JouerTourClient()
    {
        connexion.Envoyer(new Message { Type = TypeMessage.ACTION });

        Message reponse = connexion.Recevoir();


        if (reponse.Type != TypeMessage.ACTION)
            throw new InvalidDataException($"ACTION attendu, reçu : {reponse.Type}");

        ActionDto dto = JsonSerializer.Deserialize<ActionDto>(reponse.Donnees)
            ?? throw new InvalidDataException("Le message ACTION ne contient pas d'action.");
        Action action = dto.Action;

        if (!partie.ValiderAction(action))
        {

            JouerTourClient();
            return;
        }

        ResultatAction resultat = partie.AppliquerAction(action);
        AfficherResultatServeur(resultat, monAction: false);
        EnvoyerResultat(resultat, TypeMessage.RESULTAT);
    }
    public void EnvoyerEtat()
    {
    }

    private void EnvoyerResultat(ResultatAction resultat, TypeMessage type)
    {
        ResultatAction resultatPourClient = new()
        {
            Action = resultat.Action,
            DegatsInfliges = resultat.DegatsInfliges,
            EnergieRestante = partie.Joueur2.Robot.Energie,
            HPAdversaire = partie.Joueur1.Robot.HP,
            HPMaxAdversaire = partie.Joueur1.Robot.HPMax,
            HPJoueur = partie.Joueur2.Robot.HP,
            HPMaxJoueur = partie.Joueur2.Robot.HPMax,
            EstCritique = resultat.EstCritique,
            EstEsquive = resultat.EstEsquive
        };

        connexion.Envoyer(new Message
        {
            Type = type,
            Donnees = JsonSerializer.Serialize(resultatPourClient)
        });
    }

    private void EnvoyerFinPartie()
    {
        Joueur gagnant = partie.ObtenirGagnant();
        AffichageConsole.EcrireCouleur($"Partie terminée. Gagnant : {gagnant.Nom}", ConsoleColor.Yellow);
        connexion.Envoyer(new Message
        {
            Type = TypeMessage.FIN_PARTIE,
            Donnees = JsonSerializer.Serialize(new ResultatFinDto { NomGagnant = gagnant.Nom })
        });
    }

    private bool AttendreChoixRejouer()
    {
        Message message = connexion.Recevoir();
        return message.Type == TypeMessage.REJOUER;
    }

    private Action DemanderActionConsole()
    {
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

    public void GererDeconnexion()
    {
        Console.WriteLine("Connexion perdue avec le client. Retour en attente...");
        connexion.Fermer();
        connexion = new Connexion();
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
    private void AfficherResultatServeur(ResultatAction resultat, bool monAction)
    {
        // Same data as EnvoyerResultat, but seen from the server player's side.
        ResultatAction resultatPourServeur = new()
        {
            Action = resultat.Action,
            DegatsInfliges = resultat.DegatsInfliges,
            EnergieRestante = partie.Joueur1.Robot.Energie,
            HPJoueur = partie.Joueur1.Robot.HP,
            HPMaxJoueur = partie.Joueur1.Robot.HPMax,
            HPAdversaire = partie.Joueur2.Robot.HP,
            HPMaxAdversaire = partie.Joueur2.Robot.HPMax, 
            EstCritique = resultat.EstCritique,
            EstEsquive = resultat.EstEsquive
        };

        AffichageConsole.AfficherResultat(resultatPourServeur, monAction);
    }

    private static string? ObtenirIpLocale()
    {
        try
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // UDP connect sends nothing, it only makes the OS pick the active network interface.
            socket.Connect("8.8.8.8", 65530);
            return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString();
        }
        catch (SocketException)
        {
            // No network available.
            return null;
        }
    }
}
