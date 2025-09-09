using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

namespace WinFormsScanSaraff.Twain
{
    [DebuggerDisplay("SupportedCaps = {SupportedCaps.Get().Count}")]
    public sealed class TwainCapabilities : MarshalByRefObject
    {
        private Dictionary<TwCap, Type> _caps = new Dictionary<TwCap, Type>();

        internal TwainCapabilities(Twain32 twain)
        {
            MethodInfo method = typeof(TwainCapabilities).GetMethod("CreateCapability", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PropertyInfo property in typeof(TwainCapabilities).GetProperties())
            {
                object[] customAttributes = property.GetCustomAttributes(typeof(TwainCapabilities.CapabilityAttribute), false);
                if (customAttributes.Length != 0)
                {
                    TwainCapabilities.CapabilityAttribute capabilityAttribute = customAttributes[0] as TwainCapabilities.CapabilityAttribute;
                    this._caps.Add(capabilityAttribute.Cap, property.PropertyType);
                    property.SetValue((object)this, method.MakeGenericMethod(property.PropertyType.GetGenericArguments()[0]).Invoke((object)this, new object[2]
                    {
            (object) twain,
            (object) capabilityAttribute.Cap
                    }), (object[])null);
                }
            }
        }

        private TwainCapabilities.Capability<T> CreateCapability<T>(Twain32 twain, TwCap cap)
        {
            return Activator.CreateInstance(typeof(TwainCapabilities.Capability<T>), (object)twain, (object)cap) as TwainCapabilities.Capability<T>;
        }

        [TwainCapabilities.Capability(TwCap.DeviceEvent)]
        public ICapability2<TwDE> DeviceEvent { get; private set; }

        [TwainCapabilities.Capability(TwCap.Alarms)]
        public ICapability2<TwAL> Alarms { get; private set; }

