
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.RFIDScanner, "OpenPOS RFIDScanner", "OPOS RFIDScanner Alternative CCO Interop", 1, 14)]

    public class OpenPOSRFIDScanner : RFIDScanner, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSRFIDScanner _cco = null;
        private const string _oposDeviceClass = "RFIDScanner";
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
        public OpenPOSRFIDScanner()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSRFIDScanner()
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
                    _cco.DataEvent -= (_IOPOSRFIDScannerEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSRFIDScannerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSRFIDScannerEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSRFIDScannerEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSRFIDScannerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        public override bool AutoDisable
        {
            get
            {
                return _cco.AutoDisable;
            }
            set
            {
                _cco.AutoDisable = value;
                VerifyResult(_cco.ResultCode);
            }
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
                    _cco = new POS.Devices.OPOSRFIDScanner();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSRFIDScannerEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSRFIDScannerEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSRFIDScannerEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSRFIDScannerEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSRFIDScannerEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSRFIDScannerEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSRFIDScannerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSRFIDScannerEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSRFIDScannerEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSRFIDScannerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSRFIDScanner  Specific Properties

        public override bool CapContinuousRead
        {
            get { return _cco.CapContinuousRead; }
        }

        public override bool CapDisableTag
        {
            get { return _cco.CapDisableTag; }
        }

        public override bool CapLockTag
        {
            get { return _cco.CapLockTag; }
        }

        public override RFIDProtocols CapMultipleProtocols
        {
            get { return (RFIDProtocols)InteropEnum<RFIDProtocols>.ToEnumFromInteger(_cco.CapMultipleProtocols); }
        }

        public override bool CapReadTimer
        {
            get { return _cco.CapReadTimer; }
        }

        public override WriteTagSections CapWriteTag
        {
            get { return (WriteTagSections)InteropEnum<WriteTagSections>.ToEnumFromInteger(_cco.CapWriteTag); }
        }

        public override bool ContinuousReadMode
        {
            get { return _cco.ContinuousReadMode; }
        }

        public override byte[] CurrentTagId
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.CurrentTagID, _binaryConversion); }
        }

        public override RFIDProtocols CurrentTagProtocol
        {
            get { return (RFIDProtocols)InteropEnum<RFIDProtocols>.ToEnumFromInteger(_cco.CurrentTagProtocol); }
        }

        public override byte[] CurrentTagUserData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.CurrentTagUserData, _binaryConversion); }
        }

        public override RFIDProtocols ProtocolMask
        {
            get
            {
                return (RFIDProtocols)InteropEnum<RFIDProtocols>.ToEnumFromInteger(_cco.ProtocolMask);
            }
            set
            {
                _cco.ProtocolMask = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int ReadTimerInterval
        {
            get
            {
                return _cco.ReadTimerInterval;
            }
            set
            {
                _cco.ReadTimerInterval = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int TagCount
        {
            get { return _cco.TagCount; }
        }

        #endregion

        #region OPOSRFIDScanner  Specific Methodss

        public override void FirstTag()
        {
            VerifyResult(_cco.FirstTag());
        }

        public override void NextTag()
        {
            VerifyResult(_cco.NextTag());
        }

        public override void PreviousTag()
        {
            VerifyResult(_cco.PreviousTag());
        }

        public override void DisableTag(byte[] tagId, int timeout, byte[] password)
        {
            string sTagID = InteropCommon.ToStringFromByteArray(tagId, _binaryConversion);
            string sPassword = InteropCommon.ToStringFromByteArray(password, _binaryConversion);
            VerifyResult(_cco.DisableTag(sTagID, timeout, sPassword));
        }

        public override void LockTag(byte[] tagId, int timeout, byte[] password)
        {
            string sTagID = InteropCommon.ToStringFromByteArray(tagId, _binaryConversion);
            string sPassword = InteropCommon.ToStringFromByteArray(password, _binaryConversion);
            VerifyResult(_cco.LockTag(sTagID, timeout, sPassword));
        }

        public override void ReadTags(RFIDReadOptions cmd, byte[] filterId, byte[] filterMask, int start, int length, int timeout, byte[] password)
        {
            string sFilterID = InteropCommon.ToStringFromByteArray(filterId, _binaryConversion);
            string sFilterMask = InteropCommon.ToStringFromByteArray(filterMask, _binaryConversion);
            string sPassword = InteropCommon.ToStringFromByteArray(password, _binaryConversion);
            VerifyResult(_cco.ReadTags((int)cmd, sFilterID, sFilterMask, start, length, timeout, sPassword));
        }

        public override void StartReadTags(RFIDReadOptions cmd, byte[] filterId, byte[] filterMask, int start, int length, byte[] password)
        {
            string sFilterID = InteropCommon.ToStringFromByteArray(filterId, _binaryConversion);
            string sFilterMask = InteropCommon.ToStringFromByteArray(filterMask, _binaryConversion);
            string sPassword = InteropCommon.ToStringFromByteArray(password, _binaryConversion);
            VerifyResult(_cco.StartReadTags((int)cmd, sFilterID, sFilterMask, start, length, sPassword));
        }

        public override void StopReadTags(byte[] password)
        {
            string sPassword = InteropCommon.ToStringFromByteArray(password, _binaryConversion);
            VerifyResult(_cco.StopReadTags(sPassword));
        }

        public override void WriteTagData(byte[] tagId, byte[] userData, int start, int timeout, byte[] password)
        {
            string sTagID = InteropCommon.ToStringFromByteArray(tagId, _binaryConversion);
            string sUserData = InteropCommon.ToStringFromByteArray(userData, _binaryConversion);
            string sPassword = InteropCommon.ToStringFromByteArray(password, _binaryConversion);
            VerifyResult(_cco.WriteTagData(sTagID, sUserData, start, timeout, sPassword));
        }

        public override void WriteTagId(byte[] sourceTagId, byte[] destinationTagId, int timeout, byte[] password)
        {
            string sSourceID = InteropCommon.ToStringFromByteArray(sourceTagId, _binaryConversion);
            string sDestID = InteropCommon.ToStringFromByteArray(destinationTagId, _binaryConversion);
            string sPassword = InteropCommon.ToStringFromByteArray(password, _binaryConversion);
            VerifyResult(_cco.WriteTagID(sSourceID, sDestID, timeout, sPassword));
        }

        #endregion
    }
}
