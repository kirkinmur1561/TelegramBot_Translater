using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Translater
{
    public class Client:IEquatable<Client>
    {
        public bool IsProceed { get; set; } = true;
        public long? id { get; set; }
        public TimeSpan? StartMessage { get; set; }
        public TimeSpan? EndMessage { get; set; }

        public int? CountWord { get; set; }

        public bool Equals(Client other) =>
            id == other.id;
        
    }
}
