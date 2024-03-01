/*
    Ce fichier contient le code principal pour extraire les tickets Zendesk et les stocker dans une base de données SQL.
    Il utilise l'API ZendeskApi_v2 pour récupérer les tickets et la bibliothèque System.Data.SqlClient pour se connecter à la base de données SQL.

    Le code commence par récupérer les informations de configuration à partir de fichiers texte ou en les demandant à l'utilisateur.
    Ensuite, il crée une connexion à la base de données SQL et crée les tables nécessaires.
    Ensuite, il récupère les tickets Zendesk à l'aide de l'API ZendeskApi_v2 et les insère dans la base de données SQL.
    Enfin, il offre à l'utilisateur la possibilité d'afficher les tickets actuels.

    Assurez-vous de configurer correctement les informations de connexion à la base de données SQL et les fichiers de configuration Zendesk avant d'exécuter ce code.

    Pour plus d'informations sur l'API ZendeskApi_v2, consultez la documentation officielle : https://github.com/CloudCoreo/ZendeskApi_v2

    Pour plus d'informations sur la bibliothèque System.Data.SqlClient, consultez la documentation officielle : https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient?view=net-5.0

    Auteur : Thybault Jallu
    Date : 27/02/2024
*/

using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using ZendeskApi_v2;

// Définition des chemins des fichiers de configuration
string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
string configFolderPath = Path.Combine(baseDirectory, "config");
string zendeskSiteFilePath = Path.Combine(configFolderPath, "zendeskSite.txt");
string userEmailFilePath = Path.Combine(configFolderPath, "userEmail.txt");
string userTokenFilePath = Path.Combine(configFolderPath, "userToken.txt");

string SqlserverLogin = Path.Combine(configFolderPath, "serverLogin.txt");
string SqlserverPsw = Path.Combine(configFolderPath, "serverPassword.txt");
string SqlserverAdress = Path.Combine(configFolderPath, "serverAdress.txt");
string DatabaseNamePath = Path.Combine(configFolderPath, "BaseName.txt");

// Créer le dossier de configuration s'il n'existe pas déjà
Directory.CreateDirectory(configFolderPath);
#pragma warning disable CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.

string zendeskSite = string.Empty;
string userEmail = string.Empty;
string userToken = string.Empty;
string SqlLogin = string.Empty;
string SqlAdress = string.Empty;
string SqlPsw = string.Empty;
string SqlDatabase = string.Empty;
string? reponse;
do
{
    Console.WriteLine("Voulez-vous vous connecter au dernier serveur et au dernier zendesk ? (o/n)");
    reponse = Console.ReadLine();

} while (reponse != "o" && reponse != "n");

