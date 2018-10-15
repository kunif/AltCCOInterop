
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.Scale, "OpenPOS Scale", "OPOS Scale Alternative CCO Interop", 1, 14)]

    public class OpenPOSScale : Scale, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSScale _cco = null;
        private const string _oposDeviceClass = "Scale";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DataEventHandler DataEvent;
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSScale()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSScale()
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
                    _cco.DataEvent -= (_IOPOSScaleEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSScaleEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSScaleEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSScaleEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSScale();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSScaleEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSScaleEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSScaleEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.StatusUpdateEvent += new _IOPOSScaleEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSScaleEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSScaleEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSScaleEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.StatusUpdateEvent -= (_IOPOSScaleEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSScale  Specific Properties

        public override bool CapDisplay
        {
            get { return _cco.CapDisplay; }
        }

        public override bool CapDisplayText
        {
            get { return _cco.CapDisplayText; }
        }

        public override bool CapPriceCalculating
        {
            get { return _cco.CapPriceCalculating; }
        }

        public override bool CapTareWeight
        {
            get { return _cco.CapTareWeight; }
        }

        public override bool CapZeroScale
        {
            get { return _cco.CapZeroScale; }
        }

        public override bool CapFreezeValue
        {
            get { return _cco.CapFreezeValue; }
        }

        public override bool CapReadLiveWeightWithTare
        {
            get { return _cco.CapReadLiveWeightWithTare; }
        }

        public override bool CapSetPriceCalculationMode
        {
            get { return _cco.CapSetPriceCalculationMode; }
        }

        public override bool CapSetUnitPriceWithWeightUnit
        {
            get { return _cco.CapSetUnitPriceWithWeightUnit; }
        }

        public override bool CapSpecialTare
        {
            get { return _cco.CapSpecialTare; }
        }

        public override bool CapTarePriority
        {
            get { return _cco.CapTarePriority; }
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

        public override int MaxDisplayTextChars
        {
            get { return _cco.MaxDisplayTextChars; }
        }

        public override decimal MaximumWeight
        {
            get { return _cco.MaximumWeight; }
        }

        public override decimal MinimumWeight
        {
            get { return _cco.MinimumWeight; }
        }

        public override decimal SalesPrice
        {
            get { return _cco.SalesPrice; }
        }

        public override decimal TareWeight
        {
            get
            {
                return _cco.TareWeight;
            }
            set
            {
                _cco.TareWeight = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override decimal UnitPrice
        {
            get
            {
                return _cco.UnitPrice;
            }
            set
            {
                _cco.UnitPrice = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override WeightUnit WeightUnit
        {
            get { return (WeightUnit)InteropEnum<WeightUnit>.ToEnumFromInteger(_cco.WeightUnit); }
        }

        public override bool ZeroValid
        {
            get
            {
                return _cco.ZeroValid;
            }
            set
            {
                _cco.ZeroValid = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        #endregion

        #region OPOSScale  Specific Methodss


        public override void DisplayText(string data)
        {
            VerifyResult(_cco.DisplayText(data));
        }

        public override decimal ReadWeight(int timeout)
        {
            int result = 0;
            VerifyResult(_cco.ReadWeight(out result, timeout));
            return new decimal(result);
        }

        public override void ZeroScale()
        {
            VerifyResult(_cco.ZeroScale());
        }

        public override PriceCalculation DoPriceCalculating(int weightData, int tare, decimal unitPrice, decimal unitPriceX, int weightUnitX, int weightNumeratorX, int weightDenominatorX, decimal price, int timeout)
        {
            VerifyResult(_cco.DoPriceCalculating(out weightData, out tare, out unitPrice, out unitPriceX, out weightUnitX, out weightNumeratorX, out weightDenominatorX, out price, timeout));
            return new PriceCalculation(weightData, tare, unitPrice, unitPriceX, weightUnitX, weightNumeratorX, weightDenominatorX, price);
        }

        public override void FreezeValue(int item, bool freeze)
        {
            VerifyResult(_cco.FreezeValue(item, freeze));
        }

        public override WeightTareInfo ReadLiveWeightWithTare(int weightData, int tare, int timeout)
        {
            VerifyResult(_cco.ReadLiveWeightWithTare(out weightData, out tare, timeout));
            return new WeightTareInfo(weightData, tare);
        }

        public override void SetPriceCalculationMode(PriceCalculationMode mode)
        {
            VerifyResult(_cco.SetPriceCalculationMode((int)mode));
        }

        public override void SetSpecialTare(SpecialTare mode, int data)
        {
            VerifyResult(_cco.SetSpecialTare((int)mode, data));
        }

        public override void SetTarePriority(TarePriority priority)
        {
            VerifyResult(_cco.SetTarePriority((int)priority));
        }

        public override void SetUnitPriceWithWeightUnit(decimal unitPrice, int weightUnit, int weightNumerator, int weightDenominator)
        {
            VerifyResult(_cco.SetUnitPriceWithWeightUnit(unitPrice, weightUnit, weightNumerator, weightDenominator));
        }

        #endregion
    }
}
