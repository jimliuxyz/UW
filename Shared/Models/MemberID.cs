using System;
using System.Text;
using UW.Shared;

namespace UW.Shared.Models
{
    public class MemberID
    {
        private static readonly decimal VOLSIZE = 100;

        public readonly long Volume;
        public readonly string Guid;

        public MemberID(long volume, string guid = null)
        {
            this.Volume = volume;
            this.Guid = guid ?? F.NewGuid();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberId">V1-yFzIAhUuVE6kgHFvOno3UQ</param>
        /// <returns></returns>
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
        override public string ToString() => $"V{Volume}-{Guid}";

        public string Mod(int size)
        {
            var asInt = BitConverter.ToInt32(Encoding.Default.GetBytes(Guid), 0);
            asInt = asInt == int.MinValue ? asInt + 1 : asInt;
            return $"{Math.Abs(asInt) % size}";
        }


    }
}
