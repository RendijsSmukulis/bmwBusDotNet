using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmw
{
    enum ExtractorState
    {
        Invalid,

        WaitingForStart,

        WaitingForLength,

        WaitingForDestination,

        WaitingForXor,

        BuildingPacket,

        LostSync,
    }

    class BmwBusPacketExtractor
    {
        private List<byte> bytes;
        private ExtractorState state;
        private int messageStartPointer = 0;
        private int currentBytePointer = 0;
        private BusPacket packet;
        public Queue<BusPacket> OutputPackets { get; }

        public BmwBusPacketExtractor()
        {
            this.bytes = new List<byte>();
            this.state = ExtractorState.WaitingForStart;
            this.OutputPackets = new Queue<BusPacket>(); 
        }

        public void PushByte(byte b)
        {
            this.bytes.Add(b);
            this.processBytes();
        }

        public void PushBytes(IEnumerable<byte> b)
        {
            this.bytes.AddRange(b);
            this.processBytes();
        }

        private void processBytes()
        {
            while (this.currentBytePointer < this.bytes.Count)
            {
                switch (this.state)
                {
                    case ExtractorState.WaitingForStart:
                        // 1. Create a new message
                        this.packet = new BusPacket
                        {
                            From = (BusDevice)bytes[currentBytePointer++]
                        };

                        // 2. change state to WaitForLen
                        this.state = ExtractorState.WaitingForLength;

                        // 3. Advance current byte pointer
                        break;
                    case ExtractorState.WaitingForLength:
                        var lenValue = bytes[currentBytePointer++];
                        // 1. If the byte is < 3 (?), change state to LostSync, advance msgstart by 1 and reset bytePointer t osame
                        if (lenValue < 3)
                        {
                            this.state = ExtractorState.LostSync;
                        }
                        else
                        {
                            // 2. set message len to the byte val
                            // 3. advance byte pointer by 1
                            this.packet.Len = lenValue;
                            this.state = ExtractorState.WaitingForDestination;
                        }

                        break;
                    case ExtractorState.WaitingForDestination:
                        this.packet.Dest = (BusDevice)this.bytes[this.currentBytePointer];
                        this.currentBytePointer++;
                        this.state = ExtractorState.BuildingPacket;

                        break;
                    case ExtractorState.BuildingPacket:
                        this.packet.Payload.Add(this.bytes[this.currentBytePointer]);

                        // If we have read number of bytes specified by the packet's Len field,
                        // we should expect the XOR
                        if (this.currentBytePointer - this.messageStartPointer == packet.Len)
                        {
                            this.state = ExtractorState.WaitingForXor;
                        }

                        this.currentBytePointer++;

                        break;
                    case ExtractorState.WaitingForXor:
                        // 1. add byte to packet
                        this.packet.Payload.Add(this.bytes[this.currentBytePointer++]);

                        // Perform xor validation
                        if (this.packet.CheckXor())
                        {
                            // If checksum was ok, add finished packet to output and 
                            // start waiting for the next packet
                            this.messageStartPointer = this.currentBytePointer;
                            this.OutputPackets.Enqueue(this.packet);
                            this.state = ExtractorState.WaitingForStart;
                        }
                        else
                        {
                            this.state = ExtractorState.LostSync;
                        }

                        break;
                    case ExtractorState.LostSync:
                        Console.WriteLine($"State: {this.state}");

                        this.messageStartPointer++;
                        this.currentBytePointer = this.messageStartPointer;
                        this.state = ExtractorState.WaitingForStart;
                        // TODO:
                        // 1. try to parse two messages. 
                        //  if success, set state to waitingforstart
                        //  if failure, advance both by 1
                        break;
                }
            }
        }
    }
}
 