using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Collections;

namespace WebRTCServer2.OnMediaCode
{
    public static class AppendOggFiles
    {





        static string urlFolder = HttpRuntime.AppDomainAppPath + @"Download_Data\";
        static string urlFolderOutput = HttpRuntime.AppDomainAppPath + @"Download_Data\Audiofiles\";

        public static void AppendOggFilesMain()
        {








         

            DirectoryInfo di = new DirectoryInfo(urlFolder);


            FileInfo[] files = di.GetFiles("*.txt");



            byte[] oggs = Encoding.ASCII.GetBytes("OggS");

            for (int k = 0; k < files.Length; k++)
            {
            start:
                getDataArray(files[k].FullName);

                byte[] file1Bytes;

                try
                {
                    file1Bytes = File.ReadAllBytes(getFileNameFromPath(OggFilesArray[0][1], urlFolder));
                }
                catch (FileNotFoundException e)
                {
                    k++;
                    if (k >= files.Length) goto end;
                    else goto start;
                }


                List<int> places = FindCharsInArray(file1Bytes, oggs, false);

                List<OggPageAdaption> pages = new List<OggPageAdaption>(places.Count);

                OggPageAdaption previousOggPageAdaption = new OggPageAdaption(file1Bytes, places[places.Count - 1]);
                UInt32 previous_page_sequence_number_uint32 = previousOggPageAdaption.page_sequence_number_uint32;

                UInt64 previous_granule_position_u64 = 0;
                previous_granule_position_u64 = previousOggPageAdaption.granule_position_u64;
                using (FileStream fs = new FileStream(urlFolderOutput + files[k].Name.Substring(0, files[k].Name.Length - 4) + ".ogg", FileMode.Append))
                {
                    fs.Write(file1Bytes, 0, file1Bytes.Length);



                    for (int j = 1; j < OggFilesArray.Count; j++)
                    {







                        byte[] file2Bytes = File.ReadAllBytes(getFileNameFromPath(OggFilesArray[j][1], urlFolder));



                        List<int> places2 = FindCharsInArray(file2Bytes, oggs, false);



                        OggPageAdaption newOggPageAdaption = new OggPageAdaption();



                        for (int i = 0; i < places2.Count; i++)
                        {

                          


                            Array.Copy(previousOggPageAdaption.bitstream_serial_number, 0, file2Bytes, places2[i] + 14, 4);
                            newOggPageAdaption = new OggPageAdaption(file2Bytes, places2[i]);


                            var finalGranule_position_u64 = newOggPageAdaption.granule_position_u64 + previous_granule_position_u64;

                            byte[] finalGranule_position = BitConverter.GetBytes(finalGranule_position_u64);

                            Array.Copy(finalGranule_position, 0, file2Bytes, places2[i] + 6, 8);


                            var newPage_sequence_number_uint32 = previous_page_sequence_number_uint32;

                            if (i > 1)
                            {
                                newPage_sequence_number_uint32 = previous_page_sequence_number_uint32 + 1;
                            }

                            var newPage_secuence_number = BitConverter.GetBytes(newPage_sequence_number_uint32);

                            Array.Copy(newPage_secuence_number, 0, file2Bytes, places2[i] + 18, 4);




                            previous_page_sequence_number_uint32 = newPage_sequence_number_uint32;


                            int end = 0;
                            int start = places2[i];
                            if (i >= places2.Count - 1)
                            {
                                end = file2Bytes.Length - 1;
                            }
                            else end = places2[i + 1];
                            if (i >= places2.Count - 1) end = file2Bytes.Length;

                            file2Bytes[places2[i] + 22] = Convert.ToByte(0);
                            file2Bytes[places2[i] + 23] = Convert.ToByte(0);
                            file2Bytes[places2[i] + 24] = Convert.ToByte(0);
                            file2Bytes[places2[i] + 25] = Convert.ToByte(0);

                            int checkSum = OggCrc.checksum(0, file2Bytes, start, end - start);


                            byte[] crc = BitConverter.GetBytes(checkSum);

                            file2Bytes[places2[i] + 22] = crc[0];
                            file2Bytes[places2[i] + 23] = crc[1];
                            file2Bytes[places2[i] + 24] = crc[2];
                            file2Bytes[places2[i] + 25] = crc[3];

                        }

                        previous_granule_position_u64 = previous_granule_position_u64 + newOggPageAdaption.granule_position_u64;

                        fs.Write(file2Bytes, places2[2], file2Bytes.Length - places2[2]);

                        previousOggPageAdaption = newOggPageAdaption;





                        File.Move(urlFolder + getFileNameFromPath(OggFilesArray[j][1], ""), urlFolder + "archive/" + getFileNameFromPath(OggFilesArray[j][1], ""));
                    }
                }



                files[k].MoveTo(urlFolder + "archive/" + files[k].Name);

            }

        end: int a = 1;
        }





