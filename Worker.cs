using System.Text.Json;
using ModbusExtension;

namespace ModbusServiceDLL;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Server>? jsonServersList = JsonSerializer.Deserialize<List<Server>>(File.ReadAllText("JsonData/servers.json"));
        
        var  extendedMachines = new List<ModbusMachineExtended<string, string, string,string>>();

        for (int i = 0; i < jsonServersList!.Count; i++)
        {
            extendedMachines.Add(Tasks.CreateModbusMachine(jsonServersList[i].Ip!, jsonServersList[i].ServerType!));
        }

        var tasks = extendedMachines.Select(async extendedMachine =>
        {
            await Tasks.MainTask(extendedMachine, stoppingToken);
        }).ToArray();

        await Task.WhenAll(tasks);
        _logger.LogInformation("Service Completed.");
    }

}