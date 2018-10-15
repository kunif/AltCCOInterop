
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.FiscalPrinter, "OpenPOS FiscalPrinter", "OPOS FiscalPrinter Alternative CCO Interop", 1, 14)]

    public class OpenPOSFiscalPrinter : FiscalPrinter, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSFiscalPrinter _cco = null;
        private const string _oposDeviceClass = "FiscalPrinter";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event OutputCompleteEventHandler OutputCompleteEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSFiscalPrinter()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSFiscalPrinter()
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
                    _cco.DirectIOEvent -= (_IOPOSFiscalPrinterEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSFiscalPrinterEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSFiscalPrinterEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSFiscalPrinterEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSFiscalPrinter();

                    // Register event handler
                    _cco.DirectIOEvent += new _IOPOSFiscalPrinterEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSFiscalPrinterEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSFiscalPrinterEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSFiscalPrinterEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DirectIOEvent -= (_IOPOSFiscalPrinterEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSFiscalPrinterEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSFiscalPrinterEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSFiscalPrinterEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region FiscalPrinter Specific Properties

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

        public override bool CapCoverSensor
        {
            get { return _cco.CapCoverSensor; }
        }

        public override bool CapJrnEmptySensor
        {
            get { return _cco.CapJrnEmptySensor; }
        }

        public override bool CapJrnNearEndSensor
        {
            get { return _cco.CapJrnNearEndSensor; }
        }

        public override bool CapJrnPresent
        {
            get { return _cco.CapJrnPresent; }
        }

        public override bool CapRecEmptySensor
        {
            get { return _cco.CapRecEmptySensor; }
        }

        public override bool CapRecNearEndSensor
        {
            get { return _cco.CapRecNearEndSensor; }
        }

        public override bool CapRecPresent
        {
            get { return _cco.CapRecPresent; }
        }

        public override bool CapSlpEmptySensor
        {
            get { return _cco.CapSlpEmptySensor; }
        }

        public override bool CapSlpFullSlip
        {
            get { return _cco.CapSlpFullSlip; }
        }

        public override bool CapSlpNearEndSensor
        {
            get { return _cco.CapSlpNearEndSensor; }
        }

        public override bool CapSlpPresent
        {
            get { return _cco.CapSlpPresent; }
        }

        public override bool CoverOpen
        {
            get { return _cco.CoverOpen; }
        }

        public override FiscalErrorLevel ErrorLevel
        {
            get { return (FiscalErrorLevel)InteropEnum<FiscalErrorLevel>.ToEnumFromInteger(_cco.ErrorLevel); }
        }

        public override FiscalPrinterStations ErrorStation
        {
            get { return (FiscalPrinterStations)InteropEnum<FiscalPrinterStations>.ToEnumFromInteger(_cco.ErrorStation); }
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

        public override bool JrnEmpty
        {
            get { return _cco.JrnEmpty; }
        }

        public override bool JrnNearEnd
        {
            get { return _cco.JrnNearEnd; }
        }

        public override bool RecEmpty
        {
            get { return _cco.RecEmpty; }
        }

        public override bool RecNearEnd
        {
            get { return _cco.RecNearEnd; }
        }

        public override bool SlpEmpty
        {
            get { return _cco.SlpEmpty; }
        }

        public override bool SlpNearEnd
        {
            get { return _cco.SlpNearEnd; }
        }

        public override bool CapAdditionalHeader
        {
            get { return _cco.CapAdditionalHeader; }
        }

        public override bool CapAdditionalLines
        {
            get { return _cco.CapAdditionalLines; }
        }

        public override bool CapAdditionalTrailer
        {
            get { return _cco.CapAdditionalTrailer; }
        }

        public override bool CapAmountAdjustment
        {
            get { return _cco.CapAmountAdjustment; }
        }

        public override bool CapChangeDue
        {
            get { return _cco.CapChangeDue; }
        }

        public override bool CapCheckTotal
        {
            get { return _cco.CapCheckTotal; }
        }

        public override bool CapDoubleWidth
        {
            get { return _cco.CapDoubleWidth; }
        }

        public override bool CapDuplicateReceipt
        {
            get { return _cco.CapDuplicateReceipt; }
        }

        public override bool CapEmptyReceiptIsVoidable
        {
            get { return _cco.CapEmptyReceiptIsVoidable; }
        }

        public override bool CapFiscalReceiptStation
        {
            get { return _cco.CapFiscalReceiptStation; }
        }

        public override bool CapFiscalReceiptType
        {
            get { return _cco.CapFiscalReceiptType; }
        }

        public override bool CapFixedOutput
        {
            get { return _cco.CapFixedOutput; }
        }

        public override bool CapHasVatTable
        {
            get { return _cco.CapHasVatTable; }
        }

        public override bool CapIndependentHeader
        {
            get { return _cco.CapIndependentHeader; }
        }

        public override bool CapItemList
        {
            get { return _cco.CapItemList; }
        }

        public override bool CapMultiContractor
        {
            get { return _cco.CapMultiContractor; }
        }

        public override bool CapNonFiscalMode
        {
            get { return _cco.CapNonFiscalMode; }
        }

        public override bool CapOnlyVoidLastItem
        {
            get { return _cco.CapOnlyVoidLastItem; }
        }

        public override bool CapOrderAdjustmentFirst
        {
            get { return _cco.CapOrderAdjustmentFirst; }
        }

        public override bool CapPackageAdjustment
        {
            get { return _cco.CapPackageAdjustment; }
        }

        public override bool CapPercentAdjustment
        {
            get { return _cco.CapPercentAdjustment; }
        }

        public override bool CapPositiveAdjustment
        {
            get { return _cco.CapPositiveAdjustment; }
        }

        public override bool CapPositiveSubtotalAdjustment
        {
            get { return _cco.CapPositiveSubtotalAdjustment; }
        }

        public override bool CapPostPreLine
        {
            get { return _cco.CapPostPreLine; }
        }

        public override bool CapPowerLossReport
        {
            get { return _cco.CapPowerLossReport; }
        }

        public override bool CapPredefinedPaymentLines
        {
            get { return _cco.CapPredefinedPaymentLines; }
        }

        public override bool CapReceiptNotPaid
        {
            get { return _cco.CapReceiptNotPaid; }
        }

        public override bool CapRemainingFiscalMemory
        {
            get { return _cco.CapRemainingFiscalMemory; }
        }

        public override bool CapReservedWord
        {
            get { return _cco.CapReservedWord; }
        }

        public override bool CapSetCurrency
        {
            get { return _cco.CapSetCurrency; }
        }

        public override bool CapSetHeader
        {
            get { return _cco.CapSetHeader; }
        }

        public override bool CapSetPosId
        {
            get { return _cco.CapSetPOSID; }
        }

        public override bool CapSetStoreFiscalId
        {
            get { return _cco.CapSetStoreFiscalID; }
        }

        public override bool CapSetTrailer
        {
            get { return _cco.CapSetTrailer; }
        }

        public override bool CapSetVatTable
        {
            get { return _cco.CapSetVatTable; }
        }

        public override bool CapSlpFiscalDocument
        {
            get { return _cco.CapSlpFiscalDocument; }
        }

        public override bool CapSlpValidation
        {
            get { return _cco.CapSlpValidation; }
        }

        public override bool CapSubAmountAdjustment
        {
            get { return _cco.CapSubAmountAdjustment; }
        }

        public override bool CapSubPercentAdjustment
        {
            get { return _cco.CapSubPercentAdjustment; }
        }

        public override bool CapSubtotal
        {
            get { return _cco.CapSubtotal; }
        }

        public override bool CapTotalizerType
        {
            get { return _cco.CapTotalizerType; }
        }

        public override bool CapTrainingMode
        {
            get { return _cco.CapTrainingMode; }
        }

        public override bool CapValidateJournal
        {
            get { return _cco.CapValidateJournal; }
        }

        public override bool CapXReport
        {
            get { return _cco.CapXReport; }
        }

        public override FiscalCurrency ActualCurrency
        {
            get { return (FiscalCurrency)InteropEnum<FiscalCurrency>.ToEnumFromInteger(_cco.ActualCurrency); }
        }

        public override string AdditionalHeader
        {
            get
            {
                return _cco.AdditionalHeader;
            }
            set
            {
                _cco.AdditionalHeader = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string AdditionalTrailer
        {
            get
            {
                return _cco.AdditionalTrailer;
            }
            set
            {
                _cco.AdditionalTrailer = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int AmountDecimalPlaces
        {
            get { return _cco.AmountDecimalPlaces; }
        }

        public override string ChangeDue
        {
            get
            {
                return _cco.ChangeDue;
            }
            set
            {
                _cco.ChangeDue = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool CheckTotal
        {
            get
            {
                return _cco.CheckTotal;
            }
            set
            {
                _cco.CheckTotal = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override FiscalContractorId ContractorId
        {
            get
            {
                return (FiscalContractorId)InteropEnum<FiscalContractorId>.ToEnumFromInteger(_cco.ContractorId);
            }
            set
            {
                _cco.ContractorId = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override FiscalCountryCodes CountryCode
        {
            get { return (FiscalCountryCodes)InteropEnum<FiscalCountryCodes>.ToEnumFromInteger(_cco.CountryCode); }
        }

        public override FiscalDateType DateType
        {
            get
            {
                return (FiscalDateType)InteropEnum<FiscalDateType>.ToEnumFromInteger(_cco.DateType);
            }
            set
            {
                _cco.DateType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool DayOpened
        {
            get { return _cco.DayOpened; }
        }

        public override int DescriptionLength
        {
            get { return _cco.DescriptionLength; }
        }

        public override bool DuplicateReceipt
        {
            get
            {
                return _cco.DuplicateReceipt;
            }
            set
            {
                _cco.DuplicateReceipt = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int ErrorOutId
        {
            get { return _cco.ErrorOutID; }
        }

        public override FiscalPrinterState ErrorState
        {
            get { return (FiscalPrinterState)InteropEnum<FiscalPrinterState>.ToEnumFromInteger(_cco.ErrorState); }
        }

        public override FiscalReceiptStation FiscalReceiptStation
        {
            get
            {
                return (FiscalReceiptStation)InteropEnum<FiscalReceiptStation>.ToEnumFromInteger(_cco.FiscalReceiptStation);
            }
            set
            {
                _cco.FiscalReceiptStation = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override FiscalReceiptType FiscalReceiptType
        {
            get
            {
                return (FiscalReceiptType)InteropEnum<FiscalReceiptType>.ToEnumFromInteger(_cco.FiscalReceiptType);
            }
            set
            {
                _cco.FiscalReceiptType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int MessageLength
        {
            get { return _cco.MessageLength; }
        }

        public override FiscalMessageType MessageType
        {
            get
            {
                return (FiscalMessageType)InteropEnum<FiscalMessageType>.ToEnumFromInteger(_cco.MessageType);
            }
            set
            {
                _cco.MessageType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int NumHeaderLines
        {
            get { return _cco.NumHeaderLines; }
        }

        public override int NumTrailerLines
        {
            get { return _cco.NumTrailerLines; }
        }

        public override int NumVatRates
        {
            get { return _cco.NumVatRates; }
        }

        public override string PostLine
        {
            get
            {
                return _cco.PostLine;
            }
            set
            {
                _cco.PostLine = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string[] PredefinedPaymentLines
        {
            get
            {
                string strLines = _cco.PredefinedPaymentLines;
                string[] sa = new string[0];

                if (!string.IsNullOrEmpty(strLines))
                {
                    sa = strLines.Split(',');
                }

                return (string[])sa.Clone();
            }
        }

        public override string PreLine
        {
            get
            {
                return _cco.PreLine;
            }
            set
            {
                _cco.PreLine = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override FiscalPrinterState PrinterState
        {
            get { return (FiscalPrinterState)InteropEnum<FiscalPrinterState>.ToEnumFromInteger(_cco.PrinterState); }
        }

        public override int QuantityDecimalPlaces
        {
            get { return _cco.QuantityDecimalPlaces; }
        }

        public override int QuantityLength
        {
            get { return _cco.QuantityLength; }
        }

        public override int RemainingFiscalMemory
        {
            get { return _cco.RemainingFiscalMemory; }
        }

        public override string ReservedWord
        {
            get { return _cco.ReservedWord; }
        }

        public override FiscalSlipSelection SlipSelection
        {
            get
            {
                return (FiscalSlipSelection)InteropEnum<FiscalSlipSelection>.ToEnumFromInteger(_cco.SlipSelection);
            }
            set
            {
                _cco.SlipSelection = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override FiscalTotalizerType TotalizerType
        {
            get
            {
                return (FiscalTotalizerType)InteropEnum<FiscalTotalizerType>.ToEnumFromInteger(_cco.TotalizerType);
            }
            set
            {
                _cco.TotalizerType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool TrainingModeActive
        {
            get { return _cco.TrainingModeActive; }
        }

        #endregion

        #region FiscalPrinter Specific Methodss

        public override void BeginInsertion(int timeout)
        {
            VerifyResult(_cco.BeginInsertion(timeout));
        }

        public override void BeginRemoval(int timeout)
        {
            VerifyResult(_cco.BeginRemoval(timeout));
        }

        public override void EndInsertion()
        {
            VerifyResult(_cco.EndInsertion());
        }

        public override void EndRemoval()
        {
            VerifyResult(_cco.EndRemoval());
        }

        public override void BeginFiscalDocument(int documentAmount)
        {
            VerifyResult(_cco.BeginFiscalDocument(documentAmount));
        }

        public override void BeginFiscalReceipt(bool printHeader)
        {
            VerifyResult(_cco.BeginFiscalReceipt(printHeader));
        }

        public override void BeginFixedOutput(FiscalReceiptStation station, int documentType)
        {
            VerifyResult(_cco.BeginFixedOutput((int)station, documentType));
        }

        public override void BeginItemList(int vatId)
        {
            VerifyResult(_cco.BeginItemList(vatId));
        }

        public override void BeginNonFiscal()
        {
            VerifyResult(_cco.BeginNonFiscal());
        }

        public override void BeginTraining()
        {
            VerifyResult(_cco.BeginTraining());
        }

        public override void ClearError()
        {
            VerifyResult(_cco.ClearError());
        }

        public override void EndFiscalDocument()
        {
            VerifyResult(_cco.EndFiscalDocument());
        }

        public override void EndFiscalReceipt(bool printHeader)
        {
            VerifyResult(_cco.EndFiscalReceipt(printHeader));
        }

        public override void EndFixedOutput()
        {
            VerifyResult(_cco.EndFixedOutput());
        }

        public override void EndItemList()
        {
            VerifyResult(_cco.EndItemList());
        }

        public override void EndNonFiscal()
        {
            VerifyResult(_cco.EndNonFiscal());
        }

        public override void EndTraining()
        {
            VerifyResult(_cco.EndTraining());
        }

        public override FiscalDataItem GetData(FiscalData dataItem, int optArgs)
        {
            string result = "";
            VerifyResult(_cco.GetData((int)dataItem, out optArgs, out result));
            return new FiscalDataItem(result, optArgs);
        }

        public override DateTime GetDate()
        {
            var result = "";
            VerifyResult(_cco.GetDate(out result));
            return DateTime.ParseExact(result, "ddMMyyyyHHmm", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
        }

        public override string GetTotalizer(int vatId, FiscalTotalizer optArgs)
        {
            var result = "";
            VerifyResult(_cco.GetTotalizer(vatId, (int)optArgs, out result));
            return result;
        }

        public override int GetVatEntry(int vatId, int optArgs)
        {
            int result = 0;
            VerifyResult(_cco.GetVatEntry(vatId, optArgs, out result));
            return result;
        }

        public override void PrintDuplicateReceipt()
        {
            VerifyResult(_cco.PrintDuplicateReceipt());
        }

        public override void PrintFiscalDocumentLine(string documentLine)
        {
            VerifyResult(_cco.PrintFiscalDocumentLine(documentLine));
        }

        public override void PrintFixedOutput(int documentType, int lineNumber, string data)
        {
            VerifyResult(_cco.PrintFixedOutput(documentType, lineNumber, data));
        }

        public override void PrintNormal(FiscalPrinterStations station, string data)
        {
            VerifyResult(_cco.PrintNormal((int)station, data));
        }

        public override void PrintPeriodicTotalsReport(DateTime startingDate, DateTime endingDate)
        {
            VerifyResult(_cco.PrintPeriodicTotalsReport(startingDate.ToString("ddMMyyyyHHmm"), endingDate.ToString("ddMMyyyyHHmm")));
        }

        public override void PrintPowerLossReport()
        {
            VerifyResult(_cco.PrintPowerLossReport());
        }

        public override void PrintRecCash(decimal amount)
        {
            VerifyResult(_cco.PrintRecCash(amount));
        }

        public override void PrintRecItem(string description, decimal price, int quantity, int vatInfo, decimal unitPrice, string unitName)
        {
            VerifyResult(_cco.PrintRecItem(description, price, quantity, vatInfo, unitPrice, unitName));
        }

        public override void PrintRecItemAdjustment(FiscalAdjustment adjustmentType, string description, decimal amount, int vatInfo)
        {
            VerifyResult(_cco.PrintRecItemAdjustment((int)adjustmentType, description, amount, vatInfo));
        }

        public override void PrintRecItemAdjustmentVoid(FiscalAdjustment adjustmentType, string description, decimal amount, int vatInfo)
        {
            VerifyResult(_cco.PrintRecItemAdjustmentVoid((int)adjustmentType, description, amount, vatInfo));
        }

        public override void PrintRecItemFuel(string description, decimal price, int quantity, int vatInfo, decimal unitPrice, string unitName, decimal specialTax, string specialTaxName)
        {
            VerifyResult(_cco.PrintRecItemFuel(description, price, quantity, vatInfo, unitPrice, unitName, specialTax, specialTaxName));
        }

        public override void PrintRecItemFuelVoid(string description, decimal price, int vatInfo, decimal specialTax)
        {
            VerifyResult(_cco.PrintRecItemFuelVoid(description, price, vatInfo, specialTax));
        }

        public override void PrintRecItemVoid(string description, decimal price, int quantity, int vatInfo, decimal unitPrice, string unitName)
        {
            VerifyResult(_cco.PrintRecItemVoid(description, price, quantity, vatInfo, unitPrice, unitName));
        }

        public override void PrintRecMessage(string message)
        {
            VerifyResult(_cco.PrintRecMessage(message));
        }

        public override void PrintRecNotPaid(string description, decimal amount)
        {
            VerifyResult(_cco.PrintRecNotPaid(description, amount));
        }

        public override void PrintRecPackageAdjustment(FiscalAdjustmentType adjustmentType, string description, IEnumerable<VatInfo> vatAdjustments)
        {
            VerifyResult(_cco.PrintRecPackageAdjustment((int)adjustmentType, description, vatAdjustments.ToString()));
        }

        public override void PrintRecPackageAdjustVoid(FiscalAdjustmentType adjustmentType, IEnumerable<VatInfo> vatAdjustments)
        {
            VerifyResult(_cco.PrintRecPackageAdjustVoid((int)adjustmentType, vatAdjustments.ToString()));
        }

        public override void PrintRecRefund(string description, decimal amount, int vatInfo)
        {
            VerifyResult(_cco.PrintRecRefund(description, amount, vatInfo));
        }

        public override void PrintRecRefundVoid(string description, decimal amount, int vatInfo)
        {
            VerifyResult(_cco.PrintRecRefundVoid(description, amount, vatInfo));
        }

        public override void PrintRecSubtotal(decimal amount)
        {
            VerifyResult(_cco.PrintRecSubtotal(amount));
        }

        public override void PrintRecSubtotalAdjustment(FiscalAdjustment adjustmentType, string description, decimal amount)
        {
            VerifyResult(_cco.PrintRecSubtotalAdjustment((int)adjustmentType, description, amount));
        }

        public override void PrintRecSubtotalAdjustVoid(FiscalAdjustment adjustmentType, decimal amount)
        {
            VerifyResult(_cco.PrintRecSubtotalAdjustVoid((int)adjustmentType, amount));
        }

        public override void PrintRecTaxId(string taxId)
        {
            VerifyResult(_cco.PrintRecTaxID(taxId));
        }

        public override void PrintRecTotal(decimal total, decimal payment, string description)
        {
            VerifyResult(_cco.PrintRecTotal(total, payment, description));
        }

        public override void PrintRecVoid(string description)
        {
            VerifyResult(_cco.PrintRecVoid(description));
        }

        public override void PrintReport(ReportType reportType, string startNum, string endNum)
        {
            VerifyResult(_cco.PrintReport((int)reportType, startNum, endNum));
        }

        public override void PrintXReport()
        {
            VerifyResult(_cco.PrintXReport());
        }

        public override void PrintZReport()
        {
            VerifyResult(_cco.PrintZReport());
        }

        public override void ResetPrinter()
        {
            VerifyResult(_cco.ResetPrinter());
        }

        public override void SetCurrency(FiscalCurrency newCurrency)
        {
            VerifyResult(_cco.EndTraining());
        }

        public override void SetDate(DateTime newDate)
        {
            VerifyResult(_cco.SetDate(newDate.ToString("ddMMyyyyHHmm")));
        }

        public override void SetHeaderLine(int lineNumber, string text, bool doubleWidth)
        {
            VerifyResult(_cco.SetHeaderLine(lineNumber, text, doubleWidth));
        }

        public override void SetPosId(string posId, string cashierId)
        {
            VerifyResult(_cco.SetPOSID(posId, cashierId));
        }

        public override void SetStoreFiscalId(string id)
        {
            VerifyResult(_cco.SetStoreFiscalID(id));
        }

        public override void SetTrailerLine(int lineNumber, string text, bool doubleWidth)
        {
            VerifyResult(_cco.SetTrailerLine(lineNumber, text, doubleWidth));
        }

        public override void SetVatTable()
        {
            VerifyResult(_cco.SetVatTable());
        }

        public override void SetVatValue(int vatId, string vatValue)
        {
            VerifyResult(_cco.SetVatValue(vatId, vatValue));
        }

        public override void VerifyItem(string itemName, int vatId)
        {
            VerifyResult(_cco.VerifyItem(itemName, vatId));
        }

        #endregion
    }
}
