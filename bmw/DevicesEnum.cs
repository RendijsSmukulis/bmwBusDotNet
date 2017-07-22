using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmw
{
    enum Device
    {
        Broadcast = 0,
        CdPlayer = 0x18,
        Navigation = 0x3B,
        MenuScreen = 0x43,
        MFLSteeringWheel = 0x50,
        ParkDistanceControl = 0x60,
        Radio = 0x68,
        DSP = 0x6A,
        IKE = 0x80,
        TV = 0xBB,
        LightControl = 0xBF,
        MIDButtonx = 0xC0,
        Telephone = 0xC8,
        NavigationLocation = 0xD0,
        OBCTextBar = 0xE7,
        LightsWipersSeatMem = 0xED, 
        BoardMonitorButtons = 0xF0,
        Broadcast2 = 0xFF
    }
}
