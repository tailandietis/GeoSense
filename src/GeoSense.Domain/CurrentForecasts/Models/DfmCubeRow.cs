using System.Linq;
using System.Text;

namespace GeoDa.Domain.CurrentForecasts.Models
{
    public class DfmCubeRow
    {
        public const int CountOfValues = 35;

        public int Obj { get; set; }

        public int Idat { get; set; }

        public int Hh { get; set; }

        public int Lin { get; set; }

        public double[] Values { get; set; } = new double[CountOfValues];

        public override string ToString()
        {
            var str = (new StringBuilder()).Append($"{GetType().Name}() {{")
                        .AppendJoin(", ", GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"))
                        .Append("}")
                        .ToString();

            str += $" Value: {string.Join(", ", Values.Select(v => v.ToString()).ToList())}";

            return str;
        }
    }
}
