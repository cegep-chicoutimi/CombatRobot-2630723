using System.Net.Sockets;
using System.Text;

namespace RobotCombat.Commun.Reseau;

public class Connexion
{
    private TcpClient? client;
    private NetworkStream? flux;

    public string IP { get; set; } = string.Empty;
    public int Port { get; set; }

    public bool Connecter()
    {
        try
        {
            client = new TcpClient();
            client.Connect(IP, Port);
            flux = client.GetStream();
            return true;
        }
        catch (Exception ex) when (ex is SocketException or ArgumentException or ObjectDisposedException)
        {
            return false;
        }
    }

    public void Attacher(TcpClient clientAccepte)
    {
        client = clientAccepte;
        flux = client.GetStream();
    }

    public void Envoyer(Message message)
    {
        if (flux is null)
            throw new InvalidOperationException("Impossible d'envoyer : la connexion n'est pas établie.");

        byte[] contenu = Encoding.UTF8.GetBytes(message.Serialiser());
        byte[] taille = BitConverter.GetBytes(contenu.Length);

        flux.Write(taille, 0, taille.Length);
        flux.Write(contenu, 0, contenu.Length);
    }

    public Message Recevoir()
    {
        if (flux is null)
            throw new InvalidOperationException("Impossible de recevoir : la connexion n'est pas établie.");

        int taille = BitConverter.ToInt32(LireExactement(4), 0);

        if (taille <= 0)
            throw new IOException("La taille du message est invalide.");

        string donnees = Encoding.UTF8.GetString(LireExactement(taille));
        return Message.Deserialiser(donnees);
    }

    public void Fermer()
    {
        flux?.Dispose();
        client?.Close();
    }

    private byte[] LireExactement(int nombreOctets)
    {
        byte[] tampon = new byte[nombreOctets];
        int totalLu = 0;

        while (totalLu < nombreOctets)
        {
            int lu = flux!.Read(tampon, totalLu, nombreOctets - totalLu);

            if (lu == 0)
                throw new IOException("La connexion a été fermée par l'autre partie.");

            totalLu += lu;
        }

        return tampon;
    }
}