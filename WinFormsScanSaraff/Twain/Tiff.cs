using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsScanSaraff.Twain
{
    internal sealed class Tiff
    {
        private Tiff.TiffHeader _header;
        private Tiff.Ifd _ifd;

        private static Tiff FromPtr(IntPtr ptr)
        {
            Tiff tiff = new Tiff()
            {
                _header = (Tiff.TiffHeader)Marshal.PtrToStructure(ptr, typeof(Tiff.TiffHeader))
            };
            if (tiff._header.magic == Tiff.MagicValues.BigEndian)
                throw new NotSupportedException();
            tiff._ifd = Tiff.Ifd.FromPtr((IntPtr)(ptr.ToInt64() + (long)tiff._header.dirOffset), ptr);
            return tiff;
        }

        public static Stream FromPtrToImage(IntPtr ptr)
        {
            byte[] numArray = new byte[Tiff.FromPtr(ptr)._GetSize()];
            Marshal.Copy(ptr, numArray, 0, numArray.Length);
            return (Stream)new MemoryStream(numArray);
        }

        private int _GetSize()
        {
            int size = 0;
            for (Tiff.Ifd ifd = this.ImageFileDirectory; ifd != null; ifd = ifd.NextIfd)
            {
                if (ifd.Offset + ifd.Size > size)
                    size = ifd.Offset + ifd.Size;
                Tiff.IfdEntry ifdEntry1 = (Tiff.IfdEntry)null;
                Tiff.IfdEntry ifdEntry2 = (Tiff.IfdEntry)null;
                Tiff.IfdEntry ifdEntry3 = (Tiff.IfdEntry)null;
                Tiff.IfdEntry ifdEntry4 = (Tiff.IfdEntry)null;
                Tiff.IfdEntry ifdEntry5 = (Tiff.IfdEntry)null;
                Tiff.IfdEntry ifdEntry6 = (Tiff.IfdEntry)null;
                foreach (Tiff.IfdEntry ifdEntry7 in (Collection<Tiff.IfdEntry>)ifd)
                {
                    if (ifdEntry7.DataOffset + ifdEntry7.DataSize > size)
                        size = ifdEntry7.DataOffset + ifdEntry7.DataSize;
                    Tiff.TiffTags tag = ifdEntry7.Tag;
                    if ((uint)tag <= 288U)
                    {
                        switch (tag)
                        {
                            case Tiff.TiffTags.STRIPOFFSETS:
                                ifdEntry1 = ifdEntry7;
                                continue;
                            case Tiff.TiffTags.STRIPBYTECOUNTS:
                                ifdEntry2 = ifdEntry7;
                                continue;
                            case Tiff.TiffTags.FREEOFFSETS:
                                ifdEntry3 = ifdEntry7;
                                continue;
                            default:
                                continue;
                        }
                    }
                    else
                    {
                        switch (tag)
                        {
                            case Tiff.TiffTags.FREEBYTECOUNTS:
                                ifdEntry4 = ifdEntry7;
                                continue;
                            case Tiff.TiffTags.TILEOFFSETS:
                                ifdEntry5 = ifdEntry7;
                                continue;
                            case Tiff.TiffTags.TILEBYTECOUNTS:
                                ifdEntry6 = ifdEntry7;
                                continue;
                            default:
                                continue;
                        }
                    }
                }
                Tiff.IfdEntry[][] ifdEntryArray1 = new Tiff.IfdEntry[3][]
                {
          new Tiff.IfdEntry[2]{ ifdEntry1, ifdEntry2 },
          new Tiff.IfdEntry[2]{ ifdEntry3, ifdEntry4 },
          new Tiff.IfdEntry[2]{ ifdEntry5, ifdEntry6 }
                };
                foreach (Tiff.IfdEntry[] ifdEntryArray2 in ifdEntryArray1)
                {
                    if (ifdEntryArray2[0] != null && ifdEntryArray2[1] != null && ifdEntryArray2[0].Length == ifdEntryArray2[1].Length)
                    {
                        for (int index = 0; index < ifdEntryArray2[0].Length; ++index)
                        {
                            int int32_1 = Convert.ToInt32(ifdEntryArray2[0][index]);
                            int int32_2 = Convert.ToInt32(ifdEntryArray2[1][index]);
                            if (int32_1 + int32_2 > size)
                                size = int32_1 + int32_2;
                        }
                    }
                }
            }
            return size;
        }

        private Tiff.Ifd ImageFileDirectory => this._ifd;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private sealed class TiffHeader
        {
            [MarshalAs(UnmanagedType.U2)]
            public Tiff.MagicValues magic;
            public ushort version;
            public uint dirOffset;
        }

        private enum MagicValues : ushort
        {
            LittleEndian = 18761, // 0x4949
            BigEndian = 19789, // 0x4D4D
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private sealed class TiffDirEntry
        {
            [MarshalAs(UnmanagedType.U2)]
            public Tiff.TiffTags tag;
            [MarshalAs(UnmanagedType.U2)]
            public Tiff.TiffDataType type;
            public uint count;
            public uint offset;
        }

        [DebuggerDisplay("{Tag}; DataType = {DataType}; Length = {Length}")]
        private sealed class IfdEntry
        {
            private Array _data;

            public static Tiff.IfdEntry FromPtr(IntPtr ptr, IntPtr baseAddr)
            {
                Tiff.TiffDirEntry structure = (Tiff.TiffDirEntry)Marshal.PtrToStructure(ptr, typeof(Tiff.TiffDirEntry));
                Tiff.IfdEntry ifdEntry = new Tiff.IfdEntry()
                {
                    Tag = structure.tag,
                    DataType = structure.type,
                    _data = Array.CreateInstance(Tiff.TiffDataTypeHelper.Typeof(structure.type), structure.count <= (uint)int.MaxValue ? (long)structure.count : 0L)
                };
                IntPtr num1 = (IntPtr)((ifdEntry.DataSize = Tiff.TiffDataTypeHelper.Sizeof(structure.type) * ifdEntry._data.Length) > 4 ? baseAddr.ToInt64() + (long)structure.offset : ptr.ToInt64() + 8L);
                ifdEntry.DataOffset = (int)(num1.ToInt64() - baseAddr.ToInt64());
                int index = 0;
                int num2 = Tiff.TiffDataTypeHelper.Sizeof(structure.type);
                for (; index < ifdEntry._data.Length; ++index)
                    ifdEntry._data.SetValue(Marshal.PtrToStructure((IntPtr)(num1.ToInt64() + (long)(num2 * index)), Tiff.TiffDataTypeHelper.Typeof(structure.type)), index);
                return ifdEntry;
            }

            public Tiff.TiffTags Tag { get; private set; }

            public Tiff.TiffDataType DataType { get; private set; }

            public int DataOffset { get; private set; }

            public int DataSize { get; private set; }

            public int Length => this._data.Length;

            public object this[int index] => this._data.GetValue(index);
        }

        private sealed class Ifd : Collection<Tiff.IfdEntry>
        {
            public static Tiff.Ifd FromPtr(IntPtr ptr, IntPtr baseAddr)
            {
                Tiff.Ifd ifd = new Tiff.Ifd();
                ifd.Load(ptr, baseAddr);
                return ifd;
            }

            private void Load(IntPtr ptr, IntPtr baseAddr)
            {
                ushort structure1 = (ushort)Marshal.PtrToStructure(ptr, typeof(ushort));
                IntPtr ptr1 = (IntPtr)(ptr.ToInt64() + 2L);
                int num1 = 0;
                int num2 = Marshal.SizeOf(typeof(Tiff.TiffDirEntry));
                while (num1 < (int)structure1)
                {
                    this.Add(Tiff.IfdEntry.FromPtr(ptr1, baseAddr));
                    ++num1;
                    ptr1 = (IntPtr)(ptr1.ToInt64() + (long)num2);
                }
                int structure2 = (int)Marshal.PtrToStructure(ptr1, typeof(int));
                if (structure2 != 0)
                    this.NextIfd = Tiff.Ifd.FromPtr((IntPtr)(baseAddr.ToInt64() + (long)structure2), baseAddr);
                this.Offset = (int)(ptr.ToInt64() - baseAddr.ToInt64());
                this.Size = 6 + Marshal.SizeOf(typeof(Tiff.TiffDirEntry)) * (int)structure1;
            }

            public int Offset { get; private set; }

            public int Size { get; private set; }

            public Tiff.Ifd NextIfd { get; private set; }

            public Tiff.IfdEntry this[Tiff.TiffTags tag]
            {
                get
                {
                    for (int index = 0; index < this.Count; ++index)
                    {
                        if (this[index].Tag == tag)
                            return this[index];
                    }
                    throw new KeyNotFoundException();
                }
            }
        }

        private enum TiffDataType : ushort
        {
            TIFF_NOTYPE,
            TIFF_BYTE,
            TIFF_ASCII,
            TIFF_SHORT,
            TIFF_LONG,
            TIFF_RATIONAL,
            TIFF_SBYTE,
            TIFF_UNDEFINED,
            TIFF_SSHORT,
            TIFF_SLONG,
            TIFF_SRATIONAL,
            TIFF_FLOAT,
            TIFF_DOUBLE,
            TIFF_IFD,
        }

        private sealed class TiffDataTypeHelper
        {
            private static Dictionary<Tiff.TiffDataType, int> _sizeDictionary;
            private static Dictionary<Tiff.TiffDataType, Type> _typeDictionary;

            public static int Sizeof(Tiff.TiffDataType type)
            {
                try
                {
                    return Tiff.TiffDataTypeHelper.SizeDictionary[type];
                }
                catch (KeyNotFoundException ex)
                {
                    return 0;
                }
            }

            public static Type Typeof(Tiff.TiffDataType type)
            {
                try
                {
                    return Tiff.TiffDataTypeHelper.TypeDictionary[type];
                }
                catch (KeyNotFoundException ex)
                {
                    return typeof(object);
                }
            }

            private static Dictionary<Tiff.TiffDataType, int> SizeDictionary
            {
                get
                {
                    if (Tiff.TiffDataTypeHelper._sizeDictionary == null)
                        Tiff.TiffDataTypeHelper._sizeDictionary = new Dictionary<Tiff.TiffDataType, int>()
            {
              {
                Tiff.TiffDataType.TIFF_NOTYPE,
                0
              },
              {
                Tiff.TiffDataType.TIFF_BYTE,
                1
              },
              {
                Tiff.TiffDataType.TIFF_ASCII,
                1
              },
              {
                Tiff.TiffDataType.TIFF_SHORT,
                2
              },
              {
                Tiff.TiffDataType.TIFF_LONG,
                4
              },
              {
                Tiff.TiffDataType.TIFF_RATIONAL,
                8
              },
              {
                Tiff.TiffDataType.TIFF_SBYTE,
                1
              },
              {
                Tiff.TiffDataType.TIFF_UNDEFINED,
                1
              },
              {
                Tiff.TiffDataType.TIFF_SSHORT,
                2
              },
              {
                Tiff.TiffDataType.TIFF_SLONG,
                4
              },
              {
                Tiff.TiffDataType.TIFF_SRATIONAL,
                8
              },
              {
                Tiff.TiffDataType.TIFF_FLOAT,
                4
              },
              {
                Tiff.TiffDataType.TIFF_DOUBLE,
                8
              },
              {
                Tiff.TiffDataType.TIFF_IFD,
                4
              }
            };
                    return Tiff.TiffDataTypeHelper._sizeDictionary;
                }
            }

            private static Dictionary<Tiff.TiffDataType, Type> TypeDictionary
            {
                get
                {
                    if (Tiff.TiffDataTypeHelper._typeDictionary == null)
                        Tiff.TiffDataTypeHelper._typeDictionary = new Dictionary<Tiff.TiffDataType, Type>()
            {
              {
                Tiff.TiffDataType.TIFF_NOTYPE,
                typeof (object)
              },
              {
                Tiff.TiffDataType.TIFF_BYTE,
                typeof (byte)
              },
              {
                Tiff.TiffDataType.TIFF_ASCII,
                typeof (byte)
              },
              {
                Tiff.TiffDataType.TIFF_SHORT,
                typeof (ushort)
              },
              {
                Tiff.TiffDataType.TIFF_LONG,
                typeof (uint)
              },
              {
                Tiff.TiffDataType.TIFF_RATIONAL,
                typeof (ulong)
              },
              {
                Tiff.TiffDataType.TIFF_SBYTE,
                typeof (sbyte)
              },
              {
                Tiff.TiffDataType.TIFF_UNDEFINED,
                typeof (byte)
              },
              {
                Tiff.TiffDataType.TIFF_SSHORT,
                typeof (short)
              },
              {
                Tiff.TiffDataType.TIFF_SLONG,
                typeof (int)
              },
              {
                Tiff.TiffDataType.TIFF_SRATIONAL,
                typeof (long)
              },
              {
                Tiff.TiffDataType.TIFF_FLOAT,
                typeof (float)
              },
              {
                Tiff.TiffDataType.TIFF_DOUBLE,
                typeof (double)
              },
              {
                Tiff.TiffDataType.TIFF_IFD,
                typeof (int)
              }
            };
                    return Tiff.TiffDataTypeHelper._typeDictionary;
                }
            }
        }

        private enum TiffTags : ushort
        {
            SUBFILETYPE = 254, // 0x00FE
            OSUBFILETYPE = 255, // 0x00FF
            IMAGEWIDTH = 256, // 0x0100
            IMAGELENGTH = 257, // 0x0101
            BITSPERSAMPLE = 258, // 0x0102
            COMPRESSION = 259, // 0x0103
            PHOTOMETRIC = 262, // 0x0106
            THRESHHOLDING = 263, // 0x0107
            CELLWIDTH = 264, // 0x0108
            CELLLENGTH = 265, // 0x0109
            FILLORDER = 266, // 0x010A
            DOCUMENTNAME = 269, // 0x010D
            IMAGEDESCRIPTION = 270, // 0x010E
            MAKE = 271, // 0x010F
            MODEL = 272, // 0x0110
            STRIPOFFSETS = 273, // 0x0111
            ORIENTATION = 274, // 0x0112
            SAMPLESPERPIXEL = 277, // 0x0115
            ROWSPERSTRIP = 278, // 0x0116
            STRIPBYTECOUNTS = 279, // 0x0117
            MINSAMPLEVALUE = 280, // 0x0118
            MAXSAMPLEVALUE = 281, // 0x0119
            XRESOLUTION = 282, // 0x011A
            YRESOLUTION = 283, // 0x011B
            PLANARCONFIG = 284, // 0x011C
            PAGENAME = 285, // 0x011D
            XPOSITION = 286, // 0x011E
            YPOSITION = 287, // 0x011F
            FREEOFFSETS = 288, // 0x0120
            FREEBYTECOUNTS = 289, // 0x0121
            GRAYRESPONSEUNIT = 290, // 0x0122
            GRAYRESPONSECURVE = 291, // 0x0123
            GROUP3OPTIONS = 292, // 0x0124
            T4OPTIONS = 292, // 0x0124
            GROUP4OPTIONS = 293, // 0x0125
            T6OPTIONS = 293, // 0x0125
            RESOLUTIONUNIT = 296, // 0x0128
            PAGENUMBER = 297, // 0x0129
            COLORRESPONSEUNIT = 300, // 0x012C
            TRANSFERFUNCTION = 301, // 0x012D
            SOFTWARE = 305, // 0x0131
            DATETIME = 306, // 0x0132
            ARTIST = 315, // 0x013B
            HOSTCOMPUTER = 316, // 0x013C
            PREDICTOR = 317, // 0x013D
            WHITEPOINT = 318, // 0x013E
            PRIMARYCHROMATICITIES = 319, // 0x013F
            COLORMAP = 320, // 0x0140
            HALFTONEHINTS = 321, // 0x0141
            TILEWIDTH = 322, // 0x0142
            TILELENGTH = 323, // 0x0143
            TILEOFFSETS = 324, // 0x0144
            TILEBYTECOUNTS = 325, // 0x0145
            BADFAXLINES = 326, // 0x0146
            CLEANFAXDATA = 327, // 0x0147
            CONSECUTIVEBADFAXLINES = 328, // 0x0148
            SUBIFD = 330, // 0x014A
            INKSET = 332, // 0x014C
            INKNAMES = 333, // 0x014D
            NUMBEROFINKS = 334, // 0x014E
            DOTRANGE = 336, // 0x0150
            TARGETPRINTER = 337, // 0x0151
            EXTRASAMPLES = 338, // 0x0152
            SAMPLEFORMAT = 339, // 0x0153
            SMINSAMPLEVALUE = 340, // 0x0154
            SMAXSAMPLEVALUE = 341, // 0x0155
            CLIPPATH = 343, // 0x0157
            XCLIPPATHUNITS = 344, // 0x0158
            YCLIPPATHUNITS = 345, // 0x0159
            INDEXED = 346, // 0x015A
            JPEGTABLES = 347, // 0x015B
            OPIPROXY = 351, // 0x015F
            JPEGPROC = 512, // 0x0200
            JPEGIFOFFSET = 513, // 0x0201
            JPEGIFBYTECOUNT = 514, // 0x0202
            JPEGRESTARTINTERVAL = 515, // 0x0203
            JPEGLOSSLESSPREDICTORS = 517, // 0x0205
            JPEGPOINTTRANSFORM = 518, // 0x0206
            JPEGQTABLES = 519, // 0x0207
            JPEGDCTABLES = 520, // 0x0208
            JPEGACTABLES = 521, // 0x0209
            YCBCRCOEFFICIENTS = 529, // 0x0211
            YCBCRSUBSAMPLING = 530, // 0x0212
            YCBCRPOSITIONING = 531, // 0x0213
            REFERENCEBLACKWHITE = 532, // 0x0214
            XMLPACKET = 700, // 0x02BC
            OPIIMAGEID = 32781, // 0x800D
            REFPTS = 32953, // 0x80B9
            REGIONTACKPOINT = 32954, // 0x80BA
            REGIONWARPCORNERS = 32955, // 0x80BB
            REGIONAFFINE = 32956, // 0x80BC
            MATTEING = 32995, // 0x80E3
            DATATYPE = 32996, // 0x80E4
            IMAGEDEPTH = 32997, // 0x80E5
            TILEDEPTH = 32998, // 0x80E6
            PIXAR_IMAGEFULLWIDTH = 33300, // 0x8214
            PIXAR_IMAGEFULLLENGTH = 33301, // 0x8215
            PIXAR_TEXTUREFORMAT = 33302, // 0x8216
            PIXAR_WRAPMODES = 33303, // 0x8217
            PIXAR_FOVCOT = 33304, // 0x8218
            PIXAR_MATRIX_WORLDTOSCREEN = 33305, // 0x8219
            PIXAR_MATRIX_WORLDTOCAMERA = 33306, // 0x821A
            WRITERSERIALNUMBER = 33405, // 0x827D
            COPYRIGHT = 33432, // 0x8298
            RICHTIFFIPTC = 33723, // 0x83BB
            IT8SITE = 34016, // 0x84E0
            IT8COLORSEQUENCE = 34017, // 0x84E1
            IT8HEADER = 34018, // 0x84E2
            IT8RASTERPADDING = 34019, // 0x84E3
            IT8BITSPERRUNLENGTH = 34020, // 0x84E4
            IT8BITSPEREXTENDEDRUNLENGTH = 34021, // 0x84E5
            IT8COLORTABLE = 34022, // 0x84E6
            IT8IMAGECOLORINDICATOR = 34023, // 0x84E7
            IT8BKGCOLORINDICATOR = 34024, // 0x84E8
            IT8IMAGECOLORVALUE = 34025, // 0x84E9
            IT8BKGCOLORVALUE = 34026, // 0x84EA
            IT8PIXELINTENSITYRANGE = 34027, // 0x84EB
            IT8TRANSPARENCYINDICATOR = 34028, // 0x84EC
            IT8COLORCHARACTERIZATION = 34029, // 0x84ED
            IT8HCUSAGE = 34030, // 0x84EE
            IT8TRAPINDICATOR = 34031, // 0x84EF
            IT8CMYKEQUIVALENT = 34032, // 0x84F0
            FRAMECOUNT = 34232, // 0x85B8
            PHOTOSHOP = 34377, // 0x8649
            EXIFIFD = 34665, // 0x8769
            ICCPROFILE = 34675, // 0x8773
            JBIGOPTIONS = 34750, // 0x87BE
            GPSIFD = 34853, // 0x8825
            FAXRECVPARAMS = 34908, // 0x885C
            FAXSUBADDRESS = 34909, // 0x885D
            FAXRECVTIME = 34910, // 0x885E
            FAXDCS = 34911, // 0x885F
            FEDEX_EDR = 34929, // 0x8871
            STONITS = 37439, // 0x923F
            INTEROPERABILITYIFD = 40965, // 0xA005
            DNGVERSION = 50706, // 0xC612
            DNGBACKWARDVERSION = 50707, // 0xC613
            UNIQUECAMERAMODEL = 50708, // 0xC614
            LOCALIZEDCAMERAMODEL = 50709, // 0xC615
            CFAPLANECOLOR = 50710, // 0xC616
            CFALAYOUT = 50711, // 0xC617
            LINEARIZATIONTABLE = 50712, // 0xC618
            BLACKLEVELREPEATDIM = 50713, // 0xC619
            BLACKLEVEL = 50714, // 0xC61A
            BLACKLEVELDELTAH = 50715, // 0xC61B
            BLACKLEVELDELTAV = 50716, // 0xC61C
            WHITELEVEL = 50717, // 0xC61D
            DEFAULTSCALE = 50718, // 0xC61E
            DEFAULTCROPORIGIN = 50719, // 0xC61F
            DEFAULTCROPSIZE = 50720, // 0xC620
            COLORMATRIX1 = 50721, // 0xC621
            COLORMATRIX2 = 50722, // 0xC622
            CAMERACALIBRATION1 = 50723, // 0xC623
            CAMERACALIBRATION2 = 50724, // 0xC624
            REDUCTIONMATRIX1 = 50725, // 0xC625
            REDUCTIONMATRIX2 = 50726, // 0xC626
            ANALOGBALANCE = 50727, // 0xC627
            ASSHOTNEUTRAL = 50728, // 0xC628
            ASSHOTWHITEXY = 50729, // 0xC629
            BASELINEEXPOSURE = 50730, // 0xC62A
            BASELINENOISE = 50731, // 0xC62B
            BASELINESHARPNESS = 50732, // 0xC62C
            BAYERGREENSPLIT = 50733, // 0xC62D
            LINEARRESPONSELIMIT = 50734, // 0xC62E
            CAMERASERIALNUMBER = 50735, // 0xC62F
            LENSINFO = 50736, // 0xC630
            CHROMABLURRADIUS = 50737, // 0xC631
            ANTIALIASSTRENGTH = 50738, // 0xC632
            SHADOWSCALE = 50739, // 0xC633
            DNGPRIVATEDATA = 50740, // 0xC634
            MAKERNOTESAFETY = 50741, // 0xC635
            CALIBRATIONILLUMINANT1 = 50778, // 0xC65A
            CALIBRATIONILLUMINANT2 = 50779, // 0xC65B
            BESTQUALITYSCALE = 50780, // 0xC65C
            RAWDATAUNIQUEID = 50781, // 0xC65D
            ORIGINALRAWFILENAME = 50827, // 0xC68B
            ORIGINALRAWFILEDATA = 50828, // 0xC68C
            ACTIVEAREA = 50829, // 0xC68D
            MASKEDAREAS = 50830, // 0xC68E
            ASSHOTICCPROFILE = 50831, // 0xC68F
            ASSHOTPREPROFILEMATRIX = 50832, // 0xC690
            CURRENTICCPROFILE = 50833, // 0xC691
            CURRENTPREPROFILEMATRIX = 50834, // 0xC692
            DCSHUESHIFTVALUES = 65535, // 0xFFFF
        }

        private enum SubFileTypeValues
        {
            REDUCEDIMAGE = 1,
            PAGE = 2,
            MASK = 4,
        }

        private enum OSubFileTypeValues
        {
            IMAGE = 1,
            REDUCEDIMAGE = 2,
            PAGE = 3,
        }

        private enum CompressionValues
        {
            NONE = 1,
            CCITTRLE = 2,
            CCITTFAX3 = 3,
            CCITT_T4 = 3,
            CCITTFAX4 = 4,
            CCITT_T6 = 4,
            LZW = 5,
            OJPEG = 6,
            JPEG = 7,
            ADOBE_DEFLATE = 8,
            NEXT = 32766, // 0x00007FFE
            CCITTRLEW = 32771, // 0x00008003
            PACKBITS = 32773, // 0x00008005
            THUNDERSCAN = 32809, // 0x00008029
            IT8CTPAD = 32895, // 0x0000807F
            IT8LW = 32896, // 0x00008080
            IT8MP = 32897, // 0x00008081
            IT8BL = 32898, // 0x00008082
            PIXARFILM = 32908, // 0x0000808C
            PIXARLOG = 32909, // 0x0000808D
            DEFLATE = 32946, // 0x000080B2
            DCS = 32947, // 0x000080B3
            JBIG = 34661, // 0x00008765
            SGILOG = 34676, // 0x00008774
            SGILOG24 = 34677, // 0x00008775
            JP2000 = 34712, // 0x00008798
        }

        private enum PhotoMetricValues
        {
            MINISWHITE = 0,
            MINISBLACK = 1,
            RGB = 2,
            PALETTE = 3,
            MASK = 4,
            SEPARATED = 5,
            YCBCR = 6,
            CIELAB = 8,
            ICCLAB = 9,
            ITULAB = 10, // 0x0000000A
            LOGL = 32844, // 0x0000804C
            LOGLUV = 32845, // 0x0000804D
        }

        private enum ThreshHoldingValues
        {
            BILEVEL = 1,
            HALFTONE = 2,
            ERRORDIFFUSE = 3,
        }

        private enum FillOrderValues
        {
            MSB2LSB = 1,
            LSB2MSB = 2,
        }

        private enum OrientationValues
        {
            TOPLEFT = 1,
            TOPRIGHT = 2,
            BOTRIGHT = 3,
            BOTLEFT = 4,
            LEFTTOP = 5,
            RIGHTTOP = 6,
            RIGHTBOT = 7,
            LEFTBOT = 8,
        }

        private enum PlanarConfigValues
        {
            CONTIG = 1,
            SEPARATE = 2,
        }

        private enum GrayResponseUnitValues
        {
            _10S = 1,
            _100S = 2,
            _1000S = 3,
            _10000S = 4,
            _100000S = 5,
        }

        private enum Group3OptionsValues
        {
            _2DENCODING = 1,
            UNCOMPRESSED = 2,
            FILLBITS = 4,
        }

        private enum Group4OptionsValues
        {
            UNCOMPRESSED = 2,
        }

        private enum ResolutionUnitValues
        {
            NONE = 1,
            INCH = 2,
            CENTIMETER = 3,
        }

        private enum ColorResponseUnitValues
        {
            _10S = 1,
            _100S = 2,
            _1000S = 3,
            _10000S = 4,
            _100000S = 5,
        }

        private enum PredictorValues
        {
            NONE = 1,
            HORIZONTAL = 2,
            FLOATINGPOINT = 3,
        }

        private enum CleanFaxDataValues
        {
            CLEAN,
            REGENERATED,
            UNCLEAN,
        }

        private enum InkSetValues
        {
            CMYK = 1,
            MULTIINK = 2,
        }

        private enum ExtraSamplesValues
        {
            UNSPECIFIED,
            ASSOCALPHA,
            UNASSALPHA,
        }

        private enum SampleFormatValues
        {
            UINT = 1,
            INT = 2,
            IEEEFP = 3,
            VOID = 4,
            COMPLEXINT = 5,
            COMPLEXIEEEFP = 6,
        }

        private enum JpegProcValues
        {
            BASELINE = 1,
            LOSSLESS = 14, // 0x0000000E
        }

        private enum YcbcrPositionINValues
        {
            CENTERED = 1,
            COSITED = 2,
        }
    }
}
