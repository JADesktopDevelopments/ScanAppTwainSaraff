using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//using System.Xml;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;

namespace WinFormsScanSaraff.Twain
{
    [ToolboxBitmap(typeof(Twain32), "Resources.scanner.bmp")]
    [DebuggerDisplay("ProductName = {_appid.ProductName.Value}, Version = {_appid.Version.Info}, DS = {_srcds.ProductName}")]
    [DefaultEvent("AcquireCompleted")]
    [DefaultProperty("AppProductName")]
    public sealed class Twain32 : Component
    {
        private Twain32._DsmEntry _dsmEntry;
        private IntPtr _hTwainDll;
        private IContainer _components = (IContainer)new System.ComponentModel.Container();
        private IntPtr _hwnd;
        private TwIdentity _appid;
        private TwIdentity _srcds;
        private Twain32._MessageFilter _filter;
        private TwIdentity[] _sources = new TwIdentity[0];
        private ApplicationContext _context;
        private Collection<Twain32._Image> _images = new Collection<Twain32._Image>();
        private Twain32.TwainStateFlag _twainState;
        private bool _isTwain2Enable = IntPtr.Size != 4 || Environment.OSVersion.Platform == PlatformID.Unix;
        private CallBackProc _callbackProc;
        private TwainCapabilities _capabilities;

        public Twain32()
        {
            this._srcds = new TwIdentity();
            this._srcds.Id = 0U;
            this._filter = new Twain32._MessageFilter(this);
            this.ShowUI = true;
            this.DisableAfterAcquire = true;
            this.Palette = new Twain32.TwainPalette(this);
            this._callbackProc = new CallBackProc(this._TwCallbackProc);
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    break;
                case PlatformID.MacOSX:
                    throw new NotImplementedException();
                default:
                    Form form = new Form();
                    this._components.Add((IComponent)form);
                    this._hwnd = form.Handle;
                    break;
            }
        }

