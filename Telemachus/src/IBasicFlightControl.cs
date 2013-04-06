using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    interface IBasicFlightControl
    {
        void activateNextStage();
        void throttleUp();
        void throttleDown();
    }
}
