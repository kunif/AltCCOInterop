
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.ElectronicJournal, "OpenPOS ElectronicJournal", "OPOS ElectronicJournal Alternative CCO Interop", 1, 14)]

    public class OpenPOSElectronicJournal : ElectronicJournal, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSElectronicJournal _cco = null;
        private const string _oposDeviceClass = "ElectronicJournal";
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
        public OpenPOSElectronicJournal()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSElectronicJournal()
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
                    _cco.DataEvent -= (_IOPOSElectronicJournalEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSElectronicJournalEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSElectronicJournalEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSElectronicJournalEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSElectronicJournalEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSElectronicJournal();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSElectronicJournalEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSElectronicJournalEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSElectronicJournalEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSElectronicJournalEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSElectronicJournalEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSElectronicJournalEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSElectronicJournalEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSElectronicJournalEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSElectronicJournalEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSElectronicJournalEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSElectronicJournal  Specific Properties

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

        public override bool CapAddMarker
        {
            get { return _cco.CapAddMarker; }
        }

        public override bool CapErasableMedium
        {
            get { return _cco.CapErasableMedium; }
        }

        public override bool CapInitializeMedium
        {
            get { return _cco.CapInitializeMedium; }
        }

        public override bool CapMediumIsAvailable
        {
            get { return _cco.CapMediumIsAvailable; }
        }

        public override bool CapPrintContent
        {
            get { return _cco.CapPrintContent; }
        }

        public override bool CapPrintContentFile
        {
            get { return _cco.CapPrintContentFile; }
        }

        public override bool CapRetrieveCurrentMarker
        {
            get { return _cco.CapRetrieveCurrentMarker; }
        }

        public override bool CapRetrieveMarker
        {
            get { return _cco.CapRetrieveMarker; }
        }

        public override bool CapRetrieveMarkerByDateTime
        {
            get { return _cco.CapRetrieveMarkerByDateTime; }
        }

        public override bool CapRetrieveMarkersDateTime
        {
            get { return _cco.CapRetrieveMarkersDateTime; }
        }

        public override ElectronicJournalStations CapStation
        {
            get { return (ElectronicJournalStations)InteropEnum<ElectronicJournalStations>.ToEnumFromInteger(_cco.CapStation); }
        }

        public override bool CapStorageEnabled
        {
            get { return _cco.CapStorageEnabled; }
        }

        public override bool CapSuspendPrintContent
        {
            get { return _cco.CapSuspendPrintContent; }
        }

        public override bool CapSuspendQueryContent
        {
            get { return _cco.CapSuspendQueryContent; }
        }

        public override bool CapWatermark
        {
            get { return _cco.CapWaterMark; }
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

        public override decimal MediumFreeSpace
        {
            get { return _cco.MediumFreeSpace; }
        }

        public override string MediumId
        {
            get { return _cco.MediumID; }
        }

        public override bool MediumIsAvailable
        {
            get { return _cco.MediumIsAvailable; }
        }

        public override decimal MediumSize
        {
            get { return _cco.MediumSize; }
        }

        public override ElectronicJournalStations Station
        {
            get
            {
                return (ElectronicJournalStations)InteropEnum<ElectronicJournalStations>.ToEnumFromInteger(_cco.Station);
            }
            set
            {
                _cco.Station = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool StorageEnabled
        {
            get
            {
                return _cco.StorageEnabled;
            }
            set
            {
                _cco.StorageEnabled = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool Suspended
        {
            get { return _cco.Suspended; }
        }

        public override bool Watermark
        {
            get
            {
                return _cco.WaterMark;
            }
            set
            {
                _cco.WaterMark = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        #endregion

        #region OPOSElectronicJournal  Specific Methodss

        public override void AddMarker(string marker)
        {
            VerifyResult(_cco.AddMarker(marker));
        }

        public override void CancelPrintContent()
        {
            VerifyResult(_cco.CancelPrintContent());
        }

        public override void CancelQueryContent()
        {
            VerifyResult(_cco.CancelQueryContent());
        }

        public override void EraseMedium()
        {
            VerifyResult(_cco.EraseMedium());
        }

        public override void InitializeMedium(string mediumId)
        {
            VerifyResult(_cco.InitializeMedium(mediumId));
        }

        public override void PrintContent(string fromMarker, string toMarker)
        {
            VerifyResult(_cco.PrintContent(fromMarker, toMarker));
        }

        public override void PrintContentFile(string fileName)
        {
            VerifyResult(_cco.PrintContentFile(fileName));
        }

        public override void QueryContent(string fileName, string fromMarker, string toMarker)
        {
            VerifyResult(_cco.QueryContent(fileName, fromMarker, toMarker));
        }

        public override void ResumePrintContent()
        {
            VerifyResult(_cco.ResumePrintContent());
        }

        public override void ResumeQueryContent()
        {
            VerifyResult(_cco.ResumeQueryContent());
        }

        public override string RetrieveCurrentMarker(MarkerType markerType)
        {
            var result = "";
            VerifyResult(_cco.RetrieveCurrentMarker((int)markerType, out result));
            return result.ToString();
        }

        public override string RetrieveMarker(MarkerType markerType, int sessionNumber, int documentNumber)
        {
            var result = "";
            VerifyResult(_cco.RetrieveMarker((int)markerType, sessionNumber, documentNumber, out result));
            return result.ToString();
        }

        public override string RetrieveMarkerByDateTime(MarkerType markerType, string dateTime, string markerNumber)
        {
            var result = "";
            VerifyResult(_cco.RetrieveMarkerByDateTime((int)markerType, dateTime, markerNumber, out result));
            return result.ToString();
        }

        public override string RetrieveMarkersDateTime(string marker)
        {
            var result = "";
            VerifyResult(_cco.RetrieveMarkersDateTime(marker, out result));
            return result.ToString();
        }

        public override void SuspendPrintContent()
        {
            VerifyResult(_cco.SuspendPrintContent());
        }

        public override void SuspendQueryContent()
        {
            VerifyResult(_cco.SuspendQueryContent());
        }

        #endregion
    }
}