        [TwainCapabilities.Capability(TwCap.AlarmVolume)]
        public ICapability<int> AlarmVolume { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticSenseMedium)]
        public ICapability<bool> AutomaticSenseMedium { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutoDiscardBlankPages)]
        public ICapability<TwBP> AutoDiscardBlankPages { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticBorderDetection)]
        public ICapability<bool> AutomaticBorderDetection { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticColorEnabled)]
        public ICapability<bool> AutomaticColorEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticColorNonColorPixelType)]
        public ICapability<TwPixelType> AutomaticColorNonColorPixelType { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticCropUsesFrame)]
        public ICapability<bool> AutomaticCropUsesFrame { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticDeskew)]
        public ICapability<bool> AutomaticDeskew { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticLengthDetection)]
        public ICapability<bool> AutomaticLengthDetection { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticRotate)]
        public ICapability<bool> AutomaticRotate { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutoSize)]
        public ICapability<TwAS> AutoSize { get; private set; }

        [TwainCapabilities.Capability(TwCap.FlipRotation)]
        public ICapability<TwFR> FlipRotation { get; private set; }

        [TwainCapabilities.Capability(TwCap.ImageMerge)]
        public ICapability<TwIM> ImageMerge { get; private set; }

        [TwainCapabilities.Capability(TwCap.ImageMergeHeightThreshold)]
        public ICapability<float> ImageMergeHeightThreshold { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutomaticCapture)]
        public ICapability<int> AutomaticCapture { get; private set; }

        [TwainCapabilities.Capability(TwCap.TimeBeforeFirstCapture)]
        public ICapability<int> TimeBeforeFirstCapture { get; private set; }

        [TwainCapabilities.Capability(TwCap.TimeBetweenCaptures)]
        public ICapability<int> TimeBetweenCaptures { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutoScan)]
        public ICapability<bool> AutoScan { get; private set; }

        [TwainCapabilities.Capability(TwCap.CameraEnabled)]
        public ICapability<bool> CameraEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.CameraOrder)]
        public ICapability2<TwPixelType> CameraOrder { get; private set; }

        [TwainCapabilities.Capability(TwCap.CameraSide)]
        public ICapability<TwCS> CameraSide { get; private set; }

        [TwainCapabilities.Capability(TwCap.ClearBuffers)]
        public ICapability<TwCB> ClearBuffers { get; private set; }

        [TwainCapabilities.Capability(TwCap.MaxBatchBuffers)]
        public ICapability<uint> MaxBatchBuffers { get; private set; }

        [TwainCapabilities.Capability(TwCap.BarCodeDetectionEnabled)]
        public ICapability<bool> BarCodeDetectionEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.SupportedBarCodeTypes)]
        public ICapability2<TwBT> SupportedBarCodeTypes { get; private set; }

        [TwainCapabilities.Capability(TwCap.BarCodeMaxRetries)]
        public ICapability<uint> BarCodeMaxRetries { get; private set; }

        [TwainCapabilities.Capability(TwCap.BarCodeMaxSearchPriorities)]
        public ICapability<uint> BarCodeMaxSearchPriorities { get; private set; }

        [TwainCapabilities.Capability(TwCap.BarCodeSearchMode)]
        public ICapability<TwBD> BarCodeSearchMode { get; private set; }

        [TwainCapabilities.Capability(TwCap.BarCodeSearchPriorities)]
        public ICapability2<TwBT> BarCodeSearchPriorities { get; private set; }

        [TwainCapabilities.Capability(TwCap.BarCodeTimeout)]
        public ICapability<uint> BarCodeTimeout { get; private set; }

        [TwainCapabilities.Capability(TwCap.SupportedCaps)]
        public ICapability2<TwCap> SupportedCaps { get; private set; }

        [TwainCapabilities.Capability(TwCap.ColorManagementEnabled)]
        public ICapability<bool> ColorManagementEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.Filter)]
        public ICapability2<TwFT> Filter { get; private set; }

        [TwainCapabilities.Capability(TwCap.Gamma)]
        public ICapability<float> Gamma { get; private set; }

        [TwainCapabilities.Capability(TwCap.IccProfile)]
        public ICapability<TwIC> IccProfile { get; private set; }

        [TwainCapabilities.Capability(TwCap.PlanarChunky)]
        public ICapability<TwPC> PlanarChunky { get; private set; }

        [TwainCapabilities.Capability(TwCap.BitOrderCodes)]
        public ICapability<TwBO> BitOrderCodes { get; private set; }

        [TwainCapabilities.Capability(TwCap.CcittKFactor)]
        public ICapability<ushort> CcittKFactor { get; private set; }

        [TwainCapabilities.Capability(TwCap.ICompression)]
        public ICapability<TwCompression> Compression { get; private set; }

        [TwainCapabilities.Capability(TwCap.JpegPixelType)]
        public ICapability<TwPixelType> JpegPixelType { get; private set; }

        [TwainCapabilities.Capability(TwCap.JpegQuality)]
        public ICapability<TwJQ> JpegQuality { get; private set; }

        [TwainCapabilities.Capability(TwCap.JpegSubSampling)]
        public ICapability<TwJS> JpegSubSampling { get; private set; }

        [TwainCapabilities.Capability(TwCap.PixelFlavor)]
        public ICapability<TwPF> PixelFlavor { get; private set; }

        [TwainCapabilities.Capability(TwCap.TimeFill)]
        public ICapability<ushort> TimeFill { get; private set; }

        [TwainCapabilities.Capability(TwCap.DeviceOnline)]
        public ICapability<bool> DeviceOnline { get; private set; }

        [TwainCapabilities.Capability(TwCap.DeviceTimeDate)]
        public ICapability<string> DeviceTimeDate { get; private set; }

        [TwainCapabilities.Capability(TwCap.SerialNumber)]
        public ICapability<string> SerialNumber { get; private set; }

        [TwainCapabilities.Capability(TwCap.MinimumHeight)]
        public ICapability<float> MinimumHeight { get; private set; }

        [TwainCapabilities.Capability(TwCap.MinimumWidth)]
        public ICapability<float> MinimumWidth { get; private set; }

        [TwainCapabilities.Capability(TwCap.ExposureTime)]
        public ICapability<float> ExposureTime { get; private set; }

        [TwainCapabilities.Capability(TwCap.FlashUsed2)]
        public ICapability<TwFL> FlashUsed2 { get; private set; }

        [TwainCapabilities.Capability(TwCap.ImageFilter)]
        public ICapability<TwIF> ImageFilter { get; private set; }

        [TwainCapabilities.Capability(TwCap.LampState)]
        public ICapability<bool> LampState { get; private set; }

        [TwainCapabilities.Capability(TwCap.LightPath)]
        public ICapability<TwLP> LightPath { get; private set; }

        [TwainCapabilities.Capability(TwCap.LightSource)]
        public ICapability<TwLS> LightSource { get; private set; }

        [TwainCapabilities.Capability(TwCap.NoiseFilter)]
        public ICapability<TwNF> NoiseFilter { get; private set; }

        [TwainCapabilities.Capability(TwCap.OverScan)]
        public ICapability<TwOV> OverScan { get; private set; }

        [TwainCapabilities.Capability(TwCap.PhysicalHeight)]
        public ICapability<float> PhysicalHeight { get; private set; }

        [TwainCapabilities.Capability(TwCap.PhysicalWidth)]
        public ICapability<float> PhysicalWidth { get; private set; }

        [TwainCapabilities.Capability(TwCap.IUnits)]
        public ICapability<TwUnits> Units { get; private set; }

        [TwainCapabilities.Capability(TwCap.ZoomFactor)]
        public ICapability<short> ZoomFactor { get; private set; }

        [TwainCapabilities.Capability(TwCap.DoubleFeedDetection)]
        public ICapability2<TwDF> DoubleFeedDetection { get; private set; }

        [TwainCapabilities.Capability(TwCap.DoubleFeedDetectionLength)]
        public ICapability<float> DoubleFeedDetectionLength { get; private set; }

        [TwainCapabilities.Capability(TwCap.DoubleFeedDetectionSensitivity)]
        public ICapability<TwUS> DoubleFeedDetectionSensitivity { get; private set; }

        [TwainCapabilities.Capability(TwCap.DoubleFeedDetectionResponse)]
        public ICapability2<TwDP> DoubleFeedDetectionResponse { get; private set; }

        [TwainCapabilities.Capability(TwCap.Endorser)]
        public ICapability<uint> Endorser { get; private set; }

        [TwainCapabilities.Capability(TwCap.Printer)]
        public ICapability<TwPR> Printer { get; private set; }

        [TwainCapabilities.Capability(TwCap.PrinterEnabled)]
        public ICapability<bool> PrinterEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.PrinterIndex)]
        public ICapability<uint> PrinterIndex { get; private set; }

        [TwainCapabilities.Capability(TwCap.PrinterMode)]
        public ICapability<TwPM> PrinterMode { get; private set; }

        [TwainCapabilities.Capability(TwCap.PrinterString)]
        public ICapability<string> PrinterString { get; private set; }

        [TwainCapabilities.Capability(TwCap.PrinterSuffix)]
        public ICapability<string> PrinterSuffix { get; private set; }

        [TwainCapabilities.Capability(TwCap.PrinterVerticalOffset)]
        public ICapability<float> PrinterVerticalOffset { get; private set; }

        [TwainCapabilities.Capability(TwCap.Author)]
        public ICapability<string> Author { get; private set; }

        [TwainCapabilities.Capability(TwCap.Caption)]
        public ICapability<string> Caption { get; private set; }

        [TwainCapabilities.Capability(TwCap.TimeDate)]
        public ICapability<string> TimeDate { get; private set; }

        [TwainCapabilities.Capability(TwCap.ExtImageInfo)]
        public ICapability<bool> ExtImageInfo { get; private set; }

        [TwainCapabilities.Capability(TwCap.SupportedExtImageInfo)]
        public ICapability2<TwEI> SupportedExtImageInfo { get; private set; }

        [TwainCapabilities.Capability(TwCap.ThumbnailsEnabled)]
        public ICapability<bool> ThumbnailsEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutoBright)]
        public ICapability<bool> AutoBright { get; private set; }

        [TwainCapabilities.Capability(TwCap.Brightness)]
        public ICapability<float> Brightness { get; private set; }

        [TwainCapabilities.Capability(TwCap.Contrast)]
        public ICapability<float> Contrast { get; private set; }

        [TwainCapabilities.Capability(TwCap.Highlight)]
        public ICapability<float> Highlight { get; private set; }

        [TwainCapabilities.Capability(TwCap.ImageDataSet)]
        public ICapability2<uint> ImageDataSet { get; private set; }

        [TwainCapabilities.Capability(TwCap.Mirror)]
        public ICapability<TwNF> Mirror { get; private set; }

        [TwainCapabilities.Capability(TwCap.Orientation)]
        public ICapability<TwOR> Orientation { get; private set; }

        [TwainCapabilities.Capability(TwCap.Rotation)]
        public ICapability<float> Rotation { get; private set; }

        [TwainCapabilities.Capability(TwCap.Shadow)]
        public ICapability<float> Shadow { get; private set; }

        [TwainCapabilities.Capability(TwCap.XScaling)]
        public ICapability<float> XScaling { get; private set; }

        [TwainCapabilities.Capability(TwCap.YScaling)]
        public ICapability<float> YScaling { get; private set; }

        [TwainCapabilities.Capability(TwCap.BitDepth)]
        public ICapability<ushort> BitDepth { get; private set; }

        [TwainCapabilities.Capability(TwCap.BitDepthReduction)]
        public ICapability<TwBR> BitDepthReduction { get; private set; }

        [TwainCapabilities.Capability(TwCap.BitOrder)]
        public ICapability<TwBO> BitOrder { get; private set; }

        [TwainCapabilities.Capability(TwCap.CustHalftone)]
        public ICapability2<byte> CustHalftone { get; private set; }

        [TwainCapabilities.Capability(TwCap.Halftones)]
        public ICapability<string> Halftones { get; private set; }

        [TwainCapabilities.Capability(TwCap.IPixelType)]
        public ICapability<TwPixelType> PixelType { get; private set; }

        [TwainCapabilities.Capability(TwCap.Threshold)]
        public ICapability<float> Threshold { get; private set; }

        [TwainCapabilities.Capability(TwCap.Language)]
        public ICapability<TwLanguage> Language { get; private set; }

        [TwainCapabilities.Capability(TwCap.MicrEnabled)]
        public ICapability<bool> MicrEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.Segmented)]
        public ICapability<TwSG> Segmented { get; private set; }

        [TwainCapabilities.Capability(TwCap.Frames)]
        public ICapability<RectangleF> Frames { get; private set; }

        [TwainCapabilities.Capability(TwCap.MaxFrames)]
        public ICapability<ushort> MaxFrames { get; private set; }

        [TwainCapabilities.Capability(TwCap.SupportedSizes)]
        public ICapability<TwSS> SupportedSizes { get; private set; }

        [TwainCapabilities.Capability(TwCap.AutoFeed)]
        public ICapability<bool> AutoFeed { get; private set; }

        [TwainCapabilities.Capability(TwCap.ClearPage)]
        public ICapability<bool> ClearPage { get; private set; }

        [TwainCapabilities.Capability(TwCap.Duplex)]
        public ICapability<TwDX> Duplex { get; private set; }

        [TwainCapabilities.Capability(TwCap.DuplexEnabled)]
        public ICapability<bool> DuplexEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeederAlignment)]
        public ICapability<TwFA> FeederAlignment { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeederEnabled)]
        public ICapability<bool> FeederEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeederLoaded)]
        public ICapability<bool> FeederLoaded { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeederOrder)]
        public ICapability<TwFO> FeederOrder { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeederPocket)]
        public ICapability2<TwFP> FeederPocket { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeederPrep)]
        public ICapability<bool> FeederPrep { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeedPage)]
        public ICapability<bool> FeedPage { get; private set; }

        [TwainCapabilities.Capability(TwCap.PaperDetectable)]
        public ICapability<bool> PaperDetectable { get; private set; }

        [TwainCapabilities.Capability(TwCap.PaperHandling)]
        public ICapability2<TwPH> PaperHandling { get; private set; }

        [TwainCapabilities.Capability(TwCap.ReacquireAllowed)]
        public ICapability<bool> ReacquireAllowed { get; private set; }

        [TwainCapabilities.Capability(TwCap.RewindPage)]
        public ICapability<bool> RewindPage { get; private set; }

        [TwainCapabilities.Capability(TwCap.FeederType)]
        public ICapability<TwFE> FeederType { get; private set; }

        [TwainCapabilities.Capability(TwCap.PatchCodeDetectionEnabled)]
        public ICapability<bool> PatchCodeDetectionEnabled { get; private set; }

        [TwainCapabilities.Capability(TwCap.SupportedPatchCodeTypes)]
        public ICapability2<TwPch> SupportedPatchCodeTypes { get; private set; }

        [TwainCapabilities.Capability(TwCap.PatchCodeMaxSearchPriorities)]
        public ICapability<uint> PatchCodeMaxSearchPriorities { get; private set; }

        [TwainCapabilities.Capability(TwCap.PatchCodeSearchPriorities)]
        public ICapability2<TwPch> PatchCodeSearchPriorities { get; private set; }

        [TwainCapabilities.Capability(TwCap.PatchCodeSearchMode)]
        public ICapability<TwBD> PatchCodeSearchMode { get; private set; }

        [TwainCapabilities.Capability(TwCap.PatchCodeMaxRetries)]
        public ICapability<uint> PatchCodeMaxRetries { get; private set; }

        [TwainCapabilities.Capability(TwCap.PatchCodeTimeout)]
        public ICapability<uint> PatchCodeTimeout { get; private set; }

        [TwainCapabilities.Capability(TwCap.BatteryMinutes)]
        public ICapability<TwBM1> BatteryMinutes { get; private set; }

        [TwainCapabilities.Capability(TwCap.BatteryPercentage)]
        public ICapability<TwBM2> BatteryPercentage { get; private set; }

        [TwainCapabilities.Capability(TwCap.PowerSaveTime)]
        public ICapability<int> PowerSaveTime { get; private set; }

        [TwainCapabilities.Capability(TwCap.PowerSupply)]
        public ICapability<TwPS> PowerSupply { get; private set; }

        [TwainCapabilities.Capability(TwCap.XNativeResolution)]
        public ICapability<float> XNativeResolution { get; private set; }

        [TwainCapabilities.Capability(TwCap.XResolution)]
        public ICapability<float> XResolution { get; private set; }

        [TwainCapabilities.Capability(TwCap.YNativeResolution)]
        public ICapability<float> YNativeResolution { get; private set; }

        [TwainCapabilities.Capability(TwCap.YResolution)]
        public ICapability<float> YResolution { get; private set; }

        [TwainCapabilities.Capability(TwCap.JobControl)]
        public ICapability<TwJC> JobControl { get; private set; }

        [TwainCapabilities.Capability(TwCap.SheetCount)]
        public ICapability<uint> SheetCount { get; private set; }

        [TwainCapabilities.Capability(TwCap.XferCount)]
        public ICapability<short> XferCount { get; private set; }

        [TwainCapabilities.Capability(TwCap.ImageFileFormat)]
        public ICapability<TwFF> ImageFileFormat { get; private set; }

        [TwainCapabilities.Capability(TwCap.Tiles)]
        public ICapability<bool> Tiles { get; private set; }

        [TwainCapabilities.Capability(TwCap.UndefinedImageSize)]
        public ICapability<bool> UndefinedImageSize { get; private set; }

        [TwainCapabilities.Capability(TwCap.IXferMech)]
        public ICapability<TwSX> XferMech { get; private set; }

        [TwainCapabilities.Capability(TwCap.CameraPreviewUI)]
        public ICapability<bool> CameraPreviewUI { get; private set; }

        [TwainCapabilities.Capability(TwCap.CustomDSData)]
        public ICapability<bool> CustomDSData { get; private set; }

        [TwainCapabilities.Capability(TwCap.CustomInterfaceGuid)]
        public ICapability<string> CustomInterfaceGuid { get; private set; }

        [TwainCapabilities.Capability(TwCap.EnableDSUIOnly)]
        public ICapability<bool> EnableDSUIOnly { get; private set; }

        [TwainCapabilities.Capability(TwCap.Indicators)]
        public ICapability<bool> Indicators { get; private set; }

        [TwainCapabilities.Capability(TwCap.IndicatorsMode)]
        public ICapability2<TwCI> IndicatorsMode { get; private set; }

        [TwainCapabilities.Capability(TwCap.UIControllable)]
        public ICapability<bool> UIControllable { get; private set; }

        [DebuggerDisplay("{ToString()}")]
        private class Capability<T> : ICapability<T>, ICapability2<T>
        {
            public Capability(Twain32 twain, TwCap cap)
            {
                this._Twain32 = twain;
                this._Cap = cap;
            }

            public Twain32.Enumeration Get() => this.ToEnumeration(this._Twain32.GetCap(this._Cap));

            public T GetCurrent() => (T)this._Twain32.GetCurrentCap(this._Cap);

            public object[] GetCurrentArray()
            {
                return this.ToEnumeration(this._Twain32.GetCurrentCap(this._Cap)).Items;
            }

            public T GetDefault() => (T)this._Twain32.GetDefaultCap(this._Cap);

            public object[] GetDefaultArray()
            {
                return this.ToEnumeration(this._Twain32.GetDefaultCap(this._Cap)).Items;
            }

            public void Set(T value)
            {
                if (!(this._Twain32.Capabilities._caps[this._Cap] == typeof(ICapability2<T>)) && this.GetCurrent().Equals((object)value))
                    return;
                this._Twain32.SetCap(this._Cap, (object)value);
            }

            public void Set(params T[] value)
            {
                object[] objArray = new object[value.Length];
                for (int index = 0; index < objArray.Length; ++index)
                    objArray[index] = (object)value[index];
                this._Twain32.SetCap(this._Cap, objArray);
            }

            public void SetConstraint(T value)
            {
                this._Twain32.SetConstraintCap(this._Cap, (object)value);
            }

            public void SetConstraint(params T[] value)
            {
                object[] objArray = new object[value.Length];
                for (int index = 0; index < objArray.Length; ++index)
                    objArray[index] = (object)value[index];
                this._Twain32.SetConstraintCap(this._Cap, objArray);
            }

            public void SetConstraint(Twain32.Range value)
            {
                if (value.CurrentValue.GetType() != typeof(T))
                    throw new ArgumentException();
                this._Twain32.SetConstraintCap(this._Cap, value);
            }

            public void SetConstraint(Twain32.Enumeration value)
            {
                if (value.Items == null || value.Items.Length == 0 || value.Items[0].GetType() != typeof(T))
                    throw new ArgumentException();
                this._Twain32.SetConstraintCap(this._Cap, value);
            }

            public void Reset() => this._Twain32.ResetCap(this._Cap);

            public TwQC IsSupported() => this._Twain32.IsCapSupported(this._Cap);

            public bool IsSupported(TwQC operation) => (this.IsSupported() & operation) == operation;

            protected Twain32 _Twain32 { get; private set; }

            protected TwCap _Cap { get; private set; }

            private Twain32.Enumeration ToEnumeration(object value)
            {
                Twain32.Enumeration enumeration = Twain32.Enumeration.FromObject(value);
                for (int index = 0; index < enumeration.Count; ++index)
                    enumeration[index] = typeof(T).IsEnum ? (object)(T)enumeration[index] : Convert.ChangeType(enumeration[index], typeof(T));
                return enumeration;
            }

            public override string ToString()
            {
                TwQC twQc = this.IsSupported();
                return string.Format("{0}, {1}{2}{3}{4}", (object)this._Cap, twQc == (TwQC)0 ? (object)"Not Supported" : (object)"", (twQc & TwQC.GetCurrent) != (TwQC)0 ? (object)string.Format("Current = {0}, ", (object)this.GetCurrent()) : (object)"", (twQc & TwQC.GetDefault) != (TwQC)0 ? (object)string.Format("Default = {0}, ", (object)this.GetDefault()) : (object)"", twQc != (TwQC)0 ? (object)string.Format("Supported = {{{0}}}", (object)this.IsSupported()) : (object)"");
            }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
        private sealed class CapabilityAttribute : Attribute
        {
            public CapabilityAttribute(TwCap cap) => this.Cap = cap;

            public TwCap Cap { get; private set; }
        }
    }
}
