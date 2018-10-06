# Alternative way to call OPOS control from POS for.NET

This is POS for.NET service object to call OPOS control from POS for.NET.

In POS for.NET, a mechanism called Legacy COM Interop, called OPOS control, is built inside POS for.NET.  
However, this mechanism has the following issues:

- Only 24 of 36 types of device classes defined in UnifiedPOS are supported (in case of v1.14.1)  
  Unsupported devices:  
  - Belt
  - Bill Acceptor
  - Bill Dispenser
  - Biometrics
  - Coin Acceptor
  - Electronic Journal
  - Electronic Value Reader/Writer
  - Gate
  - Image Scanner
  - Item Dispenser
  - Lights
  - RFID Scanner
- Changing the value of OPOS's BinaryConversion property requires a different conversion process than when calling OPOS directly

In order to solve these problems, i created a service object with the following features:

- Supported all 36 types of device classes defined in UnifiedPOS.  
- BinaryConversion processing for OPOS was divided into two kinds.  
  - In POS for.NET and OPOS, string properties/parameters are passed through without processing anything.  
  - Properties/parameters such as byte[] or Bitmap etc. in POS for.NET perform conversion processing according to the value of BinaryConversion.  
- When reading the property considered as Enum in POS for.NET, if OPOS notifies the undefined value, it raises PosControlException and notifies it by storing it in the exception's ErrorCodeExtended property.  
- Information on the corresponding OPOS device name is defined in the Configuration.xml file of POS for.NET.  

## Development/Execuion environment

To develop and execute this program you need:

- Visual Studio 2017 or Visual Studio Community 2017  version 15.8.5 (development only)  
- .NET framework 3.5 and 4.0 or later  
- Microsoft Point of Service for .NET v1.14.1 (POS for.NET) : https://www.microsoft.com/en-us/download/details.aspx?id=55758  
- Common Control Objects 1.14.001 : http://monroecs.com/oposccos_current.htm  
- OPOS for .NET Assemblies 1.14.001 : http://monroecs.com/posfordotnet/opos_dotnet.htm  
- OPOS service object of target device  

To develop/execute this service object, OPOS for .NET Assemblies is required together with Common Control Objects.  
OPOS for .NET Assemblies is not installed when only the target device's .ocx is installed by the device vendor's OPOS, or installed using the CCO Runtime .zip file, please install it separately.  
Both are installed by the CCO Installer .msi file installer.
Therefore, i recommend using this.

## Installation on execution environment

To install on the execution environment, please follow the procedure below.

- Create an appropriate folder and copy POS.AltCCOInterop.dll.  
  It is not the root of the drive, and the path name of the folder does not include the blank space and should consist only of alphanumeric characters less equal 0x7E.  
  There is less chance of that person having a problem.  

- Register with the arbitrary name with the above folder as the value in the ControlAssemblies key of the POS for.NET registry.  
  For example "AltCCOInterops"="C:\\\\POSforNET\\\\CCOInterop\\\\"  
  However, during development work, it is automatically registered as part of the processing at build time.  
  The position of the target key is as follows.  
  - 64bitOS: HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\POSfor.NET\\ControlAssemblies  
  - 32bitOS: HKEY_LOCAL_MACHINE\\SOFTWARE\\POSfor.NET\\ControlAssemblies  

## Configuration

Create a device entry and set its properties using the posdm.exe program of POS for.NET.

- Create a device entry with the ADDDEVICE command of posdm  
  Example usage: posdm ADDDEVICE OposEVRW1 /type:ElectronicValueRW /soname:"OpenPOS ElectronicValueRW"  

  - The specified device name ("OposEVRW1" in the example) is stored as the value of "HardwarePath"  
    Please specify a unique name that does not overlap the name of other POS for.NET or OPOS name.  
  - Please append "OpenPOS " to the head of /soname: and specify the device class name enclosed in double quotes.  
    For example, "OpenPOS CashDrawer", "OpenPOS POSPrinter", "OpenPOS Scanner" etc.  

- Set the OPOS device name to be used with the ADDPROPERTY command of posdm  
  Example usage: posdm ADDPROPERTY OposDeviceName VenderName_ModelName /type:ElectronicValueRW /soname:"OpenPOS ElectronicValueRW" /path:OposEVRW1  

  - The property name to be set is "OposDeviceName".  
  - Please specify the device name key or logical device name that exists in the OPOS registry for the value to be set ("VenderName_ModelName" in the example).  