        private static string getFileNameFromPath(string path, string urlFolder)
        {
#if DEBUG
            string filename = path.Substring(path.LastIndexOf("Body"));
            path = urlFolder + filename;
#endif
            return path;
        }



        static void appendFileToFinalFile(string url, byte[] arrayFile)
        {

        }


        private static int IndexFile = 0;

        private static string[] DataFiles;
        private static int[] indexes;

        private static List<string[]> OggFilesArray;

        static void getDataArray(string urlDataText)
        {
            string[] data = File.ReadAllLines(urlDataText);


            List<string[]> dataLines = new List<string[]>(data.Length);


            for (int i = 0; i < data.Length; i++)
            {

                dataLines.Add(data[i].Split(';'));
            }
            dataLines.OrderBy(p => p[0]);

            OggFilesArray = dataLines;

        }

     



        static void printInFileHeadersOfFileArray(byte[] file1Bytes, int type = 0)
        {

#if DEBUG
            string fileName = type == 0 ? "log.txt" : "logOriginal.txt";
            byte[] chars = Encoding.ASCII.GetBytes("OggS");

            List<int> places = FindCharsInArray(file1Bytes, chars, false);

            List<OggPage> pages = new List<OggPage>(places.Count);

            for (int i = 0; i < places.Count; i++)
            {
                OggPage newOggPage = new OggPage(file1Bytes, places[i]);
                pages.Add(newOggPage);
            }

            using (StreamWriter sw = new StreamWriter(urlFolder + fileName, true))
            {

                for (int i = 0; i < pages.Count; i++)
                {

                    sw.WriteLine("Page No :" + i.ToString());
                    sw.WriteLine(" capture_pattern : {0} ", Encoding.ASCII.GetString(pages[i].capture_pattern));
                    sw.WriteLine(" Version : {0} ", Convert.ToInt32(pages[i].Version[0]).ToString());
                    sw.WriteLine(" header_type.packet_continued : {0} , header_type.bos : {1}, header_type.eos : {2} ",
                        new BitArray(pages[i].header_type).Get(1), new BitArray(pages[i].header_type).Get(2), new BitArray(pages[i].header_type).Get(4));
                    sw.WriteLine(" granule_position : {0} ", BitConverter.ToUInt64(pages[i].granule_position, 0));
                    sw.WriteLine(" bitstream_serial_number : {0}, {1}, {2} ,{3} ", pages[i].bitstream_serial_number[0].ToString(), pages[i].bitstream_serial_number[1].ToString(),
                        pages[i].bitstream_serial_number[2].ToString(), pages[i].bitstream_serial_number[3].ToString());

                    sw.WriteLine(" page_sequence_number : {0} ", BitConverter.ToUInt32(pages[i].page_sequence_number, 0));

                    sw.WriteLine(" CRC_checksum : {0} ", BitConverter.ToInt32(pages[i].CRC_checksum, 0));
                    sw.WriteLine(" page_segments : {0} ", ((uint)pages[i].page_segments[0]).ToString());
                    for (int j = 0; j < pages[i].segments_number; j++)
                    {
                        sw.WriteLine("segment {0} : {1}", j, pages[i].segments_table[j]);
                    }

                    sw.WriteLine("-------------------");

                }
            }

#endif

        }





