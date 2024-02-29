using System.Data.SqlClient;
using ZendeskApi_v2;

string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
string configFolderPath = Path.Combine(baseDirectory, "config");
string zendeskSiteFilePath = Path.Combine(configFolderPath, "zendeskSite.txt");
string userEmailFilePath = Path.Combine(configFolderPath, "userEmail.txt");
string userTokenFilePath = Path.Combine(configFolderPath, "userToken.txt");

// Créer le dossier de configuration s'il n'existe pas déjà
Directory.CreateDirectory(configFolderPath);

string zendeskSite, userEmail, userToken;

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




Console.WriteLine($"Le dossier de configuration est à: {configFolderPath}");


var api = new ZendeskApi(zendeskSite, userEmail, userToken, "true");

string connectionString = """Server=LROLAP\SQLEXPRESS01;Database=Zendesk_database;User Id=Wizard;Password=Wizard;""";

using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();

    var tickets = api.Tickets.GetAllTickets();

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
            command.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(item.Type) ? (object)DBNull.Value : item.Type); command.Parameters.AddWithValue("@Url", item.Url);

            command.ExecuteNonQuery();
        }

    }
    Console.WriteLine("[1] Voulez-vous afficher les tickets actuels\n[Autre touche] Quitter");

    if (Console.ReadLine() == "1")
    {


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

}