Target device entry in Configuration.xml after execution example:


    <ServiceObject Name="OpenPOS ElectronicValueRW" Type="ElectronicValueRW">
      <Device HardwarePath="" Enabled="yes" PnP="no" />
      <Device HardwarePath="OposEVRW1" Enabled="yes" PnP="no">
        <Property Name="OposDeviceName" Value="VenderName_ModelName" />
      </Device>
    </ServiceObject>


## How to call

Here is a procedure and an example of calling the device entry created in the usage example of the above setting.

- Call PoExplorer's GetDevices method with the device class name and DeviceCompatibilities specified and get the device collection of the corresponding device class  
- Search for a DeviceInfo whose ServiceObjectName and HardwarePath match in the acquired device collection and generate an object with the CreateInstance method based on it  
- Register event handler  
- Call Open method  

Example code:


    ElectronicValueRW evrwObj1 = null;
    PosExplorer explorer = new PosExplorer();
    DeviceCollection evrwList = explorer.GetDevices("ElectronicValueRW", DeviceCompatibilities.CompatibilityLevel1);
    foreach (DeviceInfo evrwInfo in evrwList)
    {
        if (evrwInfo.ServiceObjectName == "OpenPOS ElectronicValueRW")
        {
            if (evrwInfo.HardwarePath == "OposEVRW1")
            {
                evrwObj1 = (ElectronicValueRW)explorer.CreateInstance(evrwInfo);
                break;
            }
        }
    }
    if (evrwObj1 != null)
    {
        evrwObj1.DataEvent += evrwObj1_DataEvent;
        evrwObj1.DirestIOEvent += evrwObj1_DirectIOEvent;
        evrwObj1.ErrorEvent += evrwObj1_ErrorEvent;
        evrwObj1.OutputCompleteEvent += evrwObj1_OutputCompleteEvent;
        evrwObj1.StatusUpdateEvent += evrwObj1_StatusUpdateEvent;
        evrwObj1.TransitionEvent += evrwObj1_TransitionEvent;
     
        evrwObj1.Open();
    }


Note: The value of the Compatibility property(DeviceCompatibilities) varies from case to case.  
In the state listed in DeviceCollection/DeviceInfo, it is "CompatibilityLevel1", and for objects generated by CreateInstance it is "Opos".

## Known issues

Currently known issues are as follows.

- Have not checked the operation using actual OPOS or device.  
- In particular, it is unknown whether the conversion of string (OPOS) and Bitmap etc (POS for.NET) of the following property/parameter/return value is correct.  
  - BiometricsInformationRecord(BIR) related property/parameter/return value of Biometrics device  
  - RawSensorData property of Biometrics device  
  - ImageData property of CheckScanner device  
  - FrameData property of ImageScanner device  
- There are no functions such as acquisition of operation record and information acquisition for troubleshooting.  

## Issues of POS for.NET  

The following issues were found in POS for.NET during the development process.  

- Information is insufficient in Biometrics' BIR defined in POS for.NET.  
  It does not contain the following information in the BIR diagram described in the UnifiedPOS specification.  
  - Quality  
  - Product ID (Owner, Type)  
  - Subtype  
  - Index Flag  
  - Index (UUID)  
  - Digital Signature  
- Also at BIR, whether it is a issue or not is not clear, but the situation is as follows.  
  - The Creation Date/Creation Time is the date and time when the BIR object was created in the POS for.NET service object, not the date and time the device read the information.  
    Although it may not be a problem with POS for.NET's own service object, information is rewritten in the case of processing relayed to/from OPOS.  

## Customize

If you want to customize for specific user/vendor specific processing etc, please do it freely.  
However, in that case, please change all the following information to make it an independent file so that it can be used concurrently with this service object at the same time.  

- File name: POS.AltCCOInterop.dll  
- namespace: POS.AltCCOInterop  
- GUID: [assembly: Guid("d8eca985-8c8f-4968-8b67-5657246fa78e")]  
- Service object name: [ServiceObject(DeviceType.Xxxx, "OpenPOS Xxxx",  
- Class name: public class OpenPOSXxxx :  

Note) "Xxxx" in the above contains the device class name of UnifiedPOS/POS for.NET.

It is good to reduce the amount of work to extract only the device you want to customize and create a new one.  

In Addition, if it is good function with versatility, please propose it here.

## License

Licensed under the [zlib License](./LICENSE).