        public Twain32(IContainer container)
          : this()
        {
            container.Add((IComponent)this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.CloseDSM();
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        this._UnloadDSM();
                        if (this._components != null)
                        {
                            this._components.Dispose();
                            break;
                        }
                        break;
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        this._filter.Dispose();
                        goto case PlatformID.Unix;
                }
            }
            base.Dispose(disposing);
        }

        public bool OpenDSM()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        this._hTwainDll = Twain32._Platform.Load("/usr/local/lib/libtwaindsm.so");
                        break;
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        string path = Path.ChangeExtension(Path.Combine(Environment.SystemDirectory, "TWAINDSM"), ".dll");
                        this._hTwainDll = Twain32._Platform.Load(!File.Exists(path) || !this.IsTwain2Enable ? Path.ChangeExtension(Path.Combine(Environment.SystemDirectory, "..\\twain_32"), ".dll") : path);
                        if (this.Parent != null)
                        {
                            this._hwnd = this.Parent.Handle;
                            break;
                        }
                        break;
                }
                IntPtr ptr = this._hTwainDll != IntPtr.Zero ? Twain32._Platform.GetProcAddr(this._hTwainDll, "DSM_Entry") : throw new TwainException("Cann't load DSM.");
                this._dsmEntry = ptr != IntPtr.Zero ? Twain32._DsmEntry.Create(ptr) : throw new TwainException("Cann't find DSM_Entry entry point.");
                Twain32._Memory._SetEntryPoints((TwEntryPoint)null);
                TwRC rc1 = this._dsmEntry.DsmParent(this._AppId, IntPtr.Zero, TwDG.Control, TwDAT.Parent, TwMSG.OpenDSM, ref this._hwnd);
                if (rc1 != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc1);
                this._TwainState |= Twain32.TwainStateFlag.DSMOpen;
                if (this.IsTwain2Supported)
                {
                    TwEntryPoint data = new TwEntryPoint();
                    TwRC rc2 = this._dsmEntry.DsmInvoke<TwEntryPoint>(this._AppId, TwDG.Control, TwDAT.EntryPoint, TwMSG.Get, ref data);
                    if (rc2 != TwRC.Success)
                        throw new TwainException(this._GetTwainStatus(), rc2);
                    Twain32._Memory._SetEntryPoints(data);
                }
                this._GetAllSorces();
            }
            return (this._TwainState & Twain32.TwainStateFlag.DSMOpen) != 0;
        }

        public bool SelectSource()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                throw new NotSupportedException("DG_CONTROL / DAT_IDENTITY / MSG_USERSELECT is not available on Linux.");
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) != (Twain32.TwainStateFlag)0)
                return false;
            if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
            {
                this.OpenDSM();
                if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
                    return false;
            }
            TwIdentity data = new TwIdentity();
            TwRC rc = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.UserSelect, ref data);
            switch (rc)
            {
                case TwRC.Success:
                    this._srcds = data;
                    return true;
                case TwRC.Cancel:
                    return false;
                default:
                    throw new TwainException(this._GetTwainStatus(), rc);
            }
        }

        public bool OpenDataSource()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) != (Twain32.TwainStateFlag)0 && (this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0)
            {
                TwRC rc = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.OpenDS, ref this._srcds);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
                this._TwainState |= Twain32.TwainStateFlag.DSOpen;
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        this._RegisterCallback();
                        break;
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        if (this.IsTwain2Supported && (this._srcds.SupportedGroups & TwDG.DS2) != (TwDG)0)
                        {
                            this._RegisterCallback();
                            break;
                        }
                        break;
                }
            }
            return (this._TwainState & Twain32.TwainStateFlag.DSOpen) != 0;
        }

        private void _RegisterCallback()
        {
            TwCallback2 data = new TwCallback2()
            {
                CallBackProc = this._callbackProc
            };
            TwRC rc = this._dsmEntry.DsInvoke<TwCallback2>(this._AppId, this._srcds, TwDG.Control, TwDAT.Callback2, TwMSG.RegisterCallback, ref data);
            if (rc != TwRC.Success)
                throw new TwainException(this._GetTwainStatus(), rc);
        }

        private bool _EnableDataSource()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) != (Twain32.TwainStateFlag)0 && (this._TwainState & Twain32.TwainStateFlag.DSEnabled) == (Twain32.TwainStateFlag)0)
            {
                TwUserInterface data = new TwUserInterface()
                {
                    ShowUI = (TwBool)this.ShowUI,
                    ModalUI = (TwBool)this.ModalUI,
                    ParentHand = this._hwnd
                };
                TwRC rc = this._dsmEntry.DsInvoke<TwUserInterface>(this._AppId, this._srcds, TwDG.Control, TwDAT.UserInterface, TwMSG.EnableDS, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
                if ((this._TwainState & Twain32.TwainStateFlag.DSReady) != (Twain32.TwainStateFlag)0)
                    this._TwainState &= ~Twain32.TwainStateFlag.DSReady;
                else
                    this._TwainState |= Twain32.TwainStateFlag.DSEnabled;
            }
            return (this._TwainState & Twain32.TwainStateFlag.DSEnabled) != 0;
        }

        public void Acquire()
        {
            if (!this.OpenDSM() || !this.OpenDataSource() || !this._EnableDataSource())
                return;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    break;
                case PlatformID.MacOSX:
                    throw new NotImplementedException();
                default:
                    if (!this.IsTwain2Supported || (this._srcds.SupportedGroups & TwDG.DS2) == (TwDG)0)
                        this._filter.SetFilter();
                    if (Application.MessageLoop)
                        break;
                    Application.Run(this._context = new ApplicationContext());
                    break;
            }
        }

        private bool _DisableDataSource()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSEnabled) == (Twain32.TwainStateFlag)0)
                return false;
            try
            {
                TwUserInterface data = new TwUserInterface()
                {
                    ParentHand = this._hwnd,
                    ShowUI = (TwBool)false
                };
                TwRC rc = this._dsmEntry.DsInvoke<TwUserInterface>(this._AppId, this._srcds, TwDG.Control, TwDAT.UserInterface, TwMSG.DisableDS, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
            }
            finally
            {
                this._TwainState &= ~Twain32.TwainStateFlag.DSEnabled;
                if (this._context != null)
                {
                    this._context.ExitThread();
                    this._context.Dispose();
                    this._context = (ApplicationContext)null;
                }
            }
            return (this._TwainState & Twain32.TwainStateFlag.DSEnabled) == (Twain32.TwainStateFlag)0;
        }

        public bool CloseDataSource()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0 || (this._TwainState & Twain32.TwainStateFlag.DSEnabled) != (Twain32.TwainStateFlag)0)
                return false;
            this._images.Clear();
            TwRC rc = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.CloseDS, ref this._srcds);
            if (rc != TwRC.Success)
                throw new TwainException(this._GetTwainStatus(), rc);
            this._TwainState &= ~Twain32.TwainStateFlag.DSOpen;
            return (this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0;
        }

        public bool CloseDSM()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSEnabled) != (Twain32.TwainStateFlag)0)
                this._DisableDataSource();
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) != (Twain32.TwainStateFlag)0)
                this.CloseDataSource();
            if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0 || (this._TwainState & Twain32.TwainStateFlag.DSOpen) != (Twain32.TwainStateFlag)0)
                return false;
            TwRC rc = this._dsmEntry.DsmParent(this._AppId, IntPtr.Zero, TwDG.Control, TwDAT.Parent, TwMSG.CloseDSM, ref this._hwnd);
            if (rc != TwRC.Success)
                throw new TwainException(this._GetTwainStatus(), rc);
            this._TwainState &= ~Twain32.TwainStateFlag.DSMOpen;
            this._UnloadDSM();
            return (this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0;
        }

        private void _UnloadDSM()
        {
            this._AppId = (TwIdentity)null;
            if (!(this._hTwainDll != IntPtr.Zero))
                return;
            Twain32._Platform.Unload(this._hTwainDll);
            this._hTwainDll = IntPtr.Zero;
        }

        public Image GetImage(int index) => (Image)this._images[index];

        [Browsable(false)]
        public int ImageCount => this._images.Count;

        [DefaultValue(true)]
        [Category("Behavior")]
        [Description("Возвращает или устанавливает значение, указывающее на необходимость деактивации источника данных после получения изображения.")]
        public bool DisableAfterAcquire { get; set; }

        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("Возвращает или устанавливает значение, указывающее на необходимость использования TWAIN 2.0.")]
        public bool IsTwain2Enable
        {
            get => this._isTwain2Enable;
            set
            {
                if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) != (Twain32.TwainStateFlag)0)
                    throw new InvalidOperationException("DSM already opened.");
                if (IntPtr.Size != 4 && !value)
                    throw new InvalidOperationException("In x64 mode only TWAIN 2.x enabled.");
                if (Environment.OSVersion.Platform == PlatformID.Unix && !value)
                    throw new InvalidOperationException("On UNIX platform only TWAIN 2.x enabled.");
                if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                    throw new NotImplementedException();
                if (this._isTwain2Enable = value)
                    this._AppId.SupportedGroups |= TwDG.APP2;
                else
                    this._AppId.SupportedGroups &= ~TwDG.APP2;
                this._AppId.ProtocolMajor = this._isTwain2Enable ? (ushort)2 : (ushort)1;
                this._AppId.ProtocolMinor = this._isTwain2Enable ? (ushort)3 : (ushort)9;
            }
        }

        [Browsable(false)]
        public bool IsTwain2Supported
        {
            get
            {
                if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
                    throw new InvalidOperationException("DSM is not open.");
                return (this._AppId.SupportedGroups & TwDG.DSM2) > (TwDG)0;
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public int SourceIndex
        {
            get
            {
                if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
                    return -1;
                int sourceIndex = 0;
                while (sourceIndex < this._sources.Length && !this._sources[sourceIndex].Equals((object)this._srcds))
                    ++sourceIndex;
                return sourceIndex;
            }
            set
            {
                if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
                    throw new TwainException("Менеджер источников данных не открыт.");
                if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) != (Twain32.TwainStateFlag)0)
                    throw new TwainException("Источник данных уже открыт.");
                this._srcds = this._sources[value];
            }
        }

        [Browsable(false)]
        public int SourcesCount => this._sources.Length;

        public string GetSourceProductName(int index) => (string)this._sources[index].ProductName;

        public Twain32.Identity GetSourceIdentity(int index)
        {
            return new Twain32.Identity(this._sources[index]);
        }

        public bool GetIsSourceTwain2Compatible(int index)
        {
            return (this._sources[index].SupportedGroups & TwDG.DS2) > (TwDG)0;
        }

        public void SetDefaultSource(int index)
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
                throw new TwainException("DSM не открыт.");
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) != (Twain32.TwainStateFlag)0)
                throw new TwainException("Источник данных уже открыт. Необходимо сперва закрыть источник данных.");
            TwIdentity source = this._sources[index];
            TwRC rc = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.Set, ref source);
            if (rc != TwRC.Success)
                throw new TwainException(this._GetTwainStatus(), rc);
        }

        public int GetDefaultSource()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSMOpen) == (Twain32.TwainStateFlag)0)
                throw new TwainException("DSM не открыт.");
            TwIdentity data = new TwIdentity();
            TwRC rc = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.GetDefault, ref data);
            if (rc != TwRC.Success)
                throw new TwainException(this._GetTwainStatus(), rc);
            for (int defaultSource = 0; defaultSource < this._sources.Length; ++defaultSource)
            {
                if ((int)data.Id == (int)this._sources[defaultSource].Id)
                    return defaultSource;
            }
            throw new TwainException("Не удалось найти источник данных по умолчанию.");
        }

        [Browsable(false)]
        [ReadOnly(true)]
        private TwIdentity _AppId
        {
            get
            {
                if (this._appid == null)
                {
                    Assembly assembly = typeof(Twain32).Assembly;
                    AssemblyName assemblyName = new AssemblyName(assembly.FullName);
                    Version version = new Version(((AssemblyFileVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0]).Version);
                    this._appid = new TwIdentity()
                    {
                        Id = 0U,
                        Version = new TwVersion()
                        {
                            MajorNum = (ushort)version.Major,
                            MinorNum = (ushort)version.Minor,
                            Language = TwLanguage.SPANISH,
                            Country = TwCountry.GUATEMALA,
                            Info = (TwStr32)assemblyName.Version.ToString()
                        },
                        ProtocolMajor = this._isTwain2Enable ? (ushort)2 : (ushort)1,
                        ProtocolMinor = this._isTwain2Enable ? (ushort)3 : (ushort)9,
                        SupportedGroups = (TwDG)(3 | (this._isTwain2Enable ? 536870912 : 0)),
                        Manufacturer = (TwStr32)((AssemblyCompanyAttribute)assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0]).Company,
                        ProductFamily = (TwStr32)"TWAIN Class Library",
                        ProductName = (TwStr32)((AssemblyProductAttribute)assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product
                    };
                }
                return this._appid;
            }
            set
            {
                if (value != null)
                    throw new ArgumentException("Is read only property.");
                this._appid = (TwIdentity)null;
            }
        }

        [Category("Behavior")]
        [Description("Возвращает или устанавливает имя приложения.")]
        public string AppProductName
        {
            get => (string)this._AppId.ProductName;
            set => this._AppId.ProductName = (TwStr32)value;
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Возвращает или устанавливает значение указывающие на необходимость отображения UI TWAIN-источника.")]
        public bool ShowUI { get; set; }

        [Category("Behavior")]
        [DefaultValue(false)]
        private bool ModalUI { get; set; }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Возвращает или устанавливает родительское окно для TWAIN-источника.")]
        public IWin32Window Parent { get; set; }

        [Category("Culture")]
        [DefaultValue(TwLanguage.SPANISH)]
        [Description("Возвращает или устанавливает используемый приложением язык. Get or set the primary language for your application.")]
        public TwLanguage Language
        {
            get => this._AppId.Version.Language;
            set => this._AppId.Version.Language = value;
        }

        [Category("Culture")]
        [DefaultValue(TwCountry.BELARUS)]
        [Description("Возвращает или устанавливает страну происхождения приложения. Get or set the primary country where your application is intended to be distributed.")]
        public TwCountry Country
        {
            get => this._AppId.Version.Country;
            set => this._AppId.Version.Country = value;
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public RectangleF ImageLayout
        {
            get
            {
                TwImageLayout data = new TwImageLayout();
                TwRC rc = this._dsmEntry.DsInvoke<TwImageLayout>(this._AppId, this._srcds, TwDG.Image, TwDAT.ImageLayout, TwMSG.Get, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
                return (RectangleF)data.Frame;
            }
            set
            {
                TwImageLayout data = new TwImageLayout()
                {
                    Frame = (TwFrame)value
                };
                TwRC rc = this._dsmEntry.DsInvoke<TwImageLayout>(this._AppId, this._srcds, TwDG.Image, TwDAT.ImageLayout, TwMSG.Set, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public TwainCapabilities Capabilities
        {
            get
            {
                if (this._capabilities == null)
                    this._capabilities = new TwainCapabilities(this);
                return this._capabilities;
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public Twain32.TwainPalette Palette { get; private set; }

        [Obsolete("Use Twain32.Capabilities.XResolution.Get() instead.", true)]
        public Twain32.Enumeration GetResolutions()
        {
            return Twain32.Enumeration.FromObject(this.GetCap(TwCap.XResolution));
        }

        [Obsolete("Use Twain32.Capabilities.XResolution.Set(value) and Twain32.Capabilities.YResolution.Set(value) instead.", true)]
        public void SetResolutions(float value)
        {
            this.SetCap(TwCap.XResolution, (object)value);
            this.SetCap(TwCap.YResolution, (object)value);
        }

        [Obsolete("Use Twain32.Capabilities.PixelType.Get() instead.", true)]
        public Twain32.Enumeration GetPixelTypes()
        {
            Twain32.Enumeration pixelTypes = Twain32.Enumeration.FromObject(this.GetCap(TwCap.IPixelType));
            for (int index = 0; index < pixelTypes.Count; ++index)
                pixelTypes[index] = (object)(TwPixelType)pixelTypes[index];
            return pixelTypes;
        }

        [Obsolete("Use Twain32.Capabilities.PixelType.Set(value) instead.", true)]
        public void SetPixelType(TwPixelType value) => this.SetCap(TwCap.IPixelType, (object)value);

        [Obsolete("Use Twain32.Capabilities.Units.Get() instead.", true)]
        public Twain32.Enumeration GetUnitOfMeasure()
        {
            Twain32.Enumeration unitOfMeasure = Twain32.Enumeration.FromObject(this.GetCap(TwCap.IUnits));
            for (int index = 0; index < unitOfMeasure.Count; ++index)
                unitOfMeasure[index] = (object)(TwUnits)unitOfMeasure[index];
            return unitOfMeasure;
        }

        [Obsolete("Use Twain32.Capabilities.Units.Set(value) instead.", true)]
        public void SetUnitOfMeasure(TwUnits value) => this.SetCap(TwCap.IUnits, (object)value);

        public TwQC IsCapSupported(TwCap capability)
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0)
                throw new TwainException("Источник данных не открыт.");
            TwCapability data = new TwCapability(capability);
            try
            {
                return this._dsmEntry.DsInvoke<TwCapability>(this._AppId, this._srcds, TwDG.Control, TwDAT.Capability, TwMSG.QuerySupport, ref data) == TwRC.Success ? (TwQC)((TwOneValue)data.GetValue()).Item : (TwQC)0;
            }
            finally
            {
                data.Dispose();
            }
        }

        private object _GetCapCore(TwCap capability, TwMSG msg)
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0)
                throw new TwainException("Источник данных не открыт.");
            TwCapability data = new TwCapability(capability);
            try
            {
                TwRC rc = this._dsmEntry.DsInvoke<TwCapability>(this._AppId, this._srcds, TwDG.Control, TwDAT.Capability, msg, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
                switch (data.ConType)
                {
                    case TwOn.Array:
                        return (object)((__ITwArray)data.GetValue()).Items;
                    case TwOn.Enum:
                        __ITwEnumeration itwEnumeration = data.GetValue() as __ITwEnumeration;
                        return (object)Twain32.Enumeration.CreateEnumeration(itwEnumeration.Items, itwEnumeration.CurrentIndex, itwEnumeration.DefaultIndex);
                    case TwOn.One:
                        object obj = data.GetValue();
                        return obj is TwOneValue twOneValue ? TwTypeHelper.CastToCommon(twOneValue.ItemType, TwTypeHelper.ValueToTw<uint>(twOneValue.ItemType, twOneValue.Item)) : obj;
                    case TwOn.Range:
                        return (object)Twain32.Range.CreateRange((TwRange)data.GetValue());
                    default:
                        return data.GetValue();
                }
            }
            finally
            {
                data.Dispose();
            }
        }

        public object GetCap(TwCap capability) => this._GetCapCore(capability, TwMSG.Get);

        public object GetCurrentCap(TwCap capability) => this._GetCapCore(capability, TwMSG.GetCurrent);

        public object GetDefaultCap(TwCap capability) => this._GetCapCore(capability, TwMSG.GetDefault);

        public void ResetCap(TwCap capability)
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0)
                throw new TwainException("Источник данных не открыт.");
            TwCapability data = new TwCapability(capability);
            try
            {
                TwRC rc = this._dsmEntry.DsInvoke<TwCapability>(this._AppId, this._srcds, TwDG.Control, TwDAT.Capability, TwMSG.Reset, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
            }
            finally
            {
                data.Dispose();
            }
        }

        public void ResetAllCap()
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0)
                throw new TwainException("Источник данных не открыт.");
            TwCapability data = new TwCapability(TwCap.SupportedCaps);
            try
            {
                TwRC rc = this._dsmEntry.DsInvoke<TwCapability>(this._AppId, this._srcds, TwDG.Control, TwDAT.Capability, TwMSG.ResetAll, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
            }
            finally
            {
                data.Dispose();
            }
        }

        private void _SetCapCore(TwCapability cap, TwMSG msg)
        {
            if ((this._TwainState & Twain32.TwainStateFlag.DSOpen) == (Twain32.TwainStateFlag)0)
                throw new TwainException("Источник данных не открыт.");
            try
            {
                TwRC rc = this._dsmEntry.DsInvoke<TwCapability>(this._AppId, this._srcds, TwDG.Control, TwDAT.Capability, msg, ref cap);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
            }
            finally
            {
                cap.Dispose();
            }
        }

        private void _SetCapCore(TwCap capability, TwMSG msg, object value)
        {
            TwCapability cap;
            if (value is string)
            {
                object[] customAttributes = typeof(TwCap).GetField(capability.ToString()).GetCustomAttributes(typeof(TwTypeAttribute), false);
                cap = customAttributes.Length == 0 ? new TwCapability(capability, (string)value, TwTypeHelper.TypeOf(value)) : new TwCapability(capability, (string)value, ((TwTypeAttribute)customAttributes[0]).TwType);
            }
            else
            {
                TwType type = TwTypeHelper.TypeOf(value.GetType());
                cap = new TwCapability(capability, TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(type, value)), type);
            }
            this._SetCapCore(cap, msg);
        }

        private void _SetCapCore(TwCap capability, TwMSG msg, object[] value)
        {
            object[] customAttributes = typeof(TwCap).GetField(capability.ToString()).GetCustomAttributes(typeof(TwTypeAttribute), false);
            int cap = (int)capability;
            TwArray array = new TwArray();
            array.ItemType = customAttributes.Length != 0 ? ((TwTypeAttribute)customAttributes[0]).TwType : TwTypeHelper.TypeOf(value[0]);
            array.NumItems = (uint)value.Length;
            object[] arrayValue = value;
            this._SetCapCore(new TwCapability((TwCap)cap, array, arrayValue), msg);
        }

        private void _SetCapCore(TwCap capability, TwMSG msg, Twain32.Range value)
        {
            this._SetCapCore(new TwCapability(capability, value.ToTwRange()), msg);
        }

        private void _SetCapCore(TwCap capability, TwMSG msg, Twain32.Enumeration value)
        {
            object[] customAttributes = typeof(TwCap).GetField(capability.ToString()).GetCustomAttributes(typeof(TwTypeAttribute), false);
            int cap = (int)capability;
            TwEnumeration enumeration = new TwEnumeration();
            enumeration.ItemType = customAttributes.Length != 0 ? ((TwTypeAttribute)customAttributes[0]).TwType : TwTypeHelper.TypeOf(value[0]);
            enumeration.NumItems = (uint)value.Count;
            enumeration.CurrentIndex = (uint)value.CurrentIndex;
            enumeration.DefaultIndex = (uint)value.DefaultIndex;
            object[] items = value.Items;
            this._SetCapCore(new TwCapability((TwCap)cap, enumeration, items), msg);
        }

        public void SetCap(TwCap capability, object value)
        {
            this._SetCapCore(capability, TwMSG.Set, value);
        }

        public void SetCap(TwCap capability, object[] value)
        {
            this._SetCapCore(capability, TwMSG.Set, value);
        }

        public void SetCap(TwCap capability, Twain32.Range value)
        {
            this._SetCapCore(capability, TwMSG.Set, value);
        }

        public void SetCap(TwCap capability, Twain32.Enumeration value)
        {
            this._SetCapCore(capability, TwMSG.Set, value);
        }

        public void SetConstraintCap(TwCap capability, object value)
        {
            this._SetCapCore(capability, TwMSG.SetConstraint, value);
        }

        public void SetConstraintCap(TwCap capability, object[] value)
        {
            this._SetCapCore(capability, TwMSG.SetConstraint, value);
        }

        public void SetConstraintCap(TwCap capability, Twain32.Range value)
        {
            this._SetCapCore(capability, TwMSG.SetConstraint, value);
        }

        public void SetConstraintCap(TwCap capability, Twain32.Enumeration value)
        {
            this._SetCapCore(capability, TwMSG.SetConstraint, value);
        }

        private void _NativeTransferPictures()
        {
            if (this._srcds.Id == 0U)
                return;
            IntPtr zero1 = IntPtr.Zero;
            TwPendingXfers data = new TwPendingXfers();
            try
            {
                this._images.Clear();
                do
                {
                    data.Count = (ushort)0;
                    IntPtr zero2 = IntPtr.Zero;
                    TwRC rc1 = this._dsmEntry.DSImageXfer(this._AppId, this._srcds, TwDG.Image, TwDAT.ImageNativeXfer, TwMSG.Get, ref zero2);
                    if (rc1 != TwRC.XferDone)
                        throw new TwainException(this._GetTwainStatus(), rc1);
                    if (!this._OnXferDone(new Twain32.XferDoneEventArgs(new Twain32.GetImageInfoCallback(this._GetImageInfo), new Twain32.GetExtImageInfoCallback(this._GetExtImageInfo))))
                    {
                        IntPtr num = Twain32._Memory.Lock(zero2);
                        try
                        {
                            Twain32._Image image;
                            switch (Environment.OSVersion.Platform)
                            {
                                case PlatformID.Unix:
                                    image = (Twain32._Image)Tiff.FromPtrToImage(num);
                                    break;
                                case PlatformID.MacOSX:
                                    throw new NotImplementedException();
                                default:
                                    image = (Twain32._Image)DibToImage.WithStream(num, this.GetService(typeof(IStreamProvider)) as IStreamProvider);
                                    break;
                            }
                            this._images.Add(image);
                            if (this._OnEndXfer(new Twain32.EndXferEventArgs((object)image)))
                                return;
                        }
                        finally
                        {
                            Twain32._Memory.Unlock(zero2);
                            Twain32._Memory.Free(zero2);
                        }
                        TwRC rc2 = this._dsmEntry.DsInvoke<TwPendingXfers>(this._AppId, this._srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.EndXfer, ref data);
                        if (rc2 != TwRC.Success)
                            throw new TwainException(this._GetTwainStatus(), rc2);
                    }
                    else
                        goto label_1;
                }
                while (data.Count != (ushort)0);
                goto label_13;
            label_1:
                return;
            label_13:;
            }
            finally
            {
                int num = (int)this._dsmEntry.DsInvoke<TwPendingXfers>(this._AppId, this._srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.Reset, ref data);
            }
        }

        private void _FileTransferPictures()
        {
            if (this._srcds.Id == 0U)
                return;
            TwPendingXfers data1 = new TwPendingXfers();
            try
            {
                this._images.Clear();
                TwSetupFileXfer data2;
                do
                {
                    data1.Count = (ushort)0;
                    data2 = new TwSetupFileXfer()
                    {
                        Format = (this.Capabilities.ImageFileFormat.IsSupported() & TwQC.GetCurrent) != (TwQC)0 ? this.Capabilities.ImageFileFormat.GetCurrent() : TwFF.Bmp,
                        FileName = (TwStr255)Path.GetTempFileName()
                    };
                    Twain32.SetupFileXferEventArgs e = new Twain32.SetupFileXferEventArgs();
                    if (!this._OnSetupFileXfer(e))
                    {
                        if (!string.IsNullOrEmpty(e.FileName))
                            data2.FileName = (TwStr255)e.FileName;
                        if ((this.Capabilities.ImageFileFormat.IsSupported() & TwQC.GetCurrent) != (TwQC)0)
                            data2.Format = this.Capabilities.ImageFileFormat.GetCurrent();
                        TwRC rc1 = this._dsmEntry.DsInvoke<TwSetupFileXfer>(this._AppId, this._srcds, TwDG.Control, TwDAT.SetupFileXfer, TwMSG.Set, ref data2);
                        if (rc1 != TwRC.Success)
                            throw new TwainException(this._GetTwainStatus(), rc1);
                        TwRC rc2 = this._dsmEntry.DsRaw(this._AppId, this._srcds, TwDG.Image, TwDAT.ImageFileXfer, TwMSG.Get, IntPtr.Zero);
                        if (rc2 != TwRC.XferDone)
                            throw new TwainException(this._GetTwainStatus(), rc2);
                        if (!this._OnXferDone(new Twain32.XferDoneEventArgs(new Twain32.GetImageInfoCallback(this._GetImageInfo), new Twain32.GetExtImageInfoCallback(this._GetExtImageInfo))))
                        {
                            TwRC rc3 = this._dsmEntry.DsInvoke<TwPendingXfers>(this._AppId, this._srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.EndXfer, ref data1);
                            if (rc3 != TwRC.Success)
                                throw new TwainException(this._GetTwainStatus(), rc3);
                            TwRC rc4 = this._dsmEntry.DsInvoke<TwSetupFileXfer>(this._AppId, this._srcds, TwDG.Control, TwDAT.SetupFileXfer, TwMSG.Get, ref data2);
                            if (rc4 != TwRC.Success)
                                throw new TwainException(this._GetTwainStatus(), rc4);
                        }
                        else
                            goto label_20;
                    }
                    else
                        goto label_1;
                }
                while (!this._OnFileXfer(new Twain32.FileXferEventArgs(Twain32.ImageFileXfer.Create(data2))) && data1.Count != (ushort)0);
                goto label_14;
            label_1:
                return;
            label_20:
                return;
            label_14:;
            }
            finally
            {
                int num = (int)this._dsmEntry.DsInvoke<TwPendingXfers>(this._AppId, this._srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.Reset, ref data1);
            }
        }

        private void _MemoryTransferPictures(bool isMemFile)
        {
            if (this._srcds.Id == 0U)
                return;
            TwPendingXfers data1 = new TwPendingXfers();
            try
            {
                this._images.Clear();
                do
                {
                    data1.Count = (ushort)0;
                    Twain32.ImageInfo imageInfo = this._GetImageInfo();
                    if (isMemFile && (this.Capabilities.ImageFileFormat.IsSupported() & TwQC.GetCurrent) != (TwQC)0)
                    {
                        TwSetupFileXfer data2 = new TwSetupFileXfer()
                        {
                            Format = this.Capabilities.ImageFileFormat.GetCurrent()
                        };
                        TwRC rc = this._dsmEntry.DsInvoke<TwSetupFileXfer>(this._AppId, this._srcds, TwDG.Control, TwDAT.SetupFileXfer, TwMSG.Set, ref data2);
                        if (rc != TwRC.Success)
                            throw new TwainException(this._GetTwainStatus(), rc);
                    }
                    TwSetupMemXfer data3 = new TwSetupMemXfer();
                    TwRC rc1 = this._dsmEntry.DsInvoke<TwSetupMemXfer>(this._AppId, this._srcds, TwDG.Control, TwDAT.SetupMemXfer, TwMSG.Get, ref data3);
                    if (rc1 != TwRC.Success)
                        throw new TwainException(this._GetTwainStatus(), rc1);
                    if (!this._OnSetupMemXfer(new Twain32.SetupMemXferEventArgs(imageInfo, data3.Preferred)))
                    {
                        IntPtr handle = Twain32._Memory.Alloc((int)data3.Preferred);
                        if (handle == IntPtr.Zero)
                            throw new TwainException("Ошибка выделениия памяти.");
                        try
                        {
                            TwMemory twMemory = new TwMemory()
                            {
                                Flags = TwMF.AppOwns | TwMF.Pointer,
                                Length = data3.Preferred,
                                TheMem = Twain32._Memory.Lock(handle)
                            };
                            TwRC twRc;
                            do
                            {
                                TwImageMemXfer data4 = new TwImageMemXfer()
                                {
                                    Memory = twMemory
                                };
                                Twain32._Memory.ZeroMemory(data4.Memory.TheMem, (IntPtr)(long)data4.Memory.Length);
                                twRc = this._dsmEntry.DsInvoke<TwImageMemXfer>(this._AppId, this._srcds, TwDG.Image, isMemFile ? TwDAT.ImageMemFileXfer : TwDAT.ImageMemXfer, TwMSG.Get, ref data4);
                                switch (twRc)
                                {
                                    case TwRC.Success:
                                    case TwRC.XferDone:
                                        if (!this._OnMemXfer(new Twain32.MemXferEventArgs(imageInfo, Twain32.ImageMemXfer.Create(data4))))
                                            continue;
                                        goto label_20;
                                    default:
                                        int twainStatus = (int)this._GetTwainStatus();
                                        int num = (int)this._dsmEntry.DsInvoke<TwPendingXfers>(this._AppId, this._srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.EndXfer, ref data1);
                                        int rc2 = (int)twRc;
                                        throw new TwainException((TwCC)twainStatus, (TwRC)rc2);
                                }
                            }
                            while (twRc != TwRC.XferDone);
                            goto label_18;
                        label_20:
                            return;
                        label_18:
                            if (this._OnXferDone(new Twain32.XferDoneEventArgs(new Twain32.GetImageInfoCallback(this._GetImageInfo), new Twain32.GetExtImageInfoCallback(this._GetExtImageInfo))))
                                return;
                        }
                        finally
                        {
                            Twain32._Memory.Unlock(handle);
                            Twain32._Memory.Free(handle);
                        }
                        TwRC rc3 = this._dsmEntry.DsInvoke<TwPendingXfers>(this._AppId, this._srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.EndXfer, ref data1);
                        if (rc3 != TwRC.Success)
                            throw new TwainException(this._GetTwainStatus(), rc3);
                    }
                    else
                        goto label_1;
                }
                while (data1.Count != (ushort)0);
                goto label_16;
            label_1:
                return;
            label_16:;
            }
            finally
            {
                int num = (int)this._dsmEntry.DsInvoke<TwPendingXfers>(this._AppId, this._srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.Reset, ref data1);
            }
        }

        private TwRC _TwCallbackProc(
          TwIdentity srcId,
          TwIdentity appId,
          TwDG dg,
          TwDAT dat,
          TwMSG msg,
          IntPtr data)
        {
            try
            {
                if (appId == null || (int)appId.Id != (int)this._AppId.Id)
                    return TwRC.Failure;
                if ((this._TwainState & Twain32.TwainStateFlag.DSEnabled) == (Twain32.TwainStateFlag)0)
                    this._TwainState |= Twain32.TwainStateFlag.DSEnabled | Twain32.TwainStateFlag.DSReady;
                this._TwCallbackProcCore(msg, (Twain32.Action<bool>)(isCloseReq =>
                {
                    if (!isCloseReq && !this.DisableAfterAcquire)
                        return;
                    this._DisableDataSource();
                }));
            }
            catch (Exception ex)
            {
                this._OnAcquireError(new Twain32.AcquireErrorEventArgs(new TwainException(ex.Message, ex)));
            }
            return TwRC.Success;
        }

        private void _TwCallbackProcCore(TwMSG msg, Twain32.Action<bool> endAction)
        {
            try
            {
                switch (msg)
                {
                    case TwMSG.XFerReady:
                        switch (this.Capabilities.XferMech.GetCurrent())
                        {
                            case TwSX.File:
                                this._FileTransferPictures();
                                break;
                            case TwSX.Memory:
                                this._MemoryTransferPictures(false);
                                break;
                            case TwSX.MemFile:
                                this._MemoryTransferPictures(true);
                                break;
                            default:
                                this._NativeTransferPictures();
                                break;
                        }
                        endAction(false);
                        this._OnAcquireCompleted(new EventArgs());
                        break;
                    case TwMSG.CloseDSReq:
                        endAction(true);
                        break;
                    case TwMSG.CloseDSOK:
                        endAction(false);
                        break;
                    case TwMSG.DeviceEvent:
                        this._DeviceEventObtain();
                        break;
                }
            }
            catch (TwainException ex)
            {
                try
                {
                    endAction(false);
                }
                catch
                {
                }
                this._OnAcquireError(new Twain32.AcquireErrorEventArgs(ex));
            }
            catch
            {
                try
                {
                    endAction(false);
                }
                catch
                {
                }
                throw;
            }
        }

        private void _DeviceEventObtain()
        {
            TwDeviceEvent data = new TwDeviceEvent();
            if (this._dsmEntry.DsInvoke<TwDeviceEvent>(this._AppId, this._srcds, TwDG.Control, TwDAT.DeviceEvent, TwMSG.Get, ref data) != TwRC.Success)
                return;
            this._OnDeviceEvent(new Twain32.DeviceEventEventArgs(data));
        }

        private void _OnAcquireCompleted(EventArgs e)
        {
            if (this.AcquireCompleted == null)
                return;
            this.AcquireCompleted((object)this, e);
        }

        private void _OnAcquireError(Twain32.AcquireErrorEventArgs e)
        {
            if (this.AcquireError == null)
                return;
            this.AcquireError((object)this, e);
        }

        private bool _OnXferDone(Twain32.XferDoneEventArgs e)
        {
            if (this.XferDone != null)
                this.XferDone((object)this, e);
            return e.Cancel;
        }

        private bool _OnEndXfer(Twain32.EndXferEventArgs e)
        {
            if (this.EndXfer != null)
                this.EndXfer((object)this, e);
            return e.Cancel;
        }

        private bool _OnSetupMemXfer(Twain32.SetupMemXferEventArgs e)
        {
            if (this.SetupMemXferEvent != null)
                this.SetupMemXferEvent((object)this, e);
            return e.Cancel;
        }

        private bool _OnMemXfer(Twain32.MemXferEventArgs e)
        {
            if (this.MemXferEvent != null)
                this.MemXferEvent((object)this, e);
            return e.Cancel;
        }

        private bool _OnSetupFileXfer(Twain32.SetupFileXferEventArgs e)
        {
            if (this.SetupFileXferEvent != null)
                this.SetupFileXferEvent((object)this, e);
            return e.Cancel;
        }

        private bool _OnFileXfer(Twain32.FileXferEventArgs e)
        {
            if (this.FileXferEvent != null)
                this.FileXferEvent((object)this, e);
            return e.Cancel;
        }

        private void _OnDeviceEvent(Twain32.DeviceEventEventArgs e)
        {
            if (this.DeviceEvent == null)
                return;
            this.DeviceEvent((object)this, e);
        }

        private void _GetAllSorces()
        {
            List<TwIdentity> twIdentityList = new List<TwIdentity>();
            TwIdentity data = new TwIdentity();
            try
            {
                TwRC rc1 = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.GetFirst, ref data);
                if (rc1 != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc1);
                twIdentityList.Add(data);
                TwRC rc2;
                while (true)
                {
                    data = new TwIdentity();
                    rc2 = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.GetNext, ref data);
                    switch (rc2)
                    {
                        case TwRC.Success:
                            twIdentityList.Add(data);
                            continue;
                        case TwRC.EndOfList:
                            goto label_7;
                        default:
                            goto label_6;
                    }
                }
            label_6:
                throw new TwainException(this._GetTwainStatus(), rc2);
            label_7:
                TwRC rc3 = this._dsmEntry.DsmInvoke<TwIdentity>(this._AppId, TwDG.Control, TwDAT.Identity, TwMSG.GetDefault, ref this._srcds);
                if (rc3 != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc3);
            }
            finally
            {
                this._sources = twIdentityList.ToArray();
            }
        }

        private Twain32.TwainStateFlag _TwainState
        {
            get => this._twainState;
            set
            {
                if (this._twainState == value)
                    return;
                this._twainState = value;
                if (this.TwainStateChanged == null)
                    return;
                this.TwainStateChanged((object)this, new Twain32.TwainStateEventArgs(this._twainState));
            }
        }

        private TwCC _GetTwainStatus()
        {
            TwStatus data = new TwStatus();
            int num = (int)this._dsmEntry.DsInvoke<TwStatus>(this._AppId, this._srcds, TwDG.Control, TwDAT.Status, TwMSG.Get, ref data);
            return data.ConditionCode;
        }

        private Twain32.ImageInfo _GetImageInfo()
        {
            TwImageInfo data = new TwImageInfo();
            TwRC rc = this._dsmEntry.DsInvoke<TwImageInfo>(this._AppId, this._srcds, TwDG.Image, TwDAT.ImageInfo, TwMSG.Get, ref data);
            if (rc != TwRC.Success)
                throw new TwainException(this._GetTwainStatus(), rc);
            return Twain32.ImageInfo.FromTwImageInfo(data);
        }

        private Twain32.ExtImageInfo _GetExtImageInfo(TwEI[] extInfo)
        {
            TwInfo[] info = new TwInfo[extInfo.Length];
            for (int index = 0; index < extInfo.Length; ++index)
                info[index] = new TwInfo()
                {
                    InfoId = extInfo[index]
                };
            IntPtr ptr = TwExtImageInfo.ToPtr(info);
            try
            {
                TwRC rc = this._dsmEntry.DsRaw(this._AppId, this._srcds, TwDG.Image, TwDAT.ExtImageInfo, TwMSG.Get, ptr);
                if (rc != TwRC.Success)
                    throw new TwainException(this._GetTwainStatus(), rc);
                return Twain32.ExtImageInfo.FromPtr(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Category("Action")]
        [Description("Возникает в момент окончания сканирования. Occurs when the acquire is completed.")]
        public event EventHandler AcquireCompleted;

        [Category("Action")]
        [Description("Возникает в момент получения ошибки в процессе сканирования. Occurs when error received during acquire.")]
        public event EventHandler<Twain32.AcquireErrorEventArgs> AcquireError;

        [Category("Native Mode Action")]
        [Description("Возникает в момент окончания получения изображения приложением. Occurs when the transfer into application was completed (Native Mode Transfer).")]
        public event EventHandler<Twain32.EndXferEventArgs> EndXfer;

        [Category("Action")]
        [Description("Возникает в момент окончания получения изображения источником. Occurs when the transfer was completed.")]
        public event EventHandler<Twain32.XferDoneEventArgs> XferDone;

        [Category("Memory Mode Action")]
        [Description("Возникает в момент установки размера буфера памяти. Occurs when determined size of buffer to use during the transfer (Memory Mode Transfer and MemFile Mode Transfer).")]
        public event EventHandler<Twain32.SetupMemXferEventArgs> SetupMemXferEvent;

        [Category("Memory Mode Action")]
        [Description("Возникает в момент получения очередного блока данных. Occurs when the memory block for the data was recived (Memory Mode Transfer and MemFile Mode Transfer).")]
        public event EventHandler<Twain32.MemXferEventArgs> MemXferEvent;

        [Category("File Mode Action")]
        [Description("Возникает в момент, когда необходимо задать имя файла изображения. Occurs when you need to specify the filename. (File Mode Transfer)")]
        public event EventHandler<Twain32.SetupFileXferEventArgs> SetupFileXferEvent;

        [Category("File Mode Action")]
        [Description("Возникает в момент окончания получения файла изображения приложением. Occurs when the transfer into application was completed (File Mode Transfer).")]
        public event EventHandler<Twain32.FileXferEventArgs> FileXferEvent;

        [Category("Behavior")]
        [Description("Возникает в момент изменения состояния twain-устройства. Occurs when TWAIN state was changed.")]
        public event EventHandler<Twain32.TwainStateEventArgs> TwainStateChanged;

        [Category("Behavior")]
        [Description("Возникает в момент, когда источник уведомляет приложение о произошедшем событии. Occurs when enabled the source sends this message to the Application to alert it that some event has taken place.")]
        public event EventHandler<Twain32.DeviceEventEventArgs> DeviceEvent;

        [Flags]
        public enum TwainStateFlag
        {
            DSMOpen = 1,
            DSOpen = 2,
            DSEnabled = 4,
            DSReady = 8,
        }

        [Serializable]
        public sealed class EndXferEventArgs : Twain32.SerializableCancelEventArgs
        {
            private Twain32._Image _image;

            internal EndXferEventArgs(object image) => this._image = image as Twain32._Image;

            public Image Image => (Image)this._image;

            //public ImageSource ImageSource => (ImageSource)this._image;
        }

        public sealed class XferDoneEventArgs : Twain32.SerializableCancelEventArgs
        {
            private Twain32.GetImageInfoCallback _imageInfoMethod;
            private Twain32.GetExtImageInfoCallback _extImageInfoMethod;

            internal XferDoneEventArgs(
              Twain32.GetImageInfoCallback method1,
              Twain32.GetExtImageInfoCallback method2)
            {
                this._imageInfoMethod = method1;
                this._extImageInfoMethod = method2;
            }

            public Twain32.ImageInfo GetImageInfo() => this._imageInfoMethod();

            public Twain32.ExtImageInfo GetExtImageInfo(params TwEI[] extInfo)
            {
                return this._extImageInfoMethod(extInfo);
            }
        }

        [Serializable]
        public sealed class SetupMemXferEventArgs : Twain32.SerializableCancelEventArgs
        {
            internal SetupMemXferEventArgs(Twain32.ImageInfo info, uint bufferSize)
            {
                this.ImageInfo = info;
                this.BufferSize = bufferSize;
            }

            public Twain32.ImageInfo ImageInfo { get; private set; }

            public uint BufferSize { get; private set; }
        }

        [Serializable]
        public sealed class MemXferEventArgs : Twain32.SerializableCancelEventArgs
        {
            internal MemXferEventArgs(Twain32.ImageInfo info, Twain32.ImageMemXfer image)
            {
                this.ImageInfo = info;
                this.ImageMemXfer = image;
            }

            public Twain32.ImageInfo ImageInfo { get; private set; }

            public Twain32.ImageMemXfer ImageMemXfer { get; private set; }
        }

        [Serializable]
        public sealed class SetupFileXferEventArgs : Twain32.SerializableCancelEventArgs
        {
            internal SetupFileXferEventArgs()
            {
            }

            public string FileName { get; set; }
        }

        [Serializable]
        public sealed class FileXferEventArgs : Twain32.SerializableCancelEventArgs
        {
            internal FileXferEventArgs(Twain32.ImageFileXfer image) => this.ImageFileXfer = image;

            public Twain32.ImageFileXfer ImageFileXfer { get; private set; }
        }

        [Serializable]
        public sealed class TwainStateEventArgs : EventArgs
        {
            internal TwainStateEventArgs(Twain32.TwainStateFlag flags) => this.TwainState = flags;

            public Twain32.TwainStateFlag TwainState { get; private set; }
        }

        public sealed class DeviceEventEventArgs : EventArgs
        {
            private TwDeviceEvent _deviceEvent;

            internal DeviceEventEventArgs(TwDeviceEvent deviceEvent) => this._deviceEvent = deviceEvent;

            public TwDE Event => this._deviceEvent.Event;

            public string DeviceName => (string)this._deviceEvent.DeviceName;

            public uint BatteryMinutes => this._deviceEvent.BatteryMinutes;

            public short BatteryPercentAge => this._deviceEvent.BatteryPercentAge;

            public int PowerSupply => this._deviceEvent.PowerSupply;

            public float XResolution => (float)this._deviceEvent.XResolution;

            public float YResolution => (float)this._deviceEvent.YResolution;

            public uint FlashUsed2 => this._deviceEvent.FlashUsed2;

            public uint AutomaticCapture => this._deviceEvent.AutomaticCapture;

            public uint TimeBeforeFirstCapture => this._deviceEvent.TimeBeforeFirstCapture;

            public uint TimeBetweenCaptures => this._deviceEvent.TimeBetweenCaptures;
        }

        [Serializable]
        public sealed class AcquireErrorEventArgs : EventArgs
        {
            internal AcquireErrorEventArgs(TwainException ex) => this.Exception = ex;

            public TwainException Exception { get; private set; }
        }

        [Serializable]
        public class SerializableCancelEventArgs : EventArgs
        {
            public bool Cancel { get; set; }
        }

        private sealed class _DsmEntry
        {
            private _DsmEntry(IntPtr ptr)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        this.DsmParent = new Twain32._DSMparent(Twain32._DsmEntry._LinuxDsmParent);
                        this.DsmRaw = new Twain32._DSMraw(Twain32._DsmEntry._LinuxDsmRaw);
                        this.DSImageXfer = new Twain32._DSixfer(Twain32._DsmEntry._LinuxDsImageXfer);
                        this.DsRaw = new Twain32._DSraw(Twain32._DsmEntry._LinuxDsRaw);
                        break;
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        MethodInfo method = typeof(Twain32._DsmEntry).GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.NonPublic);
                        foreach (PropertyInfo property in typeof(Twain32._DsmEntry).GetProperties())
                            property.SetValue((object)this, method.MakeGenericMethod(property.PropertyType).Invoke((object)this, new object[1]
                            {
                (object) ptr
                            }), (object[])null);
                        break;
                }
            }

            public static Twain32._DsmEntry Create(IntPtr ptr) => new Twain32._DsmEntry(ptr);

            private static T CreateDelegate<T>(IntPtr ptr) where T : class
            {
                return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
            }

            public TwRC DsmInvoke<T>(TwIdentity origin, TwDG dg, TwDAT dat, TwMSG msg, ref T data) where T : class
            {
                if ((object)data == null)
                    throw new ArgumentNullException();
                IntPtr num1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
                try
                {
                    Marshal.StructureToPtr((object)data, num1, true);
                    int num2 = (int)this.DsmRaw(origin, IntPtr.Zero, dg, dat, msg, num1);
                    if (num2 == 0)
                        data = (T)Marshal.PtrToStructure(num1, typeof(T));
                    return (TwRC)num2;
                }
                finally
                {
                    Marshal.FreeHGlobal(num1);
                }
            }

            public TwRC DsInvoke<T>(
              TwIdentity origin,
              TwIdentity dest,
              TwDG dg,
              TwDAT dat,
              TwMSG msg,
              ref T data)
              where T : class
            {
                if ((object)data == null)
                    throw new ArgumentNullException();
                IntPtr num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
                try
                {
                    Marshal.StructureToPtr((object)data, num, true);
                    TwRC twRc = this.DsRaw(origin, dest, dg, dat, msg, num);
                    switch (twRc)
                    {
                        case TwRC.Success:
                        case TwRC.DSEvent:
                        case TwRC.XferDone:
                            data = (T)Marshal.PtrToStructure(num, typeof(T));
                            break;
                    }
                    return twRc;
                }
                finally
                {
                    Marshal.FreeHGlobal(num);
                }
            }

            public Twain32._DSMparent DsmParent { get; private set; }

            public Twain32._DSMraw DsmRaw { get; private set; }

            public Twain32._DSixfer DSImageXfer { get; private set; }

            public Twain32._DSraw DsRaw { get; private set; }

            [DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry", CharSet = CharSet.Ansi)]
            private static extern TwRC _LinuxDsmParent(
              [In, Out] TwIdentity origin,
              IntPtr zeroptr,
              TwDG dg,
              TwDAT dat,
              TwMSG msg,
              ref IntPtr refptr);

            [DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry", CharSet = CharSet.Ansi)]
            private static extern TwRC _LinuxDsmRaw(
              [In, Out] TwIdentity origin,
              IntPtr zeroptr,
              TwDG dg,
              TwDAT dat,
              TwMSG msg,
              IntPtr rawData);

            [DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry", CharSet = CharSet.Ansi)]
            private static extern TwRC _LinuxDsImageXfer(
              [In, Out] TwIdentity origin,
              [In, Out] TwIdentity dest,
              TwDG dg,
              TwDAT dat,
              TwMSG msg,
              ref IntPtr hbitmap);

            [DllImport("/usr/local/lib/libtwaindsm.so", EntryPoint = "DSM_Entry", CharSet = CharSet.Ansi)]
            private static extern TwRC _LinuxDsRaw(
              [In, Out] TwIdentity origin,
              [In, Out] TwIdentity dest,
              TwDG dg,
              TwDAT dat,
              TwMSG msg,
              IntPtr arg);
        }

        internal sealed class _Memory
        {
            private static TwEntryPoint _entryPoint;

            public static IntPtr Alloc(int size)
            {
                if (Twain32._Memory._entryPoint != null && Twain32._Memory._entryPoint.MemoryAllocate != null)
                    return Twain32._Memory._entryPoint.MemoryAllocate(size);
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        throw new NotSupportedException();
                    default:
                        return Twain32._Memory.GlobalAlloc(66, size);
                }
            }

            public static void Free(IntPtr handle)
            {
                if (Twain32._Memory._entryPoint != null && Twain32._Memory._entryPoint.MemoryFree != null)
                {
                    Twain32._Memory._entryPoint.MemoryFree(handle);
                }
                else
                {
                    switch (Environment.OSVersion.Platform)
                    {
                        case PlatformID.Unix:
                        case PlatformID.MacOSX:
                            throw new NotSupportedException();
                        default:
                            Twain32._Memory.GlobalFree(handle);
                            break;
                    }
                }
            }

            public static IntPtr Lock(IntPtr handle)
            {
                if (Twain32._Memory._entryPoint != null && Twain32._Memory._entryPoint.MemoryLock != null)
                    return Twain32._Memory._entryPoint.MemoryLock(handle);
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        throw new NotSupportedException();
                    default:
                        return Twain32._Memory.GlobalLock(handle);
                }
            }

            public static void Unlock(IntPtr handle)
            {
                if (Twain32._Memory._entryPoint != null && Twain32._Memory._entryPoint.MemoryUnlock != null)
                {
                    Twain32._Memory._entryPoint.MemoryUnlock(handle);
                }
                else
                {
                    switch (Environment.OSVersion.Platform)
                    {
                        case PlatformID.Unix:
                        case PlatformID.MacOSX:
                            throw new NotSupportedException();
                        default:
                            Twain32._Memory.GlobalUnlock(handle);
                            break;
                    }
                }
            }

            public static void ZeroMemory(IntPtr dest, IntPtr size)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        byte[] source = new byte[size.ToInt32()];
                        Marshal.Copy(source, 0, dest, source.Length);
                        break;
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        Twain32._Memory._ZeroMemory(dest, size);
                        break;
                }
            }

            internal static void _SetEntryPoints(TwEntryPoint entry)
            {
                Twain32._Memory._entryPoint = entry;
            }

            [DllImport("kernel32.dll")]
            private static extern IntPtr GlobalAlloc(int flags, int size);

            [DllImport("kernel32.dll")]
            private static extern IntPtr GlobalLock(IntPtr handle);

            [DllImport("kernel32.dll")]
            private static extern bool GlobalUnlock(IntPtr handle);

            [DllImport("kernel32.dll")]
            private static extern IntPtr GlobalFree(IntPtr handle);

            [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory")]
            private static extern void _ZeroMemory(IntPtr dest, IntPtr size);
        }

        internal sealed class _Platform
        {
            internal static IntPtr Load(string fileName)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        return Twain32._Platform.dlopen(fileName, 1);
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        return Twain32._Platform.LoadLibrary(fileName);
                }
            }

            internal static void Unload(IntPtr hModule)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        Twain32._Platform.dlclose(hModule);
                        break;
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        Twain32._Platform.FreeLibrary(hModule);
                        break;
                }
            }

            internal static IntPtr GetProcAddr(IntPtr hModule, string procName)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        return Twain32._Platform.dlsym(hModule, procName);
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        return Twain32._Platform.GetProcAddress(hModule, procName);
                }
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr LoadLibrary(string fileName);

            [DllImport("kernel32.dll")]
            private static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
            private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("libdl.so")]
            private static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("libdl.so")]
            private static extern bool dlclose(IntPtr hModule);

            [DllImport("libdl.so")]
            private static extern IntPtr dlsym(IntPtr hModule, string procName);
        }

        private sealed class _MessageFilter : IMessageFilter, IDisposable
        {
            private Twain32 _twain;
            private bool _is_set_filter;
            private TwEvent _evtmsg = new TwEvent();

            public _MessageFilter(Twain32 twain)
            {
                this._twain = twain;
                this._evtmsg.EventPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Twain32._MessageFilter.WINMSG)));
            }

            public bool PreFilterMessage(ref Message m)
            {
                try
                {
                    if (this._twain._srcds.Id == 0U)
                        return false;
                    Marshal.StructureToPtr((object)new Twain32._MessageFilter.WINMSG()
                    {
                        hwnd = m.HWnd,
                        message = m.Msg,
                        wParam = m.WParam,
                        lParam = m.LParam
                    }, this._evtmsg.EventPtr, true);
                    this._evtmsg.Message = TwMSG.Null;
                    switch (this._twain._dsmEntry.DsInvoke<TwEvent>(this._twain._AppId, this._twain._srcds, TwDG.Control, TwDAT.Event, TwMSG.ProcessEvent, ref this._evtmsg))
                    {
                        case TwRC.Failure:
                            throw new TwainException(this._twain._GetTwainStatus(), TwRC.Failure);
                        case TwRC.DSEvent:
                            this._twain._TwCallbackProcCore(this._evtmsg.Message, (Twain32.Action<bool>)(isCloseReq =>
                            {
                                if (!isCloseReq && !this._twain.DisableAfterAcquire)
                                    return;
                                this._RemoveFilter();
                                this._twain._DisableDataSource();
                            }));
                            break;
                        case TwRC.NotDSEvent:
                            return false;
                        default:
                            throw new InvalidOperationException("Получен неверный код результата операции. Invalid a Return Code value.");
                    }
                }
                catch (TwainException ex)
                {
                    this._twain._OnAcquireError(new Twain32.AcquireErrorEventArgs(ex));
                }
                catch (Exception ex)
                {
                    this._twain._OnAcquireError(new Twain32.AcquireErrorEventArgs(new TwainException(ex.Message, ex)));
                }
                return true;
            }

            public void Dispose()
            {
                if (this._evtmsg == null || !(this._evtmsg.EventPtr != IntPtr.Zero))
                    return;
                Marshal.FreeHGlobal(this._evtmsg.EventPtr);
                this._evtmsg.EventPtr = IntPtr.Zero;
            }

            public void SetFilter()
            {
                if (this._is_set_filter)
                    return;
                this._is_set_filter = true;
                Application.AddMessageFilter((IMessageFilter)this);
            }

            private void _RemoveFilter()
            {
                Application.RemoveMessageFilter((IMessageFilter)this);
                this._is_set_filter = false;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 2)]
            internal struct WINMSG
            {
                public IntPtr hwnd;
                public int message;
                public IntPtr wParam;
                public IntPtr lParam;
            }
        }

        [Serializable]
        private sealed class _Image
        {
            private Stream _stream;
            [NonSerialized]
            private Image _image;
            //[NonSerialized]
            //private BitmapImage _image2;

            private _Image()
            {
            }

            public static implicit operator Twain32._Image(Stream stream)
            {
                return new Twain32._Image() { _stream = stream };
            }

            public static implicit operator Image(Twain32._Image value)
            {
                if (value._image == null)
                {
                    value._stream.Seek(0L, SeekOrigin.Begin);
                    value._image = Image.FromStream(value._stream);
                }
                return value._image;
            }

            //public static implicit operator ImageSource(Twain32._Image value)
            //{
            //    if (value._image2 == null)
            //    {
            //        value._stream.Seek(0L, SeekOrigin.Begin);
            //        value._image2 = new BitmapImage();
            //        value._image2.BeginInit();
            //        value._image2.StreamSource = value._stream;
            //        value._image2.CacheOption = BitmapCacheOption.OnLoad;
            //        value._image2.EndInit();
            //        value._image2.Freeze();
            //    }
            //    return (ImageSource)value._image2;
            //}
        }

        [Serializable]
        public sealed class Range
        {
            private Range()
            {
            }

            private Range(TwRange range)
            {
                this.MinValue = TwTypeHelper.CastToCommon(range.ItemType, TwTypeHelper.ValueToTw<uint>(range.ItemType, range.MinValue));
                this.MaxValue = TwTypeHelper.CastToCommon(range.ItemType, TwTypeHelper.ValueToTw<uint>(range.ItemType, range.MaxValue));
                this.StepSize = TwTypeHelper.CastToCommon(range.ItemType, TwTypeHelper.ValueToTw<uint>(range.ItemType, range.StepSize));
                this.CurrentValue = TwTypeHelper.CastToCommon(range.ItemType, TwTypeHelper.ValueToTw<uint>(range.ItemType, range.CurrentValue));
                this.DefaultValue = TwTypeHelper.CastToCommon(range.ItemType, TwTypeHelper.ValueToTw<uint>(range.ItemType, range.DefaultValue));
            }

            internal static Twain32.Range CreateRange(TwRange range) => new Twain32.Range(range);

            public static Twain32.Range CreateRange(
              object minValue,
              object maxValue,
              object stepSize,
              object defaultValue,
              object currentValue)
            {
                return new Twain32.Range()
                {
                    MinValue = minValue,
                    MaxValue = maxValue,
                    StepSize = stepSize,
                    DefaultValue = defaultValue,
                    CurrentValue = currentValue
                };
            }

            public object MinValue { get; set; }

            public object MaxValue { get; set; }

            public object StepSize { get; set; }

            public object DefaultValue { get; set; }

            public object CurrentValue { get; set; }

            internal TwRange ToTwRange()
            {
                TwType type = TwTypeHelper.TypeOf(this.CurrentValue.GetType());
                return new TwRange()
                {
                    ItemType = type,
                    MinValue = TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(type, this.MinValue)),
                    MaxValue = TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(type, this.MaxValue)),
                    StepSize = TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(type, this.StepSize)),
                    DefaultValue = TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(type, this.DefaultValue)),
                    CurrentValue = TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(type, this.CurrentValue))
                };
            }
        }

        [Serializable]
        public sealed class Enumeration
        {
            private object[] _items;

            private Enumeration(object[] items, int currentIndex, int defaultIndex)
            {
                this._items = items;
                this.CurrentIndex = currentIndex;
                this.DefaultIndex = defaultIndex;
            }

            public static Twain32.Enumeration CreateEnumeration(
              object[] items,
              int currentIndex,
              int defaultIndex)
            {
                return new Twain32.Enumeration(items, currentIndex, defaultIndex);
            }

            public int Count => this._items.Length;

            public int CurrentIndex { get; private set; }

            public int DefaultIndex { get; private set; }

            public object this[int index]
            {
                get => this._items[index];
                internal set => this._items[index] = value;
            }

            internal object[] Items => this._items;

            public static Twain32.Enumeration FromRange(Twain32.Range value)
            {
                int currentIndex = 0;
                int defaultIndex = 0;
                object[] items = new object[(int)(((double)Convert.ToSingle(value.MaxValue) - (double)Convert.ToSingle(value.MinValue)) / (double)Convert.ToSingle(value.StepSize))];
                for (int index = 0; index < items.Length; ++index)
                {
                    items[index] = (object)(float)((double)Convert.ToSingle(value.MinValue) + (double)Convert.ToSingle(value.StepSize) * (double)index);
                    if ((double)Convert.ToSingle(items[index]) == (double)Convert.ToSingle(value.CurrentValue))
                        currentIndex = index;
                    if ((double)Convert.ToSingle(items[index]) == (double)Convert.ToSingle(value.DefaultValue))
                        defaultIndex = index;
                }
                return Twain32.Enumeration.CreateEnumeration(items, currentIndex, defaultIndex);
            }

            public static Twain32.Enumeration FromArray(object[] value)
            {
                return Twain32.Enumeration.CreateEnumeration(value, 0, 0);
            }

            public static Twain32.Enumeration FromOneValue(ValueType value)
            {
                return Twain32.Enumeration.CreateEnumeration(new object[1]
                {
          (object) value
                }, 0, 0);
            }

            internal static Twain32.Enumeration FromObject(object value)
            {
                switch (value)
                {
                    case Twain32.Range _:
                        return Twain32.Enumeration.FromRange((Twain32.Range)value);
                    case object[] _:
                        return Twain32.Enumeration.FromArray((object[])value);
                    case ValueType _:
                        return Twain32.Enumeration.FromOneValue((ValueType)value);
                    case string _:
                        return Twain32.Enumeration.CreateEnumeration(new object[1]
                        {
              value
                        }, 0, 0);
                    default:
                        return value as Twain32.Enumeration;
                }
            }
        }

        [Serializable]
        public sealed class ImageInfo
        {
            private ImageInfo()
            {
            }

            internal static Twain32.ImageInfo FromTwImageInfo(TwImageInfo info)
            {
                return new Twain32.ImageInfo()
                {
                    BitsPerPixel = info.BitsPerPixel,
                    BitsPerSample = Twain32.ImageInfo._Copy(info.BitsPerSample, (int)info.SamplesPerPixel),
                    Compression = info.Compression,
                    ImageLength = info.ImageLength,
                    ImageWidth = info.ImageWidth,
                    PixelType = info.PixelType,
                    Planar = (bool)info.Planar,
                    XResolution = (float)info.XResolution,
                    YResolution = (float)info.YResolution
                };
            }

            private static short[] _Copy(short[] array, int len)
            {
                short[] numArray = new short[len];
                for (int index = 0; index < len; ++index)
                    numArray[index] = array[index];
                return numArray;
            }

            public float XResolution { get; private set; }

            public float YResolution { get; private set; }

            public int ImageWidth { get; private set; }

            public int ImageLength { get; private set; }

            public short[] BitsPerSample { get; private set; }

            public short BitsPerPixel { get; private set; }

            public bool Planar { get; private set; }

            public TwPixelType PixelType { get; private set; }

            public TwCompression Compression { get; private set; }
        }

        [Serializable]
        public sealed class ExtImageInfo : Collection<Twain32.ExtImageInfo.InfoItem>
        {
            private ExtImageInfo()
            {
            }

            internal static Twain32.ExtImageInfo FromPtr(IntPtr ptr)
            {
                int num1 = Marshal.SizeOf(typeof(TwExtImageInfo));
                int num2 = Marshal.SizeOf(typeof(TwInfo));
                TwExtImageInfo structure1 = Marshal.PtrToStructure(ptr, typeof(TwExtImageInfo)) as TwExtImageInfo;
                Twain32.ExtImageInfo extImageInfo = new Twain32.ExtImageInfo();
                for (int index = 0; (long)index < (long)structure1.NumInfos; ++index)
                {
                    using (TwInfo structure2 = Marshal.PtrToStructure((IntPtr)(ptr.ToInt64() + (long)num1 + (long)(num2 * index)), typeof(TwInfo)) as TwInfo)
                        extImageInfo.Add(Twain32.ExtImageInfo.InfoItem.FromTwInfo(structure2));
                }
                return extImageInfo;
            }

            public Twain32.ExtImageInfo.InfoItem this[TwEI infoId]
            {
                get
                {
                    foreach (Twain32.ExtImageInfo.InfoItem infoItem in (Collection<Twain32.ExtImageInfo.InfoItem>)this)
                    {
                        if (infoItem.InfoId == infoId)
                            return infoItem;
                    }
                    throw new KeyNotFoundException();
                }
            }

            [DebuggerDisplay("InfoId = {InfoId}, IsSuccess = {IsSuccess}, Value = {Value}")]
            [Serializable]
            public sealed class InfoItem
            {
                private InfoItem()
                {
                }

                internal static Twain32.ExtImageInfo.InfoItem FromTwInfo(TwInfo info)
                {
                    return new Twain32.ExtImageInfo.InfoItem()
                    {
                        InfoId = info.InfoId,
                        IsNotSupported = info.ReturnCode == TwRC.InfoNotSupported,
                        IsNotAvailable = info.ReturnCode == TwRC.DataNotAvailable,
                        IsSuccess = info.ReturnCode == TwRC.Success,
                        Value = info.GetValue()
                    };
                }

                public TwEI InfoId { get; private set; }

                public bool IsNotSupported { get; private set; }

                public bool IsNotAvailable { get; private set; }

                public bool IsSuccess { get; private set; }

                public object Value { get; private set; }
            }
        }

        [Serializable]
        public sealed class ImageMemXfer
        {
            private ImageMemXfer()
            {
            }

            internal static Twain32.ImageMemXfer Create(TwImageMemXfer data)
            {
                Twain32.ImageMemXfer imageMemXfer = new Twain32.ImageMemXfer()
                {
                    BytesPerRow = data.BytesPerRow,
                    Columns = data.Columns,
                    Compression = data.Compression,
                    Rows = data.Rows,
                    XOffset = data.XOffset,
                    YOffset = data.YOffset
                };
                if ((data.Memory.Flags & TwMF.Handle) != (TwMF)0)
                {
                    IntPtr source = Twain32._Memory.Lock(data.Memory.TheMem);
                    try
                    {
                        imageMemXfer.ImageData = new byte[(int)data.BytesWritten];
                        Marshal.Copy(source, imageMemXfer.ImageData, 0, imageMemXfer.ImageData.Length);
                    }
                    finally
                    {
                        Twain32._Memory.Unlock(data.Memory.TheMem);
                    }
                }
                else
                {
                    imageMemXfer.ImageData = new byte[(int)data.BytesWritten];
                    Marshal.Copy(data.Memory.TheMem, imageMemXfer.ImageData, 0, imageMemXfer.ImageData.Length);
                }
                return imageMemXfer;
            }

            public TwCompression Compression { get; private set; }

            public uint BytesPerRow { get; private set; }

            public uint Columns { get; private set; }

            public uint Rows { get; private set; }

            public uint XOffset { get; private set; }

            public uint YOffset { get; private set; }

            public byte[] ImageData { get; private set; }
        }

        [Serializable]
        public sealed class ImageFileXfer
        {
            private ImageFileXfer()
            {
            }

            internal static Twain32.ImageFileXfer Create(TwSetupFileXfer data)
            {
                return new Twain32.ImageFileXfer()
                {
                    FileName = (string)data.FileName,
                    Format = data.Format
                };
            }

            public string FileName { get; private set; }

            public TwFF Format { get; private set; }
        }

        public sealed class TwainPalette : MarshalByRefObject
        {
            private Twain32 _twain;

            internal TwainPalette(Twain32 twain) => this._twain = twain;

            public Twain32.ColorPalette Get()
            {
                TwPalette8 data = new TwPalette8();
                TwRC rc = this._twain._dsmEntry.DsInvoke<TwPalette8>(this._twain._AppId, this._twain._srcds, TwDG.Image, TwDAT.Palette8, TwMSG.Get, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._twain._GetTwainStatus(), rc);
                return (Twain32.ColorPalette)data;
            }

            public Twain32.ColorPalette GetDefault()
            {
                TwPalette8 data = new TwPalette8();
                TwRC rc = this._twain._dsmEntry.DsInvoke<TwPalette8>(this._twain._AppId, this._twain._srcds, TwDG.Image, TwDAT.Palette8, TwMSG.GetDefault, ref data);
                if (rc != TwRC.Success)
                    throw new TwainException(this._twain._GetTwainStatus(), rc);
                return (Twain32.ColorPalette)data;
            }

            public void Reset(Twain32.ColorPalette palette)
            {
                TwRC rc = this._twain._dsmEntry.DsInvoke<Twain32.ColorPalette>(this._twain._AppId, this._twain._srcds, TwDG.Image, TwDAT.Palette8, TwMSG.Reset, ref palette);
                if (rc != TwRC.Success)
                    throw new TwainException(this._twain._GetTwainStatus(), rc);
            }

            public void Set(Twain32.ColorPalette palette)
            {
                TwRC rc = this._twain._dsmEntry.DsInvoke<Twain32.ColorPalette>(this._twain._AppId, this._twain._srcds, TwDG.Image, TwDAT.Palette8, TwMSG.Set, ref palette);
                if (rc != TwRC.Success)
                    throw new TwainException(this._twain._GetTwainStatus(), rc);
            }
        }

        [Serializable]
        public sealed class ColorPalette
        {
            private ColorPalette()
            {
            }

            internal static Twain32.ColorPalette Create(TwPalette8 palette)
            {
                Twain32.ColorPalette colorPalette = new Twain32.ColorPalette()
                {
                    PaletteType = palette.PaletteType,
                    Colors = new System.Drawing.Color[(int)palette.NumColors]
                };
                for (int index = 0; index < (int)palette.NumColors; ++index)
                    colorPalette.Colors[index] = (System.Drawing.Color)palette.Colors[index];
                return colorPalette;
            }

            public TwPA PaletteType { get; private set; }

            public System.Drawing.Color[] Colors { get; private set; }
        }

        [DebuggerDisplay("{Name}, Version = {Version}")]
        [Serializable]
        public sealed class Identity
        {
            internal Identity(TwIdentity identity)
            {
                this.Family = (string)identity.ProductFamily;
                this.Manufacturer = (string)identity.Manufacturer;
                this.Name = (string)identity.ProductName;
                this.ProtocolVersion = new Version((int)identity.ProtocolMajor, (int)identity.ProtocolMinor);
                this.Version = new Version((int)identity.Version.MajorNum, (int)identity.Version.MinorNum);
            }

            public Version Version { get; private set; }

            public Version ProtocolVersion { get; private set; }

            public string Manufacturer { get; private set; }

            public string Family { get; private set; }

            public string Name { get; private set; }
        }

        private delegate TwRC _DSMparent(
          [In, Out] TwIdentity origin,
          IntPtr zeroptr,
          TwDG dg,
          TwDAT dat,
          TwMSG msg,
          ref IntPtr refptr);

        private delegate TwRC _DSMraw(
          [In, Out] TwIdentity origin,
          IntPtr zeroptr,
          TwDG dg,
          TwDAT dat,
          TwMSG msg,
          IntPtr rawData);

        private delegate TwRC _DSixfer(
          [In, Out] TwIdentity origin,
          [In, Out] TwIdentity dest,
          TwDG dg,
          TwDAT dat,
          TwMSG msg,
          ref IntPtr hbitmap);

        private delegate TwRC _DSraw(
          [In, Out] TwIdentity origin,
          [In, Out] TwIdentity dest,
          TwDG dg,
          TwDAT dat,
          TwMSG msg,
          IntPtr arg);

        internal delegate Twain32.ImageInfo GetImageInfoCallback();

        internal delegate Twain32.ExtImageInfo GetExtImageInfoCallback(TwEI[] extInfo);

        private delegate void Action<T>(T arg);
    }
}

