
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.RemoteOrderDisplay, "OpenPOS RemoteOrderDisplay", "OPOS RemoteOrderDisplay Alternative CCO Interop", 1, 14)]

    public class OpenPOSRemoteOrderDisplay : RemoteOrderDisplay, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSRemoteOrderDisplay _cco = null;
        private const string _oposDeviceClass = "RemoteOrderDisplay";
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
        public OpenPOSRemoteOrderDisplay()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSRemoteOrderDisplay()
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
                    _cco.DataEvent -= (_IOPOSRemoteOrderDisplayEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSRemoteOrderDisplayEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSRemoteOrderDisplayEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSRemoteOrderDisplayEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSRemoteOrderDisplayEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSRemoteOrderDisplay();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSRemoteOrderDisplayEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSRemoteOrderDisplayEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSRemoteOrderDisplayEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSRemoteOrderDisplayEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSRemoteOrderDisplayEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSRemoteOrderDisplayEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSRemoteOrderDisplayEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSRemoteOrderDisplayEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSRemoteOrderDisplayEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSRemoteOrderDisplayEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSRemoteOrderDisplay  Specific Properties

        public override bool CapMapCharacterSet
        {
            get { return _cco.CapMapCharacterSet; }
        }

        public override bool CapSelectCharacterSet
        {
            get { return _cco.CapSelectCharacterSet; }
        }

        public override bool CapTone
        {
            get { return _cco.CapTone; }
        }

        public override bool CapTouch
        {
            get { return _cco.CapTouch; }
        }

        public override bool CapTransaction
        {
            get { return _cco.CapTransaction; }
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

        public override int AutoToneDuration
        {
            get
            {
                return _cco.AutoToneDuration;
            }
            set
            {
                _cco.AutoToneDuration = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int AutoToneFrequency
        {
            get
            {
                return _cco.AutoToneFrequency;
            }
            set
            {
                _cco.AutoToneFrequency = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int CharacterSet
        {
            get { return _cco.CharacterSet; }
        }

        public override int[] CharacterSetList
        {
            get { return InteropCommon.ToIntegerArray(_cco.CharacterSetList, ','); }
        }

        public override int Clocks
        {
            get { return _cco.Clocks; }
        }

        public override DeviceUnits CurrentUnitId
        {
            get
            {
                return (DeviceUnits)InteropEnum<DeviceUnits>.ToEnumFromInteger(_cco.CurrentUnitID);
            }
            set
            {
                _cco.CurrentUnitID = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string ErrorString
        {
            get { return _cco.ErrorString; }
        }

        public override DeviceUnits ErrorUnits
        {
            get { return (DeviceUnits)InteropEnum<DeviceUnits>.ToEnumFromInteger(_cco.ErrorUnits); }
        }

        public override string EventString
        {
            get { return _cco.EventString; }
        }

        public override RemoteOrderDisplayEventTypes EventType
        {
            get
            {
                return (RemoteOrderDisplayEventTypes)InteropEnum<RemoteOrderDisplayEventTypes>.ToEnumFromInteger(_cco.EventType);
            }
            set
            {
                _cco.EventType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override DeviceUnits EventUnitId
        {
            get { return (DeviceUnits)InteropEnum<DeviceUnits>.ToEnumFromInteger(_cco.EventUnitID); }
        }

        public override DeviceUnits EventUnits
        {
            get { return (DeviceUnits)InteropEnum<DeviceUnits>.ToEnumFromInteger(_cco.EventUnits); }
        }

        public override bool MapCharacterSet
        {
            get
            {
                return _cco.MapCharacterSet;
            }
            set
            {
                _cco.MapCharacterSet = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int SystemClocks
        {
            get { return _cco.SystemClocks; }
        }

        public override int SystemVideoSaveBuffers
        {
            get { return _cco.SystemVideoSaveBuffers; }
        }

        public override int Timeout
        {
            get
            {
                return _cco.Timeout;
            }
            set
            {
                _cco.Timeout = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override DeviceUnits UnitsOnline
        {
            get { return (DeviceUnits)InteropEnum<DeviceUnits>.ToEnumFromInteger(_cco.UnitsOnline); }
        }

        public override int VideoDataCount
        {
            get { return _cco.VideoDataCount; }
        }

        public override int VideoMode
        {
            get
            {
                return _cco.VideoMode;
            }
            set
            {
                _cco.VideoMode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        private static Regex s_vm = new Regex(@"[0-9]+:[0-9]+x[0-9]+x[0-9]+[MC]", RegexOptions.Compiled);

        public override VideoMode[] VideoModesList
        {
            get
            {
                string strVideoModesList = Regex.Replace(_cco.VideoModesList, @"\s", "");
                List<VideoMode> vml = new List<VideoMode>();

                foreach (string strVM in strVideoModesList.Split(','))
                {
                    if (s_vm.IsMatch(strVM))
                    {
                        bool IsColor = IsColor = strVM.EndsWith("C");
                        string strWork = strVM.Replace(':', 'x').TrimEnd('M', 'C');
                        int[] iParam = InteropCommon.ToIntegerArray(strWork, 'x');
                        vml.Add(new VideoMode(iParam[0], iParam[1], iParam[2], iParam[3], IsColor));
                    }
                }

                return vml.ToArray();
            }
        }

        public override int VideoSaveBuffers
        {
            get { return _cco.VideoSaveBuffers; }
        }

        #endregion

        #region OPOSRemoteOrderDisplay  Specific Methodss

        public override void ClearVideo(DeviceUnits units, VideoAttributes attribute)
        {
            VerifyResult(_cco.ClearVideo((int)units, (int)attribute));
        }

        public override void ClearVideoRegion(DeviceUnits units, int row, int column, int height, int width, VideoAttributes attribute)
        {
            VerifyResult(_cco.ClearVideoRegion((int)units, row, column, height, width, (int)attribute));
        }

        public override void ControlClock(DeviceUnits units, ClockFunction clockFunction, int clockId, int hours, int minutes, int seconds, int row, int column, VideoAttributes attribute, ClockMode mode)
        {
            VerifyResult(_cco.ControlClock((int)units, (int)clockFunction, clockId, hours, minutes, seconds, row, column, (int)attribute, (int)mode));
        }

        public override void ControlCursor(DeviceUnits units, VideoCursorType cursorType)
        {
            VerifyResult(_cco.ControlCursor((int)units, (int)cursorType));
        }

        public override void CopyVideoRegion(DeviceUnits units, int row, int column, int height, int width, int targetRow, int targetColumn)
        {
            VerifyResult(_cco.CopyVideoRegion((int)units, row, column, height, width, targetRow, targetColumn));
        }

        public override void DisplayData(DeviceUnits units, int row, int column, VideoAttributes attribute, string data)
        {
            VerifyResult(_cco.DisplayData((int)units, row, column, (int)attribute, data));
        }

        public override void DrawBox(DeviceUnits units, int row, int column, int height, int width, VideoAttributes attribute, BorderType borderType)
        {
            VerifyResult(_cco.DrawBox((int)units, row, column, height, width, (int)attribute, (int)borderType));
        }

        public override void FreeVideoRegion(DeviceUnits units, int bufferId)
        {
            VerifyResult(_cco.FreeVideoRegion((int)units, bufferId));
        }

        public override void ResetVideo(DeviceUnits units)
        {
            VerifyResult(_cco.ResetVideo((int)units));
        }

        public override void RestoreVideoRegion(DeviceUnits units, int targetRow, int targetColumn, int bufferId)
        {
            VerifyResult(_cco.RestoreVideoRegion((int)units, targetRow, targetColumn, bufferId));
        }

        public override void SaveVideoRegion(DeviceUnits units, int row, int column, int height, int width, int bufferId)
        {
            VerifyResult(_cco.SaveVideoRegion((int)units, row, column, height, width, bufferId));
        }

        public override void SelectCharacterSet(DeviceUnits units, int characterSet)
        {
            VerifyResult(_cco.SelectCharacterSet((int)units, characterSet));
        }

        public override void SetCursor(DeviceUnits units, int row, int column)
        {
            VerifyResult(_cco.SetCursor((int)units, row, column));
        }

        public override void TransactionDisplay(DeviceUnits units, RemoteOrderDisplayTransaction transactionFunction)
        {
            VerifyResult(_cco.TransactionDisplay((int)units, (int)transactionFunction));
        }

        public override void UpdateVideoRegionAttribute(DeviceUnits units, VideoAttributeCommand attributeFunction, int row, int column, int height, int width, VideoAttributes attribute)
        {
            VerifyResult(_cco.UpdateVideoRegionAttribute((int)units, (int)attributeFunction, row, column, height, width, (int)attribute));
        }

        public override void VideoSound(DeviceUnits units, int frequency, int duration, int numberOfCycles, int interSoundWait)
        {
            VerifyResult(_cco.VideoSound((int)units, frequency, duration, numberOfCycles, interSoundWait));
        }

        #endregion
    }
}