        static void printHeadersOfFileArray(byte[] file1Bytes)
        {

            byte[] chars = Encoding.ASCII.GetBytes("OggS");

            List<int> places = FindCharsInArray(file1Bytes, chars, false);

            List<OggPage> pages = new List<OggPage>(places.Count);

            for (int i = 0; i < places.Count; i++)
            {
                OggPage newOggPage = new OggPage(file1Bytes, places[i]);
                pages.Add(newOggPage);
            }


            for (int i = 0; i < pages.Count; i++)
            {
                Console.WriteLine("Page No :" + i.ToString());
                Console.WriteLine(" capture_pattern : {0} ", Encoding.ASCII.GetString(pages[i].capture_pattern));
                Console.WriteLine(" Version : {0} ", Convert.ToInt32(pages[i].Version[0]).ToString());
                Console.WriteLine(" header_type.packet_continued : {0} , header_type.bos : {1}, header_type.eos : {2} ",
                    new BitArray(pages[i].header_type).Get(1), new BitArray(pages[i].header_type).Get(2), new BitArray(pages[i].header_type).Get(4));
                Console.WriteLine(" granule_position : {0} ", BitConverter.ToUInt64(pages[i].granule_position, 0));
                Console.WriteLine(" bitstream_serial_number : {0}, {1}, {2} ,{3} ", pages[i].bitstream_serial_number[0].ToString(), pages[i].bitstream_serial_number[1].ToString(),
                    pages[i].bitstream_serial_number[2].ToString(), pages[i].bitstream_serial_number[3].ToString());

                Console.WriteLine(" page_sequence_number : {0} ", BitConverter.ToUInt32(pages[i].page_sequence_number, 0));
                var crcBits = new BitArray(pages[i].CRC_checksum);
                Console.WriteLine(" CRC_checksum : {0} ", BitConverter.ToInt32(pages[i].CRC_checksum, 0));
                Console.WriteLine(" page_segments : {0} ", ((uint)pages[i].page_segments[0]).ToString());
                for (int j = 0; j < pages[i].segments_number; j++)
                {
                    Console.WriteLine("segment {0} : {1}", j, pages[i].segments_table[j]);
                }

                Console.WriteLine("-------------------");

            }



        }



        static List<int> FindCharsInArray(byte[] arrayBytes, byte[] chars, bool verify = true)
        {

            List<int> places = new List<int>();

            for (int i = 0; i < arrayBytes.Length; i++)
            {
                for (int j = 0; j < chars.Length; j++)
                {
                    if (arrayBytes[i + j] != chars[j]) break;
                    if (j == (chars.Length - 1)) places.Add(i);
                }
            }
            if (verify)
            {
                for (int j = 0; j < places.Count; j++)
                {
                    int end = 0;

                    if (j >= places.Count - 1)
                    {
                        end = arrayBytes.Length - 1;
                    }
                    else end = places[j + 1] - 1;
                    if (!verifyChecksum(places[j], end, arrayBytes)) places.Remove(j);

                }
            }
            return places;
        }


        static bool verifyChecksum(int start, int end, byte[] arrayBytes)
        {
            int originalValue = BitConverter.ToInt32(arrayBytes, 22);
            arrayBytes[start + 22] = Convert.ToByte(0);
            arrayBytes[start + 23] = Convert.ToByte(0);
            arrayBytes[start + 24] = Convert.ToByte(0);
            arrayBytes[start + 25] = Convert.ToByte(0);

            return (originalValue - OggCrc.checksum(0, arrayBytes, start, end - start)) == 0;

        }



        static void Append2Files()
        {
            string file2 = @"\BodyPart_682e08b7-6beb-4048-b77f-f79153620d4c.ogg";
            string file1 = @"\BodyPart_d5d09ca6-3d1f-42ad-b1b0-d860749c6f7d.ogg";


            byte[] file1Bytes = File.ReadAllBytes(urlFolder + file1);
            byte[] file2Bytes = File.ReadAllBytes(urlFolder + file2);

            byte[] fileBytesFinale = file1Bytes.Concat(file2Bytes).ToArray();



            File.WriteAllBytes(urlFolder + @"\final2.ogg", fileBytesFinale);
        }
    }




