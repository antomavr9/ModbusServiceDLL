using Modbus.Net;
using ModbusExtension;

namespace ModbusServiceDLL;
public class DataObject
{
    public ReturnStruct<byte[]> ReturnGetObject { get; set; }
    public ModbusMachineExtended<string,string,string,string>? ExtendedMachine { get; set; }
    public string? CommunicationTag { get; set; }

    // Constructor
    public DataObject(ReturnStruct<byte[]> returnGetObject, ModbusMachineExtended<string, string, string, string>? extendedMachine, string? communicationTag)
    {
        ReturnGetObject = returnGetObject;
        ExtendedMachine = extendedMachine;
        CommunicationTag = communicationTag;
    }
}