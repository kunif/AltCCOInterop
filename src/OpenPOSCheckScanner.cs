
namespace POS.AltCCOInterop
{
    using System;
    using System.Drawing;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.PointOfService;
    using POS.Devices;

    [ServiceObject(DeviceType.CheckScanner, "OpenPOS CheckScanner", "OPOS CheckScanner Alternative CCO Interop", 1, 14)]

    public class OpenPOSCheckScanner : CheckScanner, ILegacyControlObject, IDisposable
    {
        private POS.Devices.OPOSCheckScanner _cco = null;
        private const string _oposDeviceClass = "CheckScanner";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable
        public override event DataEventHandler DataEvent;
        public override event DirectIOEventHandler DirectIOEvent;
        public override event DeviceErrorEventHandler ErrorEvent;
        public override event StatusUpdateEventHandler StatusUpdateEvent;
        #endregion

        #region Constructor, Destructor
        public OpenPOSCheckScanner()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSCheckScanner()
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
                    _cco.DataEvent -= (_IOPOSCheckScannerEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSCheckScannerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSCheckScannerEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSCheckScannerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
                    _cco = new POS.Devices.OPOSCheckScanner();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSCheckScannerEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSCheckScannerEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSCheckScannerEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.StatusUpdateEvent += new _IOPOSCheckScannerEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            _cco.DataEvent -= (_IOPOSCheckScannerEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSCheckScannerEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSCheckScannerEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.StatusUpdateEvent -= (_IOPOSCheckScannerEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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

        #region OPOSCheckScanner  Specific Properties

        public override bool CapAutoGenerateFileId
        {
            get { return _cco.CapAutoGenerateFileID; }
        }

        public override bool CapAutoGenerateImageTagData
        {
            get { return _cco.CapAutoGenerateImageTagData; }
        }

        public override bool CapAutoSize
        {
            get { return _cco.CapAutoSize; }
        }

        public override CheckColors CapColor
        {
            get { return (CheckColors)InteropEnum<CheckColors>.ToEnumFromInteger(_cco.CapColor); }
        }

        public override bool CapConcurrentMicr
        {
            get { return _cco.CapConcurrentMICR; }
        }

        public override bool CapDefineCropArea
        {
            get { return _cco.CapDefineCropArea; }
        }

        public override CheckImageFormats CapImageFormat
        {
            get { return (CheckImageFormats)InteropEnum<CheckImageFormats>.ToEnumFromInteger(_cco.CapImageFormat); }
        }

        public override bool CapImageTagData
        {
            get { return _cco.CapImageTagData; }
        }

        public override bool CapMicrDevice
        {
            get { return _cco.CapMICRDevice; }
        }

        public override bool CapStoreImageFiles
        {
            get { return _cco.CapStoreImageFiles; }
        }

        public override bool CapValidationDevice
        {
            get { return _cco.CapValidationDevice; }
        }

        public override CheckColors Color
        {
            get
            {
                int num = _cco.Color;

                switch (num)
                {
                    case 1: num = 1; break;

                    case 2: num = 2; break;

                    case 3: num = 4; break;

                    case 4: num = 8; break;

                    case 5: num = 16; break;

                    default: break;
                }

                return (CheckColors)InteropEnum<CheckColors>.ToEnumFromInteger(num);
            }
            set
            {
                int num = 0;

                switch (value)
                {
                    case CheckColors.Mono: num = 1; break;

                    case CheckColors.GrayScale: num = 2; break;

                    case CheckColors.Color16: num = 3; break;

                    case CheckColors.Color256: num = 4; break;

                    case CheckColors.Full: num = 5; break;
                }

                _cco.Color = num;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool ConcurrentMicr
        {
            get
            {
                return _cco.ConcurrentMICR;
            }
            set
            {
                _cco.ConcurrentMICR = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int CropAreaCount
        {
            get { return _cco.CropAreaCount; }
        }

        public override int DocumentHeight
        {
            get
            {
                return _cco.DocumentHeight;
            }
            set
            {
                _cco.DocumentHeight = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int DocumentWidth
        {
            get
            {
                return _cco.DocumentWidth;
            }
            set
            {
                _cco.DocumentWidth = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string FileId
        {
            get
            {
                return _cco.FileID;
            }
            set
            {
                _cco.FileID = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int FileIndex
        {
            get
            {
                return _cco.FileIndex;
            }
            set
            {
                _cco.FileIndex = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override System.Drawing.Bitmap ImageData
        {
            get
            {
                Bitmap Result = null;
                string ImageDataString = _cco.ImageData;

                if (!string.IsNullOrWhiteSpace(ImageDataString))
                {
                    Result = new Bitmap(new MemoryStream(InteropCommon.ToByteArrayFromString(ImageDataString, _binaryConversion)));
                }

                return Result;
            }
        }

        public override CheckImageFormats ImageFormat
        {
            get
            {
                int num = _cco.ImageFormat;

                switch (num)
                {
                    case 1: num = 1; break;

                    case 2: num = 2; break;

                    case 3: num = 4; break;

                    case 4: num = 8; break;

                    case 5: num = 16; break;

                    default: break;
                }

                return (CheckImageFormats)InteropEnum<CheckImageFormats>.ToEnumFromInteger(num);
            }
            set
            {
                int num = 0;

                switch (value)
                {
                    case CheckImageFormats.Native: num = 1; break;

                    case CheckImageFormats.Tiff: num = 2; break;

                    case CheckImageFormats.Bmp: num = 3; break;

                    case CheckImageFormats.Jpeg: num = 4; break;

                    case CheckImageFormats.Gif: num = 5; break;
                }

                _cco.ImageFormat = num;
                VerifyResult(_cco.ResultCode);
            }
        }


        public override ImageMemoryStatus ImageMemoryStatus
        {
            get { return (ImageMemoryStatus)InteropEnum<ImageMemoryStatus>.ToEnumFromInteger(_cco.ImageMemoryStatus); }
        }

        public override string ImageTagData
        {
            get
            {
                return _cco.ImageTagData;
            }
            set
            {
                _cco.ImageTagData = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override MapMode MapMode
        {
            get
            {
                return (MapMode)InteropEnum<MapMode>.ToEnumFromInteger(_cco.MapMode);
            }
            set
            {
                _cco.MapMode = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int MaxCropAreas
        {
            get { return _cco.MaxCropAreas; }
        }

        public override int Quality
        {
            get
            {
                return _cco.Quality;
            }
            set
            {
                _cco.Quality = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int[] QualityList
        {
            get { return InteropCommon.ToIntegerArray(_cco.QualityList, ','); }
        }

        public override int RemainingImagesEstimate
        {
            get { return _cco.RemainingImagesEstimate; }
        }

        #endregion

        #region OPOSCheckScanner  Specific Methodss

        public override void BeginInsertion(int timeout)
        {
            VerifyResult(_cco.BeginInsertion(timeout));
        }

        public override void EndInsertion()
        {
            VerifyResult(_cco.EndInsertion());
        }

        public override void BeginRemoval(int timeout)
        {
            VerifyResult(_cco.BeginRemoval(timeout));
        }

        public override void EndRemoval()
        {
            VerifyResult(_cco.EndRemoval());
        }

        public override void ClearImage(CheckImageClear by)
        {
            VerifyResult(_cco.ClearImage((int)by));
        }

        public override void DefineCropArea(int cropAreaId, int x, int y, int width, int height)
        {
            VerifyResult(_cco.DefineCropArea(cropAreaId, x, y, width, height));
        }

        public override void RetrieveImage(int cropAreaId)
        {
            VerifyResult(_cco.RetrieveImage(cropAreaId));
        }

        public override void RetrieveMemory(CheckImageLocate by)
        {
            VerifyResult(_cco.RetrieveMemory((int)by));
        }

        public override void StoreImage(int cropAreaId)
        {
            VerifyResult(_cco.StoreImage(cropAreaId));
        }

        #endregion
    }
}
