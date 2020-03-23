using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SoundGen
{
    public class WavFileGenerator
    {
        private const short BitsPerSampleQmLab = 16; //Important for QMLab 
        private const byte DataTypeQmLab = 1;
        private readonly Encoding _encoding = Encoding.GetEncoding("windows-1251");

        private string _fileName;
        private int _fileSize;

        public string FileName => _fileName;

        public int FileSize => _fileSize;

        public void WriteSoundData(string fileName, int divider)
        {
            //Leaving all parameters in this method - we might want to pass them from the UI with eventArgs
            int sampleRate = 44100;
            short channels = 6;
            int columnOffset = 3;
            int precision = 1000;
            int maxPossibleValue = 20;
            var header = CreateHeader(channels, sampleRate);

            _fileName = fileName;

            int linesinCsv = GetCsvLinesCount(fileName) - channels;
            
            using var reader = new StreamReader(new BufferedStream(new FileStream(fileName, FileMode.Open)),
                _encoding);
            
            SkipCsvHeader(channels, reader);

            using var writer = new BinaryWriter(new BufferedStream(new FileStream(fileName + ".wav", FileMode.Create)));

            writer.Write(header);

            string line;
            int fileLenBytes = header.Length;
            int linesProcessed = 0;
            while ((line = reader.ReadLine()) != null)
            {
                var data = processLine(line, channels, columnOffset, precision, divider, maxPossibleValue);
                var bytesWritten = WriteSingleSample(1, sampleRate, writer, data);
                fileLenBytes += bytesWritten;
                linesProcessed++;
                Console.WriteLine("Processed "+linesProcessed +" of "+linesinCsv+" : "+ (linesProcessed*100/linesinCsv) + "%");
            }

            WriteHeader(AddFileLengthToHeader(header, fileLenBytes), writer);
            
            _fileSize = fileLenBytes;
        }

        private int GetCsvLinesCount(string fileName)
        {
            using var reader = new StreamReader(new BufferedStream(new FileStream(fileName, FileMode.Open)));
            
            int lines = 0;
            while (!reader.EndOfStream)
            {
                reader.ReadLine();
                lines++;
            }

            return lines;
        }

        private short[] processLine(string line, short channels, int columnOffset, int precision, int divider,
            int maxPossibleValue)
        {
            string[] stringData = line.Split(";");
            float[] valuesByChannel = new float[channels];
            for (int i = 0; i < channels; i++)
            {
                valuesByChannel[i] = float.Parse(stringData[i + columnOffset],
                    CultureInfo.GetCultureInfo("ru-RU").NumberFormat);
            }

            var data = valuesByChannel.Select(f =>
                {
                    return Convert.ToInt16(
                        (((f * precision) / divider) / (maxPossibleValue * precision)) * short.MaxValue);
                })
                .ToArray();
            return data;
        }

        private static void SkipCsvHeader(short channels, StreamReader reader)
        {
            for (int i = 0; i < channels; i++)
            {
                reader.ReadLine();
            }
        }

        private int WriteSingleSample(int lengthInSeconds, int sampleRate, BinaryWriter writer,
            short[] data)
        {
            //TODO Generate one big array, fill it with data, then write it at once.
            var sampleSize = lengthInSeconds * sampleRate * data.Length * 2;
            //Time
            for (int i = 0; i < lengthInSeconds; i++)
            {
                //Samples 
                for (int j = 0; j < sampleRate; j++)
                {
                    foreach (short sample in data)
                    {
                        writer.Write(BitConverter.GetBytes(sample));
                    }
                }
            }

            return sampleSize; //2 - size of short data type in bytes
        }

        private byte[] CreateHeader(short channels, int smplRate)
        {
            byte[] header = new byte[44];
            //RIFF
            header[0] = Convert.ToByte('R');
            header[1] = Convert.ToByte('I');
            header[2] = Convert.ToByte('F');
            header[3] = Convert.ToByte('F');
            //SIZE, 4 bytes, 0 for now
            for (int i = 4; i <= 7; i++)
            {
                header[i] = 0;
            }

            //WAVE
            header[8] = Convert.ToByte('W');
            header[9] = Convert.ToByte('A');
            header[10] = Convert.ToByte('V');
            header[11] = Convert.ToByte('E');
            //fmt
            header[12] = Convert.ToByte('f');
            header[13] = Convert.ToByte('m');
            header[14] = Convert.ToByte('t');
            header[15] = Convert.ToByte(' ');
            //Header length, 4 bytes, 16 - 19 
            byte[] headerLen = BitConverter.GetBytes(16);
            Array.Copy(headerLen, 0, header, 16, 4);
            //Data Type, 2 bytes - 20 - 21
            byte[] dataType = BitConverter.GetBytes((byte) DataTypeQmLab);
            Array.Copy(dataType, 0, header, 20, 2);
            //Channels, 2 bytes - 22 - 23
            byte[] channelsNum = BitConverter.GetBytes(channels);
            Array.Copy(channelsNum, 0, header, 22, 2);
            //Discretization, Hz, 4 bytes, 24 - 27
            byte[] sampleRate = BitConverter.GetBytes(smplRate);
            Array.Copy(sampleRate, 0, header, 24, 4);
            //(Sample Rate * BitsPerSample * Channels) / 8,  4 bytes, 28 - 31
            byte[] coeff = BitConverter.GetBytes((smplRate * BitsPerSampleQmLab * channels) / 8);
            Array.Copy(coeff, 0, header, 28, 4);
            //(BitsPerSample * Channels) / 8,  2 bytes, 32 - 33
            byte[] bytesForAllChannels = BitConverter.GetBytes((short) ((BitsPerSampleQmLab * channels) / 8));
            Array.Copy(bytesForAllChannels, 0, header, 32, 2);
            //Bits per sample, 2 bytes, 34 - 35
            byte[] bitsPerSampleArr = BitConverter.GetBytes(BitsPerSampleQmLab);
            Array.Copy(bitsPerSampleArr, 0, header, 34, 2);
            //data, 
            header[36] = Convert.ToByte('d');
            header[37] = Convert.ToByte('a');
            header[38] = Convert.ToByte('t');
            header[39] = Convert.ToByte('a');
            //File size, 4 bytes
            return header;
        }

        private byte[] AddFileLengthToHeader(byte[] header, int fileSize)
        {
            byte[] newHeader = new byte[44];
            header.CopyTo(newHeader, 0);
            byte[] fileWithHeader = BitConverter.GetBytes(fileSize - 8);
            Array.Copy(fileWithHeader, 0, newHeader, 4, 4);
            byte[] fileData = BitConverter.GetBytes(fileSize - 44);
            Array.Copy(fileData, 0, newHeader, 40, 4);
            return newHeader;
        }

        private void WriteHeader(byte[] header, BinaryWriter writer)
        {
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(header);
        }
    }
}