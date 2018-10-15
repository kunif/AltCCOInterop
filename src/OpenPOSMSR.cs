
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.Msr, "OpenPOS MSR", "OPOS MSR Alternative CCO Interop", 1, 14)]

    public class OpenPOSMSR : Msr, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSMSR _cco = null;
        private const string _oposDeviceClass = "MSR";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DataEventHandler DataEvent;
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSMSR()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSMSR()
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
                    _cco.DataEvent -= (_IOPOSMSREvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSMSREvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSMSREvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSMSREvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSMSR();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSMSREvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSMSREvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSMSREvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.StatusUpdateEvent += new _IOPOSMSREvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSMSREvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSMSREvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSMSREvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.StatusUpdateEvent -= (_IOPOSMSREvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSMSR  Specific Properties

        public override string CapCardAuthentication
        {
            get { return _cco.CapCardAuthentication; }
        }

        public override EncryptionAlgorithm CapDataEncryption
        {
            get { return (EncryptionAlgorithm)InteropEnum<EncryptionAlgorithm>.ToEnumFromInteger(_cco.CapDataEncryption); }
        }

        public override DeviceAuthenticationLevel CapDeviceAuthentication
        {
            get { return (DeviceAuthenticationLevel)InteropEnum<DeviceAuthenticationLevel>.ToEnumFromInteger(_cco.CapDeviceAuthentication); }
        }

        public override bool CapIso
        {
            get { return _cco.CapISO; }
        }

        public override bool CapJisOne
        {
            get { return _cco.CapJISOne; }
        }

        public override bool CapJisTwo
        {
            get { return _cco.CapJISTwo; }
        }

        public override bool CapTrackDataMasking
        {
            get { return _cco.CapTrackDataMasking; }
        }

        public override bool CapTransmitSentinels
        {
            get { return _cco.CapTransmitSentinels; }
        }

        public override MsrTracks CapWritableTracks
        {
            get { return (MsrTracks)InteropEnum<MsrTracks>.ToEnumFromInteger(_cco.CapWritableTracks); }
        }

        public override bool DecodeData
        {
            get
            {
                return _cco.DecodeData;
            }
            set
            {
                _cco.DecodeData = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool ParseDecodeData
        {
            get
            {
                return _cco.ParseDecodeData;
            }
            set
            {
                _cco.ParseDecodeData = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override MsrErrorReporting ErrorReportingType
        {
            get
            {
                return (MsrErrorReporting)InteropEnum<MsrErrorReporting>.ToEnumFromInteger(_cco.ErrorReportingType);
            }
            set
            {
                _cco.ErrorReportingType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override MsrTracks TracksToRead
        {
            get
            {
                return (MsrTracks)InteropEnum<MsrTracks>.ToEnumFromInteger(_cco.TracksToRead);
            }
            set
            {
                _cco.TracksToRead = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override MsrTracks TracksToWrite
        {
            get
            {
                return (MsrTracks)InteropEnum<MsrTracks>.ToEnumFromInteger(_cco.TracksToWrite);
            }
            set
            {
                _cco.TracksToWrite = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool TransmitSentinels
        {
            get
            {
                return _cco.TransmitSentinels;
            }
            set
            {
                _cco.TransmitSentinels = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override EncryptionAlgorithm DataEncryptionAlgorithm
        {
            get
            {
                return (EncryptionAlgorithm)InteropEnum<EncryptionAlgorithm>.ToEnumFromInteger(_cco.DataEncryptionAlgorithm);
            }
            set
            {
                _cco.DataEncryptionAlgorithm = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool DeviceAuthenticated
        {
            get { return _cco.DeviceAuthenticated; }
        }

        public override DeviceAuthenticationProtocol DeviceAuthenticationProtocol
        {
            get { return (DeviceAuthenticationProtocol)InteropEnum<DeviceAuthenticationProtocol>.ToEnumFromInteger(_cco.DeviceAuthenticationProtocol); }
        }

        public override int EncodingMaxLength
        {
            get { return _cco.EncodingMaxLength; }
        }

        public override string WriteCardType
        {
            get
            {
                return _cco.WriteCardType;
            }
            set
            {
                _cco.WriteCardType = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string AccountNumber
        {
            get { return _cco.AccountNumber; }
        }

        public override string Title
        {
            get { return _cco.Title; }
        }

        public override string FirstName
        {
            get { return _cco.FirstName; }
        }

        public override string MiddleInitial
        {
            get { return _cco.MiddleInitial; }
        }

        public override string Surname
        {
            get { return _cco.Surname; }
        }

        public override string Suffix
        {
            get { return _cco.Suffix; }
        }

        public override string ExpirationDate
        {
            get { return _cco.ExpirationDate; }
        }

        public override string ServiceCode
        {
            get { return _cco.ServiceCode; }
        }

        public override byte[] Track1Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track1Data, _binaryConversion); }
        }

        public override byte[] Track1DiscretionaryData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track1DiscretionaryData, _binaryConversion); }
        }

        public override byte[] Track1EncryptedData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track1EncryptedData, _binaryConversion); }
        }

        public override int Track1EncryptedDataLength
        {
            get { return _cco.Track1EncryptedDataLength; }
        }

        public override byte[] Track2Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track2Data, _binaryConversion); }
        }

        public override byte[] Track2DiscretionaryData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track2DiscretionaryData, _binaryConversion); }
        }

        public override byte[] Track2EncryptedData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track2EncryptedData, _binaryConversion); }
        }

        public override int Track2EncryptedDataLength
        {
            get { return _cco.Track2EncryptedDataLength; }
        }

        public override byte[] Track3Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track3Data, _binaryConversion); }
        }

        public override byte[] Track3EncryptedData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track3EncryptedData, _binaryConversion); }
        }

        public override int Track3EncryptedDataLength
        {
            get { return _cco.Track3EncryptedDataLength; }
        }

        public override byte[] Track4Data
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track4Data, _binaryConversion); }
        }

        public override byte[] Track4EncryptedData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.Track4EncryptedData, _binaryConversion); }
        }

        public override int Track4EncryptedDataLength
        {
            get { return _cco.Track4EncryptedDataLength; }
        }

        public override byte[] AdditionalSecurityInformation
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.AdditionalSecurityInformation, _binaryConversion); }
        }

        public override byte[] CardAuthenticationData
        {
            get { return InteropCommon.ToByteArrayFromString(_cco.CardAuthenticationData, _binaryConversion); }
        }

        public override int CardAuthenticationDataLength
        {
            get { return _cco.CardAuthenticationDataLength; }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<string> CardPropertyList
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<string>(_cco.CardPropertyList.Split(',')); }
        }

        public override string CardType
        {
            get { return _cco.CardType; }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<string> CardTypeList
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<string>(_cco.CardTypeList.Split(',')); }
        }

        #endregion

        #region OPOSMSR  Specific Methodss

        public override void AuthenticateDevice(byte[] responseToken)
        {
            string sValue = InteropCommon.ToStringFromByteArray(responseToken, _binaryConversion);
            VerifyResult(_cco.AuthenticateDevice(sValue));
        }

        public override void DeauthenticateDevice(byte[] responseToken)
        {
            string sValue = InteropCommon.ToStringFromByteArray(responseToken, _binaryConversion);
            VerifyResult(_cco.DeauthenticateDevice(sValue));
        }

        public override string RetrieveCardProperty(string name)
        {
            string Result = "";
            VerifyResult(_cco.RetrieveCardProperty(name, out Result));
            return Convert.ToString(Result);
        }

        public override byte[] RetrieveDeviceAuthenticationData()
        {
            string Result = "";
            VerifyResult(_cco.RetrieveDeviceAuthenticationData(ref Result));
            return InteropCommon.ToByteArrayFromString(Result, _binaryConversion);
        }

        public override void UpdateKey(string key, string keyName)
        {
            VerifyResult(_cco.UpdateKey(key, keyName));
        }

        public override void WriteTracks(byte[] track1Data, byte[] track2Data, byte[] track3Data, byte[] track4Data, int timeout)
        {
            List<string> TrackData = new List<string>();
            TrackData.Add(InteropCommon.ToStringFromByteArray(track1Data, _binaryConversion));
            TrackData.Add(InteropCommon.ToStringFromByteArray(track2Data, _binaryConversion));
            TrackData.Add(InteropCommon.ToStringFromByteArray(track3Data, _binaryConversion));
            TrackData.Add(InteropCommon.ToStringFromByteArray(track4Data, _binaryConversion));
            VerifyResult(_cco.WriteTracks(TrackData.ToArray(), timeout));
        }

        #endregion
    }
}
