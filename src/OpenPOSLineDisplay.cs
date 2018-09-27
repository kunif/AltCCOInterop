
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.LineDisplay, "OpenPOS LineDisplay", "OPOS LineDisplay Alternative CCO Interop", 1, 14)]

    public class OpenPOSLineDisplay : LineDisplay, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSLineDisplay _cco = null;
        private const string _oposDeviceClass = "LineDisplay";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DirectIOEventHandler DirectIOEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSLineDisplay()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSLineDisplay()
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
                    _cco.DirectIOEvent -= (_IOPOSLineDisplayEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSLineDisplayEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSLineDisplay();

                    // Register event handler
                    _cco.DirectIOEvent += new _IOPOSLineDisplayEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.StatusUpdateEvent += new _IOPOSLineDisplayEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DirectIOEvent -= (_IOPOSLineDisplayEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.StatusUpdateEvent -= (_IOPOSLineDisplayEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSLineDisplay  Specific Properties

        public override int BlinkRate
        {
            get
            {
                return _cco.BlinkRate;
            }
            set
            {
                _cco.BlinkRate = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool CapBitmap
        {
            get { return _cco.CapBitmap; }
        }

        public override DisplayBlink CapBlink
        {
            get { return (DisplayBlink)InteropEnum<DisplayBlink>.ToEnumFromInteger(_cco.CapBlink); }
        }

        public override bool CapBlinkRate
        {
            get { return _cco.CapBlinkRate; }
        }

        public override bool CapBrightness
        {
            get { return _cco.CapBrightness; }
        }

        public override CharacterSetCapability CapCharacterSet
        {
            get { return (CharacterSetCapability)InteropEnum<CharacterSetCapability>.ToEnumFromInteger(_cco.CapCharacterSet); }
        }

        public override DisplayCursors CapCursorType
        {
            get { return (DisplayCursors)InteropEnum<DisplayCursors>.ToEnumFromInteger(_cco.CapCursorType); }
        }

        public override bool CapCustomGlyph
        {
            get { return _cco.CapCustomGlyph; }
        }

        public override bool CapDescriptors
        {
            get { return _cco.CapDescriptors; }
        }

        public override bool CapHMarquee
        {
            get { return _cco.CapHMarquee; }
        }

        public override bool CapICharWait
        {
            get { return _cco.CapICharWait; }
        }

        public override bool CapMapCharacterSet
        {
            get { return _cco.CapMapCharacterSet; }
        }

        public override DisplayReadBack CapReadBack
        {
            get { return (DisplayReadBack)InteropEnum<DisplayReadBack>.ToEnumFromInteger(_cco.CapReadBack); }
        }

        public override DisplayReverse CapReverse
        {
            get { return (DisplayReverse)InteropEnum<DisplayReverse>.ToEnumFromInteger(_cco.CapReverse); }
        }

        public override bool CapScreenMode
        {
            get { return _cco.CapScreenMode; }
        }

        public override bool CapVMarquee
        {
            get { return _cco.CapVMarquee; }
        }

        public override int CharacterSet
        {
            get
            {
                return _cco.CharacterSet;
            }
            set
            {
                _cco.CharacterSet = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int[] CharacterSetList
        {
            get { return InteropCommon.ToIntegerArray(_cco.CharacterSetList, ','); }
        }

        public override int Columns
        {
            get { return _cco.Columns; }
        }

        public override int CurrentWindow
        {
            get
            {
                return _cco.CurrentWindow;
            }
            set
            {
                _cco.CurrentWindow = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int CursorColumn
        {
            get
            {
                return _cco.CursorColumn;
            }
            set
            {
                _cco.CursorColumn = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int CursorRow
        {
            get
            {
                return _cco.CursorRow;
            }
            set
            {
                _cco.CursorRow = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override DisplayCursors CursorType
        {
            get
            {
                return (DisplayCursors)InteropEnum<DisplayCursors>.ToEnumFromInteger(_cco.CursorType);
            }
            set
            {
                _cco.CursorType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool CursorUpdate
        {
            get
            {
                return _cco.CursorUpdate;
            }
            set
            {
                _cco.CursorUpdate = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override RangeOfCharacters[] CustomGlyphList
        {
            get
            {
                string strGlyphList = _cco.CustomGlyphList;
                List<RangeOfCharacters> roc = new List<RangeOfCharacters>();

                if (!string.IsNullOrWhiteSpace(strGlyphList))
                {
                    foreach (string strRange in strGlyphList.Split(','))
                    {
                        int[] iRange = null;

                        if (!string.IsNullOrWhiteSpace(strRange))
                        {
                            List<int> iList = new List<int>();

                            foreach (string s in strRange.Split('-'))
                            {
                                if (string.IsNullOrWhiteSpace(s))
                                {
                                    continue;
                                }

                                int iValue = 0;

                                if (Int32.TryParse(s.Trim(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out iValue))
                                {
                                    iList.Add(iValue);
                                }
                            }

                            iRange = iList.ToArray();
                        }

                        if (iRange.Length == 1)
                        {
                            roc.Add(new RangeOfCharacters(Convert.ToChar(iRange[0])));
                        }
                        else if (iRange.Length >= 2)
                        {
                            roc.Add(new RangeOfCharacters(Convert.ToChar(iRange[0]), Convert.ToChar(iRange[1])));
                        }
                    }
                }

                return roc.ToArray();
            }
        }

        public override int DeviceBrightness
        {
            get
            {
                return _cco.DeviceBrightness;
            }
            set
            {
                _cco.DeviceBrightness = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int DeviceColumns
        {
            get { return _cco.DeviceColumns; }
        }

        public override int DeviceDescriptors
        {
            get { return _cco.DeviceDescriptors; }
        }

        public override int DeviceRows
        {
            get { return _cco.DeviceRows; }
        }

        public override int DeviceWindows
        {
            get { return _cco.DeviceWindows; }
        }

        public override int GlyphHeight
        {
            get { return _cco.GlyphHeight; }
        }

        public override int GlyphWidth
        {
            get { return _cco.GlyphWidth; }
        }

        public override int InterCharacterWait
        {
            get
            {
                return _cco.InterCharacterWait;
            }
            set
            {
                _cco.InterCharacterWait = value;
                VerifyResult(_cco.ResultCode);
            }
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

        public override DisplayMarqueeFormat MarqueeFormat
        {
            get
            {
                return (DisplayMarqueeFormat)InteropEnum<DisplayMarqueeFormat>.ToEnumFromInteger(_cco.MarqueeFormat);
            }
            set
            {
                _cco.MarqueeFormat = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int MarqueeRepeatWait
        {
            get
            {
                return _cco.MarqueeRepeatWait;
            }
            set
            {
                _cco.MarqueeRepeatWait = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override DisplayMarqueeType MarqueeType
        {
            get
            {
                return (DisplayMarqueeType)InteropEnum<DisplayMarqueeType>.ToEnumFromInteger(_cco.MarqueeType);
            }
            set
            {
                _cco.MarqueeType = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int MarqueeUnitWait
        {
            get
            {
                return _cco.MarqueeUnitWait;
            }
            set
            {
                _cco.MarqueeUnitWait = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int MaximumX
        {
            get { return _cco.MaximumX; }
        }

        public override int MaximumY
        {
            get { return _cco.MaximumY; }
        }

        public override int Rows
        {
            get { return _cco.Rows; }
        }

        public override int ScreenMode
        {
            get
            {
                return _cco.ScreenMode;
            }
            set
            {
                _cco.ScreenMode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override DisplayScreenMode[] ScreenModeList
        {
            get
            {
                string strScreenModeList = _cco.ScreenModeList;
                List<DisplayScreenMode> dsm = new List<DisplayScreenMode>();

                foreach (string strScreen in strScreenModeList.Split(','))
                {
                    int[] iScreen = InteropCommon.ToIntegerArray(strScreen, 'x');

                    if (iScreen.Length == 2)
                    {
                        dsm.Add(new DisplayScreenMode(Convert.ToChar(iScreen[0]), Convert.ToChar(iScreen[1])));
                    }
                }

                return dsm.ToArray();
            }
        }

        #endregion

        #region OPOSLineDisplay  Specific Methodss

        public override void ClearDescriptors()
        {
            VerifyResult(_cco.ClearDescriptors());
        }

        public override void ClearText()
        {
            VerifyResult(_cco.ClearText());
        }

        public override void CreateWindow(int viewportRow, int viewportColumn, int viewportHeight, int viewportWidth, int windowHeight, int windowWidth)
        {
            VerifyResult(_cco.CreateWindow(viewportRow, viewportColumn, viewportHeight, viewportWidth, windowHeight, windowWidth));
        }

        public override void DefineGlyph(int glyphCode, byte[] glyph)
        {
            VerifyResult(_cco.DefineGlyph(glyphCode, InteropCommon.ToStringFromByteArray(glyph, _binaryConversion)));
        }

        public override void DestroyWindow()
        {
            VerifyResult(_cco.DestroyWindow());
        }

        public override void DisplayBitmap(string fileName, int alignmentX, int alignmentY)
        {
            VerifyResult(_cco.DisplayBitmap(fileName, DisplayBitmapAsIs, alignmentX, alignmentY));
        }

        public override void DisplayBitmap(string fileName, int width, int alignmentX, int alignmentY)
        {
            VerifyResult(_cco.DisplayBitmap(fileName, width, alignmentX, alignmentY));
        }

        public override void DisplayText(string data)
        {
            VerifyResult(_cco.DisplayText(data, (int)DisplayTextMode.Normal));
        }

        public override void DisplayText(string data, DisplayTextMode attribute)
        {
            VerifyResult(_cco.DisplayText(data, (int)attribute));
        }

        public override void DisplayTextAt(int row, int column, string data)
        {
            VerifyResult(_cco.DisplayTextAt(row, column, data, (int)DisplayTextMode.Normal));
        }

        public override void DisplayTextAt(int row, int column, string data, DisplayTextMode attribute)
        {
            VerifyResult(_cco.DisplayTextAt(row, column, data, (int)attribute));
        }

        public override int ReadCharacterAtCursor()
        {
            int iValue = 0;
            VerifyResult(_cco.ReadCharacterAtCursor(out iValue));
            return iValue;
        }

        public override void RefreshWindow(int window)
        {
            VerifyResult(_cco.RefreshWindow(window));
        }

        public override void ScrollText(DisplayScrollText direction, int units)
        {
            VerifyResult(_cco.ScrollText((int)direction, units));
        }

        public override void SetBitmap(int bitmapNumber, string fileName, int alignmentX, int alignmentY)
        {
            VerifyResult(_cco.SetBitmap(bitmapNumber, fileName, DisplayBitmapAsIs, alignmentX, alignmentY));
        }

        public override void SetBitmap(int bitmapNumber, string fileName, int width, int alignmentX, int alignmentY)
        {
            VerifyResult(_cco.SetBitmap(bitmapNumber, fileName, width, alignmentX, alignmentY));
        }

        public override void SetDescriptor(int descriptor, DisplaySetDescriptor attribute)
        {
            VerifyResult(_cco.SetDescriptor(descriptor, (int)attribute));
        }

        #endregion
    }
}
