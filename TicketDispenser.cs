using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

class TicketDispencer
{
    private Dictionary<string, int> ticketCounters;
    private readonly string filePath = Path.Combine(Path.GetTempPath(), "fnumero.txt");

    private List<Client> clients;

    public TicketDispencer()
    {
        ticketCounters = new Dictionary<string, int> { { "V", 0 }, { "R", 0 }, { "I", 0 } };
        clients = new List<Client>();
        File.WriteAllText(filePath, string.Empty);
        LoadCounters();
    }

    public class Client
    {
        public string Prenom { get; set; }
        public string Nom { get; set; }
        public string TypeTicket { get; set; }

        public override string ToString() => $"{Prenom} {Nom} - Ticket: {TypeTicket}";
    }

    public void Run()
    {
        while (true)
        {
            // Console.Clear();
            Console.WriteLine("Bienvenue au guichet automatique");
            string prenom;
            while (true)
            {
                Console.WriteLine("Veuillez entrer votre prenom:");
                prenom = Console.ReadLine();
                if (IsValidName(prenom))
                {
                    break;
                }
                Console.WriteLine("Prenom invalide. Veuillez entrer un prenom valide.");
            }

            // Vérification du nom
            string nom;
            while (true)
            {
                Console.WriteLine("Veuillez entrer votre nom: ");
                nom = Console.ReadLine();
                if (IsValidName(nom))
                {
                    break;
                }
                Console.WriteLine("Nom invalide. Veuillez entrer un nom valide.");
            }

            Console.WriteLine("Choisir le type d'operation: ");
            Console.WriteLine("1. Versement");
            Console.WriteLine("2. Retrait");
            Console.WriteLine("3. Information");
            Console.WriteLine("4. Quitter");
            Console.Write("Entrer votre choix 1, 2, 3 ou 4: \n");
            //int choix = int.Parse(Console.ReadLine() ?? "0"); //?? operateur de coalescence il renvoie une valeur par defaut (quon specifie) 
            //au cas ou la valeur entree est null ex si le clique sur entrer sans rien ecrire
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out int choix))
            {
                Console.WriteLine("Choix invalide. Réessayer:\n");
                continue;
            }

            string type = choix switch //expression switch et non structure de control switch case 
            {
                1 => "V",
                2 => "R",
                3 => "I",
                4 => "Q",
                _ => "0" //joker aka default 
            };


            if (type == "0")
            {
                Console.WriteLine("Cette option n'existe pas. Reessayer:\n");
                continue;

            }
            if (type == "Q")
            {
                Console.WriteLine("Liste des clients:");
                foreach (var client in clients)
                {
                    Console.WriteLine(client);
                }
                Console.WriteLine("Merci de votre visite");
                break;
            }

            int ticketNumber = ++ticketCounters[type];
            var clientInfo = new Client { Prenom = prenom, Nom = nom, TypeTicket = type };
            clients.Add(clientInfo); // Ajouter à la liste des clients
            SaveCounters();
            if (ticketNumber - 1 == 0)
            {
                Console.WriteLine($"Votre numero est {type}-{ticketNumber}, et il n'y a personne qui attend avant vous");
            }
            else
            {
                Console.WriteLine($"Votre numero est {type}-{ticketNumber}, et il ya {ticketNumber - 1} personnes qui attendent avant vous");
            }
            Console.WriteLine("Voulez vous continuer? (O/N)");
            // if(Console.ReadLine()?.ToUpper()!="O" && Console.ReadLine()?.ToUpper() != "N"){
            //     Console.WriteLine("Choix invalide. Entrer O pour continuer ou N pour quitter");
            //     continue;
            // }
            //Console.ReadLine();
            string reponse = Console.ReadLine()?.ToUpper();
            while (reponse != "O" && reponse != "N")
            {

                Console.WriteLine("Choix invalide. Entrer O pour continuer ou N pour quitter");
                reponse = Console.ReadLine()?.ToUpper();
            }

            // if (Console.ReadLine()?.ToUpper() != "O" && Console.ReadLine()?.ToUpper() != "N")
            // {
            //     Console.WriteLine("Merci de votre visite");
            //     break;
            // }

            continue;


        }
    }
    private bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && Regex.IsMatch(name, @"^[a-zA-Z]+$");
    }

    //.WriteAllText() from System.IO Creates a new file,
    // write the contents to the file, and then closes the file.
    // If the target file already exists, it is truncated and overwritten.
    public void LoadCounters()

    {
        //     if (!File.Exists(filePath))
        //     { // si file nexiste pas ie a la premiere exec ou le file a ete supp 
        //         File.WriteAllText(filePath, "V:0\nR:0\nI:0");// il le cree et y ecrit les premieres ligne
        //         return;
        //     }

        //     foreach (var line in File.ReadAllLines(filePath))
        //     {// le file existe dans ce cas il update le contenu de ticketCounters par le contenu du file
        //         var parts = line.Split(':');
        //         ticketCounters[parts[0]] = int.Parse(parts[1]);
        //     }

        if (!File.Exists(filePath))
        {
            // Si le fichier n'existe pas, le créer avec des compteurs par défaut
            File.WriteAllText(filePath, "V:0\nR:0\nI:0\n\n"); // Ajoute un séparateur pour les clients
            return;
        }

        bool readingCounters = true; // Pour savoir si on lit les compteurs ou les clients

        foreach (var line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                readingCounters = false; // Passer à la lecture des clients
                continue;
            }

            if (readingCounters)
            {
                // Chargement des compteurs
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    ticketCounters[parts[0]] = int.Parse(parts[1]);
                }
            }
            else
            {
                // Chargement des clients
                var parts = line.Split(',');
                if (parts.Length == 3)
                {
                    clients.Add(new Client
                    {
                        Prenom = parts[0],
                        Nom = parts[1],
                        TypeTicket = parts[2]
                    });
                }
            }
        }

    }

    public void SaveCounters()
    {
        //     using var writre = new StreamWriter(filePath);
        //     foreach (var entry in ticketCounters)
        //     {
        //         writre.WriteLine($"{entry.Key}:{entry.Value}");


        //     }
        // }

        using var writer = new StreamWriter(filePath);
        // Sauvegarder les compteurs
        foreach (var entry in ticketCounters)
        {
            writer.WriteLine($"{entry.Key}:{entry.Value}");
        }
        writer.WriteLine(); // Ligne vide pour séparer les compteurs des clients

        // Sauvegarder les clients
        foreach (var client in clients)
        {
            writer.WriteLine($"{client.Prenom},{client.Nom},{client.TypeTicket}");
        }
    }


}