using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Payments.Model {
    public class Wallet {

        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

    }
}
