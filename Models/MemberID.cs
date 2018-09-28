using System;
using System.Text;

namespace UW.Models
{
    public class MemberID
    {
        private static readonly decimal VOLSIZE = 100;

        public readonly long volume;
        public readonly string guid;

        public MemberID(long volume, string guid = null)
        {
            this.volume = volume;
            this.guid = guid ?? F.NewGuid();
        }

        public MemberID parse(string memberId)
        {
            try
            {
                var part = memberId.Trim().Split("-");

                var volume = part[0].Substring(1, part[0].Length).ToLong();
                var guid = part[2];

                return new MemberID(volume, guid);
            }
            catch (System.Exception)
            {
                throw new Exception("Unknown member id format!");
            }
        }

        public static long AmountToVolume(long amount) => (long)Math.Ceiling(amount / VOLSIZE);
        public string ToString => $"V{volume}-{guid}";

        public string Mod(int size)
        {
            var asInt = BitConverter.ToInt32(Encoding.Default.GetBytes(guid), 0);
            asInt = asInt == int.MinValue ? asInt + 1 : asInt;
            return $"{Math.Abs(asInt) % size}";
        }


    }
}
