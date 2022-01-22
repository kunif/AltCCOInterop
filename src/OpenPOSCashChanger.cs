﻿
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.CashChanger, "OpenPOS CashChanger", "OPOS CashChanger Alternative CCO Interop", 1, 14)]

    public class OpenPOSCashChanger : CashChanger, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSCashChanger _cco = null;
        private const string _oposDeviceClass = "CashChanger";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DataEventHandler DataEvent;
        public override event DirectIOEventHandler DirectIOEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSCashChanger()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSCashChanger()
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
                    _cco.DataEvent -= (_IOPOSCashChangerEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSCashChangerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSCashChangerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSCashChanger();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSCashChangerEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSCashChangerEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.StatusUpdateEvent += new _IOPOSCashChangerEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSCashChangerEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSCashChangerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.StatusUpdateEvent -= (_IOPOSCashChangerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSCashChanger  Specific Properties

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

        public override int AsyncResultCode
        {
            get { return _cco.AsyncResultCode; }
        }

        public override int AsyncResultCodeExtended
        {
            get { return _cco.AsyncResultCodeExtended; }
        }

        public override bool CapDeposit
        {
            get { return _cco.CapDeposit; }
        }

        public override bool CapDepositDataEvent
        {
            get { return _cco.CapDepositDataEvent; }
        }

        public override bool CapDiscrepancy
        {
            get { return _cco.CapDiscrepancy; }
        }

        public override bool CapEmptySensor
        {
            get { return _cco.CapEmptySensor; }
        }

        public override bool CapFullSensor
        {
            get { return _cco.CapFullSensor; }
        }

        public override bool CapJamSensor
        {
            get { return _cco.CapJamSensor; }
        }

        public override bool CapNearEmptySensor
        {
            get { return _cco.CapNearEmptySensor; }
        }

        public override bool CapNearFullSensor
        {
            get { return _cco.CapNearFullSensor; }
        }

        public override bool CapPauseDeposit
        {
            get { return _cco.CapPauseDeposit; }
        }

        public override bool CapRepayDeposit
        {
            get { return _cco.CapRepayDeposit; }
        }

        public override bool CapRealTimeData
        {
            get { return _cco.CapRealTimeData; }
        }

        public override CashUnits CurrencyCashList
        {
            get { return InteropCommon.ToCashUnits(_cco.CurrencyCashList); }
        }

        public override string CurrencyCode
        {
            get
            {
                return _cco.CurrencyCode;
            }
            set
            {
                _cco.CurrencyCode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string[] CurrencyCodeList
        {
            get { return _cco.CurrencyCodeList.Split(','); }
        }

        public override int CurrentExit
        {
            get
            {
                return _cco.CurrentExit;
            }
            set
            {
                _cco.CurrentExit = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int CurrentService
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

        public override int DepositAmount
        {
            get { return _cco.DepositAmount; }
        }

        public override CashUnits DepositCashList
        {
            get { return InteropCommon.ToCashUnits(_cco.DepositCashList); }
        }

        public override string[] DepositCodeList
        {
            get { return _cco.DepositCodeList.Split(','); }
        }

        public override CashCount[] DepositCounts
        {
            get { return InteropCommon.ToCashCountArray(_cco.DepositCounts); }
        }

        public override CashDepositStatus DepositStatus
        {
            get { return (CashDepositStatus)InteropEnum<CashDepositStatus>.ToEnumFromInteger(_cco.DepositStatus); }
        }

        public override int DeviceExits
        {
            get { return _cco.DeviceExits; }
        }

        public override CashChangerStatus DeviceStatus
        {
            get { return (CashChangerStatus)InteropEnum<CashChangerStatus>.ToEnumFromInteger(_cco.DeviceStatus); }
        }

        public override CashUnits ExitCashList
        {
            get { return InteropCommon.ToCashUnits(_cco.ExitCashList); }
        }

        public override CashChangerFullStatus FullStatus
        {
            get { return (CashChangerFullStatus)InteropEnum<CashChangerFullStatus>.ToEnumFromInteger(_cco.FullStatus); }
        }

        public override bool RealTimeDataEnabled
        {
            get
            {
                return _cco.RealTimeDataEnabled;
            }
            set
            {
                _cco.RealTimeDataEnabled = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int ServiceCount
        {
            get { return _cco.ServiceCount; }
        }

        public override ServiceIndex ServiceIndex
        {
            get
            {
                byte[] value = BitConverter.GetBytes(_cco.ServiceIndex);
                return new ServiceIndex(value[0], value[1], value[2], value[3]);
            }
        }

        #endregion

        #region OPOSCashChanger  Specific Methodss

        public override void AdjustCashCounts(IEnumerable<CashCount> cashCounts)
        {
            string strCashCounts = InteropCommon.ToCashCountsString(cashCounts);
            VerifyResult(_cco.AdjustCashCounts(strCashCounts));
        }

        public override void BeginDeposit()
        {
            VerifyResult(_cco.BeginDeposit());
        }

        public override void DispenseCash(CashCount[] cashCounts)
        {
            VerifyResult(_cco.DispenseCash(InteropCommon.ToCashCountsString(cashCounts)));
        }

        public override void DispenseChange(int amount)
        {
            VerifyResult(_cco.DispenseChange(amount));
        }

        public override void EndDeposit(CashDepositAction successAction)
        {
            VerifyResult(_cco.EndDeposit((int)successAction));
        }

        public override void FixDeposit()
        {
            VerifyResult(_cco.FixDeposit());
        }

        public override void PauseDeposit(CashDepositPause pauseAction)
        {
            VerifyResult(_cco.PauseDeposit((int)pauseAction));
        }

        public override CashCounts ReadCashCounts()
        {
            string strCashCounts = "";
            bool bDiscrepancy = false;
            VerifyResult(_cco.ReadCashCounts(out strCashCounts, out bDiscrepancy));
            return new CashCounts(InteropCommon.ToCashCountArray(strCashCounts), bDiscrepancy);
        }

        #endregion
    }
}
