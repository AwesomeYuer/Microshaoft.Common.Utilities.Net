// thanks for : namespace NSubsys
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    public class PeUtility : IDisposable
    {
        public enum SubSystemType : UInt16
        {
            IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
            IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_DOS_HEADER
        {
            [FieldOffset(60)]
            public UInt32 e_lfanew;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_OPTIONAL_HEADER
        {
            [FieldOffset(68)]
            public UInt16 Subsystem;
        }

        private long fileHeaderOffset;
        private IMAGE_OPTIONAL_HEADER optionalHeader;
        private FileStream curFileStream;

        public PeUtility(string filePath)
        {
            curFileStream = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
            var reader = new BinaryReader(curFileStream);
            var dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

            // Seek the new PE Header and skip NtHeadersSignature (4 bytes) & IMAGE_FILE_HEADER struct (20bytes).
            curFileStream.Seek(dosHeader.e_lfanew + 4 + 20, SeekOrigin.Begin);

            fileHeaderOffset = curFileStream.Position;
            optionalHeader = FromBinaryReader<IMAGE_OPTIONAL_HEADER>(reader);
        }

        /// <summary>
        /// Reads in a block from a file and converts it to the struct
        /// type specified by the template parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T FromBinaryReader<T>(BinaryReader reader)
        {
            // Read in a byte array
            var bytes = reader.ReadBytes(Marshal.SizeOf<T>());

            // Pin the managed memory while, copy it out the data, then unpin it
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var theStructure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return theStructure;
        }

        public void Dispose()
        {
            curFileStream?.Dispose();
        }

        /// <summary>
        /// Gets the optional header
        /// </summary>
        public IMAGE_OPTIONAL_HEADER OptionalHeader
        {
            get => optionalHeader;
        }

        /// <summary>
        /// Gets the PE file stream for R/W functions.
        /// </summary> 
        public FileStream Stream
        {
            get => curFileStream;
        }

        public long MainHeaderOffset
        {
            get => fileHeaderOffset;
        }

        public bool ProcessFile(string exeFilePath)
        {
            //Log.LogMessage("NSubsys Subsystem Changer for Windows PE files.");
            //Log.LogMessage($"[NSubsys] Target EXE `{exeFilePath}`.");

            using (var peFile = new PeUtility(exeFilePath))
            {
                SubSystemType subsysVal;
                var subsysOffset = peFile.MainHeaderOffset;

                subsysVal = (SubSystemType)peFile.OptionalHeader.Subsystem;
                subsysOffset += Marshal.OffsetOf<IMAGE_OPTIONAL_HEADER>("Subsystem").ToInt32();

                switch (subsysVal)
                {
                    case SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI:
                        //Log.LogWarning("Executable file is already a Win32 App!");
                        return true;
                    case SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_CUI:
                        //Log.LogMessage("Console app detected...");
                        //Log.LogMessage("Converting...");

                        var subsysSetting = BitConverter.GetBytes((ushort)SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI);

                        if (!BitConverter.IsLittleEndian)
                            Array.Reverse(subsysSetting);

                        if (peFile.Stream.CanWrite)
                        {
                            peFile.Stream.Seek(subsysOffset, SeekOrigin.Begin);
                            peFile.Stream.Write(subsysSetting, 0, subsysSetting.Length);
                            //Log.LogMessage("Conversion Complete...");
                        }
                        else
                        {
                            //Log.LogMessage("Can't write changes!");
                            //Log.LogMessage("Conversion Failed...");
                        }

                        return true;
                    default:
                        //Log.LogMessage($"Unsupported subsystem : {Enum.GetName(typeof(SubSystemType), subsysVal)}.");
                        return false;
                }
            }
        }

    }
}