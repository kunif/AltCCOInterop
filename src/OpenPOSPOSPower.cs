
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.PosPower, "OpenPOS POSPower", "OPOS POSPower Alternative CCO Interop", 1, 14)]

    public class OpenPOSPOSPower : PosPower, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSPOSPower _cco = null;
        private const string _oposDeviceClass = "POSPower";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DirectIOEventHandler DirectIOEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSPOSPower()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSPOSPower()
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
                    _cco.DirectIOEvent -= (_IOPOSPOSPowerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSPOSPowerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSPOSPower();

                    // Register event handler
                    _cco.DirectIOEvent += new _IOPOSPOSPowerEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.StatusUpdateEvent += new _IOPOSPOSPowerEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DirectIOEvent -= (_IOPOSPOSPowerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.StatusUpdateEvent -= (_IOPOSPOSPowerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSPOSPower  Specific Properties

        public override bool CapFanAlarm
        {
            get { return _cco.CapFanAlarm; }
        }

        public override bool CapHeatAlarm
        {
            get { return _cco.CapHeatAlarm; }
        }

        public override bool CapQuickCharge
        {
            get { return _cco.CapQuickCharge; }
        }

        public override bool CapRestartPos
        {
            get { return _cco.CapRestartPOS; }
        }

        public override bool CapShutdownPos
        {
            get { return _cco.CapShutdownPOS; }
        }

        public override bool CapStandbyPos
        {
            get { return _cco.CapStandbyPOS; }
        }

        public override bool CapSuspendPos
        {
            get { return _cco.CapSuspendPOS; }
        }

        public override UpsChargeStates CapUpsChargeState
        {
            get { return (UpsChargeStates)InteropEnum<UpsChargeStates>.ToEnumFromInteger(_cco.CapUPSChargeState); }
        }

        public override bool CapVariableBatteryCriticallyLowThreshold
        {
            get { return _cco.CapVariableBatteryCriticallyLowThreshold; }
        }

        public override bool CapVariableBatteryLowThreshold
        {
            get { return _cco.CapVariableBatteryLowThreshold; }
        }

        public override int EnforcedShutdownDelayTime
        {
            get
            {
                return _cco.EnforcedShutdownDelayTime;
            }
            set
            {
                _cco.EnforcedShutdownDelayTime = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int PowerFailDelayTime
        {
            get { return _cco.PowerFailDelayTime; }
        }

        public override bool QuickChargeMode
        {
            get { return _cco.QuickChargeMode; }
        }

        public override int QuickChargeTime
        {
            get { return _cco.QuickChargeTime; }
        }

        public override UpsChargeStates UpsChargeState
        {
            get { return (UpsChargeStates)InteropEnum<UpsChargeStates>.ToEnumFromInteger(_cco.UPSChargeState); }
        }
        public override int BatteryCapacityRemaining
        {
            get { return _cco.BatteryCapacityRemaining; }
        }

        public override int BatteryCriticallyLowThreshold
        {
            get
            {
                return _cco.BatteryCriticallyLowThreshold;
            }
            set
            {
                _cco.BatteryCriticallyLowThreshold = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int BatteryLowThreshold
        {
            get
            {
                return _cco.BatteryLowThreshold;
            }
            set
            {
                _cco.BatteryLowThreshold = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool CapBatteryCapacityRemaining
        {
            get { return _cco.CapBatteryCapacityRemaining; }
        }

        public override PowerSource PowerSource
        {
            get { return (PowerSource)InteropEnum<PowerSource>.ToEnumFromInteger(_cco.PowerSource); }
        }

        #endregion

        #region OPOSPOSPower  Specific Methodss

        public override void RestartPos()
        {
            VerifyResult(_cco.RestartPOS());
        }

        public override void ShutdownPos()
        {
            VerifyResult(_cco.ShutdownPOS());
        }

        public override void StandbyPos(SystemStateChangeReason reason)
        {
            VerifyResult(_cco.StandbyPOS((int)reason));
        }

        public override void SuspendPos(SystemStateChangeReason reason)
        {
            VerifyResult(_cco.SuspendPOS((int)reason));
        }

        #endregion
    }
}
