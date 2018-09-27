
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.PointCardRW, "OpenPOS PointCardRW", "OPOS PointCardRW Alternative CCO Interop", 1, 14)]

    public class OpenPOSPointCardRW : PointCardRW, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSPointCardRW _cco = null;
        private const string _oposDeviceClass = "PointCardRW";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DataEventHandler DataEvent;
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event OutputCompleteEventHandler OutputCompleteEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSPointCardRW()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSPointCardRW()
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
                    _cco.DataEvent -= (_IOPOSPointCardRWEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSPointCardRWEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSPointCardRWEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSPointCardRWEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSPointCardRWEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        private void _cco_DataEvent(int Status)
        {
            if (this.DataEvent != null)
            {
                DataEvent(this, new DataEventArgs(Status));
            }
        }

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
                return (BinaryConversion)InteropEnum<BinaryConversion>.ToEnumFromInteger(_cco.BinaryConversion);
            }
            set
            {
                _cco.BinaryConversion = (int)value;
                VerifyResult(_cco.ResultCode);
                _binaryConversion = _cco.BinaryConversion;
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
            get { return (PowerReporting)InteropEnum<PowerReporting>.ToEnumFromInteger(_cco.CapPowerReporting); }
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

        public override int DataCount
        {
            get { return _cco.DataCount; }
        }

        public override bool DataEventEnabled
        {
            get
            {
                return _cco.DataEventEnabled;
            }
            set
            {
                _cco.DataEventEnabled = value;
                VerifyResult(_cco.ResultCode);
            }
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

        public override int OutputId
        {
            get { return _cco.OutputID; }
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
            get { return (PowerState)InteropEnum<PowerState>.ToEnumFromInteger(_cco.PowerState); }
        }

        public override ControlState State
        {
            get { return (ControlState)InteropEnum<ControlState>.ToEnumFromInteger(_cco.State); }
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
                    _cco = new POS.Devices.OPOSPointCardRW();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSPointCardRWEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSPointCardRWEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSPointCardRWEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSPointCardRWEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSPointCardRWEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSPointCardRWEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSPointCardRWEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSPointCardRWEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSPointCardRWEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSPointCardRWEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        public override void ClearInput()
        {
            VerifyResult(_cco.ClearInput());
        }

        public override void ClearInputProperties()
        {
            VerifyResult(_cco.ClearInputProperties());
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

        #region OPOSPointCardRW  Specific Properties

        public override bool CapBold
        {
            get { return _cco.CapBold; }
        }

        public override bool CapCardEntranceSensor
        {
            get { return _cco.CapCardEntranceSensor; }
        }

        public override CharacterSetCapability CapCharacterSet
        {
            get { return (CharacterSetCapability)InteropEnum<CharacterSetCapability>.ToEnumFromInteger(_cco.CapCharacterSet); }
        }

        public override bool CapCleanCard
        {
            get { return _cco.CapCleanCard; }
        }

        public override bool CapClearPrint
        {
            get { return _cco.CapClearPrint; }
        }

        public override bool CapDHigh
        {
            get { return _cco.CapDhigh; }
        }

        public override bool CapDWide
        {
            get { return _cco.CapDwide; }
        }

        public override bool CapDWideDHigh
        {
            get { return _cco.CapDwideDhigh; }
        }

        public override bool CapItalic
        {
            get { return _cco.CapItalic; }
        }

        public override bool CapLeft90
        {
            get { return _cco.CapLeft90; }
        }

        public override bool CapMapCharacterSet
        {
            get { return _cco.CapMapCharacterSet; }
        }

        public override bool CapPrint
        {
            get { return _cco.CapPrint; }
        }

        public override bool CapPrintMode
        {
            get { return _cco.CapPrintMode; }
        }

        public override bool CapRight90
        {
            get { return _cco.CapRight90; }
        }

        public override bool CapRotate180
        {
            get { return _cco.CapRotate180; }
        }

        public override PointCardRWTracks CapTracksToRead
        {
            get { return (PointCardRWTracks)InteropEnum<PointCardRWTracks>.ToEnumFromInteger(_cco.CapTracksToRead); }
        }

        public override PointCardRWTracks CapTracksToWrite
        {
            get { return (PointCardRWTracks)InteropEnum<PointCardRWTracks>.ToEnumFromInteger(_cco.CapTracksToWrite); }
        }

        public override PointCardState CardState
        {
            get { return (PointCardState)InteropEnum<PointCardState>.ToEnumFromInteger(_cco.CardState); }
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

        public override string[] FontTypefaceList
        {
            get { return _cco.FontTypeFaceList.Split(','); }
        }

        public override int LineChars
        {
            get
            {
                return _cco.LineChars;
            }
            set
            {
                _cco.LineChars = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int[] LineCharsList
        {
            get { return InteropCommon.ToIntegerArray(_cco.LineCharsList, ','); }
        }

        public override int LineHeight
        {
            get
            {
                return _cco.LineHeight;
            }
            set
            {
                _cco.LineHeight = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int LineSpacing
        {
            get
            {
                return _cco.LineSpacing;
            }
            set
            {
                _cco.LineSpacing = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int LineWidth
        {
            get { return _cco.LineWidth; }
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

        public override int MaxLine
        {
            get { return _cco.MaxLine; }
        }

        public override int PrintHeight
        {
            get { return _cco.PrintHeight; }
        }

        public override PointCardReadWriteStates ReadState
        {
            get
            {
                int iState1 = _cco.ReadState1;
                int iState2 = _cco.ReadState2;
                PointCardReadWriteState eTrack1 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger((iState1 & 0xFF));
                PointCardReadWriteState eTrack2 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger(((iState1 & 0xFF00) / 0x100));
                PointCardReadWriteState eTrack3 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger(((iState1 & 0xFF0000) / 0x10000));
                PointCardReadWriteState eTrack4 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger((int)(((uint)iState1 & 0xFF000000) / 0x1000000));
                PointCardReadWriteState eTrack5 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger((iState2 & 0xFF));
                PointCardReadWriteState eTrack6 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger(((iState2 & 0xFF00)));
                return new PointCardReadWriteStates(eTrack1, eTrack2, eTrack3, eTrack4, eTrack5, eTrack6);
            }
        }

        public override PointCardReceiveLengths RecvLength
        {
            get
            {
                int iLength1 = _cco.RecvLength1;
                int iLength2 = _cco.RecvLength2;
                int iTrack1 = iLength1 & 0xFF;
                int iTrack2 = (iLength1 & 0xFF00) / 0x100;
                int iTrack3 = (iLength1 & 0xFF0000) / 0x10000;
                int iTrack4 = (int)(((uint)iLength1 & 0xFF000000) / 0x1000000);
                int iTrack5 = iLength2 & 0xFF;
                int iTrack6 = (iLength2 & 0xFF00) / 0x100;
                return new PointCardReceiveLengths(iTrack1, iTrack2, iTrack3, iTrack4, iTrack5, iTrack6);
            }
        }

        public override int SidewaysMaxChars
        {
            get { return _cco.SidewaysMaxChars; }
        }

        public override int SidewaysMaxLines
        {
            get { return _cco.SidewaysMaxLines; }
        }

        public override PointCardRWTracks TracksToRead
        {
            get
            {
                return (PointCardRWTracks)InteropEnum<PointCardRWTracks>.ToEnumFromInteger(_cco.TracksToRead);
            }
            set
            {
                _cco.TracksToRead = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override PointCardRWTracks TracksToWrite
        {
            get
            {
                return (PointCardRWTracks)InteropEnum<PointCardRWTracks>.ToEnumFromInteger(_cco.TracksToWrite);
            }
            set
            {
                _cco.TracksToWrite = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Track1Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track1Data, _binaryConversion); }
        }

        public override byte[] Track2Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track2Data, _binaryConversion); }
        }

        public override byte[] Track3Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track3Data, _binaryConversion); }
        }

        public override byte[] Track4Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track4Data, _binaryConversion); }
        }

        public override byte[] Track5Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track5Data, _binaryConversion); }
        }

        public override byte[] Track6Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track6Data, _binaryConversion); }
        }

        public override PointCardReadWriteStates WriteState
        {
            get
            {
                int iState1 = _cco.WriteState1;
                int iState2 = _cco.WriteState2;
                PointCardReadWriteState eTrack1 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger((iState1 & 0xFF));
                PointCardReadWriteState eTrack2 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger(((iState1 & 0xFF00) / 0x100));
                PointCardReadWriteState eTrack3 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger(((iState1 & 0xFF0000) / 0x10000));
                PointCardReadWriteState eTrack4 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger((int)(((uint)iState1 & 0xFF000000) / 0x1000000));
                PointCardReadWriteState eTrack5 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger((iState2 & 0xFF));
                PointCardReadWriteState eTrack6 = (PointCardReadWriteState)InteropEnum<PointCardReadWriteState>.ToEnumFromInteger(((iState2 & 0xFF00)));
                return new PointCardReadWriteStates(eTrack1, eTrack2, eTrack3, eTrack4, eTrack5, eTrack6);
            }
        }

        public override byte[] Write1Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Write1Data, _binaryConversion);
            }
            set
            {
                _cco.Write1Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Write2Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Write2Data, _binaryConversion);
            }
            set
            {
                _cco.Write2Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Write3Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Write3Data, _binaryConversion);
            }
            set
            {
                _cco.Write3Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Write4Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Write4Data, _binaryConversion);
            }
            set
            {
                _cco.Write4Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Write5Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Write5Data, _binaryConversion);
            }
            set
            {
                _cco.Write5Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Write6Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Write6Data, _binaryConversion);
            }
            set
            {
                _cco.Write6Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        #endregion

        #region OPOSPointCardRW  Specific Methodss

        public override void BeginInsertion(int timeout)
        {
            VerifyResult(_cco.BeginInsertion(timeout));
        }

        public override void EndInsertion()
        {
            VerifyResult(_cco.EndInsertion());
        }

        public override void BeginRemoval(int timeout)
        {
            VerifyResult(_cco.BeginRemoval(timeout));
        }

        public override void EndRemoval()
        {
            VerifyResult(_cco.EndRemoval());
        }

        public override void CleanCard()
        {
            VerifyResult(_cco.CleanCard());
        }

        public override void ClearPrintWrite(PointCardAreas kind, int horizontalPosition, int verticalPosition, int width, int height)
        {
            VerifyResult(_cco.ClearPrintWrite((int)kind, horizontalPosition, verticalPosition, width, height));
        }

        public override void PrintWrite(PointCardAreas kind, int horizontalPosition, int verticalPosition, string data)
        {
            VerifyResult(_cco.PrintWrite((int)kind, horizontalPosition, verticalPosition, data));
        }

        public override void RotatePrint(Rotation rotation)
        {
            VerifyResult(_cco.RotatePrint((int)rotation));
        }

        public override void ValidateData(string data)
        {
            VerifyResult(_cco.ValidateData(data));
        }

        #endregion
    }
}
