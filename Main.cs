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
    // Suppression des tables si elles existent déjà
    string dropTablesSql = @"
    IF OBJECT_ID('TicketTags', 'U') IS NOT NULL DROP TABLE TicketTags;
    IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
    IF OBJECT_ID('Tags', 'U') IS NOT NULL DROP TABLE Tags;
    IF OBJECT_ID('Tickets', 'U') IS NOT NULL DROP TABLE Tickets;
    ";

    using (SqlCommand dropTablesCommand = new SqlCommand(dropTablesSql, connection))
    {
        dropTablesCommand.ExecuteNonQuery();
    }

    // Création de la table Tickets
    string createTicketsTableSql = @"
    CREATE TABLE Tickets (
        Id BIGINT PRIMARY KEY,
        Subject NVARCHAR(255),
        Status NVARCHAR(50),
        Priority NVARCHAR(50),
        Requester BIGINT,
        Assigned BIGINT,
        Type NVARCHAR(50),
        Url NVARCHAR(255),
        CreatedAt DATETIME,
        Markers NVARCHAR(255)
    );";

    using (SqlCommand createTicketsTableCommand = new SqlCommand(createTicketsTableSql, connection))
    {
        createTicketsTableCommand.ExecuteNonQuery();
    }

    // Création de la table Tags
    string createTagsTableSql = @"
    CREATE TABLE Tags (
        Id INT PRIMARY KEY IDENTITY,
        Name NVARCHAR(50)
    );";

    using (SqlCommand createTagsTableCommand = new SqlCommand(createTagsTableSql, connection))
    {
        createTagsTableCommand.ExecuteNonQuery();
    }

    // Création de la table Users
    string createUsersTableSql = @"
    CREATE TABLE Users (
        Id BIGINT PRIMARY KEY,
        Name NVARCHAR(150),
        Email NVARCHAR(255)
    );";

    using (SqlCommand createUsersTableCommand = new SqlCommand(createUsersTableSql, connection))
    {
        createUsersTableCommand.ExecuteNonQuery();
    }

    // Création de la table TicketTags
    string createTicketTagsTableSql = @"
    CREATE TABLE TicketTags (
        TicketId BIGINT FOREIGN KEY REFERENCES Tickets(Id),
        TagId INT FOREIGN KEY REFERENCES Tags(Id),
        PRIMARY KEY (TicketId, TagId)
    );";

    using (SqlCommand createTicketTagsTableCommand = new SqlCommand(createTicketTagsTableSql, connection))
    {
        createTicketTagsTableCommand.ExecuteNonQuery();
    }

    // Récupération des tickets de l'API Zendesk
    var tickets = api.Tickets.GetAllTickets();

    // Insertion des tickets dans la table Tickets
    foreach (var ticket in tickets.Tickets)
    {
        string insertTicketSql = @"
        INSERT INTO Tickets (Id, Subject, Status, Priority, Requester, Assigned, Url, CreatedAt, Markers)
        VALUES (@Id, @Subject, @Status, @Priority, @Requester, @Assigned, @Url, @CreatedAt, @Markers);";

        using (SqlCommand insertTicketCommand = new SqlCommand(insertTicketSql, connection))
        {
            insertTicketCommand.Parameters.AddWithValue("@Id", ticket.Id);
            insertTicketCommand.Parameters.AddWithValue("@Subject", ticket.Subject);
            insertTicketCommand.Parameters.AddWithValue("@Status", ticket.Status);
            insertTicketCommand.Parameters.AddWithValue("@Priority", ticket.Priority);
            insertTicketCommand.Parameters.AddWithValue("@Requester", ticket.RequesterId);
            insertTicketCommand.Parameters.AddWithValue("@Assigned", ticket.AssigneeId);
            insertTicketCommand.Parameters.AddWithValue("@Url", ticket.Url);
            insertTicketCommand.Parameters.AddWithValue("@CreatedAt", ticket.CreatedAt);
            insertTicketCommand.Parameters.AddWithValue("@Markers", string.Join(",", ticket.Tags)); // Supposons que les tags sont stockés dans une liste de chaînes

            insertTicketCommand.ExecuteNonQuery();
        }
    }

    // Récupération des utilisateurs de l'API Zendesk
    var users = api.Users.GetAllUsers();

    // Insertion des utilisateurs dans la table Users
    foreach (var user in users.Users)
    {
        string insertUserSql = @"
        INSERT INTO Users (Id, Name, Email)
        VALUES (@Id, @Name, @Email);";

        using (SqlCommand insertUserCommand = new SqlCommand(insertUserSql, connection))
        {
            insertUserCommand.Parameters.AddWithValue("@Id", user.Id);
            insertUserCommand.Parameters.AddWithValue("@Name", user.Name);
            insertUserCommand.Parameters.AddWithValue("@Email", user.Email);

            insertUserCommand.ExecuteNonQuery();
        }
    }

    // Insertion des tags dans la table Tags
    foreach (var ticket in tickets.Tickets)
    {
        foreach (var tag in ticket.Tags)
        {
            string insertTagSql = @"
            IF NOT EXISTS (SELECT * FROM Tags WHERE Name = @Name)
            INSERT INTO Tags (Name)
            VALUES (@Name);";

            using (SqlCommand insertTagCommand = new SqlCommand(insertTagSql, connection))
            {
                insertTagCommand.Parameters.AddWithValue("@Name", tag);

                insertTagCommand.ExecuteNonQuery();
            }
        }
    }

    // Insertion des relations ticket-tag dans la table TicketTags
    foreach (var ticket in tickets.Tickets)
    {
        foreach (var tag in ticket.Tags)
        {
            string insertTicketTagSql = @"
            INSERT INTO TicketTags (TicketId, TagId)
            SELECT @TicketId, Id FROM Tags WHERE Name = @TagName;";

            using (SqlCommand insertTicketTagCommand = new SqlCommand(insertTicketTagSql, connection))
            {
                insertTicketTagCommand.Parameters.AddWithValue("@TicketId", ticket.Id);
                insertTicketTagCommand.Parameters.AddWithValue("@TagName", tag);

                insertTicketTagCommand.ExecuteNonQuery();
            }
        }
    }

    connection.Close();
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

