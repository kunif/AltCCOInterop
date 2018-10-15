
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.Cat, "OpenPOS CAT", "OPOS CAT Alternative CCO Interop", 1, 14)]

    public class OpenPOSCAT : Cat, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSCAT _cco = null;
        private const string _oposDeviceClass = "CAT";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event OutputCompleteEventHandler OutputCompleteEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSCAT()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSCAT()
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
                    _cco.DirectIOEvent -= (_IOPOSCATEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSCATEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSCATEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSCATEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSCAT();

                    // Register event handler
                    _cco.DirectIOEvent += new _IOPOSCATEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSCATEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSCATEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSCATEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DirectIOEvent -= (_IOPOSCATEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSCATEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSCATEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSCATEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region CAT Specific Properties

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

        public override string ApprovalCode
        {
            get { return _cco.ApprovalCode; }
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

        public override bool CapAdditionalSecurityInformation
        {
            get { return _cco.CapAdditionalSecurityInformation; }
        }

        public override bool CapAuthorizeCompletion
        {
            get { return _cco.CapAuthorizeCompletion; }
        }

        public override bool CapAuthorizePreSales
        {
            get { return _cco.CapAuthorizePreSales; }
        }

        public override bool CapAuthorizeRefund
        {
            get { return _cco.CapAuthorizeRefund; }
        }

        public override bool CapAuthorizeVoid
        {
            get { return _cco.CapAuthorizeVoid; }
        }

        public override bool CapAuthorizeVoidPreSales
        {
            get { return _cco.CapAuthorizeVoidPreSales; }
        }

        public override bool CapCashDeposit
        {
            get { return _cco.CapCashDeposit; }
        }

        public override bool CapCenterResultCode
        {
            get { return _cco.CapCenterResultCode; }
        }

        public override bool CapCheckCard
        {
            get { return _cco.CapCheckCard; }
        }

        public override CatLogs CapDailyLog
        {
            get { return (CatLogs)InteropEnum<CatLogs>.ToEnumFromInteger(_cco.CapDailyLog); }
        }

        public override bool CapInstallments
        {
            get { return _cco.CapInstallments; }
        }

        public override bool CapLockTerminal
        {
            get { return _cco.CapLockTerminal; }
        }

        public override bool CapLogStatus
        {
            get { return _cco.CapLogStatus; }
        }

        public override bool CapPaymentDetail
        {
            get { return _cco.CapPaymentDetail; }
        }

        public override bool CapTaxOthers
        {
            get { return _cco.CapTaxOthers; }
        }

        public override bool CapTrainingMode
        {
            get { return _cco.CapTrainingMode; }
        }

        public override bool CapTransactionNumber
        {
            get { return _cco.CapTransactionNumber; }
        }

        public override bool CapUnlockTerminal
        {
            get { return _cco.CapUnlockTerminal; }
        }

        public override string CardCompanyId
        {
            get { return _cco.CardCompanyID; }
        }

        public override string CenterResultCode
        {
            get { return _cco.CenterResultCode; }
        }

        public override string DailyLog
        {
            get { return _cco.DailyLog; }
        }

        public override DealingLogStatus LogStatus
        {
            get { return (DealingLogStatus)InteropEnum<DealingLogStatus>.ToEnumFromInteger(_cco.LogStatus); }
        }

        public override PaymentCondition PaymentCondition
        {
            get { return (PaymentCondition)InteropEnum<PaymentCondition>.ToEnumFromInteger(_cco.PaymentCondition); }
        }

        public override string PaymentDetail
        {
            get { return _cco.PaymentDetail; }
        }

        public override PaymentMedia PaymentMedia
        {
            get
            {
                return (PaymentMedia)InteropEnum<PaymentMedia>.ToEnumFromInteger(_cco.PaymentMedia);
            }
            set
            {
                _cco.PaymentMedia = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int SequenceNumber
        {
            get { return _cco.SequenceNumber; }
        }

        public override decimal SettledAmount
        {
            get { return _cco.SettledAmount; }
        }

        public override string SlipNumber
        {
            get { return _cco.SlipNumber; }
        }

        public override bool TrainingMode
        {
            get
            {
                return _cco.TrainingMode;
            }
            set
            {
                _cco.TrainingMode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string TransactionNumber
        {
            get { return _cco.TransactionNumber; }
        }

        public override CreditTransactionType TransactionType
        {
            get { return (CreditTransactionType)InteropEnum<CreditTransactionType>.ToEnumFromInteger(_cco.TransactionType); }
        }

        #endregion

        #region CAT Specific Methodss

        public override void AccessDailyLog(int sequenceNumber, CatLogs type, int timeout)
        {
            VerifyResult(_cco.AccessDailyLog(sequenceNumber, (int)type, timeout));
        }

        public override void AuthorizeCompletion(int sequenceNumber, decimal amount, decimal taxOthers, int timeout)
        {
            VerifyResult(_cco.AuthorizeCompletion(sequenceNumber, amount, taxOthers, timeout));
        }

        public override void AuthorizePreSales(int sequenceNumber, decimal amount, decimal taxOthers, int timeout)
        {
            VerifyResult(_cco.AuthorizePreSales(sequenceNumber, amount, taxOthers, timeout));
        }

        public override void AuthorizeRefund(int sequenceNumber, decimal amount, decimal taxOthers, int timeout)
        {
            VerifyResult(_cco.AuthorizeRefund(sequenceNumber, amount, taxOthers, timeout));
        }

        public override void AuthorizeSales(int sequenceNumber, decimal amount, decimal taxOthers, int timeout)
        {
            VerifyResult(_cco.AuthorizeSales(sequenceNumber, amount, taxOthers, timeout));
        }

        public override void AuthorizeVoid(int sequenceNumber, decimal amount, decimal taxOthers, int timeout)
        {
            VerifyResult(_cco.AuthorizeVoid(sequenceNumber, amount, taxOthers, timeout));
        }

        public override void AuthorizeVoidPreSales(int sequenceNumber, decimal amount, decimal taxOthers, int timeout)
        {
            VerifyResult(_cco.AuthorizeVoidPreSales(sequenceNumber, amount, taxOthers, timeout));
        }

        public override void CashDeposit(int sequenceNumber, decimal amount, int timeout)
        {
            VerifyResult(_cco.CashDeposit(sequenceNumber, amount, timeout));
        }

        public override void CheckCard(int sequenceNumber, int timeout)
        {
            VerifyResult(_cco.CheckCard(sequenceNumber, timeout));
        }

        public override void LockTerminal()
        {
            VerifyResult(_cco.LockTerminal());
        }

        public override void UnlockTerminal()
        {
            VerifyResult(_cco.UnlockTerminal());
        }

        #endregion
    }
}
