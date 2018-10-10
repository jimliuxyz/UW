using System;
using System.Text;
using UW.Shared;
using UW.Shared.Misc;
using System.Linq;

namespace UW.Shared.Misc
{
    public class Pkuid
    {
        public string PK {get { return Prefix + "-" + PkIdx; } }
        public readonly long PkIdx;
        public readonly string Prefix;
        public readonly string Guid;

        override public string ToString() => Guid;

        public Pkuid(long pkIdx, string prefix, string guid)
        {
            this.PkIdx = pkIdx;
            this.Prefix = prefix;
            this.Guid = guid;
        }

        public string Mod(int size)
        {
            var asInt = BitConverter.ToInt32(Encoding.Default.GetBytes(Guid), 0);
            asInt = asInt == int.MinValue ? asInt + 1 : asInt;
            return $"{Math.Abs(asInt) % size}";
        }
    }
    public class PkuidGen
    {
        private static readonly int ModeVol = 0;
        private static readonly int ModeRandom = 1;
        private int mode = ModeVol;

        private readonly string Prefix;
        private int VolSize;
        private int RandomStart, RandomEnd;
        public PkuidGen(string Prefix) { this.Prefix = Prefix; }

        /// <summary>
        /// 設定每個pk的容量 pk=(amount/volume)
        /// </summary>
        /// <param name="volSize"></param>
        /// <returns></returns>
        public PkuidGen SetPkVolume(int volSize)
        {
            mode = ModeVol;
            VolSize = volSize;
            return this;
        }

        /// <summary>
        /// 設定隨機的pk
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public PkuidGen SetRandomRange(int start, int end)
        {
            mode = ModeRandom;
            RandomStart = start;
            RandomEnd = end;
            return this;
        }

        private int GuidToSum(string guid)
        {
            return guid.ToCharArray().Aggregate(0, (sum, next) =>
            {
                return sum + next;
            });
        }

        /// <summary>
        /// Generate a GUID with partition key information
        /// </summary>
        /// <param name="amount">搭配SetPkVolume時使用</param>
        /// <returns></returns>
        public Pkuid Generate(long amount = 0)
        {
            var part = new string[2];

            part[1] = F.NewGuid();

            var seed =  part[1].GetSum();
            var b62c = new Base62Converter(seed);

            long pkIdx = 0;
            if (mode == ModeVol)
                pkIdx = AmountToVolume(amount);
            else if (mode == ModeRandom)
                pkIdx = F.Random(RandomStart, RandomEnd);

            part[0] = b62c.Encode(Prefix + ":" + pkIdx);
            var guid = part[0] + "-" + part[1];

            return new Pkuid(pkIdx, Prefix, guid);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">Format : Encode(PK:pkidx)-Guid</param>
        /// <returns></returns>
        public Pkuid Parse(string guid)
        {
            try
            {
                var part = guid.Trim().Split("-");

                var seed = part[1].GetSum();
                var b62c = new Base62Converter(seed);

                var part0 = b62c.Decode(part[0]).Split(":");
                if (part0[0] != Prefix)
                    throw new Exception();

                var pkIdx = long.Parse(part0[1]);

                return new Pkuid(pkIdx, Prefix, guid);
            }
            catch (System.Exception)
            {
                throw new Exception("Unknown member id format!");
            }
        }
        public long AmountToVolume(long amount) => (long)Math.Floor((decimal)amount / VolSize);
    }

}
