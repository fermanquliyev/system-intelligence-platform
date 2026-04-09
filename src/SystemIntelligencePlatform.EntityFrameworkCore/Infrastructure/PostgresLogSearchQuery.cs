using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SystemIntelligencePlatform.EntityFrameworkCore;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Infrastructure;

public class PostgresLogSearchQuery : ILogSearchQuery, ITransientDependency
{
    private readonly IDbContextProvider<SystemIntelligencePlatformDbContext> _dbContextProvider;

    public PostgresLogSearchQuery(IDbContextProvider<SystemIntelligencePlatformDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<(IReadOnlyList<Guid> Ids, int TotalCount)> SearchAsync(
        string? fullTextQuery,
        string? containsFallback,
        Guid? applicationId,
        LogLevel? minLevel,
        DateTime? fromUtc,
        DateTime? toUtc,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var db = await _dbContextProvider.GetDbContextAsync();
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        var useFts = !string.IsNullOrWhiteSpace(fullTextQuery);
        var term = (fullTextQuery ?? containsFallback ?? "").Trim();
        if (string.IsNullOrEmpty(term))
            return (Array.Empty<Guid>(), 0);

        var where = new StringBuilder(" WHERE 1=1 ");
        if (useFts)
            where.Append(@"AND to_tsvector('english', coalesce(""Message"",'')) @@ plainto_tsquery('english', @term) ");
        else
            where.Append(@"AND ""Message"" ILIKE @like ESCAPE '\' ");

        if (applicationId.HasValue)
            where.Append(@"AND ""ApplicationId"" = @appId ");

        if (minLevel.HasValue)
            where.Append(@"AND ""Level"" >= @minLevel ");

        if (fromUtc.HasValue)
            where.Append(@"AND ""Timestamp"" >= @fromUtc ");

        if (toUtc.HasValue)
            where.Append(@"AND ""Timestamp"" <= @toUtc ");

        var countSql = @"SELECT COUNT(*) FROM ""AppLogEvents""" + where;
        int total;
        await using (var countCmd = conn.CreateCommand())
        {
            countCmd.CommandText = countSql;
            AddParams(countCmd, term, useFts, applicationId, minLevel, fromUtc, toUtc);
            var totalObj = await countCmd.ExecuteScalarAsync(cancellationToken);
            total = Convert.ToInt32(totalObj);
        }

        var listSql = @"SELECT ""Id"" FROM ""AppLogEvents""" + where +
                      @" ORDER BY ""Timestamp"" DESC LIMIT @take OFFSET @skip";
        var ids = new List<Guid>();
        await using (var idCmd = conn.CreateCommand())
        {
            idCmd.CommandText = listSql;
            AddParams(idCmd, term, useFts, applicationId, minLevel, fromUtc, toUtc);
            AddIntParam(idCmd, "@take", take);
            AddIntParam(idCmd, "@skip", skip);

            await using (var reader = await idCmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    ids.Add(reader.GetGuid(0));
            }
        }

        return (ids, total);
    }

    private static void AddParams(
        System.Data.Common.DbCommand cmd,
        string term,
        bool useFts,
        Guid? applicationId,
        LogLevel? minLevel,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (useFts)
            AddTextParam(cmd, "@term", term);
        else
            AddTextParam(cmd, "@like", "%" + EscapeLike(term) + "%");

        if (applicationId.HasValue)
            AddGuidParam(cmd, "@appId", applicationId.Value);

        if (minLevel.HasValue)
            AddIntParam(cmd, "@minLevel", (int)minLevel.Value);

        if (fromUtc.HasValue)
            AddDateParam(cmd, "@fromUtc", fromUtc.Value);

        if (toUtc.HasValue)
            AddDateParam(cmd, "@toUtc", toUtc.Value);
    }

    private static string EscapeLike(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private static void AddTextParam(System.Data.Common.DbCommand cmd, string name, string value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private static void AddGuidParam(System.Data.Common.DbCommand cmd, string name, Guid value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private static void AddIntParam(System.Data.Common.DbCommand cmd, string name, int value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private static void AddDateParam(System.Data.Common.DbCommand cmd, string name, DateTime value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
