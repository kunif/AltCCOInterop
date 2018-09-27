
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.ElectronicValueRW, "OpenPOS ElectronicValueRW", "OPOS ElectronicValueRW Alternative CCO Interop", 1, 14)]

    public class OpenPOSElectronicValueRW : ElectronicValueRW, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSElectronicValueRW _cco = null;
        private const string _oposDeviceClass = "ElectronicValueRW";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DataEventHandler DataEvent;
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event OutputCompleteEventHandler OutputCompleteEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        public override event TransitionEventHandler TransitionEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSElectronicValueRW()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSElectronicValueRW()
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
                    _cco.DataEvent -= (_IOPOSElectronicValueRWEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSElectronicValueRWEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSElectronicValueRWEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSElectronicValueRWEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSElectronicValueRWEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
                    _cco.TransitionEvent -= (_IOPOSElectronicValueRWEvents_TransitionEventEventHandler)_cco_TransitionEvent;
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

        private void _cco_TransitionEvent(int EventNumber, ref int pData, ref string pString)
        {
            if (this.TransitionEvent != null)
            {
                TransitionEventArgs eTE = new TransitionEventArgs(EventNumber, pData, pString);
                TransitionEvent(new object(), eTE);
                pData = eTE.Data;
                pString = Convert.ToString(eTE.String);
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

        public override int OutputId
        {
            get { return _cco.OutputID; }
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
            get { return InteropCommon.ToVersion(_cco.ServiceObjectVersion); }
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
                    _cco = new POS.Devices.OPOSElectronicValueRW();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSElectronicValueRWEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSElectronicValueRWEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSElectronicValueRWEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSElectronicValueRWEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSElectronicValueRWEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
                    _cco.TransitionEvent += new _IOPOSElectronicValueRWEvents_TransitionEventEventHandler(_cco_TransitionEvent);
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

            _cco.DataEvent -= (_IOPOSElectronicValueRWEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSElectronicValueRWEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSElectronicValueRWEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSElectronicValueRWEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSElectronicValueRWEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
            _cco.TransitionEvent -= (_IOPOSElectronicValueRWEvents_TransitionEventEventHandler)_cco_TransitionEvent;
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

        #region ElectronicValueRW Specific Properties

        public override string AccountNumber
        {
            get { return _cco.AccountNumber; }
        }

        public override string AdditionalSecurityInformation
        {
            get
            {
                return _cco.AdditionalSecurityInformation;
            }
            set
            {
                _cco.AdditionalSecurityInformation = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override decimal Amount
        {
            get
            {
                return _cco.Amount;
            }
            set
            {
                _cco.Amount = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string ApprovalCode
        {
            get
            {
                return _cco.ApprovalCode;
            }
            set
            {
                _cco.ApprovalCode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

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

        public override decimal Balance
        {
            get { return _cco.Balance; }
        }

        public override decimal BalanceOfPoint
        {
            get { return _cco.BalanceOfPoint; }
        }

        public override bool CapActivateService
        {
            get { return _cco.CapActivateService; }
        }

        public override bool CapAddValue
        {
            get { return _cco.CapAddValue; }
        }

        public override bool CapCancelValue
        {
            get { return _cco.CapCancelValue; }
        }

        public override CardDetectionTypes CapCardSensor
        {
            get { return (CardDetectionTypes)InteropEnum<CardDetectionTypes>.ToEnumFromInteger(_cco.CapCardSensor); }
        }

        public override CardDetectionControl CapDetectionControl
        {
            get { return (CardDetectionControl)InteropEnum<CardDetectionControl>.ToEnumFromInteger(_cco.CapDetectionControl); }
        }

        public override bool CapElectronicMoney
        {
            get { return _cco.CapElectronicMoney; }
        }

        public override bool CapEnumerateCardServices
        {
            get { return _cco.CapEnumerateCardServices; }
        }

        public override bool CapIndirectTransactionLog
        {
            get { return _cco.CapIndirectTransactionLog; }
        }

        public override bool CapLockTerminal
        {
            get { return _cco.CapLockTerminal; }
        }

        public override bool CapLogStatus
        {
            get { return _cco.CapLogStatus; }
        }

        public override bool CapMediumId
        {
            get { return _cco.CapMediumID; }
        }

        public override bool CapMembershipCertificate
        {
            get { return _cco.CapMembershipCertificate; }
        }

        public override bool CapPINDevice
        {
            get { return _cco.CapPINDevice; }
        }

        public override bool CapPoint
        {
            get { return _cco.CapPoint; }
        }

        public override bool CapSubtractValue
        {
            get { return _cco.CapSubtractValue; }
        }

        public override bool CapTrainingMode
        {
            get { return _cco.CapTrainingMode; }
        }

        public override bool CapTransaction
        {
            get { return _cco.CapTransaction; }
        }

        public override bool CapTransactionLog
        {
            get { return _cco.CapTransactionLog; }
        }

        public override bool CapUnlockTerminal
        {
            get { return _cco.CapUnlockTerminal; }
        }

        public override bool CapUpdateKey
        {
            get { return _cco.CapUpdateKey; }
        }

        public override bool CapVoucher
        {
            get { return _cco.CapVoucher; }
        }

        public override bool CapWriteValue
        {
            get { return _cco.CapWriteValue; }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<string> CardServiceList
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<string>(_cco.CardServiceList.Split(',')); }
        }

        public override string CurrentService
        {
            get
            {
                return _cco.CurrentService;
            }
            set
            {
                _cco.CurrentService = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool DetectionControl
        {
            get
            {
                return _cco.DetectionControl;
            }
            set
            {
                _cco.DetectionControl = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override DetectionState DetectionStatus
        {
            get { return (DetectionState)InteropEnum<DetectionState>.ToEnumFromInteger(_cco.DetectionStatus); }
        }

        public override DateTime ExpirationDate
        {
            get { return DateTime.ParseExact(_cco.ExpirationDate, "yyyyMMDD", null); }
        }

        public override DateTime LastUsedDate
        {
            get { return DateTime.ParseExact(_cco.LastUsedDate, "yyyyMMDDHHmmss", null); }
        }

        public override LogState LogStatus
        {
            get { return (LogState)InteropEnum<LogState>.ToEnumFromInteger(_cco.LogStatus); }
        }

        public override string MediumId
        {
            get
            {
                return _cco.MediumID;
            }
            set
            {
                _cco.MediumID = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override PinEntryType PINEntry
        {
            get
            {
                return (PinEntryType)InteropEnum<PinEntryType>.ToEnumFromInteger(_cco.PINEntry);
            }
            set
            {
                _cco.PINEntry = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override decimal Point
        {
            get
            {
                return _cco.Point;
            }
            set
            {
                _cco.Point = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<string> ReaderWriterServiceList
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<string>(_cco.ReaderWriterServiceList.Split(',')); }
        }

        public override int SequenceNumber
        {
            get { return _cco.SequenceNumber; }
        }

        public override ServiceType ServiceType
        {
            get { return (ServiceType)InteropEnum<ServiceType>.ToEnumFromInteger(_cco.ServiceType); }
        }

        public override decimal SettledAmount
        {
            get { return _cco.SettledAmount; }
        }

        public override decimal SettledPoint
        {
            get { return _cco.SettledPoint; }
        }

        public override TrainingModeState TrainingModeState
        {
            get
            {
                return (TrainingModeState)InteropEnum<TrainingModeState>.ToEnumFromInteger(_cco.TrainingModeState);
            }
            set
            {
                _cco.TrainingModeState = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string TransactionLog
        {
            get { return _cco.TransactionLog; }
        }

        public override string VoucherId
        {
            get
            {
                return _cco.VoucherID;
            }
            set
            {
                _cco.VoucherID = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string VoucherIdList
        {
            get
            {
                return _cco.VoucherIDList;
            }
            set
            {
                _cco.VoucherIDList = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        #endregion

        #region ElectronicValueRW Specific Methodss

        public override EVRWResult AccessData(AccessDataType dataType, int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.AccessData((int)dataType, ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override void AccessLog(int sequenceNumber, TransactionLogType type, int timeout)
        {
            VerifyResult(_cco.AccessLog(sequenceNumber, (int)type, timeout));
        }

        public override EVRWResult ActivateEVService(int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.ActivateEVService(ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override EVRWResult ActivateService(int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.ActivateService(ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override void AddValue(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.AddValue(sequenceNumber, timeout));
        }

        public override void BeginDetection(BeginDetectionType type, int timeout)
        {
            VerifyResult(_cco.BeginDetection((int)type, timeout));
        }

        public override void BeginRemoval(int timeout)
        {
            VerifyResult(_cco.BeginRemoval(timeout));
        }

        public override void CancelValue(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.CancelValue(sequenceNumber, timeout));
        }

        public override void CaptureCard()
        {
            VerifyResult(_cco.CaptureCard());
        }

        public override void CheckServiceRegistrationToMedium(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.CheckServiceRegistrationToMedium(sequenceNumber, timeout));
        }

        public override void ClearParameterInformation()
        {
            VerifyResult(_cco.ClearParameterInformation());
        }

        public override EVRWResult CloseDailyEVService(int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.CloseDailyEVService(ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override EVRWResult DeactivateEVService(int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.DeactivateEVService(ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override void EndDetection()
        {
            VerifyResult(_cco.EndDetection());
        }

        public override void EndRemoval()
        {
            VerifyResult(_cco.EndRemoval());
        }

        public override void EnumerateCardServices()
        {
            VerifyResult(_cco.EnumerateCardServices());
        }

        public override void LockTerminal()
        {
            VerifyResult(_cco.LockTerminal());
        }

        public override EVRWResult OpenDailyEVService(int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.OpenDailyEVService(ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override void QueryLastSuccessfulTransactionResult()
        {
            VerifyResult(_cco.QueryLastSuccessfulTransactionResult());
        }

        public override void ReadValue(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.ReadValue(sequenceNumber, timeout));
        }

        public override void RegisterServiceToMedium(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.RegisterServiceToMedium(sequenceNumber, timeout));
        }

        public override EVRWResultInformation RetrieveResultInformation(string name, string value)
        {
            VerifyResult(_cco.RetrieveResultInformation(name, ref value));
            return new EVRWResultInformation(name, value);
        }

        public override void SetParameterInformation(string name, string value)
        {
            VerifyResult(_cco.SetParameterInformation(name, value));
        }

        public override void SubtractValue(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.SubtractValue(sequenceNumber, timeout));
        }

        public override void TransactionAccess(TransactionControl control)
        {
            VerifyResult(_cco.TransactionAccess((int)control));
        }

        public override void UnlockTerminal()
        {
            VerifyResult(_cco.UnlockTerminal());
        }

        public override void UnregisterServiceToMedium(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.UnregisterServiceToMedium(sequenceNumber, timeout));
        }

        public override EVRWResult UpdateData(AccessDataType dataType, int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.UpdateData((int)dataType, ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override EVRWResult UpdateKey(int data, object obj)
        {
            string sObj = Convert.ToString(obj);
            VerifyResult(_cco.UpdateKey(ref data, ref sObj));
            return new EVRWResult(data, sObj);
        }

        public override void WriteValue(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.WriteValue(sequenceNumber, timeout));
        }

        #endregion
    }
}
