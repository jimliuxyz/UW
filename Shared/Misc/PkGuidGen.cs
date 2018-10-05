using System;
using System.Text;
using UW.Shared;
using UW.Shared.Misc;
using System.Linq;

namespace UW.Shared.Misc
{
    public class PkGuid
    {
        public readonly long PkIdx;
        public readonly string Prefix;
        public readonly string Guid;

        public PkGuid(long pkIdx, string prefix, string guid = null)
        {
            this.PkIdx = pkIdx;
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
    public class PkGuidGen
    {
        private static readonly int ModeVol = 0;
        private static readonly int ModeRandom = 1;
        private int mode = ModeVol;

        private int VolSize;
        private int RandomStart, RandomEnd;
        public PkGuidGen() { }

        /// <summary>
        /// 設定每個pk的容量 pk=(amount/volume)
        /// </summary>
        /// <param name="volSize"></param>
        /// <returns></returns>
        public PkGuidGen SetPkVolume(int volSize)
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
        public PkGuidGen SetRandomRange(int start, int end)
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
        public PkGuid Generate(long amount = 0, string guid = null, long pkIdx = -1)
        {
            guid = guid ?? F.NewGuid();
            var seed = guid.GetSum();
            var b62c = new Base62Converter(seed);

            if (pkIdx >= 0) { }
            else if (mode == ModeVol)
                pkIdx = AmountToVolume(amount);
            else if (mode == ModeRandom)
                pkIdx = new Random().Next(RandomStart, RandomEnd);

            var prefix = "PK:" + pkIdx;

            var prefix_encoded = b62c.Encode(prefix.ToString());

            return new PkGuid(pkIdx, prefix_encoded, guid);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberId">Format : Encode(PK:pkidx)-Guid</param>
        /// <returns></returns>
        public PkGuid Parse(string memberId)
        {
            try
            {
                var part = memberId.Trim().Split("-");

                var guid = part[1];
                var seed = guid.GetSum();
                var b62c = new Base62Converter(seed);

                var prefix = b62c.Decode(part[0].ToString());

                part = prefix.Split(":");
                if (part[0] != "PK")
                    throw new Exception();

                var pkIdx = long.Parse(part[1]);

                return new PkGuid(pkIdx, prefix, guid);
            }
            catch (System.Exception)
            {
                throw new Exception("Unknown member id format!");
            }
        }

        public long AmountToVolume(long amount) => (long)Math.Floor((decimal)amount / VolSize);
    }

}
