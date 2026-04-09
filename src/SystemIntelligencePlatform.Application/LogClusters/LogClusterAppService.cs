using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.LogClusters;

[Authorize(SystemIntelligencePlatformPermissions.LogEvents.Default)]
public class LogClusterAppService : ApplicationService, ILogClusterAppService
{
    private static readonly Regex DigitRun = new(@"\d+", RegexOptions.Compiled);

    private readonly IRepository<LogCluster, Guid> _clusterRepository;
    private readonly IRepository<LogEvent, Guid> _logRepository;

    public LogClusterAppService(
        IRepository<LogCluster, Guid> clusterRepository,
        IRepository<LogEvent, Guid> logRepository)
    {
        _clusterRepository = clusterRepository;
        _logRepository = logRepository;
    }

    public async Task<PagedResultDto<LogClusterDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var q = await _clusterRepository.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(q);
        var list = await AsyncExecuter.ToListAsync(q.OrderByDescending(c => c.LastSeen).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<LogClusterDto>(total,
            ObjectMapper.Map<List<LogCluster>, List<LogClusterDto>>(list));
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogEvents.Search)]
    public async Task<int> RunClusteringAsync(Guid? applicationId)
    {
        var q = await _logRepository.GetQueryableAsync();
        q = q.Where(e => e.ClusterId == null);
        if (applicationId.HasValue)
            q = q.Where(e => e.ApplicationId == applicationId.Value);

        var batch = await AsyncExecuter.ToListAsync(q.OrderByDescending(e => e.Timestamp).Take(3000));
        var groups = batch.GroupBy(e => TemplateHash(e.Message));

        var assigned = 0;
        foreach (var g in groups)
        {
            var sig = g.Key;
            var first = g.Min(x => x.Timestamp);
            var last = g.Max(x => x.Timestamp);
            var sample = g.OrderBy(x => x.Timestamp).First();
            var summary = sample.Message.Length > 200 ? sample.Message[..200] + "..." : sample.Message;

            var existing = await AsyncExecuter.FirstOrDefaultAsync(
                (await _clusterRepository.GetQueryableAsync()).Where(c => c.SignatureHash == sig));

            LogCluster cluster;
            if (existing == null)
            {
                cluster = new LogCluster(GuidGenerator.Create(), sig, summary, first)
                {
                    ApplicationId = sample.ApplicationId,
                    LastSeen = last,
                    EventCount = g.Count()
                };
                await _clusterRepository.InsertAsync(cluster);
            }
            else
            {
                cluster = existing;
                cluster.LastSeen = last;
                cluster.EventCount += g.Count();
                if (string.IsNullOrWhiteSpace(cluster.Summary))
                    cluster.Summary = summary;
                await _clusterRepository.UpdateAsync(cluster);
            }

            foreach (var e in g)
            {
                e.ClusterId = cluster.Id;
                await _logRepository.UpdateAsync(e);
                assigned++;
            }
        }

        return assigned;
    }

    private static string TemplateHash(string message)
    {
        var templated = DigitRun.Replace(message, "#");
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(templated));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
