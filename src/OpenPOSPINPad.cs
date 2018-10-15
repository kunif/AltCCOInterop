
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.PinPad, "OpenPOS PINPad", "OPOS PINPad Alternative CCO Interop", 1, 14)]

    public class OpenPOSPINPad : PinPad, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSPINPad _cco = null;
        private const string _oposDeviceClass = "PINPad";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DataEventHandler DataEvent;
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSPINPad()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSPINPad()
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
                    _cco.DataEvent -= (_IOPOSPINPadEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSPINPadEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSPINPadEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSPINPadEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSPINPad();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSPINPadEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSPINPadEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSPINPadEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.StatusUpdateEvent += new _IOPOSPINPadEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSPINPadEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSPINPadEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSPINPadEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.StatusUpdateEvent -= (_IOPOSPINPadEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSPINPad  Specific Properties

        public override string AccountNumber
        {
            get
            {
                return _cco.AccountNumber;
            }
            set
            {
                _cco.AccountNumber = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string AdditionalSecurityInformation
        {
            get { return _cco.AdditionalSecurityInformation; }
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

        public override System.Globalization.CultureInfo[] AvailableLanguagesList
        {
            get
            {
                List<System.Globalization.CultureInfo> ciList = new List<System.Globalization.CultureInfo>();
                int[] alList = InteropCommon.ToIntegerArray(_cco.AvailableLanguagesList, ',');

                foreach (int i in alList)
                {
                    try
                    {
                        ciList.Add(new System.Globalization.CultureInfo(i));
                    }
                    catch
                    {
                    }
                }

                return ciList.ToArray();
            }
        }

        public override PinPadMessage[] AvailablePromptsList
        {
            get
            {
                List<PinPadMessage> pmList = new List<PinPadMessage>();
                int[] apList = InteropCommon.ToIntegerArray(_cco.AvailablePromptsList, ',');

                foreach (int i in apList)
                {
                    if (Enum.IsDefined(typeof(PinPadMessage), i))
                    {
                        pmList.Add((PinPadMessage)InteropEnum<PinPadMessage>.ToEnumFromInteger(i));
                    }
                }

                return pmList.ToArray();
            }
        }

        public override PinPadDisplay CapDisplay
        {
            get { return (PinPadDisplay)InteropEnum<PinPadDisplay>.ToEnumFromInteger(_cco.CapDisplay); }
        }

        public override bool CapKeyboard
        {
            get { return _cco.CapKeyboard; }
        }

        public override PinPadLanguage CapLanguage
        {
            get { return (PinPadLanguage)InteropEnum<PinPadLanguage>.ToEnumFromInteger(_cco.CapLanguage); }
        }

        public override bool CapMacCalculation
        {
            get { return _cco.CapMACCalculation; }
        }

        public override bool CapTone
        {
            get { return _cco.CapTone; }
        }


        public override string EncryptedPin
        {
            get { return _cco.EncryptedPIN; }
        }

        public override int MaximumPinLength
        {
            get
            {
                return _cco.MaximumPINLength;
            }
            set
            {
                _cco.MaximumPINLength = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string MerchantId
        {
            get
            {
                return _cco.MerchantID;
            }
            set
            {
                _cco.MerchantID = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int MinimumPinLength
        {
            get
            {
                return _cco.MinimumPINLength;
            }
            set
            {
                _cco.MinimumPINLength = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool PinEntryEnabled
        {
            get { return _cco.PINEntryEnabled; }
        }

        public override PinPadMessage Prompt
        {
            get
            {
                return (PinPadMessage)InteropEnum<PinPadMessage>.ToEnumFromInteger(_cco.Prompt);
            }
            set
            {
                _cco.Prompt = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override System.Globalization.CultureInfo PromptLanguage
        {
            get
            {
                return new System.Globalization.CultureInfo(_cco.PromptLanguage);
            }
            set
            {
                _cco.PromptLanguage = value.LCID;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string TerminalId
        {
            get
            {
                return _cco.TerminalID;
            }
            set
            {
                _cco.TerminalID = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Track1Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Track1Data, _binaryConversion);
            }
            set
            {
                _cco.Track1Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Track2Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Track2Data, _binaryConversion);
            }
            set
            {
                _cco.Track2Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Track3Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Track3Data, _binaryConversion);
            }
            set
            {
                _cco.Track3Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override byte[] Track4Data
        {
            get
            {
                return InteropCommon.ToByteArrayFromString(_cco.Track4Data, _binaryConversion);
            }
            set
            {
                _cco.Track4Data = InteropCommon.ToStringFromByteArray(value, _binaryConversion);
                VerifyResult(_cco.ResultCode);
            }
        }

        public override EftTransactionType TransactionType
        {
            get
            {
                return (EftTransactionType)InteropEnum<EftTransactionType>.ToEnumFromInteger(_cco.TransactionType);
            }
            set
            {
                _cco.TransactionType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        #endregion

        #region OPOSPINPad  Specific Methodss

        private static Dictionary<Enum, string> s_P4dN2PpadSystem = new Dictionary<Enum, string>()
        {
            { PinPadSystem.Apacs40, "APACS40" },
            { PinPadSystem.AS2805, "AS2805" },
            { PinPadSystem.Dukpt, "DUKPT" },
            { PinPadSystem.Jdebit2, "JDEBIT2" },
            { PinPadSystem.MasterSession, "M/S" }
        };

        public override void BeginEftTransaction(PinPadSystem pinpadSystem, int transactionHost)
        {
            string sValue = s_P4dN2PpadSystem[pinpadSystem];
            VerifyResult(_cco.BeginEFTTransaction(sValue, transactionHost));
        }

        public override void BeginEftTransaction(string pinpadSystem, int transactionHost)
        {
            VerifyResult(_cco.BeginEFTTransaction(pinpadSystem, transactionHost));
        }

        public override string ComputeMac(string inMsg)
        {
            string sOutMsg = "";
            VerifyResult(_cco.ComputeMAC(inMsg, out sOutMsg));
            return Convert.ToString(sOutMsg);
        }

        public override void EnablePinEntry()
        {
            VerifyResult(_cco.EnablePINEntry());
        }

        public override void EndEftTransaction(EftTransactionCompletion completionCode)
        {
            VerifyResult(_cco.EndEFTTransaction((int)completionCode));
        }

        public override void UpdateKey(int keyNumber, string key)
        {
            VerifyResult(_cco.UpdateKey(keyNumber, key));
        }

        public override void VerifyMac(string message)
        {
            VerifyResult(_cco.VerifyMAC(message));
        }

        #endregion
    }
}
