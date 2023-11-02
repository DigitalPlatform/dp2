using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// https://stackoverflow.com/questions/18330692/iso-iec13239-crc16-implementation

namespace DigitalPlatform.RFID
{
    public static class CRC16
    {
        private static UInt16 POLYNOMIAL = 0x8408;
        private static UInt16 PRESET_VALUE = 0xFFFF;

        // ISO13239
        // https://reveng.sourceforge.io/crc-catalogue/16.htm
        // CRC-16/IBM-SDLC
        // Alias: CRC-16/ISO-HDLC, CRC-16/ISO-IEC-14443-3-B, CRC-16/X-25, CRC-B, X-25
        // width=16 poly=0x1021 init=0xffff refin=true refout=true xorout=0xffff check=0x906e residue=0xf0b8 name="CRC-16/IBM-SDLC"
        public static byte [] crc16x25(byte[] data)
        {
            UInt32 current_crc_value = PRESET_VALUE;
            for (int i = 0; i < data.Length; i++)
            {
                current_crc_value ^= ReverseBitsWith4Operations(data[i]) & (UInt32)0xFF;

                // current_crc_value ^= ((UInt16)data[i]) & 0xFF;
                for (int j = 0; j < 8; j++)
                {
                    if ((current_crc_value & 1) != 0)
                    {
                        current_crc_value = (current_crc_value >> 1) ^ POLYNOMIAL;
                    }
                    else
                    {
                        current_crc_value = current_crc_value >> 1;
                    }
                }
            }
            current_crc_value = ~current_crc_value;

            var result =  (UInt16)(current_crc_value & (UInt32)0xFFFF);

            var bytes = BitConverter.GetBytes(result);
            bytes[0] = ReverseBitsWith4Operations(bytes[0]);
            bytes[1] = ReverseBitsWith4Operations(bytes[1]);
            return bytes;
        }

        // https://itecnote.com/tecnote/c-built-in-function-to-reverse-bit-order/
        public static byte ReverseBitsWith4Operations(byte b)
        {
            return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
        }
    }
}
