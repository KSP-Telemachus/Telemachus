using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace Telemachus
{
    public class IterationToEvent<E> where E : EventArgs
    {
        public event EventHandler<E> Iterated;

        public void update(E eventArgs)
        {
            if (Iterated != null)
            {
                Iterated(this, eventArgs);
            }
        }
    }
}