if (reponse == "o")
{
    
    // Lire les informations de configuration des fichiers s'ils existent, sinon demander à l'utilisateur de les fournir
    if (!File.Exists(zendeskSiteFilePath))
    {
        Console.Write("Veuillez entrer l'URL de votre site Zendesk : ");

        string inputUrl = Console.ReadLine();

        while (string.IsNullOrEmpty(inputUrl))
        {
            Console.WriteLine("L'URL ne peut pas être vide. Veuillez entrer une URL valide : ");
            inputUrl = Console.ReadLine();
        }
        zendeskSite = inputUrl;
        File.WriteAllText(zendeskSiteFilePath, zendeskSite);
    }
    else
    {
        zendeskSite = File.ReadAllText(zendeskSiteFilePath);
    }

    if (!File.Exists(userEmailFilePath))
    {
        Console.Write("Veuillez entrer votre email : ");
        string inputEmail = Console.ReadLine();
        while (string.IsNullOrEmpty(inputEmail))
        {
            Console.WriteLine("L'email ne peut pas être vide. Veuillez entrer une adresse email valide : ");
            inputEmail = Console.ReadLine();
        }
        userEmail = inputEmail;
        File.WriteAllText(userEmailFilePath, userEmail);
    }
    else
    {
        userEmail = File.ReadAllText(userEmailFilePath);
    }

    if (!File.Exists(userTokenFilePath))
    {
        Console.Write("Veuillez entrer votre token : ");
        string inputToken = Console.ReadLine();
        while (string.IsNullOrEmpty(inputToken))
        {
            Console.WriteLine("Le token ne peut pas être vide. Veuillez entrer un token valide : ");
            inputToken = Console.ReadLine();
        }
        userToken = inputToken;
        File.WriteAllText(userTokenFilePath, userToken);
    }
    else
    {
        userToken = File.ReadAllText(userTokenFilePath);
    }
    
    if (!File.Exists(SqlserverLogin))
    {
        Console.Write("Veuillez entrer le login du server sql : ");
        string inputLogin = Console.ReadLine();
        while (string.IsNullOrEmpty(inputLogin))
        {
            Console.WriteLine("Le login ne peut pas être vide. Veuillez entrer un login valide : ");
            inputLogin = Console.ReadLine();
        }
        SqlLogin = inputLogin;
        File.WriteAllText(SqlserverLogin,inputLogin);
    }
    else
    {
        SqlLogin = File.ReadAllText(SqlserverLogin);
    }
    
    if (!File.Exists(SqlserverPsw))
    {
        Console.Write("Veuillez entrer le mot de passe du server sql : ");
        SqlPsw = Console.ReadLine();
        
    }
    else
    {
        SqlPsw = File.ReadAllText(SqlserverPsw);
    }
    
    if (!File.Exists(SqlserverAdress))
    {
        Console.Write("Veuillez entrer l'adresse du server sql : ");
        string inputAdress = Console.ReadLine();
        while (string.IsNullOrEmpty(inputAdress))
        {
            Console.WriteLine("L'adresse ne peut pas être vide. Veuillez entrer une adresse valide : ");
            inputAdress = Console.ReadLine();
        }
        SqlAdress = inputAdress;
        File.WriteAllText(SqlserverAdress, inputAdress);
    }
    else
    {
        SqlAdress = File.ReadAllText(SqlserverAdress);
    }
    
    if (!File.Exists(DatabaseNamePath))
    {
        Console.Write("Veuillez entrer le nom de la base de donnée dans le server sql : ");
        string inputDatabase = Console.ReadLine();
        while (string.IsNullOrEmpty(inputDatabase))
        {
            Console.WriteLine("Le nom de la base de donnée ne peut pas être vide. Veuillez entrer un nom de base de donnée valide : ");
            inputDatabase = Console.ReadLine();
        }
        SqlDatabase = inputDatabase;
        File.WriteAllText(DatabaseNamePath, inputDatabase);
    }
    else
    {
        SqlDatabase = File.ReadAllText(DatabaseNamePath);
    }
    
}
else if (reponse == "n")
{
    
    // Lire les informations de configuration des fichiers s'ils existent, sinon demander à l'utilisateur de les fournir
    Console.Write("Veuillez entrer l'URL de votre site Zendesk : ");
    string inputUrl = Console.ReadLine();
    while (string.IsNullOrEmpty(inputUrl))
    {
        Console.WriteLine("L'URL ne peut pas être vide. Veuillez entrer une URL valide : ");
        inputUrl = Console.ReadLine();
    }
    zendeskSite = inputUrl;
    File.WriteAllText(zendeskSiteFilePath, zendeskSite);


    Console.Write("Veuillez entrer votre email : ");
    string inputEmail = Console.ReadLine();
    while (string.IsNullOrEmpty(inputEmail))
    {
        Console.WriteLine("L'email ne peut pas être vide. Veuillez entrer une adresse email valide : ");
        inputEmail = Console.ReadLine();
    }
    userEmail = inputEmail;
    File.WriteAllText(userEmailFilePath, userEmail);


    Console.Write("Veuillez entrer votre token : ");
    string inputToken = Console.ReadLine();
    while (string.IsNullOrEmpty(inputToken))
    {
        Console.WriteLine("Le token ne peut pas être vide. Veuillez entrer un token valide : ");
        inputToken = Console.ReadLine();
    }
    userToken = inputToken;
    File.WriteAllText(userTokenFilePath, userToken);


    Console.Write("Veuillez entrer le login du server sql : ");
    string inputLogin = Console.ReadLine();
    while (string.IsNullOrEmpty(inputLogin))
    {
        Console.WriteLine("Le login ne peut pas être vide. Veuillez entrer un login valide : ");
        inputLogin = Console.ReadLine();
    }
    SqlLogin = inputLogin;
    File.WriteAllText(SqlserverLogin,inputLogin);

    
    Console.Write("Veuillez entrer le mot de passe du server sql : ");
    SqlPsw = Console.ReadLine();

    
    Console.Write("Veuillez entrer l'adresse du server sql : ");
    string inputAdress = Console.ReadLine();
    while (string.IsNullOrEmpty(inputAdress))
    {
        Console.WriteLine("L'adresse ne peut pas être vide. Veuillez entrer une adresse valide : ");
        inputAdress = Console.ReadLine();
    }
    SqlAdress = inputAdress;
    File.WriteAllText(SqlserverAdress, inputAdress);

    
    Console.Write("Veuillez entrer le nom de la base de donnée dans le server sql : ");
    string inputDatabase = Console.ReadLine();
    while (string.IsNullOrEmpty(inputDatabase))
    {
        Console.WriteLine("Le nom de la base de donnée ne peut pas être vide. Veuillez entrer un nom de base de donnée valide : ");
        inputDatabase = Console.ReadLine();
    }
    SqlDatabase = inputDatabase;
    File.WriteAllText(DatabaseNamePath, inputDatabase);

}

