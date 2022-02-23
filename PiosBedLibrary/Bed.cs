using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using static PiosBedLibrary.Functions;

namespace PiosBedLibrary
{
    public class Bed
    {
        /// <summary>
        /// Magic string of a <see cref="Bed"/>
        /// </summary>
        public char[] MAGIC { get; set; }
        /// <summary>
        /// Header of a <see cref="Bed"/>
        /// </summary>
        public byte[] header { get; set; }
        /// <summary>
        /// Epl files contained in a <see cref="Bed"/>
        /// </summary>
        public List<Epl> EplFiles { get; set; }
        /// <summary>
        /// Size of the whole <see cref="Bed"/>
        /// </summary>
        public ulong BedSize
        {
            get
            {
                ulong size = 1584;
                foreach (Epl epl in EplFiles)
                    size += epl.EplSize;

                return size;
            }
        }
        /// <summary>
        /// Creates a new empty <see cref="Bed"/>.
        /// </summary>
        public Bed()
        {

        }
        public Bed(byte[] Bytes, long End = 0)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Bytes)))
                ReadData(reader, End);
        }
        /// <summary>
        /// Reads a <see cref="Bed"/> from the
        /// current postion of a <see cref="BinaryReader"/>
        /// if no end point is specified bed will
        /// be read to the end of a file
        /// </summary>
        public Bed(BinaryReader reader, long End = 0)
        {
            ReadData(reader, End);
                
        }
        /// <summary>
        /// Reads a <see cref="Bed"/> from a file.
        /// </summary>
        public Bed(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                ReadData(reader);

        }
        /// <summary>
        /// Saves a <see cref="Bed"/> to a file.
        /// </summary>
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(path, FileMode.Create)))
                WriteData(writer);

        }
        /// <summary>
        /// Writes a <see cref="Bed"/> at the
        /// current position of a <see cref="BinaryWriter"/>
        /// </summary>
        public void Save(BinaryWriter writer)
        {
            WriteData(writer);
        }

        private void ReadData(BinaryReader reader, long End = 0)
        {
            if (End == 0)
                End = reader.BaseStream.Length;
            MAGIC = reader.ReadChars(8);
            string magic = new string(MAGIC);
            if (!magic.Contains("BED400"))
                throw new InvalidOperationException("Not a valid bed file");
            header = reader.ReadBytes(1576);
            EplFiles = new List<Epl>();
            while (reader.BaseStream.Position < End)
            {
                EplFiles.Add(new Epl(reader));
                if (reader.BaseStream.Position == End - 16)
                {
                    reader.ReadBytes(16);
                }
            }
        }

        private void WriteData(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            writer.Write(header);
            foreach (Epl EplFile in EplFiles)
            {
                EplFile.Save(writer);
            }
        }
            
    }
    
}
