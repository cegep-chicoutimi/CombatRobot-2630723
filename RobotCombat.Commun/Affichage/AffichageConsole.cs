using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotCombat.Commun.Modeles;
using Action = RobotCombat.Commun.Modeles.Action;
namespace RobotCombat.Commun.Affichage
{
    public class AffichageConsole
    {
        public static void AfficherBarreVie(string nom, int hp, int hpMax)
        {
            if (hpMax <= 0)
            {
                Console.WriteLine($"{nom,-10} {hp} HP");
                return;
            }

            const int largeur = 20;
            int pleins = (int)Math.Round((double)hp / hpMax * largeur);
            pleins = Math.Clamp(pleins, 0, largeur);

            string barre = new string('█', pleins) + ' ' + new string('-', largeur - pleins);
            Console.WriteLine($"{nom, -10} [{barre}] {hp}/{hpMax}");
        }

        public static void EcrireCouleur(string texte, ConsoleColor couleur)
        {
            ConsoleColor couleurOriginale = Console.ForegroundColor;
            Console.ForegroundColor = couleur;
            Console.WriteLine(texte);
            Console.ForegroundColor = couleurOriginale;
        }

        public static void AfficherMenuActions()
        {
            Console.WriteLine("Choisis ton action :");
            EcrireOption("1", "Attaque", "dégâts normaux");
            EcrireOption("2", "Attaque puissante", "dégâts doublés, coûte de l'énergie");
            EcrireOption("3", "Défense", "protège de la prochaine attaque");
            EcrireOption("4", "Recharge", "récupère de l'énergie");
            Console.Write("> ");
        }

        private static void EcrireOption(string touche, string nom, string description)
        {
            ConsoleColor couleurOriginale = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  [{touche}] ");
            Console.ForegroundColor = couleurOriginale;
            Console.Write($"{nom,-20}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(description);
            Console.ForegroundColor = couleurOriginale;
        }

        public static void AfficherSeparateur()
        {
            Console.WriteLine(new string('-', 40));
        }

        public static void AfficherResultat(ResultatAction resultat, bool monAction)
        {
            bool estAttaque = resultat.Action is Action.ATTAQUE or Action.ATTAQUE_PUISSANTE;

            AfficherSeparateur();
            EcrireCouleur(
                monAction ? $"Tu utilises : {resultat.Action}" : $"L'adversaire utilise : {resultat.Action}",
                monAction ? ConsoleColor.Green : ConsoleColor.Yellow);

            AfficherEvenementsAleatoires(resultat.EstEsquive, resultat.EstCritique);

            if (resultat.DegatsInfliges > 0)
                EcrireCouleur($"Dégâts infligés : {resultat.DegatsInfliges}", ConsoleColor.Red);
            else if (estAttaque && !resultat.EstEsquive)
                EcrireCouleur("Attaque entièrement absorbée par l'armure.", ConsoleColor.DarkGray);

            AfficherBarreVie("Toi", resultat.HPJoueur, resultat.HPMaxJoueur);
            AfficherBarreVie("Adversaire", resultat.HPAdversaire, resultat.HPMaxAdversaire);
            Console.WriteLine($"Ton énergie : {resultat.EnergieRestante}");
        }

        public static void AfficherEvenementsAleatoires(bool estEsquive, bool estCritique)
        {
            if (estEsquive)
                EcrireCouleur("Coup esquivé !", ConsoleColor.Cyan);
            else if (estCritique)
                EcrireCouleur("Coup critique !", ConsoleColor.Yellow);
        }
    }
}
