using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Channels;
using Modbus.Net;
using Modbus.Net.Modbus;
using ModbusExtension;

namespace ModbusServiceDLL;

public class Tasks
{
    public static async Task MainTask(ModbusMachineExtended<string, string, string, string> extendedMachine, CancellationToken stoppingToken)
    {
        bool connectionStatusMachine = await ConnectToServer(extendedMachine);

        if (!connectionStatusMachine)
        {
            Console.WriteLine("Connection in Server Port " + extendedMachine.ConnectionToken + " is Timed Out!");
        }
        else
        {
            Console.WriteLine("Connection in Server Port " + extendedMachine.ConnectionToken + " is Successful!");

            await SetData(extendedMachine, "City", 11);
            await SetData(extendedMachine, "Date & Time", 22);
            await SetData(extendedMachine, "Time Zone", 33);
            await Task.WhenAll();
            await Task.Delay(5000);

            Console.WriteLine("--------------SetData in Server Port " + extendedMachine.ConnectionToken + " Successfully!--------------");


            #region OldGetData
            // var getDataTask1 = new Task(async () =>
            // {
            //     while (!stoppingToken.IsCancellationRequested)
            //     {
            //         // Console.WriteLine("Task 1, Timestamp: " + formattedTimestamp);
            //         await GetData(extendedMachine, "City");
            //         await Task.Delay(5000);
            //     }
            // });

            // var getDataTask2 = new Task(async () =>
            // {
            //     while (!stoppingToken.IsCancellationRequested)
            //     {
            //         // Console.WriteLine("Task 2, Timestamp: " + formattedTimestamp);
            //         await GetData(extendedMachine, "Date & Time");
            //         await Task.Delay(5000);
            //     }
            // });

            // var getDataTask3 = new Task(async () =>
            // {
            //     while (!stoppingToken.IsCancellationRequested)
            //     {
            //         // Console.WriteLine("Task 3, Timestamp: " + formattedTimestamp);
            //         await GetData(extendedMachine, "Time Zone");
            //         await Task.Delay(5000);
            //     }
            // });
            // getDataTask1.Start();
            // // await Task.Delay(2000); // Server Conflict without Delay
            // getDataTask2.Start();
            // // await Task.Delay(2000); // Server Conflict without Delay
            // getDataTask3.Start();

            // // Task[] tasks = { getDataTask1, getDataTask2, getDataTask3 };
            // await Task.WhenAll(getDataTask1, getDataTask2, getDataTask3);
            #endregion


            #region NewGetData
            var getDataTask1 = new Task(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Console.WriteLine("Task 1, Timestamp: " + formattedTimestamp);
                    var returnGetObject1 = await GetAsync(extendedMachine, "City");
                    PrintDataObject(returnGetObject1,extendedMachine, "City");
                    await Task.Delay(1000);
                }
            });

            var getDataTask2 = new Task(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Console.WriteLine("Task 2, Timestamp: " + formattedTimestamp);
                    var returnGetObject2 = await GetAsync(extendedMachine, "Date & Time");
                    PrintDataObject(returnGetObject2,extendedMachine, "Date & Time");
                    await Task.Delay(3000);
                }
            });

            var getDataTask3 = new Task(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Console.WriteLine("Task 3, Timestamp: " + formattedTimestamp);
                    var returnGetObject3 = await GetAsync(extendedMachine, "Time Zone");
                    PrintDataObject(returnGetObject3,extendedMachine, "Time Zone");
                    await Task.Delay(5000);
                }
            });

            getDataTask1.Start();
            getDataTask2.Start();
            getDataTask3.Start();

            await Task.WhenAll(getDataTask1, getDataTask2, getDataTask3);
            #endregion
}
    }

    public static ModbusMachineExtended<string, string, string,string> CreateModbusMachine(string ip, string serverType)
    {
        string jsonFilePath = "JsonData/"+serverType+".json";
        List<Base>? jsonDataArrayBase = JsonHandler.LoadFromFileBase(jsonFilePath);
        List<AddressUnit<string,int,int>>? BaseAddressUnits = JsonHandler.AddressUnitCreator(jsonDataArrayBase!);
        return new ModbusMachineExtended<string,string,string,string>("1", ModbusType.Tcp, ip, BaseAddressUnits, false, 1, 0, Endian.BigEndianLsb);
    }

    public static Task<bool> ConnectToServer(ModbusMachineExtended<string,string,string,string> extendedMachine) 
    {
        return extendedMachine.ConnectAsync();
    }

    public static async Task SetData(ModbusMachineExtended<string,string,string,string> extendedMachine, string communicationTag, double dataValue)
    {
        string setTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var returnSetObject = await extendedMachine.SetDatasByCommunicationTag(communicationTag, dataValue);
        int SetErrorCode = returnSetObject.ErrorCode;
        string SetErrorMsg = returnSetObject.ErrorMsg;
        bool SetSuccessStatus = returnSetObject.IsSuccess;

        if (SetSuccessStatus)
            Console.WriteLine("Set '" + communicationTag + "' = " + dataValue + ", to Server Port " + extendedMachine.ConnectionToken + ", at " + setTimestamp + ".");
        else
        {
            Console.WriteLine("Set Data Status: Error Code: " + SetErrorCode + ". Error Message: " + SetErrorMsg + ".");
        }
    }

    public static async Task GetData(ModbusMachineExtended<string,string,string,string> extendedMachine, string communicationTag)
    {
        string getTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var returnGetObject = await extendedMachine.GetDatasByCommunicationTag(communicationTag);
        byte[]? GetDatas = returnGetObject.Datas;
        int GetErrorCode = returnGetObject.ErrorCode;
        string GetErrorMsg = returnGetObject.ErrorMsg;
        bool GetSuccessStatus = returnGetObject.IsSuccess;

        if (GetSuccessStatus && GetDatas != null)
        {
            Console.WriteLine("Get '" + communicationTag + "' = " + GetDatas[^1] + ", from Server Port " + extendedMachine.ConnectionToken + ", at " + getTimestamp + ".");
        }
        else
        {
            Console.WriteLine("Get '" + communicationTag + "' from Server Port " + extendedMachine.ConnectionToken + ": Error Code: " + GetErrorCode + ". Error Message: " + GetErrorMsg + ".");
        }
    }

    public static async Task<ReturnStruct<byte[]>> GetAsync(ModbusMachineExtended<string,string,string,string> extendedMachine, string communicationTag)
    {
        var returnGetObject = await extendedMachine.GetDatasByCommunicationTag(communicationTag);
        return returnGetObject;       
    }

    public static void PrintDataObject(ReturnStruct<byte[]> returnGetObject, ModbusMachineExtended<string,string,string,string> extendedMachine, string communicationTag)
    {
        string getTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        byte[]? GetDatas = returnGetObject.Datas;
        int GetErrorCode = returnGetObject.ErrorCode;
        string GetErrorMsg = returnGetObject.ErrorMsg;
        bool GetSuccessStatus = returnGetObject.IsSuccess;

        if (GetSuccessStatus && GetDatas != null)
        {
            Console.WriteLine("Get '" + communicationTag + "' = " + GetDatas[^1] + ", from Server Port " + extendedMachine.ConnectionToken + ", at " + getTimestamp + ".");
        }
        else
        {
            Console.WriteLine("Get '" + communicationTag + "' from Server Port " + extendedMachine.ConnectionToken + ": Error Code: " + GetErrorCode + ". Error Message: " + GetErrorMsg + ".");
        }
    }  
}