    public class OggPageAdaption
    {


        public byte[] granule_position = new byte[8];
        public UInt64 granule_position_u64;



        public byte[] bitstream_serial_number = new byte[4];





        public byte[] page_sequence_number = new byte[4];
        public UInt32 page_sequence_number_uint32;



        public OggPageAdaption() { }
        public OggPageAdaption(byte[] arrayFile, int indexStart)
        {

          
            Array.Copy(arrayFile, indexStart + 6, granule_position, 0, 8);
            Array.Copy(arrayFile, indexStart + 14, bitstream_serial_number, 0, 4);
            Array.Copy(arrayFile, indexStart + 18, page_sequence_number, 0, 4);


         



            granule_position_u64 = BitConverter.ToUInt64(this.granule_position, 0);

            page_sequence_number_uint32 = BitConverter.ToUInt32(this.page_sequence_number, 0);




        }





    }




    public class OggPage
    {


        public byte[] capture_pattern = new byte[4];

        public byte[] Version = new byte[1];

        public byte[] header_type = new byte[1];

        public byte[] granule_position = new byte[8];

        public byte[] bitstream_serial_number = new byte[4];

        public byte[] page_sequence_number = new byte[4];

        public byte[] CRC_checksum = new byte[4];

        public byte[] page_segments = new byte[1];

        public int segments_number;

        public int[] segments_table;

        public OggPage(byte[] arrayFile, int indexStart)
        {

            Array.Copy(arrayFile, indexStart, capture_pattern, 0, 4);
            Array.Copy(arrayFile, indexStart + 4, Version, 0, 1);
            Array.Copy(arrayFile, indexStart + 5, header_type, 0, 1);
            Array.Copy(arrayFile, indexStart + 6, granule_position, 0, 8);
            Array.Copy(arrayFile, indexStart + 14, bitstream_serial_number, 0, 4);
            bitstream_serial_number = bitstream_serial_number.Reverse().ToArray();
            Array.Copy(arrayFile, indexStart + 18, page_sequence_number, 0, 4);
            Array.Copy(arrayFile, indexStart + 22, CRC_checksum, 0, 4);
            Array.Copy(arrayFile, indexStart + 26, page_segments, 0, 1);

            segments_number = Convert.ToInt32(page_segments[0]);


            segments_table = new int[segments_number];

            for (int i = 0; i < segments_number; i++)
            {
                segments_table[i] = Convert.ToInt32(arrayFile[indexStart + 27 + i]);
            }





        }



    }

    public class OggPageArraySegment
    {
        public ArraySegment<byte> capture_pattern;

        public ArraySegment<byte> Version;

        public ArraySegment<byte> header_type;

        public ArraySegment<byte> granule_position;

        public ArraySegment<byte> bitstream_serial_number;

        public ArraySegment<byte> page_sequence_number;

        public ArraySegment<byte> CRC_checksum;

        public ArraySegment<byte> page_segments;




        public OggPageArraySegment(byte[] oggPage, int indexStart)
        {
            capture_pattern = new ArraySegment<byte>(oggPage, indexStart, 4);
            Version = new ArraySegment<byte>(oggPage, indexStart, 1);
            header_type = new ArraySegment<byte>(oggPage, indexStart, 1);
            granule_position = new ArraySegment<byte>(oggPage, indexStart, 8);
            bitstream_serial_number = new ArraySegment<byte>(oggPage, indexStart, 4);
            page_sequence_number = new ArraySegment<byte>(oggPage, indexStart, 4);
            CRC_checksum = new ArraySegment<byte>(oggPage, indexStart, 4);
            page_segments = new ArraySegment<byte>(oggPage, indexStart, 1);
        }
    }











}
