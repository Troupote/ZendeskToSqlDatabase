# ZendeskToSqlDatabase

This program allows you to retrieve all your Zendesk tickets and users and write them into a SQL database. This allows users to perform SQL queries on this data.

## Prerequisites

- .NET 8.0
- An internet connection

## Usage

When you run the program, it first checks your internet connection. If you are not connected to the internet, the program automatically closes after 10 seconds.

Next, the program asks you if you want to connect to the last used server and Zendesk. If you answer "yes", the program tries to read the configuration information from existing files. If these files do not exist, the program asks you to provide the necessary information, such as the URL of your Zendesk site.

## Configuration

Configuration information is stored in the `config` folder. The configuration files are as follows:

- `zendeskSite.txt`: contains the URL of your Zendesk site.
- `userEmail.txt`: contains the user's email.
- `userToken.txt`: contains the user's token.
- `serverAdress.txt`: contains the SQL server address.
- `BaseName.txt`: contains the name of the SQL database.

## Dependencies

This project uses the following packages:

- System.Data.SqlClient (version 4.8.6)
- ZendeskApi_v2 (version 3.12.4)

## License

See the [LICENSE](LICENSE) file for details.