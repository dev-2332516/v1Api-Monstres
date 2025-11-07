using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiV1ControlleurMonstre.Models
{
    public class ServiceTimestamp
    {
        public int Id { get; set; }
        public long Timestamp { get; set; }

        public ServiceTimestamp() { }
    }
}
