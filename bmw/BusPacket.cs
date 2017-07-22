using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmw
{
    class BusPacket
    {
        public BusPacket()
        {
            this.Payload = new List<byte>();
        }

        public BusDevice From { get; set; }

        public byte Len { get; set; }

        public BusDevice Dest { get; set; }

        public List<byte> Payload { get; set; }

        public bool CheckXor()
        {
            var checksum = (int)this.From ^ (int)this.Len ^ (int)this.Dest;
            foreach (var b in Payload)
            {
                checksum ^= b;
            }

            return checksum == 0;
        }
    }
}
