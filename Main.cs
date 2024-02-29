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

string zendeskSite, userEmail, userToken, SqlLogin, SqlAdress, SqlPsw, SqlDatabase;

#pragma warning disable CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.

// Lire les informations de configuration des fichiers s'ils existent, sinon demander à l'utilisateur de les fournir
if (File.Exists(zendeskSiteFilePath))
{
    zendeskSite = File.ReadAllText(zendeskSiteFilePath);
}
else
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

if (File.Exists(userEmailFilePath))
{
    userEmail = File.ReadAllText(userEmailFilePath);
}
else
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

if (File.Exists(userTokenFilePath))
{
    userToken = File.ReadAllText(userTokenFilePath);
}
else
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

if (!File.Exists(SqlserverLogin))
{
    Console.Write("Veuillez entrer le login du server sql : ");
    string inputLogin = Console.ReadLine();
    while (string.IsNullOrEmpty(inputLogin))
    {
        Console.WriteLine("L'URL ne peut pas être vide. Veuillez entrer une URL valide : ");
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
    string inputPsw = Console.ReadLine();
    while (string.IsNullOrEmpty(inputPsw))
    {
        Console.WriteLine("L'email ne peut pas être vide. Veuillez entrer une adresse email valide : ");
        inputPsw = Console.ReadLine();
    }
    SqlPsw = inputPsw;
    File.WriteAllText(SqlserverPsw, inputPsw);
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
        Console.WriteLine("L'email ne peut pas être vide. Veuillez entrer une adresse email valide : ");
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
        Console.WriteLine("L'email ne peut pas être vide. Veuillez entrer une adresse email valide : ");
        inputDatabase = Console.ReadLine();
    }
    SqlDatabase = inputDatabase;
    File.WriteAllText(DatabaseNamePath, inputDatabase);
}
else
{
    SqlDatabase = File.ReadAllText(DatabaseNamePath);
}
#pragma warning restore CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.

// Création de l'objet ZendeskApi pour se connecter à l'API Zendesk
var api = new ZendeskApi(zendeskSite, userEmail, userToken, "true");

// Chaîne de connexion à la base de données SQL
string connectionString = $"Server={SqlAdress};Database={SqlDatabase};User Id={SqlLogin};Password={SqlPsw};";;

// Connexion à la base de données SQL
using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();

    // Création de la table Tickets si elle n'existe pas déjà
    string sqlStatement = @"
    IF OBJECT_ID('Tickets', 'U') IS NOT NULL 
    DROP TABLE Tickets;

    CREATE TABLE Tickets (
    Id INT PRIMARY KEY, 
    Priority NVARCHAR(50), 
    Type NVARCHAR(50), 
    Subject NVARCHAR(100), 
    Status NVARCHAR(50), 
    Requester_Id BIGINT, 
    Assigned_Id BIGINT, 
    Created_at NVARCHAR(50), 
    Url NVARCHAR(100)
    
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
        sqlStatement = "INSERT INTO Tickets (Id, Priority, Type, Subject, Status, Requester_Id, Assigned_Id, Created_at, Url) VALUES (@Id, @Priority, @Type, @Subject, @Status, @Requester, @Assigned, @Created_at, @Url)";

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

