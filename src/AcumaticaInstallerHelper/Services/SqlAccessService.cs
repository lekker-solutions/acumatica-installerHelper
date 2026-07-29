using System.Data.Odbc;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

/// <summary>
///     Grants SQL Server access to the site's IIS application pool identity.
///     ac.exe only does this for pools created interactively by the
///     Configuration wizard — its command-line NewInstance scenario creates the
///     pool and the database but no login, so a freshly deployed site fails
///     with "Login failed for user 'IIS APPPOOL\&lt;pool&gt;'" on first request.
///     Uses ODBC with the in-box Windows "SQL Server" driver so the module
///     needs no SqlClient native dependencies.
/// </summary>
public class SqlAccessService : ISqlAccessService
{
    private readonly ILoggingService _loggingService;

    public SqlAccessService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public bool GrantAppPoolAccess(SiteConfiguration siteConfig)
    {
        string login = $@"IIS APPPOOL\{siteConfig.IISAppPool}";

        try
        {
            using var connection = new OdbcConnection(BuildConnectionString(siteConfig));
            connection.Open();

            _loggingService.WriteStep($"Granting SQL Server access to '{login}'");

            string escapedLoginLiteral = login.Replace("'", "''");
            string escapedLoginBracket = login.Replace("]", "]]");
            string escapedDbBracket    = siteConfig.DBName.Replace("]", "]]");

            ExecuteNonQuery(connection,
                $"IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'{escapedLoginLiteral}') " +
                $"CREATE LOGIN [{escapedLoginBracket}] FROM WINDOWS;");

            ExecuteNonQuery(connection,
                $"USE [{escapedDbBracket}]; " +
                $"IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{escapedLoginLiteral}') " +
                $"CREATE USER [{escapedLoginBracket}] FOR LOGIN [{escapedLoginBracket}]; " +
                $"ALTER ROLE db_owner ADD MEMBER [{escapedLoginBracket}];");

            _loggingService.WriteSuccess($"'{login}' has db_owner access to database '{siteConfig.DBName}'");
            return true;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError(
                $"Failed to grant SQL Server access to '{login}' on database '{siteConfig.DBName}': {ex.Message}");
            return false;
        }
    }

    private static string BuildConnectionString(SiteConfiguration siteConfig)
    {
        // The legacy in-box "SQL Server" ODBC driver resolves 'localhost' over
        // TCP/named pipes, which fails on instances that only listen on shared
        // memory; '(local)' always uses shared memory for a local instance.
        string server = siteConfig.DBServer;
        if (string.IsNullOrEmpty(server)
            || server.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || server == ".")
        {
            server = "(local)";
        }

        return siteConfig.DBServerAuth == DBServerAuthType.SQL
            ? $"Driver={{SQL Server}};Server={server};Uid={siteConfig.DBServerUsername};Pwd={siteConfig.DBServerPassword};"
            : $"Driver={{SQL Server}};Server={server};Trusted_Connection=yes;";
    }

    private static void ExecuteNonQuery(OdbcConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
