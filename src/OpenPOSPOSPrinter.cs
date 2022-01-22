
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.PosPrinter, "OpenPOS POSPrinter", "OPOS POSPrinter Alternative CCO Interop", 1, 14)]

    public class OpenPOSPOSPrinter : PosPrinter, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSPOSPrinter _cco = null;
        private const string _oposDeviceClass = "POSPrinter";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event OutputCompleteEventHandler OutputCompleteEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSPOSPrinter()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSPOSPrinter()
        {
            Dispose(false);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: Discard the managed state (managed object).
                }

                if (_cco != null)
                {
                    _cco.DirectIOEvent -= (_IOPOSPOSPrinterEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSPOSPrinterEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSPOSPrinterEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSPOSPrinterEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
                    _cco = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #endregion

        #region Utility subroutine

        /// <summary>
        /// Check the processing result value of OPOS and generate a PosControlException exception if it is an error.
        /// </summary>
        /// <param name="value">OPOS method return value or ResultCode property value</param>
        private void VerifyResult(int value)
        {
            if (value != (int)ErrorCode.Success)
            {
                ErrorCode eValue = (ErrorCode)InteropEnum<ErrorCode>.ToEnumFromInteger(value);
                throw new Microsoft.PointOfService.PosControlException((_oposDeviceClass + ":" + _oposDeviceName), eValue, _cco.ResultCodeExtended);
            }
        }

        #endregion

        #region Process of relaying OPOS event and generating POS for.NET event

        private void _cco_DirectIOEvent(int EventNumber, ref int pData, ref string pString)
        {
            if (this.DirectIOEvent != null)
            {
                DirectIOEventArgs eDE = new DirectIOEventArgs(EventNumber, pData, pString);
                DirectIOEvent(this, eDE);
                pData = eDE.Data;
                pString = Convert.ToString(eDE.Object);
            }
        }

        private void _cco_ErrorEvent(int ResultCode, int ResultCodeExtended, int ErrorLocus, ref int pErrorResponse)
        {
            if (this.ErrorEvent != null)
            {
                ErrorCode eCode = (ErrorCode)InteropEnum<ErrorCode>.ToEnumFromInteger(ResultCode);
                ErrorLocus eLocus = (ErrorLocus)InteropEnum<ErrorLocus>.ToEnumFromInteger(ErrorLocus);
                ErrorResponse eResponse = (ErrorResponse)InteropEnum<ErrorResponse>.ToEnumFromInteger(pErrorResponse);
                DeviceErrorEventArgs eEE = new DeviceErrorEventArgs(eCode, ResultCodeExtended, eLocus, eResponse);
                ErrorEvent(this, eEE);
                pErrorResponse = (int)eEE.ErrorResponse;
            }
        }

        private void _cco_OutputCompleteEvent(int OutputID)
        {
            if (this.OutputCompleteEvent != null)
            {
                OutputCompleteEvent(this, new OutputCompleteEventArgs(OutputID));
            }
        }

        private void _cco_StatusUpdateEvent(int Data)
        {
            if (this.StatusUpdateEvent != null)
            {
                StatusUpdateEvent(this, new StatusUpdateEventArgs(Data));
            }
        }

        #endregion

        #region ILegacyControlObject member

        public BinaryConversion BinaryConversion
        {
            get
            {
                _binaryConversion = _cco.BinaryConversion;
                return (BinaryConversion)InteropEnum<BinaryConversion>.ToEnumFromInteger(_binaryConversion);
            }
            set
            {
                _cco.BinaryConversion = (int)value;
                _binaryConversion = _cco.BinaryConversion;
                VerifyResult(_cco.ResultCode);
            }
        }

        public string ControlObjectDescription
        {
            get { return _cco.ControlObjectDescription; }
        }

        public Version ControlObjectVersion
        {
            get { return InteropCommon.ToVersion(_cco.ControlObjectVersion); }
        }

        #endregion

        #region Device common properties

        public override bool CapCompareFirmwareVersion
        {
            get { return _cco.CapCompareFirmwareVersion; }
        }

        public override PowerReporting CapPowerReporting
        {
            get
            {
                return (PowerReporting)InteropEnum<PowerReporting>.ToEnumFromInteger(_cco.CapPowerReporting);
            }
        }

        public override bool CapStatisticsReporting
        {
            get { return _cco.CapStatisticsReporting; }
        }

        public override bool CapUpdateFirmware
        {
            get { return _cco.CapUpdateFirmware; }
        }

        public override bool CapUpdateStatistics
        {
            get { return _cco.CapUpdateStatistics; }
        }

        public override string CheckHealthText
        {
            get { return _cco.CheckHealthText; }
        }

        public override bool Claimed
        {
            get { return _cco.Claimed; }
        }

        public override int OutputId
        {
            get { return _cco.OutputID; }
        }

        public override bool DeviceEnabled
        {
            get
            {
                return _cco.DeviceEnabled;
            }
            set
            {
                _cco.DeviceEnabled = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool FreezeEvents
        {
            get
            {
                return _cco.FreezeEvents;
            }
            set
            {
                _cco.FreezeEvents = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override PowerNotification PowerNotify
        {
            get
            {
                return (PowerNotification)InteropEnum<PowerNotification>.ToEnumFromInteger(_cco.PowerNotify);
            }
            set
            {
                _cco.PowerNotify = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override PowerState PowerState
        {
            get
            {
                return (PowerState)InteropEnum<PowerState>.ToEnumFromInteger(_cco.PowerState);
            }
        }

        public override ControlState State
        {
            get
            {
                return (ControlState)InteropEnum<ControlState>.ToEnumFromInteger(_cco.State);
            }
        }

        public override string ServiceObjectDescription
        {
            get { return _cco.ServiceObjectDescription; }
        }

        public override Version ServiceObjectVersion
        {
            get { return InteropCommon.ToVersion(_cco.ControlObjectVersion); }
        }

        public override string DeviceDescription
        {
            get { return _cco.DeviceDescription; }
        }

        public override string DeviceName
        {
            get { return _cco.DeviceName; }
        }

        #endregion

        #region Device common method

        public override void Open()
        {
            if (string.IsNullOrWhiteSpace(_oposDeviceName))
            {
                try
                {
                    _oposDeviceName = GetConfigurationProperty("OposDeviceName");
                    _oposDeviceName.Trim();
                }
                catch
                {
                    _oposDeviceName = "";
                }
            }

            if (string.IsNullOrWhiteSpace(_oposDeviceName))
            {
                string strMessage = "OposDeviceName is not configured on " + DevicePath + ".";
                throw new Microsoft.PointOfService.PosControlException(strMessage, ErrorCode.NoExist);
            }

            if (_cco == null)
            {
                try
                {
                    // CCO object CreateInstance
                    _cco = new POS.Devices.OPOSPOSPrinter();

                    // Register event handler
                    _cco.DirectIOEvent += new _IOPOSPOSPrinterEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSPOSPrinterEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSPOSPrinterEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSPOSPrinterEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
                }
                catch
                {
                    string strMessage = "Can not create Common ControlObject on " + DevicePath + ".";
                    throw new Microsoft.PointOfService.PosControlException(strMessage, ErrorCode.Failure);
                }
            }

            VerifyResult(_cco.Open(_oposDeviceName));
        }

        public override void Close()
        {
            VerifyResult(_cco.Close());

            _cco.DirectIOEvent -= (_IOPOSPOSPrinterEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSPOSPrinterEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSPOSPrinterEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSPOSPrinterEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
            _cco = null;
        }

        public override void Claim(int timeout)
        {
            VerifyResult(_cco.ClaimDevice(timeout));
        }

        public override void Release()
        {
            VerifyResult(_cco.ReleaseDevice());
        }

        public override string CheckHealth(HealthCheckLevel level)
        {
            VerifyResult(_cco.CheckHealth((int)level));
            return _cco.CheckHealthText;
        }

        public override void ClearOutput()
        {
            VerifyResult(_cco.ClearOutput());
        }

        public override DirectIOData DirectIO(int command, int data, object obj)
        {
            var intValue = data;
            var stringValue = Convert.ToString(obj);
            VerifyResult(_cco.DirectIO(command, ref intValue, ref stringValue));
            return new DirectIOData(intValue, stringValue);
        }

        public override CompareFirmwareResult CompareFirmwareVersion(string firmwareFileName)
        {
            int result;
            VerifyResult(_cco.CompareFirmwareVersion(firmwareFileName, out result));
            return (CompareFirmwareResult)InteropEnum<CompareFirmwareResult>.ToEnumFromInteger(result);
        }

        public override void UpdateFirmware(string firmwareFileName)
        {
            VerifyResult(_cco.UpdateFirmware(firmwareFileName));
        }

        public override void ResetStatistic(string statistic)
        {
            VerifyResult(_cco.ResetStatistics(statistic));
        }

        public override void ResetStatistics(string[] statistics)
        {
            VerifyResult(_cco.ResetStatistics(string.Join(",", statistics)));
        }

        public override void ResetStatistics(StatisticCategories statistics)
        {
            VerifyResult(_cco.ResetStatistics(Enum.GetName(typeof(StatisticCategories), statistics)));
        }

        public override void ResetStatistics()
        {
            VerifyResult(_cco.ResetStatistics(""));
        }

        public override string RetrieveStatistic(string statistic)
        {
            var result = statistic;
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override string RetrieveStatistics(string[] statistics)
        {
            var result = string.Join(",", statistics);
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override string RetrieveStatistics(StatisticCategories statistics)
        {
            var result = Enum.GetName(typeof(StatisticCategories), statistics);
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override string RetrieveStatistics()
        {
            var result = "";
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override void UpdateStatistic(string name, object value)
        {
            VerifyResult(_cco.UpdateStatistics(name + "=" + value));
        }

        public override void UpdateStatistics(StatisticCategories statistics, object value)
        {
            VerifyResult(_cco.UpdateStatistics(Enum.GetName(typeof(StatisticCategories), statistics) + "=" + value));
        }

        public override void UpdateStatistics(Statistic[] statistics)
        {
            VerifyResult(_cco.UpdateStatistics(InteropCommon.ToStatisticsString(statistics)));
        }

        #endregion

        #region POSPrinter Specific Properties

        public override bool AsyncMode
        {
            get
            {
                return _cco.AsyncMode;
            }
            set
            {
                _cco.AsyncMode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override CharacterSetCapability CapCharacterSet
        {
            get { return (CharacterSetCapability)InteropEnum<CharacterSetCapability>.ToEnumFromInteger(_cco.CapCharacterSet); }
        }

        public override bool CapConcurrentJrnRec
        {
            get { return _cco.CapConcurrentJrnRec; }
        }

        public override bool CapConcurrentJrnSlp
        {
            get { return _cco.CapConcurrentJrnSlp; }
        }

        public override bool CapConcurrentRecSlp
        {
            get { return _cco.CapConcurrentRecSlp; }
        }

        public override bool CapConcurrentPageMode
        {
            get { return _cco.CapConcurrentPageMode; }
        }

        public override bool CapCoverSensor
        {
            get { return _cco.CapCoverSensor; }
        }

        public override bool CapJrn2Color
        {
            get { return _cco.CapJrn2Color; }
        }

        public override bool CapJrnBold
        {
            get { return _cco.CapJrnBold; }
        }

        public override PrinterCartridgeSensors CapJrnCartridgeSensor
        {
            get { return (PrinterCartridgeSensors)InteropEnum<PrinterCartridgeSensors>.ToEnumFromInteger(_cco.CapJrnCartridgeSensor); }
        }

        public override PrinterColors CapJrnColor
        {
            get { return (PrinterColors)_cco.CapJrnColor; }
        }

        public override bool CapJrnDHigh
        {
            get { return _cco.CapJrnDhigh; }
        }

        public override bool CapJrnDWide
        {
            get { return _cco.CapJrnDwide; }
        }

        public override bool CapJrnDWideDHigh
        {
            get { return _cco.CapJrnDwideDhigh; }
        }

        public override bool CapJrnEmptySensor
        {
            get { return _cco.CapJrnEmptySensor; }
        }

        public override bool CapJrnItalic
        {
            get { return _cco.CapJrnItalic; }
        }

        public override bool CapJrnNearEndSensor
        {
            get { return _cco.CapJrnNearEndSensor; }
        }

        public override bool CapJrnPresent
        {
            get { return _cco.CapJrnPresent; }
        }

        public override bool CapJrnUnderline
        {
            get { return _cco.CapJrnUnderline; }
        }

        public override bool CapMapCharacterSet
        {
            get { return _cco.CapMapCharacterSet; }
        }

        public override bool CapRec2Color
        {
            get { return _cco.CapRec2Color; }
        }

        public override bool CapRecBarCode
        {
            get { return _cco.CapRecBarCode; }
        }

        public override bool CapRecBitmap
        {
            get { return _cco.CapRecBitmap; }
        }

        public override bool CapRecBold
        {
            get { return _cco.CapRecBold; }
        }

        public override PrinterCartridgeSensors CapRecCartridgeSensor
        {
            get { return (PrinterCartridgeSensors)InteropEnum<PrinterCartridgeSensors>.ToEnumFromInteger(_cco.CapRecCartridgeSensor); }
        }

        public override PrinterColors CapRecColor
        {
            get { return (PrinterColors)InteropEnum<PrinterColors>.ToEnumFromInteger(_cco.CapRecColor); }
        }

        public override bool CapRecDHigh
        {
            get { return _cco.CapRecDhigh; }
        }

        public override bool CapRecDWide
        {
            get { return _cco.CapRecDwide; }
        }

        public override bool CapRecDWideDHigh
        {
            get { return _cco.CapRecDwideDhigh; }
        }

        public override bool CapRecEmptySensor
        {
            get { return _cco.CapRecEmptySensor; }
        }

        public override bool CapRecItalic
        {
            get { return _cco.CapRecItalic; }
        }

        public override bool CapRecLeft90
        {
            get { return _cco.CapRecLeft90; }
        }

        public override PrinterMarkFeeds CapRecMarkFeed
        {
            get { return (PrinterMarkFeeds)InteropEnum<PrinterMarkFeeds>.ToEnumFromInteger(_cco.CapRecMarkFeed); }
        }

        public override bool CapRecNearEndSensor
        {
            get { return _cco.CapRecNearEndSensor; }
        }

        public override bool CapRecPageMode
        {
            get { return _cco.CapRecPageMode; }
        }

        public override bool CapRecPaperCut
        {
            get { return _cco.CapRecPapercut; }
        }

        public override bool CapRecPresent
        {
            get { return _cco.CapRecPresent; }
        }

        public override bool CapRecRight90
        {
            get { return _cco.CapRecRight90; }
        }

        public override bool CapRecRotate180
        {
            get { return _cco.CapRecRotate180; }
        }

        public override LineDirection CapRecRuledLine
        {
            get { return (LineDirection)InteropEnum<LineDirection>.ToEnumFromInteger(_cco.CapRecRuledLine); }
        }

        public override bool CapRecStamp
        {
            get { return _cco.CapRecStamp; }
        }

        public override bool CapRecUnderline
        {
            get { return _cco.CapRecUnderline; }
        }

        public override bool CapSlp2Color
        {
            get { return _cco.CapSlp2Color; }
        }

        public override bool CapSlpBarCode
        {
            get { return _cco.CapSlpBarCode; }
        }

        public override bool CapSlpBitmap
        {
            get { return _cco.CapSlpBitmap; }
        }

        public override bool CapSlpBold
        {
            get { return _cco.CapSlpBold; }
        }

        public override bool CapSlpBothSidesPrint
        {
            get { return _cco.CapSlpBothSidesPrint; }
        }

        public override PrinterCartridgeSensors CapSlpCartridgeSensor
        {
            get { return (PrinterCartridgeSensors)InteropEnum<PrinterCartridgeSensors>.ToEnumFromInteger(_cco.CapSlpCartridgeSensor); }
        }

        public override PrinterColors CapSlpColor
        {
            get { return (PrinterColors)InteropEnum<PrinterColors>.ToEnumFromInteger(_cco.CapSlpColor); }
        }

        public override bool CapSlpDHigh
        {
            get { return _cco.CapSlpDhigh; }
        }

        public override bool CapSlpDWide
        {
            get { return _cco.CapSlpDwide; }
        }

        public override bool CapSlpDWideDHigh
        {
            get { return _cco.CapSlpDwideDhigh; }
        }

        public override bool CapSlpEmptySensor
        {
            get { return _cco.CapSlpEmptySensor; }
        }

        public override bool CapSlpFullSlip
        {
            get { return _cco.CapSlpFullslip; }
        }

        public override bool CapSlpItalic
        {
            get { return _cco.CapSlpItalic; }
        }

        public override bool CapSlpLeft90
        {
            get { return _cco.CapSlpLeft90; }
        }

        public override bool CapSlpNearEndSensor
        {
            get { return _cco.CapSlpNearEndSensor; }
        }

        public override bool CapSlpPageMode
        {
            get { return _cco.CapSlpPageMode; }
        }

        public override bool CapSlpPresent
        {
            get { return _cco.CapSlpPresent; }
        }

        public override bool CapSlpRight90
        {
            get { return _cco.CapSlpRight90; }
        }

        public override bool CapSlpRotate180
        {
            get { return _cco.CapSlpRotate180; }
        }

        public override LineDirection CapSlpRuledLine
        {
            get { return (LineDirection)InteropEnum<LineDirection>.ToEnumFromInteger(_cco.CapSlpRuledLine); }
        }

        public override bool CapSlpUnderline
        {
            get { return _cco.CapSlpUnderline; }
        }

        public override bool CapTransaction
        {
            get { return _cco.CapTransaction; }
        }

        public override PrinterCartridgeNotify CartridgeNotify
        {
            get
            {
                return (PrinterCartridgeNotify)InteropEnum<PrinterCartridgeNotify>.ToEnumFromInteger(_cco.CartridgeNotify);
            }
            set
            {
                _cco.CartridgeNotify = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int CharacterSet
        {
            get
            {
                return _cco.CharacterSet;
            }
            set
            {
                _cco.CharacterSet = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int[] CharacterSetList
        {
            get { return InteropCommon.ToIntegerArray(_cco.CharacterSetList, ','); }
        }

        public override bool CoverOpen
        {
            get { return _cco.CoverOpen; }
        }

        public override PrinterErrorLevel ErrorLevel
        {
            get { return (PrinterErrorLevel)InteropEnum<PrinterErrorLevel>.ToEnumFromInteger(_cco.ErrorLevel); }
        }

        public override PrinterStation ErrorStation
        {
            get { return (PrinterStation)InteropEnum<PrinterStation>.ToEnumFromInteger(_cco.ErrorStation); }
        }

        public override string ErrorString
        {
            get { return _cco.ErrorString; }
        }

        public override bool FlagWhenIdle
        {
            get
            {
                return _cco.FlagWhenIdle;
            }
            set
            {
                _cco.FlagWhenIdle = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string[] FontTypefaceList
        {
            get { return _cco.FontTypefaceList.Split(','); }
        }

        public override PrinterCartridgeStates JrnCartridgeState
        {
            get { return (PrinterCartridgeStates)InteropEnum<PrinterCartridgeStates>.ToEnumFromInteger(_cco.JrnCartridgeState); }
        }

        public override PrinterColors JrnCurrentCartridge
        {
            get
            {
                return (PrinterColors)InteropEnum<PrinterColors>.ToEnumFromInteger(_cco.JrnCurrentCartridge);
            }
            set
            {
                _cco.JrnCurrentCartridge = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool JrnEmpty
        {
            get { return _cco.JrnEmpty; }
        }

        public override bool JrnLetterQuality
        {
            get
            {
                return _cco.JrnLetterQuality;
            }
            set
            {
                _cco.JrnLetterQuality = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int JrnLineChars
        {
            get
            {
                return _cco.JrnLineChars;
            }
            set
            {
                _cco.JrnLineChars = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int[] JrnLineCharsList
        {
            get { return InteropCommon.ToIntegerArray(_cco.JrnLineCharsList, ','); }
        }

        public override int JrnLineHeight
        {
            get
            {
                return _cco.JrnLineHeight;
            }
            set
            {
                _cco.JrnLineHeight = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int JrnLineSpacing
        {
            get
            {
                return _cco.JrnLineSpacing;
            }
            set
            {
                _cco.JrnLineSpacing = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int JrnLineWidth
        {
            get { return _cco.JrnLineWidth; }
        }

        public override bool JrnNearEnd
        {
            get { return _cco.JrnNearEnd; }
        }

        public override bool MapCharacterSet
        {
            get
            {
                return _cco.MapCharacterSet;
            }
            set
            {
                _cco.MapCharacterSet = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override MapMode MapMode
        {
            get
            {
                return (MapMode)InteropEnum<MapMode>.ToEnumFromInteger(_cco.MapMode);
            }
            set
            {
                _cco.MapMode = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override Rotation[] RecBarCodeRotationList
        {
            get { return InteropCommon.ToRotationArray(_cco.RecBarCodeRotationList); }
        }

        public override Rotation[] RecBitmapRotationList
        {
            get { return InteropCommon.ToRotationArray(_cco.RecBitmapRotationList); }
        }

        public override PrinterCartridgeStates RecCartridgeState
        {
            get { return (PrinterCartridgeStates)InteropEnum<PrinterCartridgeStates>.ToEnumFromInteger(_cco.RecCartridgeState); }
        }

        public override PrinterColors RecCurrentCartridge
        {
            get
            {
                return (PrinterColors)InteropEnum<PrinterColors>.ToEnumFromInteger(_cco.RecCurrentCartridge);
            }
            set
            {
                _cco.RecCurrentCartridge = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool RecEmpty
        {
            get { return _cco.RecEmpty; }
        }

        public override bool RecLetterQuality
        {
            get
            {
                return _cco.RecLetterQuality;
            }
            set
            {
                _cco.RecLetterQuality = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int RecLineChars
        {
            get
            {
                return _cco.RecLineChars;
            }
            set
            {
                _cco.RecLineChars = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int[] RecLineCharsList
        {
            get { return InteropCommon.ToIntegerArray(_cco.RecLineCharsList, ','); }
        }

        public override int RecLineHeight
        {
            get
            {
                return _cco.RecLineHeight;
            }
            set
            {
                _cco.RecLineHeight = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int RecLineSpacing
        {
            get
            {
                return _cco.RecLineSpacing;
            }
            set
            {
                _cco.RecLineSpacing = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int RecLineWidth
        {
            get { return _cco.RecLineWidth; }
        }

        public override int RecLinesToPaperCut
        {
            get { return _cco.RecLinesToPaperCut; }
        }

        public override bool RecNearEnd
        {
            get { return _cco.RecNearEnd; }
        }

        public override int RecSidewaysMaxChars
        {
            get { return _cco.RecSidewaysMaxChars; }
        }

        public override int RecSidewaysMaxLines
        {
            get { return _cco.RecSidewaysMaxLines; }
        }

        public override Rotation RotateSpecial
        {
            get
            {
                return (Rotation)InteropEnum<Rotation>.ToEnumFromInteger(_cco.RotateSpecial);
            }
            set
            {
                _cco.RotateSpecial = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override Rotation[] SlpBarCodeRotationList
        {
            get { return InteropCommon.ToRotationArray(_cco.SlpBarCodeRotationList); }
        }

        public override Rotation[] SlpBitmapRotationList
        {
            get { return InteropCommon.ToRotationArray(_cco.SlpBitmapRotationList); }
        }

        public override PrinterCartridgeStates SlpCartridgeState
        {
            get { return (PrinterCartridgeStates)InteropEnum<PrinterCartridgeStates>.ToEnumFromInteger(_cco.SlpCartridgeState); }
        }

        public override PrinterColors SlpCurrentCartridge
        {
            get
            {
                return (PrinterColors)InteropEnum<PrinterColors>.ToEnumFromInteger(_cco.SlpCurrentCartridge);
            }
            set
            {
                _cco.SlpCurrentCartridge = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool SlpEmpty
        {
            get { return _cco.SlpEmpty; }
        }

        public override bool SlpLetterQuality
        {
            get
            {
                return _cco.SlpLetterQuality;
            }
            set
            {
                _cco.SlpLetterQuality = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int SlpLineChars
        {
            get
            {
                return _cco.SlpLineChars;
            }
            set
            {
                _cco.SlpLineChars = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int[] SlpLineCharsList
        {
            get { return InteropCommon.ToIntegerArray(_cco.SlpLineCharsList, ','); }
        }

        public override int SlpLineHeight
        {
            get
            {
                return _cco.SlpLineHeight;
            }
            set
            {
                _cco.SlpLineHeight = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int SlpLineSpacing
        {
            get
            {
                return _cco.SlpLineSpacing;
            }
            set
            {
                _cco.SlpLineSpacing = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int SlpLineWidth
        {
            get { return _cco.SlpLineWidth; }
        }

        public override int SlpLinesNearEndToEnd
        {
            get { return _cco.SlpLinesNearEndToEnd; }
        }

        public override int SlpMaxLines
        {
            get { return _cco.SlpMaxLines; }
        }

        public override bool SlpNearEnd
        {
            get { return _cco.SlpNearEnd; }
        }

        public override PrinterSide SlpPrintSide
        {
            get { return (PrinterSide)InteropEnum<PrinterSide>.ToEnumFromInteger(_cco.SlpPrintSide); }
        }

        public override int SlpSidewaysMaxChars
        {
            get { return _cco.SlpSidewaysMaxChars; }
        }

        public override int SlpSidewaysMaxLines
        {
            get { return _cco.SlpSidewaysMaxLines; }
        }

        public override Point PageModeArea
        {
            get
            {
                string work = _cco.PageModeArea;
                if (string.IsNullOrWhiteSpace(work))
                {
                    return Point.Empty;
                }
                else
                {
                    int[] pair = work.Split(',').Select<string, int>(int.Parse).ToArray();
                    return new Point(pair[0], pair[1]);
                }
            }
        }

        public override PageModeDescriptors PageModeDescriptor
        {
            get
            {
                return (PageModeDescriptors)InteropEnum<PageModeDescriptors>.ToEnumFromInteger(_cco.PageModeDescriptor);
            }
        }

        public override int PageModeHorizontalPosition
        {
            get
            {
                return _cco.PageModeHorizontalPosition;
            }
            set
            {
                _cco.PageModeHorizontalPosition = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override Rectangle PageModePrintArea
        {
            get
            {
                string work = _cco.PageModePrintArea;
                if (string.IsNullOrWhiteSpace(work))
                {
                    return Rectangle.Empty;
                }
                else
                {
                    int[] rect = work.Split(',').Select<string, int>(int.Parse).ToArray();
                    return new Rectangle(rect[0], rect[1], rect[2], rect[3]);
                }
            }
            set
            {
                _cco.PageModePrintArea = string.Join(",", new[] { value.X, value.Y, value.Width, value.Height });
                VerifyResult(_cco.ResultCode);
            }
        }

        public override PageModePrintDirection PageModePrintDirection
        {
            get
            {
                return (PageModePrintDirection)InteropEnum<PageModePrintDirection>.ToEnumFromInteger(_cco.PageModePrintDirection);
            }
            set
            {
                _cco.PageModePrintDirection = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override PrinterStation PageModeStation
        {
            get
            {
                return (PrinterStation)InteropEnum<PrinterStation>.ToEnumFromInteger(_cco.PageModeStation);
            }
            set
            {
                _cco.PageModeStation = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int PageModeVerticalPosition
        {
            get
            {
                return _cco.PageModeVerticalPosition;
            }
            set
            {
                _cco.PageModeVerticalPosition = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        #endregion POSPrinter Specific Properties

        #region POSPrinter Specific Methodss

        public override void BeginInsertion(int timeout)
        {
            VerifyResult(_cco.BeginInsertion(timeout));
        }

        public override void BeginRemoval(int timeout)
        {
            VerifyResult(_cco.BeginRemoval(timeout));
        }

        public override void ChangePrintSide(PrinterSide side)
        {
            VerifyResult(_cco.ChangePrintSide((int)side));
        }

        public override void ClearPrintArea()
        {
            VerifyResult(_cco.ClearPrintArea());
        }

        public override void CutPaper(int percentage)
        {
            VerifyResult(_cco.CutPaper(percentage));
        }

        public override void DrawRuledLine(PrinterStation station, string positionList, LineDirection direction, int lineWidth, LineStyle style, int lineColor)
        {
            VerifyResult(_cco.DrawRuledLine((int)station, positionList, (int)direction, lineWidth, (int)style, lineColor));
        }

        public override void EndInsertion()
        {
            VerifyResult(_cco.EndInsertion());
        }

        public override void EndRemoval()
        {
            VerifyResult(_cco.EndRemoval());
        }

        public override void MarkFeed(PrinterMarkFeeds type)
        {
            VerifyResult(_cco.MarkFeed((int)type));
        }

        public override void PageModePrint(PageModePrintControl control)
        {
            VerifyResult(_cco.PageModePrint((int)control));
        }

        public override void PrintBarCode(PrinterStation station, string data, BarCodeSymbology symbology, int height, int width, int alignment, BarCodeTextPosition textPosition)
        {
            VerifyResult(_cco.PrintBarCode((int)station, data, (int)symbology, height, width, alignment, (int)textPosition));
        }

        public override void PrintBitmap(PrinterStation station, string fileName, int width, int alignment)
        {
            VerifyResult(_cco.PrintBitmap((int)station, fileName, width, alignment));
        }

        public override void PrintImmediate(PrinterStation station, string data)
        {
            VerifyResult(_cco.PrintImmediate((int)station, data));
        }

        public override void PrintMemoryBitmap(PrinterStation station, Bitmap data, int width, int alignment)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                data.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                string memorybitmap = InteropCommon.ToStringFromByteArray(ms.ToArray(), _binaryConversion);
                VerifyResult(_cco.PrintMemoryBitmap((int)station, memorybitmap, (int)OPOSPOSPrinterConstants.PTR_BMT_BMP, width, alignment));
            }
        }

        public override void PrintNormal(PrinterStation station, string data)
        {
            VerifyResult(_cco.PrintNormal((int)station, data));
        }

        public override void PrintTwoNormal(PrinterStation stations, string data1, string data2)
        {
            VerifyResult(_cco.PrintTwoNormal((int)stations, data1, data2));
        }

        public override void RotatePrint(PrinterStation station, PrintRotation rotation)
        {
            VerifyResult(_cco.RotatePrint((int)station, (int)rotation));
        }

        public override void SetBitmap(int bitmapNumber, PrinterStation station, string fileName, int width, int alignment)
        {
            VerifyResult(_cco.SetBitmap(bitmapNumber, (int)station, fileName, width, alignment));
        }

        public override void SetLogo(PrinterLogoLocation location, string data)
        {
            VerifyResult(_cco.SetLogo((int)location, data));
        }

        public override void TransactionPrint(PrinterStation station, PrinterTransactionControl control)
        {
            VerifyResult(_cco.TransactionPrint((int)station, (int)control));
        }

        public override void ValidateData(PrinterStation station, string data)
        {
            VerifyResult(_cco.ValidateData((int)station, data));
        }

        #endregion
    }
}
