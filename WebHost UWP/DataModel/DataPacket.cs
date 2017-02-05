using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebHost_UWP.DataModel
{
    public class DataPacket
    {
        /// <summary>
        /// Current speed of the vehicle.
        /// </summary>
        public int Speed { get; set; }
        /// <summary>
        /// Odometer reading of the vehicle.
        /// </summary>
        public int Odometer { get; set; }
        /// <summary>
        /// Defines whether headlights are on or off.
        /// </summary>
        public bool Headlights { get; set; }
        /// <summary>
        /// Left turn signal on/off.
        /// </summary>
        public bool TurnSignalLeft { get; set; }
        /// <summary>
        /// Right turn signal on/off.
        /// </summary>
        public bool TurnSignalRight { get; set; }
        /// <summary>
        /// Battery life as a percentage.
        /// </summary>
        public int BatteryLife { get; set; }
        /// <summary>
        /// Range remaining in miles.
        /// </summary>
        public int Range { get; set; }
        /// <summary>
        /// Defines the number of Watt-hours remaining per mile.
        /// </summary>
        public float WhPerMile { get; set; }
        /// <summary>
        /// Defines the outdoor temperature of the vehicle.
        /// </summary>
        public int OutdoorTemp { get; set; }
        /// <summary>
        /// Defines whether cruise control is on or off.
        /// </summary>
        public bool CruiseControlEnabled { get; set; }
        /// <summary>
        /// Defines the hold speed for the cruise control.
        /// </summary>
        public int CruiseHoldSpeed { get; set; }
        /// <summary>
        /// Defines the current date and time.
        /// </summary>
        public DateTime CurrentDateTime { get
            {
                return DateTime.Now;
            }
        }

    }
}
