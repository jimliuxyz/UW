using System;
using System.Text;
using UW.Shared;
using UW.Shared.Misc;

namespace UW.Shared.Models
{
    public class PkGuidGen
    {
        private readonly static Random rnd = new Random(8734);

        private readonly Base62Converter B62C;
        private readonly long VolSize;
        public PkGuidGen(int seed, long VolSize)
        {
            this.B62C = new Base62Converter(seed);
            this.VolSize = VolSize;
        }

        public PkGuid Generate(long amount)
        {
            var rnd3 = rnd.Next(100, 199);
            var pk = "V" + AmountToVolume(amount);
            // var prefix = rnd3 + B62C.Encode(amount.ToString());
            var prefix = rnd3.ToString().Scramble() + amount.ToString().Scramble(rnd3);
            var guid = F.NewGuid();
 Console.WriteLine("rnd3 " + rnd3);
 Console.WriteLine("rnd3 text " + rnd3.ToString().Scramble());

            return new PkGuid(amount, pk, prefix, guid);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberId">Format : 3RandomChart+Amount-Guid</param>
        /// <returns></returns>
        public PkGuid Parse(string memberId)
        {
            try
            {
                var part = memberId.Trim().Split("-");
                // var prefix = part[0].Substring(3, part[0].Length-3); //skip first 3 str
Console.WriteLine("a");
                var rnd3 = int.Parse(part[0].Substring(0, 3).Descramble());
 Console.WriteLine("b" + part[0].Substring(0, 3));
 Console.WriteLine("b" + rnd3);
               var prefix = part[0].Substring(3, part[0].Length-3).Descramble(rnd3);
Console.WriteLine("c");

                var postfix = part[1];

                var volume = long.Parse(B62C.Decode(prefix));
                var pk = "V" + AmountToVolume(volume);

                return new PkGuid(volume, pk, prefix, postfix);
            }
            catch (System.Exception)
            {
                throw new Exception("Unknown member id format!");
            }
        }

        public long AmountToVolume(long amount) => (long)Math.Ceiling((decimal)amount / VolSize);

    }
    public class PkGuid
    {
        public readonly long Volume;
        public readonly string PK;
        public readonly string Prefix;
        public readonly string Guid;

        public PkGuid(long volume, string pk, string prefix, string guid = null)
        {
            this.Volume = volume;
            this.PK = pk;
            this.Prefix = prefix;
            this.Guid = guid;
        }

        override public string ToString() => $"{Prefix}-{Guid}";

        public string Mod(int size)
        {
            var asInt = BitConverter.ToInt32(Encoding.Default.GetBytes(Guid), 0);
            asInt = asInt == int.MinValue ? asInt + 1 : asInt;
            return $"{Math.Abs(asInt) % size}";
        }


    }
}