#pragma warning restore CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.

// Création de l'objet ZendeskApi pour se connecter à l'API Zendesk
var api = new ZendeskApi(zendeskSite, userEmail, userToken, "true");

// Chaîne de connexion à la base de données SQL
string connectionString = $"Server={SqlAdress};Database={SqlDatabase};User Id={SqlLogin};Password={SqlPsw};";

// Connexion à la base de données SQL
using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();

    // Création de la table Tickets si elle n'existe pas déjà
    string sqlStatement = @"
    IF OBJECT_ID('Tickets', 'U') IS NOT NULL 
    DROP TABLE Tickets;

    CREATE TABLE Tickets (
    Id BIGINT PRIMARY KEY, 
    Priority NVARCHAR(50), 
    Type NVARCHAR(50), 
    Subject NVARCHAR(100), 
    Status NVARCHAR(50), 
    Requester_Id BIGINT, 
    Assigned_Id BIGINT, 
    Created_at NVARCHAR(50), 
    Url NVARCHAR(100),
    Markers NVARCHAR(100)
    
    );";
    using (SqlCommand command = new SqlCommand(sqlStatement, connection))
    {
        command.ExecuteNonQuery();
    }

    // Création de la table Users si elle n'existe pas déjà
    string createUserTableSql = @"
    IF OBJECT_ID('Users', 'U') IS NOT NULL 
    DROP TABLE Users;

    CREATE TABLE Users (
        Id BIGINT PRIMARY KEY,
        Name NVARCHAR(100),
        Email NVARCHAR(100),
        Role NVARCHAR(50)
    );";
    
    // Création de la table Markers si elle n'existe pas déjà
    string createMarkerTableSql = @"
    IF OBJECT_ID('Markers', 'U') IS NOT NULL 
    DROP TABLE Markers;

    CREATE TABLE Markers (
        Id BIGINT PRIMARY KEY,
        Name NVARCHAR(100),
    );";

    using (SqlCommand createMarkerTableCommand = new SqlCommand(createMarkerTableSql, connection))
    {
        createMarkerTableCommand.ExecuteNonQuery();
    }
    
    
    // Récupération des tickets Zendesk et insertion des tags uniques dans la table Markers
    var tag1 = api.Tickets.GetAllTickets();
    foreach (var ticket in tag1.Tickets)
    {
        foreach (var tag in ticket.Tags)
        {
            string countSql = "SELECT COUNT(*) FROM Markers";
            SqlCommand countCommand = new SqlCommand(countSql, connection);
            int i = (int)countCommand.ExecuteScalar() + 1;

            string insertMarkerSql = @"
            IF NOT EXISTS (SELECT 1 FROM Markers WHERE Name = @Name)
            BEGIN
                INSERT INTO Markers (Id, Name) VALUES (@Id, @Name);
            END";

        using (SqlCommand insertMarkerCommand = new SqlCommand(insertMarkerSql, connection))
        {
            insertMarkerCommand.Parameters.AddWithValue("@Id", i);
            insertMarkerCommand.Parameters.AddWithValue("@Name", tag);
            insertMarkerCommand.ExecuteNonQuery();
        }
            
        }
    }
    
    using (SqlCommand createUserTableCommand = new SqlCommand(createUserTableSql, connection))
    {
        createUserTableCommand.ExecuteNonQuery();
    }

    // Récupération des utilisateurs Zendesk et insertion dans la table Users
    var groupUserResponse = api.Users.GetAllUsers();
    foreach (var user in groupUserResponse.Users)
    {
        string insertUserSql = "INSERT INTO Users (Id, Name, Email, Role) VALUES (@Id, @Name, @Email, @Role)";

        using (SqlCommand insertUserCommand = new SqlCommand(insertUserSql, connection))
        {
            insertUserCommand.Parameters.AddWithValue("@Id", user.Id);
            insertUserCommand.Parameters.AddWithValue("@Name", user.Name ?? "n/a");
            insertUserCommand.Parameters.AddWithValue("@Email", user.Email);
            insertUserCommand.Parameters.AddWithValue("@Role", user.Role ?? "n/a");

            insertUserCommand.ExecuteNonQuery();
        }
    }

    // Récupération des tickets Zendesk et insertion dans la table Tickets
    var tickets = api.Tickets.GetAllTickets();
    foreach (var item in tickets.Tickets)
    {
        sqlStatement = "INSERT INTO Tickets (Id, Priority, Type, Subject, Status, Requester_Id, Assigned_Id, Created_at, Url, Markers) VALUES (@Id, @Priority, @Type, @Subject, @Status, @Requester, @Assigned, @Created_at, @Url, @Markers)";

        using (SqlCommand command = new SqlCommand(sqlStatement, connection))
        {
            command.Parameters.AddWithValue("@Id", item.Id);
            command.Parameters.AddWithValue("@Subject", item.Subject);
            command.Parameters.AddWithValue("@Status", item.Status);
            command.Parameters.AddWithValue("@Created_at", item.CreatedAt);
            command.Parameters.AddWithValue("@Priority", item.Priority);
            command.Parameters.AddWithValue("@Requester", item.RequesterId);
            command.Parameters.AddWithValue("@Assigned", item.AssigneeId);
            command.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(item.Type) ? (object)DBNull.Value : item.Type);
            command.Parameters.AddWithValue("@Url", item.Url);

            // Convertir la liste de tags en une chaîne séparée par des virgules
            string markers = string.Join(" , ", item.Tags);
            command.Parameters.AddWithValue("@Markers", markers);

            command.ExecuteNonQuery();
        }
    } 
    
}
// Affichage des tickets actuels si l'utilisateur le souhaite
Console.WriteLine("[1] Voulez-vous afficher les tickets actuels\n[Autre touche] Quitter");

if (Console.ReadLine() == "1")
{   
    var tickets = api.Tickets.GetAllTickets();

    foreach (var item in tickets.Tickets)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("ID du ticket: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(item.Id);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(", message du ticket: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(item.Subject);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(", status du ticket: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(item.Status + " ");

        Console.Write(item.Priority + " ");
        Console.Write(item.RequesterId + " ");
        Console.Write(item.AssigneeId + " ");
        Console.Write(item.Type + " ");
        Console.Write(item.Url + " ");
        Console.Write(item.CreatedAt + " ");
        Console.WriteLine("");
        Console.WriteLine("");
    }

    Console.WriteLine("Appuyez sur une touche pour quitter");
    Console.ReadKey();
}
else
{
    Environment.Exit(0);
}

