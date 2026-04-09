using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Playbooks;

[Authorize(SystemIntelligencePlatformPermissions.Playbooks.Default)]
public class PlaybookAppService : ApplicationService, IPlaybookAppService
{
    private readonly IRepository<Playbook, Guid> _playbookRepository;
    private readonly IRepository<PlaybookStep, Guid> _stepRepository;
    private readonly IRepository<PlaybookRun, Guid> _runRepository;
    private readonly IRepository<PlaybookRunStep, Guid> _runStepRepository;
    private readonly IIncidentRepository _incidentRepository;

    public PlaybookAppService(
        IRepository<Playbook, Guid> playbookRepository,
        IRepository<PlaybookStep, Guid> stepRepository,
        IRepository<PlaybookRun, Guid> runRepository,
        IRepository<PlaybookRunStep, Guid> runStepRepository,
        IIncidentRepository incidentRepository)
    {
        _playbookRepository = playbookRepository;
        _stepRepository = stepRepository;
        _runRepository = runRepository;
        _runStepRepository = runStepRepository;
        _incidentRepository = incidentRepository;
    }

    public async Task<PlaybookDto> GetAsync(Guid id)
    {
        var p = await _playbookRepository.GetAsync(id);
        return await MapPlaybookAsync(p);
    }

    public async Task<PagedResultDto<PlaybookDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var q = await _playbookRepository.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(q);
        var list = await AsyncExecuter.ToListAsync(q.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));
        var dtos = new List<PlaybookDto>();
        foreach (var p in list)
            dtos.Add(await MapPlaybookAsync(p));

        return new PagedResultDto<PlaybookDto>(total, dtos);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Playbooks.Manage)]
    public async Task<PlaybookDto> CreateAsync(CreatePlaybookDto input)
    {
        var id = GuidGenerator.Create();
        var p = new Playbook(id, input.Name, input.TriggerDefinitionJson)
        {
            Description = input.Description
        };

        foreach (var s in input.Steps.OrderBy(x => x.SortOrder))
        {
            p.Steps.Add(new PlaybookStep(GuidGenerator.Create(), id, s.SortOrder, s.Title, s.Body));
        }

        await _playbookRepository.InsertAsync(p);
        return await MapPlaybookAsync(p);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Playbooks.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var runIds = await AsyncExecuter.ToListAsync(
            (await _runRepository.GetQueryableAsync()).Where(r => r.PlaybookId == id).Select(r => r.Id));
        foreach (var rid in runIds)
        {
            var rSteps = await AsyncExecuter.ToListAsync(
                (await _runStepRepository.GetQueryableAsync()).Where(s => s.PlaybookRunId == rid));
            foreach (var rs in rSteps)
                await _runStepRepository.DeleteAsync(rs);
            await _runRepository.DeleteAsync(rid);
        }

        var steps = await AsyncExecuter.ToListAsync((await _stepRepository.GetQueryableAsync()).Where(s => s.PlaybookId == id));
        foreach (var s in steps)
            await _stepRepository.DeleteAsync(s);

        await _playbookRepository.DeleteAsync(id);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Playbooks.Run)]
    public async Task<PlaybookRunDto> RunForIncidentAsync(Guid playbookId, Guid incidentId)
    {
        await _incidentRepository.GetAsync(incidentId);
        var playbook = await _playbookRepository.GetAsync(playbookId);
        var steps = await AsyncExecuter.ToListAsync(
            (await _stepRepository.GetQueryableAsync()).Where(s => s.PlaybookId == playbookId).OrderBy(s => s.SortOrder));

        var runId = GuidGenerator.Create();
        var run = new PlaybookRun(runId, playbookId, incidentId);
        foreach (var s in steps)
        {
            run.RunSteps.Add(new PlaybookRunStep(GuidGenerator.Create(), runId, s.SortOrder, s.Title));
        }

        await _runRepository.InsertAsync(run);
        return await MapRunAsync(run);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Playbooks.Run)]
    public async Task<PlaybookRunDto> CompleteRunStepAsync(Guid runId, int stepOrder)
    {
        var run = await _runRepository.GetAsync(runId);
        var step = await AsyncExecuter.FirstOrDefaultAsync(
            (await _runStepRepository.GetQueryableAsync()).Where(s => s.PlaybookRunId == runId && s.StepOrder == stepOrder));
        if (step == null)
            throw new Volo.Abp.BusinessException("SIP:PlaybookStepNotFound");

        step.IsCompleted = true;
        step.CompletedAt = DateTime.UtcNow;
        await _runStepRepository.UpdateAsync(step);

        var remaining = await AsyncExecuter.CountAsync(
            (await _runStepRepository.GetQueryableAsync()).Where(s => s.PlaybookRunId == runId && !s.IsCompleted));
        if (remaining == 0)
        {
            run.Status = PlaybookRunStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
            await _runRepository.UpdateAsync(run);
        }

        return await MapRunAsync(await _runRepository.GetAsync(runId));
    }

    public async Task<PlaybookRunDto?> GetActiveRunAsync(Guid incidentId)
    {
        var q = await _runRepository.GetQueryableAsync();
        var run = await AsyncExecuter.FirstOrDefaultAsync(
            q.Where(r => r.IncidentId == incidentId && r.Status == PlaybookRunStatus.InProgress)
                .OrderByDescending(r => r.StartedAt));
        return run == null ? null : await MapRunAsync(run);
    }

    private async Task<PlaybookDto> MapPlaybookAsync(Playbook p)
    {
        var steps = await AsyncExecuter.ToListAsync(
            (await _stepRepository.GetQueryableAsync()).Where(s => s.PlaybookId == p.Id).OrderBy(s => s.SortOrder));
        return new PlaybookDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            TriggerDefinitionJson = p.TriggerDefinitionJson,
            IsActive = p.IsActive,
            CreationTime = p.CreationTime,
            CreatorId = p.CreatorId,
            LastModificationTime = p.LastModificationTime,
            LastModifierId = p.LastModifierId,
            Steps = steps.Select(s => new PlaybookStepDto
            {
                Id = s.Id,
                SortOrder = s.SortOrder,
                Title = s.Title,
                Body = s.Body
            }).ToList()
        };
    }

    private async Task<PlaybookRunDto> MapRunAsync(PlaybookRun run)
    {
        var steps = await AsyncExecuter.ToListAsync(
            (await _runStepRepository.GetQueryableAsync()).Where(s => s.PlaybookRunId == run.Id).OrderBy(s => s.StepOrder));
        return new PlaybookRunDto
        {
            Id = run.Id,
            PlaybookId = run.PlaybookId,
            IncidentId = run.IncidentId,
            Status = run.Status,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            CreationTime = run.CreationTime,
            CreatorId = run.CreatorId,
            LastModificationTime = run.LastModificationTime,
            LastModifierId = run.LastModifierId,
            RunSteps = steps.Select(s => new PlaybookRunStepDto
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                Title = s.Title,
                IsCompleted = s.IsCompleted,
                CompletedAt = s.CompletedAt
            }).ToList()
        };
    }
}
